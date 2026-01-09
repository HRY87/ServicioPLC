using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PLCServicio.Configuracion;
using PLCServicio.Modelos;
using PLCServicio.Utilidades;
using PLCServicio.Eventos;
using PLCServicio.Mensajes;

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
                GestorMensajes.MostrarInformacion($"🔌 Conectando a {_configuracion.Nombre} ({_configuracion.Ip}:{_configuracion.Puerto})...");
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

                GestorMensajes.MostrarExito($"✓ {_configuracion.Nombre} conectado");
                GestorEventos.RegistrarEvento(TiposEvento.PlcConectado, 
                    $"{_configuracion.Nombre} conectado exitosamente", _configuracion.Id);
                
                return true;
            }
            catch (TimeoutException ex)
            {
                _estado.Conectado = false;
                _estado.MensajeEstado = "Timeout de conexión";
                
                GestorMensajes.MostrarAdvertencia($"⏱ {_configuracion.Nombre} - Timeout");
                _logger.Error($"Timeout al conectar con {_configuracion.Nombre}", ex);
                
                GestorEventos.RegistrarEvento(TiposEvento.PlcDesconectado, 
                    $"Error al conectar: {ex.Message}", _configuracion.Id);
                
                Desconectar();
                return false;
            }
            catch (SocketException ex)
            {
                _estado.Conectado = false;
                _estado.MensajeEstado = "Error de red";
                
                string mensajeError = ex.SocketErrorCode switch
                {
                    SocketError.ConnectionRefused => "Conexión rechazada",
                    SocketError.HostNotFound => "Host no encontrado",
                    SocketError.HostUnreachable => "Host no alcanzable",
                    SocketError.NetworkUnreachable => "Red no alcanzable",
                    SocketError.TimedOut => "Timeout de conexión",
                    _ => $"Error de socket: {ex.SocketErrorCode}"
                };
                
                GestorMensajes.MostrarError($"✗ {_configuracion.Nombre} - {mensajeError}");
                _logger.Error($"Error de socket al conectar con {_configuracion.Nombre}: {mensajeError}", ex);
                
                GestorEventos.RegistrarEvento(TiposEvento.PlcDesconectado, 
                    mensajeError, _configuracion.Id);
                
                Desconectar();
                return false;
            }
            catch (Exception ex)
            {
                _estado.Conectado = false;
                _estado.MensajeEstado = $"Error: {ex.Message}";
                
                GestorMensajes.MostrarError($"✗ {_configuracion.Nombre} - Error inesperado");
                _logger.Error($"Error inesperado al conectar con {_configuracion.Nombre}", ex);
                
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
                    FechaHoraLectura = DateTime.Now,
                    Produccion = new DatosProduccion()
                };

                // Producción Actual
                lectura.Produccion.KgHoraActual = await LeerFloatAsync(DatosProduccion.ADDR_KG_HORA_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.EspesorActual = await LeerFloatAsync(DatosProduccion.ADDR_ESPESOR_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.AnchoBrutoActual = await LeerFloatAsync(DatosProduccion.ADDR_ANCHO_BRUTO_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.GramajeActual = await LeerFloatAsync(DatosProduccion.ADDR_GRAMAJE_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.AnchoNetoActual = await LeerFloatAsync(DatosProduccion.ADDR_ANCHO_NETO_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.MetrosPorMinActual = await LeerFloatAsync(DatosProduccion.ADDR_METROS_POR_MIN_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;

                // PRODUCCIÓN PROGRAMADA
                lectura.Produccion.KgHoraProgramado = await LeerFloatAsync(DatosProduccion.ADDR_KG_HORA_PROGRAMADO, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.EspesorProgramado = await LeerFloatAsync(DatosProduccion.ADDR_ESPESOR_PROGRAMADO, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.AnchoBrutoProgramado = await LeerFloatAsync(DatosProduccion.ADDR_ANCHO_BRUTO_PROGRAMADO, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.AnchoNetoProgramado = await LeerFloatAsync(DatosProduccion.ADDR_ANCHO_NETO_PROGRAMADO, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.GramajeProgramado = await LeerFloatAsync(DatosProduccion.ADDR_GRAMAJE_PROGRAMADO, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.MetrosPorMinProgramado = await LeerFloatAsync(DatosProduccion.ADDR_METROS_POR_MIN_PROGRAMADO, TIPO_DADOS, cancellationToken) ?? 0;
                
                // Rosca A - Programado
                lectura.Produccion.RoscaA_GramaMetroProgramado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_GRAMA_METRO_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_EspesorProgramado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_ESPESOR_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_PorcentajeProgramado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_PORCENTAJE_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_KgHoraProgramado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_KG_HORA_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo1Programado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO1_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo2Programado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO2_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo3Programado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO3_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo4Programado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO4_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo5Programado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO5_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo6Programado = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO6_PROG, TIPO_DADOS, cancellationToken) ?? 0;
                
                // Rosca A - Actual
                lectura.Produccion.RoscaA_GramaMetroActual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_GRAMA_METRO_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_EspesorActual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_ESPESOR_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_KgHoraActual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_KG_HORA_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_PorcentajeActual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_PORCENTAJE_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo1Actual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO1_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo2Actual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO2_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo3Actual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO3_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo4Actual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO4_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo5Actual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO5_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_Silo6Actual = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_SILO6_ACTUAL, TIPO_DADOS, cancellationToken) ?? 0;
                
                // Consumo
                lectura.Produccion.AmperesL1  = await LeerFloatAsync(DatosProduccion.ADDR_AMPERES_L1, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.ConsumoActualKW = await LeerFloatAsync(DatosProduccion.ADDR_CONSUMO_ACTUAL_KW, TIPO_DADOS, cancellationToken) ?? 0;
                
                // OP / Producción
                lectura.Produccion.NumeroOP = await LeerStringAsync(DatosProduccion.ADDR_NUMERO_OP, 16, TIPO_DADOS, cancellationToken) ?? "";
                lectura.Produccion.KgPorMetroOP = await LeerFloatAsync(DatosProduccion.ADDR_KG_POR_METRO_OP, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.TamanoBobinaOP = await LeerFloatAsync(DatosProduccion.ADDR_TAMANO_BOBINA_OP, TIPO_DADOS, cancellationToken) ?? 0;
                //data.RecortesOP = await LeerFloatAsync(ADDR_RECORTES_OP, TIPO_DADOS, cancellationToken) ?? 0;
                //lectura.Produccion.EstadoOP = await LeerWordAsync(DatosProduccion.ADDR_ESTADO_OP, 16, TIPO_DADOS, cancellationToken) ?? "";
                lectura.Produccion.KgProducidos = await LeerFloatAsync(DatosProduccion.ADDR_KG_PRODUCIDOS, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.MetrosProducidos = await LeerFloatAsync(DatosProduccion.ADDR_METROS_PRODUCIDOS, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.ConsumoTotalOP = await LeerFloatAsync(DatosProduccion.ADDR_CONSUMO_TOTAL_OP, TIPO_DADOS, cancellationToken) ?? 0;
                
                // Totalizadores
                lectura.Produccion.RoscaA_TotalSilo1 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_TOTAL_SILO1, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_TotalSilo2 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_TOTAL_SILO2, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_TotalSilo3 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_TOTAL_SILO3, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_TotalSilo4 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_TOTAL_SILO4, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_TotalSilo5 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_TOTAL_SILO5, TIPO_DADOS, cancellationToken) ?? 0;
                lectura.Produccion.RoscaA_TotalSilo6 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_TOTAL_SILO6, TIPO_DADOS, cancellationToken) ?? 0;
                
                // Densidades
                lectura.Produccion.RoscaA_DensidadSilo1 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_DENSIDAD_SILO1) ?? 0;
                lectura.Produccion.RoscaA_DensidadSilo2 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_DENSIDAD_SILO2) ?? 0;
                lectura.Produccion.RoscaA_DensidadSilo3 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_DENSIDAD_SILO3) ?? 0;
                lectura.Produccion.RoscaA_DensidadSilo4 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_DENSIDAD_SILO4) ?? 0;
                lectura.Produccion.RoscaA_DensidadSilo5 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_DENSIDAD_SILO5) ?? 0;
                lectura.Produccion.RoscaA_DensidadSilo6 = await LeerFloatAsync(DatosProduccion.ADDR_ROSCA_A_DENSIDAD_SILO6) ?? 0;
                _estado.UltimaLectura = DateTime.Now;
                _estado.MensajeEstado = "Lectura exitosa";

                return lectura;
            }
            catch (OperationCanceledException)
            {
                // Cancelación normal, propagar
                throw;
            }
            catch (TimeoutException ex)
            {
                GestorMensajes.MostrarAdvertencia($"⏱ {_configuracion.Nombre} - Timeout de lectura");
                _logger.Error($"Timeout al leer datos de {_configuracion.Nombre}", ex);
                
                GestorEventos.RegistrarEvento(TiposEvento.PlcSinRespuesta, 
                    "Timeout en lectura", _configuracion.Id);
                
                throw;
            }
            catch (Exception ex)
            {
                GestorMensajes.MostrarError($"✗ {_configuracion.Nombre} - Error de lectura");
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

        private async Task<float?> LeerFloatAsync(int direccion, int tipo = TIPO_DADOS, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var words = await LeerWordsAsync(direccion, 2, tipo, cancellationToken);
                if (words.Length >= 2)
                {
                    byte[] bytes =
                    [
                        (byte)(words[0] & 0xFF),
                        (byte)((words[0] >> 8) & 0xFF),
                        (byte)(words[1] & 0xFF),
                        (byte)((words[1] >> 8) & 0xFF),
                    ];

                    return BitConverter.ToSingle(bytes, 0);
                }
            }
            catch { }
            
            return null;
        }

        public async Task<string> LeerStringAsync(int direccion, int numChars, int tipo = TIPO_DADOS,
            CancellationToken cancellationToken = default)
        {
            try
            {
                int numWords = (numChars + 1) / 2;
                var words = await LeerWordsAsync(direccion, numWords, tipo, cancellationToken);

                if (words == null || words.Length == 0)
                    return null;

                StringBuilder result = new StringBuilder(numChars);

                foreach (var word in words)
                {
                    char char1 = (char)(word & 0xFF);
                    char char2 = (char)((word >> 8) & 0xFF);

                    result.Append(char1);
                    result.Append(char2);
                }

                return result.ToString().Replace("\0", "").Trim();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ReconectarAsync(CancellationToken cancellationToken = default)
        {
            GestorMensajes.MostrarInformacion($"🔄 Reconectando {_configuracion.Nombre}...");
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