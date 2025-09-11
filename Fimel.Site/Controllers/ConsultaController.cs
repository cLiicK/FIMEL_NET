using Fimel.Models;
using Fimel.Models.Params;
using Fimel.Site.ViewModels;
using Fimel.Utils;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json;

namespace Fimel.Site.Controllers
{
    public class ConsultaController : Controller
    {
        private static IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly APIClient APIBase = new APIClient(config["API_URL"]);


        public ActionResult NuevaConsulta()
        {
            return View();
        }
        public ActionResult HistorialConsultas()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ObtenerHistorial(QueryBuscarPacientes query)
        {
            try
            {
                Pacientes paciente = null;
                if (query.Rut.HasValue && query.Rut > 0)
                    { paciente = APIBase.Get<Pacientes>($"Pacientes/GetByRut/{query.Rut}"); }
                if (!string.IsNullOrEmpty(query.NumDoc))
                    { paciente = APIBase.Get<Pacientes>($"Pacientes/GetByNumeroDocumento/{query.NumDoc}"); }

                List<Consultas> historial = APIBase.Get<List<Consultas>>($"Consultas/GetByIdPaciente/{paciente.Id}").OrderByDescending(t => t.FechaConsulta).ToList();

                if (query.FechaConsultaDesde.HasValue)
                    historial = historial.Where(x => x.FechaConsulta >= query.FechaConsultaDesde).ToList();
                if (query.FechaConsultaHasta.HasValue)
                    historial = historial.Where(x => x.FechaConsulta <= query.FechaConsultaHasta).ToList();

                return Json(new { success = true, data = historial, dataPaciente = paciente });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Obtener Historial: {ex}");
                return null;
            }
        }

