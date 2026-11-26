using System.Security.Cryptography;
using System.Text;

namespace Fimel.Utils
{
    /// <summary>
    /// Links firmados para que el paciente confirme o anule su cita desde el correo, sin iniciar sesión.
    /// La firma es un HMAC-SHA256 de (id, acción, fecha de la cita) con la llave compartida ApiSeguridad:ApiKey,
    /// así nadie puede responder por otra cita cambiando el Id, y si la cita se reagenda el link anterior deja de servir.
    /// </summary>
    public static class EnlacesCita
    {
        public const string Confirmar = "confirmar";
        public const string Anular = "anular";

        private static string? Llave =>
            string.IsNullOrWhiteSpace(APIClient.ApiKeyCompartida)
                ? Environment.GetEnvironmentVariable("FIMEL_API_KEY")
                : APIClient.ApiKeyCompartida;

        /// <summary>True si hay llave configurada para firmar links.</summary>
        public static bool Disponible => !string.IsNullOrWhiteSpace(Llave);

        public static string Firmar(int idCita, string accion, DateTime fechaHoraInicio)
        {
            string llave = Llave ?? throw new InvalidOperationException("Falta ApiSeguridad:ApiKey para firmar links de citas.");
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(llave));
            byte[] firma = hmac.ComputeHash(Encoding.UTF8.GetBytes($"cita|{idCita}|{accion}|{fechaHoraInicio:yyyyMMddHHmm}"));
            // Base64 URL-safe, recortado a 22 caracteres (128 bits): suficiente y deja el link corto
            return Convert.ToBase64String(firma).TrimEnd('=').Replace('+', '-').Replace('/', '_').Substring(0, 22);
        }

        public static bool Validar(int idCita, string accion, DateTime fechaHoraInicio, string? firma)
        {
            if (!Disponible || string.IsNullOrEmpty(firma)) return false;
            byte[] esperado = Encoding.ASCII.GetBytes(Firmar(idCita, accion, fechaHoraInicio));
            byte[] recibido = Encoding.ASCII.GetBytes(firma);
            return CryptographicOperations.FixedTimeEquals(esperado, recibido);
        }

        /// <summary>URL pública (en Fimel.Site) para responder a la cita.</summary>
        public static string Url(string urlSitio, int idCita, string accion, DateTime fechaHoraInicio) =>
            $"{urlSitio.TrimEnd('/')}/Cita/{idCita}/{accion}?t={Firmar(idCita, accion, fechaHoraInicio)}";

        /// <summary>Parte variable del botón de URL de la plantilla de WhatsApp: "{id}/{accion}/{firma}".</summary>
        public static string SufijoWhatsApp(int idCita, string accion, DateTime fechaHoraInicio) =>
            $"{idCita}/{accion}/{Firmar(idCita, accion, fechaHoraInicio)}";
    }
}
