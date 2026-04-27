using Fimel.Models;
using Fimel.Models.Integraciones;
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
            Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
            if (usuario != null)
            {
                ViewBag.NombreDoctor = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();
                ViewBag.IdInstitucion = usuario.IdInstitucion;

                if (usuario.IdInstitucion.HasValue)
                {
                    Instituciones inst = APIBase.Get<Instituciones>($"Instituciones/{usuario.IdInstitucion}");
                    ViewBag.NombreInstitucion = inst?.RazonSocial ?? "FIMEL";
                }
                else
                {
                    ViewBag.NombreInstitucion = "FIMEL";
                }
            }
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

                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                if (usuario != null)
                {
                    ViewBag.NombreDoctor = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();
                    if (usuario.IdInstitucion.HasValue)
                    {
                        Instituciones inst = APIBase.Get<Instituciones>($"Instituciones/{usuario.IdInstitucion}");
                        ViewBag.NombreInstitucion = inst?.RazonSocial ?? "FIMEL";
                    }
                    else ViewBag.NombreInstitucion = "FIMEL";
                }

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
                //List<Consultas> consultasAnteriores = APIBase.Get<List<Consultas>>($"Consultas/GetByIdPaciente/{pacienteConsultado.Id}").OrderByDescending(t => t.FechaConsulta).ToList();
                List<Consultas> consultasAnteriores = APIBase.Get<List<Consultas>>($"Consultas/GetByIdPaciente/{pacienteConsultado.Id}")
                                                                                                .OrderByDescending(t => t.FechaConsulta ?? t.FechaCreacion).ToList();

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
                List<Consultas> consultasAnteriores = APIBase.Get<List<Consultas>>($"Consultas/GetByIdPaciente/{pacienteConsultado.Id}")
                                                                  .OrderByDescending(t => t.FechaConsulta ?? t.FechaCreacion).ToList();
                
                
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

        public ActionResult ObtenerPlantillasPorTipo(string tipo)
        {
            try
            {
                Usuarios usuarioConectado = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                
                if (usuarioConectado == null)
                    return Json(new { success = false, message = "Usuario no autenticado" });

                List<PlantillaConsulta> plantillas = APIBase.Get<List<PlantillaConsulta>>($"PlantillasConsulta/GetByTipo/{tipo}/{usuarioConectado.Id}");
                return Json(new { success = true, data = plantillas });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ObtenerPlantillasPorTipo: {ex}");
                return Json(new { success = false, message = "Error al obtener las plantillas" });
            }
        }

        [HttpPost]
        public ActionResult GuardarPlantilla(string tipo, string titulo, string contenido)
        {
            try
            {
                Usuarios usuarioConectado = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                
                if (usuarioConectado == null)
                    return Json(new { success = false, message = "Usuario no autenticado" });

                PlantillaConsulta nuevaPlantilla = new PlantillaConsulta
                {
                    Titulo = titulo,
                    Contenido = contenido,
                    TipoPlantilla = tipo,
                    Vigente = "S",
                    FechaCreacion = DateTime.Now,
                    Usuario = usuarioConectado
                };

                PlantillaConsulta plantillaGuardada = APIBase.Post<PlantillaConsulta>("PlantillasConsulta", nuevaPlantilla);
                
                if (plantillaGuardada != null)
                {
                    return Json(new { success = true, message = "Plantilla guardada correctamente" });
                }
                else
                {
                    return Json(new { success = false, message = "Error al guardar la plantilla" });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al GuardarPlantilla: {ex}");
                return Json(new { success = false, message = "Error al guardar la plantilla" });
            }
        }

        [HttpPost]
        public ActionResult ActualizarPlantilla(int id, string tipo, string titulo, string contenido)
        {
            try
            {
                Usuarios usuarioConectado = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                
                if (usuarioConectado == null)
                    return Json(new { success = false, message = "Usuario no autenticado" });

                PlantillaConsulta plantillaActualizada = APIBase.Put<PlantillaConsulta>($"PlantillasConsulta/{id}", new PlantillaConsulta
                {
                    Id = id,
                    Titulo = titulo,
                    Contenido = contenido,
                    TipoPlantilla = tipo,
                    Vigente = "S",
                    Usuario = usuarioConectado
                });
                
                if (plantillaActualizada != null)
                {
                    return Json(new { success = true, message = "Plantilla actualizada correctamente" });
                }
                else
                {
                    return Json(new { success = false, message = "Error al actualizar la plantilla" });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ActualizarPlantilla: {ex}");
                return Json(new { success = false, message = "Error al actualizar la plantilla" });
            }
        }

        [HttpPost]
        public ActionResult EnviarReceta(string emailPaciente, string nombrePaciente, string rutPaciente,
            string edadPaciente, string fechaConsulta, string medicamentosJson)
        {
            try
            {
                Usuarios usuarioConectado = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                if (usuarioConectado == null)
                    return Json(new { success = false, message = "Sesión no válida." });

                if (string.IsNullOrEmpty(emailPaciente))
                    return Json(new { success = false, message = "El paciente no tiene correo registrado." });

                Instituciones? institucion = null;
                if (usuarioConectado.IdInstitucion.HasValue)
                    institucion = APIBase.Get<Instituciones>($"Instituciones/{usuarioConectado.IdInstitucion}");

                string nombreDoctor = $"{usuarioConectado.Nombres} {usuarioConectado.ApellidoPaterno}".Trim();
                string nombreInstitucion = institucion?.RazonSocial ?? "FIMEL";

                var medicamentos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(medicamentosJson ?? "[]");

                var medicRows = new System.Text.StringBuilder();
                for (int i = 0; i < medicamentos.Count; i++)
                {
                    var m = medicamentos[i];
                    medicRows.AppendLine($@"<tr>
                        <td style='padding:6px 10px;border-bottom:1px solid #eee;'>{i + 1}</td>
                        <td style='padding:6px 10px;border-bottom:1px solid #eee;'><strong>{m.GetValueOrDefault("medicamento", "")}</strong></td>
                        <td style='padding:6px 10px;border-bottom:1px solid #eee;'>{m.GetValueOrDefault("dosis", "")}</td>
                        <td style='padding:6px 10px;border-bottom:1px solid #eee;'>{m.GetValueOrDefault("posologia", "")}</td>
                    </tr>");
                }

                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");
                string logoEfectivo = logoPath;
                string? logoTempPath = null;

                if (!string.IsNullOrEmpty(institucion?.Logo))
                {
                    logoTempPath = Path.Combine(Path.GetTempPath(), $"logo_receta_{usuarioConectado.IdInstitucion}.png");
                    System.IO.File.WriteAllBytes(logoTempPath, Convert.FromBase64String(institucion.Logo));
                    logoEfectivo = logoTempPath;
                }

                string cuerpo = $@"<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;max-width:700px;margin:0 auto;padding:20px;'>
<div style='border-bottom:3px solid #0E96CC;padding-bottom:15px;margin-bottom:20px;display:flex;align-items:center;gap:16px;'>
    <img src='cid:logoImage' style='max-height:60px;max-width:180px;' alt='{nombreInstitucion}'>
    <div><h2 style='color:#0E96CC;margin:0;'>RECETA M&Eacute;DICA</h2><p style='color:#666;margin:4px 0 0;'>{nombreInstitucion}</p></div>
</div>
<table style='width:100%;margin-bottom:20px;border-collapse:collapse;'>
    <tr><td style='padding:4px 0;width:50%;'><strong>M&eacute;dico:</strong> {nombreDoctor}</td><td style='padding:4px 0;'><strong>Fecha:</strong> {fechaConsulta}</td></tr>
    <tr><td style='padding:4px 0;'><strong>Paciente:</strong> {nombrePaciente}</td><td style='padding:4px 0;'><strong>Documento:</strong> {rutPaciente}</td></tr>
    <tr><td style='padding:4px 0;'><strong>Edad:</strong> {edadPaciente} a&ntilde;os</td><td></td></tr>
</table>
<h4 style='color:#0E96CC;border-bottom:1px solid #eee;padding-bottom:8px;'>Medicamentos</h4>
<table style='width:100%;border-collapse:collapse;'>
    <thead><tr style='background:#f0f8ff;'>
        <th style='padding:8px 10px;text-align:left;border-bottom:2px solid #0E96CC;width:30px;'>#</th>
        <th style='padding:8px 10px;text-align:left;border-bottom:2px solid #0E96CC;'>Medicamento</th>
        <th style='padding:8px 10px;text-align:left;border-bottom:2px solid #0E96CC;'>Dosis</th>
        <th style='padding:8px 10px;text-align:left;border-bottom:2px solid #0E96CC;'>Posolog&iacute;a</th>
    </tr></thead>
    <tbody>{medicRows}</tbody>
</table>
<div style='margin-top:50px;text-align:right;border-top:1px solid #ccc;padding-top:15px;'>
    <p style='color:#444;margin:0;font-weight:bold;'>{nombreDoctor}</p>
    <p style='color:#888;font-size:0.85rem;margin:4px 0 0;'>{nombreInstitucion}</p>
</div>
</body></html>";

                var imagenesCorreo = new List<(string Path, string ContentId, string Mime)>
                {
                    (logoEfectivo, "logoImage", "image/png")
                };

                var correo = new EnvioCorreo
                {
                    Destinatarios = new List<string> { emailPaciente },
                    Asunto = $"Receta Médica - {nombreDoctor} - {fechaConsulta}",
                    CuerpoCorreo = cuerpo
                };

                new Utileria().EnviarCorreo(correo, imagenesCorreo, nombreInstitucion);

                if (logoTempPath != null && System.IO.File.Exists(logoTempPath))
                    System.IO.File.Delete(logoTempPath);

                return Json(new { success = true, message = $"Receta enviada a {emailPaciente}" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error EnviarReceta: {ex}");
                return Json(new { success = false, message = "Error al enviar la receta." });
            }
        }

        public ActionResult EliminarPlantilla(int id)
        {
            try
            {
                bool eliminada = APIBase.Delete<bool>($"PlantillasConsulta/{id}");
                
                if (eliminada)
                {
                    return Json(new { success = true, message = "Plantilla eliminada correctamente" });
                }
                else
                {
                    return Json(new { success = false, message = "Error al eliminar la plantilla" });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al EliminarPlantilla: {ex}");
                return Json(new { success = false, message = "Error al eliminar la plantilla" });
            }
        }
    }
}
