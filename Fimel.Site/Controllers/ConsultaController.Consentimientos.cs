using Fimel.Models;
using Fimel.Site.Services;
using Fimel.Utils;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace Fimel.Site.Controllers
{
    /// <summary>Consentimientos informados desde Nueva Consulta: imprimir o enviar a firma digital con QR.</summary>
    public partial class ConsultaController
    {
        private Usuarios? UsuarioSesion() =>
            new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));

        private string UrlBaseRequest() => $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";

        private static bool PuedeVerConsentimiento(Usuarios usuario, ConsentimientoFirma c) =>
            c.IdUsuario == usuario.Id || (usuario.IdInstitucion.HasValue && usuario.IdInstitucion == c.IdInstitucion);

        /// <summary>Arma el consentimiento con los datos de la paciente (desde la API) y del profesional en sesión.</summary>
        private ConsentimientoFirma? ArmarConsentimiento(Usuarios usuario, ConsentimientosInformados.Tipo tipo, int idPaciente,
            string? fechaConsulta, string? dispositivo, string? observaciones, out string? logoInstitucion)
        {
            logoInstitucion = null;
            Pacientes? paciente = APIBase.Get<Pacientes>($"Pacientes/{idPaciente}");
            if (paciente == null) return null;

            var utileria = new Utileria();
            string nombrePaciente = string.Join(" ", new[] { paciente.Nombres, paciente.PrimerApellido, paciente.SegundoApellido }
                .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
            string rutPaciente = paciente.Rut.HasValue && paciente.Rut > 0
                ? utileria.FormatearRut(paciente.Rut.Value, paciente.Dv ?? "")
                : paciente.NumeroDocumento ?? "";

            ConfiguracionUsuario? config = APIBase.Get<ConfiguracionUsuario>($"ConfiguracionesUsuario/GetByUser/{usuario.Id}");
            string nombreProfesional = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();

            string nombreInstitucion = "FIMEL";
            if (usuario.IdInstitucion.HasValue)
            {
                Instituciones? inst = APIBase.Get<Instituciones>($"Instituciones/{usuario.IdInstitucion}");
                nombreInstitucion = inst?.RazonSocial ?? "FIMEL";
                logoInstitucion = inst?.Logo;
            }

            string fecha = DateTime.TryParseExact(fechaConsulta, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime f) ? f.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            var datos = new ConsentimientosInformados.Datos
            {
                Paciente = nombrePaciente,
                Rut = rutPaciente,
                Domicilio = paciente.Direccion?.Trim(),
                Fecha = fecha,
                Profesional = nombreProfesional,
                Dispositivo = dispositivo?.Trim(),
                Observaciones = observaciones?.Trim()
            };

            return new ConsentimientoFirma
            {
                TipoConsentimiento = tipo.Codigo,
                TituloDocumento = tipo.Titulo,
                IdPaciente = paciente.Id,
                IdUsuario = usuario.Id,
                IdInstitucion = usuario.IdInstitucion,
                NombrePaciente = nombrePaciente,
                RutPaciente = rutPaciente,
                NombreProfesional = nombreProfesional,
                RutProfesional = usuario.Rut.HasValue ? utileria.FormatearRut(usuario.Rut.Value, usuario.Dv ?? "") : "",
                TituloProfesional = config?.TituloProfesional,
                FirmaProfesional = config?.Firma,
                NombreInstitucion = nombreInstitucion,
                HtmlCuerpo = ConsentimientosInformados.RenderCuerpo(tipo, datos),
                Estado = "Pendiente"
            };
        }

        /// <summary>Devuelve el HTML listo para imprimir (se abre en una ventana nueva desde JS).</summary>
        [HttpPost]
        public IActionResult ConsentimientoImprimible(string tipo, int idPaciente, string? fechaConsulta, string? dispositivo, string? observaciones)
        {
            try
            {
                Usuarios? usuario = UsuarioSesion();
                if (usuario == null) return Unauthorized();

                var def = ConsentimientosInformados.ObtenerTipo(tipo);
                if (def == null) return BadRequest("Tipo de consentimiento no válido.");

                ConsentimientoFirma? c = ArmarConsentimiento(usuario, def, idPaciente, fechaConsulta, dispositivo, observaciones, out string? logo);
                if (c == null) return BadRequest("No se encontró a la paciente.");

                string html = ConsentimientosInformados.RenderPagina(def, c, logo, UrlBaseRequest(), imprimirAlCargar: true);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error ConsentimientoImprimible: {ex}");
                return StatusCode(500, "Error al generar el consentimiento.");
            }
        }

        /// <summary>Crea un consentimiento pendiente de firma y devuelve el link y el QR para que la paciente firme desde su celular.</summary>
        [HttpPost]
        public IActionResult CrearConsentimientoQR(string tipo, int idPaciente, string? fechaConsulta, string? dispositivo, string? observaciones)
        {
            try
            {
                Usuarios? usuario = UsuarioSesion();
                if (usuario == null) return Json(new { success = false, message = "Tu sesión expiró. Vuelve a ingresar." });

                var def = ConsentimientosInformados.ObtenerTipo(tipo);
                if (def == null) return Json(new { success = false, message = "Tipo de consentimiento no válido." });

                ConsentimientoFirma? c = ArmarConsentimiento(usuario, def, idPaciente, fechaConsulta, dispositivo, observaciones, out _);
                if (c == null) return Json(new { success = false, message = "No se encontró a la paciente." });

                ConsentimientoFirma? creado = APIBase.Post<ConsentimientoFirma>("ConsentimientosFirma", c);
                if (creado == null || string.IsNullOrEmpty(creado.Token))
                    return Json(new { success = false, message = "No se pudo crear el consentimiento." });

                string urlSitio = config["URL_SITIO"] ?? UrlBaseRequest();
                if (!urlSitio.EndsWith('/')) urlSitio += "/";
                string url = $"{urlSitio}Consentimiento/Firmar/{creado.Token}";

                using var generador = new QRCodeGenerator();
                using QRCodeData datosQr = generador.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
                byte[] png = new PngByteQRCode(datosQr).GetGraphic(8);

                return Json(new
                {
                    success = true,
                    token = creado.Token,
                    url,
                    qr = "data:image/png;base64," + Convert.ToBase64String(png),
                    expira = creado.FechaExpiracion.ToString("HH:mm", CultureInfo.InvariantCulture)
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error CrearConsentimientoQR: {ex}");
                return Json(new { success = false, message = "Error al crear el consentimiento para firma." });
            }
        }

        [HttpGet]
        public IActionResult EstadoConsentimiento(string token)
        {
            try
            {
                Usuarios? usuario = UsuarioSesion();
                if (usuario == null) return Json(new { success = false });

                ConsentimientoFirma? c = APIBase.Get<ConsentimientoFirma>($"ConsentimientosFirma/GetByToken/{Uri.EscapeDataString(token ?? "")}");
                if (c == null || !PuedeVerConsentimiento(usuario, c)) return Json(new { success = false });

                return Json(new
                {
                    success = true,
                    estado = c.Estado,
                    expirado = c.Estado == "Pendiente" && c.FechaExpiracion < DateTime.Now,
                    nombreFirmante = c.NombreFirmante,
                    fechaFirma = c.FechaFirma?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error EstadoConsentimiento: {ex}");
                return Json(new { success = false });
            }
        }

        [HttpGet]
        public IActionResult ObtenerConsentimientosFirmados(int idPaciente)
        {
            try
            {
                Usuarios? usuario = UsuarioSesion();
                if (usuario == null) return Json(new { success = false, data = Array.Empty<object>() });

                var lista = (APIBase.Get<List<ConsentimientoFirma>>($"ConsentimientosFirma/GetByPaciente/{idPaciente}") ?? new())
                    .Where(c => PuedeVerConsentimiento(usuario, c))
                    .Select(c => new
                    {
                        token = c.Token,
                        titulo = ConsentimientosInformados.ObtenerTipo(c.TipoConsentimiento)?.Nombre ?? c.TituloDocumento,
                        fechaFirma = c.FechaFirma?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                        nombreFirmante = c.NombreFirmante
                    });

                return Json(new { success = true, data = lista });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error ObtenerConsentimientosFirmados: {ex}");
                return Json(new { success = false, data = Array.Empty<object>() });
            }
        }

        /// <summary>Muestra un consentimiento firmado con la cajita de evidencia, listo para imprimir.</summary>
        [HttpGet]
        public IActionResult VerConsentimiento(string token)
        {
            try
            {
                Usuarios? usuario = UsuarioSesion();
                if (usuario == null) return RedirectToAction("Index", "Login");

                ConsentimientoFirma? c = APIBase.Get<ConsentimientoFirma>($"ConsentimientosFirma/GetByToken/{Uri.EscapeDataString(token ?? "")}");
                if (c == null || !PuedeVerConsentimiento(usuario, c)) return NotFound();

                var def = ConsentimientosInformados.ObtenerTipo(c.TipoConsentimiento);
                if (def == null) return NotFound();

                string? logo = c.IdInstitucion.HasValue ? APIBase.Get<Instituciones>($"Instituciones/{c.IdInstitucion}")?.Logo : null;
                string html = ConsentimientosInformados.RenderPagina(def, c, logo, UrlBaseRequest(), imprimirAlCargar: false);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error VerConsentimiento: {ex}");
                return StatusCode(500);
            }
        }
    }
}
