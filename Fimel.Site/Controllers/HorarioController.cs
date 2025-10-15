using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Site.ViewModels;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Text.Json;
using static Fimel.Models.Enums;

namespace Fimel.Site.Controllers
{
    public class HorarioController : Controller
    {
        private static IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly APIClient APIBase = new APIClient(config["API_URL"]);

        public IActionResult MiHorario()
        {
            Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

            MiHorarioVM vm = new MiHorarioVM();
            vm.HorariosAtencion = APIBase.Get<List<HorarioAtencion>>($"HorariosAtencion/GetByUser/{usuario.Id}");

            // Validar que los horarios específicos no sean null antes de asignarlos
            try
            {
                var horariosEspecificos = APIBase.Get<List<HorarioEspecifico>>($"HorariosEspecificos/GetByUser/{usuario.Id}");
                vm.HorariosEspecificos = horariosEspecificos ?? new List<HorarioEspecifico>();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener horarios específicos: {ex}");
                vm.HorariosEspecificos = new List<HorarioEspecifico>();
            }

            vm.ConfiguracionUsuario = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{usuario.Id}");

            if (usuario.IdPerfil == (int)EnumPerfiles.Administrativo)
            {
                List<Usuarios> listaUsuarios = APIBase.Get<List<Usuarios>>($"Usuarios/GetByInstitucion/{usuario.IdInstitucion}");
                vm.ListaUsuarios = listaUsuarios.Where(t => t.IdPerfil == (int)EnumPerfiles.Especialista).Select(u => new UsuarioVM
                {
                    Id = u.Id,
                    NombreCompleto = u.Nombres + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                }).ToList();
            }

            // Pasar el perfil del usuario a la vista
            ViewBag.IdPerfilUsuario = usuario.IdPerfil;

            HttpContext.Session.SetString("HorarioUser", JsonSerializer.Serialize(vm.HorariosAtencion));

            return View(vm);
        }

