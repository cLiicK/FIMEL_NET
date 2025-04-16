using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionesUsuarioController : ControllerBase
    {
        private FimelDbContext db;
        public ConfiguracionesUsuarioController(FimelDbContext context)
        {
            db = context;
        }

        [HttpPost]
        public IActionResult Post(ConfiguracionUsuario config)
        {
            try
            {
                Usuarios? dbUsuario = db.Usuarios.Find(config.Usuario.Id);

                if (dbUsuario == null)
                    return BadRequest("No se encuentra el Usuario");

                config.Usuario = dbUsuario;
                config.FechaCreacion = DateTime.Now;
                config.Vigente = "S";

                db.ConfiguracionesUsuario.Add(config);
                db.SaveChanges();

                return Ok(config);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error ConfiguracionUsuario Post: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, ConfiguracionUsuario config)
        {
            try
            {
                Usuarios? dbUsuario = db.Usuarios.Where(x => x.Id == config.Usuario.Id).FirstOrDefault();

                if (dbUsuario == null)
                    return BadRequest("No se encontró el Usuario");

                ConfiguracionUsuario? dbConfig = db.ConfiguracionesUsuario.Find(id);

                if (dbConfig == null)
                    return BadRequest("No se encontró la configuracion");

                dbConfig.DuracionBloqueHorario = config.DuracionBloqueHorario;

                db.SaveChanges();

                return Ok(config);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al actualizar ConfiguracionUsuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByUser/{id}")]
        public IActionResult GetByUser(int id)
        {
            try
            {
                ConfiguracionUsuario? config = db.ConfiguracionesUsuario.Include(x => x.Usuario).Where(x => x.Usuario.Id == id).FirstOrDefault();

                return Ok(config);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error ConfiguracionUsuario GetByUser: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
