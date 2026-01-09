using System;
using System.IO;
using System.Text;

namespace PLCServicio.Utilidades
{
    public class Logger
    {
        private readonly string _rutaArchivoLog;
        private readonly object _lockArchivo = new object();
        private readonly long _tamañoMaximoBytes = 10 * 1024 * 1024; // 10 MB

        public Logger(string nombreArchivo = "servicio_plc.log")
        {
            var directorioLogs = "Logs";
            if (!Directory.Exists(directorioLogs))
            {
                Directory.CreateDirectory(directorioLogs);
            }

            _rutaArchivoLog = Path.Combine(directorioLogs, nombreArchivo);
        }

        public void Informacion(string mensaje)
        {
            EscribirLog("INFO", mensaje);
        }

        public void Advertencia(string mensaje)
        {
            EscribirLog("WARN", mensaje);
        }

        public void Error(string mensaje, Exception excepcion = null)
        {
            var mensajeCompleto = mensaje;
            if (excepcion != null)
            {
                mensajeCompleto += $"\nExcepción: {excepcion.GetType().Name}";
                mensajeCompleto += $"\nMensaje: {excepcion.Message}";
                mensajeCompleto += $"\nStackTrace: {excepcion.StackTrace}";
                
                if (excepcion.InnerException != null)
                {
                    mensajeCompleto += $"\nInnerException: {excepcion.InnerException.Message}";
                }
            }
            
            EscribirLog("ERROR", mensajeCompleto);
        }

        public void Debug(string mensaje)
        {
            #if DEBUG
            EscribirLog("DEBUG", mensaje);
            #endif
        }

        private void EscribirLog(string nivel, string mensaje)
        {
            lock (_lockArchivo)
            {
                try
                {
                    RotarLogSiEsNecesario();

                    var linea = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{nivel}] {mensaje}";
                    File.AppendAllText(_rutaArchivoLog, linea + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al escribir en log: {ex.Message}");
                }
            }
        }

        private void RotarLogSiEsNecesario()
        {
            try
            {
                var archivoInfo = new FileInfo(_rutaArchivoLog);
                
                if (archivoInfo.Exists && archivoInfo.Length > _tamañoMaximoBytes)
                {
                    var rutaBackup = $"{_rutaArchivoLog}.{DateTime.Now:yyyyMMddHHmmss}.bak";
                    File.Move(_rutaArchivoLog, rutaBackup);
                    
                    // Eliminar backups antiguos (mantener solo los últimos 5)
                    EliminarBackupsAntiguos();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al rotar log: {ex.Message}");
            }
        }

        private void EliminarBackupsAntiguos()
        {
            try
            {
                var directorio = Path.GetDirectoryName(_rutaArchivoLog);
                var nombreArchivo = Path.GetFileName(_rutaArchivoLog);
                var patron = $"{nombreArchivo}.*.bak";
                
                var archivosBackup = Directory.GetFiles(directorio, patron);
                Array.Sort(archivosBackup);
                
                // Mantener solo los últimos 5
                if (archivosBackup.Length > 5)
                {
                    for (int i = 0; i < archivosBackup.Length - 5; i++)
                    {
                        File.Delete(archivosBackup[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar backups antiguos: {ex.Message}");
            }
        }

        public void LimpiarLogsAntiguos(int diasAntiguedad = 30)
        {
            try
            {
                var directorio = Path.GetDirectoryName(_rutaArchivoLog);
                var archivos = Directory.GetFiles(directorio, "*.bak");
                
                foreach (var archivo in archivos)
                {
                    var archivoInfo = new FileInfo(archivo);
                    if (archivoInfo.LastWriteTime < DateTime.Now.AddDays(-diasAntiguedad))
                    {
                        File.Delete(archivo);
                        Informacion($"Log antiguo eliminado: {archivo}");
                    }
                }
            }
            catch (Exception ex)
            {
                Error("Error al limpiar logs antiguos", ex);
            }
        }
    }
}
