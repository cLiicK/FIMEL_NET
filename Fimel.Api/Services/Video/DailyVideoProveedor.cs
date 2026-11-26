using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Fimel.Models;
using Fimel.Utils;
using Microsoft.Extensions.Options;

namespace Fimel.Api.Services.Video
{
    /// <summary>
    /// Salas de Daily (daily.co) vía REST API: https://docs.daily.co/reference/rest-api
    /// Cada cita tiene una sala privada (solo se entra con token) que abre 15 min antes y se cierra 30 min después,
    /// expulsando a quien siga dentro. El paciente entra con un token de invitado y el profesional con uno de dueño.
    /// </summary>
    public class DailyVideoProveedor : IVideoProveedor
    {
        private readonly HttpClient http;
        private readonly VideoconsultaOptions.DailyOptions opciones;
        private readonly bool habilitada;

        public DailyVideoProveedor(HttpClient http, IOptions<VideoconsultaOptions> opciones)
        {
            this.http = http;
            this.opciones = opciones.Value.Daily;
            habilitada = opciones.Value.Habilitada;
            this.http.Timeout = TimeSpan.FromSeconds(15);
        }

        public bool Disponible => habilitada && !string.IsNullOrWhiteSpace(opciones.ApiKey) && !string.IsNullOrWhiteSpace(Subdominio);

        /// <summary>Acepta "fimel", "fimel.daily.co" o "https://fimel.daily.co/".</summary>
        private string Subdominio =>
            (opciones.Dominio ?? string.Empty).Trim()
                .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                .TrimEnd('/')
                .Replace(".daily.co", "", StringComparison.OrdinalIgnoreCase);

        public async Task<SalaVideo> CrearSalaAsync(Cita cita)
        {
            if (!Disponible)
                throw new InvalidOperationException("Daily no está configurado (Videoconsulta:Habilitada, Daily:ApiKey y Daily:Dominio).");

            long nbf = VideoconsultaHorario.AUnix(VideoconsultaHorario.Apertura(cita.FechaHoraInicio));
            long exp = VideoconsultaHorario.AUnix(VideoconsultaHorario.Cierre(cita.FechaHoraFinal));

            // Nombre no adivinable (la sala es privada igual, pero no expone el Id de la cita)
            string nombre = $"fimel-{cita.Id}-{Guid.NewGuid().ToString("N")[..12]}";

            using JsonDocument sala = await EnviarAsync(HttpMethod.Post, "rooms", new
            {
                name = nombre,
                privacy = "private",
                properties = new
                {
                    nbf,
                    exp,
                    eject_at_room_exp = true,
                    enable_prejoin_ui = true,
                    lang = "es"
                    // enable_recording no se define: sin grabación
                }
            });

            string nombreSala = sala.RootElement.GetProperty("name").GetString() ?? nombre;

            try
            {
                string paciente = string.Join(" ", new[] { cita.NombrePaciente, cita.ApellidoPaciente }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                string profesional = cita.Usuario == null
                    ? "Profesional"
                    : $"{cita.Usuario.Nombres} {cita.Usuario.ApellidoPaterno}".Trim();

                string tokenPaciente = await CrearTokenAsync(nombreSala, string.IsNullOrWhiteSpace(paciente) ? "Paciente" : paciente, false, exp);
                string tokenProfesional = await CrearTokenAsync(nombreSala, string.IsNullOrWhiteSpace(profesional) ? "Profesional" : profesional, true, exp);

                string urlBase = $"https://{Subdominio}.daily.co/{nombreSala}";
                return new SalaVideo(nombreSala, $"{urlBase}?t={tokenPaciente}", $"{urlBase}?t={tokenProfesional}");
            }
            catch
            {
                // No dejar salas huérfanas si fallan los tokens
                try { await EliminarSalaAsync(nombreSala); } catch { }
                throw;
            }
        }

        public async Task EliminarSalaAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(opciones.ApiKey)) return;

            using var req = Solicitud(HttpMethod.Delete, $"rooms/{Uri.EscapeDataString(nombre)}", null);
            using HttpResponseMessage resp = await http.SendAsync(req);

            // 404: ya no existe (expiró y Daily la borró, o se eliminó antes)
            if (resp.StatusCode == HttpStatusCode.NotFound) return;
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Daily DELETE rooms/{nombre}: {(int)resp.StatusCode} {await resp.Content.ReadAsStringAsync()}");
        }

        private async Task<string> CrearTokenAsync(string sala, string nombreUsuario, bool esDueno, long exp)
        {
            using JsonDocument token = await EnviarAsync(HttpMethod.Post, "meeting-tokens", new
            {
                properties = new
                {
                    room_name = sala,
                    user_name = nombreUsuario,
                    is_owner = esDueno,
                    exp
                }
            });
            return token.RootElement.GetProperty("token").GetString()
                   ?? throw new HttpRequestException("Daily no devolvió el token de la sala.");
        }

        private async Task<JsonDocument> EnviarAsync(HttpMethod metodo, string ruta, object cuerpo)
        {
            using var req = Solicitud(metodo, ruta, cuerpo);
            using HttpResponseMessage resp = await http.SendAsync(req);
            string json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Daily {metodo} {ruta}: {(int)resp.StatusCode} {json}");

            return JsonDocument.Parse(json);
        }

        private HttpRequestMessage Solicitud(HttpMethod metodo, string ruta, object? cuerpo)
        {
            var req = new HttpRequestMessage(metodo, $"{opciones.BaseUrl.TrimEnd('/')}/{ruta}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opciones.ApiKey);
            if (cuerpo != null)
                req.Content = new StringContent(JsonSerializer.Serialize(cuerpo), Encoding.UTF8, "application/json");
            return req;
        }
    }
}
