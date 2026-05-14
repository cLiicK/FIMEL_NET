using Fimel.Models;

namespace Fimel.Site.ViewModels
{
    public class ConfiguracionUsuarioVM
    {
        public ConfiguracionUsuario Configuracion { get; set; } = new ConfiguracionUsuario();
        public Usuarios Usuario { get; set; }
        public Instituciones? Institucion { get; set; }
    }
}
