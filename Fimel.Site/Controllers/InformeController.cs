using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    public class InformeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly APIClient APIBase;

        public InformeController(IConfiguration config)
        {
            _config = config;
            APIBase = new APIClient(config["API_URL"]);
        }

        private Usuarios? ObtenerUsuario() =>
            new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

        // ── VISTAS ─────────────────────────────────────────────────────────────

        public ActionResult NuevoInforme()
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return RedirectToAction("Index", "Login");

            var plantillas = APIBase.Get<List<PlantillaInforme>>($"PlantillasInforme/GetByUsuario/{usuario.Id}") ?? new List<PlantillaInforme>();
            var configUsr = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{usuario.Id}");

            ViewBag.Plantillas = plantillas;
            ViewBag.NombreDoctor = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();
            ViewBag.TituloProfesional = configUsr?.TituloProfesional ?? "Matrón/a";
            ViewBag.UsuarioId = usuario.Id;

            string urlBase = _config["URL_SITIO"]?.TrimEnd('/') ?? "";
            ViewBag.UrlBase = urlBase;

            return View();
        }

        public ActionResult MantenedorPlantillas()
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return RedirectToAction("Index", "Login");

            var plantillas = APIBase.Get<List<PlantillaInforme>>($"PlantillasInforme/GetByUsuario/{usuario.Id}") ?? new List<PlantillaInforme>();
            ViewBag.Plantillas = plantillas;
            ViewBag.UsuarioId = usuario.Id;

            return View();
        }

        // Página pública del celular — sin verificación de sesión
        public ActionResult Dictado(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        // ── DICTADO ─────────────────────────────────────────────────────────────

        [HttpPost]
        public ActionResult _IniciarDictadoCelular()
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false, message = "Sesión no válida." });

            try
            {
                var result = APIBase.Post<object, object>("Dictado/Iniciar", usuario.Id);
                if (result == null) return Json(new { success = false, message = "Error al iniciar sesión de dictado." });

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                string token = data!.token;

                string urlBase = _config["URL_SITIO"]?.TrimEnd('/') ?? "";
                string urlDictado = $"{urlBase}/Informe/Dictado/{token}";

                return Json(new { success = true, token, url = urlDictado });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _IniciarDictadoCelular: {ex}");
                return Json(new { success = false, message = "Error al iniciar dictado." });
            }
        }

        // Llamado desde el celular (sin sesión)
        [HttpPost]
        public ActionResult _RecibirTextoDictado(string token, string texto)
        {
            try
            {
                APIBase.Post<object, object>($"Dictado/{token}/Texto", texto);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _RecibirTextoDictado: {ex}");
                return Json(new { success = false });
            }
        }

        // Polling desde el desktop
        [HttpGet]
        public ActionResult _PollDictado(string token)
        {
            try
            {
                var result = APIBase.Get<object>($"Dictado/{token}/Pendiente");
                return Json(result);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _PollDictado: {ex}");
                return Json(new { activa = false, textos = new List<string>() });
            }
        }

        [HttpPost]
        public ActionResult _CerrarDictado(string token)
        {
            try
            {
                APIBase.Post<object, object>($"Dictado/{token}/Cerrar", new { });
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _CerrarDictado: {ex}");
                return Json(new { success = false });
            }
        }

        // ── PLANTILLAS ──────────────────────────────────────────────────────────

        [HttpPost]
        public ActionResult _GuardarPlantilla(PlantillaInforme plantilla)
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false });

            try
            {
                plantilla.UsuarioId = usuario.Id;
                var result = APIBase.Post<PlantillaInforme>("PlantillasInforme", plantilla);
                return Json(new { success = true, id = result?.Id });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GuardarPlantilla: {ex}");
                return Json(new { success = false, message = "Error al guardar la plantilla." });
            }
        }

        [HttpPost]
        public ActionResult _ActualizarPlantilla(int id, PlantillaInforme plantilla)
        {
            try
            {
                APIBase.Put<PlantillaInforme>($"PlantillasInforme/{id}", plantilla);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _ActualizarPlantilla: {ex}");
                return Json(new { success = false, message = "Error al actualizar la plantilla." });
            }
        }

        [HttpPost]
        public ActionResult _EliminarPlantilla(int id)
        {
            try
            {
                APIBase.Delete<bool>($"PlantillasInforme/{id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _EliminarPlantilla: {ex}");
                return Json(new { success = false, message = "Error al eliminar la plantilla." });
            }
        }

        [HttpGet]
        public ActionResult _ObtenerPlantilla(int id)
        {
            try
            {
                var plantilla = APIBase.Get<PlantillaInforme>($"PlantillasInforme/{id}");
                return Json(new { success = true, data = plantilla });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _ObtenerPlantilla: {ex}");
                return Json(new { success = false });
            }
        }

        // ── CAMPOS ──────────────────────────────────────────────────────────────

        [HttpPost]
        public ActionResult _GuardarCampo(PlantillaCampo campo)
        {
            try
            {
                var result = APIBase.Post<PlantillaCampo>("PlantillasCampo", campo);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GuardarCampo: {ex}");
                return Json(new { success = false, message = "Error al guardar el campo." });
            }
        }

        [HttpPost]
        public ActionResult _ActualizarCampo(int id, PlantillaCampo campo)
        {
            try
            {
                APIBase.Put<PlantillaCampo>($"PlantillasCampo/{id}", campo);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _ActualizarCampo: {ex}");
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public ActionResult _EliminarCampo(int id)
        {
            try
            {
                APIBase.Delete<bool>($"PlantillasCampo/{id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _EliminarCampo: {ex}");
                return Json(new { success = false });
            }
        }

        // ── INFORME ─────────────────────────────────────────────────────────────

        [HttpPost]
        public ActionResult _GuardarInforme(InformeGenerado informe)
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false });

            try
            {
                informe.UsuarioId = usuario.Id;
                var result = APIBase.Post<InformeGenerado>("InformesGenerados", informe);
                return Json(new { success = true, id = result?.Id });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GuardarInforme: {ex}");
                return Json(new { success = false, message = "Error al guardar el informe." });
            }
        }
    }
}
