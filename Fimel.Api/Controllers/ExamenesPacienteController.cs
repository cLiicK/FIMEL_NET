using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamenesPacienteController : ControllerBase
    {
        private FimelDbContext db;
        public ExamenesPacienteController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet("{idPaciente}")]
        public IActionResult GetByPaciente(int idPaciente)
        {
            try
            {
                var examenes = db.ExamenesPaciente
                    .Where(e => e.IdPaciente == idPaciente)
                    .OrderByDescending(e => e.Fecha)
                    .Select(e => new
                    {
                        e.Id,
                        e.Descripcion,
                        e.Fecha,
                        e.NombreArchivo,
                        e.MimeType,
                        e.FechaCreacion,
                        TieneArchivo = e.ContenidoArchivo != null
                    })
                    .ToList();

                return Ok(examenes);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetByPaciente ExamenesPaciente: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var examen = db.ExamenesPaciente.Find(id);
                if (examen == null) return NotFound();
                return Ok(examen);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetById ExamenPaciente: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] ExamenPaciente examen)
        {
            try
            {
                examen.FechaCreacion = DateTime.Now;
                db.ExamenesPaciente.Add(examen);
                db.SaveChanges();
                return Ok(examen);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Post ExamenPaciente: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var examen = db.ExamenesPaciente.Find(id);
                if (examen == null) return NotFound();
                db.ExamenesPaciente.Remove(examen);
                db.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Delete ExamenPaciente: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
