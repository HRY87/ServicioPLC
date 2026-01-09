using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PLCServicio.Configuracion;
using PLCServicio.Modelos;
using PLCServicio.Utilidades;
using PLCServicio.Eventos;

namespace PLCServicio.Servicios
{
    /// <summary>
    /// Lector de PLC usando protocolo TCP/IP personalizado (Controlplast)
    /// </summary>
    public class LectorPlc : IDisposable
    {
        private readonly ConfiguracionPlc _configuracion;
        private readonly Logger _logger;
        private readonly int _timeoutSegundos;
        private readonly int _maximoReintentos;
        private EstadoPlc _estado;
        
        // Cliente TCP
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private bool _disposed;
        
        // Tipos de memoria del PLC
        private const int TIPO_DADOS = 0x8DFF;
        private const int TIPO_PARAMETROS = 0x08FF;

        public EstadoPlc Estado => _estado;

        public LectorPlc(ConfiguracionPlc configuracion, int timeoutSegundos, 
            int maximoReintentos, Logger logger)
        {
            _configuracion = configuracion ?? throw new ArgumentNullException(nameof(configuracion));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeoutSegundos = timeoutSegundos;
            _maximoReintentos = maximoReintentos;
            
            _estado = new EstadoPlc
            {
                PlcId = configuracion.Id,
                Nombre = configuracion.Nombre
            };
        }

        public async Task<bool> ConectarAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.Informacion($"Conectando a {_configuracion.Nombre} ({_configuracion.Ip}:{_configuracion.Puerto})...");

                _tcpClient = new TcpClient();
                _tcpClient.SendTimeout = _timeoutSegundos * 1000;
                _tcpClient.ReceiveTimeout = _timeoutSegundos * 1000;
                
