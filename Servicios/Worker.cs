using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using PLCServicio.Configuracion;
using PLCServicio.Eventos;
using PLCServicio.Utilidades;

namespace PLCServicio.Servicios
{
    /// <summary>
    /// Worker Service principal que reemplaza al ServiceBase de .NET Framework
    /// </summary>
    public class WorkerPLC : BackgroundService
    {
        private readonly ILogger<WorkerPLC> _loggerMs;
        private readonly Logger _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private GestorLecturas _gestorLecturas;

        public WorkerPLC(
            ILogger<WorkerPLC> loggerMs, 
            Logger logger,
            IHostApplicationLifetime lifetime)
        {
            _loggerMs = loggerMs;
            _logger = logger;
            _lifetime = lifetime;
        }

        /// <summary>
        /// Se ejecuta cuando el servicio inicia
        /// </summary>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Informacion("=== SERVICIO PLC INICIANDO ===");
            _loggerMs.LogInformation("Servicio PLC iniciando...");
            
            GestorEventos.RegistrarEvento(TiposEvento.ServicioIniciado, 
                "Servicio PLC Worker iniciado correctamente");
            
            return base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// Bucle principal del servicio (se ejecuta continuamente)
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Cargar configuración
                _logger.Informacion("Cargando configuración del servicio...");
                var configuracion = ConfiguracionManager.CargarConfiguracion();
                _logger.Informacion($"Configuración cargada: {configuracion.Plcs.Count} PLCs configurados");

                // Inicializar gestor de lecturas
                _gestorLecturas = new GestorLecturas(configuracion, _logger);
                _logger.Informacion("Gestor de lecturas inicializado correctamente");

                // Iniciar lectura continua (este método maneja su propio bucle)
                _logger.Informacion("Iniciando lectura continua de PLCs...");
                await _gestorLecturas.IniciarLecturaContinuaAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.Informacion("Servicio detenido por solicitud del usuario");
            }
            catch (Exception ex)
            {
                _logger.Error("Error crítico en el servicio", ex);
                _loggerMs.LogCritical(ex, "Error crítico que requiere reinicio del servicio");
                
                GestorEventos.RegistrarEvento(TiposEvento.ErrorCritico, 
                    $"Error crítico: {ex.Message}");
                
                // Detener la aplicación si hay error crítico
                _lifetime.StopApplication();
            }
        }

        /// <summary>
        /// Se ejecuta cuando el servicio se detiene
        /// </summary>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Informacion("=== SERVICIO PLC DETENIENDO ===");
            _loggerMs.LogInformation("Servicio PLC deteniendo...");
            
            try
            {
                // Liberar recursos del gestor
                _gestorLecturas?.Dispose();
                _logger.Informacion("Recursos liberados correctamente");
            }
            catch (Exception ex)
            {
                _logger.Error("Error al liberar recursos", ex);
            }
            
            GestorEventos.RegistrarEvento(TiposEvento.ServicioDetenido, 
                "Servicio PLC detenido correctamente");
            
            return base.StopAsync(cancellationToken);
        }
    }
}