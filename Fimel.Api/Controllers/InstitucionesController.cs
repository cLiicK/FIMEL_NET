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

        [HttpPost]
        public IActionResult Post(Instituciones institucion)
        {
            try
            {
                institucion.Vigente = "S";
                institucion.FechaCreacion = DateTime.Now;
                db.Instituciones.Add(institucion);
                db.SaveChanges();
                return Ok(institucion);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST Institucion: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Instituciones institucion)
        {
            try
            {
                Instituciones? dbInst = db.Instituciones.Find(id);

                if (dbInst == null)
                    return NotFound();

                dbInst.RazonSocial = institucion.RazonSocial;
                dbInst.Dirección = institucion.Dirección;
                dbInst.Telefono = institucion.Telefono;
                dbInst.Email = institucion.Email;

                db.SaveChanges();

                return Ok(dbInst);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al actualizar institución: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var instituciones = db.Instituciones
                    .Where(x => x.Vigente == "S")
                    .OrderBy(x => x.RazonSocial)
                    .ToList();

                return Ok(instituciones);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetAll Instituciones: {ex}");
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
