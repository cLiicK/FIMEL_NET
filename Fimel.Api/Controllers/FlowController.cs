using Fimel.Api.Services.Flow;
using Fimel.Models;
using Fimel.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fimel.Api.Controllers
{
    /// <summary>
    /// Endpoints de diagnóstico para probar la conexión con Flow. Solo responden en Development,
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
            return Ejecutar(async () => new { flow.BaseUrl, planes = await flow.ListarPlanes() });
        }

        // POST api/Flow/Planes { "planId": "FimelMensual", "nombre": "Fimel mensual", "monto": 10000, "diasPrueba": 30 }
        // Crea el plan en Flow y lo deja como el plan vigente de Fimel (las nuevas suscripciones lo usan).
        [HttpPost("Planes")]
        public Task<IActionResult> CrearPlan([FromBody] CrearPlanRequest request)
        {
            return Ejecutar(async () =>
            {
                var respuesta = await flow.CrearPlan(request.PlanId, request.Nombre, request.Monto, 3, request.DiasPrueba, request.UrlCallback);

                foreach (PlanSuscripcion otro in db.PlanesSuscripcion.Where(p => p.Vigente == "S" && p.CodigoFlow != request.PlanId))
                    otro.Vigente = "N";

                PlanSuscripcion? plan = db.PlanesSuscripcion.FirstOrDefault(p => p.CodigoFlow == request.PlanId);
                if (plan == null)
                {
                    plan = new PlanSuscripcion { CodigoFlow = request.PlanId, FechaCreacion = DateTime.Now };
                    db.PlanesSuscripcion.Add(plan);
                }

                plan.Nombre = request.Nombre;
                plan.Monto = request.Monto;
                plan.DiasPrueba = request.DiasPrueba;
                plan.Vigente = "S";
                db.SaveChanges();

                return respuesta;
            });
        }

        [HttpGet("Planes/{planId}")]
        public Task<IActionResult> ObtenerPlan(string planId)
        {
            return Ejecutar(async () => await flow.ObtenerPlan(planId));
        }

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
            public string Nombre { get; set; } = "Fimel mensual";
            public int Monto { get; set; }
            public int DiasPrueba { get; set; } = 30;
            public string? UrlCallback { get; set; }
        }
    }
}
