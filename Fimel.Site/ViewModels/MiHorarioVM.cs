using Fimel.Models;

namespace Fimel.Site.ViewModels
{
    public class MiHorarioVM
    {
        public ConfiguracionUsuario ConfiguracionUsuario { get; set; }
        public List<HorarioAtencion> HorariosAtencion { get; set; }
        public List<UsuarioVM> ListaUsuarios { get; set; }
    }

    public class UsuarioVM
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
    }
}