        public ActionResult DetalleConsulta(string idEncrypted)
        {
            try
            {
                string idConsulta = new Utileria().DesencryptarBase64(idEncrypted);

                Consultas consulta = APIBase.Get<Consultas>($"Consultas/{Convert.ToInt32(idConsulta)}");
                Pacientes paciente = APIBase.Get<Pacientes>($"Pacientes/{consulta.Id_Paciente}");

                DetalleConsultaVM vm = new DetalleConsultaVM()
                {
                    Consulta = consulta,
                    Paciente = paciente
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al cargar el DetalleConsulta: {ex}");
                return null;
            }
        }

        public ActionResult Test()
        {
            return View();
        }

        public ActionResult GrabarConsulta(string datosConsulta, int rutPaciente, string numDocumento)
        {
            Usuarios usuarioConectado = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

            if (usuarioConectado == null)
                return RedirectToAction("Login", "Login");

            RespuestaAPI response = new RespuestaAPI();
            try
            {
                Consultas nuevaConsulta = Newtonsoft.Json.JsonConvert.DeserializeObject<Consultas>(datosConsulta);
                Pacientes paciente = null;
                if (rutPaciente != 0)
                    paciente = APIBase.Get<Pacientes>($"Pacientes/GetByRut/{rutPaciente}");
                else
                    paciente = APIBase.Get<Pacientes>($"Pacientes/GetByNumeroDocumento/{numDocumento}");

                nuevaConsulta.Id_Paciente = paciente.Id;
                nuevaConsulta.UsuarioCreacion = usuarioConectado.Id;
                nuevaConsulta = APIBase.Post<Consultas>($"Consultas", nuevaConsulta);
                if (nuevaConsulta != null)
                {
                    response.Codigo = 200;
                    response.Mensaje = "CREADO CORRECTAMENTE";
                }
            }
            catch (Exception ex)
            {
                response.Codigo = 500;
                response.Mensaje = "ERROR AL CREAR CONSULTA";
                Logger.Log($"Error al crear la Consulta: {ex}");
            }
            return Json(response);
        }

        public ActionResult ObtenerConsultasAnteriores(int rutPaciente)
        {
            try
            {
                Pacientes pacienteConsultado = APIBase.Get<Pacientes>($"Pacientes/GetByRut/{rutPaciente}");
                List<Consultas> consultasAnteriores = APIBase.Get<List<Consultas>>($"Consultas/GetByIdPaciente/{pacienteConsultado.Id}").OrderByDescending(t => t.FechaConsulta).ToList();
                return Json(consultasAnteriores);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ObtenerConsultasAnteriores: {ex}");
                return null;
            }
        }

        public ActionResult ObtenerConsultasAnterioresPorNumDocumento(string rutPaciente)
        {
            try
            {
                Pacientes pacienteConsultado = APIBase.Get<Pacientes>($"Pacientes/GetByNumeroDocumento/{rutPaciente}");
                List<Consultas> consultasAnteriores = APIBase.Get<List<Consultas>>($"Consultas/GetByIdPaciente/{pacienteConsultado.Id}").OrderByDescending(t => t.FechaConsulta).ToList();
                return Json(consultasAnteriores);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ObtenerConsultasAnteriores: {ex}");
                return null;
            }
        }

        public ActionResult ObtenerConsulta(int idConsulta)
        {
            try
            {
                Consultas consulta = APIBase.Get<Consultas>($"Consultas/{idConsulta}");
                return Json(consulta);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Obtener la Consulta: {ex}");
                return null;
            }
        }

        public ActionResult ActualizarConsulta(string _datosConsultaJson)
        {
            Consultas _datosConsulta = JsonConvert.DeserializeObject<Consultas>(_datosConsultaJson);

            Consultas _consulta;
            RespuestaAPI response = new RespuestaAPI();
            try
            {
                _consulta = APIBase.Get<Consultas>($"Consultas/{_datosConsulta.Id}");

                _consulta.TipoConsulta = _datosConsulta.TipoConsulta;
                _consulta.Peso = _datosConsulta.Peso;
                _consulta.Talla = _datosConsulta.Talla;
                _consulta.IMC = _datosConsulta.IMC;
                _consulta.PresionArterial = _datosConsulta.PresionArterial;
                _consulta.EstadoNutricional = _datosConsulta.EstadoNutricional;
                _consulta.MotivoConsulta = _datosConsulta.MotivoConsulta;
                _consulta.Anamnesis = _datosConsulta.Anamnesis;
                _consulta.ExamenFisico = _datosConsulta.ExamenFisico;
                _consulta.Diagnostico = _datosConsulta.Diagnostico;
                _consulta.Indicaciones = _datosConsulta.Indicaciones;
                _consulta.Receta = _datosConsulta.Receta;
                _consulta.OrdenExamenes = _datosConsulta.OrdenExamenes;
                
                // Convertir fechas de string a DateTime
                if (!string.IsNullOrEmpty(_datosConsulta.FechaProximoControl?.ToString()))
                {
                    if (DateTime.TryParse(_datosConsulta.FechaProximoControl.ToString(), out DateTime fechaProximoControl))
                    {
                        _consulta.FechaProximoControl = fechaProximoControl;
                    }
                }
                
                if (!string.IsNullOrEmpty(_datosConsulta.FechaConsulta?.ToString()))
                {
                    if (DateTime.TryParse(_datosConsulta.FechaConsulta.ToString(), out DateTime fechaConsulta))
                    {
                        _consulta.FechaConsulta = fechaConsulta;
                    }
                    }
                Consultas actualizada = APIBase.Put<Consultas>($"Consultas/{_datosConsulta.Id}", _consulta);
                if (actualizada != null)
                {
                    response.Codigo = 200;
                    response.Mensaje = "ACTUALIZADO CORRECTAMENTE";
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Actualizar Consulta: {ex}");
                response.Codigo = 500;
                response.Mensaje = "ERROR AL ACTUALIZAR CONSULTA";
            }
            return Json(response);
        }

        public ActionResult EliminarConsulta(int idConsulta)
        {
            try
            {
                bool response = APIBase.Delete<bool>($"Consultas/{idConsulta}");

                return Json(new { success = response });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error EliminarConsulta: {ex}");
                return null;
            }
        }
    }
}
