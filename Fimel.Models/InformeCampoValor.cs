namespace Fimel.Models
{
    public class InformeCampoValor : LayerSuperType
    {
        public int InformeGeneradoId { get; set; }
        public string NombreCampo { get; set; } = string.Empty;
        public string? Valor { get; set; }
    }
}
