using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Models.Params;
using Fimel.Site.ViewModels;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Fimel.Models.Enums;

namespace Fimel.Site.Controllers
{
    public class PacientesController : Controller
    {
        private static IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly APIClient APIBase = new APIClient(config["API_URL"]);


        public ActionResult FichaPaciente()
        {
            return View();
        }

        public ActionResult HistorialPacientes()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ObtenerHistorial(QueryBuscarPacientes query)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

                query.UsuarioId = usuario.Id;
                List<Pacientes> pacientes = APIBase.Get<List<Pacientes>>("Pacientes/GetByCriteria", query);

                return Json(new { success = true, data = pacientes});
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Obtener Historial: {ex}");
                return null;
            }
        }

        public ActionResult DetallePaciente(string idEncrypted)
        {
            try
            {
                string idPaciente = new Utileria().DesencryptarBase64(idEncrypted);

                Pacientes paciente = APIBase.Get<Pacientes>($"Pacientes/{Convert.ToInt32(idPaciente)}");

                return View(paciente);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al cargar el DetallePaciente: {ex}");
                return null;
            }
        }

        public JsonResult BuscarPaciente(int rutPaciente)
        {
            try
            {
                Pacientes paciente = APIBase.Get<Pacientes>($"Pacientes/GetByRut/{rutPaciente}");
                if (paciente == null) { paciente = new Pacientes(); }
                paciente.UsuarioConectado = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                return Json(paciente);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Buscar Paciente: {ex}");
                return null;
            }
        }

        public JsonResult BuscarPacientePorNumDocumento(string numDoc)
        {
            try
            {
                Pacientes paciente = APIBase.Get<Pacientes>($"Pacientes/GetByNumeroDocumento/{numDoc}");
                if (paciente == null) { paciente = new Pacientes(); }
                paciente.UsuarioConectado = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                return Json(paciente);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Buscar Paciente Por Num Documento: {ex}");
                return null;
            }
        }

        public JsonResult GrabarNuevoPaciente(string datosPaciente)
        {
            Usuarios usuarioConectado = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
            RespuestaAPI response = new RespuestaAPI();

            try
            {
                Pacientes nuevoPaciente = JsonConvert.DeserializeObject<Pacientes>(datosPaciente);

                Pacientes exist = APIBase.Get<Pacientes>($"Pacientes/GetByRut/{nuevoPaciente.Rut}");

                if (exist != null)
                {
                    response.Codigo = 500;
                    response.Mensaje = "El Rut ingresado ya existe";
                    return Json(response);
                }

                nuevoPaciente.UsuarioCreacion = usuarioConectado.Id;
                nuevoPaciente = APIBase.Post<Pacientes>($"Pacientes", nuevoPaciente);
                if (nuevoPaciente != null)
                {
                    //Session["PacienteConsultado"] = nuevoPaciente;
                    response.Codigo = 200;
                    response.Mensaje = "CREADO CORRECTAMENTE";
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al crear paciente: {ex}");
                response.Codigo = 500;
                response.Mensaje = "ERROR AL CREAR PACIENTE";
            }
            return Json(response);
        }

        public JsonResult ActualizarPaciente(string datosPaciente)
        {
            RespuestaAPI response = new RespuestaAPI();
            try
            {
                Pacientes paciente = JsonConvert.DeserializeObject<Pacientes>(datosPaciente);
                Pacientes pacienteConsultado = null;
                if (paciente.TipoDocumento == EnumTiposDocumento.RUT.ToString())
                {
                    pacienteConsultado = APIBase.Get<Pacientes>($"Pacientes/GetByRut/{paciente.Rut.Value}");
                }
                else
                {
                    pacienteConsultado = APIBase.Get<Pacientes>($"Pacientes/GetByNumeroDocumento/{paciente.NumeroDocumento}");
                }

                paciente.Id = pacienteConsultado.Id;
                paciente.FechaCreacion = pacienteConsultado.FechaCreacion;
                paciente.UsuarioCreacion = pacienteConsultado.UsuarioCreacion;

                APIBase.Put<Pacientes>($"Pacientes/{paciente.Id}", paciente);
                if (paciente != null)
                {
                    response.Codigo = 200;
                    response.Mensaje = "ACTUALIZADO CORRECTAMENTE";
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al actualizar paciente: {ex}");
                response.Codigo = 500;
                response.Mensaje = "ERROR AL ACTUALIZAR PACIENTE";
            }
            return Json(response);
        }

        public ActionResult EliminarPaciente(int idPaciente)
        {
            try
            {
                bool response = APIBase.Delete<bool>($"Pacientes/{idPaciente}");

                return Json(new { success = response });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error EliminarPaciente: {ex}");
                return null;
            }
        }

        [HttpGet]
        public ActionResult ObtenerProximosControles(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                if (usuario == null) return Json(new { success = false });

                string desde = (fechaDesde ?? DateTime.Today).ToString("yyyy-MM-dd");
                string hasta = (fechaHasta ?? DateTime.Today.AddDays(30)).ToString("yyyy-MM-dd");

                List<Consultas> controles = APIBase.Get<List<Consultas>>(
                    $"Consultas/GetProximosControles?idUsuario={usuario.Id}&fechaDesde={desde}&fechaHasta={hasta}");

                return Json(new { success = true, data = controles ?? new List<Consultas>() });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error ObtenerProximosControles: {ex}");
                return Json(new { success = false, data = new List<object>() });
            }
        }

        [HttpPost]
        public ActionResult EnviarCorreoProximoControl(int idConsulta)
        {
            try
            {
                Usuarios usuarioConectado = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

                Consultas consulta = APIBase.Get<Consultas>($"Consultas/{idConsulta}");
                if (consulta == null || !consulta.FechaProximoControl.HasValue)
                    return Json(new { success = false, message = "Consulta no encontrada o sin fecha de próximo control." });

                Pacientes paciente = APIBase.Get<Pacientes>($"Pacientes/{consulta.Id_Paciente}");
                if (paciente == null || string.IsNullOrEmpty(paciente.Email))
                    return Json(new { success = false, message = "El paciente no tiene correo registrado." });

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-proximo-control.html");
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");

                if (!System.IO.File.Exists(templatePath))
                    return Json(new { success = false, message = "Plantilla de correo no encontrada." });

                string templateHtml = System.IO.File.ReadAllText(templatePath);
                string nombreCompleto = $"{paciente.Nombres} {paciente.PrimerApellido}".Trim();
                string fechaFormateada = consulta.FechaProximoControl.Value.ToString("dd/MM/yyyy");
                string nombreProfesional = $"{usuarioConectado.Nombres} {usuarioConectado.ApellidoPaterno}".Trim();

                Instituciones? institucion = null;
                if (usuarioConectado.IdInstitucion.HasValue)
                    institucion = APIBase.Get<Instituciones>($"Instituciones/{usuarioConectado.IdInstitucion}");

                string remitente = institucion?.RazonSocial ?? "FIMEL";

                string cuerpo = templateHtml
                    .Replace("{{nombre_paciente}}", nombreCompleto)
                    .Replace("{{fecha_proximo_control}}", fechaFormateada)
                    .Replace("{{nombre_profesional}}", nombreProfesional)
                    .Replace("{{nombre_institucion}}", remitente);

                string logoEfectivo = logoPath;
                string? logoTempPath = null;

                if (!string.IsNullOrEmpty(institucion?.Logo))
                {
                    logoTempPath = Path.Combine(Path.GetTempPath(), $"logo_inst_{usuarioConectado.IdInstitucion}.png");
                    System.IO.File.WriteAllBytes(logoTempPath, Convert.FromBase64String(institucion.Logo));
                    logoEfectivo = logoTempPath;
                }

                var imagenesCorreo = new List<(string Path, string ContentId, string Mime)>
                {
                    (logoEfectivo, "logoImage", "image/png")
                };

                var correo = new EnvioCorreo
                {
                    Destinatarios = new List<string> { paciente.Email },
                    Asunto = $"Recordatorio: tu próximo control es el {fechaFormateada}",
                    CuerpoCorreo = cuerpo
                };

                new Utileria().EnviarCorreo(correo, imagenesCorreo, remitente);

                if (logoTempPath != null && System.IO.File.Exists(logoTempPath))
                    System.IO.File.Delete(logoTempPath);

                return Json(new { success = true, message = $"Correo enviado a {paciente.Email}" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error EnviarCorreoProximoControl: {ex}");
                return Json(new { success = false, message = "Error al enviar el correo." });
            }
        }

        public ActionResult Documentos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GrabarDocumentos(string archivosAdjuntos)
        {
            return View();
        }
    }
}
