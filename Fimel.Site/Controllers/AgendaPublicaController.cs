using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    public class AgendaPublicaController : Controller
    {
        private static readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly APIClient _api = new APIClient(_config["API_URL"]);

        [Route("AgendaPublica/{token}")]
        public IActionResult Index(string token)
        {
            ConfiguracionUsuario? cfg = _api.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByToken/{token}");
            if (cfg?.Usuario == null) return NotFound();

            if (cfg.Usuario.IdInstitucion.HasValue)
                cfg.Usuario.Institucion = _api.Get<Instituciones>($"Instituciones/{cfg.Usuario.IdInstitucion}");

            ViewBag.Token = token;
            return View(cfg);
        }

        [HttpGet("AgendaPublica/ObtenerCitas")]
        public IActionResult ObtenerCitas(string token, DateTime desde, DateTime hasta)
        {
            try
            {
                ConfiguracionUsuario? cfg = _api.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByToken/{token}");
                if (cfg?.Usuario == null) return Json(new { ok = false });

                var citas = _api.Get<List<Cita>>($"Citas/GetByCriteria", new
                {
                    FechaInicio = desde,
                    FechaTermino = hasta,
                    UsuarioId = cfg.Usuario.Id
                }) ?? new();

                var resultado = citas
                    .Where(c => c.Vigente != "N")
                    .Select(c => new
                    {
                        fecha = c.FechaHoraInicio.ToString("yyyy-MM-dd"),
                        horaInicio = c.FechaHoraInicio.ToString("HH:mm"),
                        horaFin = c.FechaHoraFinal.ToString("HH:mm"),
                        paciente = string.Join(" ", new[] { c.NombrePaciente, c.ApellidoPaciente }
                            .Where(s => !string.IsNullOrWhiteSpace(s)))
                    });

                return Json(new { ok = true, citas = resultado });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error AgendaPublicaController ObtenerCitas: {ex}");
                return Json(new { ok = false });
            }
        }
    }
}
