namespace Fimel.Models.Integraciones
{
    /// <summary>Intercambio Site ↔ API para iniciar la suscripción y confirmar la tarjeta inscrita en Flow.</summary>
    public class SolicitudSuscripcion
    {
        public int IdUsuario { get; set; }
        public int IdPlan { get; set; }
        public string? UrlRetorno { get; set; }
        public string? Token { get; set; }

        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public string? UrlFlow { get; set; }
        public SuscripcionUsuario? Suscripcion { get; set; }
    }
}
