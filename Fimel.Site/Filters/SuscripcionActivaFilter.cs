using System.Text.Json;
using Fimel.Models;
using Fimel.Models.Extensions;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using static Fimel.Models.Enums;

namespace Fimel.Site.Filters
{
    /// <summary>
    /// Bloquea el sistema a los profesionales sin suscripción vigente y los lleva a "Mi suscripción".
    /// Solo actúa si Suscripcion:Exigir = true. Quedan fuera: usuarios creados antes de Suscripcion:ExigirDesde,
    /// usuarios sin perfil Especialista (secretarias, administrativos) y SuperAdmin.
    /// </summary>
    public class SuscripcionActivaFilter : IActionFilter
    {
        private const string ClaveSesion = "SuscripcionEstado";
        private static readonly TimeSpan VigenciaCache = TimeSpan.FromMinutes(30);

        // Páginas que deben funcionar sin suscripción (login, la propia página de pago, páginas públicas)
        private static readonly HashSet<string> ControladoresLibres = new(StringComparer.OrdinalIgnoreCase)
        {
            "Login", "Suscripcion", "Landing", "AgendaPublica", "Agendar", "Error"
        };

        private readonly IConfiguration config;
        private readonly ITempDataDictionaryFactory tempDataFactory;

        public SuscripcionActivaFilter(IConfiguration config, ITempDataDictionaryFactory tempDataFactory)
        {
            this.config = config;
            this.tempDataFactory = tempDataFactory;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!config.GetValue<bool>("Suscripcion:Exigir"))
                return;

            if (context.ActionDescriptor is not ControllerActionDescriptor accion || ControladoresLibres.Contains(accion.ControllerName))
                return;

            ISession sesion = context.HttpContext.Session;
            Usuarios? usuario = new Utileria().ObtenerSesion(sesion.GetString("UsuarioConectado"));
            if (usuario == null || !RequiereSuscripcion(usuario))
                return;

            if (EstaVigente(ObtenerEstado(sesion, usuario.Id)))
                return;

            if (context.HttpContext.Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                context.Result = new ObjectResult(new { success = false, message = "Tu suscripción no está vigente." }) { StatusCode = 402 };
                return;
            }

            tempDataFactory.GetTempData(context.HttpContext)["MensajeError"] = "Para seguir usando Fimel necesitas una suscripción vigente.";
            context.Result = new RedirectToActionResult("Index", "Suscripcion", null);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

        public static void GuardarEnSesion(ISession sesion, SuscripcionUsuario? suscripcion)
        {
            sesion.SetString(ClaveSesion, JsonSerializer.Serialize(new EstadoCache(suscripcion?.Estado, suscripcion?.PagadoHasta, DateTime.Now)));
        }

        private bool RequiereSuscripcion(Usuarios usuario)
        {
            if (usuario.TienePerfil(EnumPerfiles.SuperAdmin) || !usuario.TienePerfil(EnumPerfiles.Especialista))
                return false;

            // Los usuarios que ya existían al empezar a cobrar quedan exentos
            DateTime? exigirDesde = config.GetValue<DateTime?>("Suscripcion:ExigirDesde");
            if (exigirDesde == null || usuario.FechaCreacion == null || usuario.FechaCreacion < exigirDesde)
                return false;

            return true;
        }

        private EstadoCache ObtenerEstado(ISession sesion, int idUsuario)
        {
            string? json = sesion.GetString(ClaveSesion);
            EstadoCache? cache = string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<EstadoCache>(json);

            if (cache != null && DateTime.Now - cache.Consultado < VigenciaCache)
                return cache;

            SuscripcionUsuario? suscripcion = new APIClient(config["API_URL"]).Get<SuscripcionUsuario>($"Suscripciones/GetByUsuario/{idUsuario}");
            GuardarEnSesion(sesion, suscripcion);
            return new EstadoCache(suscripcion?.Estado, suscripcion?.PagadoHasta, DateTime.Now);
        }

        private static bool EstaVigente(EstadoCache estado)
        {
            return estado.Estado switch
            {
                EstadosSuscripcion.Prueba or EstadosSuscripcion.Activa or EstadosSuscripcion.Exenta => true,
                // Canceló pero ya había pagado el período: mantiene el acceso hasta el final
                EstadosSuscripcion.Cancelada => estado.PagadoHasta != null && estado.PagadoHasta >= DateTime.Today,
                _ => false
            };
        }

        private record EstadoCache(string? Estado, DateTime? PagadoHasta, DateTime Consultado);
    }
}
