using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fimel.Models
{
    public class Recordatorio : LayerSuperType
    {
        public int IdPaciente { get; set; }
        public string? Titulo { get; set; }
        public string? Cuerpo { get; set; }
        public string? RepetirCada { get; set; }
        public DateTime FechaProximoEnvio { get; set; }
        public DateTime? UltimaFechaEnvio { get; set; }
        public int? UsuarioCreacion { get; set; }

        [NotMapped]
        public Pacientes? Paciente { get; set; }
    }
}
