namespace Fimel.Api.Services.Video
{
    /// <summary>
    /// Sección "Videoconsulta" de la configuración. La ApiKey de Daily va en appsettings.Secrets.json (no versionado).
    /// </summary>
    public class VideoconsultaOptions
    {
        public const string Seccion = "Videoconsulta";

        /// <summary>Apagado por defecto: sin esto las citas marcadas como videoconsulta se guardan sin sala.</summary>
        public bool Habilitada { get; set; }

        /// <summary>"Daily" por ahora.</summary>
        public string Proveedor { get; set; } = "Daily";

        public DailyOptions Daily { get; set; } = new();

        public class DailyOptions
        {
            public string BaseUrl { get; set; } = "https://api.daily.co/v1";
            /// <summary>dashboard.daily.co → Developers → API key.</summary>
            public string? ApiKey { get; set; }
            /// <summary>Subdominio de la cuenta: "fimel" para https://fimel.daily.co (también se acepta "fimel.daily.co").</summary>
            public string? Dominio { get; set; }
        }
    }
}
