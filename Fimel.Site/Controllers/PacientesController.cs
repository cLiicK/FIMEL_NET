using Fimel.Models;
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
