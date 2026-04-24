using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstitucionesController : ControllerBase
    {
        private FimelDbContext db;
        public InstitucionesController(FimelDbContext context)
        {
            db = context;
        }

        [HttpPatch("{id}/Logo")]
        public IActionResult ActualizarLogo(int id, [FromBody] string logoBase64)
        {
            try
            {
                Instituciones? institucion = db.Instituciones.Find(id);

                if (institucion == null)
                    return NotFound();

                institucion.Logo = logoBase64;
                db.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al actualizar logo de institución: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Instituciones? institucion = db.Instituciones.Where(x => x.Id == id && x.Vigente == "S").FirstOrDefault();

                if (institucion == null)
                    return NotFound();

                return Ok(institucion);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener institucion by id: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
