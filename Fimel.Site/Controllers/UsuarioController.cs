using System.Runtime.Intrinsics.Arm;
using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Site.ViewModels;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    public class UsuarioController : Controller
    {
        private static IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly APIClient APIBase = new APIClient(config["API_URL"]);

        public ActionResult Configuracion()
        {
            Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

            ConfiguracionUsuario dbConfig = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{usuario.Id}");

            ConfiguracionUsuarioVM vm = new ConfiguracionUsuarioVM();
            vm.Configuracion = dbConfig != null ? dbConfig : new ConfiguracionUsuario();
            vm.Usuario = usuario;

            if (usuario.IdInstitucion.HasValue)
            {
                Instituciones? inst = APIBase.Get<Instituciones>($"Instituciones/{usuario.IdInstitucion}");
                ViewBag.LogoBase64 = inst?.Logo;
            }

            string urlBase = config["URL_SITIO"]?.TrimEnd('/') ?? "";
            ViewBag.UrlPublica = !string.IsNullOrEmpty(vm.Configuracion.TokenPublico)
                ? $"{urlBase}/Agendar/{vm.Configuracion.TokenPublico}"
                : null;

            return View(vm);
        }

        public ActionResult _GuardarConfiguracion(ConfiguracionUsuario config)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                config.Usuario = usuario;

                ConfiguracionUsuario? configPost = APIBase.Post<ConfiguracionUsuario>($"ConfiguracionesUsuario", config);

                if (configPost == null)
                    return Json(new { success = true, message = "Error interno al guardar configuración..." });

                return Json(new { success = true, message = "Configuración Guardada!" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Usuario GuardarConfiguracion: {ex}");
                return null;
            }
        }

        public ActionResult _ActualizarConfiguracion(int id, ConfiguracionUsuario config)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                config.Usuario = usuario;

                ConfiguracionUsuario? configPut = APIBase.Put<ConfiguracionUsuario>($"ConfiguracionesUsuario/{id}", config);

                if (configPut == null)
                    return Json(new { success = true, message = "Error interno al actualizar configuración..." });

                return Json(new { success = true, message = "Configuración Actualizada!" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Usuario _ActualizarConfiguracion: {ex}");
                return null;
            }
        }

        [HttpPost]
        public ActionResult _GenerarTokenPublico()
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
                ConfiguracionUsuario dbConfig = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{usuario.Id}");

                if (dbConfig == null || dbConfig.Id == 0)
                {
                    // Crear configuración por defecto con bloque de 30 minutos
                    var nuevaConfig = new ConfiguracionUsuario
                    {
                        Usuario = usuario,
                        DuracionBloqueHorario = TimeSpan.FromMinutes(30)
                    };
                    dbConfig = APIBase.Post<ConfiguracionUsuario>("ConfiguracionesUsuario", nuevaConfig);

                    if (dbConfig == null || dbConfig.Id == 0)
                        return Json(new { success = false, message = "Error al inicializar la configuración." });
                }

                string token = Guid.NewGuid().ToString("N");
                APIBase.Put<string>($"ConfiguracionesUsuario/{dbConfig.Id}/TokenPublico", token);

                string urlBase = config["URL_SITIO"]?.TrimEnd('/') ?? "";
                return Json(new { success = true, token, url = $"{urlBase}/Agendar/{token}" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Usuario _GenerarTokenPublico: {ex}");
                return Json(new { success = false, message = "Error al generar el enlace." });
            }
        }

        [HttpPost]
        public ActionResult _SubirLogo(IFormFile logo)
        {
            try
            {
                Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

                if (logo == null || logo.Length == 0)
                    return Json(new { success = false, message = "Debe seleccionar una imagen." });

                var extensionesPermitidas = new[] { ".png", ".jpg", ".jpeg" };
                string ext = Path.GetExtension(logo.FileName).ToLower();
                if (!extensionesPermitidas.Contains(ext))
                    return Json(new { success = false, message = "Solo se permiten imágenes PNG o JPG." });

                if (logo.Length > 2 * 1024 * 1024)
                    return Json(new { success = false, message = "El archivo no puede superar 2 MB." });

                if (!usuario.IdInstitucion.HasValue)
                    return Json(new { success = false, message = "El usuario no tiene institución asignada." });

                string logoBase64;
                using (var ms = new MemoryStream())
                {
                    logo.CopyTo(ms);
                    logoBase64 = Convert.ToBase64String(ms.ToArray());
                }

                APIBase.Patch($"Instituciones/{usuario.IdInstitucion}/Logo", logoBase64);

                return Json(new { success = true, message = "Logo guardado correctamente.", logoBase64 });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Usuario _SubirLogo: {ex}");
                return Json(new { success = false, message = "Error al guardar el logo." });
            }
        }

        public ActionResult PruebaCumpleanos(int dia, int mes)
        {
            try
            {
                List<Pacientes> pacientes = APIBase.Get<List<Pacientes>>(
                    $"Pacientes/GetByCumpleanos?dia={dia}&mes={mes}");

                if (pacientes == null || pacientes.Count == 0)
                    return Json(new { success = false, message = $"No hay pacientes con cumpleaños el {dia}/{mes} o no tienen email registrado." });

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-cumpleanos.html");
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");
                string templateHtml = System.IO.File.ReadAllText(templatePath);

                var utileria = new Utileria();
                var enviados = new List<string>();

                foreach (var paciente in pacientes)
                {
                    string nombreCompleto = $"{paciente.Nombres} {paciente.PrimerApellido}".Trim();
                    string cuerpo = templateHtml.Replace("{{nombre_paciente}}", nombreCompleto);

                    string remitente = paciente.UsuarioConectado?.Institucion?.RazonSocial ?? "FIMEL";

                    string logoEfectivo = logoPath;
                    string? logoBase64 = paciente.UsuarioConectado?.Institucion?.Logo;
                    string? logoTempPath = null;

                    if (!string.IsNullOrEmpty(logoBase64))
                    {
                        logoTempPath = Path.Combine(Path.GetTempPath(), $"logo_inst_{paciente.UsuarioConectado!.IdInstitucion}.png");
                        System.IO.File.WriteAllBytes(logoTempPath, Convert.FromBase64String(logoBase64));
                        logoEfectivo = logoTempPath;
                    }

                    var imagenesCorreo = new List<(string Path, string ContentId, string Mime)>
                    {
                        (logoEfectivo, "logoImage", "image/png")
                    };

                    var correo = new EnvioCorreo
                    {
                        Destinatarios = new List<string> { paciente.Email },
                        Asunto = $"¡Feliz Cumpleaños, {paciente.Nombres}!",
                        CuerpoCorreo = cuerpo
                    };

                    utileria.EnviarCorreo(correo, imagenesCorreo, remitente);

                    if (logoTempPath != null && System.IO.File.Exists(logoTempPath))
                        System.IO.File.Delete(logoTempPath);

                    enviados.Add(paciente.Email);
                }

                return Json(new { success = true, message = $"Correos enviados a: {string.Join(", ", enviados)}" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PruebaCumpleanos: {ex}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
