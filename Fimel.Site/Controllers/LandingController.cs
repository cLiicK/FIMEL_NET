using Fimel.Models.Integraciones;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    public class LandingController : Controller
    {
        private readonly IConfiguration _config;

        public LandingController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contacto(string nombre, string email, string mensaje)
        {
            try
            {
                var correo = new EnvioCorreo
                {
                    Destinatarios = new List<string> { "contacto@fimel.cl" },
                    Asunto = $"Contacto desde la web — {nombre}",
                    CuerpoCorreo = $@"
                        <p><strong>Nombre:</strong> {System.Net.WebUtility.HtmlEncode(nombre)}</p>
                        <p><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(email)}</p>
                        <p><strong>Mensaje:</strong><br>{System.Net.WebUtility.HtmlEncode(mensaje).Replace("\n", "<br>")}</p>"
                };

                var utileria = new Utileria();
                utileria.EnviarCorreo(correo);

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }
    }
}
