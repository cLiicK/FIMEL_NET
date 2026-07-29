using Fimel.Models;
using Fimel.Models.Extensions;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using static Fimel.Models.Enums;

namespace Fimel.Site.Controllers
{
    public class FinancieroController : Controller
    {
        private readonly IConfiguration _config;
        private readonly APIClient APIBase;

        public FinancieroController(IConfiguration config)
        {
            _config = config;
            APIBase = new APIClient(config["API_URL"]);
        }

        private Usuarios? ObtenerUsuario() =>
            new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

        // ── VISTAS ─────────────────────────────────────────────────────────────

        public ActionResult Index()
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return RedirectToAction("Index", "Login");

            bool esSuperAdmin = usuario.IdInstitucion == null;
            bool esAdmin = !esSuperAdmin && usuario.TienePerfil(EnumPerfiles.Administrador);
            bool esEspecialista = usuario.TienePerfil(EnumPerfiles.Especialista);

            ViewBag.UsuarioId = usuario.Id;
            ViewBag.InstitucionId = usuario.IdInstitucion;
            ViewBag.EsSuperAdmin = esSuperAdmin;
            ViewBag.EsAdmin = esAdmin;
            ViewBag.EsEspecialista = esEspecialista;

            if (esSuperAdmin)
            {
                var instituciones = APIBase.Get<List<Instituciones>>("Instituciones/GetAll") ?? new List<Instituciones>();
                ViewBag.Instituciones = instituciones;
            }
            else
            {
                var categorias = APIBase.Get<List<CategoriaFinanciera>>($"CategoriasFinancieras/GetByInstitucion/{usuario.IdInstitucion}") ?? new List<CategoriaFinanciera>();
                ViewBag.Categorias = categorias;
            }

            return View();
        }

        public ActionResult Categorias()
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return RedirectToAction("Index", "Login");

            bool esSuperAdmin = usuario.IdInstitucion == null;

            if (!esSuperAdmin && usuario.TienePerfil(EnumPerfiles.Especialista))
                return RedirectToAction("Index");

            ViewBag.EsSuperAdmin = esSuperAdmin;
            ViewBag.InstitucionId = usuario.IdInstitucion;

            if (esSuperAdmin)
            {
                var instituciones = APIBase.Get<List<Instituciones>>("Instituciones/GetAll") ?? new List<Instituciones>();
                ViewBag.Instituciones = instituciones;
                ViewBag.Categorias = new List<CategoriaFinanciera>();
            }
            else
            {
                var categorias = APIBase.Get<List<CategoriaFinanciera>>($"CategoriasFinancieras/GetByInstitucion/{usuario.IdInstitucion}") ?? new List<CategoriaFinanciera>();
                ViewBag.Categorias = categorias;
            }

