using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModulosController : ControllerBase
    {
        private readonly FimelDbContext db;
        public ModulosController(FimelDbContext context) => db = context;

        [HttpGet]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var lista = db.Modulos
                    .Where(x => x.Vigente == "S")
                    .OrderBy(x => x.Orden)
                    .ToList();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetAll Modulos: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(Modulos modulo)
        {
            try
            {
                modulo.Vigente = "S";
                modulo.FechaCreacion = DateTime.Now;
                db.Modulos.Add(modulo);
                db.SaveChanges();
                return Ok(modulo);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST Modulo: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Modulos modulo)
        {
            try
            {
                var db_ = db.Modulos.Find(id);
                if (db_ == null) return NotFound();
                db_.Nombre = modulo.Nombre;
                db_.Controller = modulo.Controller;
                db_.Accion = modulo.Accion;
                db_.Orden = modulo.Orden;
                db.SaveChanges();
                return Ok(db_);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PUT Modulo: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var db_ = db.Modulos.Find(id);
                if (db_ == null) return NotFound();
                db_.Vigente = "N";
                db.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error DELETE Modulo: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
