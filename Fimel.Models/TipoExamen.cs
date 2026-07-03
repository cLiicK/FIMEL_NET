using System.ComponentModel.DataAnnotations.Schema;

namespace Fimel.Models
{
    public class TipoExamen : LayerSuperType
    {
        public int CategoriaExamenId { get; set; }
        public string NombreExamen { get; set; } = string.Empty;
        public string? CodigoFonasa { get; set; }
        public int Orden { get; set; }

        public CategoriaExamen? Categoria { get; set; }

        [NotMapped]
        public string? CategoriaNombre { get; set; }
    }
}
