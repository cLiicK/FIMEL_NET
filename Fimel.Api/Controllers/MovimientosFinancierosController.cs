using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosFinancierosController : ControllerBase
    {
        private FimelDbContext db;
        public MovimientosFinancierosController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("GetByCriteria")]
        public IActionResult GetByCriteria(int? idInstitucion, int? idUsuario, DateTime? fechaDesde, DateTime? fechaHasta, string? tipo)
        {
            try
            {
                var query = db.MovimientosFinancieros.Where(x => x.Vigente == "S");

                if (idInstitucion.HasValue)
                    query = query.Where(x => x.InstitucionId == idInstitucion.Value);

                if (idUsuario.HasValue)
                    query = query.Where(x => x.UsuarioId == idUsuario.Value);

                if (fechaDesde.HasValue)
                    query = query.Where(x => x.Fecha >= fechaDesde.Value.Date);

                if (fechaHasta.HasValue)
                    query = query.Where(x => x.Fecha < fechaHasta.Value.Date.AddDays(1));

                if (!string.IsNullOrEmpty(tipo))
                    query = query.Where(x => x.Tipo == tipo);

                var movimientos = query.OrderByDescending(x => x.Fecha).ToList();

                if (!movimientos.Any())
                    return Ok(movimientos);

                var categoriaIds = movimientos.Select(m => m.CategoriaFinancieraId).Distinct().ToList();
                var categorias = db.CategoriasFinancieras.Where(c => categoriaIds.Contains(c.Id)).ToList();

                var usuarioIds = movimientos.Select(m => m.UsuarioId).Distinct().ToList();
                var usuarios = db.Usuarios.Where(u => usuarioIds.Contains(u.Id)).ToList();

                var consultaIds = movimientos.Where(m => m.ConsultaId.HasValue).Select(m => m.ConsultaId!.Value).Distinct().ToList();
                List<Consultas> consultas = new();
                if (consultaIds.Any())
                    consultas = db.Consultas.Where(c => consultaIds.Contains(c.Id)).ToList();

                var pacienteIds = consultas.Select(c => c.Id_Paciente).Distinct().ToList();
                var pacientes = pacienteIds.Any()
                    ? db.Pacientes.Where(p => pacienteIds.Contains(p.Id)).ToList()
                    : new List<Pacientes>();

                foreach (var mov in movimientos)
                {
                    mov.Categoria = categorias.FirstOrDefault(c => c.Id == mov.CategoriaFinancieraId);
                    mov.Usuario = usuarios.FirstOrDefault(u => u.Id == mov.UsuarioId);

                    if (mov.ConsultaId.HasValue)
                    {
                        var consulta = consultas.FirstOrDefault(c => c.Id == mov.ConsultaId.Value);
                        if (consulta != null)
                        {
                            var paciente = pacientes.FirstOrDefault(p => p.Id == consulta.Id_Paciente);
                            mov.NombrePaciente = paciente != null
                                ? $"{paciente.Nombres} {paciente.PrimerApellido}".Trim()
                                : null;
                        }
                    }
                }

                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetByCriteria MovimientosFinancieros: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(MovimientoFinanciero movimiento)
        {
            try
            {
                movimiento.Vigente = "S";
                movimiento.FechaCreacion = DateTime.Now;

                db.MovimientosFinancieros.Add(movimiento);
                db.SaveChanges();

                return Ok(movimiento);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error POST MovimientoFinanciero: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, MovimientoFinanciero movimiento)
        {
            try
            {
                MovimientoFinanciero? dbMov = db.MovimientosFinancieros.Find(id);

                if (dbMov == null)
                    return BadRequest("No se encontró el movimiento");

                dbMov.Monto = movimiento.Monto;
                dbMov.Fecha = movimiento.Fecha;
                dbMov.Descripcion = movimiento.Descripcion;
                dbMov.Tipo = movimiento.Tipo;
                dbMov.CategoriaFinancieraId = movimiento.CategoriaFinancieraId;
                dbMov.ConsultaId = movimiento.ConsultaId;

                db.SaveChanges();

                return Ok(dbMov);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error PUT MovimientoFinanciero: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            try
            {
                MovimientoFinanciero? dbMov = db.MovimientosFinancieros.Find(id);
                if (dbMov == null)
                    return false;

                dbMov.Vigente = "N";
                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error DELETE MovimientoFinanciero: {ex}");
                return false;
            }
        }
    }
}
