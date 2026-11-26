using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsentimientosFirmaController : ControllerBase
    {
        private static readonly TimeSpan VigenciaLink = TimeSpan.FromMinutes(60);
        private const int MaxLargoFirma = 400_000; // ~300 KB de PNG en Base64

        private readonly FimelDbContext db;
        public ConsentimientosFirmaController(FimelDbContext context)
        {
            db = context;
        }

        [HttpPost]
        public IActionResult Post(ConsentimientoFirma consentimiento)
        {
            try
            {
                consentimiento.Id = 0;
                consentimiento.Token = GenerarToken();
                consentimiento.Estado = "Pendiente";
                consentimiento.Vigente = "S";
                consentimiento.FechaCreacion = DateTime.Now;
                consentimiento.FechaExpiracion = DateTime.Now.Add(VigenciaLink);
                consentimiento.FechaFirma = null;
                consentimiento.TipoFirmante = null;
                consentimiento.NombreFirmante = null;
                consentimiento.RutFirmante = null;
                consentimiento.FirmaPaciente = null;
                consentimiento.IpFirma = null;
                consentimiento.UserAgentFirma = null;
                consentimiento.HashDocumento = null;

                db.ConsentimientosFirma.Add(consentimiento);
                db.SaveChanges();

                return Ok(consentimiento);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST ConsentimientoFirma: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet("GetByToken/{token}")]
        public IActionResult GetByToken(string token)
        {
            try
            {
                ConsentimientoFirma? consentimiento = db.ConsentimientosFirma
                    .FirstOrDefault(c => c.Token == token && c.Vigente == "S");

                return Ok(consentimiento);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetByToken ConsentimientoFirma: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet("GetByPaciente/{idPaciente}")]
        public IActionResult GetByPaciente(int idPaciente)
        {
            try
            {
                // Solo los firmados y sin las imágenes, para que el listado sea liviano
                var lista = db.ConsentimientosFirma
                    .Where(c => c.IdPaciente == idPaciente && c.Vigente == "S" && c.Estado == "Firmado")
                    .OrderByDescending(c => c.FechaFirma)
                    .Select(c => new ConsentimientoFirma
                    {
                        Id = c.Id,
                        Token = c.Token,
                        TipoConsentimiento = c.TipoConsentimiento,
                        TituloDocumento = c.TituloDocumento,
                        IdPaciente = c.IdPaciente,
                        IdUsuario = c.IdUsuario,
                        IdInstitucion = c.IdInstitucion,
                        NombreProfesional = c.NombreProfesional,
                        Estado = c.Estado,
                        FechaFirma = c.FechaFirma,
                        TipoFirmante = c.TipoFirmante,
                        NombreFirmante = c.NombreFirmante,
                        FechaCreacion = c.FechaCreacion
                    })
                    .ToList();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetByPaciente ConsentimientoFirma: {ex}");
                return StatusCode(500, ex);
            }
        }

        /// <summary>
        /// Registra la firma de la paciente. La fecha y hora se toman del servidor y se calcula un hash
        /// SHA-256 del documento firmado (cuerpo + datos del firmante + firma) como código de verificación.
        /// </summary>
        [HttpPut("Firmar/{token}")]
        public IActionResult Firmar(string token, ConsentimientoFirma datosFirma)
        {
            try
            {
                ConsentimientoFirma? consentimiento = db.ConsentimientosFirma
                    .FirstOrDefault(c => c.Token == token && c.Vigente == "S");

                if (consentimiento == null)
                    return BadRequest("El consentimiento no existe.");
                if (consentimiento.Estado != "Pendiente")
                    return BadRequest("Este consentimiento ya fue firmado o anulado.");
                if (consentimiento.FechaExpiracion < DateTime.Now)
                    return BadRequest("El link de firma venció. Pide a tu profesional que genere uno nuevo.");
                if (string.IsNullOrWhiteSpace(datosFirma.NombreFirmante))
                    return BadRequest("Falta el nombre de quien firma.");
                if (string.IsNullOrWhiteSpace(datosFirma.FirmaPaciente) || datosFirma.FirmaPaciente.Length > MaxLargoFirma)
                    return BadRequest("La firma no es válida.");

                byte[] firmaBytes;
                try { firmaBytes = Convert.FromBase64String(datosFirma.FirmaPaciente); }
                catch { return BadRequest("La firma no es válida."); }
                bool esPng = firmaBytes.Length > 8 && firmaBytes[0] == 0x89 && firmaBytes[1] == 0x50 && firmaBytes[2] == 0x4E && firmaBytes[3] == 0x47;
                if (!esPng)
                    return BadRequest("La firma no es válida.");

                consentimiento.Estado = "Firmado";
                consentimiento.FechaFirma = DateTime.Now;
                consentimiento.TipoFirmante = datosFirma.TipoFirmante == "Representante" ? "Representante" : "Paciente";
                consentimiento.NombreFirmante = Recortar(datosFirma.NombreFirmante.Trim(), 200);
                consentimiento.RutFirmante = Recortar(datosFirma.RutFirmante?.Trim(), 20);
                consentimiento.FirmaPaciente = datosFirma.FirmaPaciente;
                consentimiento.IpFirma = Recortar(datosFirma.IpFirma, 64);
                consentimiento.UserAgentFirma = Recortar(datosFirma.UserAgentFirma, 400);
                consentimiento.HashDocumento = CalcularHash(consentimiento);

                db.SaveChanges();

                return Ok(consentimiento);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Firmar ConsentimientoFirma: {ex}");
                return StatusCode(500, ex);
            }
        }

        private static string GenerarToken()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(24);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        private static string? Recortar(string? texto, int max) =>
            texto == null ? null : (texto.Length > max ? texto[..max] : texto);

        private static string CalcularHash(ConsentimientoFirma c)
        {
            string contenido = string.Join("\n",
                c.Token,
                c.TipoConsentimiento,
                c.HtmlCuerpo ?? "",
                c.NombrePaciente ?? "",
                c.RutPaciente ?? "",
                c.NombreProfesional ?? "",
                c.TipoFirmante ?? "",
                c.NombreFirmante ?? "",
                c.RutFirmante ?? "",
                c.FechaFirma?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                c.IpFirma ?? "",
                c.FirmaPaciente ?? "");

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(contenido));
            return Convert.ToHexString(hash);
        }
    }
}
