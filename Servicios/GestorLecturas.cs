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
        private readonly BaseDatos _repositorio;
        private readonly SemaphoreSlim _semaforoLectura;
        private bool _disposed = false;
        private bool _enProcesoDetencion = false;

        public GestorLecturas(ConfiguracionManager configuracion, Logger logger)
        {
            _configuracion = configuracion ?? throw new ArgumentNullException(nameof(configuracion));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lectores = new List<LectorPlc>();
            _semaforoLectura = new SemaphoreSlim(1, 1);
            _repositorio = new BaseDatos(_configuracion.Databases, _logger);

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

            GestorMensajes.MostrarExito($"✓ {_lectores.Count} lectores PLC inicializados correctamente");
            _logger.Informacion("Gestor de lecturas inicializado correctamente");
        }

        public async Task IniciarLecturaContinuaAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Conectar a todos los PLCs
                await ConectarTodosAsync(cancellationToken);

                // Bucle principal de lectura
                while (!cancellationToken.IsCancellationRequested && !_disposed)
                {
                    try
                    {
                        // Verificar si el semáforo fue liberado durante la detención
                        if (_disposed || _enProcesoDetencion) break;

                        await _semaforoLectura.WaitAsync(cancellationToken);
                        
                        try
                        {
                            await LeerTodosLosPLCsAsync(cancellationToken);
                        }
                        finally
                        {
                            // Solo liberar si no estamos en proceso de detención
                            if (!_disposed && !_enProcesoDetencion)
                            {
                                _semaforoLectura.Release();
                            }
                        }

                        // Esperar el intervalo configurado
                        await Task.Delay(
                            TimeSpan.FromSeconds(_configuracion.IntervaloLecturaSegundos), 
                            cancellationToken
                        );
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancelación normal, no es un error
                        GestorMensajes.MostrarInformacion("⏹ Deteniendo lectura de PLCs...");
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        // El servicio se está deteniendo, salir silenciosamente
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error en el bucle de lectura continua", ex);
                        GestorMensajes.MostrarError($"✗ Error en ciclo de lectura: {ex.Message}");
                        
                        // Esperar antes de reintentar para evitar bucle infinito de errores
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                GestorMensajes.MostrarInformacion("⏹ Lectura continua cancelada");
            }
            catch (Exception ex)
            {
                _logger.Error("Error crítico en lectura continua", ex);
                GestorMensajes.MostrarError($"✗✗✗ ERROR CRÍTICO: {ex.Message}");
                throw;
            }
        }

        private async Task ConectarTodosAsync(CancellationToken cancellationToken)
        {
            GestorMensajes.MostrarInformacion($"🔌 Conectando a {_lectores.Count} PLCs...");
            _logger.Informacion($"Conectando a {_lectores.Count} PLCs...");
            
            var tareasConexion = _lectores.Select(lector => 
                ConectarConReintentosAsync(lector, cancellationToken)
            );

            await Task.WhenAll(tareasConexion);
            
            var conectados = _lectores.Count(l => l.Estado.Conectado);
            
            if (conectados == _lectores.Count)
            {
                GestorMensajes.MostrarExito($"✓ Todos los PLCs conectados ({conectados}/{_lectores.Count})");
            }
            else if (conectados > 0)
            {
                GestorMensajes.MostrarAdvertencia($"⚠ PLCs conectados: {conectados}/{_lectores.Count}");
            }
            else
            {
                GestorMensajes.MostrarError($"✗ Ningún PLC conectado (0/{_lectores.Count})");
            }
            
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
                    GestorMensajes.MostrarAdvertencia($"⟳ Reintento {intento}/{_configuracion.MaximoReintentos} - {lector.Estado.Nombre}");
                    _logger.Advertencia($"Reintentando conexión a {lector.Estado.Nombre} ({intento}/{_configuracion.MaximoReintentos})...");
                    
                    await Task.Delay(
                        TimeSpan.FromSeconds(_configuracion.IntervaloReconexionSegundos), 
                        cancellationToken
                    );
                }
            }

            GestorMensajes.MostrarError($"✗ {lector.Estado.Nombre} no disponible después de {_configuracion.MaximoReintentos} intentos");
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
                if (lecturasExitosas == totalConectados && totalConectados > 0)
                {
                    GestorMensajes.MostrarExito($"✓ Ciclo completado: {lecturasExitosas}/{totalConectados} lecturas OK");
                }
                else if (lecturasExitosas > 0)
                {
                    GestorMensajes.MostrarAdvertencia($"⚠ Ciclo parcial: {lecturasExitosas}/{totalConectados} lecturas OK");
                }
                else
                {
                    GestorMensajes.MostrarError($"✗ Sin lecturas exitosas (0/{totalConectados})");
                }
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
            catch (OperationCanceledException)
            {
                // Cancelación normal, no mostrar error
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al leer {lector.Estado.Nombre}", ex);
                GestorMensajes.MostrarError($"✗ Error leyendo {lector.Estado.Nombre}: {ex.Message}");
                
                // Intentar reconectar en segundo plano
                _ = Task.Run(async () => 
                {
                    try
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(_configuracion.IntervaloReconexionSegundos), 
                            cancellationToken
                        );
                        
                        if (!lector.Estado.Conectado && !_disposed)
                        {
                            GestorMensajes.MostrarInformacion($"🔄 Reconectando {lector.Estado.Nombre}...");
                            await lector.ReconectarAsync(cancellationToken);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignorar errores en reconexión en segundo plano
                    }
                }, cancellationToken);

                return null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _enProcesoDetencion = true;
            _logger.Informacion("Liberando recursos del Gestor de Lecturas...");
            GestorMensajes.MostrarInformacion("🔧 Liberando recursos...");
            
            // Desconectar todos los lectores primero
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
            
            // Liberar repositorio
            try
            {
                _repositorio?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error("Error al liberar repositorio", ex);
            }
            
            // Liberar semáforo al final
            try
            {
                _semaforoLectura?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error("Error al liberar semáforo", ex);
            }
            
            _disposed = true;
            GestorMensajes.MostrarExito("✓ Recursos liberados correctamente");
        }
    }
}