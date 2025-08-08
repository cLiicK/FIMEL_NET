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

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ObtenerUsuario by Username: {ex}");
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
