using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TiposExamenController : ControllerBase
    {
        private readonly FimelDbContext db;
        public TiposExamenController(FimelDbContext context) => db = context;

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var lista = db.TiposExamen
                    .Include(x => x.Categoria)
                    .Where(x => x.Vigente == "S")
                    .OrderBy(x => x.Categoria!.Orden)
                    .ThenBy(x => x.Orden)
                    .Select(x => new
                    {
                        x.Id,
                        x.NombreExamen,
                        x.CodigoFonasa,
                        x.Orden,
                        x.CategoriaExamenId,
                        CategoriaNombre = x.Categoria!.Nombre
                    })
                    .ToList();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GET TiposExamen: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet("GetActivos")]
        public IActionResult GetActivos(string? q)
        {
            try
            {
                var query = db.TiposExamen
                    .Include(x => x.Categoria)
                    .Where(x => x.Vigente == "S" && x.Categoria!.Vigente == "S");

                if (!string.IsNullOrWhiteSpace(q))
                    query = query.Where(x => x.NombreExamen.Contains(q));

                var lista = query
                    .OrderBy(x => x.Categoria!.Orden)
                    .ThenBy(x => x.Orden)
                    .Select(x => new
                    {
                        x.Id,
                        x.NombreExamen,
                        x.CodigoFonasa,
                        CategoriaNombre = x.Categoria!.Nombre
                    })
                    .ToList();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GET TiposExamen/GetActivos: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(TipoExamen examen)
        {
            try
            {
                examen.Vigente = "S";
                examen.FechaCreacion = DateTime.Now;
                db.TiposExamen.Add(examen);
                db.SaveChanges();
                return Ok(examen);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST TipoExamen: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, TipoExamen examen)
        {
            try
            {
                var db_ = db.TiposExamen.Find(id);
                if (db_ == null) return BadRequest("No encontrado");
                db_.NombreExamen = examen.NombreExamen;
                db_.CodigoFonasa = examen.CodigoFonasa;
                db_.CategoriaExamenId = examen.CategoriaExamenId;
                db_.Orden = examen.Orden;
                db.SaveChanges();
                return Ok(db_);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PUT TipoExamen: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            try
            {
                var db_ = db.TiposExamen.Find(id);
                if (db_ == null) return false;
                db_.Vigente = "N";
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error DELETE TipoExamen: {ex}");
                return false;
            }
        }
    }
}