            return View();
        }

        // ── MOVIMIENTOS ─────────────────────────────────────────────────────────

        [HttpPost]
        public ActionResult _GetMovimientos(int? idInstitucion, int? idUsuario, DateTime? fechaDesde, DateTime? fechaHasta, string? tipo)
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false, message = "Sesión no válida." });

            try
            {
                bool esSuperAdmin = usuario.IdInstitucion == null;
                bool esEspecialista = usuario.TienePerfil(EnumPerfiles.Especialista);

                // Aplicar restricciones según rol
                if (!esSuperAdmin)
                    idInstitucion = usuario.IdInstitucion;

                if (esEspecialista)
                    idUsuario = usuario.Id;

                var query = new Dictionary<string, string?>();
                if (idInstitucion.HasValue) query["idInstitucion"] = idInstitucion.ToString();
                if (idUsuario.HasValue)    query["idUsuario"] = idUsuario.ToString();
                if (fechaDesde.HasValue)   query["fechaDesde"] = fechaDesde.Value.ToString("yyyy-MM-dd");
                if (fechaHasta.HasValue)   query["fechaHasta"] = fechaHasta.Value.ToString("yyyy-MM-dd");
                if (!string.IsNullOrEmpty(tipo)) query["tipo"] = tipo;

                var movimientos = APIBase.Get<List<MovimientoFinanciero>>("MovimientosFinancieros/GetByCriteria", query) ?? new List<MovimientoFinanciero>();

                // Cargar categorías si el instit fue filtrada para dropdown
                List<CategoriaFinanciera> categorias = new();
                if (!esSuperAdmin && usuario.IdInstitucion.HasValue)
                    categorias = APIBase.Get<List<CategoriaFinanciera>>($"CategoriasFinancieras/GetByInstitucion/{usuario.IdInstitucion}") ?? new List<CategoriaFinanciera>();
                else if (idInstitucion.HasValue)
                    categorias = APIBase.Get<List<CategoriaFinanciera>>($"CategoriasFinancieras/GetByInstitucion/{idInstitucion}") ?? new List<CategoriaFinanciera>();

                return Json(new { success = true, data = movimientos, categorias });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GetMovimientos: {ex}");
                return Json(new { success = false, message = "Error al obtener movimientos." });
            }
        }

        [HttpPost]
        public ActionResult _GuardarMovimiento(MovimientoFinanciero movimiento)
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false, message = "Sesión no válida." });

            try
            {
                movimiento.UsuarioId = usuario.Id;
                movimiento.InstitucionId = usuario.IdInstitucion ?? movimiento.InstitucionId;

                var result = APIBase.Post<MovimientoFinanciero>("MovimientosFinancieros", movimiento);
                return Json(new { success = true, id = result?.Id });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GuardarMovimiento: {ex}");
                return Json(new { success = false, message = "Error al guardar el movimiento." });
            }
        }

        [HttpPost]
        public ActionResult _ActualizarMovimiento(int id, MovimientoFinanciero movimiento)
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false, message = "Sesión no válida." });

            try
            {
                APIBase.Put<MovimientoFinanciero>($"MovimientosFinancieros/{id}", movimiento);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _ActualizarMovimiento: {ex}");
                return Json(new { success = false, message = "Error al actualizar el movimiento." });
            }
        }

        [HttpPost]
        public ActionResult _EliminarMovimiento(int id)
        {
            try
            {
                APIBase.Delete<bool>($"MovimientosFinancieros/{id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _EliminarMovimiento: {ex}");
                return Json(new { success = false, message = "Error al eliminar el movimiento." });
            }
        }

        // ── CATEGORÍAS ──────────────────────────────────────────────────────────

        [HttpGet]
        public ActionResult _GetCategorias(int idInstitucion)
        {
            try
            {
                var categorias = APIBase.Get<List<CategoriaFinanciera>>($"CategoriasFinancieras/GetByInstitucion/{idInstitucion}") ?? new List<CategoriaFinanciera>();
                return Json(new { success = true, data = categorias });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GetCategorias: {ex}");
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public ActionResult _GuardarCategoria(CategoriaFinanciera categoria)
        {
            Usuarios? usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false, message = "Sesión no válida." });

            try
            {
                if (categoria.InstitucionId == 0)
                    categoria.InstitucionId = usuario.IdInstitucion ?? 0;

                var result = APIBase.Post<CategoriaFinanciera>("CategoriasFinancieras", categoria);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GuardarCategoria: {ex}");
                return Json(new { success = false, message = "Error al guardar la categoría." });
            }
        }

        [HttpPost]
        public ActionResult _ActualizarCategoria(int id, CategoriaFinanciera categoria)
        {
            try
            {
                APIBase.Put<CategoriaFinanciera>($"CategoriasFinancieras/{id}", categoria);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _ActualizarCategoria: {ex}");
                return Json(new { success = false, message = "Error al actualizar la categoría." });
            }
        }

        [HttpPost]
        public ActionResult _EliminarCategoria(int id)
        {
            try
            {
                APIBase.Delete<bool>($"CategoriasFinancieras/{id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _EliminarCategoria: {ex}");
                return Json(new { success = false, message = "Error al eliminar la categoría." });
            }
        }

        // ── AUXILIARES ──────────────────────────────────────────────────────────

        [HttpGet]
        public ActionResult _GetConsultasByPaciente(int idPaciente)
        {
            try
            {
                var consultas = APIBase.Get<List<Consultas>>($"Consultas/GetByIdPaciente/{idPaciente}") ?? new List<Consultas>();
                var resultado = consultas.Select(c => new
                {
                    c.Id,
                    Fecha = c.FechaConsulta?.ToString("dd/MM/yyyy") ?? "Sin fecha",
                    c.TipoConsulta,
                    c.MotivoConsulta
                });
                return Json(new { success = true, data = resultado });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error _GetConsultasByPaciente: {ex}");
                return Json(new { success = false });
            }
        }
    }
}
