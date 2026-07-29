using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InformesCampoValorController : ControllerBase
    {
        private FimelDbContext db;
        public InformesCampoValorController(FimelDbContext context) { db = context; }

        [HttpGet]
        [Route("GetByInforme/{informeId}")]
        public IActionResult GetByInforme(int informeId)
        {
            try
            {
                List<InformeCampoValor> valores = db.InformesCampoValor
                    .Where(x => x.InformeGeneradoId == informeId && x.Vigente == "S")
                    .ToList();

                return Ok(valores);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error InformeCampoValor GetByInforme: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(InformeCampoValor valor)
        {
            try
            {
                valor.Vigente = "S";
                valor.FechaCreacion = DateTime.Now;

                db.InformesCampoValor.Add(valor);
                db.SaveChanges();

                return Ok(valor);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error InformeCampoValor Post: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, InformeCampoValor valor)
        {
            try
            {
                InformeCampoValor? dbValor = db.InformesCampoValor.Find(id);
                if (dbValor == null) return NotFound();

                dbValor.Valor = valor.Valor;
                db.SaveChanges();
                return Ok(dbValor);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error InformeCampoValor Put: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
