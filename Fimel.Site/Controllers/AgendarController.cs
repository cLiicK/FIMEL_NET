using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    public class AgendarController : Controller
    {
        private static readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly APIClient _api = new APIClient(_config["API_URL"]);

        private static readonly Dictionary<string, DayOfWeek> _diasSemana = new()
        {
            { "Lunes",     DayOfWeek.Monday    },
            { "Martes",    DayOfWeek.Tuesday   },
            { "Miércoles", DayOfWeek.Wednesday },
            { "Jueves",    DayOfWeek.Thursday  },
            { "Viernes",   DayOfWeek.Friday    },
            { "Sábado",    DayOfWeek.Saturday  },
            { "Domingo",   DayOfWeek.Sunday    }
        };

        [Route("Agendar/{token}")]
        public IActionResult Index(string token)
        {
            ConfiguracionUsuario? cfg = _api.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByToken/{token}");

            if (cfg?.Usuario == null)
                return NotFound();

            if (cfg.Usuario.IdInstitucion.HasValue)
                cfg.Usuario.Institucion = _api.Get<Instituciones>($"Instituciones/{cfg.Usuario.IdInstitucion}");

            ViewBag.Token = token;
            return View(cfg);
        }

        [HttpGet("Agendar/ObtenerDisponibilidad")]
        public IActionResult ObtenerDisponibilidad(string token, DateTime desde, DateTime hasta)
        {
            try
            {
                ConfiguracionUsuario? cfg = _api.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByToken/{token}");
                if (cfg?.Usuario == null) return Json(new { ok = false });

                int userId = cfg.Usuario.Id;
                TimeSpan duracion = cfg.DuracionBloqueHorario == TimeSpan.Zero
                    ? TimeSpan.FromMinutes(30)
                    : cfg.DuracionBloqueHorario;

                var horariosSemanales = _api.Get<List<HorarioAtencion>>($"HorariosAtencion/GetByUser/{userId}") ?? new();
                var horariosEspecificos = _api.Get<List<HorarioEspecifico>>($"HorariosEspecificos/GetByUser/{userId}") ?? new();
                var citas = _api.Get<List<Cita>>($"Citas/GetByCriteria", new
                {
                    FechaInicio = desde,
                    FechaTermino = hasta,
                    UsuarioId = userId
                }) ?? new();

                var slots = new List<object>();

                for (var fecha = desde.Date; fecha <= hasta.Date; fecha = fecha.AddDays(1))
                {
                    if (fecha < DateTime.Today) continue;

                    var ventanas = new List<(TimeSpan inicio, TimeSpan fin)>();

                    foreach (var h in horariosSemanales.Where(h =>
                        _diasSemana.TryGetValue(h.DiaSemana ?? "", out var dow) && dow == fecha.DayOfWeek))
                        ventanas.Add((h.HoraInicio, h.HoraFin));

                    foreach (var h in horariosEspecificos.Where(h => h.FechaEspecifica.Date == fecha))
                        ventanas.Add((h.HoraInicio, h.HoraFin));

                    foreach (var (inicio, fin) in ventanas)
                    {
                        for (var t = inicio; t + duracion <= fin; t += duracion)
                        {
                            var dtInicio = fecha.Add(t);
                            var dtFin = dtInicio.Add(duracion);

                            if (dtInicio <= DateTime.Now) continue;

                            bool ocupado = citas.Any(c => c.FechaHoraInicio < dtFin && c.FechaHoraFinal > dtInicio);
                            if (!ocupado)
                                slots.Add(new
                                {
                                    fecha = fecha.ToString("yyyy-MM-dd"),
                                    horaInicio = t.ToString(@"hh\:mm"),
                                    horaFin = (t + duracion).ToString(@"hh\:mm")
                                });
                        }
                    }
                }

                return Json(new { ok = true, slots });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error AgendarController ObtenerDisponibilidad: {ex}");
                return Json(new { ok = false });
            }
        }

        [HttpPost("Agendar/Reservar")]
        public IActionResult Reservar([FromForm] string token, [FromForm] string nombre,
            [FromForm] string correo, [FromForm] string? telefono, [FromForm] string? nota,
            [FromForm] DateTime fechaHoraInicio, [FromForm] DateTime fechaHoraFin)
        {
            try
            {
                ConfiguracionUsuario? cfg = _api.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByToken/{token}");
                if (cfg?.Usuario == null)
                    return Json(new { ok = false, error = "Enlace no válido." });

                // Verificar que el slot siga disponible
                var citasExistentes = _api.Get<List<Cita>>($"Citas/GetByCriteria", new
                {
                    FechaInicio = fechaHoraInicio,
                    FechaTermino = fechaHoraFin,
                    UsuarioId = cfg.Usuario.Id
                }) ?? new();

                if (citasExistentes.Count > 0)
                    return Json(new { ok = false, error = "Este horario ya no está disponible. Por favor elige otro." });

                Usuarios? profesional = _api.Get<Usuarios>($"Usuarios/{cfg.Usuario.Id}");
                if (profesional == null)
                    return Json(new { ok = false, error = "Error interno." });

                var cita = new Cita
                {
                    FechaHoraInicio = fechaHoraInicio,
                    FechaHoraFinal = fechaHoraFin,
                    NombrePaciente = nombre,
                    CorreoPaciente = correo,
                    Telefono = telefono,
                    Nota = nota,
                    Usuario = profesional
                };

                Cita? citaCreada = _api.Post<Cita>("Citas", cita);
                if (citaCreada == null)
                    return Json(new { ok = false, error = "No se pudo crear la cita." });

                try { EnviarCorreoConfirmacion(citaCreada, profesional); }
                catch (Exception ex) { Logger.Log($"Error correo confirmación pública: {ex}"); }

                try { EnviarCorreoNotificacionProfesional(citaCreada, profesional); }
                catch (Exception ex) { Logger.Log($"Error correo notificación profesional: {ex}"); }

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error AgendarController Reservar: {ex}");
                return Json(new { ok = false, error = "Error al crear la cita." });
            }
        }

        private void EnviarCorreoConfirmacion(Cita cita, Usuarios profesional)
        {
            string nombreProfesional = $"{profesional.Nombres} {profesional.ApellidoPaterno} {profesional.ApellidoMaterno}".Trim();
            string fechaCita = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
            string horaCita = cita.FechaHoraInicio.ToString("HH:mm");
            string direccion = "Dirección no disponible";

            if (profesional.IdInstitucion.HasValue)
            {
                try
                {
                    Instituciones? inst = _api.Get<Instituciones>($"Instituciones/{profesional.IdInstitucion}");
                    if (!string.IsNullOrEmpty(inst?.Dirección)) direccion = inst.Dirección;
                }
                catch (Exception ex) { Logger.Log($"Error obteniendo institución para correo: {ex.Message}"); }
            }

            string ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-confirmacion-cita.html");
            string html = System.IO.File.ReadAllText(ruta);

            html = html
                .Replace("{{paciente}}", cita.NombrePaciente)
                .Replace("{{profesional}}", nombreProfesional)
                .Replace("{{fecha_cita}}", fechaCita)
                .Replace("{{hora_cita}}", horaCita)
                .Replace("{{direccion}}", direccion);

            var correo = new EnvioCorreo
            {
                Destinatarios = new List<string> { cita.CorreoPaciente },
                Asunto = $"Confirmación de Cita — {cita.FechaHoraInicio:dd/MM/yyyy}",
                CuerpoCorreo = html
            };

            var imagenes = new List<(string, string, string)>
            {
                (Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png"), "logoImage", "image/png")
            };

            new Utileria().EnviarCorreo(correo, imagenes, $"Mat. {profesional.Nombres} {profesional.ApellidoPaterno}");
        }

        private void EnviarCorreoNotificacionProfesional(Cita cita, Usuarios profesional)
        {
            if (string.IsNullOrEmpty(profesional.Email)) return;

            string nombreProfesional = $"{profesional.Nombres} {profesional.ApellidoPaterno}".Trim();
            string fechaCita = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
            string horaCita = cita.FechaHoraInicio.ToString("HH:mm");

            string ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-nueva-cita-profesional.html");
            string html = System.IO.File.ReadAllText(ruta);

            html = html
                .Replace("{{profesional}}", nombreProfesional)
                .Replace("{{paciente}}", cita.NombrePaciente)
                .Replace("{{correo_paciente}}", cita.CorreoPaciente)
                .Replace("{{telefono}}", cita.Telefono ?? "No indicado")
                .Replace("{{fecha_cita}}", fechaCita)
                .Replace("{{hora_cita}}", horaCita)
                .Replace("{{nota}}", string.IsNullOrEmpty(cita.Nota) ? "Sin nota" : cita.Nota);

            var correo = new EnvioCorreo
            {
                Destinatarios = new List<string> { profesional.Email },
                Asunto = $"Nueva cita agendada — {cita.FechaHoraInicio:dd/MM/yyyy HH:mm}",
                CuerpoCorreo = html
            };

            var imagenes = new List<(string, string, string)>
            {
                (Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png"), "logoImage", "image/png")
            };

            new Utileria().EnviarCorreo(correo, imagenes, "Fimel");
        }
    }
}
