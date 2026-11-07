using System.Security.Cryptography;
using System.Text;

namespace Fimel.Api.Seguridad
{
    /// <summary>
    /// Exige que cada request a /api traiga el header X-Api-Key con la llave compartida entre Fimel.Site y Fimel.Api.
    /// La llave se lee de ApiSeguridad:ApiKey (user-secrets / variable de entorno ApiSeguridad__ApiKey) o de FIMEL_API_KEY.
    /// Si no hay llave configurada la API sigue abierta y se avisa en el log, para poder desplegar sin cortar el Site.
    /// </summary>
    public class ApiKeyMiddleware
    {
        public const string Header = "X-Api-Key";

        private readonly RequestDelegate next;
        private readonly byte[]? llave;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config, ILogger<ApiKeyMiddleware> logger)
        {
            this.next = next;
            string? valor = config["ApiSeguridad:ApiKey"];
            if (string.IsNullOrWhiteSpace(valor)) valor = config["FIMEL_API_KEY"];

            if (string.IsNullOrWhiteSpace(valor))
                logger.LogWarning("ApiSeguridad:ApiKey no está configurada: la API acepta requests sin {Header}.", Header);
            else
                llave = Encoding.UTF8.GetBytes(valor.Trim());
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Sin llave configurada, o fuera de /api (p. ej. Swagger en Development), no se exige nada.
            if (llave == null || !context.Request.Path.StartsWithSegments("/api") || HttpMethods.IsOptions(context.Request.Method))
            {
                await next(context);
                return;
            }

            string? recibida = context.Request.Headers[Header];
            if (string.IsNullOrEmpty(recibida) ||
                !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(recibida), llave))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await next(context);
        }
    }
}
