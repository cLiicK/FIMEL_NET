using Fimel.Models;
using Fimel.Site.Services;
using Fimel.Utils;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    /// <summary>
    /// Página pública (sin sesión) donde la paciente firma un consentimiento desde su celular,
    /// a la que llega escaneando el QR que se muestra en la consulta.
    /// </summary>
    public class ConsentimientoController : Controller
    {
        private readonly APIClient _api;
        public ConsentimientoController(IConfiguration config) => _api = new APIClient(config["API_URL"]);

        private ConsentimientoFirma? Obtener(string token) =>
            string.IsNullOrWhiteSpace(token) || token.Length > 64
                ? null
                : _api.Get<ConsentimientoFirma>($"ConsentimientosFirma/GetByToken/{Uri.EscapeDataString(token)}");

        [HttpGet("Consentimiento/Firmar/{token}")]
        public IActionResult Firmar(string token)
        {
            try
            {
                ConsentimientoFirma? c = Obtener(token);
                var def = c == null ? null : ConsentimientosInformados.ObtenerTipo(c.TipoConsentimiento);
                if (c == null || def == null) return View("FirmarEstado", "El link no es válido.");
                if (c.Estado == "Firmado") return View("FirmarEstado", "Este consentimiento ya fue firmado. ¡Gracias!");
                if (c.Estado != "Pendiente") return View("FirmarEstado", "Este consentimiento fue anulado.");
                if (c.FechaExpiracion < DateTime.Now) return View("FirmarEstado", "El link de firma venció. Pide a tu profesional que genere uno nuevo.");

                string? logo = c.IdInstitucion.HasValue ? _api.Get<Instituciones>($"Instituciones/{c.IdInstitucion}")?.Logo : null;
                string urlBase = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";

                ViewBag.Token = token;
                ViewBag.Encabezado = ConsentimientosInformados.RenderEncabezado(logo, c.NombreInstitucion, urlBase);
                ViewBag.Firmas = ConsentimientosInformados.RenderFirmas(def, c);
                return View(c);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Consentimiento Firmar GET: {ex}");
                return View("FirmarEstado", "No pudimos cargar el consentimiento. Intenta nuevamente en unos minutos.");
            }
        }

        [HttpPost("Consentimiento/Firmar")]
        public IActionResult Firmar(string token, string tipoFirmante, string nombre, string? rut, string firma, bool acepta)
        {
            try
            {
                if (!acepta)
                    return Json(new { success = false, message = "Debes confirmar que leíste y aceptas el consentimiento." });
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { success = false, message = "Ingresa el nombre de quien firma." });

                const string prefijo = "data:image/png;base64,";
                if (string.IsNullOrEmpty(firma) || !firma.StartsWith(prefijo))
                    return Json(new { success = false, message = "Dibuja tu firma en el recuadro." });

                var ip = HttpContext.Connection.RemoteIpAddress;
                if (ip != null && ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();

                ConsentimientoFirma? firmado = _api.Put<ConsentimientoFirma>($"ConsentimientosFirma/Firmar/{Uri.EscapeDataString(token ?? "")}", new ConsentimientoFirma
                {
                    TipoFirmante = tipoFirmante == "Representante" ? "Representante" : "Paciente",
                    NombreFirmante = nombre.Trim(),
                    RutFirmante = rut?.Trim(),
                    FirmaPaciente = firma[prefijo.Length..],
                    IpFirma = ip?.ToString(),
                    UserAgentFirma = Request.Headers.UserAgent.ToString()
                });

                if (firmado == null || firmado.Estado != "Firmado")
                    return Json(new { success = false, message = "No se pudo registrar la firma. Puede que el link haya vencido o ya se haya firmado." });

                return Json(new
                {
                    success = true,
                    fechaFirma = firmado.FechaFirma?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    codigo = firmado.HashDocumento?.Length >= 16
                        ? string.Join("-", Enumerable.Range(0, 4).Select(i => firmado.HashDocumento.Substring(i * 4, 4)))
                        : firmado.HashDocumento
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Consentimiento Firmar POST: {ex}");
                return Json(new { success = false, message = "No se pudo registrar la firma. Intenta nuevamente." });
            }
        }
    }
}
