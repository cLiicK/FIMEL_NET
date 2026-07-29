using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fimel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DictadoController : ControllerBase
    {
        private FimelDbContext db;
        public DictadoController(FimelDbContext context) { db = context; }

        [HttpPost("Iniciar")]
        public IActionResult Iniciar([FromBody] int usuarioId)
        {
            try
            {
                // Cerrar sesiones previas activas del usuario
                var sesionesViejas = db.DictadoSesiones
                    .Where(s => s.UsuarioId == usuarioId && s.Activa)
                    .ToList();
                foreach (var s in sesionesViejas) s.Activa = false;

                var sesion = new DictadoSesion
                {
                    Token = Guid.NewGuid().ToString("N"),
                    UsuarioId = usuarioId,
                    TextosPendientes = "[]",
                    Expiracion = DateTime.Now.AddHours(4),
                    Activa = true,
                    FechaCreacion = DateTime.Now
                };

                db.DictadoSesiones.Add(sesion);
                db.SaveChanges();

                return Ok(new { token = sesion.Token, expiracion = sesion.Expiracion });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Dictado Iniciar: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost("{token}/Texto")]
        public IActionResult RecibirTexto(string token, [FromBody] string texto)
        {
            try
            {
                DictadoSesion? sesion = db.DictadoSesiones
                    .Where(s => s.Token == token && s.Activa && s.Expiracion > DateTime.Now)
                    .FirstOrDefault();

                if (sesion == null) return NotFound("Sesión no válida o expirada.");

                var textos = JsonConvert.DeserializeObject<List<string>>(sesion.TextosPendientes ?? "[]") ?? new List<string>();
                textos.Add(texto);
                sesion.TextosPendientes = JsonConvert.SerializeObject(textos);

                db.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Dictado RecibirTexto: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpGet("{token}/Pendiente")]
        public IActionResult ObtenerPendientes(string token)
        {
            try
            {
                DictadoSesion? sesion = db.DictadoSesiones
                    .Where(s => s.Token == token && s.Activa && s.Expiracion > DateTime.Now)
                    .FirstOrDefault();

                if (sesion == null) return Ok(new { activa = false, textos = new List<string>() });

                var textos = JsonConvert.DeserializeObject<List<string>>(sesion.TextosPendientes ?? "[]") ?? new List<string>();

                // Limpiar pendientes tras entregar
                sesion.TextosPendientes = "[]";
                db.SaveChanges();

                return Ok(new { activa = true, textos });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Dictado ObtenerPendientes: {ex}");
                return StatusCode(500, ex);
            }
        }

        [HttpPost("{token}/Cerrar")]
        public IActionResult Cerrar(string token)
        {
            try
            {
                DictadoSesion? sesion = db.DictadoSesiones
                    .Where(s => s.Token == token)
                    .FirstOrDefault();

                if (sesion != null)
                {
                    sesion.Activa = false;
                    db.SaveChanges();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Dictado Cerrar: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
