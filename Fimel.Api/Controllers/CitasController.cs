using Fimel.Models;
using Fimel.Models.Params;
using Fimel.Utils;
using Fimel.Models.Integraciones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private FimelDbContext db;
        public CitasController(FimelDbContext context)
        {
            db = context;
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

                if(q.UsuarioId.HasValue)
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
            catch(Exception ex)
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

                // Enviar correo de confirmación al paciente
                EnviarCorreoConfirmacionCita(cita, dbUsuario);

                return Ok(cita);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Cita Post: {ex}");
                return StatusCode(500, ex);
            }
        }

        private void EnviarCorreoConfirmacionCita(Cita cita, Usuarios profesional)
        {
            try
            {
                // Obtener información de la institución si está disponible
                string direccionInstitucion = "Dirección no disponible";
                if (profesional.IdInstitucion.HasValue)
                {
                    var institucion = db.Instituciones.FirstOrDefault(x => x.Id == profesional.IdInstitucion.Value);
                    if (institucion != null && !string.IsNullOrEmpty(institucion.Dirección))
                    {
                        direccionInstitucion = institucion.Dirección;
                    }
                }

                // Generar el contenido del correo HTML
                string contenidoCorreo = GenerarContenidoCorreoConfirmacion(cita, profesional, direccionInstitucion);

                // Crear el objeto de correo
                var correo = new EnvioCorreo
                {
                    Destinatarios = new List<string> { cita.CorreoPaciente },
                    Asunto = $"Confirmación de Cita - {cita.FechaHoraInicio.ToString("dd/MM/yyyy")}",
                    CuerpoCorreo = contenidoCorreo
                };

                // Enviar el correo usando la utilería existente
                Utileria utileria = new Utileria();
                utileria.EnviarCorreo(correo);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al enviar correo de confirmación de cita: {ex}");
                // No lanzar la excepción para no afectar la creación de la cita
            }
        }

        private string GenerarContenidoCorreoConfirmacion(Cita cita, Usuarios profesional, string direccionInstitucion)
        {
            try
            {
                // Obtener el nombre completo del profesional
                string nombreProfesional = $"{profesional.Nombres} {profesional.ApellidoPaterno} {profesional.ApellidoMaterno}".Trim();

                // Formatear fecha y hora
                string fechaCita = cita.FechaHoraInicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
                string horaCita = cita.FechaHoraInicio.ToString("HH:mm");

                // Generar el HTML del correo directamente
                string contenido = $@"
<!doctype html>
<html lang=""es"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
  <title>Fimel - Cita</title>
  <style>
    @media only screen and (max-width:620px){{
      .container{{width:100% !important; padding:0 16px !important;}}
      .btn{{display:block !important; width:100% !important; margin:8px 0 !important;}}
      .btn-wrap{{display:block !important;}}
    }}
  </style>
</head>
<body style=""margin:0; padding:0; background:#f7f7f9;"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
    <tr>
      <td align=""center"" style=""padding:24px 10px;"">
        <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" width=""600"" class=""container"" style=""width:600px; background:#ffffff; border-radius:12px; overflow:hidden; border:1px solid #eef0f3;"">
          
          <!-- Header blanco -->
          <tr>
            <td style=""padding-top:22px; background:#ffffff;"">
              <table role=""presentation"" width=""100%"">
                <tr>
                  <td align=""center"">
                    <img src=""http://testing.fimel.cl/img/logo1.svg"" width=""180"" alt=""FIMEL"">
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <!-- Línea coral -->
          <tr>
            <td style=""height:6px; background:#f78a8a;""></td>
          </tr>

          <!-- Contenido -->
          <tr>
            <td style=""padding:26px; font-family:Arial,Helvetica,sans-serif; color:#333;"">
              <h1 style=""margin:0 0 10px 0; color:#0d86b3;"">Cita Agendada</h1>

              <!-- Datos de la cita -->
              <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""font-size:14px; margin:6px 0 12px 0;"">
                <tr><td style=""padding:4px 0;""><strong>Paciente:</strong> {cita.NombrePaciente}</td></tr>
                <tr><td style=""padding:4px 0;""><strong>Profesional:</strong> {nombreProfesional}</td></tr>
                <tr><td style=""padding:4px 0;""><strong>Fecha:</strong> {fechaCita}</td></tr>
                <tr><td style=""padding:4px 0;""><strong>Hora:</strong> {horaCita}</td></tr>
                <tr><td style=""padding:4px 0;""><strong>Ubicación:</strong> {direccionInstitucion}</td></tr>
              </table>
            </td>
          </tr>

          <tr><td style=""height:1px; background:#eef0f3;""></td></tr>

          <!-- Footer -->
          <tr>
            <td style=""padding:18px 26px 24px 26px; font-family:Arial,Helvetica,sans-serif; font-size:12px; color:#7a8794;"">
              <table role=""presentation"" width=""100%"">
                <tr>
                  <td valign=""top"">
                    <strong style=""color:#0d86b3;"">FIMEL S.A.</strong><br>
                    Web: <a href=""https://fimel.cl/landing"" style=""color:#0d86b3; text-decoration:none;"">fimel.cl</a>
                  </td>
                </tr>
                <tr>
                  <td colspan=""2"" style=""padding-top:10px; color:#9aa1a7;"">
                    Este correo fue enviado automáticamente por FIMEL. No respondas a este mensaje.
                  </td>
                </tr>
              </table>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

                return contenido;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al generar contenido del correo: {ex}");
                // Retornar un mensaje simple en caso de error
                return $"Su cita ha sido confirmada para el {cita.FechaHoraInicio.ToString("dd/MM/yyyy")} a las {cita.FechaHoraInicio.ToString("HH:mm")} con {profesional.Nombres} {profesional.ApellidoPaterno}.";
            }
        }
    }
}
