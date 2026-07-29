namespace Fimel.Models
{
    public class DictadoSesion
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string? TextosPendientes { get; set; }
        public DateTime Expiracion { get; set; }
        public bool Activa { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
    }
}
