using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InformesGeneradosController : ControllerBase
    {
        private FimelDbContext db;
        public InformesGeneradosController(FimelDbContext context) { db = context; }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                InformeGenerado? informe = db.InformesGenerados
                    .Where(x => x.Id == id && x.Vigente == "S")
                    .FirstOrDefault();

                if (informe == null) return NotFound();

                informe.Valores = db.InformesCampoValor
                    .Where(v => v.InformeGeneradoId == id && v.Vigente == "S")
                    .ToList();

                return Ok(informe);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error InformeGenerado Get: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByPaciente/{pacienteId}")]
        public IActionResult GetByPaciente(int pacienteId)
        {
            try
            {
                List<InformeGenerado> informes = db.InformesGenerados
                    .Where(x => x.PacienteId == pacienteId && x.Vigente == "S")
                    .OrderByDescending(x => x.FechaCreacion)
                    .ToList();

                return Ok(informes);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error InformeGenerado GetByPaciente: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByUsuario/{usuarioId}")]
        public IActionResult GetByUsuario(int usuarioId)
        {
            try
            {
                List<InformeGenerado> informes = db.InformesGenerados
                    .Where(x => x.UsuarioId == usuarioId && x.Vigente == "S")
                    .OrderByDescending(x => x.FechaCreacion)
                    .ToList();

                return Ok(informes);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error InformeGenerado GetByUsuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(InformeGenerado informe)
        {
            try
            {
                informe.Vigente = "S";
                informe.FechaCreacion = DateTime.Now;
                informe.Estado ??= "Borrador";

                var valores = informe.Valores;
                informe.Valores = new List<InformeCampoValor>();

                db.InformesGenerados.Add(informe);
                db.SaveChanges();

                foreach (var valor in valores)
                {
                    valor.InformeGeneradoId = informe.Id;
                    valor.Vigente = "S";
                    valor.FechaCreacion = DateTime.Now;
                    db.InformesCampoValor.Add(valor);
                }
                db.SaveChanges();

                informe.Valores = valores;
                return Ok(informe);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error InformeGenerado Post: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, InformeGenerado informe)
        {
            try
            {
                InformeGenerado? dbInforme = db.InformesGenerados.Find(id);
                if (dbInforme == null) return NotFound();

                dbInforme.HtmlFinal = informe.HtmlFinal;
                dbInforme.Estado = informe.Estado;

                // Actualizar valores: borrar los existentes y recrear
                var valoresExistentes = db.InformesCampoValor
                    .Where(v => v.InformeGeneradoId == id)
                    .ToList();
                foreach (var v in valoresExistentes) v.Vigente = "N";

                foreach (var valor in informe.Valores)
                {
                    valor.InformeGeneradoId = id;
                    valor.Vigente = "S";
                    valor.FechaCreacion = DateTime.Now;
                    db.InformesCampoValor.Add(valor);
                }

                db.SaveChanges();
                return Ok(dbInforme);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error InformeGenerado Put: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                InformeGenerado? dbInforme = db.InformesGenerados.Find(id);
                if (dbInforme == null) return NotFound();

                dbInforme.Vigente = "N";

                var valores = db.InformesCampoValor.Where(v => v.InformeGeneradoId == id).ToList();
                foreach (var v in valores) v.Vigente = "N";

                db.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error InformeGenerado Delete: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
