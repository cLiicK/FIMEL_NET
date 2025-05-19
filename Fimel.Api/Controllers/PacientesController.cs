using Fimel.Models;
using Fimel.Models.Params;
using Fimel.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private FimelDbContext db;
        public PacientesController(FimelDbContext context)
        {
            db = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Pacientes? paciente = db.Pacientes.Where(x => x.Id == id && x.Vigente == "S").FirstOrDefault();

                if (paciente == null)
                    return NotFound();

                return Ok(paciente);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Paciente Get By Id: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByRut/{rut}")]
        public IActionResult GetByRut(int rut)
        {
            try
            {
                Pacientes? paciente = db.Pacientes.Where(x => x.Rut == rut && x.Vigente == "S").FirstOrDefault();

                if (paciente == null)
                    return NotFound();

                return Ok(paciente);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ObtenerPaciente By Rut: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByNumeroDocumento/{numDoc}")]
        public IActionResult GetByNumeroDocumento(string numDoc)
        {
            try
            {
                Pacientes? paciente = db.Pacientes.Where(x => x.NumeroDocumento == numDoc && x.Vigente == "S").FirstOrDefault();

                if (paciente == null)
                    return NotFound();

                return Ok(paciente);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ObtenerPaciente By NumeroDocumento: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByInstitucionId/{institucionId}")]
        public IActionResult GetByInstitucionId(int institucionId)
        {
            try
            {
                List<Usuarios> _usuarios = db.Usuarios.Where(t => t.IdInstitucion == institucionId && t.Vigente == "S").ToList();

                if (_usuarios.Count == 0)
                    return NotFound();

                List<Pacientes> pacientes = new List<Pacientes>();
                foreach (var usuario in _usuarios)
                {
                    pacientes.AddRange(db.Pacientes.Where(t => t.UsuarioCreacion == usuario.Id && t.Vigente == "S").ToList());
                }
                if (pacientes.Count == 0)
                    return NotFound();

                return Ok(pacientes);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al ObtenerPaciente By InstitucionId: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetByCriteria")]
        public IActionResult GetByCriteria([FromQuery] QueryBuscarPacientes q)
        {
            try
            {
                IQueryable<Pacientes> query = db.Pacientes;

                query = query.Where(x => x.Vigente == "S");

                if(q.UsuarioId.HasValue && q.UsuarioId > 0)
                    query = query.Where(x => x.UsuarioCreacion == q.UsuarioId);

                if (q.FechaConsultaDesde.HasValue)
                    query = query.Where(x => x.FechaCreacion >= q.FechaConsultaDesde);

                if (q.FechaConsultaHasta.HasValue)
                    query = query.Where(x => x.FechaCreacion <= q.FechaConsultaHasta);


                if (!string.IsNullOrEmpty(q.NumDoc))
                    query = query.Where(x => x.NumeroDocumento == q.NumDoc);

                if (q.Rut.HasValue && q.Rut > 0)
                    query = query.Where(x => x.Rut == q.Rut);

                List<Pacientes> pacientes = query.ToList();

                if (pacientes == null)
                    return NotFound();

                return Ok(pacientes);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Pacientes GetByCriteria: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public IActionResult Post(Pacientes paciente)
        {
            try
            {
                paciente.Vigente = "S";
                paciente.FechaCreacion = DateTime.Now;

                db.Pacientes.Add(paciente);
                db.SaveChanges();

                return Ok(paciente);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al crear Paciente: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Pacientes paciente)
        {
            try
            {
                Pacientes? dbPaciente = db.Pacientes.Find(id);

                if (dbPaciente == null)
                    return BadRequest("No se encontró el Paciente");

                dbPaciente.Rut = paciente.Rut;
                dbPaciente.Dv = paciente.Dv;
                dbPaciente.Nombres = paciente.Nombres;
                dbPaciente.FechaNacimiento = paciente.FechaNacimiento;
                dbPaciente.Direccion = paciente.Direccion;
                dbPaciente.Celular = paciente.Celular;
                dbPaciente.Email = paciente.Email;
                dbPaciente.Nacionalidad = paciente.Nacionalidad;
                dbPaciente.Prevision = paciente.Prevision;
                dbPaciente.AntFamiliares = paciente.AntFamiliares;
                dbPaciente.AntPersonales = paciente.AntPersonales;
                dbPaciente.AntQuirurgicos = paciente.AntQuirurgicos;
                dbPaciente.Tabaco = paciente.Tabaco;
                dbPaciente.DescTabaco = paciente.DescTabaco;
                dbPaciente.Alcohol = paciente.Alcohol;
                dbPaciente.DescAlcohol = paciente.DescAlcohol;
                dbPaciente.Drogas = paciente.Drogas;
                dbPaciente.DescDrogas = paciente.DescDrogas;
                dbPaciente.Alergias = paciente.Alergias;
                dbPaciente.DescAlergias = paciente.DescAlergias;
                dbPaciente.Vigente = "S";
                dbPaciente.Gesta = paciente.Gesta;
                dbPaciente.Parto = paciente.Parto;
                dbPaciente.Aborto = paciente.Aborto;
                dbPaciente.Menarquia = paciente.Menarquia;
                dbPaciente.Menopausia = paciente.Menopausia;
                dbPaciente.Religion = paciente.Religion;
                dbPaciente.RegimenAlimenticio = paciente.RegimenAlimenticio;
                dbPaciente.Medicamentos = paciente.Medicamentos;
                dbPaciente.SexoBiologico = paciente.SexoBiologico;
                dbPaciente.IdentidadGenero = paciente.IdentidadGenero;
                dbPaciente.OrientacionSexual = paciente.OrientacionSexual;
                dbPaciente.PrimerApellido = paciente.PrimerApellido;
                dbPaciente.SegundoApellido = paciente.SegundoApellido;
                dbPaciente.TipoDocumento = paciente.TipoDocumento;
                dbPaciente.NumeroDocumento = paciente.NumeroDocumento;
                dbPaciente.GrupoRH = paciente.GrupoRH;
                dbPaciente.Inmunizaciones = paciente.Inmunizaciones;

                db.SaveChanges();

                return Ok(dbPaciente);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al actualizar Paciente: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            try
            {
                Pacientes? dbPaciente = db.Pacientes.Find(id);
                if (dbPaciente == null)
                    return false;

                List<Consultas>? dbConsultas = db.Consultas.Where(x => x.Id_Paciente == dbPaciente.Id).ToList();

                foreach (var consulta in dbConsultas)
                {
                    consulta.Vigente = "N";
                    db.SaveChanges();
                }

                dbPaciente.Vigente = "N";
                
                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Pacientes Delete: {ex}");
                return false;
            }
        }

    }
}
