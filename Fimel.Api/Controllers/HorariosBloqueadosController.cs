using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorariosBloqueadosController : ControllerBase
    {
        private FimelDbContext db;
        public HorariosBloqueadosController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                HorarioBloqueado? horarioBloqueado = db.HorariosBloqueados
                    .Where(x => x.Id == id && x.Vigente == "S")
                    .Include(x => x.Usuario)
                    .FirstOrDefault();

                if (horarioBloqueado == null)
                    return NotFound();

                return Ok(horarioBloqueado);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener horario bloqueado By Id: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(HorarioBloqueado horario)
        {
            try
            {
                Usuarios? dbUsuario = db.Usuarios.Find(horario.Usuario.Id);

                if (dbUsuario == null)
                    return BadRequest("No se encuentra el Usuario");

                horario.Usuario = dbUsuario;
                horario.FechaCreacion = DateTime.Now;
                horario.Vigente = "S";

                db.HorariosBloqueados.Add(horario);
                db.SaveChanges();

                return Ok(horario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error HorariosBloqueados Post: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByUser/{id}")]
        public IActionResult GetByUser(int id)
        {
            try
            {
                List<HorarioBloqueado> horariosBloqueados = db.HorariosBloqueados
                    .Where(x => x.Usuario.Id == id && x.Vigente == "S")
                    .Include(x => x.Usuario)
                    .OrderBy(x => x.FechaBloqueo)
                    .ThenBy(x => x.HoraInicio)
                    .ToList();

                return Ok(horariosBloqueados);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener horarios bloqueados por usuario: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, HorarioBloqueado horario)
        {
            try
            {
                HorarioBloqueado? dbHorario = db.HorariosBloqueados.Find(id);

                if (dbHorario == null)
                    return BadRequest("No se encontró el horario bloqueado");

                dbHorario.FechaBloqueo = horario.FechaBloqueo;
                dbHorario.HoraInicio = horario.HoraInicio;
                dbHorario.HoraFin = horario.HoraFin;
                dbHorario.Comentario = horario.Comentario;
                dbHorario.Vigente = horario.Vigente;

                db.SaveChanges();

                return Ok(dbHorario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error HorariosBloqueados Put: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                HorarioBloqueado? dbHorario = db.HorariosBloqueados.Find(id);

                if (dbHorario == null)
                    return BadRequest("No se encontró el horario bloqueado");

                dbHorario.Vigente = "N";
                db.SaveChanges();

                return Ok(new { success = true, message = "Horario bloqueado eliminado correctamente" });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error HorariosBloqueados Delete: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
