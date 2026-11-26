using Fimel.Models;

namespace Fimel.Api.Services.Video
{
    /// <summary>Sala creada en el proveedor de video, con un link para cada participante.</summary>
    public record SalaVideo(string Nombre, string UrlPaciente, string UrlProfesional);

    /// <summary>
    /// Proveedor de videollamadas para las videoconsultas. Hoy Daily (DailyVideoProveedor); para agregar otro
    /// (p.ej. Whereby) basta con implementar esta interfaz y registrarlo en Program.cs según Videoconsulta:Proveedor.
    /// </summary>
    public interface IVideoProveedor
    {
        /// <summary>True si el proveedor está habilitado y configurado; si no, las citas se guardan sin sala.</summary>
        bool Disponible { get; }

        /// <summary>Crea una sala privada para la cita, abierta entre 15 min antes del inicio y 30 min después del término.</summary>
        Task<SalaVideo> CrearSalaAsync(Cita cita);

        /// <summary>Elimina la sala (cita anulada o reagendada). No falla si la sala ya no existe.</summary>
        Task EliminarSalaAsync(string nombre);
    }

    /// <summary>Se usa cuando Videoconsulta:Habilitada = false o el proveedor configurado no existe.</summary>
    public class SinVideoProveedor : IVideoProveedor
    {
        public bool Disponible => false;
        public Task<SalaVideo> CrearSalaAsync(Cita cita) => throw new InvalidOperationException("Las videoconsultas no están habilitadas (Videoconsulta:Habilitada).");
        public Task EliminarSalaAsync(string nombre) => Task.CompletedTask;
    }
}
