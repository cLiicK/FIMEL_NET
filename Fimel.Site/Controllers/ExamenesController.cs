using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    public class ExamenesController : Controller
    {
        private readonly IConfiguration _config;
        private readonly APIClient APIBase;

        public ExamenesController(IConfiguration config)
        {
            _config = config;
            APIBase = new APIClient(config["API_URL"]);
        }

        private Usuarios? ObtenerUsuario() =>
            new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

        public IActionResult Index()
        {
            var usuario = ObtenerUsuario();
            if (usuario == null) return RedirectToAction("Index", "Login");
            return View();
        }

        // ── API PROXY ─────────────────────────────────────────────────────────

        public IActionResult GetCategorias()
        {
            var usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false });
            var data = APIBase.Get<List<CategoriaExamen>>("CategoriaExamenes") ?? new();
            return Json(new { success = true, data });
        }

        public IActionResult GetExamenes()
        {
            var usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false });
            var data = APIBase.Get<List<object>>("TiposExamen") ?? new();
            return Json(new { success = true, data });
        }

        [HttpGet]
        public IActionResult GetSugerencias(string? q)
        {
            var usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false });
            var url = "TiposExamen/GetActivos" + (string.IsNullOrWhiteSpace(q) ? "" : $"?q={Uri.EscapeDataString(q)}");
            var data = APIBase.Get<List<TipoExamen>>(url) ?? new();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public IActionResult GuardarCategoria(CategoriaExamen categoria)
        {
            var usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false });
            if (categoria.Id == 0)
                APIBase.Post<CategoriaExamen>("CategoriaExamenes", categoria);
            else
                APIBase.Put<CategoriaExamen>($"CategoriaExamenes/{categoria.Id}", categoria);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult EliminarCategoria(int id)
        {
            var usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false });
            APIBase.Delete<bool>($"CategoriaExamenes/{id}");
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GuardarExamen(TipoExamen examen)
        {
            var usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false });
            if (examen.Id == 0)
                APIBase.Post<TipoExamen>("TiposExamen", examen);
            else
                APIBase.Put<TipoExamen>($"TiposExamen/{examen.Id}", examen);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult EliminarExamen(int id)
        {
            var usuario = ObtenerUsuario();
            if (usuario == null) return Json(new { success = false });
            APIBase.Delete<bool>($"TiposExamen/{id}");
            return Json(new { success = true });
        }
    }
}
