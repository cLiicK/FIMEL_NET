using Fimel.Models;
using Fimel.Site.Services;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    /// <summary>
    /// Herramientas de prueba que solo responden con ASPNETCORE_ENVIRONMENT=Development (en el servidor devuelven 404).
    /// </summary>
    public class DesarrolloController : Controller
    {
        private readonly APIClient _api;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DesarrolloController> _logger;

        public DesarrolloController(IConfiguration config, IWebHostEnvironment env, ILogger<DesarrolloController> logger)
        {
            _api = new APIClient(config["API_URL"]);
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Envía ahora el recordatorio de la cita indicada, sin esperar a las 08:00.
        /// Con ?para=correo se envía a esa casilla en vez de la del paciente.
        /// Ej.: /Desarrollo/EnviarRecordatorio/123?para=yo@correo.cl
        /// </summary>
        [HttpGet("Desarrollo/EnviarRecordatorio/{id:int}")]
        public IActionResult EnviarRecordatorio(int id, string? para)
        {
            if (!_env.IsDevelopment()) return NotFound();

            Cita? cita = _api.Get<Cita>($"Citas/{id}");
            if (cita == null) return NotFound($"No existe la cita {id} (o no está vigente).");

            if (cita.Usuario?.IdInstitucion is int idInst)
                cita.Usuario.Institucion = _api.Get<Instituciones>($"Instituciones/{idInst}");

            if (!string.IsNullOrWhiteSpace(para)) cita.CorreoPaciente = para.Trim();

            RecordatorioCitaBackgroundService.EnviarRecordatorio(cita, _logger);
            return Content($"Recordatorio de la cita {id} enviado a {cita.CorreoPaciente}. Botones: {(EnlacesCita.Disponible ? "sí" : "no (falta ApiSeguridad:ApiKey)")}.");
        }
    }
}
