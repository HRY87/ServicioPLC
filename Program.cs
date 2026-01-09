using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PLCServicio.Servicios;
using PLCServicio.Utilidades;
using PLCServicio.Configuracion;
using System;
using System.Runtime.InteropServices;

namespace PLCServicio
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CrearHostBuilder(args).Build();
                
                // Mostrar información si se ejecuta en modo consola
                if (Environment.UserInteractive)
                {
                    MostrarBannerConsola();
                }
                
                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico al iniciar el servicio: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Log en archivo
                try
                {
                    var logger = new Logger();
                    logger.Error("Error crítico al iniciar servicio", ex);
                }
                catch { }
                
                Environment.ExitCode = 1;
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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         SERVICIO PLC                                           ║");
            Console.WriteLine("║         Worker Service .NET 8.0                                ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine("Modo: " + (Environment.UserInteractive ? "CONSOLA (Debug)" : "SERVICIO WINDOWS"));
            Console.WriteLine("Presiona Ctrl+C para detener...");
            Console.WriteLine();
        }
    }
}