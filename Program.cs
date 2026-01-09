using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PLCServicio.Servicios;
using PLCServicio.Utilidades;
using PLCServicio.Configuracion;
using PLCServicio.Mensajes;
using System;
using System.Runtime.InteropServices;
using System.IO;

namespace PLCServicio
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                // Mostrar información si se ejecuta en modo consola
                if (Environment.UserInteractive)
                {
                    MostrarBannerConsola();
                }

                var host = CrearHostBuilder(args).Build();
                host.Run();
                
                return 0; // Salida exitosa
            }
            catch (HostAbortedException)
            {
                // El host fue abortado intencionalmente - no es un error
                GestorMensajes.MostrarInformacion("⏹ Servicio detenido");
                return 0;
            }
            catch (FileNotFoundException ex)
            {
                MostrarErrorCritico("Archivo No Encontrado", ex.Message, ex.FileName);
                return 1;
            }
            catch (InvalidOperationException ex)
            {
                MostrarErrorCritico("Error de Configuración", ex.Message, 
                    ex.InnerException?.Message);
                return 2;
            }
            catch (Exception ex)
            {
                MostrarErrorCritico("Error Crítico", ex.GetType().Name, ex.Message);
                return 99;
            }
        }

        public static IHostBuilder CrearHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // IMPORTANTE: Esto hace que funcione como servicio de Windows
                .UseWindowsService(options =>
                {
                    options.ServiceName = "ServicioPLC";
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Registrar el Worker principal
                    services.AddHostedService<WorkerPLC>();
                    
                    // Registrar servicios como Singleton para reutilización
                    services.AddSingleton<Logger>();
                    services.AddSingleton(ConfiguracionManager.CargarConfiguracion());
                    services.AddSingleton<GestorLecturas>();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    // Limpiar providers por defecto
                    logging.ClearProviders();
                    
                    // Agregar consola solo en desarrollo
                    if (hostContext.HostingEnvironment.IsDevelopment() || Environment.UserInteractive)
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Warning); // Solo warnings y errores
                    }
                    
                    // Agregar Event Log para producción (Windows Event Viewer)
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Environment.UserInteractive)
                    {
                        logging.AddEventLog(settings =>
                        {
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                settings.SourceName = "ServicioPLC";
                            }
                        });
                    }
                });

        private static void MostrarBannerConsola()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                                ║");
            Console.WriteLine("║                          🏭 SERVICIO PLC - TCP/IP                              ║");
            Console.WriteLine("║                          Worker Service .NET 8.0                               ║");
            Console.WriteLine("║                                                                                ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════╝");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            
            var modo = Environment.UserInteractive ? "CONSOLA (Debug)" : "SERVICIO WINDOWS";
            Console.WriteLine($"  Modo: {modo}");
            Console.WriteLine($"  Fecha: {DateTime.Now:dddd, dd MMMM yyyy HH:mm:ss}");
            
            if (Environment.UserInteractive)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  💡 Presiona Ctrl+C para detener el servicio");
                Console.ForegroundColor = ConsoleColor.White;
            }
            
            Console.WriteLine();
            Console.WriteLine(new string('─', 84));
            Console.WriteLine();
        }

        private static void MostrarErrorCritico(string titulo, string mensaje, string detalle = null)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ ✗✗✗ {titulo.ToUpper().PadRight(73)} ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════╝");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine($"  Mensaje: {mensaje}");
            
            if (!string.IsNullOrEmpty(detalle))
            {
                Console.WriteLine($"  Detalle: {detalle}");
            }
            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  📋 Revisa los logs en: Logs/servicio_plc.log");
            Console.WriteLine("  📋 Revisa los eventos en: Logs/eventos.log");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            
            // Log en archivo
            try
            {
                var logger = new Logger();
                logger.Error($"Error crítico al iniciar servicio: {titulo}", 
                    new Exception($"{mensaje} | {detalle}"));
            }
            catch { }
            
            if (Environment.UserInteractive)
            {
                Console.WriteLine("  Presiona cualquier tecla para salir...");
                Console.ReadKey();
            }
        }
    }
}