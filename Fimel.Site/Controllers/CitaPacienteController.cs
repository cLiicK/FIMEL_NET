using System.Globalization;
using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    /// <summary>
    /// Página pública (sin sesión) a la que llega el paciente desde los botones "Confirmar cita" / "Anular cita"
    /// del correo de recordatorio. El GET solo muestra la cita y un botón; el cambio se hace en el POST, para que los
    /// antivirus de correo que abren los links por adelantado no confirmen ni anulen citas solos.
    /// </summary>
    public class CitaPacienteController : Controller
    {
        private static readonly CultureInfo Cultura = new("es-ES");
        private readonly APIClient _api;

        public CitaPacienteController(IConfiguration config) => _api = new APIClient(config["API_URL"]);

        public class RespuestaCitaVM
        {
            public Cita Cita { get; set; } = null!;
            public string Accion { get; set; } = string.Empty;
            public string Firma { get; set; } = string.Empty;
            public string NombreProfesional { get; set; } = string.Empty;
            public string Fecha { get; set; } = string.Empty;
            public string Hora { get; set; } = string.Empty;
        }

        [HttpGet("Cita/{id:int}/{accion}")]
        public IActionResult Responder(int id, string accion, string? t)
        {
            try
            {
                var (cita, error) = ObtenerCitaValida(id, accion, t);
                if (cita == null) return View("Estado", error);

                if (cita.EstadoConfirmacion == "Confirmada" && accion == EnlacesCita.Confirmar)
                    return View("Estado", "Tu cita ya estaba confirmada. ¡Te esperamos!");

                return View(new RespuestaCitaVM
                {
                    Cita = cita,
                    Accion = accion,
                    Firma = t!,
                    NombreProfesional = NombreProfesional(cita.Usuario),
                    Fecha = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", Cultura),
                    Hora = cita.FechaHoraInicio.ToString("HH:mm")
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error CitaPaciente Responder GET: {ex}");
                return View("Estado", "No pudimos cargar tu cita. Intenta nuevamente en unos minutos.");
            }
        }

        [HttpPost("Cita/{id:int}/{accion}")]
        [ValidateAntiForgeryToken]
        public IActionResult ResponderPost(int id, string accion, string? t)
        {
            try
            {
                var (cita, error) = ObtenerCitaValida(id, accion, t);
                if (cita == null) return View("Estado", error);

                Cita? actualizada = _api.Put<Cita>($"Citas/{id}/RespuestaPaciente?accion={accion}", new Cita());
                if (actualizada == null)
                    return View("Estado", "No pudimos registrar tu respuesta. Intenta nuevamente en unos minutos.");

                if (accion == EnlacesCita.Anular)
                {
                    try { NotificarAnulacionAlProfesional(cita); }
                    catch (Exception ex) { Logger.Log($"Error al notificar anulación de cita {id}: {ex}"); }

                    return View("Estado", "Tu cita fue anulada. Si quieres agendar otra hora, comunícate con tu profesional.");
                }

                return View("Estado", $"¡Listo! Tu cita del {cita.FechaHoraInicio.ToString("dd/MM", Cultura)} a las {cita.FechaHoraInicio:HH:mm} quedó confirmada.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error CitaPaciente Responder POST: {ex}");
                return View("Estado", "No pudimos registrar tu respuesta. Intenta nuevamente en unos minutos.");
            }
        }

        private (Cita? cita, string error) ObtenerCitaValida(int id, string accion, string? firma)
        {
            if (accion != EnlacesCita.Confirmar && accion != EnlacesCita.Anular)
                return (null, "El link no es válido.");

            Cita? cita = _api.Get<Cita>($"Citas/{id}");
            if (cita == null)
                return (null, "Esta cita ya fue anulada o no existe.");

            if (!EnlacesCita.Validar(id, accion, cita.FechaHoraInicio, firma))
                return (null, "El link no es válido o la cita fue reagendada. Revisa el correo más reciente.");

            if (cita.FechaHoraInicio < DateTime.Now)
                return (null, "Esta cita ya pasó.");

            return (cita, string.Empty);
        }

        private static string NombreProfesional(Usuarios? u) =>
            u == null ? string.Empty : $"{u.Nombres} {u.ApellidoPaterno}".Trim();

        private void NotificarAnulacionAlProfesional(Cita cita)
        {
            Usuarios? profesional = cita.Usuario;
            if (string.IsNullOrWhiteSpace(profesional?.Email)) return;

            string paciente = string.Join(" ", new[] { cita.NombrePaciente, cita.ApellidoPaciente, cita.SegundoApellidoPaciente }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
            string fecha = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", Cultura);

            string html = $@"<div style=""font-family:Arial,Helvetica,sans-serif; color:#333; font-size:14px;"">
<p>Hola {System.Net.WebUtility.HtmlEncode(NombreProfesional(profesional))},</p>
<p><strong>{System.Net.WebUtility.HtmlEncode(paciente)}</strong> anuló su cita desde el correo de recordatorio:</p>
<p>{fecha} a las {cita.FechaHoraInicio:HH:mm}</p>
<p>El horario quedó libre en tu agenda.</p>
<p style=""color:#7a8794; font-size:12px;"">Correo automático de FIMEL.</p></div>";

            new Utileria().EnviarCorreo(new EnvioCorreo
            {
                Destinatarios = new List<string> { profesional!.Email! },
                Asunto = $"Cita anulada por el paciente — {cita.FechaHoraInicio:dd/MM/yyyy HH:mm}",
                CuerpoCorreo = html,
                ResponderA = cita.CorreoPaciente
            }, new List<(string, string, string)>(), "Fimel");
        }
    }
}
