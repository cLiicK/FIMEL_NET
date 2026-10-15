using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    public class SuscripcionController : Controller
    {
        private readonly IConfiguration config;
        private readonly APIClient APIBase;
        public SuscripcionController(IConfiguration configuration) { config = configuration; APIBase = new APIClient(config["API_URL"]); }

        public ActionResult Index()
        {
            Usuarios? usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
            if (usuario == null)
                return RedirectToAction("Login", "Login");

            SuscripcionUsuario? suscripcion = APIBase.Get<SuscripcionUsuario>($"Suscripciones/GetByUsuario/{usuario.Id}");
            ViewBag.Planes = APIBase.Get<List<PlanSuscripcion>>("Suscripciones/Planes") ?? new List<PlanSuscripcion>();
            return View(suscripcion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Suscribirse(int idPlan)
        {
            Usuarios? usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
            if (usuario == null)
                return RedirectToAction("Login", "Login");

            SolicitudSuscripcion solicitud = new SolicitudSuscripcion
            {
                IdUsuario = usuario.Id,
                IdPlan = idPlan,
                UrlRetorno = $"{config["URL_SITIO"]?.TrimEnd('/')}/Suscripcion/RetornoFlow"
            };

            SolicitudSuscripcion? respuesta = APIBase.Post<SolicitudSuscripcion>("Suscripciones/Iniciar", solicitud);

            if (respuesta?.Exito == true && !string.IsNullOrWhiteSpace(respuesta.UrlFlow))
                return Redirect(respuesta.UrlFlow);

            TempData["MensajeError"] = respuesta?.Mensaje ?? "No se pudo iniciar la suscripción.";
            return RedirectToAction("Index");
        }

        // Flow redirige aquí (POST con "token") después de que el usuario inscribe su tarjeta.
        // Viene desde otro sitio, así que no depende de la sesión ni del antiforgery: el token se verifica con Flow en la API.
        [AcceptVerbs("GET", "POST")]
        [IgnoreAntiforgeryToken]
        public ActionResult RetornoFlow(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["MensajeError"] = "Flow no devolvió el resultado de la inscripción de tarjeta.";
                return RedirectToAction("Index");
            }

            SolicitudSuscripcion? respuesta = APIBase.Post<SolicitudSuscripcion>("Suscripciones/ConfirmarTarjeta", new SolicitudSuscripcion { Token = token });

            if (respuesta?.Exito == true)
                TempData["MensajeExito"] = respuesta.Suscripcion?.Estado == EstadosSuscripcion.Prueba
                    ? $"¡Listo! Tu tarjeta quedó inscrita y tu prueba gratis dura hasta el {respuesta.Suscripcion.FinPrueba:dd/MM/yyyy}."
                    : "¡Listo! Tu tarjeta quedó inscrita y tu suscripción está activa.";
            else
                TempData["MensajeError"] = respuesta?.Mensaje ?? "No se pudo confirmar la inscripción de la tarjeta.";

            return RedirectToAction("Index");
        }
    }
}
