namespace Fimel.Models
{
    public class PlantillaCampo : LayerSuperType
    {
        public int PlantillaInformeId { get; set; }
        public string NombreCampo { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
        public int Orden { get; set; }
        public bool Obligatorio { get; set; } = false;
    }
}
