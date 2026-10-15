namespace Fimel.Models
{
    /// <summary>Plan de suscripción de Fimel, espejo del plan creado en Flow (CodigoFlow = planId en Flow).</summary>
    public class PlanSuscripcion : LayerSuperType
    {
        public string CodigoFlow { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public int Monto { get; set; }
        public int DiasPrueba { get; set; }
    }
}
