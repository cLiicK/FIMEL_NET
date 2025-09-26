using Fimel.Models;
using Fimel.Models.Params;
using Fimel.Utils;
using Fimel.Models.Integraciones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private FimelDbContext db;
        private readonly IConfiguration _configuration;
        
        public CitasController(FimelDbContext context, IConfiguration configuration)
        {
            db = context;
            _configuration = configuration;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Cita? cita = db.Citas
                    .Where(x => x.Id == id && x.Vigente == "S")
                    .Include(x => x.Usuario).ThenInclude(t => t.Perfil)
                    .FirstOrDefault();

                if (cita == null)
                    return NotFound();

                return Ok(cita);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Cita Get By Id: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByUser/{id}")]
        public IActionResult GetByUser(int id)
        {
            try
            {
                List<Cita> citas = db.Citas.Where(x => x.Usuario.Id == id && x.Vigente == "S").ToList();

                return Ok(citas);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Citas GetByUser: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByCriteria")]
        public IActionResult GetByCriteria([FromQuery] QueryBuscarCita q)
        {
            try
            {
                IQueryable<Cita> query = db.Citas;

                query = query.Where(x => x.Vigente == "S");

                if (q.FechaInicio.HasValue)
                    query = query.Where(x => x.FechaHoraInicio >= q.FechaInicio.Value);

                if (q.FechaTermino.HasValue)
                    query = query.Where(x => x.FechaHoraFinal <= q.FechaTermino.Value);

                if (q.UsuarioId.HasValue)
                    query = query.Where(x => x.Usuario.Id == q.UsuarioId.Value);

                List<Cita> citas = query.Include(x => x.Usuario).ToList();

                if (citas == null)
                    return NotFound();

                return Ok(citas);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Citas GetByCriteria: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Cita cita)
        {
            try
            {
                Cita? dbCita = db.Citas.Find(id);

                dbCita.NumeroDocumento = cita.NumeroDocumento;
                dbCita.TipoDocumento = cita.TipoDocumento;
                dbCita.CorreoPaciente = cita.CorreoPaciente;
                dbCita.NombrePaciente = cita.NombrePaciente;
                dbCita.FechaHoraFinal = cita.FechaHoraFinal;
                dbCita.FechaHoraInicio = cita.FechaHoraInicio;
                dbCita.Vigente = cita.Vigente;

                db.SaveChanges();

                return Ok(dbCita);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Cita: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(Cita cita)
        {
            try
            {
                Usuarios? dbUsuario = db.Usuarios.Find(cita.Usuario.Id);

                if (dbUsuario == null)
                    return BadRequest();

                cita.Usuario = dbUsuario;
                cita.FechaCreacion = DateTime.Now;
                cita.Vigente = "S";

                db.Citas.Add(cita);
                db.SaveChanges();

                return Ok(cita);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Cita Post: {ex}");
                return StatusCode(500, ex);
            }
        }

      
    }
}
