using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Fimel.Utils
{
    /// <summary>
    /// Envío de mensajes de plantilla por WhatsApp Cloud API de Meta (graph.facebook.com), sin intermediarios.
    /// Configuración en la sección "WhatsApp" (el AccessToken va en appsettings.Secrets.json, nunca en git):
    ///   Habilitado, PhoneNumberId, AccessToken, ApiVersion, PlantillaRecordatorioCita, Idioma.
    /// Fuera de una conversación abierta por el paciente, Meta solo permite enviar plantillas aprobadas.
    /// </summary>
    public static class WhatsAppCloud
    {
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(20) };

        public static bool Habilitado { get; private set; }
        public static string? PhoneNumberId { get; private set; }
        private static string? AccessToken { get; set; }
        public static string ApiVersion { get; private set; } = "v23.0";
        public static string PlantillaRecordatorioCita { get; private set; } = "recordatorio_cita";
        public static string Idioma { get; private set; } = "es";

        /// <summary>Se llama una vez al arrancar (Program.cs), con la configuración que ya incluye appsettings.Secrets.json.</summary>
        public static void Configurar(IConfiguration config)
        {
            var s = config.GetSection("WhatsApp");
            PhoneNumberId = s["PhoneNumberId"];
            AccessToken = s["AccessToken"];
            if (!string.IsNullOrWhiteSpace(s["ApiVersion"])) ApiVersion = s["ApiVersion"]!;
            if (!string.IsNullOrWhiteSpace(s["PlantillaRecordatorioCita"])) PlantillaRecordatorioCita = s["PlantillaRecordatorioCita"]!;
            if (!string.IsNullOrWhiteSpace(s["Idioma"])) Idioma = s["Idioma"]!;
            Habilitado = bool.TryParse(s["Habilitado"], out bool h) && h
                         && !string.IsNullOrWhiteSpace(PhoneNumberId) && !string.IsNullOrWhiteSpace(AccessToken);
        }

        /// <summary>
        /// Lleva un teléfono escrito a mano al formato que pide Meta (solo dígitos con código de país, p.ej. 56912345678).
        /// Celulares chilenos: "9 1234 5678", "+56 9 1234 5678", "12345678" (formato antiguo de 8 dígitos).
        /// Números con "+" de otro país se respetan. Devuelve null si no parece un celular válido.
        /// </summary>
        public static string? NormalizarTelefono(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono)) return null;
            string t = telefono.Trim();
            string d = new string(t.Where(char.IsDigit).ToArray());
            if (d.StartsWith("00")) d = d[2..];

            if (d.Length == 11 && d.StartsWith("569")) return d;
            if (d.Length == 9 && d.StartsWith("9")) return "56" + d;
            if (d.Length == 8 && !t.StartsWith("+")) return "569" + d;
            if (t.StartsWith("+") && !d.StartsWith("56") && d.Length is >= 10 and <= 15) return d;
            return null;
        }

        /// <summary>Meta rechaza parámetros de cuerpo con saltos de línea, tabs o más de 4 espacios seguidos.</summary>
        private static string LimpiarParametro(string? valor)
        {
            string v = Regex.Replace(valor ?? string.Empty, @"[\r\n\t]+", " ");
            v = Regex.Replace(v, @" {4,}", "   ").Trim();
            return string.IsNullOrEmpty(v) ? "-" : v;
        }

        /// <summary>
        /// Envía una plantilla aprobada. parametrosCuerpo reemplazan {{1}}, {{2}}… del cuerpo;
        /// sufijosBotonesUrl son la parte variable de cada botón de URL, en el orden en que están en la plantilla.
        /// Devuelve (true, id del mensaje "wamid…") o (false, error de Meta).
        /// </summary>
        public static async Task<(bool Ok, string Detalle)> EnviarPlantillaAsync(string telefono, string plantilla,
            IEnumerable<string?> parametrosCuerpo, IEnumerable<string>? sufijosBotonesUrl = null)
        {
            if (!Habilitado) return (false, "WhatsApp no está habilitado o faltan PhoneNumberId / AccessToken.");

            var componentes = new List<object>
            {
                new
                {
                    type = "body",
                    parameters = parametrosCuerpo.Select(p => new { type = "text", text = LimpiarParametro(p) }).ToArray()
                }
            };

            int i = 0;
            foreach (string sufijo in sufijosBotonesUrl ?? Enumerable.Empty<string>())
            {
                componentes.Add(new
                {
                    type = "button",
                    sub_type = "url",
                    index = (i++).ToString(),
                    parameters = new[] { new { type = "text", text = sufijo } }
                });
            }

            var cuerpo = new
            {
                messaging_product = "whatsapp",
                to = telefono,
                type = "template",
                template = new { name = plantilla, language = new { code = Idioma }, components = componentes }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, $"https://graph.facebook.com/{ApiVersion}/{PhoneNumberId}/messages")
            {
                Content = new StringContent(JsonSerializer.Serialize(cuerpo), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            try
            {
                using HttpResponseMessage resp = await Http.SendAsync(req);
                string json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    Logger.Log($"WhatsApp: error {(int)resp.StatusCode} al enviar plantilla {plantilla} a {telefono}: {json}");
                    return (false, json);
                }

                using var doc = JsonDocument.Parse(json);
                string wamid = doc.RootElement.GetProperty("messages")[0].GetProperty("id").GetString() ?? string.Empty;
                return (true, wamid);
            }
            catch (Exception ex)
            {
                Logger.Log($"WhatsApp: excepción al enviar plantilla {plantilla} a {telefono}: {ex}");
                return (false, ex.Message);
            }
        }
    }
}
