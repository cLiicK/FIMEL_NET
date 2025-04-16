using System.Text.Json;
using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Site.ViewModels;
using Fimel.Utils;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Fimel.Site.Controllers
{
    public class LoginController : Controller
    {
        private static IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly APIClient APIBase = new APIClient(config["API_URL"]);

        public IActionResult Index()
        {
            return RedirectToAction("Login", "Login");
        }

        public ActionResult Login()
        {
            HttpContext.Session.Remove("UsuarioConectado");
            return View(new Usuarios());
        }

        [HttpPost]
        public ActionResult Login(string usuario, string password)
        {
            Usuarios usuarioBD = APIBase.Get<Usuarios>($"Usuarios/GetByNombreUsuario/{usuario}");

            if (usuarioBD != null)
            {
                if (usuarioBD.Password == password)
                {
                    if (usuarioBD.RequiereCambioClave == "S")
                    {
                        string idEncriptado = Utileria.Encrypt(usuarioBD.Id.ToString());
                        return RedirectToAction("CambiarContrasenia", "Login", new { p = idEncriptado });
                    }
                    else
                    {
                        Instituciones institucion = APIBase.Get<Instituciones>($"Instituciones/{usuarioBD.IdInstitucion}");

                        HttpContext.Session.SetString("InstitucionSesion", JsonSerializer.Serialize(institucion));
                        HttpContext.Session.SetString("UsuarioConectado", JsonSerializer.Serialize(usuarioBD));
                        return RedirectToAction("Inicio", "Login");
                    }
                }
                else
                    ViewBag.MensajeError = "Contraseña incorrecta";
            }
            else
                ViewBag.MensajeError = "Usuario no existe";

            return View();
        }

        public ActionResult Inicio()
        {
            return View();
        }

        public ActionResult PruebaHTmlToPDF()
        {
            string pathHtmlSource = Path.Combine(Directory.GetCurrentDirectory(), "Views", "Generics", "receta.html");
            byte[] pdfBytes = new Utileria().HtmlToPDF(pathHtmlSource);
            return File(pdfBytes, "application/pdf", "Receta.pdf");
        }

        public ActionResult RecuperarContrasenia()
        {
            HttpContext.Session.Remove("UsuarioConectado");
            return View(new Usuarios());
        }

        [HttpPost]
        public ActionResult RecuperarContrasenia(string email)
        {
            try
            {
                Usuarios usuario = APIBase.Get<Usuarios>($"Usuarios/GetByNombreUsuario/{email}");
                string asunto = string.Empty;
                string cuerpoCorreo = string.Empty;
                string enlaceEncriptado = string.Empty;

                if (usuario != null)
                {
                    string idEncriptado = Utileria.Encrypt(usuario.Id.ToString());
                    enlaceEncriptado = string.Format("{0}Login/CambiarContrasenia?p={1}", config["URL_SITIO"], idEncriptado);
                    asunto = "Solicitaste recuperar tu contraseña";

                    CorreoVM vm = new CorreoVM()
                    {
                        IdEncriptado = enlaceEncriptado,
                        Usuario = usuario
                    };

                    string body = ConvertViewToString("Plantillas/_Plantilla_RecuperarContrasenia", vm);

                    List<string> destinatarios = new List<string>();
                    destinatarios.Add(email);

                    EnvioCorreo correo = new EnvioCorreo()
                    {
                        Asunto = asunto,
                        CuerpoCorreo = body,
                        Destinatarios = destinatarios
                    };

                    new Utileria().EnviarCorreo(correo);
                    ViewBag.MensajeExito = "Recibirás un correo con instrucciones para reiniciar tu contraseña";
                }
                else
                    ViewBag.MensajeError = "No tenemos un usuario registrado con este email. Favor contáctese con el administrador.";

            }
            catch (Exception ex)
            {
                Logger.Log($"Error al RecuperarContrasena: {ex}");
                ViewBag.MensajeError = "Ha ocurrido un problema. Favor contáctese con el administrador.";
            }
            return View("RecuperarContrasenia", null);
        }

        [IgnoreAntiforgeryToken]
        public IActionResult CambiarContrasenia()
        {
            try
            {
                string idUsuarioEncriptado = HttpContext.Request.Query["p"];

                if (!string.IsNullOrEmpty(idUsuarioEncriptado))
                    ViewBag.UsuarioEncryptado = idUsuarioEncriptado;
                else
                    ViewBag.MensajeError = "Ha ocurrido un problema. Favor contáctese con el administrador.";

            }
            catch (Exception ex)
            {
                Logger.Log($"Error al CambiarContrasenia: {ex}");
                ViewBag.MensajeError = "Ha ocurrido un problema. Favor contáctese con el administrador.";
            }

            HttpContext.Session.Remove("UsuarioConectado");
            return View();
        }

        [HttpPost]
        public ActionResult GuardarNuevaContrasenia(string password1, string password2, string usuarioEncryptado)
        {
            string idUsuario;
            RespuestaAPI response = new RespuestaAPI();
            try
            {
                if (password1 != password2)
                {
                    ViewBag.MensajeError = "Las contraseñas no coinciden"; 
                    return View();
                }

                if (password1.Length < 8)
                {
                    ViewBag.MensajeError = "La contraseña debe tener mínimo 8 caracteres"; 
                    return View();
                }

                idUsuario = Utileria.Decrypt(usuarioEncryptado);
                if (!string.IsNullOrEmpty(idUsuario))
                {
                    Usuarios usuario = APIBase.Get<Usuarios>($"Usuarios/{idUsuario}");
                    usuario.RequiereCambioClave = "N";
                    usuario.Password = password1;
                    Usuarios usuarioPut = APIBase.Put<Usuarios>($"Usuarios/{idUsuario}", usuario);
                    if (usuarioPut != null)
                    {
                        response.Codigo = 200;
                        response.Mensaje = "ACTUALIZADO CORRECTAMENTE";
                    }
                }
                else
                {
                    response.Codigo = 500;
                    response.Mensaje = "ERROR AL ACTUALIZAR USUARIO";
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Actualizar Ususario: {ex}");
                response.Codigo = 500;
                response.Mensaje = "ERROR AL ACTUALIZAR USUARIO";
            }
            return Json(response);

        }

        protected string ConvertViewToString(string viewName, object model)
        {
            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                ICompositeViewEngine viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                if (!viewResult.Success)
                {
                    throw new InvalidOperationException($"Could not find view {viewName}");
                }

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext);
                return writer.ToString();
            }
        }
    }
}
