using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class Cita : LayerSuperType
    {
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFinal { get; set; }
        public string NombrePaciente { get; set; }
        public string? ApellidoPaciente { get; set; }
        public string? SegundoApellidoPaciente { get; set; }
        public string CorreoPaciente { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Nota { get; set; }

        /// <summary>Respuesta del paciente desde el link del recordatorio: null = sin respuesta, "Confirmada" o "Anulada".</summary>
        public string? EstadoConfirmacion { get; set; }
        public DateTime? FechaRespuestaPaciente { get; set; }

        /// <summary>La cita se atiende por videollamada (sala creada en el proveedor de video, hoy Daily).</summary>
        public bool EsVideoconsulta { get; set; }
        /// <summary>Nombre de la sala en el proveedor de video (para recrearla o eliminarla).</summary>
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? VideoSalaNombre { get; set; }
        /// <summary>URL de la sala con token de invitado. No se manda directo al paciente: se embebe en /Videoconsulta/Sala/{VideoToken}.</summary>
        [System.ComponentModel.DataAnnotations.MaxLength(2000)]
        public string? VideoUrlPaciente { get; set; }
        /// <summary>URL de la sala con token de dueño (profesional).</summary>
        [System.ComponentModel.DataAnnotations.MaxLength(2000)]
        public string? VideoUrlProfesional { get; set; }
        /// <summary>Token aleatorio del link público del paciente (/Videoconsulta/Sala/{VideoToken}).</summary>
        [System.ComponentModel.DataAnnotations.MaxLength(64)]
        public string? VideoToken { get; set; }

        public Usuarios? Usuario { get; set; }

    }
}
