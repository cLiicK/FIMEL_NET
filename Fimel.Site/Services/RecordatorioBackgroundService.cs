using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Utils;

namespace Fimel.Site.Services
{
    public class RecordatorioBackgroundService : BackgroundService
    {
        private readonly ILogger<RecordatorioBackgroundService> _logger;
        private static readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static readonly APIClient _apiClient = new APIClient(_config["API_URL"]);

        private const int HoraEjecucion = 8;

        public RecordatorioBackgroundService(ILogger<RecordatorioBackgroundService> logger)
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
                _logger.LogInformation("Servicio de recordatorios manuales esperará {Minutos} minutos hasta la próxima ejecución.", (int)demora.TotalMinutes);

                await Task.Delay(demora, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    EnviarRecordatoriosManuales();
            }
        }

        private void EnviarRecordatoriosManuales()
        {
            try
            {
                _logger.LogInformation("Ejecutando envío de recordatorios manuales.");

                List<Recordatorio> recordatorios = _apiClient.Get<List<Recordatorio>>("Recordatorios/GetPendientesParaEnvioHoy");

                if (recordatorios == null || recordatorios.Count == 0)
                {
                    _logger.LogInformation("No hay recordatorios manuales pendientes de envío hoy.");
                    return;
                }

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-recordatorio-manual.html");
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");

                if (!File.Exists(templatePath))
                {
                    _logger.LogError("No se encontró la plantilla de correo: {Path}", templatePath);
                    return;
                }

                string templateHtml = File.ReadAllText(templatePath);
                var utileria = new Utileria();
                int enviados = 0;

                foreach (var recordatorio in recordatorios)
                {
                    try
                    {
                        var paciente = recordatorio.Paciente;
                        if (paciente == null || string.IsNullOrEmpty(paciente.Email)) continue;

                        string nombreCompleto = $"{paciente.Nombres} {paciente.PrimerApellido}".Trim();
                        string nombreProfesional = $"{paciente.UsuarioConectado?.Nombres} {paciente.UsuarioConectado?.ApellidoPaterno}".Trim();
                        string remitente = paciente.UsuarioConectado?.Institucion?.RazonSocial ?? "FIMEL";

                        string cuerpo = templateHtml
                            .Replace("{{titulo_recordatorio}}", recordatorio.Titulo)
                            .Replace("{{cuerpo_recordatorio}}", recordatorio.Cuerpo)
                            .Replace("{{nombre_paciente}}", nombreCompleto)
                            .Replace("{{nombre_profesional}}", nombreProfesional)
                            .Replace("{{nombre_institucion}}", remitente);

                        string logoEfectivo = logoPath;
                        string? logoBase64 = paciente.UsuarioConectado?.Institucion?.Logo;
                        string? logoTempPath = null;

                        if (!string.IsNullOrEmpty(logoBase64))
                        {
                            logoTempPath = Path.Combine(Path.GetTempPath(), $"logo_inst_{paciente.UsuarioConectado!.IdInstitucion}.png");
                            File.WriteAllBytes(logoTempPath, Convert.FromBase64String(logoBase64));
                            logoEfectivo = logoTempPath;
                        }

                        var imagenesCorreo = new List<(string Path, string ContentId, string Mime)>
                        {
                            (logoEfectivo, "logoImage", "image/png")
                        };

                        var correo = new EnvioCorreo
                        {
                            Destinatarios = new List<string> { paciente.Email },
                            Asunto = $"Recordatorio: {recordatorio.Titulo}",
                            CuerpoCorreo = cuerpo
                        };

                        utileria.EnviarCorreo(correo, imagenesCorreo, remitente);
                        enviados++;

                        if (logoTempPath != null && File.Exists(logoTempPath))
                            File.Delete(logoTempPath);

                        _apiClient.Put($"Recordatorios/MarcarEnviado/{recordatorio.Id}", recordatorio);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar recordatorio manual, Id={Id}.", recordatorio.Id);
                    }
                }

                _logger.LogInformation("Recordatorios manuales enviados: {Enviados}/{Total}.", enviados, recordatorios.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servicio de recordatorios manuales.");
            }
        }
    }
}
