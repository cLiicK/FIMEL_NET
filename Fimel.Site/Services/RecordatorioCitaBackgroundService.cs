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

                int enviados = 0, enviadosWhatsApp = 0;

                foreach (var cita in citas)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(cita.CorreoPaciente))
                        {
                            EnviarRecordatorio(cita, _logger);
                            enviados++;
                        }

                        if (WhatsAppCloud.Habilitado && EnviarRecordatorioWhatsAppAsync(cita, _logger).GetAwaiter().GetResult())
                            enviadosWhatsApp++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar recordatorio de cita Id={Id}.", cita.Id);
                    }
                }

                _logger.LogInformation("Recordatorios de citas enviados: {Enviados} por correo y {WhatsApp} por WhatsApp, de {Total} citas.", enviados, enviadosWhatsApp, citas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servicio de recordatorio de citas.");
            }
        }

        /// <summary>
        /// Arma y envía el recordatorio de una cita. Público para poder enviarlo a mano en desarrollo
        /// (ver DesarrolloController), sin esperar a las 08:00.
        /// </summary>
        public static void EnviarRecordatorio(Cita cita, ILogger logger)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mails", "correo-recordatorio-cita.html");
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_fimel_correo.png");
            string templateHtml = File.ReadAllText(templatePath);
            var utileria = new Utileria();
            var manana = DateTime.Today.AddDays(1);
            var culture = new System.Globalization.CultureInfo("es-ES");


                var profesional = cita.Usuario;
                string nombreProfesional = profesional != null
                    ? $"{profesional.Nombres} {profesional.ApellidoPaterno}".Trim()
                    : string.Empty;

                // Sale desde la casilla de Fimel, pero las respuestas le llegan al profesional (Reply-To)
                string remitente = !string.IsNullOrEmpty(nombreProfesional)
                    ? $"Mat. {nombreProfesional} vía FIMEL"
                    : profesional?.Institucion?.RazonSocial ?? "FIMEL";
                string? responderA = !string.IsNullOrWhiteSpace(profesional?.Email)
                    ? profesional!.Email
                    : profesional?.Institucion?.Email;
                string direccion = profesional?.Institucion?.Dirección ?? "Dirección no disponible";
                string fechaCita = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", culture);
                string horaCita = cita.FechaHoraInicio.ToString("HH:mm");

                bool esManana = cita.FechaHoraInicio.Date == manana;
                string asunto = esManana
                    ? $"Recordatorio: tu cita es mañana a las {horaCita}"
                    : $"Recordatorio: tu cita es el {cita.FechaHoraInicio.ToString("dddd dd/MM", culture)} a las {horaCita}";

                string cuerpo = templateHtml
                    .Replace("{{paciente}}", string.Join(" ", new[] { cita.NombrePaciente, cita.ApellidoPaciente, cita.SegundoApellidoPaciente }
                        .Where(x => !string.IsNullOrWhiteSpace(x))))
                    .Replace("{{profesional}}", nombreProfesional)
                    .Replace("{{fecha_cita}}", fechaCita)
                    .Replace("{{hora_cita}}", horaCita)
                    .Replace("{{direccion}}", direccion)
                    .Replace("{{bloque_acciones}}", BloqueAcciones(cita, logger));

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
                    CuerpoCorreo = cuerpo,
                    ResponderA = responderA
                };

                utileria.EnviarCorreo(correo, imagenesCorreo, remitente);

                if (logoTempPath != null && File.Exists(logoTempPath))
                    File.Delete(logoTempPath);
        }

        /// <summary>
        /// Envía el recordatorio por WhatsApp con la plantilla aprobada en Meta (WhatsApp:PlantillaRecordatorioCita).
        /// Cuerpo: {{1}} paciente, {{2}} profesional, {{3}} fecha, {{4}} hora, {{5}} dirección.
        /// Botones de URL: 0 = Confirmar, 1 = Anular, con sufijo "{id}/{accion}/{firma}".
        /// Si la cita ya está confirmada no se envía (el correo ya avisa "ya confirmaste").
        /// </summary>
        public static async Task<bool> EnviarRecordatorioWhatsAppAsync(Cita cita, ILogger logger, string? telefonoDestino = null)
        {
            if (cita.EstadoConfirmacion == "Confirmada") return false;

            string? telefono = WhatsAppCloud.NormalizarTelefono(telefonoDestino ?? cita.Telefono);
            if (telefono == null)
            {
                if (!string.IsNullOrWhiteSpace(cita.Telefono))
                    logger.LogWarning("Cita {Id}: teléfono '{Telefono}' no válido para WhatsApp.", cita.Id, cita.Telefono);
                return false;
            }

            if (!EnlacesCita.Disponible)
            {
                logger.LogWarning("Cita {Id}: no se envía WhatsApp porque falta ApiSeguridad:ApiKey para firmar los botones.", cita.Id);
                return false;
            }

            var culture = new System.Globalization.CultureInfo("es-ES");
            var profesional = cita.Usuario;
            string nombreProfesional = profesional != null
                ? $"Mat. {profesional.Nombres} {profesional.ApellidoPaterno}".Trim()
                : "tu profesional";

            var (ok, detalle) = await WhatsAppCloud.EnviarPlantillaAsync(
                telefono,
                WhatsAppCloud.PlantillaRecordatorioCita,
                new[]
                {
                    cita.NombrePaciente,
                    nombreProfesional,
                    cita.FechaHoraInicio.ToString("dddd dd 'de' MMMM", culture),
                    cita.FechaHoraInicio.ToString("HH:mm"),
                    profesional?.Institucion?.Dirección ?? "la consulta"
                },
                new[]
                {
                    EnlacesCita.SufijoWhatsApp(cita.Id, EnlacesCita.Confirmar, cita.FechaHoraInicio),
                    EnlacesCita.SufijoWhatsApp(cita.Id, EnlacesCita.Anular, cita.FechaHoraInicio)
                });

            if (ok) logger.LogInformation("Cita {Id}: recordatorio WhatsApp enviado a {Telefono} ({Wamid}).", cita.Id, telefono, detalle);
            else logger.LogError("Cita {Id}: error al enviar WhatsApp a {Telefono}: {Error}", cita.Id, telefono, detalle);
            return ok;
        }

        /// <summary>
        /// Botones Confirmar / Anular con links firmados. Si la cita ya fue confirmada se muestra solo el aviso,
        /// y si no hay llave para firmar (ApiSeguridad:ApiKey) o URL_SITIO no se muestran botones.
        /// </summary>
        private static string BloqueAcciones(Cita cita, ILogger logger)
        {
            if (cita.EstadoConfirmacion == "Confirmada")
                return "<p style=\"margin:10px 0 0 0; font-size:14px; color:#1e8a4c;\"><strong>&#10003; Ya confirmaste tu asistencia.</strong> ¡Te esperamos!</p>";

            string? urlSitio = _config["URL_SITIO"];
            if (!EnlacesCita.Disponible || string.IsNullOrWhiteSpace(urlSitio))
            {
                logger.LogWarning("Recordatorio de cita sin botones: falta ApiSeguridad:ApiKey o URL_SITIO.");
                return string.Empty;
            }

            string urlConfirmar = EnlacesCita.Url(urlSitio, cita.Id, EnlacesCita.Confirmar, cita.FechaHoraInicio);
            string urlAnular = EnlacesCita.Url(urlSitio, cita.Id, EnlacesCita.Anular, cita.FechaHoraInicio);

            return $@"<p style=""margin:10px 0 16px 0; font-size:14px; line-height:1.55;"">
                                Por favor confirma tu asistencia o anula la cita si no puedes asistir.
                            </p>
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""margin:8px 0 4px 0;"">
                                <tr class=""btn-wrap"">
                                    <td align=""left"" style=""padding:4px 0;"">
                                        <a class=""btn"" href=""{urlConfirmar}"" target=""_blank""
                                           style=""background:#0d86b3; color:#ffffff; text-decoration:none; padding:12px 20px; border-radius:8px; font-weight:700; display:inline-block;"">Confirmar cita</a>
                                    </td>
                                    <td align=""right"" style=""padding:4px 0;"">
                                        <a class=""btn"" href=""{urlAnular}"" target=""_blank""
                                           style=""background:#f78a8a; color:#ffffff; text-decoration:none; padding:12px 20px; border-radius:8px; font-weight:700; display:inline-block;"">Anular cita</a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""margin:14px 0 0 0; font-size:12px; color:#7a8794;"">
                                Si los botones no funcionan, usa estos enlaces:<br>
                                Confirmar: <a href=""{urlConfirmar}"" style=""color:#0d86b3; word-break:break-all;"">{urlConfirmar}</a><br>
                                Anular: <a href=""{urlAnular}"" style=""color:#0d86b3; word-break:break-all;"">{urlAnular}</a>
                            </p>";
        }
    }
}
