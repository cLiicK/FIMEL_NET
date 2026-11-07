using System.Globalization;
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

        // Planes que se ofrecen a nuevos usuarios, del más corto al más largo
        [HttpGet("Planes")]
        public IActionResult Planes()
        {
            try
            {
                return Ok(db.PlanesSuscripcion.Where(p => p.Vigente == "S").OrderBy(p => p.Meses).ToList());
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al obtener planes de suscripción: {ex}");
                return StatusCode(500, ex.Message);
            }
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

                PlanSuscripcion? plan = db.PlanesSuscripcion.FirstOrDefault(p => p.Id == solicitud.IdPlan && p.Vigente == "S");
                if (plan == null)
                    return Ok(Fallo(solicitud, "Selecciona un plan disponible."));

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
                suscripcion.PruebaUsada = true;

                // subscription/create devuelve la suscripción con el mismo formato que subscription/get
                AplicarEstadoFlow(suscripcion, creada);

                if (suscripcion.Estado == EstadosSuscripcion.Pendiente)
                {
                    // Respaldo si Flow no informó el estado: con días de prueba queda en Prueba, sin ellos Activa
                    suscripcion.Estado = diasPrueba > 0 ? EstadosSuscripcion.Prueba : EstadosSuscripcion.Activa;
                    suscripcion.FinPrueba ??= diasPrueba > 0 ? DateTime.Today.AddDays(diasPrueba) : null;
                    suscripcion.PagadoHasta ??= suscripcion.FinPrueba;
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

        // Trae de Flow el estado actual de la suscripción del usuario (estado, período pagado, cobros) y lo guarda.
        [HttpPost("Sincronizar/{idUsuario}")]
        public async Task<IActionResult> Sincronizar(int idUsuario)
        {
            try
            {
                SuscripcionUsuario? suscripcion = UltimaSuscripcion(idUsuario);

                if (suscripcion != null && !string.IsNullOrWhiteSpace(suscripcion.FlowSubscriptionId))
                {
                    await SincronizarConFlow(suscripcion);
                    db.SaveChanges();
                }

                if (suscripcion != null)
                    suscripcion.Plan = db.PlanesSuscripcion.Find(suscripcion.IdPlan);

                return Ok(suscripcion);
            }
            catch (Exception ex) when (ex is FlowException or HttpRequestException)
            {
                // Si Flow no responde se devuelve lo último guardado
                Logger.Log($"No se pudo sincronizar con Flow la suscripción del usuario {idUsuario}: {ex.Message}");
                SuscripcionUsuario? guardada = UltimaSuscripcion(idUsuario);
                if (guardada != null)
                    guardada.Plan = db.PlanesSuscripcion.Find(guardada.IdPlan);
                return Ok(guardada);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al sincronizar suscripción del usuario {idUsuario}: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        // Sincroniza todas las suscripciones vivas. Lo usan el servicio diario del Site y las notificaciones de Flow.
        [HttpPost("SincronizarTodas")]
        public async Task<IActionResult> SincronizarTodas()
        {
            int sincronizadas = 0, errores = 0;

            List<SuscripcionUsuario> suscripciones = db.SuscripcionesUsuario
                .Where(s => s.Vigente == "S" && s.FlowSubscriptionId != null
                    && s.Estado != EstadosSuscripcion.Cancelada && s.Estado != EstadosSuscripcion.Exenta)
                .ToList();

            foreach (SuscripcionUsuario suscripcion in suscripciones)
            {
                try
                {
                    await SincronizarConFlow(suscripcion);
                    db.SaveChanges();
                    sincronizadas++;
                }
                catch (Exception ex)
                {
                    errores++;
                    Logger.Log($"Error al sincronizar la suscripción {suscripcion.FlowSubscriptionId}: {ex.Message}");
                }
            }

            return Ok(new { sincronizadas, errores });
        }

        // Flow avisa aquí (vía el Site) cada cobro de un plan, con un token. No se confía en el aviso:
        // se registra el estado del pago para auditoría y se vuelve a consultar a Flow el estado de las suscripciones.
        [HttpPost("NotificacionFlow")]
        public async Task<IActionResult> NotificacionFlow([FromBody] SolicitudSuscripcion solicitud)
        {
            if (!string.IsNullOrWhiteSpace(solicitud.Token))
            {
                try
                {
                    JsonElement pago = await flow.EstadoPago(solicitud.Token);
                    Logger.Log($"Notificación de Flow recibida: {pago}");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Notificación de Flow con token no consultable: {ex.Message}");
                }
            }

            return await SincronizarTodas();
        }

        // Cancela al final del período pagado: el usuario mantiene el acceso hasta PagadoHasta.
        [HttpPost("Cancelar/{idUsuario}")]
        public async Task<IActionResult> Cancelar(int idUsuario)
        {
            SolicitudSuscripcion respuesta = new SolicitudSuscripcion { IdUsuario = idUsuario };

            try
            {
                SuscripcionUsuario? suscripcion = UltimaSuscripcion(idUsuario);
                if (suscripcion == null || string.IsNullOrWhiteSpace(suscripcion.FlowSubscriptionId))
                    return Ok(Fallo(respuesta, "No tienes una suscripción para cancelar."));

                JsonElement cancelada = await flow.CancelarSuscripcion(suscripcion.FlowSubscriptionId, alFinalDelPeriodo: true);
                AplicarEstadoFlow(suscripcion, cancelada);
                suscripcion.CancelaAlFinPeriodo = true;
                suscripcion.UltimaSincronizacion = DateTime.Now;
                db.SaveChanges();

                respuesta.Exito = true;
                respuesta.Suscripcion = suscripcion;
                return Ok(respuesta);
            }
            catch (FlowException ex)
            {
                Logger.Log($"Error Flow al cancelar suscripción del usuario {idUsuario} ({ex.StatusHttp} {ex.Codigo}): {ex.Message}");
                return Ok(Fallo(respuesta, "No pudimos cancelar la suscripción en Flow. Intenta nuevamente en unos minutos."));
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al cancelar suscripción del usuario {idUsuario}: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        // Deja sin efecto una cancelación: quien canceló y aún tiene período pagado vuelve a suscribirse con la tarjeta
        // ya inscrita. Se crea en Flow una suscripción nueva que parte al terminar el período pagado (sin prueba),
        // así el primer cobro llega cuando se acaba lo ya pagado y no se cobra dos veces.
        [HttpPost("Reactivar")]
        public async Task<IActionResult> Reactivar([FromBody] SolicitudSuscripcion solicitud)
        {
            try
            {
                SuscripcionUsuario? anterior = UltimaSuscripcion(solicitud.IdUsuario);
                if (anterior == null || !anterior.CancelaAlFinPeriodo || anterior.PagadoHasta == null || anterior.PagadoHasta.Value.Date < DateTime.Today)
                    return Ok(Fallo(solicitud, "No tienes una suscripción cancelada con período vigente para reactivar."));

                if (string.IsNullOrWhiteSpace(anterior.FlowCustomerId))
                    return Ok(Fallo(solicitud, "Tu suscripción anterior no tiene una tarjeta inscrita en Flow."));

                PlanSuscripcion? plan = db.PlanesSuscripcion.FirstOrDefault(p => p.Id == solicitud.IdPlan && p.Vigente == "S");
                if (plan == null)
                    return Ok(Fallo(solicitud, "Selecciona un plan disponible."));

                DateTime inicio = anterior.PagadoHasta.Value.Date;
                JsonElement creada = await flow.CrearSuscripcion(plan.CodigoFlow, anterior.FlowCustomerId, diasPrueba: 0, inicio: inicio);

                SuscripcionUsuario suscripcion = new SuscripcionUsuario
                {
                    IdUsuario = anterior.IdUsuario,
                    IdPlan = plan.Id,
                    FlowCustomerId = anterior.FlowCustomerId,
                    FlowSubscriptionId = FlowClient.Texto(creada, "subscriptionId"),
                    TipoTarjeta = anterior.TipoTarjeta,
                    Ultimos4 = anterior.Ultimos4,
                    FechaInicio = DateTime.Now,
                    PagadoHasta = anterior.PagadoHasta,
                    PruebaUsada = true,
                    Estado = EstadosSuscripcion.Activa,
                    UltimaSincronizacion = DateTime.Now,
                    Vigente = "S",
                    FechaCreacion = DateTime.Now
                };

                // Se guarda primero para que los cobros que informe Flow queden asociados a su Id
                db.SuscripcionesUsuario.Add(suscripcion);
                db.SaveChanges();

                // Una suscripción que aún no comienza puede venir con un status que no es "activa":
                // solo se acepta lo que Flow informe si es Activa o Morosa, para no quitar el acceso ya pagado.
                AplicarEstadoFlow(suscripcion, creada);
                if (suscripcion.Estado != EstadosSuscripcion.Activa && suscripcion.Estado != EstadosSuscripcion.Morosa)
                    suscripcion.Estado = EstadosSuscripcion.Activa;
                suscripcion.FinPrueba = null;
                db.SaveChanges();

                suscripcion.Plan = plan;
                solicitud.Exito = true;
                solicitud.Suscripcion = suscripcion;
                return Ok(solicitud);
            }
            catch (FlowException ex)
            {
                Logger.Log($"Error Flow al reactivar suscripción del usuario {solicitud.IdUsuario} ({ex.StatusHttp} {ex.Codigo}): {ex.Message}");
                return Ok(Fallo(solicitud, "No pudimos reactivar la suscripción en Flow. Intenta nuevamente en unos minutos."));
            }
            catch (Exception ex)
            {
                Logger.Log($"Error al reactivar suscripción del usuario {solicitud.IdUsuario}: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        private async Task SincronizarConFlow(SuscripcionUsuario suscripcion)
        {
            JsonElement flowSuscripcion = await flow.ObtenerSuscripcion(suscripcion.FlowSubscriptionId!);
            AplicarEstadoFlow(suscripcion, flowSuscripcion);
            suscripcion.UltimaSincronizacion = DateTime.Now;
        }

        /// <summary>
        /// Traduce el estado de una suscripción de Flow (subscription/get) al de Fimel y registra sus cobros.
        /// status de Flow: 1 activa, 2 en prueba, 4 cancelada (2 confirmado en sandbox). morose = 1: tiene cobros vencidos.
        /// </summary>
        private void AplicarEstadoFlow(SuscripcionUsuario suscripcion, JsonElement flowSuscripcion)
        {
            string? status = FlowClient.Texto(flowSuscripcion, "status");
            bool morosa = FlowClient.Texto(flowSuscripcion, "morose") == "1";

            suscripcion.Estado = status switch
            {
                "2" => EstadosSuscripcion.Prueba,
                "1" => morosa ? EstadosSuscripcion.Morosa : EstadosSuscripcion.Activa,
                "4" => EstadosSuscripcion.Cancelada,
                _ => suscripcion.Estado
            };

            suscripcion.FinPrueba = FlowClient.Fecha(flowSuscripcion, "trial_end") ?? suscripcion.FinPrueba;
            suscripcion.CancelaAlFinPeriodo = FlowClient.Texto(flowSuscripcion, "cancel_at_period_end") == "1";

            if (flowSuscripcion.ValueKind != JsonValueKind.Object
                || !flowSuscripcion.TryGetProperty("invoices", out JsonElement invoices)
                || invoices.ValueKind != JsonValueKind.Array)
                return;

            foreach (JsonElement invoice in invoices.EnumerateArray())
            {
                if (!long.TryParse(FlowClient.Texto(invoice, "id"), out long idInvoice))
                    continue;

                PagoSuscripcion? pago = db.PagosSuscripcion.FirstOrDefault(p => p.FlowInvoiceId == idInvoice);
                if (pago == null)
                {
                    pago = new PagoSuscripcion { FlowInvoiceId = idInvoice, IdSuscripcion = suscripcion.Id, Vigente = "S", FechaCreacion = DateTime.Now };
                    db.PagosSuscripcion.Add(pago);
                }

                decimal.TryParse(FlowClient.Texto(invoice, "amount"), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal monto);
                int.TryParse(FlowClient.Texto(invoice, "status"), out int estadoInvoice);

                pago.Descripcion = FlowClient.Texto(invoice, "subject");
                pago.Monto = (int)monto;
                pago.EstadoFlow = estadoInvoice;
                pago.PeriodoDesde = FlowClient.Fecha(invoice, "period_start");
                pago.PeriodoHasta = FlowClient.Fecha(invoice, "period_end");
                pago.RespuestaJson = invoice.GetRawText();

                if (estadoInvoice == 1)
                {
                    pago.FechaPago ??= DateTime.Now;

                    // El acceso llega hasta el fin del último período pagado (incluye el período de prueba, que Flow factura en $0)
                    if (pago.PeriodoHasta.HasValue && (suscripcion.PagadoHasta == null || pago.PeriodoHasta > suscripcion.PagadoHasta))
                        suscripcion.PagadoHasta = pago.PeriodoHasta;
                }
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