        public ActionResult ObtenerCitas(int? idUsuario = null)
        {
            Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

            // Determinar de qué usuario obtener las citas
            int idUsuarioFinal;
            if (idUsuario.HasValue && usuario.IdPerfil == (int)EnumPerfiles.Administrativo)
            {
                // Si es administrativo y se especifica un usuario, usar ese
                idUsuarioFinal = idUsuario.Value;
            }
            else
            {
                // Si es especialista o no se especifica, usar el usuario conectado
                idUsuarioFinal = usuario.Id;
            }

            List<Cita> citas = APIBase.Get<List<Cita>>($"Citas/GetByUser/{idUsuarioFinal}");

            var jsonEventos = new List<object>();

            foreach (var cita in citas)
            {
                jsonEventos.Add(new
                {
                    title = cita.NombrePaciente,
                    start = cita.FechaHoraInicio.ToString("yyyy-MM-dd HH:mm:ss"),
                    end = cita.FechaHoraFinal.ToString("yyyy-MM-dd HH:mm:ss"),
                    backgroundColor = "#75BDE0",
                    borderColor = "#75BDE0",
                    extendedProps = new
                    {
                        idCita = cita.Id,
                        nombre = cita.NombrePaciente,
                        correo = cita.CorreoPaciente,
                        telefono = cita.Telefono,
                        nota = cita.Nota,
                        documento = cita.TipoDocumento,
                        numDocumento = cita.TipoDocumento == "RUT" ? Utileria.FormatearRutSinDv(Convert.ToInt32(cita.NumeroDocumento)) : cita.NumeroDocumento,
                        horaInicio = cita.FechaHoraInicio.ToString("HH:mm"),
                        horaFinal = cita.FechaHoraFinal.ToString("HH:mm")
                    }
                });
            }

            // Obtener horarios semanales del usuario correspondiente
            List<HorarioAtencion> horarios = APIBase.Get<List<HorarioAtencion>>($"HorariosAtencion/GetByUser/{idUsuarioFinal}");

            foreach (var horario in horarios)
            {
                jsonEventos.Add(new
                {
                    title = "",
                    daysOfWeek = new List<int> { new Utileria().ConvertirDiaSemanaANumero(horario.DiaSemana) },
                    startTime = horario.HoraInicio.ToString(@"hh\:mm"),
                    endTime = horario.HoraFin.ToString(@"hh\:mm"),
                    display = "background",
                    backgroundColor = "#F89B9B",
                });
            }

            // Obtener horarios específicos del usuario correspondiente
            try
            {
                List<HorarioEspecifico> horariosEspecificos = APIBase.Get<List<HorarioEspecifico>>($"HorariosEspecificos/GetByUser/{idUsuarioFinal}");

                if (horariosEspecificos != null)
                {
                    foreach (var horarioEspecifico in horariosEspecificos)
                    {
                        jsonEventos.Add(new
                        {
                            title = "",
                            start = horarioEspecifico.FechaEspecifica.ToString("yyyy-MM-dd") + "T" + horarioEspecifico.HoraInicio.ToString(@"hh\:mm\:ss"),
                            end = horarioEspecifico.FechaEspecifica.ToString("yyyy-MM-dd") + "T" + horarioEspecifico.HoraFin.ToString(@"hh\:mm\:ss"),
                            display = "background",
                            backgroundColor = "#FFB366",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener horarios específicos para usuario {idUsuarioFinal}: {ex}");
            }

            return Json(jsonEventos);
        }

        public ActionResult _CrearBloqueHorario(HorarioAtencion horario, int? idUsuarioDestino = null)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

                // Determinar para qué usuario se crea el bloque de horario
                int idUsuarioFinal;
                if (idUsuarioDestino.HasValue && usuario.IdPerfil == (int)EnumPerfiles.Administrativo)
                {
                    // Si es administrativo y se especifica un usuario destino, usar ese
                    idUsuarioFinal = idUsuarioDestino.Value;
                }
                else
                {
                    // Si es especialista o no se especifica destino, usar el usuario conectado
                    idUsuarioFinal = usuario.Id;
                }

                // Obtener el usuario destino completo para asignarlo al horario
                Usuarios usuarioDestino = APIBase.Get<Usuarios>($"Usuarios/{idUsuarioFinal}");
                horario.Usuario = usuarioDestino;

                HorarioAtencion? horarioPost = APIBase.Post<HorarioAtencion>($"HorariosAtencion", horario);

                if (horarioPost == null)
                    return Json(new { success = false, message = "Error interno al guardar Bloque..." });

                return Json(new { success = true, message = "Bloque Guardado!" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _CrearBloqueHorario: {ex}");
                return null;
            }
        }

        public ActionResult _EliminarBloqueHorario(int id)
        {
            try
            {
                HorarioAtencion horarioAtencion = APIBase.Get<HorarioAtencion>($"HorariosAtencion/{id}");
                horarioAtencion.Vigente = "N";
                HorarioAtencion horarioAtencionPut = APIBase.Put<HorarioAtencion>($"HorariosAtencion/{id}", horarioAtencion);

                if (horarioAtencionPut == null)
                    return Json(new { success = false, message = "Error al eliminar el Horario de Atencion" });

                return Json(new { success = true, message = "Bloque de atención eliminado" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _EliminarBloqueHorario: {ex}");
                return null;
            }
        }

        public ActionResult _CrearHorarioEspecifico(HorarioEspecifico horario, int? idUsuarioDestino = null)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

                // Determinar para qué usuario se crea el horario específico
                int idUsuarioFinal;
                if (idUsuarioDestino.HasValue && usuario.IdPerfil == (int)EnumPerfiles.Administrativo)
                {
                    // Si es administrativo y se especifica un usuario destino, usar ese
                    idUsuarioFinal = idUsuarioDestino.Value;
                }
                else
                {
                    // Si es especialista o no se especifica destino, usar el usuario conectado
                    idUsuarioFinal = usuario.Id;
                }

                // Obtener el usuario destino completo para asignarlo al horario
                Usuarios usuarioDestino = APIBase.Get<Usuarios>($"Usuarios/{idUsuarioFinal}");
                horario.Usuario = usuarioDestino;

                HorarioEspecifico? horarioPost = APIBase.Post<HorarioEspecifico>($"HorariosEspecificos", horario);

                if (horarioPost == null)
                    return Json(new { success = false, message = "Error interno al guardar Horario Específico..." });

                return Json(new { success = true, message = "Horario Específico Guardado!" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _CrearHorarioEspecifico: {ex}");
                return null;
            }
        }

        public ActionResult _EliminarHorarioEspecifico(int id)
        {
            try
            {

                if (id <= 0)
                {
                    return Json(new { success = false, message = "ID de horario específico no válido" });
                }

                HorarioEspecifico horarioEspecifico = APIBase.Get<HorarioEspecifico>($"HorariosEspecificos/{id}");
                horarioEspecifico.Vigente = "N";
                HorarioEspecifico horarioEspecificoPut = APIBase.Put<HorarioEspecifico>($"HorariosEspecificos/{id}", horarioEspecifico);

                if (horarioEspecificoPut == null)
                    return Json(new { success = false, message = "Error al eliminar el Horario Específico" });

                return Json(new { success = true, message = "Horario Específico eliminado" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _EliminarHorarioEspecifico: {ex}");
                return null;
            }
        }

        public ActionResult _EliminarCita(int id)
        {
            try
            {
                Cita cita = APIBase.Get<Cita>($"Citas/{id}");
                cita.Vigente = "N";
                Cita citaPut = APIBase.Put<Cita>($"Citas/{id}", cita);

                if (citaPut == null)
                    return Json(new { success = false, message = "Error al eliminar cita" });

                return Json(new { success = true, message = "Cita eliminada" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _EliminarCita: {ex}");
                return null;
            }
        }

        public ActionResult _GuardarCita(Cita cita, int? idUsuarioDestino = null)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

                // Determinar para qué usuario se crea la cita
                int idUsuarioFinal;
                if (idUsuarioDestino.HasValue && usuario.IdPerfil == (int)EnumPerfiles.Administrativo)
                {
                    // Si es administrativo y se especifica un usuario destino, usar ese
                    idUsuarioFinal = idUsuarioDestino.Value;
                }
                else
                {
                    // Si es especialista o no se especifica destino, usar el usuario conectado
                    idUsuarioFinal = usuario.Id;
                }

                List<Cita> dbCita = APIBase.Get<List<Cita>>($"Citas/GetByCriteria", new
                {
                    FechaInicio = cita.FechaHoraInicio,
                    FechaTermino = cita.FechaHoraFinal,
                    UsuarioId = idUsuarioFinal
                });

                if (dbCita.Count > 0)
                    return Json(new { success = false, message = "Ya existe una cita agendada que topa con el Día y Hora" });

                // Obtener el usuario destino completo para asignarlo a la cita
                Usuarios usuarioDestino = APIBase.Get<Usuarios>($"Usuarios/{idUsuarioFinal}");
                cita.Usuario = usuarioDestino;

                Cita citaPost = APIBase.Post<Cita>($"Citas", cita);

                if (citaPost == null)
                    return Json(new { success = false, message = "Error al crear cita" });

                try
                {
                    EnviarCorreoConfirmacionCita(citaPost, usuarioDestino);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error al enviar correo de confirmación: {ex}");
                    // No fallar la creación de la cita si el correo falla
                }

                return Json(new { success = true, message = "Cita creada" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Horario _GuardarCita: {ex}");
                return null;
            }
        }

        private void EnviarCorreoConfirmacionCita(Cita cita, Usuarios profesional)
        {
            try
            {
                string contenidoCorreo = GenerarContenidoCorreoConfirmacion(cita, profesional);

                var correo = new EnvioCorreo
                {
                    Destinatarios = new List<string> { cita.CorreoPaciente },
                    Asunto = $"Confirmación de Cita - {cita.FechaHoraInicio.ToString("dd/MM/yyyy")}",
                    CuerpoCorreo = contenidoCorreo
                };

                var imagenes = new List<(string, string, string)>
                {
                    (Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png"), "logoImage", "image/png")
                };

                Utileria utileria = new Utileria();
                utileria.EnviarCorreo(correo, imagenes, $"Mta. {profesional.Nombres} {profesional.ApellidoPaterno}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al enviar correo de confirmación de cita: {ex}");
            }
        }

        private string GenerarContenidoCorreoConfirmacion(Cita cita, Usuarios profesional)
        {
            try
            {
                string nombreProfesional = $"{profesional.Nombres} {profesional.ApellidoPaterno} {profesional.ApellidoMaterno}".Trim();

                string fechaCita = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
                string horaCita = cita.FechaHoraInicio.ToString("HH:mm");

                string direccionInstitucion = "Dirección no disponible";

                if (profesional.IdInstitucion.HasValue)
                {
                    Instituciones institucion = APIBase.Get<Instituciones>($"Instituciones/{profesional.IdInstitucion.Value}");
                    if (institucion != null && !string.IsNullOrEmpty(institucion.Dirección))
                        direccionInstitucion = institucion.Dirección;
                }

                string ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-confirmacion-cita.html");
                string contenidoHTML = System.IO.File.ReadAllText(ruta);

                //Remplazar TAG del HTML
                var dataHtml = new Dictionary<string, string>
                {
                    //["{{img_logo}}"] = ConvertirImageABase64(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png")),
                    ["{{paciente}}"] = cita.NombrePaciente,
                    ["{{profesional}}"] = nombreProfesional,
                    ["{{fecha_cita}}"] = fechaCita,
                    ["{{hora_cita}}"] = horaCita,
                    ["{{direccion}}"] = direccionInstitucion,
                };

                foreach (var kv in dataHtml)
                    contenidoHTML = contenidoHTML.Replace(kv.Key, kv.Value);

                return contenidoHTML;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al generar contenido del correo: {ex}");
                return $"Su cita ha sido confirmada para el {cita.FechaHoraInicio.ToString("dd/MM/yyyy")} a las {cita.FechaHoraInicio.ToString("HH:mm")} con {profesional.Nombres} {profesional.ApellidoPaterno}.";
            }
        }

        public static string ConvertirImageABase64(string rutaImagen)
        {

            byte[] binaryData = System.IO.File.ReadAllBytes(rutaImagen);
            return Convert.ToBase64String(binaryData);
        }

        [HttpGet]
        public IActionResult ObtenerVistaAgenda(int idUsuario)
        {
            try
            {
                var horarios = APIBase.Get<List<HorarioAtencion>>($"HorariosAtencion/GetByUser/{idUsuario}");

                // Validar que los horarios específicos no sean null
                List<HorarioEspecifico> horariosEspecificos;
                try
                {
                    horariosEspecificos = APIBase.Get<List<HorarioEspecifico>>($"HorariosEspecificos/GetByUser/{idUsuario}");
                    if (horariosEspecificos == null)
                        horariosEspecificos = new List<HorarioEspecifico>();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error al obtener horarios específicos para usuario {idUsuario}: {ex}");
                    horariosEspecificos = new List<HorarioEspecifico>();
                }

                var configUsuario = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{idUsuario}");

                var vm = new MiHorarioVM
                {
                    HorariosAtencion = horarios,
                    HorariosEspecificos = horariosEspecificos,
                    ConfiguracionUsuario = configUsuario
                };

                // Guardar el ID del usuario seleccionado en la sesión para usar en las operaciones
                HttpContext.Session.SetString("UsuarioSeleccionado", idUsuario.ToString());

                return PartialView("_PartialContenedorAgenda", vm);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error ObtenerVistaAgenda: {ex}");
                return StatusCode(500);
            }
        }

        [HttpGet]
        public IActionResult ObtenerHorariosEspecificos(int idUsuario)
        {
            try
            {
                List<HorarioEspecifico> horariosEspecificos = new List<HorarioEspecifico>();

                try
                {
                    var url = $"HorariosEspecificos/GetByUser/{idUsuario}";
                    var resultado = APIBase.Get<List<HorarioEspecifico>>(url);

                    if (resultado != null && resultado.Count > 0)
                    {
                        horariosEspecificos = resultado;
                    }
                }
                catch (Exception ex)
                {

                }

                var response = new
                {
                    success = true,
                    horarios = horariosEspecificos,
                    count = horariosEspecificos.Count
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    success = false,
                    message = "Error al obtener horarios específicos",
                    horarios = new List<HorarioEspecifico>(),
                    count = 0
                };
                
                return Json(errorResponse);
            }
        }

    }
}
