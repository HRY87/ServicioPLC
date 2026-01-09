using System;
using System.IO;
using System.Text;

namespace PLCServicio.Eventos
{
    public enum TiposEvento
    {
        // Eventos del servicio
        ServicioIniciado,
        ServicioDetenido,
        
        // Eventos de PLC
        PlcConectado,
        PlcDesconectado,
        PlcSinRespuesta,
        PlcReconectado,
        
        // Eventos de red
        ConexionRedPerdida,
        ConexionRedRecuperada,
        
        // Eventos de base de datos
        BaseDatosError,
        BaseDatosRecuperada,
        BaseDatosLlena,
        
        // Eventos de máquina
        MaquinaApagada,
        MaquinaEncendida,
        TransferenciaInterrumpida,
        
        // Eventos críticos
        ErrorCritico,
        Advertencia,
        Informacion
    }

    public static class GestorEventos
    {
        private static readonly string RutaArchivoEventos = "Logs/eventos.log";
        private static readonly object _lockArchivo = new object();

        static GestorEventos()
        {
            // Crear directorio de logs si no existe
            var directorio = Path.GetDirectoryName(RutaArchivoEventos);
            if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }
        }

        public static void RegistrarEvento(TiposEvento tipo, string mensaje, int? plcId = null)
        {
            try
            {
                var evento = new EventoPlc
                {
                    TipoEvento = tipo,
                    Mensaje = mensaje,
                    PlcId = plcId,
                    FechaHora = DateTime.Now
                };

                EscribirEnArchivo(evento);
                
                // También mostrar en consola según la severidad
                MostrarEnConsola(evento);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar evento: {ex.Message}");
            }
        }

        private static void EscribirEnArchivo(EventoPlc evento)
        {
            lock (_lockArchivo)
            {
                try
                {
                    var linea = FormatearEvento(evento);
                    File.AppendAllText(RutaArchivoEventos, linea + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al escribir en archivo de eventos: {ex.Message}");
                }
            }
        }

        private static string FormatearEvento(EventoPlc evento)
        {
            var sb = new StringBuilder();
            sb.Append($"[{evento.FechaHora:yyyy-MM-dd HH:mm:ss}] ");
            sb.Append($"[{ObtenerNivelSeveridad(evento.TipoEvento)}] ");
            sb.Append($"[{evento.TipoEvento}] ");
            
            if (evento.PlcId.HasValue)
            {
                sb.Append($"[PLC:{evento.PlcId}] ");
            }
            
            sb.Append(evento.Mensaje);
            
            return sb.ToString();
        }

        private static void MostrarEnConsola(EventoPlc evento)
        {
            var nivelSeveridad = ObtenerNivelSeveridad(evento.TipoEvento);
            var colorOriginal = Console.ForegroundColor;

            Console.ForegroundColor = nivelSeveridad switch
            {
                "ERROR" => ConsoleColor.Red,
                "WARN" => ConsoleColor.Yellow,
                "INFO" => ConsoleColor.Cyan,
                _ => ConsoleColor.White
            };

            Console.WriteLine(FormatearEvento(evento));
            Console.ForegroundColor = colorOriginal;
        }

        private static string ObtenerNivelSeveridad(TiposEvento tipo)
        {
            return tipo switch
            {
                TiposEvento.ErrorCritico => "ERROR",
                TiposEvento.PlcSinRespuesta => "ERROR",
                TiposEvento.BaseDatosError => "ERROR",
                TiposEvento.ConexionRedPerdida => "ERROR",
                TiposEvento.TransferenciaInterrumpida => "ERROR",
                
                TiposEvento.Advertencia => "WARN",
                TiposEvento.PlcDesconectado => "WARN",
                TiposEvento.MaquinaApagada => "WARN",
                
                _ => "INFO"
            };
        }

        public static void LimpiarLogAntiguos(int diasAntiguedad = 30)
        {
            try
            {
                var archivoInfo = new FileInfo(RutaArchivoEventos);
                if (archivoInfo.Exists && archivoInfo.LastWriteTime < DateTime.Now.AddDays(-diasAntiguedad))
                {
                    var rutaBackup = $"{RutaArchivoEventos}.{DateTime.Now:yyyyMMdd}.bak";
                    File.Move(RutaArchivoEventos, rutaBackup);
                    RegistrarEvento(TiposEvento.Informacion, $"Log de eventos archivado: {rutaBackup}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al limpiar logs antiguos: {ex.Message}");
            }
        }
    }

    public class EventoPlc
    {
        public DateTime FechaHora { get; set; }
        public TiposEvento TipoEvento { get; set; }
        public string Mensaje { get; set; }
        public int? PlcId { get; set; }
    }
}
