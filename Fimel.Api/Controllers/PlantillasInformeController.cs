using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantillasInformeController : ControllerBase
    {
        private FimelDbContext db;
        public PlantillasInformeController(FimelDbContext context) { db = context; }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                PlantillaInforme? plantilla = db.PlantillasInforme
                    .Where(x => x.Id == id && x.Vigente == "S")
                    .FirstOrDefault();

                if (plantilla == null) return NotFound();

                plantilla.Campos = db.PlantillasCampo
                    .Where(c => c.PlantillaInformeId == id && c.Vigente == "S")
                    .OrderBy(c => c.Orden)
                    .ToList();

                return Ok(plantilla);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PlantillaInforme Get: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByUsuario/{usuarioId}")]
        public IActionResult GetByUsuario(int usuarioId)
        {
            try
            {
                List<PlantillaInforme> plantillas = db.PlantillasInforme
                    .Where(x => x.Vigente == "S" && (x.UsuarioId == usuarioId || x.UsuarioId == null))
                    .OrderBy(x => x.Nombre)
                    .ToList();

                return Ok(plantillas);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PlantillaInforme GetByUsuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(PlantillaInforme plantilla)
        {
            try
            {
                plantilla.Vigente = "S";
                plantilla.FechaCreacion = DateTime.Now;

                db.PlantillasInforme.Add(plantilla);
                db.SaveChanges();

                if (plantilla.Campos.Any())
                {
                    foreach (var campo in plantilla.Campos)
                    {
                        campo.PlantillaInformeId = plantilla.Id;
                        campo.Vigente = "S";
                        campo.FechaCreacion = DateTime.Now;
                        db.PlantillasCampo.Add(campo);
                    }
                    db.SaveChanges();
                }

                return Ok(plantilla);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PlantillaInforme Post: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, PlantillaInforme plantilla)
        {
            try
            {
                PlantillaInforme? dbPlantilla = db.PlantillasInforme.Find(id);
                if (dbPlantilla == null) return NotFound();

                dbPlantilla.Nombre = plantilla.Nombre;
                dbPlantilla.TipoExamen = plantilla.TipoExamen;
                dbPlantilla.HtmlBase = plantilla.HtmlBase;
                dbPlantilla.Activa = plantilla.Activa;

                db.SaveChanges();

                return Ok(dbPlantilla);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PlantillaInforme Put: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                PlantillaInforme? dbPlantilla = db.PlantillasInforme.Find(id);
                if (dbPlantilla == null) return NotFound();

                dbPlantilla.Vigente = "N";

                var campos = db.PlantillasCampo.Where(c => c.PlantillaInformeId == id).ToList();
                foreach (var campo in campos) campo.Vigente = "N";

                db.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PlantillaInforme Delete: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
