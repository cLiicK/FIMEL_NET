using Fimel.Api.Services.Flow;
using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    /// <summary>
    /// Endpoints de diagnóstico y administración de planes en Flow. Solo responden en Development,
    /// porque la API aún no tiene autenticación.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FlowController : ControllerBase
    {
        private readonly FlowClient flow;
        private readonly IWebHostEnvironment env;
        private readonly FimelDbContext db;

        public FlowController(FlowClient flow, IWebHostEnvironment env, FimelDbContext db)
        {
            this.flow = flow;
            this.env = env;
            this.db = db;
        }

        // GET api/Flow/Ping → lista los planes de la cuenta; si responde, la firma y las llaves están bien.
        [HttpGet("Ping")]
        public Task<IActionResult> Ping()
        {
            return Ejecutar(async () => new { flow.BaseUrl, planes = await flow.ListarPlanes(0, 50) });
        }

        // POST api/Flow/Planes { "planId": "FimelMensual", "nombre": "Fimel mensual", "monto": 11990, "meses": 1, "diasPrueba": 30 }
        // Crea el plan en Flow y lo deja vigente en Fimel.
        [HttpPost("Planes")]
        public Task<IActionResult> CrearPlan([FromBody] CrearPlanRequest request)
        {
            return Ejecutar(async () => await Crear(request));
        }

        // POST api/Flow/Planes/Predeterminados → crea los planes de lista de una vez.
        // El body es opcional: si se envía una lista de planes, se usan esos en vez de los predeterminados.
        [HttpPost("Planes/Predeterminados")]
        public Task<IActionResult> CrearPlanesPredeterminados([FromBody] List<CrearPlanRequest>? planes = null)
        {
            return Ejecutar(async () =>
            {
                List<object> resultado = new List<object>();
                foreach (CrearPlanRequest plan in planes is { Count: > 0 } ? planes : PlanesPredeterminados())
                    resultado.Add(await Crear(plan));
                return resultado;
            });
        }

        [HttpGet("Planes/{planId}")]
        public Task<IActionResult> ObtenerPlan(string planId)
        {
            return Ejecutar(async () => await flow.ObtenerPlan(planId));
        }

        // GET api/Flow/Suscripciones/{subscriptionId} → respuesta cruda de Flow, para ver estados, períodos y cobros
        [HttpGet("Suscripciones/{subscriptionId}")]
        public Task<IActionResult> ObtenerSuscripcion(string subscriptionId)
        {
            return Ejecutar(async () => await flow.ObtenerSuscripcion(subscriptionId));
        }

        // DELETE api/Flow/Planes/{planId} → deja de ofrecer el plan a nuevos usuarios (las suscripciones existentes siguen en Flow).
        [HttpDelete("Planes/{planId}")]
        public Task<IActionResult> RetirarPlan(string planId)
        {
            return Ejecutar(() =>
            {
                PlanSuscripcion? plan = db.PlanesSuscripcion.FirstOrDefault(p => p.CodigoFlow == planId);
                if (plan == null)
                    return Task.FromResult<object>(new { mensaje = "Plan no encontrado en Fimel." });

                plan.Vigente = "N";
                db.SaveChanges();
                return Task.FromResult<object>(plan);
            });
        }

        private async Task<object> Crear(CrearPlanRequest request)
        {
            var respuesta = await flow.CrearPlan(request.PlanId, request.Nombre, request.Monto, 3, request.Meses, request.DiasPrueba, request.UrlCallback);

            PlanSuscripcion? plan = db.PlanesSuscripcion.FirstOrDefault(p => p.CodigoFlow == request.PlanId);
            if (plan == null)
            {
                plan = new PlanSuscripcion { CodigoFlow = request.PlanId, FechaCreacion = DateTime.Now };
                db.PlanesSuscripcion.Add(plan);
            }

            plan.Nombre = request.Nombre;
            plan.Monto = request.Monto;
            plan.Meses = request.Meses;
            plan.DiasPrueba = request.DiasPrueba;
            plan.Vigente = "S";
            db.SaveChanges();

            return new { plan, flow = respuesta };
        }

        // Precios de lista. Flow no permite cambiar el monto de un plan con suscriptores:
        // para cambiar un precio se crea un plan con otro planId y se retira el anterior.
        private static List<CrearPlanRequest> PlanesPredeterminados() => new()
        {
            new CrearPlanRequest { PlanId = "FimelMensual",    Nombre = "Mensual",    Meses = 1,  Monto = 11990 },
            new CrearPlanRequest { PlanId = "FimelTrimestral", Nombre = "Trimestral", Meses = 3,  Monto = 34190 },
            new CrearPlanRequest { PlanId = "FimelSemestral",  Nombre = "Semestral",  Meses = 6,  Monto = 64690 },
            new CrearPlanRequest { PlanId = "FimelAnual",      Nombre = "Anual",      Meses = 12, Monto = 114990 },
        };

        private async Task<IActionResult> Ejecutar(Func<Task<object>> accion)
        {
            if (!env.IsDevelopment())
                return NotFound();

            if (!flow.Configurado)
                return StatusCode(503, "Faltan las credenciales de Flow (Flow:ApiKey y Flow:SecretKey).");

            try
            {
                return Ok(await accion());
            }
            catch (FlowException ex)
            {
                Logger.Log($"Error Flow ({ex.StatusHttp} {ex.Codigo}): {ex.Message}");
                return StatusCode(502, new { ex.StatusHttp, ex.Codigo, ex.Message });
            }
            catch (HttpRequestException ex)
            {
                Logger.Log($"Sin conexión con Flow ({flow.BaseUrl}): {ex}");
                return StatusCode(502, new { Message = $"No se pudo conectar con Flow ({flow.BaseUrl}): {ex.Message}" });
            }
        }

        public class CrearPlanRequest
        {
            public string PlanId { get; set; } = "FimelMensual";
            public string Nombre { get; set; } = "Mensual";
            public int Monto { get; set; }
            public int Meses { get; set; } = 1;
            public int DiasPrueba { get; set; } = 30;
            public string? UrlCallback { get; set; }
        }
    }
}
