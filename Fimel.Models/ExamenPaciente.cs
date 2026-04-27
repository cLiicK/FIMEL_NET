namespace Fimel.Models
{
    public class ExamenPaciente
    {
        public int Id { get; set; }
        public int IdPaciente { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? NombreArchivo { get; set; }
        public string? ContenidoArchivo { get; set; }
        public string? MimeType { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
