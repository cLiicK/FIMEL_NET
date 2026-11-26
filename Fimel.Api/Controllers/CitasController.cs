using Fimel.Models;
using Fimel.Models.Params;
using Fimel.Utils;
using Fimel.Models.Integraciones;
using Fimel.Api.Services.Video;
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
        private readonly IVideoProveedor _video;

        /// <summary>
        /// VIDEOCONSULTA OCULTA (2026-11) - poner en false para reactivar.
        /// Mientras esté en true la API ignora EsVideoconsulta al crear/modificar citas (siempre false) y no crea salas,
        /// aunque Videoconsulta:Habilitada esté en true.
        /// </summary>
        private static readonly bool VideoconsultaOculta = true;

        public CitasController(FimelDbContext context, IConfiguration configuration, IVideoProveedor video)
        {
            db = context;
            _configuration = configuration;
            _video = video;
        }

        /// <summary>True si se pueden crear salas de videoconsulta (Videoconsulta:Habilitada y proveedor configurado).</summary>
        [HttpGet("VideoconsultaHabilitada")]
        public IActionResult VideoconsultaHabilitada() => Ok(_video.Disponible);

        /// <summary>Cita de la página pública de videoconsulta del paciente, con la institución del profesional (logo).</summary>
        [HttpGet("GetByVideoToken/{token}")]
        public IActionResult GetByVideoToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) return NotFound();

                Cita? cita = db.Citas
                    .Where(x => x.VideoToken == token && x.Vigente == "S")
                    .Include(x => x.Usuario)
                    .FirstOrDefault();

                if (cita == null) return NotFound();

                if (cita.Usuario?.IdInstitucion.HasValue == true)
                    cita.Usuario.Institucion = db.Instituciones.Find(cita.Usuario.IdInstitucion.Value);

                return Ok(cita);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Cita GetByVideoToken: {ex}");
                return StatusCode(500, ex);
            }
        }

        /// <summary>
        /// Crea la sala si la cita es videoconsulta y no la tiene (Daily falló al agendar, o se habilitó después).
        /// Lo llaman las páginas del paciente y del profesional antes de entrar.
        /// </summary>
        [HttpPut("{id}/AsegurarSalaVideo")]
        public async Task<IActionResult> AsegurarSalaVideo(int id)
        {
            try
            {
                Cita? dbCita = db.Citas
                    .Where(x => x.Id == id && x.Vigente == "S")
                    .Include(x => x.Usuario)
                    .FirstOrDefault();

                if (dbCita == null) return NotFound("Cita no encontrada");

                if (!VideoconsultaOculta && dbCita.EsVideoconsulta && string.IsNullOrEmpty(dbCita.VideoSalaNombre))
                {
                    await SincronizarSalaVideoAsync(dbCita, recrear: false);
                    db.SaveChanges();
                }

                return Ok(dbCita);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Cita AsegurarSalaVideo: {ex}");
                return StatusCode(500, ex);
            }
        }

        /// <summary>
        /// Deja la sala de video acorde a la cita: la elimina si la cita dejó de ser videoconsulta o se anuló,
        /// la recrea si cambió el horario (recrear = true) y la crea si falta. Nunca lanza: si el proveedor falla,
        /// la cita se guarda igual sin sala y queda en el log (se reintenta al abrir la página con AsegurarSalaVideo).
        /// </summary>
        private async Task SincronizarSalaVideoAsync(Cita cita, bool recrear)
        {
            bool necesitaSala = cita.EsVideoconsulta && cita.Vigente == "S";

            if (!string.IsNullOrEmpty(cita.VideoSalaNombre) && (!necesitaSala || recrear))
            {
                try { await _video.EliminarSalaAsync(cita.VideoSalaNombre); }
                catch (Exception ex) { Logger.Log($"Videoconsulta: error al eliminar la sala {cita.VideoSalaNombre} de la cita {cita.Id}: {ex.Message}"); }

                cita.VideoSalaNombre = null;
                cita.VideoUrlPaciente = null;
                cita.VideoUrlProfesional = null;
            }

            if (!necesitaSala) return;

            // El link público del paciente se mantiene aunque se recree la sala (los correos anteriores siguen sirviendo)
            cita.VideoToken ??= NuevoTokenVideo();

            if (!string.IsNullOrEmpty(cita.VideoSalaNombre)) return;

            if (!_video.Disponible)
            {
                Logger.Log($"Videoconsulta: la cita {cita.Id} es videoconsulta pero Videoconsulta:Habilitada está apagada o falta configurar el proveedor; se guarda sin sala.");
                return;
            }

            if (VideoconsultaHorario.Cierre(cita.FechaHoraFinal) <= VideoconsultaHorario.AhoraChile())
                return; // ya pasó: no tiene sentido crear la sala

            try
            {
                SalaVideo sala = await _video.CrearSalaAsync(cita);
                cita.VideoSalaNombre = sala.Nombre;
                cita.VideoUrlPaciente = sala.UrlPaciente;
                cita.VideoUrlProfesional = sala.UrlProfesional;
            }
            catch (Exception ex)
            {
                Logger.Log($"Videoconsulta: error al crear la sala de la cita {cita.Id}; se guarda sin sala: {ex}");
            }
        }

        private static string NuevoTokenVideo() =>
            Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

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
        public async Task<IActionResult> Put(int id, Cita cita)
        {
            try
            {
                Cita? dbCita = db.Citas.Include(x => x.Usuario).FirstOrDefault(x => x.Id == id);

                if (dbCita == null)
                    return NotFound("Cita no encontrada");

                bool cambioHorario = dbCita.FechaHoraInicio != cita.FechaHoraInicio || dbCita.FechaHoraFinal != cita.FechaHoraFinal;
                dbCita.EsVideoconsulta = !VideoconsultaOculta && cita.EsVideoconsulta; // VIDEOCONSULTA OCULTA (2026-11): ver VideoconsultaOculta

                dbCita.NumeroDocumento = cita.NumeroDocumento;
                dbCita.TipoDocumento = cita.TipoDocumento;
                dbCita.CorreoPaciente = cita.CorreoPaciente;
                dbCita.NombrePaciente = cita.NombrePaciente;
                // Si se reagenda, la confirmación anterior ya no vale (y los links del recordatorio anterior caducan)
                if (dbCita.FechaHoraInicio != cita.FechaHoraInicio)
                {
                    dbCita.EstadoConfirmacion = null;
                    dbCita.FechaRespuestaPaciente = null;
                }
                dbCita.FechaHoraFinal = cita.FechaHoraFinal;
                dbCita.FechaHoraInicio = cita.FechaHoraInicio;
                dbCita.Telefono = cita.Telefono;
                dbCita.Nota = cita.Nota;
                dbCita.Vigente = cita.Vigente;

                // Reagendada: se recrea la sala con las nuevas horas. Anulada (Vigente = "N"): se elimina.
                await SincronizarSalaVideoAsync(dbCita, recrear: cambioHorario);

                db.SaveChanges();

                return Ok(dbCita);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Cita: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}/ActualizarConCorreo")]
        public async Task<IActionResult> ActualizarConCorreo(int id, Cita cita)
        {
            try
            {
                Cita? dbCita = db.Citas
                    .Where(x => x.Id == id && x.Vigente == "S")
                    .Include(x => x.Usuario)
                    .FirstOrDefault();

                if (dbCita == null)
                    return NotFound("Cita no encontrada");

                bool cambioHorario = dbCita.FechaHoraInicio != cita.FechaHoraInicio || dbCita.FechaHoraFinal != cita.FechaHoraFinal;
                dbCita.EsVideoconsulta = !VideoconsultaOculta && cita.EsVideoconsulta; // VIDEOCONSULTA OCULTA (2026-11): ver VideoconsultaOculta

                // Actualizar campos
                dbCita.NumeroDocumento = cita.NumeroDocumento;
                dbCita.TipoDocumento = cita.TipoDocumento;
                dbCita.CorreoPaciente = cita.CorreoPaciente;
                dbCita.NombrePaciente = cita.NombrePaciente;
                // Si se reagenda, la confirmación anterior ya no vale (y los links del recordatorio anterior caducan)
                if (dbCita.FechaHoraInicio != cita.FechaHoraInicio)
                {
                    dbCita.EstadoConfirmacion = null;
                    dbCita.FechaRespuestaPaciente = null;
                }
                dbCita.FechaHoraFinal = cita.FechaHoraFinal;
                dbCita.FechaHoraInicio = cita.FechaHoraInicio;
                dbCita.Telefono = cita.Telefono;
                dbCita.Nota = cita.Nota;

                await SincronizarSalaVideoAsync(dbCita, recrear: cambioHorario);

                db.SaveChanges();

                return Ok(dbCita);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al actualizar cita con correo: {ex}");
                return StatusCode(500, ex);
            }
        }

        /// <summary>
        /// Registra la respuesta del paciente desde el link del recordatorio. accion = "confirmar" | "anular".
        /// Anular deja la cita no vigente para liberar el horario. La firma del link la valida Fimel.Site.
        /// </summary>
        [HttpPut("{id}/RespuestaPaciente")]
        public async Task<IActionResult> RespuestaPaciente(int id, [FromQuery] string accion)
        {
            try
            {
                Cita? dbCita = db.Citas
                    .Where(x => x.Id == id && x.Vigente == "S")
                    .Include(x => x.Usuario)
                    .FirstOrDefault();

                if (dbCita == null)
                    return NotFound("Cita no encontrada");

                switch (accion?.ToLowerInvariant())
                {
                    case "confirmar":
                        dbCita.EstadoConfirmacion = "Confirmada";
                        break;
                    case "anular":
                        dbCita.EstadoConfirmacion = "Anulada";
                        dbCita.Vigente = "N";
                        break;
                    default:
                        return BadRequest("Acción no válida");
                }

                dbCita.FechaRespuestaPaciente = DateTime.Now;
                await SincronizarSalaVideoAsync(dbCita, recrear: false); // anulada: elimina la sala
                db.SaveChanges();

                return Ok(dbCita);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Cita RespuestaPaciente: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("GetParaRecordatorio")]
        public IActionResult GetParaRecordatorio()
        {
            try
            {
                var manana = DateTime.Today.AddDays(1);
                var pasadoManana = DateTime.Today.AddDays(2);

                var citas = db.Citas
                    .Where(c => c.Vigente == "S"
                             && (!string.IsNullOrEmpty(c.CorreoPaciente) || !string.IsNullOrEmpty(c.Telefono))
                             && (c.FechaHoraInicio.Date == manana || c.FechaHoraInicio.Date == pasadoManana))
                    .Include(c => c.Usuario)
                    .ToList();

                var instIds = citas
                    .Where(c => c.Usuario?.IdInstitucion.HasValue == true)
                    .Select(c => c.Usuario!.IdInstitucion!.Value)
                    .Distinct()
                    .ToList();

                var instituciones = db.Instituciones.Where(i => instIds.Contains(i.Id)).ToList();

                foreach (var cita in citas)
                    if (cita.Usuario?.IdInstitucion.HasValue == true)
                        cita.Usuario.Institucion = instituciones.FirstOrDefault(i => i.Id == cita.Usuario.IdInstitucion.Value);

                return Ok(citas);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error GetParaRecordatorio Citas: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Cita cita)
        {
            try
            {
                Usuarios? dbUsuario = db.Usuarios.Find(cita.Usuario.Id);

                if (dbUsuario == null)
                    return BadRequest();

                cita.Usuario = dbUsuario;
                cita.FechaCreacion = DateTime.Now;
                cita.Vigente = "S";
                // Los datos de la sala los genera el servidor, nunca el cliente
                cita.VideoSalaNombre = null;
                cita.VideoUrlPaciente = null;
                cita.VideoUrlProfesional = null;
                cita.VideoToken = null;
                if (VideoconsultaOculta) cita.EsVideoconsulta = false; // VIDEOCONSULTA OCULTA (2026-11): ver VideoconsultaOculta

                db.Citas.Add(cita);
                db.SaveChanges();

                // La sala necesita el Id de la cita; si Daily falla la cita queda guardada igual
                if (cita.EsVideoconsulta)
                {
                    await SincronizarSalaVideoAsync(cita, recrear: false);
                    db.SaveChanges();
                }

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
