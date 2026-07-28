using static Fimel.Models.Enums;

namespace Fimel.Models.Extensions
{
    public static class UsuarioAuthExtensions
    {
        public static bool TienePerfil(this Usuarios? usuario, EnumPerfiles perfil)
        {
            if (usuario == null) return false;

            if (usuario.PerfilesAsignados != null && usuario.PerfilesAsignados.Any())
                return usuario.PerfilesAsignados.Any(p => p.Id == (int)perfil);

            // Fallback legado: sesiones serializadas antes de existir PerfilesAsignados
            return usuario.IdPerfil == (int)perfil;
        }

        public static bool TieneAlgunPerfil(this Usuarios? usuario, params EnumPerfiles[] perfiles)
            => perfiles.Any(p => usuario.TienePerfil(p));
    }
}
