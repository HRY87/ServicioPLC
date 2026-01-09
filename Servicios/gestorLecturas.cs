using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PLCServicio.Configuracion;
using PLCServicio.Modelos;
using PLCServicio.Utilidades;
using PLCServicio.Eventos;
using PLCServicio.Mensajes;

namespace PLCServicio.Servicios
{
    public class GestorLecturas : IDisposable
    {
        private readonly ConfiguracionManager _configuracion;
        private readonly Logger _logger;
        private readonly List<LectorPlc> _lectores;
        private readonly RepositorioDatos _repositorio;
        private readonly SemaphoreSlim _semaforoLectura;

        public GestorLecturas(ConfiguracionManager configuracion, Logger logger)
        {
            _configuracion = configuracion ?? throw new ArgumentNullException(nameof(configuracion));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lectores = new List<LectorPlc>();
            _semaforoLectura = new SemaphoreSlim(1, 1);
            _repositorio = new RepositorioDatos(_configuracion.Databases, _logger);

            InicializarLectores();
        }

        private void InicializarLectores()
        {
            foreach (var plcConfig in _configuracion.Plcs.Where(p => p.Habilitada))
            {
                try
                {
                    var lector = new LectorPlc(
                        plcConfig,
                        _configuracion.TimeoutLecturaSegundos,
                        _configuracion.MaximoReintentos,
                        _logger
                    );
                    
                    _lectores.Add(lector);
                    _logger.Informacion($"Lector TCP/IP inicializado para {plcConfig.Nombre} ({plcConfig.Ip}:{plcConfig.Puerto})");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error al inicializar lector para {plcConfig.Nombre}", ex);
                }
            }

            GestorMensajes.MostrarInformacion($"Se inicializaron {_lectores.Count} lectores PLC con TCP/IP");
        }

        public async Task IniciarLecturaContinuaAsync(CancellationToken cancellationToken)
        {
            // Conectar a todos los PLCs
            await ConectarTodosAsync(cancellationToken);

            // Bucle principal de lectura
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _semaforoLectura.WaitAsync(cancellationToken);
                    
                    try
                    {
                        await LeerTodosLosPLCsAsync(cancellationToken);
                    }
                    finally
                    {
                        _semaforoLectura.Release();
                    }

                    // Esperar el intervalo configurado
                    await Task.Delay(
                        TimeSpan.FromSeconds(_configuracion.IntervaloLecturaSegundos), 
                        cancellationToken
                    );
                }
                catch (OperationCanceledException)
                {
                    _logger.Informacion("Lectura continua cancelada por solicitud del usuario");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error("Error en el bucle de lectura continua", ex);
                    
                    // Esperar antes de reintentar para evitar bucle infinito de errores
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }

        private async Task ConectarTodosAsync(CancellationToken cancellationToken)
        {
            _logger.Informacion($"Conectando a {_lectores.Count} PLCs...");
            
            var tareasConexion = _lectores.Select(lector => 
                ConectarConReintentosAsync(lector, cancellationToken)
            );

            await Task.WhenAll(tareasConexion);
            
            var conectados = _lectores.Count(l => l.Estado.Conectado);
            _logger.Informacion($"PLCs conectados: {conectados}/{_lectores.Count}");
        }

        private async Task<bool> ConectarConReintentosAsync(LectorPlc lector, CancellationToken cancellationToken)
        {
            for (int intento = 1; intento <= _configuracion.MaximoReintentos; intento++)
            {
                if (await lector.ConectarAsync(cancellationToken))
                {
                    return true;
                }

                if (intento < _configuracion.MaximoReintentos)
                {
                    _logger.Advertencia($"Reintentando conexión a {lector.Estado.Nombre} ({intento}/{_configuracion.MaximoReintentos})...");
                    await Task.Delay(
                        TimeSpan.FromSeconds(_configuracion.IntervaloReconexionSegundos), 
                        cancellationToken
                    );
                }
            }

            _logger.Error($"No se pudo conectar a {lector.Estado.Nombre} después de {_configuracion.MaximoReintentos} intentos");
            return false;
        }

        private async Task LeerTodosLosPLCsAsync(CancellationToken cancellationToken)
        {
            var tareasLectura = _lectores
                .Where(l => l.Estado.Conectado)
                .Select(lector => LeerYGuardarPlcAsync(lector, cancellationToken));

            var resultados = await Task.WhenAll(tareasLectura);
            
            var lecturasExitosas = resultados.Count(r => r != null);
            var totalConectados = _lectores.Count(l => l.Estado.Conectado);
            
            if (Environment.UserInteractive)
            {
                GestorMensajes.MostrarInformacion($"Ciclo completado: {lecturasExitosas}/{totalConectados} lecturas exitosas");
            }
        }

        private async Task<LecturaPlc> LeerYGuardarPlcAsync(LectorPlc lector, CancellationToken cancellationToken)
        {
            try
            {
                // Leer datos del PLC
                var lectura = await lector.LeerDatosAsync(cancellationToken);
                
                if (lectura != null)
                {
                    // Guardar en bases de datos (fire-and-forget para no bloquear)
                    _ = _repositorio.GuardarLecturaAsync(lectura, cancellationToken);
                    
                    // Mostrar información solo en modo consola
                    if (Environment.UserInteractive)
                    {
                        GestorMensajes.MostrarLectura(lectura);
                    }
                }

                return lectura;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al leer {lector.Estado.Nombre}", ex);
                
                // Intentar reconectar en segundo plano
                _ = Task.Run(async () => 
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(_configuracion.IntervaloReconexionSegundos), 
                        cancellationToken
                    );
                    
                    if (!lector.Estado.Conectado)
                    {
                        GestorMensajes.MostrarAdvertencia($"Reconectando {lector.Estado.Nombre}...");
                        await lector.ReconectarAsync(cancellationToken);
                    }
                }, cancellationToken);

                return null;
            }
        }

        public void Dispose()
        {
            _logger.Informacion("Liberando recursos del Gestor de Lecturas...");
            
            foreach (var lector in _lectores)
            {
                try
                {
                    lector.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error al liberar lector {lector.Estado.Nombre}", ex);
                }
            }

            _lectores.Clear();
            _repositorio?.Dispose();
            _semaforoLectura?.Dispose();
        }
    }
}