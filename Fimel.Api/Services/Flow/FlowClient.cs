using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Fimel.Api.Services.Flow
{
    /// <summary>
    /// Cliente de la API REST de Flow (https://developers.flow.cl/en/api).
    /// Cada request lleva apiKey y la firma "s": HMAC-SHA256 (hex) con la secretKey sobre
    /// los parámetros ordenados alfabéticamente y concatenados como nombrevalor.
    /// </summary>
    public class FlowClient
    {
        private readonly HttpClient http;
        private readonly FlowOptions opciones;

        public FlowClient(HttpClient http, IOptions<FlowOptions> opciones)
        {
            this.http = http;
            this.opciones = opciones.Value;
        }

        public bool Configurado => opciones.Configurado;
        public string BaseUrl => opciones.BaseUrl;

        #region Planes

        public Task<JsonElement> ListarPlanes(int inicio = 0, int limite = 10)
        {
            return Get("plans/list", new Dictionary<string, string>
            {
                ["start"] = inicio.ToString(CultureInfo.InvariantCulture),
                ["limit"] = limite.ToString(CultureInfo.InvariantCulture)
            });
        }

        /// <param name="intervalo">1 diario, 2 semanal, 3 mensual, 4 anual</param>
        public Task<JsonElement> CrearPlan(string planId, string nombre, int monto, int intervalo = 3, int diasPrueba = 0, string? urlCallback = null)
        {
            Dictionary<string, string> parametros = new Dictionary<string, string>
            {
                ["planId"] = planId,
                ["name"] = nombre,
                ["currency"] = "CLP",
                ["amount"] = monto.ToString(CultureInfo.InvariantCulture),
                ["interval"] = intervalo.ToString(CultureInfo.InvariantCulture),
                ["trial_period_days"] = diasPrueba.ToString(CultureInfo.InvariantCulture)
            };

            if (!string.IsNullOrWhiteSpace(urlCallback))
                parametros["urlCallback"] = urlCallback;

            return Post("plans/create", parametros);
        }

        public Task<JsonElement> ObtenerPlan(string planId)
        {
            return Get("plans/get", new Dictionary<string, string> { ["planId"] = planId });
        }

        #endregion

        #region Clientes y suscripciones

        public Task<JsonElement> CrearCliente(string nombre, string email, string externalId)
        {
            return Post("customer/create", new Dictionary<string, string>
            {
                ["name"] = nombre,
                ["email"] = email,
                ["externalId"] = externalId
            });
        }

        /// <summary>Devuelve url y token; el usuario se redirige a url?token=token para inscribir su tarjeta.</summary>
        public Task<JsonElement> RegistrarTarjeta(string customerId, string urlRetorno)
        {
            return Post("customer/register", new Dictionary<string, string>
            {
                ["customerId"] = customerId,
                ["url_return"] = urlRetorno
            });
        }

        public Task<JsonElement> EstadoRegistroTarjeta(string token)
        {
            return Get("customer/getRegisterStatus", new Dictionary<string, string> { ["token"] = token });
        }

        /// <param name="diasPrueba">Si se indica, reemplaza los días de prueba del plan (0 = sin prueba).</param>
        public Task<JsonElement> CrearSuscripcion(string planId, string customerId, int? diasPrueba = null)
        {
            Dictionary<string, string> parametros = new Dictionary<string, string>
            {
                ["planId"] = planId,
                ["customerId"] = customerId
            };

            if (diasPrueba.HasValue)
                parametros["trial_period_days"] = diasPrueba.Value.ToString(CultureInfo.InvariantCulture);

            return Post("subscription/create", parametros);
        }

        public Task<JsonElement> EstadoPago(string token)
        {
            return Get("payment/getStatus", new Dictionary<string, string> { ["token"] = token });
        }

        #endregion

        #region Transporte y firma

        public async Task<JsonElement> Get(string servicio, IDictionary<string, string> parametros)
        {
            string query = await new FormUrlEncodedContent(Firmar(parametros)).ReadAsStringAsync();
            using HttpResponseMessage respuesta = await http.GetAsync($"{Url(servicio)}?{query}");
            return await Leer(respuesta);
        }

        public async Task<JsonElement> Post(string servicio, IDictionary<string, string> parametros)
        {
            using FormUrlEncodedContent contenido = new FormUrlEncodedContent(Firmar(parametros));
            using HttpResponseMessage respuesta = await http.PostAsync(Url(servicio), contenido);
            return await Leer(respuesta);
        }

        /// <summary>Lee una propiedad de la respuesta de Flow como texto, venga como string o número.</summary>
        public static string? Texto(JsonElement json, string propiedad)
        {
            if (json.ValueKind != JsonValueKind.Object || !json.TryGetProperty(propiedad, out JsonElement valor))
                return null;

            return valor.ValueKind switch
            {
                JsonValueKind.String => valor.GetString(),
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => valor.ToString()
            };
        }

        private string Url(string servicio) => $"{opciones.BaseUrl.TrimEnd('/')}/{servicio.TrimStart('/')}";

        private Dictionary<string, string> Firmar(IDictionary<string, string> parametros)
        {
            if (!opciones.Configurado)
                throw new InvalidOperationException("Faltan las credenciales de Flow (Flow:ApiKey y Flow:SecretKey).");

            Dictionary<string, string> firmados = new Dictionary<string, string>(parametros) { ["apiKey"] = opciones.ApiKey! };
            firmados["s"] = CalcularFirma(firmados, opciones.SecretKey!);
            return firmados;
        }

        /// <summary>Firma según la documentación de Flow. Excluye "s" y ordena por nombre en orden ordinal (igual que ksort de PHP).</summary>
        public static string CalcularFirma(IDictionary<string, string> parametros, string secretKey)
        {
            StringBuilder cadena = new StringBuilder();
            foreach (KeyValuePair<string, string> par in parametros.Where(p => p.Key != "s").OrderBy(p => p.Key, StringComparer.Ordinal))
                cadena.Append(par.Key).Append(par.Value);

            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(cadena.ToString()));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static async Task<JsonElement> Leer(HttpResponseMessage respuesta)
        {
            string cuerpo = await respuesta.Content.ReadAsStringAsync();
            JsonElement json;

            try
            {
                json = string.IsNullOrWhiteSpace(cuerpo) ? default : JsonDocument.Parse(cuerpo).RootElement.Clone();
            }
            catch (JsonException)
            {
                throw new FlowException((int)respuesta.StatusCode, null, $"Respuesta no JSON de Flow: {cuerpo}");
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                string? codigo = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("code", out JsonElement c) ? c.ToString() : null;
                string mensaje = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("message", out JsonElement m) ? m.ToString() : cuerpo;
                throw new FlowException((int)respuesta.StatusCode, codigo, mensaje);
            }

            return json;
        }

        #endregion
    }
}
