namespace Fimel.Api.Seguridad
{
    /// <summary>
    /// Agrega headers de seguridad estándar a todas las respuestas de la API (solo devuelve JSON, nunca HTML).
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate next;

        public SecurityHeadersMiddleware(RequestDelegate next) => this.next = next;

        public Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                IHeaderDictionary h = context.Response.Headers;
                h["X-Content-Type-Options"] = "nosniff";
                h["X-Frame-Options"] = "DENY";
                h["Referrer-Policy"] = "no-referrer";
                h["Cache-Control"] = "no-store";
                h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                // Swagger UI (solo Development) necesita scripts y estilos; el resto de la API no carga nada.
                if (!context.Request.Path.StartsWithSegments("/swagger"))
                    h["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
                h.Remove("Server");
                h.Remove("X-Powered-By");
                return Task.CompletedTask;
            });
            return next(context);
        }
    }
}
