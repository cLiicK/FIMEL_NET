using Fimel.Models;
using Fimel.Site.Services;
using Fimel.Site.ViewModels;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    /// <summary>
    /// Página pública (sin sesión) a la que llega el paciente desde el botón "Unirse a la videoconsulta" de los correos.
    /// El token es aleatorio por cita; el link de Daily (con el token de invitado) nunca sale en el correo,
    /// solo se embebe aquí dentro de la ventana horaria de la sala.
    /// </summary>
    public class VideoconsultaController : Controller
    {
        private readonly APIClient _api;

        public VideoconsultaController(IConfiguration config) => _api = new APIClient(config["API_URL"]);

        [HttpGet("Videoconsulta/Sala/{token}")]
        public IActionResult Sala(string token)
        {
            Response.Headers["X-Robots-Tag"] = "noindex";
            Response.Headers["Referrer-Policy"] = "no-referrer";

            try
            {
                Cita? cita = string.IsNullOrWhiteSpace(token) || token.Length > 64
                    ? null
                    : _api.Get<Cita>($"Citas/GetByVideoToken/{Uri.EscapeDataString(token)}");

                if (cita == null || !cita.EsVideoconsulta)
                    return View("~/Views/CitaPaciente/Estado.cshtml", "Esta videoconsulta no existe o fue anulada. Si tienes dudas, comunícate con tu profesional.");

                VideoconsultaVM vm = VideoconsultaSite.ArmarVM(cita, cita.VideoUrlPaciente);

                // Daily falló al agendar o se habilitó después: se intenta crear la sala ahora
                if (vm.Estado == VideoconsultaVM.EstadoSala.SinSala)
                {
                    Cita? conSala = _api.Put<Cita>($"Citas/{cita.Id}/AsegurarSalaVideo", new Cita());
                    if (!string.IsNullOrWhiteSpace(conSala?.VideoUrlPaciente))
                        vm = VideoconsultaSite.ArmarVM(cita, conSala.VideoUrlPaciente);
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Videoconsulta Sala: {ex}");
                return View("~/Views/CitaPaciente/Estado.cshtml", "No pudimos cargar tu videoconsulta. Intenta nuevamente en unos minutos.");
            }
        }
    }
}
