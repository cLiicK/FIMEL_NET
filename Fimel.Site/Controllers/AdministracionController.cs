using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Site.Filters;
using Fimel.Site.ViewModels;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using static Fimel.Models.Enums;

namespace Fimel.Site.Controllers
{
    [RequierePerfil(EnumPerfiles.SuperAdmin)]
    public class AdministracionController : Controller
    {
        private readonly IConfiguration _config;
        private readonly APIClient APIBase;

        public AdministracionController(IConfiguration config)
        {
            _config = config;
            APIBase = new APIClient(config["API_URL"]);
        }

        // ── VISTAS ─────────────────────────────────────────────────────────────

        public ActionResult Index() => View();

        public ActionResult Usuarios() => View();

        public ActionResult Instituciones() => View();

        public ActionResult Modulos() => View();

        public ActionResult TiposConsulta() => View();

        // ── TIPOS DE CONSULTA ───────────────────────────────────────────────────

        [HttpGet]
        public ActionResult _ObtenerTiposConsulta()
        {
            try
            {
                var lista = APIBase.Get<List<TipoConsulta>>("TiposConsulta/GetAll") ?? new List<TipoConsulta>();
                return Json(new { success = true, data = lista });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _ObtenerTiposConsulta: {ex}");
                return Json(new { success = false, message = "Error al obtener los tipos de consulta." });
            }
        }

        [HttpPost]
        public ActionResult _GuardarTipoConsulta(int id, string nombre, int orden)
        {
            try
            {
                var tipoConsulta = new TipoConsulta { Id = id, Nombre = nombre, Orden = orden };

                if (id == 0)
                    APIBase.Post<TipoConsulta>("TiposConsulta", tipoConsulta);
                else
                    APIBase.Put<TipoConsulta>($"TiposConsulta/{id}", tipoConsulta);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GuardarTipoConsulta: {ex}");
                return Json(new { success = false, message = "Error al guardar el tipo de consulta." });
            }
        }

        [HttpPost]
        public ActionResult _EliminarTipoConsulta(int id)
        {
            try
            {
                APIBase.Delete<bool>($"TiposConsulta/{id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _EliminarTipoConsulta: {ex}");
                return Json(new { success = false, message = "Error al eliminar el tipo de consulta." });
            }
        }

        // ── USUARIOS ────────────────────────────────────────────────────────────

        [HttpGet]
        public ActionResult _ObtenerUsuarios()
        {
            try
            {
                var usuarios = APIBase.Get<List<Usuarios>>("Usuarios/GetAll") ?? new List<Usuarios>();
                var instituciones = APIBase.Get<List<Instituciones>>("Instituciones/GetAll") ?? new List<Instituciones>();
                var perfiles = APIBase.Get<List<Perfiles>>("Perfiles/GetAll") ?? new List<Perfiles>();
                return Json(new { success = true, usuarios, instituciones, perfiles });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _ObtenerUsuarios: {ex}");
                return Json(new { success = false, message = "Error al obtener los usuarios." });
            }
        }

        [HttpPost]
        public ActionResult _GuardarUsuario(Usuarios nuevoUsuario, List<int> perfilIds)
        {
            try
            {
                nuevoUsuario.PerfilesAsignados = (perfilIds ?? new List<int>())
                    .Select(id => new Perfiles { Id = id })
                    .ToList();

                if (!nuevoUsuario.PerfilesAsignados.Any())
                    return Json(new { success = false, message = "Seleccione al menos un perfil." });

                Usuarios? creado = APIBase.Post<Usuarios>("Usuarios", nuevoUsuario);
                if (creado == null)
                    return Json(new { success = false, message = "Error al crear el usuario. Verifique que el nombre de usuario no esté en uso." });

                EnviarCorreoBienvenida(creado);

                return Json(new { success = true, message = "Usuario creado. Se envió un correo para configurar su contraseña." });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GuardarUsuario: {ex}");
                return Json(new { success = false, message = "Error al crear el usuario." });
            }
        }

