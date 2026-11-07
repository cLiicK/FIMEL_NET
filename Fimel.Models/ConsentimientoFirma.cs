using System;

namespace Fimel.Models
{
    /// <summary>
    /// Consentimiento informado firmado digitalmente por la paciente desde su celular (vía QR o link).
    /// El cuerpo del documento se guarda como HTML al momento de crearlo, para que lo firmado no cambie
    /// aunque después se actualicen los textos de los consentimientos.
    /// </summary>
    public class ConsentimientoFirma : LayerSuperType
    {
        public string Token { get; set; } = "";
        public string TipoConsentimiento { get; set; } = "";
        public string? TituloDocumento { get; set; }
        public int IdPaciente { get; set; }
        public int IdUsuario { get; set; }
        public int? IdInstitucion { get; set; }

        // Datos impresos en el documento al crearlo
        public string? NombrePaciente { get; set; }
        public string? RutPaciente { get; set; }
        public string? NombreProfesional { get; set; }
        public string? RutProfesional { get; set; }
        public string? TituloProfesional { get; set; }
        public string? FirmaProfesional { get; set; }
        public string? NombreInstitucion { get; set; }
        public string? HtmlCuerpo { get; set; }

        /// <summary>Pendiente, Firmado o Anulado.</summary>
        public string Estado { get; set; } = "Pendiente";
        public DateTime FechaExpiracion { get; set; }

        // Evidencia de la firma
        public DateTime? FechaFirma { get; set; }
        /// <summary>Paciente o Representante (representante legal que firma en su nombre).</summary>
        public string? TipoFirmante { get; set; }
        public string? NombreFirmante { get; set; }
        public string? RutFirmante { get; set; }
        public string? FirmaPaciente { get; set; }
        public string? IpFirma { get; set; }
        public string? UserAgentFirma { get; set; }
        public string? HashDocumento { get; set; }
    }
}
