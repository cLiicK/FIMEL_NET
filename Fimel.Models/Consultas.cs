using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class Consultas : LayerSuperType
    {
        public int Id_Paciente { get; set; }
        public string? MotivoConsulta { get; set; }
        public string? Anamnesis { get; set; }
        public string? ExamenFisico { get; set; }
        public string? Diagnostico { get; set; }
        public string? Indicaciones { get; set; }
        public string? Receta { get; set; }
        public string? OrdenExamenes { get; set; }
        public string? TipoConsulta { get; set; }
        public double? Peso { get; set; }
        public double? Talla { get; set; }
        public double? IMC { get; set; }
        public string? EstadoNutricional { get; set; }
        public int? UsuarioCreacion { get; set; }
        public string? PresionArterial { get; set; }
        public DateTime? FechaConsulta { get; set; }
        public DateTime? FechaProximoControl { get; set; }

        [NotMapped]
        public int FechaPresentable { get; set; }
    }
}
