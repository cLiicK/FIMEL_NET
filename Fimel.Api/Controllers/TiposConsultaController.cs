using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TiposConsultaController : ControllerBase
    {
        private readonly FimelDbContext db;
        public TiposConsultaController(FimelDbContext context) => db = context;

        [HttpGet]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var lista = db.TiposConsulta
                    .Where(x => x.Vigente == "S")
                    .OrderBy(x => x.Orden)
                    .ToList();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GET TiposConsulta: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(TipoConsulta tipoConsulta)
        {
            try
            {
                tipoConsulta.Vigente = "S";
                tipoConsulta.FechaCreacion = DateTime.Now;
                db.TiposConsulta.Add(tipoConsulta);
                db.SaveChanges();
                return Ok(tipoConsulta);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST TipoConsulta: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, TipoConsulta tipoConsulta)
        {
            try
            {
                var db_ = db.TiposConsulta.Find(id);
                if (db_ == null) return NotFound();
                db_.Nombre = tipoConsulta.Nombre;
                db_.Orden = tipoConsulta.Orden;
                db.SaveChanges();
                return Ok(db_);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PUT TipoConsulta: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var db_ = db.TiposConsulta.Find(id);
                if (db_ == null) return NotFound();
                db_.Vigente = "N";
                db.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error DELETE TipoConsulta: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
