using System.Text.Json;
using Fimel.Api.Services.Flow;
using Fimel.Models;
using Fimel.Models.Integraciones;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    /// <summary>
    /// Suscripción mensual de cada usuario profesional, cobrada con tarjeta vía Flow.
    /// Flujo: Iniciar (cliente Flow + inscripción de tarjeta) → el usuario inscribe su tarjeta en Flow →
    /// ConfirmarTarjeta (verifica con Flow y crea la suscripción con los días de prueba del plan).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SuscripcionesController : ControllerBase
    {
        private readonly FimelDbContext db;
        private readonly FlowClient flow;

        public SuscripcionesController(FimelDbContext context, FlowClient flow)
        {
            db = context;
            this.flow = flow;
        }

        [HttpGet("GetByUsuario/{idUsuario}")]
        public IActionResult GetByUsuario(int idUsuario)
        {
            try
            {
                SuscripcionUsuario? suscripcion = UltimaSuscripcion(idUsuario);

                if (suscripcion != null)
                    suscripcion.Plan = db.PlanesSuscripcion.Find(suscripcion.IdPlan);

                return Ok(suscripcion);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener suscripción del usuario {idUsuario}: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Iniciar")]
        public async Task<IActionResult> Iniciar([FromBody] SolicitudSuscripcion solicitud)
        {
            try
            {
                Usuarios? usuario = db.Usuarios.Find(solicitud.IdUsuario);
                if (usuario == null)
                    return Ok(Fallo(solicitud, "Usuario no encontrado."));

                if (string.IsNullOrWhiteSpace(usuario.Email))
                    return Ok(Fallo(solicitud, "Tu usuario no tiene un email registrado; Flow lo necesita para la suscripción."));

                if (string.IsNullOrWhiteSpace(solicitud.UrlRetorno))
                    return Ok(Fallo(solicitud, "Falta la URL de retorno."));

                PlanSuscripcion? plan = db.PlanesSuscripcion.Where(p => p.Vigente == "S").OrderByDescending(p => p.Id).FirstOrDefault();
                if (plan == null)
                    return Ok(Fallo(solicitud, "No hay un plan de suscripción configurado."));

                SuscripcionUsuario? anterior = UltimaSuscripcion(usuario.Id);
                if (anterior != null && (anterior.Estado == EstadosSuscripcion.Prueba || anterior.Estado == EstadosSuscripcion.Activa || anterior.Estado == EstadosSuscripcion.Exenta))
                    return Ok(Fallo(solicitud, "Ya tienes una suscripción activa."));

                SuscripcionUsuario suscripcion;
                if (anterior != null && anterior.Estado == EstadosSuscripcion.Pendiente)
                {
                    // Reintento de inscripción de tarjeta: se reutiliza el registro pendiente
                    suscripcion = anterior;
                    suscripcion.IdPlan = plan.Id;
                }
                else
                {
                    suscripcion = new SuscripcionUsuario
                    {
                        IdUsuario = usuario.Id,
                        IdPlan = plan.Id,
                        Estado = EstadosSuscripcion.Pendiente,
                        FlowCustomerId = anterior?.FlowCustomerId,
                        PruebaUsada = db.SuscripcionesUsuario.Any(s => s.IdUsuario == usuario.Id && s.PruebaUsada),
                        Vigente = "S",
                        FechaCreacion = DateTime.Now
                    };
                    db.SuscripcionesUsuario.Add(suscripcion);
                }

                if (string.IsNullOrWhiteSpace(suscripcion.FlowCustomerId))
                {
                    string nombre = $"{usuario.Nombres} {usuario.ApellidoPaterno}".Trim();
                    JsonElement cliente = await flow.CrearCliente(string.IsNullOrWhiteSpace(nombre) ? usuario.Email : nombre, usuario.Email, $"fimel-usuario-{usuario.Id}");
                    suscripcion.FlowCustomerId = FlowClient.Texto(cliente, "customerId");
                }

                db.SaveChanges();

                JsonElement registro = await flow.RegistrarTarjeta(suscripcion.FlowCustomerId!, solicitud.UrlRetorno);
                string? url = FlowClient.Texto(registro, "url");
                string? token = FlowClient.Texto(registro, "token");

                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(token))
                    return Ok(Fallo(solicitud, "Flow no devolvió la URL de inscripción de tarjeta."));

                solicitud.Exito = true;
                solicitud.UrlFlow = $"{url}?token={Uri.EscapeDataString(token)}";
                solicitud.Suscripcion = suscripcion;
                return Ok(solicitud);
            }
            catch (FlowException ex)
            {
                Logger.Log($"Error Flow al iniciar suscripción del usuario {solicitud.IdUsuario} ({ex.StatusHttp} {ex.Codigo}): {ex.Message}");
                return Ok(Fallo(solicitud, "No pudimos comunicarnos con Flow. Intenta nuevamente en unos minutos."));
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al iniciar suscripción del usuario {solicitud.IdUsuario}: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("ConfirmarTarjeta")]
        public async Task<IActionResult> ConfirmarTarjeta([FromBody] SolicitudSuscripcion solicitud)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(solicitud.Token))
                    return Ok(Fallo(solicitud, "Falta el token de Flow."));

                // No se confía en lo que llega del navegador: se consulta el estado real a Flow
                JsonElement estado = await flow.EstadoRegistroTarjeta(solicitud.Token);
                string? customerId = FlowClient.Texto(estado, "customerId");

                SuscripcionUsuario? suscripcion = db.SuscripcionesUsuario
                    .Where(s => s.FlowCustomerId == customerId && s.Vigente == "S")
                    .OrderByDescending(s => s.Id)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(customerId) || suscripcion == null)
                    return Ok(Fallo(solicitud, "No encontramos la suscripción asociada a esta tarjeta."));

                solicitud.IdUsuario = suscripcion.IdUsuario;
                solicitud.Suscripcion = suscripcion;

                if (FlowClient.Texto(estado, "status") != "1")
                    return Ok(Fallo(solicitud, "La tarjeta no quedó inscrita. Puedes intentarlo de nuevo."));

                // Idempotente: si el usuario recarga la página de retorno no se crea otra suscripción
                if (!string.IsNullOrWhiteSpace(suscripcion.FlowSubscriptionId))
                {
                    solicitud.Exito = true;
                    return Ok(solicitud);
                }

                PlanSuscripcion plan = db.PlanesSuscripcion.Find(suscripcion.IdPlan)!;
                int diasPrueba = suscripcion.PruebaUsada ? 0 : plan.DiasPrueba;

                JsonElement creada = await flow.CrearSuscripcion(plan.CodigoFlow, customerId, suscripcion.PruebaUsada ? 0 : null);

                suscripcion.FlowSubscriptionId = FlowClient.Texto(creada, "subscriptionId");
                suscripcion.TipoTarjeta = FlowClient.Texto(estado, "creditCardType");
                suscripcion.Ultimos4 = FlowClient.Texto(estado, "last4CardDigits");
                suscripcion.FechaInicio = DateTime.Now;

                if (diasPrueba > 0)
                {
                    DateTime finPrueba = DateTime.TryParse(FlowClient.Texto(creada, "trial_end"), out DateTime fin) ? fin : DateTime.Today.AddDays(diasPrueba);
                    suscripcion.Estado = EstadosSuscripcion.Prueba;
                    suscripcion.FinPrueba = finPrueba;
                    suscripcion.PagadoHasta = finPrueba;
                    suscripcion.PruebaUsada = true;
                }
                else
                {
                    // Sin prueba Flow cobra el primer período de inmediato; la notificación de pago actualiza PagadoHasta
                    suscripcion.Estado = EstadosSuscripcion.Activa;
                }

                db.SaveChanges();

                solicitud.Exito = true;
                solicitud.Suscripcion.Plan = plan;
                return Ok(solicitud);
            }
            catch (FlowException ex)
            {
                Logger.Log($"Error Flow al confirmar tarjeta ({ex.StatusHttp} {ex.Codigo}): {ex.Message}");
                return Ok(Fallo(solicitud, "No pudimos confirmar la tarjeta con Flow. Intenta nuevamente en unos minutos."));
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al confirmar tarjeta: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        private SuscripcionUsuario? UltimaSuscripcion(int idUsuario)
        {
            return db.SuscripcionesUsuario
                .Where(s => s.IdUsuario == idUsuario && s.Vigente == "S")
                .OrderByDescending(s => s.Id)
                .FirstOrDefault();
        }

        private static SolicitudSuscripcion Fallo(SolicitudSuscripcion solicitud, string mensaje)
        {
            solicitud.Exito = false;
            solicitud.Mensaje = mensaje;
            return solicitud;
        }
    }
}
