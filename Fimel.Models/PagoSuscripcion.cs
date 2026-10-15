namespace Fimel.Models
{
    /// <summary>Cobro de Flow asociado a una suscripción. FlowOrder es único para que una notificación repetida no duplique.</summary>
    public class PagoSuscripcion : LayerSuperType
    {
        public int IdSuscripcion { get; set; }
        public long FlowOrder { get; set; }
        public string? Token { get; set; }
        public int Monto { get; set; }
        public int EstadoFlow { get; set; }
        public DateTime? FechaPago { get; set; }
        public string? RespuestaJson { get; set; }
    }
}
