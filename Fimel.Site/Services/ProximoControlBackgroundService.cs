using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Utils;

namespace Fimel.Site.Services
{
    public class ProximoControlBackgroundService : BackgroundService
    {
        private readonly ILogger<ProximoControlBackgroundService> _logger;
        private static readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static readonly APIClient _apiClient = new APIClient(_config["API_URL"]);

        private const int HoraEjecucion = 8;

        public ProximoControlBackgroundService(ILogger<ProximoControlBackgroundService> logger)
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
                _logger.LogInformation("Servicio de próximo control esperará {Minutos} minutos hasta la próxima ejecución.", (int)demora.TotalMinutes);

                await Task.Delay(demora, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    EnviarCorreosProximoControl();
            }
        }

        private void EnviarCorreosProximoControl()
        {
            try
            {
                _logger.LogInformation("Ejecutando envío de correos de próximo control.");

                List<Consultas> consultas = _apiClient.Get<List<Consultas>>("Consultas/GetProximosControlesParaEnvioHoy");

                if (consultas == null || consultas.Count == 0)
                {
                    _logger.LogInformation("No hay pacientes con próximo control para avisar hoy.");
                    return;
                }

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-proximo-control.html");
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");

                if (!File.Exists(templatePath))
                {
                    _logger.LogError("No se encontró la plantilla de correo: {Path}", templatePath);
                    return;
                }

                string templateHtml = File.ReadAllText(templatePath);
                var utileria = new Utileria();
                int enviados = 0;

                foreach (var consulta in consultas)
                {
                    try
                    {
                        var paciente = consulta.Paciente;
                        if (paciente == null || string.IsNullOrEmpty(paciente.Email)) continue;
                        if (!consulta.FechaProximoControl.HasValue) continue;

                        string nombreCompleto = $"{paciente.Nombres} {paciente.PrimerApellido}".Trim();
                        string fechaFormateada = consulta.FechaProximoControl.Value.ToString("dd/MM/yyyy");
                        string nombreProfesional = $"{paciente.UsuarioConectado?.Nombres} {paciente.UsuarioConectado?.ApellidoPaterno}".Trim();
                        string remitente = paciente.UsuarioConectado?.Institucion?.RazonSocial ?? "FIMEL";

                        string cuerpo = templateHtml
                            .Replace("{{nombre_paciente}}", nombreCompleto)
                            .Replace("{{fecha_proximo_control}}", fechaFormateada)
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
                            Asunto = $"Recordatorio: tu próximo control es el {fechaFormateada}",
                            CuerpoCorreo = cuerpo
                        };

                        utileria.EnviarCorreo(correo, imagenesCorreo, remitente);
                        enviados++;

                        if (logoTempPath != null && File.Exists(logoTempPath))
                            File.Delete(logoTempPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar correo de próximo control, consulta Id={Id}.", consulta.Id);
                    }
                }

                _logger.LogInformation("Correos de próximo control enviados: {Enviados}/{Total}.", enviados, consultas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servicio de próximo control.");
            }
        }
    }
}
