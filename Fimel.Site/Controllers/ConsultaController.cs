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
        private readonly APIClient APIBase;
        public ConsultaController(IConfiguration config) => APIBase = new APIClient(config["API_URL"]);


        [HttpGet]
        public ActionResult GetTiposConsulta()
        {
            try
            {
                var lista = APIBase.Get<List<TipoConsulta>>("TiposConsulta/GetAll") ?? new List<TipoConsulta>();
                return Json(new { success = true, data = lista });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetTiposConsulta: {ex}");
                return Json(new { success = false, data = new List<TipoConsulta>() });
            }
        }

        public ActionResult NuevaConsulta()
        {
            Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
            if (usuario != null)
            {
                ViewBag.NombreDoctor = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();
                ViewBag.IdInstitucion = usuario.IdInstitucion;

                ConfiguracionUsuario configUsuario = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{usuario.Id}");
                ViewBag.TituloProfesional = configUsuario?.TituloProfesional ?? "Matrón/a";
                ViewBag.FirmaProfesional = configUsuario?.Firma ?? "";
                ViewBag.RutProfesional = usuario.Rut.HasValue
                    ? new Utileria().FormatearRut(usuario.Rut.Value, usuario.Dv ?? "")
                    : "";

                if (usuario.IdInstitucion.HasValue)
                {
                    Instituciones inst = APIBase.Get<Instituciones>($"Instituciones/{usuario.IdInstitucion}");
                    ViewBag.NombreInstitucion = inst?.RazonSocial ?? "FIMEL";
                    ViewBag.LogoInstitucion = inst?.Logo ?? "";
                }
                else
                {
                    ViewBag.NombreInstitucion = "FIMEL";
                    ViewBag.LogoInstitucion = "";
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
                _consulta.TipoConsultaId = _datosConsulta.TipoConsultaId;
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

        private string ObtenerLogoDataUri(Instituciones? institucion)
        {
            if (!string.IsNullOrEmpty(institucion?.Logo))
                return $"data:image/png;base64,{institucion.Logo}";

            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");
            if (System.IO.File.Exists(logoPath))
                return $"data:image/png;base64,{Convert.ToBase64String(System.IO.File.ReadAllBytes(logoPath))}";

            return "";
        }

        private string ObtenerFirmaImgHtml(string? firmaBase64)
        {
            return string.IsNullOrEmpty(firmaBase64)
                ? ""
                : $"<img class='firma-img' src='data:image/png;base64,{firmaBase64}' alt='Firma'>";
        }

        private static string SanitizarNombreArchivo(string nombre)
        {
            string resultado = nombre ?? "";
            foreach (char c in Path.GetInvalidFileNameChars())
                resultado = resultado.Replace(c, '_');
            return resultado.Replace(" ", "_");
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

                ConfiguracionUsuario configUsuario = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{usuarioConectado.Id}");
                string tituloProfesional = configUsuario?.TituloProfesional ?? "Matrón/a";

                var medicamentos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(medicamentosJson ?? "[]");

                var medicamentosHtml = new System.Text.StringBuilder();
                for (int i = 0; i < medicamentos.Count; i++)
                {
                    var m = medicamentos[i];
                    var detalle = new List<string>();
                    if (!string.IsNullOrEmpty(m.GetValueOrDefault("dosis", ""))) detalle.Add(m["dosis"]);
                    if (!string.IsNullOrEmpty(m.GetValueOrDefault("posologia", ""))) detalle.Add(m["posologia"]);
                    medicamentosHtml.Append($@"<li class='med-item'>
                        <div class='med-bullet'>{i + 1}</div>
                        <div class='med-body'>
                            <div class='med-nombre'>{m.GetValueOrDefault("medicamento", "")}</div>
                            {(detalle.Count > 0 ? $"<div class='med-detalle'>{string.Join(" &nbsp;&middot;&nbsp; ", detalle)}</div>" : "")}
                        </div>
                    </li>");
                }

                // Genera el PDF de la receta a partir de la misma plantilla usada para impresión
                string plantillaRecetaPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "receta-medica.html");
                string htmlReceta = System.IO.File.ReadAllText(plantillaRecetaPath)
                    .Replace("{{logo_url}}", ObtenerLogoDataUri(institucion))
                    .Replace("{{institucion}}", nombreInstitucion)
                    .Replace("{{titulo}}", tituloProfesional)
                    .Replace("{{doctor}}", nombreDoctor)
                    .Replace("{{fecha}}", fechaConsulta)
                    .Replace("{{hora}}", DateTime.Now.ToString("HH:mm"))
                    .Replace("{{paciente}}", nombrePaciente)
                    .Replace("{{rut_doc}}", rutPaciente)
                    .Replace("{{edad}}", edadPaciente)
                    .Replace("{{medicamentos}}", medicamentosHtml.ToString())
                    .Replace("{{firma_img}}", ObtenerFirmaImgHtml(configUsuario?.Firma))
                    .Replace("{{rut_profesional}}", usuarioConectado.Rut.HasValue
                        ? new Utileria().FormatearRut(usuarioConectado.Rut.Value, usuarioConectado.Dv ?? "")
                        : "");

                byte[] pdfBytes = new Utileria().HtmlToPdfDesdeContenido(htmlReceta);

                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");
                string logoEfectivo = logoPath;
                string? logoTempPath = null;

                if (!string.IsNullOrEmpty(institucion?.Logo))
                {
                    logoTempPath = Path.Combine(Path.GetTempPath(), $"logo_receta_{usuarioConectado.IdInstitucion}.png");
                    System.IO.File.WriteAllBytes(logoTempPath, Convert.FromBase64String(institucion.Logo));
                    logoEfectivo = logoTempPath;
                }

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-receta-medica.html");
                string cuerpo = System.IO.File.ReadAllText(templatePath)
                    .Replace("{{nombre_institucion}}", nombreInstitucion)
                    .Replace("{{titulo_profesional}}", tituloProfesional)
                    .Replace("{{nombre_doctor}}", nombreDoctor)
                    .Replace("{{fecha_consulta}}", fechaConsulta)
                    .Replace("{{nombre_paciente}}", nombrePaciente);

                var imagenesCorreo = new List<(string Path, string ContentId, string Mime)>
                {
                    (logoEfectivo, "logoImage", "image/png")
                };

                var adjuntos = new List<(byte[] Bytes, string FileName, string Mime)>
                {
                    (pdfBytes, $"Receta_{SanitizarNombreArchivo(nombrePaciente)}_{fechaConsulta}.pdf", "application/pdf")
                };

                var correo = new EnvioCorreo
                {
                    Destinatarios = new List<string> { emailPaciente },
                    Asunto = $"Receta Médica - {nombreDoctor} - {fechaConsulta}",
                    CuerpoCorreo = cuerpo
                };

                new Utileria().EnviarCorreo(correo, imagenesCorreo, nombreInstitucion, adjuntos);

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

        [HttpPost]
        public ActionResult EnviarOrdenExamenes(string emailPaciente, string nombrePaciente, string rutPaciente,
            string edadPaciente, string fechaConsulta, string examenesJson)
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

                ConfiguracionUsuario configUsuario = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{usuarioConectado.Id}");
                string tituloProfesional = configUsuario?.TituloProfesional ?? "Matrón/a";

                var examenes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(examenesJson ?? "[]");

                var examenesHtml = new System.Text.StringBuilder();
                for (int i = 0; i < examenes.Count; i++)
                {
                    var e = examenes[i];
                    string indicaciones = e.GetValueOrDefault("indicaciones", "");
                    examenesHtml.Append($@"<li class='exam-item'>
                        <div class='exam-bullet'>{i + 1}</div>
                        <div class='exam-body'>
                            <div class='exam-nombre'>{e.GetValueOrDefault("examen", "")}</div>
                            {(!string.IsNullOrEmpty(indicaciones) ? $"<div class='exam-detalle'>{indicaciones}</div>" : "")}
                        </div>
                    </li>");
                }

                // Genera el PDF de la orden a partir de la misma plantilla usada para impresión
                string plantillaOrdenPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "orden-examenes.html");
                string htmlOrden = System.IO.File.ReadAllText(plantillaOrdenPath)
                    .Replace("{{logo_url}}", ObtenerLogoDataUri(institucion))
                    .Replace("{{institucion}}", nombreInstitucion)
                    .Replace("{{titulo}}", tituloProfesional)
                    .Replace("{{doctor}}", nombreDoctor)
                    .Replace("{{fecha}}", fechaConsulta)
                    .Replace("{{hora}}", DateTime.Now.ToString("HH:mm"))
                    .Replace("{{paciente}}", nombrePaciente)
                    .Replace("{{rut_doc}}", rutPaciente)
                    .Replace("{{edad}}", edadPaciente)
                    .Replace("{{examenes}}", examenesHtml.ToString())
                    .Replace("{{firma_img}}", ObtenerFirmaImgHtml(configUsuario?.Firma))
                    .Replace("{{rut_profesional}}", usuarioConectado.Rut.HasValue
                        ? new Utileria().FormatearRut(usuarioConectado.Rut.Value, usuarioConectado.Dv ?? "")
                        : "");

                byte[] pdfBytes = new Utileria().HtmlToPdfDesdeContenido(htmlOrden);

                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");
                string logoEfectivo = logoPath;
                string? logoTempPath = null;

                if (!string.IsNullOrEmpty(institucion?.Logo))
                {
                    logoTempPath = Path.Combine(Path.GetTempPath(), $"logo_orden_{usuarioConectado.IdInstitucion}.png");
                    System.IO.File.WriteAllBytes(logoTempPath, Convert.FromBase64String(institucion.Logo));
                    logoEfectivo = logoTempPath;
                }

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-orden-examenes.html");
                string cuerpo = System.IO.File.ReadAllText(templatePath)
                    .Replace("{{nombre_institucion}}", nombreInstitucion)
                    .Replace("{{titulo_profesional}}", tituloProfesional)
                    .Replace("{{nombre_doctor}}", nombreDoctor)
                    .Replace("{{fecha_consulta}}", fechaConsulta)
                    .Replace("{{nombre_paciente}}", nombrePaciente);

                var imagenesCorreo = new List<(string Path, string ContentId, string Mime)>
                {
                    (logoEfectivo, "logoImage", "image/png")
                };

                var adjuntos = new List<(byte[] Bytes, string FileName, string Mime)>
                {
                    (pdfBytes, $"Orden_Examenes_{SanitizarNombreArchivo(nombrePaciente)}_{fechaConsulta}.pdf", "application/pdf")
                };

                var correo = new EnvioCorreo
                {
                    Destinatarios = new List<string> { emailPaciente },
                    Asunto = $"Orden de Exámenes - {nombreDoctor} - {fechaConsulta}",
                    CuerpoCorreo = cuerpo
                };

                new Utileria().EnviarCorreo(correo, imagenesCorreo, nombreInstitucion, adjuntos);

                if (logoTempPath != null && System.IO.File.Exists(logoTempPath))
                    System.IO.File.Delete(logoTempPath);

                return Json(new { success = true, message = $"Orden de exámenes enviada a {emailPaciente}" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error EnviarOrdenExamenes: {ex}");
                return Json(new { success = false, message = "Error al enviar la orden de exámenes." });
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
