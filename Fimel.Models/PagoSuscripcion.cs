namespace Fimel.Models
{
    /// <summary>Cobro (invoice) de Flow asociado a una suscripción. FlowInvoiceId es único para que una sincronización repetida no duplique.</summary>
    public class PagoSuscripcion : LayerSuperType
    {
        public int IdSuscripcion { get; set; }
        public long FlowInvoiceId { get; set; }
        public string? Token { get; set; }
        public string? Descripcion { get; set; }
        public int Monto { get; set; }
        public int EstadoFlow { get; set; }             // invoice de Flow: 1 = pagada
        public DateTime? PeriodoDesde { get; set; }
        public DateTime? PeriodoHasta { get; set; }
        public DateTime? FechaPago { get; set; }        // cuando Fimel la vio pagada por primera vez
        public string? RespuestaJson { get; set; }
    }
}
