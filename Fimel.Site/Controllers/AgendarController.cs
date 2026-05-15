using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    public class AgendarController : Controller
    {
        private readonly APIClient _api;
        public AgendarController(IConfiguration config) => _api = new APIClient(config["API_URL"]);

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

        [HttpGet("Agendar/BuscarPaciente")]
        public IActionResult BuscarPaciente(string token, int? rut, string? numDoc)
        {
            try
            {
                ConfiguracionUsuario? cfg = _api.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByToken/{token}");
                if (cfg?.Usuario == null) return Json(new { ok = false });

                Pacientes? paciente = null;

                if (rut.HasValue)
                    paciente = _api.Get<Pacientes>($"Pacientes/GetByRut/{rut.Value}");
                else if (!string.IsNullOrWhiteSpace(numDoc))
                    paciente = _api.Get<Pacientes>($"Pacientes/GetByNumeroDocumento/{Uri.EscapeDataString(numDoc)}");

                if (paciente == null || paciente.Id == 0)
                    return Json(new { ok = false });

                return Json(new
                {
                    ok = true,
                    nombres = paciente.Nombres,
                    primerApellido = paciente.PrimerApellido,
                    segundoApellido = paciente.SegundoApellido,
                    email = paciente.Email,
                    celular = paciente.Celular
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error AgendarController BuscarPaciente: {ex}");
                return Json(new { ok = false });
            }
        }

        [HttpPost("Agendar/Reservar")]
        public IActionResult Reservar([FromForm] string token, [FromForm] string nombre,
            [FromForm] string? apellidoPaciente, [FromForm] string? segundoApellidoPaciente,
            [FromForm] string? tipoDocumento, [FromForm] string? numeroDocumento,
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
                    ApellidoPaciente = apellidoPaciente,
                    SegundoApellidoPaciente = segundoApellidoPaciente,
                    TipoDocumento = tipoDocumento,
                    NumeroDocumento = numeroDocumento,
                    CorreoPaciente = correo,
                    Telefono = telefono,
                    Nota = nota,
                    Usuario = profesional
                };

                Cita? citaCreada = _api.Post<Cita>("Citas", cita);
                if (citaCreada == null)
                    return Json(new { ok = false, error = "No se pudo crear la cita." });

                string rutaLogo = ObtenerRutaLogo(profesional);

                try { EnviarCorreoConfirmacion(citaCreada, profesional, rutaLogo); }
                catch (Exception ex) { Logger.Log($"Error correo confirmación pública: {ex}"); }

                try { EnviarCorreoNotificacionProfesional(citaCreada, profesional, rutaLogo); }
                catch (Exception ex) { Logger.Log($"Error correo notificación profesional: {ex}"); }

                try { CrearOActualizarPacienteDesde(citaCreada, cfg.Usuario.Id); }
                catch (Exception ex) { Logger.Log($"Error al crear paciente desde cita pública: {ex}"); }

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error AgendarController Reservar: {ex}");
                return Json(new { ok = false, error = "Error al crear la cita." });
            }
        }

        private string ObtenerRutaLogo(Usuarios profesional)
        {
            string rutaDefault = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");

            if (!profesional.IdInstitucion.HasValue) return rutaDefault;

            try
            {
                Instituciones? inst = _api.Get<Instituciones>($"Instituciones/{profesional.IdInstitucion}");
                if (inst == null || string.IsNullOrEmpty(inst.Logo)) return rutaDefault;

                string tempPath = Path.Combine(Path.GetTempPath(), $"logo_inst_{profesional.IdInstitucion}.png");
                System.IO.File.WriteAllBytes(tempPath, Convert.FromBase64String(inst.Logo));
                return tempPath;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener logo de institución para correo: {ex.Message}");
                return rutaDefault;
            }
        }

        private void EnviarCorreoConfirmacion(Cita cita, Usuarios profesional, string rutaLogo)
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

            string nombreCompletoPaciente = string.Join(" ",
                new[] { cita.NombrePaciente, cita.ApellidoPaciente, cita.SegundoApellidoPaciente }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            html = html
                .Replace("{{paciente}}", nombreCompletoPaciente)
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
                (rutaLogo, "logoImage", "image/png")
            };

            new Utileria().EnviarCorreo(correo, imagenes, $"Mat. {profesional.Nombres} {profesional.ApellidoPaterno}");
        }

        private void CrearOActualizarPacienteDesde(Cita cita, int idUsuario)
        {
            if (string.IsNullOrEmpty(cita.NumeroDocumento) || string.IsNullOrEmpty(cita.TipoDocumento))
                return;

            Pacientes? pacienteExistente = null;

            if (cita.TipoDocumento == "RUT" && int.TryParse(cita.NumeroDocumento, out int rut))
            {
                try { pacienteExistente = _api.Get<Pacientes>($"Pacientes/GetByRut/{rut}"); } catch { }
            }
            else
            {
                try { pacienteExistente = _api.Get<Pacientes>($"Pacientes/GetByNumeroDocumento/{Uri.EscapeDataString(cita.NumeroDocumento)}"); } catch { }
            }

            if (pacienteExistente != null && pacienteExistente.Id > 0)
                return;

            var nuevo = new Pacientes
            {
                Nombres = cita.NombrePaciente,
                PrimerApellido = cita.ApellidoPaciente,
                SegundoApellido = cita.SegundoApellidoPaciente,
                Email = cita.CorreoPaciente,
                Celular = int.TryParse(new string(cita.Telefono?.Where(char.IsDigit).ToArray()), out int tel) ? tel : null,
                TipoDocumento = cita.TipoDocumento,
                NumeroDocumento = cita.NumeroDocumento,
                UsuarioCreacion = idUsuario
            };

            if (cita.TipoDocumento == "RUT" && int.TryParse(cita.NumeroDocumento, out int rutNum))
            {
                nuevo.Rut = rutNum;
                nuevo.Dv = CalcularDvRut(rutNum);
            }

            _api.Post<Pacientes>("Pacientes", nuevo);
        }

        private static string CalcularDvRut(int rut)
        {
            int suma = 0, multiplicador = 2, numero = rut;
            while (numero > 0)
            {
                suma += (numero % 10) * multiplicador;
                numero /= 10;
                multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
            }
            int dv = 11 - (suma % 11);
            if (dv == 11) return "0";
            if (dv == 10) return "K";
            return dv.ToString();
        }

        private void EnviarCorreoNotificacionProfesional(Cita cita, Usuarios profesional, string rutaLogo)
        {
            if (string.IsNullOrEmpty(profesional.Email)) return;

            string nombreProfesional = $"{profesional.Nombres} {profesional.ApellidoPaterno}".Trim();
            string fechaCita = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
            string horaCita = cita.FechaHoraInicio.ToString("HH:mm");

            string ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-nueva-cita-profesional.html");
            string html = System.IO.File.ReadAllText(ruta);

            string nombreCompletoPaciente = string.Join(" ",
                new[] { cita.NombrePaciente, cita.ApellidoPaciente, cita.SegundoApellidoPaciente }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            html = html
                .Replace("{{profesional}}", nombreProfesional)
                .Replace("{{paciente}}", nombreCompletoPaciente)
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
                (rutaLogo, "logoImage", "image/png")
            };

            new Utileria().EnviarCorreo(correo, imagenes, "Fimel");
        }
    }
}
