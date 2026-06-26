using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantillasCampoController : ControllerBase
    {
        private FimelDbContext db;
        public PlantillasCampoController(FimelDbContext context) { db = context; }

        [HttpGet]
        [Route("GetByPlantilla/{plantillaId}")]
        public IActionResult GetByPlantilla(int plantillaId)
        {
            try
            {
                List<PlantillaCampo> campos = db.PlantillasCampo
                    .Where(x => x.PlantillaInformeId == plantillaId && x.Vigente == "S")
                    .OrderBy(x => x.Orden)
                    .ToList();

                return Ok(campos);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PlantillaCampo GetByPlantilla: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(PlantillaCampo campo)
        {
            try
            {
                campo.Vigente = "S";
                campo.FechaCreacion = DateTime.Now;

                db.PlantillasCampo.Add(campo);
                db.SaveChanges();

                return Ok(campo);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PlantillaCampo Post: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, PlantillaCampo campo)
        {
            try
            {
                PlantillaCampo? dbCampo = db.PlantillasCampo.Find(id);
                if (dbCampo == null) return NotFound();

                dbCampo.NombreCampo = campo.NombreCampo;
                dbCampo.Etiqueta = campo.Etiqueta;
                dbCampo.Orden = campo.Orden;
                dbCampo.Obligatorio = campo.Obligatorio;

                db.SaveChanges();
                return Ok(dbCampo);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PlantillaCampo Put: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                PlantillaCampo? dbCampo = db.PlantillasCampo.Find(id);
                if (dbCampo == null) return NotFound();

                dbCampo.Vigente = "N";
                db.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PlantillaCampo Delete: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