        [HttpPost]
        public ActionResult _AsignarPerfil(int idUsuario, int idPerfil)
        {
            try
            {
                APIBase.Post<object, object>($"Usuarios/{idUsuario}/Perfiles/{idPerfil}", new { });
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _AsignarPerfil: {ex}");
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public ActionResult _QuitarPerfil(int idUsuario, int idPerfil)
        {
            try
            {
                APIBase.Delete<bool>($"Usuarios/{idUsuario}/Perfiles/{idPerfil}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _QuitarPerfil: {ex}");
                return Json(new { success = false });
            }
        }

        private void EnviarCorreoBienvenida(Usuarios usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario.Email)) return;

                string idEncriptado = Utileria.Encrypt(usuario.Id.ToString());
                string enlace = string.Format("{0}Login/CambiarContrasenia?p={1}", _config["URL_SITIO"], idEncriptado);

                CorreoVM vm = new CorreoVM { Usuario = usuario, IdEncriptado = enlace };
                string body = ConvertViewToString("Plantillas/_Plantilla_BienvenidaUsuario", vm);

                EnvioCorreo correo = new EnvioCorreo
                {
                    Asunto = "Se creó tu cuenta en FIMEL",
                    CuerpoCorreo = body,
                    Destinatarios = new List<string> { usuario.Email }
                };

                new Utileria().EnviarCorreo(correo);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error EnviarCorreoBienvenida: {ex}");
            }
        }

        // ── INSTITUCIONES ───────────────────────────────────────────────────────

        [HttpGet]
        public ActionResult _ObtenerInstituciones()
        {
            try
            {
                var instituciones = APIBase.Get<List<Instituciones>>("Instituciones/GetAll") ?? new List<Instituciones>();
                return Json(new { success = true, data = instituciones });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _ObtenerInstituciones: {ex}");
                return Json(new { success = false, message = "Error al obtener las instituciones." });
            }
        }

        [HttpPost]
        public ActionResult _GuardarInstitucion(Instituciones institucion)
        {
            try
            {
                Instituciones? creada = APIBase.Post<Instituciones>("Instituciones", institucion);
                if (creada == null)
                    return Json(new { success = false, message = "Error al crear la institución." });

                return Json(new { success = true, data = creada });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GuardarInstitucion: {ex}");
                return Json(new { success = false, message = "Error al crear la institución." });
            }
        }

        // ── MÓDULOS ↔ PERFILES ──────────────────────────────────────────────────

        [HttpGet]
        public ActionResult _ObtenerMatriz()
        {
            try
            {
                var modulos = APIBase.Get<List<Modulos>>("Modulos/GetAll") ?? new List<Modulos>();
                var perfiles = APIBase.Get<List<Perfiles>>("Perfiles/GetAll") ?? new List<Perfiles>();
                var pares = APIBase.Get<List<ModuloPerfil>>("ModulosPerfiles/GetAll") ?? new List<ModuloPerfil>();
                return Json(new { success = true, modulos, perfiles, pares });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _ObtenerMatriz: {ex}");
                return Json(new { success = false, message = "Error al obtener la matriz de módulos." });
            }
        }

        [HttpPost]
        public ActionResult _GuardarModulo(Modulos modulo)
        {
            try
            {
                Modulos? creado = APIBase.Post<Modulos>("Modulos", modulo);
                if (creado == null)
                    return Json(new { success = false, message = "Error al crear el módulo." });

                return Json(new { success = true, data = creado });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GuardarModulo: {ex}");
                return Json(new { success = false, message = "Error al crear el módulo." });
            }
        }

        [HttpPost]
        public ActionResult _AsignarModuloPerfil(int idModulo, int idPerfil)
        {
            try
            {
                var par = new ModuloPerfil { ModuloId = idModulo, PerfilId = idPerfil };
                var resultado = APIBase.Post<ModuloPerfil>("ModulosPerfiles", par);
                return Json(new { success = true, id = resultado?.Id });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _AsignarModuloPerfil: {ex}");
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public ActionResult _QuitarModuloPerfil(int id)
        {
            try
            {
                APIBase.Delete<bool>($"ModulosPerfiles/{id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _QuitarModuloPerfil: {ex}");
                return Json(new { success = false });
            }
        }

        // ── HELPERS ─────────────────────────────────────────────────────────────

        protected string ConvertViewToString(string viewName, object model)
        {
            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                ICompositeViewEngine viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                if (!viewResult.Success)
                    throw new InvalidOperationException($"Could not find view {viewName}");

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
