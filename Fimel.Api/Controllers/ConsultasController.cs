using Fimel.Models;
using Fimel.Models.Params;
using Fimel.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultasController : ControllerBase
    {
        private FimelDbContext db;
        public ConsultasController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Consultas? consulta = db.Consultas.Where(x => x.Id == id && x.Vigente == "S").FirstOrDefault();

                if (consulta == null)
                    return NotFound();

                return Ok(consulta);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener Consulta By Id");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(Consultas consulta)
        {
            try
            {
                consulta.Vigente = "S";
                consulta.FechaCreacion = DateTime.Now;

                db.Consultas.Add(consulta);
                db.SaveChanges();

                return Ok(consulta);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST Consulta: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByIdPaciente/{id}")]
        public IActionResult GetByIdPaciente(int id)
        {
            try
            {
                List<Consultas>? consultas = db.Consultas.Where(x => x.Id_Paciente == id && x.Vigente == "S").ToList();

                if (consultas == null)
                    return NotFound();

                return Ok(consultas);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Obtener Consulta By Id Paciente: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Consultas consulta)
        {
            try
            {
                Consultas? dbConsulta = db.Consultas.Find(id);

                if (dbConsulta == null)
                    return BadRequest("No se encontró la consulta");

                dbConsulta.MotivoConsulta = consulta.MotivoConsulta;
                dbConsulta.Anamnesis = consulta.Anamnesis;
                dbConsulta.ExamenFisico = consulta.ExamenFisico;
                dbConsulta.Diagnostico = consulta.Diagnostico;
                dbConsulta.Indicaciones = consulta.Indicaciones;
                dbConsulta.Receta = consulta.Receta;
                dbConsulta.OrdenExamenes = consulta.OrdenExamenes;
                dbConsulta.Vigente = consulta.Vigente;
                dbConsulta.TipoConsulta = consulta.TipoConsulta;
                dbConsulta.Peso = consulta.Peso;
                dbConsulta.Talla = consulta.Talla;
                dbConsulta.IMC = consulta.IMC;
                dbConsulta.EstadoNutricional = consulta.EstadoNutricional;

                db.SaveChanges();

                return Ok(dbConsulta);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al Actualizar Consulta: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            try
            {
                Consultas? dbConsulta = db.Consultas.Find(id);
                if (dbConsulta == null)
                    return false;

                dbConsulta.Vigente = "N";
                
                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Consultas Delete: {ex}");
                return false;
            }
        }

    }
}
