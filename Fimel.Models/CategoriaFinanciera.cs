using System.ComponentModel.DataAnnotations.Schema;

namespace Fimel.Models
{
    public class CategoriaFinanciera : LayerSuperType
    {
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "Ingreso" | "Egreso"
        public int InstitucionId { get; set; }

        [NotMapped]
        public Instituciones? Institucion { get; set; }
    }
}
