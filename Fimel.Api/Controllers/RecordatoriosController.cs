using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordatoriosController : ControllerBase
    {
        private FimelDbContext db;
        public RecordatoriosController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("GetByPaciente/{idPaciente}")]
        public IActionResult GetByPaciente(int idPaciente)
        {
            try
            {
                List<Recordatorio> recordatorios = db.Recordatorios
                    .Where(r => r.IdPaciente == idPaciente && r.Vigente == "S")
                    .OrderBy(r => r.FechaProximoEnvio)
                    .ToList();

                return Ok(recordatorios);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener Recordatorios por Paciente: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetPendientesParaEnvioHoy")]
        public IActionResult GetPendientesParaEnvioHoy()
        {
            try
            {
                var hoy = DateTime.Today;

                var pendientes = db.Recordatorios
                    .Where(r => r.Vigente == "S"
                             && r.FechaProximoEnvio.Date <= hoy
                             && (r.UltimaFechaEnvio == null || r.UltimaFechaEnvio.Value.Date != hoy))
                    .ToList();

                if (!pendientes.Any()) return Ok(pendientes);

                var pacienteIds = pendientes.Select(r => r.IdPaciente).Distinct().ToList();
                var pacientes = db.Pacientes.Where(p => pacienteIds.Contains(p.Id)).ToList();

                var userIds = pendientes
                    .Where(r => r.UsuarioCreacion.HasValue)
                    .Select(r => r.UsuarioCreacion!.Value)
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

                foreach (var recordatorio in pendientes)
                {
                    recordatorio.Paciente = pacientes.FirstOrDefault(p => p.Id == recordatorio.IdPaciente);
                    if (recordatorio.Paciente != null)
                        recordatorio.Paciente.UsuarioConectado = usuarios.FirstOrDefault(u => u.Id == recordatorio.UsuarioCreacion);
                }

                return Ok(pendientes);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetPendientesParaEnvioHoy Recordatorio: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(Recordatorio recordatorio)
        {
            try
            {
                recordatorio.Vigente = "S";
                recordatorio.FechaCreacion = DateTime.Now;

                db.Recordatorios.Add(recordatorio);
                db.SaveChanges();

                return Ok(recordatorio);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST Recordatorio: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("MarcarEnviado/{id}")]
        public IActionResult MarcarEnviado(int id)
        {
            try
            {
                Recordatorio? recordatorio = db.Recordatorios.Find(id);
                if (recordatorio == null)
                    return BadRequest("No se encontró el recordatorio");

                var hoy = DateTime.Today;
                recordatorio.UltimaFechaEnvio = hoy;

                var proxima = recordatorio.FechaProximoEnvio;
                do
                {
                    proxima = Utileria.CalcularProximaFechaRecordatorio(proxima, recordatorio.RepetirCada);
                }
                while (proxima.Date <= hoy);
                recordatorio.FechaProximoEnvio = proxima;

                db.SaveChanges();

                return Ok(recordatorio);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error MarcarEnviado Recordatorio: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            try
            {
                Recordatorio? dbRecordatorio = db.Recordatorios.Find(id);

                if (dbRecordatorio == null)
                    return false;

                dbRecordatorio.Vigente = "N";

                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error DELETE Recordatorio: {ex}");
                return false;
            }
        }
    }
}
