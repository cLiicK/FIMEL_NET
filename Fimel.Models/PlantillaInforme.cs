using System.ComponentModel.DataAnnotations.Schema;

namespace Fimel.Models
{
    public class PlantillaInforme : LayerSuperType
    {
        public string Nombre { get; set; } = string.Empty;
        public string? TipoExamen { get; set; }
        public string HtmlBase { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public int? UsuarioId { get; set; }

        [NotMapped]
        public List<PlantillaCampo> Campos { get; set; } = new();
    }
}
