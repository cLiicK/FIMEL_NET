using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaExamenesController : ControllerBase
    {
        private readonly FimelDbContext db;
        public CategoriaExamenesController(FimelDbContext context) => db = context;

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var lista = db.CategoriasExamen
                    .Where(x => x.Vigente == "S")
                    .OrderBy(x => x.Orden)
                    .ToList();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GET CategoriaExamenes: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(CategoriaExamen categoria)
        {
            try
            {
                categoria.Vigente = "S";
                categoria.FechaCreacion = DateTime.Now;
                db.CategoriasExamen.Add(categoria);
                db.SaveChanges();
                return Ok(categoria);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST CategoriaExamen: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, CategoriaExamen categoria)
        {
            try
            {
                var db_ = db.CategoriasExamen.Find(id);
                if (db_ == null) return BadRequest("No encontrado");
                db_.Nombre = categoria.Nombre;
                db_.Orden = categoria.Orden;
                db.SaveChanges();
                return Ok(db_);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PUT CategoriaExamen: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            try
            {
                var db_ = db.CategoriasExamen.Find(id);
                if (db_ == null) return false;
                db_.Vigente = "N";
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error DELETE CategoriaExamen: {ex}");
                return false;
            }
        }
    }
}
