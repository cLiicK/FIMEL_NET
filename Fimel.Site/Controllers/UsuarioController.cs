using System.Runtime.Intrinsics.Arm;
using Fimel.Models;
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
    }
}
