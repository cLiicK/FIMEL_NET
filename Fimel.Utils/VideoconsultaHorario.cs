namespace Fimel.Utils
{
    /// <summary>
    /// Ventana horaria de las videoconsultas, compartida por Fimel.Api (al crear la sala) y Fimel.Site (página del paciente).
    /// Las horas de las citas se guardan como hora local de Chile (sin zona), así que para Daily (que usa Unix UTC)
    /// y para saber si la sala ya abrió se convierten con la zona America/Santiago, sin depender de la zona del servidor.
    /// </summary>
    public static class VideoconsultaHorario
    {
        /// <summary>La sala se abre 15 minutos antes del inicio de la cita.</summary>
        public static readonly TimeSpan AperturaAntes = TimeSpan.FromMinutes(15);
        /// <summary>La sala se cierra (y expulsa a los participantes) 30 minutos después del término.</summary>
        public static readonly TimeSpan CierreDespues = TimeSpan.FromMinutes(30);

        private static readonly TimeZoneInfo Zona = ObtenerZona();

        private static TimeZoneInfo ObtenerZona()
        {
            // IANA (Linux / Windows con ICU) y, si no, el id de Windows
            foreach (string id in new[] { "America/Santiago", "Pacific SA Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch { }
            }
            return TimeZoneInfo.Local;
        }

        /// <summary>Hora actual en Chile.</summary>
        public static DateTime AhoraChile() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zona);

        /// <summary>Convierte una hora local de Chile (como se guardan las citas) a Unix UTC (segundos).</summary>
        public static long AUnix(DateTime horaChile)
        {
            DateTime sinZona = DateTime.SpecifyKind(horaChile, DateTimeKind.Unspecified);
            // Hora que no existe por el cambio de horario (se adelanta el reloj): se corre una hora
            if (Zona.IsInvalidTime(sinZona)) sinZona = sinZona.AddHours(1);
            DateTime utc = TimeZoneInfo.ConvertTimeToUtc(sinZona, Zona);
            return new DateTimeOffset(utc, TimeSpan.Zero).ToUnixTimeSeconds();
        }

        public static DateTime Apertura(DateTime inicio) => inicio - AperturaAntes;
        public static DateTime Cierre(DateTime final) => final + CierreDespues;
    }
}
