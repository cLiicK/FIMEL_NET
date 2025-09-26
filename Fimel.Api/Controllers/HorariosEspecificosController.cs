using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorariosEspecificosController : ControllerBase
    {
        private FimelDbContext db;
        public HorariosEspecificosController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                HorarioEspecifico? horarioEspecifico = db.HorariosEspecificos
                    .Where(x => x.Id == id && x.Vigente == "S")
                    .Include(x => x.Usuario)
                    .FirstOrDefault();

                if (horarioEspecifico == null)
                    return NotFound();

                return Ok(horarioEspecifico);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener horario específico By Id: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(HorarioEspecifico horario)
        {
            try
            {
                Usuarios? dbUsuario = db.Usuarios.Find(horario.Usuario.Id);

                if (dbUsuario == null)
                    return BadRequest("No se encuentra el Usuario");

                horario.Usuario = dbUsuario;
                horario.FechaCreacion = DateTime.Now;
                horario.Vigente = "S";

                db.HorariosEspecificos.Add(horario);
                db.SaveChanges();

                return Ok(horario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error HorariosEspecificos Post: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByUser/{id}")]
        public IActionResult GetByUser(int id)
        {
            try
            {
                List<HorarioEspecifico> horariosEspecificos = db.HorariosEspecificos
                    .Where(x => x.Usuario.Id == id && x.Vigente == "S")
                    .Include(x => x.Usuario)
                    .OrderBy(x => x.FechaEspecifica)
                    .ThenBy(x => x.HoraInicio)
                    .ToList();

                return Ok(horariosEspecificos);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener horarios específicos por usuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, HorarioEspecifico horario)
        {
            try
            {
                HorarioEspecifico? dbHorario = db.HorariosEspecificos.Find(id);

                if (dbHorario == null)
                    return BadRequest("No se encontró el horario específico");

                dbHorario.FechaEspecifica = horario.FechaEspecifica;
                dbHorario.HoraInicio = horario.HoraInicio;
                dbHorario.HoraFin = horario.HoraFin;
                dbHorario.Comentario = horario.Comentario;
                dbHorario.Vigente = horario.Vigente;

                db.SaveChanges();

                return Ok(dbHorario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error HorariosEspecificos Put: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                HorarioEspecifico? dbHorario = db.HorariosEspecificos.Find(id);

                if (dbHorario == null)
                    return BadRequest("No se encontró el horario específico");

                dbHorario.Vigente = "N";
                db.SaveChanges();

                return Ok(new { success = true, message = "Horario específico eliminado correctamente" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error HorariosEspecificos Delete: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
