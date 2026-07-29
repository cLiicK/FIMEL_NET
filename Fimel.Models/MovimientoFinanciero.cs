using System.ComponentModel.DataAnnotations.Schema;

namespace Fimel.Models
{
    public class MovimientoFinanciero : LayerSuperType
    {
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Descripcion { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Ingreso" | "Egreso"
        public int CategoriaFinancieraId { get; set; }
        public int InstitucionId { get; set; }
        public int UsuarioId { get; set; }
        public int? ConsultaId { get; set; }

        [NotMapped]
        public CategoriaFinanciera? Categoria { get; set; }
        [NotMapped]
        public Usuarios? Usuario { get; set; }
        [NotMapped]
        public string? NombrePaciente { get; set; }
    }
}
