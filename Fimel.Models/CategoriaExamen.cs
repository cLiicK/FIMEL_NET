namespace Fimel.Models
{
    public class CategoriaExamen : LayerSuperType
    {
        public string Nombre { get; set; } = string.Empty;
        public int Orden { get; set; }

        public ICollection<TipoExamen>? TiposExamen { get; set; }
    }
}
