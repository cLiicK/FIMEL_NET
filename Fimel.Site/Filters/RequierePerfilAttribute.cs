using Fimel.Models;
using Fimel.Models.Extensions;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static Fimel.Models.Enums;

namespace Fimel.Site.Filters
{
    public class RequierePerfilAttribute : ActionFilterAttribute
    {
        private readonly EnumPerfiles _perfil;

        public RequierePerfilAttribute(EnumPerfiles perfil)
        {
            _perfil = perfil;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Usuarios? usuario = new Utileria().ObtenerSesion(context.HttpContext.Session.GetString("UsuarioConectado"));

            if (!usuario.TienePerfil(_perfil))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
