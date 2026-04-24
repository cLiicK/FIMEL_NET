using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Utils;

namespace Fimel.Site.Services
{
    public class CumpleanosBackgroundService : BackgroundService
    {
        private readonly ILogger<CumpleanosBackgroundService> _logger;
        private static readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static readonly APIClient _apiClient = new APIClient(_config["API_URL"]);

        private const int HoraEjecucion = 8; // 08:00 AM

        public CumpleanosBackgroundService(ILogger<CumpleanosBackgroundService> logger)
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
                _logger.LogInformation("Servicio de cumpleaños esperará {Minutos} minutos hasta la próxima ejecución.", (int)demora.TotalMinutes);

                await Task.Delay(demora, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    EnviarCorreosCumpleanos();
            }
        }

        private void EnviarCorreosCumpleanos()
        {
            try
            {
                var hoy = DateTime.Today;
                _logger.LogInformation("Ejecutando envío de correos de cumpleaños para {Dia}/{Mes}.", hoy.Day, hoy.Month);

                List<Pacientes> pacientes = _apiClient.Get<List<Pacientes>>(
                    $"Pacientes/GetByCumpleanos?dia={hoy.Day}&mes={hoy.Month}");

                if (pacientes == null || pacientes.Count == 0)
                {
                    _logger.LogInformation("No hay pacientes con cumpleaños hoy.");
                    return;
                }

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-cumpleanos.html");
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");

                if (!File.Exists(templatePath))
                {
                    _logger.LogError("No se encontró la plantilla de correo: {Path}", templatePath);
                    return;
                }

                string templateHtml = File.ReadAllText(templatePath);

                var utileria = new Utileria();
                int enviados = 0;

                foreach (var paciente in pacientes)
                {
                    try
                    {
                        string nombreCompleto = $"{paciente.Nombres} {paciente.PrimerApellido}".Trim();
                        string cuerpo = templateHtml.Replace("{{nombre_paciente}}", nombreCompleto);

                        string remitente = paciente.UsuarioConectado?.Institucion?.RazonSocial ?? "FIMEL";

                        // Usar logo de la institución (base64) si existe, o el logo por defecto
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
                            Asunto = $"¡Feliz Cumpleaños, {paciente.Nombres}!",
                            CuerpoCorreo = cuerpo
                        };

                        utileria.EnviarCorreo(correo, imagenesCorreo, remitente);
                        enviados++;

                        if (logoTempPath != null && File.Exists(logoTempPath))
                            File.Delete(logoTempPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar correo de cumpleaños al paciente Id={Id}.", paciente.Id);
                    }
                }

                _logger.LogInformation("Correos de cumpleaños enviados: {Enviados}/{Total}.", enviados, pacientes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servicio de correos de cumpleaños.");
            }
        }
    }
}
