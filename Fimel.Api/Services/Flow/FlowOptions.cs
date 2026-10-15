namespace Fimel.Api.Services.Flow
{
    public class FlowOptions
    {
        public const string Seccion = "Flow";

        // Sandbox por defecto. Producción: https://www.flow.cl/api
        public string BaseUrl { get; set; } = "https://sandbox.flow.cl/api";

        // Nunca en appsettings versionado: usar user-secrets en local y variables de entorno en el servidor
        // (Flow__ApiKey / Flow__SecretKey, o FLOW_API_KEY / FLOW_SECRET_KEY).
        public string? ApiKey { get; set; }
        public string? SecretKey { get; set; }

        public bool Configurado => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(SecretKey);
    }
}
