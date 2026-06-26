using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasFinancierasController : ControllerBase
    {
        private FimelDbContext db;
        public CategoriasFinancierasController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("GetByInstitucion/{idInstitucion}")]
        public IActionResult GetByInstitucion(int idInstitucion)
        {
            try
            {
                var categorias = db.CategoriasFinancieras
                    .Where(x => x.InstitucionId == idInstitucion && x.Vigente == "S")
                    .OrderBy(x => x.Tipo)
                    .ThenBy(x => x.Nombre)
                    .ToList();

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetByInstitucion CategoriasFinancieras: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(CategoriaFinanciera categoria)
        {
            try
            {
                categoria.Vigente = "S";
                categoria.FechaCreacion = DateTime.Now;

                db.CategoriasFinancieras.Add(categoria);
                db.SaveChanges();

                return Ok(categoria);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST CategoriaFinanciera: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, CategoriaFinanciera categoria)
        {
            try
            {
                CategoriaFinanciera? dbCategoria = db.CategoriasFinancieras.Find(id);

                if (dbCategoria == null)
                    return BadRequest("No se encontró la categoría");

                dbCategoria.Nombre = categoria.Nombre;
                dbCategoria.Tipo = categoria.Tipo;

                db.SaveChanges();

                return Ok(dbCategoria);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PUT CategoriaFinanciera: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            try
            {
                CategoriaFinanciera? dbCategoria = db.CategoriasFinancieras.Find(id);
                if (dbCategoria == null)
                    return false;

                dbCategoria.Vigente = "N";
                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error DELETE CategoriaFinanciera: {ex}");
                return false;
            }
        }
    }
}
