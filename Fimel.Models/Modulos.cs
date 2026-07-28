namespace Fimel.Models
{
    public class Modulos : LayerSuperType
    {
        public string Nombre { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public int Orden { get; set; }
    }
}
