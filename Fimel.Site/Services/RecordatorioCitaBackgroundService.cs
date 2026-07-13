using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Utils;

namespace Fimel.Site.Services
{
    public class RecordatorioCitaBackgroundService : BackgroundService
    {
        private readonly ILogger<RecordatorioCitaBackgroundService> _logger;
        private static readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static readonly APIClient _apiClient = new APIClient(_config["API_URL"]);

        private const int HoraEjecucion = 8;

        public RecordatorioCitaBackgroundService(ILogger<RecordatorioCitaBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var ahora = DateTime.Now;
                var proximaEjecucion = ahora.Date.AddHours(HoraEjecucion);

                if (ahora >= proximaEjecucion)
                    proximaEjecucion = proximaEjecucion.AddDays(1);

                var demora = proximaEjecucion - ahora;
                _logger.LogInformation("Servicio de recordatorio de citas esperará {Minutos} minutos hasta la próxima ejecución.", (int)demora.TotalMinutes);

                await Task.Delay(demora, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    EnviarRecordatoriosCitas();
            }
        }

        private void EnviarRecordatoriosCitas()
        {
            try
            {
                _logger.LogInformation("Ejecutando envío de recordatorios de citas.");

                List<Cita> citas = _apiClient.Get<List<Cita>>("Citas/GetParaRecordatorio");

                if (citas == null || citas.Count == 0)
                {
                    _logger.LogInformation("No hay citas próximas para recordar hoy.");
                    return;
                }

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-confirmacion-cita.html");
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");

                if (!File.Exists(templatePath))
                {
                    _logger.LogError("No se encontró la plantilla: {Path}", templatePath);
                    return;
                }

                string templateHtml = File.ReadAllText(templatePath);
                var utileria = new Utileria();
                int enviados = 0;
                var manana = DateTime.Today.AddDays(1);
                var culture = new System.Globalization.CultureInfo("es-ES");

                foreach (var cita in citas)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(cita.CorreoPaciente)) continue;

                        var profesional = cita.Usuario;
                        string nombreProfesional = profesional != null
                            ? $"{profesional.Nombres} {profesional.ApellidoPaterno}".Trim()
                            : string.Empty;

                        string remitente = profesional?.Institucion?.RazonSocial ?? "FIMEL";
                        string direccion = profesional?.Institucion?.Dirección ?? "Dirección no disponible";
                        string fechaCita = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", culture);
                        string horaCita = cita.FechaHoraInicio.ToString("HH:mm");

                        bool esManana = cita.FechaHoraInicio.Date == manana;
                        string asunto = esManana
                            ? $"Recordatorio: tu cita es mañana a las {horaCita}"
                            : $"Recordatorio: tu cita es en 2 días ({cita.FechaHoraInicio.ToString("dd/MM")})";

                        string cuerpo = templateHtml
                            .Replace("{{paciente}}", cita.NombrePaciente)
                            .Replace("{{profesional}}", nombreProfesional)
                            .Replace("{{fecha_cita}}", fechaCita)
                            .Replace("{{hora_cita}}", horaCita)
                            .Replace("{{direccion}}", direccion);

                        string logoEfectivo = logoPath;
                        string? logoBase64 = profesional?.Institucion?.Logo;
                        string? logoTempPath = null;

                        if (!string.IsNullOrEmpty(logoBase64))
                        {
                            logoTempPath = Path.Combine(Path.GetTempPath(), $"logo_inst_{profesional!.IdInstitucion}.png");
                            File.WriteAllBytes(logoTempPath, Convert.FromBase64String(logoBase64));
                            logoEfectivo = logoTempPath;
                        }

                        var imagenesCorreo = new List<(string Path, string ContentId, string Mime)>
                        {
                            (logoEfectivo, "logoImage", "image/png")
                        };

                        var correo = new EnvioCorreo
                        {
                            Destinatarios = new List<string> { cita.CorreoPaciente },
                            Asunto = asunto,
                            CuerpoCorreo = cuerpo
                        };

                        utileria.EnviarCorreo(correo, imagenesCorreo, remitente);
                        enviados++;

                        if (logoTempPath != null && File.Exists(logoTempPath))
                            File.Delete(logoTempPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar recordatorio de cita Id={Id}.", cita.Id);
                    }
                }

                _logger.LogInformation("Recordatorios de citas enviados: {Enviados}/{Total}.", enviados, citas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servicio de recordatorio de citas.");
            }
        }
    }
}
