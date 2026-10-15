namespace Fimel.Models
{
    /// <summary>Plan de suscripción de Fimel, espejo del plan creado en Flow (CodigoFlow = planId en Flow).</summary>
    public class PlanSuscripcion : LayerSuperType
    {
        public string CodigoFlow { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public int Monto { get; set; }          // valor de cada cobro (cada Meses meses), en CLP
        public int Meses { get; set; } = 1;     // duración de cada período: 1, 3, 6 o 12
        public int DiasPrueba { get; set; }
    }
}
