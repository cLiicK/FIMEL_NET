namespace Fimel.Models
{
    public class UsuarioPerfil : LayerSuperType
    {
        public int UsuarioId { get; set; }
        public int PerfilId { get; set; }

        public Usuarios? Usuario { get; set; }
        public Perfiles? Perfil { get; set; }
    }
}
