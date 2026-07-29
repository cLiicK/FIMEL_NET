namespace Fimel.Models
{
    public class ModuloPerfil : LayerSuperType
    {
        public int ModuloId { get; set; }
        public int PerfilId { get; set; }

        public Modulos? Modulo { get; set; }
        public Perfiles? Perfil { get; set; }
    }
}