                // Conectar con timeout
                var connectTask = _tcpClient.ConnectAsync(_configuracion.Ip, _configuracion.Puerto);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(_timeoutSegundos), cancellationToken);
                
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"Timeout al conectar con {_configuracion.Nombre}");
                }
                
                await connectTask;
                
                _stream = _tcpClient.GetStream();
                
                _estado.Conectado = true;
                _estado.UltimaConexion = DateTime.Now;
                _estado.IntentosReconexion = 0;
                _estado.MensajeEstado = "Conectado correctamente";

                GestorEventos.RegistrarEvento(TiposEvento.PlcConectado, 
                    $"{_configuracion.Nombre} conectado exitosamente", _configuracion.Id);
                
                return true;
            }
            catch (Exception ex)
            {
                _estado.Conectado = false;
                _estado.MensajeEstado = $"Error de conexión: {ex.Message}";
                
                _logger.Error($"Error al conectar con {_configuracion.Nombre}", ex);
                
                GestorEventos.RegistrarEvento(TiposEvento.PlcDesconectado, 
                    $"Error al conectar: {ex.Message}", _configuracion.Id);
                
                Desconectar();
                return false;
            }
        }

        public async Task<LecturaPlc> LeerDatosAsync(CancellationToken cancellationToken = default)
        {
            if (!_estado.Conectado || _stream == null)
            {
                throw new InvalidOperationException($"PLC {_configuracion.Nombre} no está conectado");
            }

            try
            {
                var lectura = new LecturaPlc
                {
                    PlcId = _configuracion.Id,
                    NombrePlc = _configuracion.Nombre,
                    FechaHoraLectura = DateTime.Now
                };

                // PRODUCCIÓN ACTUAL (800-810)
                lectura.KgHoraActual = await LeerFloatAsync(800, TIPO_DADOS,cancellationToken) ?? 0;
                lectura.EspesorActual = await LeerFloatAsync(802, TIPO_DADOS,cancellationToken) ?? 0;
                lectura.VelocidadLinea = await LeerFloatAsync(810, TIPO_DADOS,cancellationToken) ?? 0;
                
                // CONTADOR Y ESTADO
                var estadoWord = await LeerWordAsync(30023, TIPO_PARAMETROS, cancellationToken) ?? 0;
                lectura.EstadoMaquina = estadoWord > 0;
                lectura.ContadorProduccion = (int)(await LeerFloatAsync(30037, TIPO_PARAMETROS, cancellationToken) ?? 0);

                _estado.UltimaLectura = DateTime.Now;
                _estado.MensajeEstado = "Lectura exitosa";

                return lectura;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al leer datos de {_configuracion.Nombre}", ex);
                
                GestorEventos.RegistrarEvento(TiposEvento.PlcSinRespuesta, 
                    $"Error en lectura: {ex.Message}", _configuracion.Id);
                
                throw;
            }
        }

        // =====================================================
        // MÉTODOS DE PROTOCOLO TCP/IP PERSONALIZADO
        // =====================================================

        private byte[] ConstruirSolicitud(int direccion, int numWords, int tipo = TIPO_DADOS)
        {
            byte[] header = new byte[38];
            
            // Header estándar del protocolo
            header[0] = 0x01; header[1] = 0x60; header[2] = 0x00; header[3] = 0x00;
            header[4] = 0xFF; header[5] = 0x00; header[6] = 0x00; header[7] = 0x00;
            header[8] = 0x00; header[9] = 0x00; header[10] = 0x08; header[11] = 0x00;
            header[12] = 0x0C; header[13] = 0x00; header[14] = 0x69; header[15] = 0x01;
            header[16] = 0x00; header[17] = 0x00; header[18] = 0x01; header[19] = 0x00;
            header[20] = 0x00; header[21] = 0x00; header[22] = 0x00; header[23] = 0x00;
            header[24] = 0x00; header[25] = 0x00; header[26] = 0x00; header[27] = 0x00;
            
            // Tipo de memoria
            if (tipo == TIPO_DADOS)
            {
                header[28] = 0x8D;
                header[29] = 0xFF;
            }
            else
            {
                header[28] = 0x08;
                header[29] = 0xFF;
            }
            
            // Dirección (3 bytes)
            header[30] = (byte)(direccion & 0xFF);
            header[31] = (byte)((direccion >> 8) & 0xFF);
            header[32] = (byte)((direccion >> 16) & 0xFF);
            header[33] = 0x00;
            
            // Número de words (2 bytes)
            header[34] = (byte)(numWords & 0xFF);
            header[35] = (byte)((numWords >> 8) & 0xFF);
            header[36] = 0x00;
            header[37] = 0x00;
            
            return header;
        }

        private async Task<int[]> LeerWordsAsync(int direccion, int numWords, int tipo = TIPO_DADOS, 
            CancellationToken cancellationToken = default)
        {
            if (!_estado.Conectado || _stream == null)
                throw new InvalidOperationException("No hay conexión activa");
            
            byte[] solicitud = ConstruirSolicitud(direccion, numWords, tipo);
            await _stream.WriteAsync(solicitud, cancellationToken);
            
            byte[] buffer = new byte[8192];
            int bytesLeidos = await _stream.ReadAsync(buffer, cancellationToken);
            
            if (bytesLeidos < 33)
                throw new InvalidOperationException($"Respuesta incompleta: {bytesLeidos} bytes");
            
            int[] words = new int[numWords];
            for (int i = 0; i < numWords; i++)
            {
                int offset = 33 + (i * 2);
                if (offset + 1 < bytesLeidos)
                {
                    words[i] = buffer[offset] + (buffer[offset + 1] * 256);
                }
            }
            
            return words;
        }

        private async Task<int?> LeerWordAsync(int direccion, int tipo = TIPO_DADOS, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var words = await LeerWordsAsync(direccion, 1, tipo, cancellationToken);
                return words[0];
            }
            catch
            {
                return null;
            }
        }

        private async Task<float?> LeerFloatAsync(int direccion, int tipo = TIPO_DADOS, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var words = await LeerWordsAsync(direccion, 2, tipo, cancellationToken);
                if (words.Length >= 2)
                {
                    byte[] bytes = new byte[4];
                    bytes[0] = (byte)(words[0] & 0xFF);
                    bytes[1] = (byte)((words[0] >> 8) & 0xFF);
                    bytes[2] = (byte)(words[1] & 0xFF);
                    bytes[3] = (byte)((words[1] >> 8) & 0xFF);
                    
                    return BitConverter.ToSingle(bytes, 0);
                }
            }
            catch { }
            
            return null;
        }

        public async Task<bool> ReconectarAsync(CancellationToken cancellationToken = default)
        {
            _logger.Informacion($"Reconectando {_configuracion.Nombre}...");
            
            _estado.IntentosReconexion++;
            
            Desconectar();
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            
            return await ConectarAsync(cancellationToken);
        }

        public void Desconectar()
        {
            try
            {
                _stream?.Close();
                _tcpClient?.Close();
                
                _estado.Conectado = false;
                _estado.MensajeEstado = "Desconectado";
                
                GestorEventos.RegistrarEvento(TiposEvento.PlcDesconectado, 
                    $"{_configuracion.Nombre} desconectado", _configuracion.Id);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al desconectar {_configuracion.Nombre}", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            Desconectar();
            _stream?.Dispose();
            _tcpClient?.Dispose();
            
            _disposed = true;
        }
    }
}