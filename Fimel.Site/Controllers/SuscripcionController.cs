using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Site.Filters;
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

            // Consulta a Flow el estado actual (cobros, morosidad, cancelación) antes de mostrarlo
            SuscripcionUsuario? suscripcion = APIBase.Post<SuscripcionUsuario>($"Suscripciones/Sincronizar/{usuario.Id}", null!);
            SuscripcionActivaFilter.GuardarEnSesion(HttpContext.Session, suscripcion);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar()
        {
            Usuarios? usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
            if (usuario == null)
                return RedirectToAction("Login", "Login");

            SolicitudSuscripcion? respuesta = APIBase.Post<SolicitudSuscripcion>($"Suscripciones/Cancelar/{usuario.Id}", new SolicitudSuscripcion { IdUsuario = usuario.Id });

            if (respuesta?.Exito == true)
                TempData["MensajeExito"] = respuesta.Suscripcion?.PagadoHasta != null
                    ? $"Tu suscripción quedó cancelada. Mantienes el acceso hasta el {respuesta.Suscripcion.PagadoHasta:dd/MM/yyyy} y no se te volverá a cobrar."
                    : "Tu suscripción quedó cancelada y no se te volverá a cobrar.";
            else
                TempData["MensajeError"] = respuesta?.Mensaje ?? "No se pudo cancelar la suscripción.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reactivar(int idPlan)
        {
            Usuarios? usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
            if (usuario == null)
                return RedirectToAction("Login", "Login");

            SolicitudSuscripcion? respuesta = APIBase.Post<SolicitudSuscripcion>("Suscripciones/Reactivar", new SolicitudSuscripcion { IdUsuario = usuario.Id, IdPlan = idPlan });

            if (respuesta?.Exito == true)
                TempData["MensajeExito"] = respuesta.Suscripcion?.PagadoHasta != null
                    ? $"Tu suscripción sigue activa; el próximo cobro será el {respuesta.Suscripcion.PagadoHasta:dd/MM/yyyy}."
                    : "Tu suscripción sigue activa.";
            else
                TempData["MensajeError"] = respuesta?.Mensaje ?? "No se pudo reactivar la suscripción.";

            return RedirectToAction("Index");
        }

        // URL de notificación de los planes en Flow (urlCallback): Flow hace POST con "token" en cada cobro.
        // Es pública y sin sesión; la API verifica todo consultando a Flow, así que el aviso en sí no cambia nada por su cuenta.
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public ActionResult NotificacionFlow(string? token)
        {
            try
            {
                APIBase.Post<SolicitudSuscripcion>("Suscripciones/NotificacionFlow", new SolicitudSuscripcion { Token = token });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al procesar notificación de Flow: {ex}");
            }

            // Flow espera 200 para no reintentar
            return Ok();
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
