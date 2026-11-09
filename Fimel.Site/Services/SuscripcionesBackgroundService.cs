using Fimel.Utils;
using Newtonsoft.Json.Linq;

namespace Fimel.Site.Services
{
    /// <summary>
    /// Sincroniza a diario con Flow el estado de todas las suscripciones (cobros, morosidad, cancelaciones),
    /// por si alguna notificación de Flow no llegó.
    /// </summary>
    public class SuscripcionesBackgroundService : BackgroundService
    {
        private readonly ILogger<SuscripcionesBackgroundService> _logger;
        private readonly IConfiguration _config;

        private const int HoraEjecucion = 6;

        public SuscripcionesBackgroundService(ILogger<SuscripcionesBackgroundService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var ahora = DateTime.Now;
                var proximaEjecucion = ahora.Date.AddHours(HoraEjecucion);

                if (ahora >= proximaEjecucion)
                    proximaEjecucion = proximaEjecucion.AddDays(1);

                var demora = proximaEjecucion - ahora;
                _logger.LogInformation("Sincronización de suscripciones esperará {Minutos} minutos hasta la próxima ejecución.", (int)demora.TotalMinutes);

                await Task.Delay(demora, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                // Con el cobro con Flow apagado no se consulta a Flow; se revisa en cada ciclo por si se habilita
                if (_config.GetValue<bool>("Suscripcion:Habilitada"))
                    Sincronizar();
                else
                    _logger.LogInformation("Suscripciones deshabilitadas (Suscripcion:Habilitada = false): no se sincroniza con Flow.");
            }
        }

        private void Sincronizar()
        {
            try
            {
                JObject? resultado = new APIClient(_config["API_URL"]).Post<JObject>("Suscripciones/SincronizarTodas", new JObject());
                _logger.LogInformation("Sincronización de suscripciones con Flow: {Resultado}", resultado?.ToString(Newtonsoft.Json.Formatting.None));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar suscripciones con Flow.");
            }
        }
    }
}
