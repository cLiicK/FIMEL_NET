using Fimel.Models;
using Fimel.Utils;

namespace Fimel.Site.Services
{
    /// <summary>
    /// Piezas de las videoconsultas que comparten los controllers del Site y los servicios de recordatorio:
    /// link público del paciente, bloque del correo y si la API tiene las videoconsultas habilitadas.
    /// </summary>
    public static class VideoconsultaSite
    {
        private static bool? habilitadaCache;
        private static DateTime habilitadaVence = DateTime.MinValue;

        /// <summary>
        /// Pregunta a la API si se pueden crear salas (Videoconsulta:Habilitada + proveedor configurado). Se guarda 5 minutos
        /// para no consultar en cada carga de la agenda.
        /// </summary>
        public static bool Habilitada(APIClient api)
        {
            if (habilitadaCache.HasValue && DateTime.UtcNow < habilitadaVence) return habilitadaCache.Value;
            try
            {
                habilitadaCache = api.Get<bool>("Citas/VideoconsultaHabilitada");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al consultar si la videoconsulta está habilitada: {ex.Message}");
                habilitadaCache = false;
            }
            habilitadaVence = DateTime.UtcNow.AddMinutes(5);
            return habilitadaCache.Value;
        }

        /// <summary>
        /// Arma el modelo de la página de la sala según la hora (Chile): antes de la apertura "pendiente", dentro de la
        /// ventana "abierta" (con la URL del participante) y después del cierre "finalizada".
        /// </summary>
        public static Fimel.Site.ViewModels.VideoconsultaVM ArmarVM(Cita cita, string? urlParticipante)
        {
            var cultura = new System.Globalization.CultureInfo("es-ES");
            DateTime ahora = VideoconsultaHorario.AhoraChile();
            DateTime apertura = VideoconsultaHorario.Apertura(cita.FechaHoraInicio);
            DateTime cierre = VideoconsultaHorario.Cierre(cita.FechaHoraFinal);

            var vm = new Fimel.Site.ViewModels.VideoconsultaVM
            {
                Cita = cita,
                NombreProfesional = cita.Usuario == null ? string.Empty : $"{cita.Usuario.Nombres} {cita.Usuario.ApellidoPaterno}".Trim(),
                NombrePaciente = string.Join(" ", new[] { cita.NombrePaciente, cita.ApellidoPaciente, cita.SegundoApellidoPaciente }
                    .Where(s => !string.IsNullOrWhiteSpace(s))),
                Fecha = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", cultura),
                Hora = cita.FechaHoraInicio.ToString("HH:mm"),
                HoraApertura = apertura.ToString("HH:mm"),
                SegundosParaAbrir = (int)Math.Max(0, Math.Min(int.MaxValue, (apertura - ahora).TotalSeconds)),
                SegundosParaCerrar = (int)Math.Max(0, Math.Min(int.MaxValue, (cierre - ahora).TotalSeconds)),
                LogoBase64 = string.IsNullOrWhiteSpace(cita.Usuario?.Institucion?.Logo) ? null : cita.Usuario!.Institucion!.Logo,
                NombreInstitucion = cita.Usuario?.Institucion?.RazonSocial
            };

            if (ahora >= cierre) vm.Estado = Fimel.Site.ViewModels.VideoconsultaVM.EstadoSala.Finalizada;
            else if (ahora < apertura) vm.Estado = Fimel.Site.ViewModels.VideoconsultaVM.EstadoSala.Pendiente;
            else if (string.IsNullOrWhiteSpace(urlParticipante)) vm.Estado = Fimel.Site.ViewModels.VideoconsultaVM.EstadoSala.SinSala;
            else
            {
                vm.Estado = Fimel.Site.ViewModels.VideoconsultaVM.EstadoSala.Abierta;
                vm.UrlSala = urlParticipante;
            }

            return vm;
        }

        /// <summary>Página pública de Fimel donde el paciente entra a la llamada (nunca el link directo del proveedor).</summary>
        public static string? UrlSala(string? urlSitio, Cita cita) =>
            !cita.EsVideoconsulta || string.IsNullOrWhiteSpace(cita.VideoToken) || string.IsNullOrWhiteSpace(urlSitio)
                ? null
                : $"{urlSitio.TrimEnd('/')}/Videoconsulta/Sala/{cita.VideoToken}";

        /// <summary>Texto de "Ubicación" en los correos: la dirección de la consulta o "Videoconsulta (en línea)".</summary>
        public static string Ubicacion(Cita cita, string direccion) =>
            cita.EsVideoconsulta ? "Videoconsulta (en línea)" : direccion;

        /// <summary>Botón "Unirse a la videoconsulta" para los correos de confirmación y recordatorio. Vacío si no es videoconsulta.</summary>
        public static string BloqueCorreo(Cita cita, string? urlSitio)
        {
            string? url = UrlSala(urlSitio, cita);
            if (url == null) return string.Empty;

            string apertura = VideoconsultaHorario.Apertura(cita.FechaHoraInicio).ToString("HH:mm");

            return $@"<table role=""presentation"" cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""margin:6px 0 14px 0; background:#eef7fb; border-radius:10px;"">
                                <tr><td style=""padding:16px 18px; font-family:Arial,Helvetica,sans-serif; font-size:14px; color:#333; line-height:1.5;"">
                                    <strong style=""color:#0d86b3;"">Esta cita es una videoconsulta.</strong><br>
                                    Entra desde tu celular o computador con cámara y micrófono. La sala se abre a las {apertura} (15 minutos antes).
                                    <div style=""margin-top:12px;"">
                                        <a class=""btn"" href=""{url}"" target=""_blank""
                                           style=""background:#0d86b3; color:#ffffff; text-decoration:none; padding:12px 20px; border-radius:8px; font-weight:700; display:inline-block;"">Unirse a la videoconsulta</a>
                                    </div>
                                    <div style=""margin-top:10px; font-size:12px; color:#7a8794;"">
                                        Si el botón no funciona, copia este enlace: <a href=""{url}"" style=""color:#0d86b3; word-break:break-all;"">{url}</a>
                                    </div>
                                </td></tr>
                            </table>";
        }
    }
}
