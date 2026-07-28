using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private FimelDbContext db;
        public UsuariosController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var usuarios = db.Usuarios
                    .Where(u => u.Vigente == "S")
                    .OrderBy(u => u.Nombres)
                    .ToList();

                var idsUsuarios = usuarios.Select(u => u.Id).ToList();
                var asignaciones = db.UsuarioPerfil
                    .Where(up => idsUsuarios.Contains(up.UsuarioId) && up.Vigente == "S")
                    .ToList();
                var perfiles = db.Perfiles.Where(p => p.Vigente == "S").ToList();

                foreach (var u in usuarios)
                {
                    var idsPerfiles = asignaciones.Where(a => a.UsuarioId == u.Id).Select(a => a.PerfilId).ToList();
                    u.PerfilesAsignados = perfiles.Where(p => idsPerfiles.Contains(p.Id)).ToList();
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetAll Usuarios: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Usuarios? usuario = db.Usuarios.Find(id);
                if (usuario == null)
                    return NotFound();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ObtenerUsuario by Id: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByNombreUsuario/{username}")]
        public IActionResult GetByNombreUsuario(string username)
        {
            try
            {
                Usuarios? usuario = db.Usuarios.Where(x => x.Usuario == username).FirstOrDefault();
                if (usuario == null)
                    return NotFound();

                usuario.Perfil = db.Perfiles.Where(x => x.Id == usuario.IdPerfil).FirstOrDefault();

                var perfilIds = db.UsuarioPerfil
                    .Where(up => up.UsuarioId == usuario.Id && up.Vigente == "S")
                    .Select(up => up.PerfilId)
                    .ToList();

                usuario.PerfilesAsignados = db.Perfiles
                    .Where(p => perfilIds.Contains(p.Id) && p.Vigente == "S")
                    .ToList();

                usuario.ModulosVisibles = db.ModuloPerfil
                    .Where(mp => perfilIds.Contains(mp.PerfilId) && mp.Vigente == "S")
                    .Join(db.Modulos.Where(m => m.Vigente == "S"), mp => mp.ModuloId, m => m.Id, (mp, m) => m)
                    .Distinct()
                    .OrderBy(m => m.Orden)
                    .ToList();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ObtenerUsuario by Username: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(Usuarios usuario)
        {
            try
            {
                bool existe = db.Usuarios.Any(x => x.Usuario == usuario.Usuario && x.Vigente == "S");
                if (existe)
                    return BadRequest("Ya existe un usuario con ese nombre de usuario.");

                var perfilesSeleccionados = usuario.PerfilesAsignados ?? new List<Perfiles>();
                usuario.PerfilesAsignados = null;
                usuario.ModulosVisibles = null;
                usuario.Perfil = null;

                usuario.Vigente = "S";
                usuario.FechaCreacion = DateTime.Now;
                usuario.RequiereCambioClave = "S";
                usuario.Password = Guid.NewGuid().ToString("N");

                if (perfilesSeleccionados.Any())
                    usuario.IdPerfil = perfilesSeleccionados.First().Id;

                db.Usuarios.Add(usuario);
                db.SaveChanges();

                foreach (var perfil in perfilesSeleccionados)
                {
                    db.UsuarioPerfil.Add(new UsuarioPerfil
                    {
                        UsuarioId = usuario.Id,
                        PerfilId = perfil.Id,
                        Vigente = "S",
                        FechaCreacion = DateTime.Now
                    });
                }
                db.SaveChanges();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST Usuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost("{id}/Perfiles/{idPerfil}")]
        public IActionResult AsignarPerfil(int id, int idPerfil)
        {
            try
            {
                var existente = db.UsuarioPerfil
                    .FirstOrDefault(x => x.UsuarioId == id && x.PerfilId == idPerfil);

                if (existente != null)
                {
                    if (existente.Vigente != "S")
                    {
                        existente.Vigente = "S";
                        db.SaveChanges();
                    }
                    return Ok(existente);
                }

                var nuevo = new UsuarioPerfil
                {
                    UsuarioId = id,
                    PerfilId = idPerfil,
                    Vigente = "S",
                    FechaCreacion = DateTime.Now
                };
                db.UsuarioPerfil.Add(nuevo);
                db.SaveChanges();
                return Ok(nuevo);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error AsignarPerfil Usuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}/Perfiles/{idPerfil}")]
        public IActionResult QuitarPerfil(int id, int idPerfil)
        {
            try
            {
                var existente = db.UsuarioPerfil
                    .FirstOrDefault(x => x.UsuarioId == id && x.PerfilId == idPerfil && x.Vigente == "S");

                if (existente == null) return NotFound();

                existente.Vigente = "N";
                db.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error QuitarPerfil Usuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Usuarios usuario)
        {
            try
            {
                Usuarios? dbUsuario = db.Usuarios.Find(id);
                if (dbUsuario == null)
                    return NotFound("No se encontró el Usuario");

                dbUsuario.Nombres = usuario.Nombres;
                dbUsuario.ApellidoPaterno = usuario.ApellidoPaterno;
                dbUsuario.ApellidoMaterno = usuario.ApellidoMaterno;
                dbUsuario.Usuario = usuario.Usuario;
                dbUsuario.Password = usuario.Password;
                dbUsuario.Email = usuario.Email;
                dbUsuario.RequiereCambioClave = usuario.RequiereCambioClave;
                dbUsuario.Vigente = usuario.Vigente;

                db.SaveChanges();

                return Ok(dbUsuario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Put Usuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}/Email")]
        public IActionResult ActualizarEmail(int id, [FromBody] string email)
        {
            try
            {
                Usuarios? dbUsuario = db.Usuarios.Find(id);
                if (dbUsuario == null) return NotFound();
                dbUsuario.Email = email?.Trim();
                db.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error ActualizarEmail Usuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByInstitucion/{idInstitucion}")]
        public IActionResult GetByInstitucion(int idInstitucion)
        {
            try
            {
                var usuarios = db.Usuarios
                    .Where(u => u.IdInstitucion == idInstitucion && u.Vigente == "S").ToList();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener usuarios por institución: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
