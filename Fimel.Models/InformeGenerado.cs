using System.ComponentModel.DataAnnotations.Schema;

namespace Fimel.Models
{
    public class InformeGenerado : LayerSuperType
    {
        public int PacienteId { get; set; }
        public int UsuarioId { get; set; }
        public int PlantillaInformeId { get; set; }
        public string? HtmlFinal { get; set; }
        public string? Estado { get; set; }

        [NotMapped]
        public List<InformeCampoValor> Valores { get; set; } = new();
    }
}
