using System.ComponentModel.DataAnnotations.Schema;

namespace Fimel.Models
{
    /// <summary>Suscripción de un usuario profesional a Fimel, cobrada con tarjeta vía Flow.</summary>
    public class SuscripcionUsuario : LayerSuperType
    {
        public int IdUsuario { get; set; }
        public int IdPlan { get; set; }
        public string? FlowCustomerId { get; set; }
        public string? FlowSubscriptionId { get; set; }
        public string Estado { get; set; } = EstadosSuscripcion.Pendiente;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FinPrueba { get; set; }
        public DateTime? PagadoHasta { get; set; }
        public bool PruebaUsada { get; set; }
        public string? TipoTarjeta { get; set; }
        public string? Ultimos4 { get; set; }

        [NotMapped]
        public PlanSuscripcion? Plan { get; set; }
    }

    public static class EstadosSuscripcion
    {
        public const string Pendiente = "Pendiente";   // creada, falta inscribir la tarjeta
        public const string Prueba = "Prueba";         // tarjeta inscrita, dentro de los días gratis
        public const string Activa = "Activa";
        public const string Morosa = "Morosa";
        public const string Cancelada = "Cancelada";
        public const string Exenta = "Exenta";         // usuarios existentes al lanzar el cobro
    }
}
