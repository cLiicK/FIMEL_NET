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

        [HttpGet]
        [Route("GetProximosControles")]
        public IActionResult GetProximosControles(int idUsuario, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            try
            {
                var desde = fechaDesde ?? DateTime.Today;
                var hasta = (fechaHasta ?? DateTime.Today.AddDays(30)).AddDays(1);

                var consultas = db.Consultas
                    .Where(c => c.Vigente == "S"
                             && c.UsuarioCreacion == idUsuario
                             && c.FechaProximoControl.HasValue
                             && c.FechaProximoControl >= desde
                             && c.FechaProximoControl < hasta)
                    .OrderBy(c => c.FechaProximoControl)
                    .ToList();

                if (!consultas.Any()) return Ok(consultas);

                var pacienteIds = consultas.Select(c => c.Id_Paciente).Distinct().ToList();
                var pacientes = db.Pacientes.Where(p => pacienteIds.Contains(p.Id)).ToList();

                foreach (var consulta in consultas)
                    consulta.Paciente = pacientes.FirstOrDefault(p => p.Id == consulta.Id_Paciente);

                return Ok(consultas);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetProximosControles: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetProximosControlesParaEnvioHoy")]
        public IActionResult GetProximosControlesParaEnvioHoy()
        {
            try
            {
                var hoy = DateTime.Today;

                var configs = db.ConfiguracionesUsuario
                    .Include(c => c.Usuario)
                    .Where(c => c.Vigente == "S" && c.DiasAvisoPrevioControl > 0)
                    .ToList();

                if (!configs.Any()) return Ok(new List<Consultas>());

                int minDias = configs.Min(c => c.DiasAvisoPrevioControl);
                int maxDias = configs.Max(c => c.DiasAvisoPrevioControl);

                var desde = hoy.AddDays(minDias);
                var hasta = hoy.AddDays(maxDias).AddDays(1);

                var todasConsultas = db.Consultas
                    .Where(c => c.Vigente == "S"
                             && c.FechaProximoControl.HasValue
                             && c.FechaProximoControl >= desde
                             && c.FechaProximoControl < hasta)
                    .ToList();

                var resultado = new List<Consultas>();
                foreach (var cfg in configs)
                {
                    var fechaObjetivo = hoy.AddDays(cfg.DiasAvisoPrevioControl);
                    var consultas = todasConsultas
                        .Where(c => c.UsuarioCreacion == cfg.Usuario.Id
                                 && c.FechaProximoControl!.Value.Date == fechaObjetivo.Date)
                        .ToList();
                    resultado.AddRange(consultas);
                }

                if (!resultado.Any()) return Ok(resultado);

                var pacienteIds = resultado.Select(c => c.Id_Paciente).Distinct().ToList();
                var pacientes = db.Pacientes.Where(p => pacienteIds.Contains(p.Id)).ToList();

                var userIds = resultado
                    .Where(c => c.UsuarioCreacion.HasValue)
                    .Select(c => c.UsuarioCreacion!.Value)
                    .Distinct()
                    .ToList();
                var usuarios = db.Usuarios.Where(u => userIds.Contains(u.Id)).ToList();

                var instIds = usuarios
                    .Where(u => u.IdInstitucion.HasValue)
                    .Select(u => u.IdInstitucion!.Value)
                    .Distinct()
                    .ToList();
                var instituciones = db.Instituciones.Where(i => instIds.Contains(i.Id)).ToList();

                foreach (var usuario in usuarios)
                    usuario.Institucion = instituciones.FirstOrDefault(i => i.Id == usuario.IdInstitucion);

                foreach (var consulta in resultado)
                {
                    consulta.Paciente = pacientes.FirstOrDefault(p => p.Id == consulta.Id_Paciente);
                    if (consulta.Paciente != null)
                        consulta.Paciente.UsuarioConectado = usuarios.FirstOrDefault(u => u.Id == consulta.UsuarioCreacion);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetProximosControlesParaEnvioHoy: {ex}");
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
