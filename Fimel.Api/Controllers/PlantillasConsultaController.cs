using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantillasConsultaController : ControllerBase
    {
        private FimelDbContext db;
        public PlantillasConsultaController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("GetByTipo/{tipo}/{idUsuario}")]
        public IActionResult GetByTipo(string tipo, int idUsuario)
        {
            try
            {
                List<PlantillaConsulta>? plantillas = db.PlantillasConsulta
                    .Include(x => x.Usuario)
                    .Where(x => x.TipoPlantilla == tipo && x.Vigente == "S" && x.Usuario.Id == idUsuario)
                    .OrderBy(x => x.Titulo)
                    .ToList();

                if (plantillas == null || plantillas.Count == 0)
                    return Ok(new List<PlantillaConsulta>());

                return Ok(plantillas);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener Plantillas por Tipo: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(PlantillaConsulta plantilla)
        {
            try
            {
                plantilla.Vigente = "S";
                plantilla.FechaCreacion = DateTime.Now;

                // Asociar el usuario si viene en la plantilla
                if (plantilla.Usuario != null && plantilla.Usuario.Id > 0)
                {
                    var usuario = db.Set<Usuarios>().Find(plantilla.Usuario.Id);
                    if (usuario != null)
                    {
                        plantilla.Usuario = usuario;
                    }
                }

                db.PlantillasConsulta.Add(plantilla);
                db.SaveChanges();

                return Ok(plantilla);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST PlantillaConsulta: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, PlantillaConsulta plantilla)
        {
            try
            {
                PlantillaConsulta? dbPlantilla = db.PlantillasConsulta
                    .Include(x => x.Usuario)
                    .FirstOrDefault(x => x.Id == id);

                if (dbPlantilla == null)
                    return BadRequest("No se encontró la plantilla");

                dbPlantilla.Titulo = plantilla.Titulo;
                dbPlantilla.Contenido = plantilla.Contenido;
                dbPlantilla.TipoPlantilla = plantilla.TipoPlantilla;
                dbPlantilla.Vigente = plantilla.Vigente ?? "S";
                
                // Mantener el usuario original (no se puede cambiar)
                if (plantilla.Usuario != null && plantilla.Usuario.Id > 0)
                {
                    var usuario = db.Set<Usuarios>().Find(plantilla.Usuario.Id);
                    if (usuario != null)
                    {
                        dbPlantilla.Usuario = usuario;
                    }
                }

                db.SaveChanges();

                return Ok(dbPlantilla);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PUT PlantillaConsulta: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            try
            {
                PlantillaConsulta? dbPlantilla = db.PlantillasConsulta.Find(id);
                
                if (dbPlantilla == null)
                    return false;

                dbPlantilla.Vigente = "N";
                
                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error DELETE PlantillaConsulta: {ex}");
                return false;
            }
        }
    }
}

