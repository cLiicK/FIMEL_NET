using Fimel.Models;

namespace Fimel.Site.ViewModels
{
    /// <summary>Datos de la sala de videoconsulta para la página del paciente (pública) y la del profesional.</summary>
    public class VideoconsultaVM
    {
        public enum EstadoSala { Pendiente, Abierta, Finalizada, SinSala }

        public Cita Cita { get; set; } = null!;
        public EstadoSala Estado { get; set; }
        /// <summary>URL de la sala con el token del participante. Solo se entrega cuando la sala está abierta.</summary>
        public string? UrlSala { get; set; }
        public string NombreProfesional { get; set; } = string.Empty;
        public string NombrePaciente { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public string HoraApertura { get; set; } = string.Empty;
        /// <summary>Segundos que faltan para abrir la sala (para recargar la página sola).</summary>
        public int SegundosParaAbrir { get; set; }
        /// <summary>Segundos que faltan para que la sala se cierre.</summary>
        public int SegundosParaCerrar { get; set; }
        /// <summary>Logo de la institución en Base64 (PNG) o null para el logo de Fimel.</summary>
        public string? LogoBase64 { get; set; }
        public string? NombreInstitucion { get; set; }

        // Solo vista del profesional
        public string? UrlFichaPaciente { get; set; }
        public string? UrlNuevaConsulta { get; set; }
        public bool PacienteRegistrado { get; set; }
    }
}
