using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorariosAtencionController : ControllerBase
    {
        private FimelDbContext db;
        public HorariosAtencionController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                HorarioAtencion? horarioAtencion = db.HorariosAtencion.Where(x => x.Id == id && x.Vigente == "S").FirstOrDefault();

                if (horarioAtencion == null)
                    return NotFound();

                return Ok(horarioAtencion);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener horario atencion By Id");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(HorarioAtencion horario)
        {
            try
            {
                Usuarios? dbUsuario = db.Usuarios.Find(horario.Usuario.Id);

                if (dbUsuario == null)
                    return BadRequest("No se encuentra el Usuario");

                horario.Usuario = dbUsuario;
                horario.FechaCreacion = DateTime.Now;
                horario.Vigente = "S";

                db.HorariosAtencion.Add(horario);
                db.SaveChanges();

                return Ok(horario);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error HorariosAtencion Post: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByUser/{id}")]
        public IActionResult GetByUser(int id)
        {
            try
            {
                List<HorarioAtencion> horarios = db.HorariosAtencion.Where(x => x.Usuario.Id == id && x.Vigente == "S").ToList();

                return Ok(horarios);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error HorariosAtencion GetByUser: {ex}");
                return StatusCode(500, ex);
            }
        }

		[HttpPut("{id}")]
		public IActionResult Put(int id, HorarioAtencion horarioAtencion)
		{
			try
			{
				HorarioAtencion? dbHorarioAtencion = db.HorariosAtencion.Find(id);

				dbHorarioAtencion.Vigente = horarioAtencion.Vigente;

				db.SaveChanges();

				return Ok(dbHorarioAtencion);
			}
			catch (Exception ex)
			{
				Logger.Log($"Error HorariosAtencion Put: {ex}");
				return StatusCode(500, ex);
			}
		}
	}
}
