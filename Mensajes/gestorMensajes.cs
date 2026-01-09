using System;
using PLCServicio.Modelos;

namespace PLCServicio.Mensajes
{
    public static class GestorMensajes
    {
        private static readonly object _lockConsola = new object();
        private static bool _modoVerboso = true;

        public static void ConfigurarModoVerboso(bool verboso)
        {
            _modoVerboso = verboso;
        }

        public static void MostrarInformacion(string mensaje)
        {
            if (!_modoVerboso) return;

            lock (_lockConsola)
            {
                var colorOriginal = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {mensaje}");
                Console.ForegroundColor = colorOriginal;
            }
        }

        public static void MostrarExito(string mensaje)
        {
            if (!_modoVerboso) return;

            lock (_lockConsola)
            {
                var colorOriginal = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[OK] {DateTime.Now:HH:mm:ss} - {mensaje}");
                Console.ForegroundColor = colorOriginal;
            }
        }

        public static void MostrarAdvertencia(string mensaje)
        {
            lock (_lockConsola)
            {
                var colorOriginal = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} - {mensaje}");
                Console.ForegroundColor = colorOriginal;
            }
        }

        public static void MostrarError(string mensaje)
        {
            lock (_lockConsola)
            {
                var colorOriginal = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {mensaje}");
                Console.ForegroundColor = colorOriginal;
            }
        }

        public static void MostrarLectura(LecturaPlc lectura)
        {
            if (!_modoVerboso) return;

            lock (_lockConsola)
            {
                var colorOriginal = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[LECTURA] {DateTime.Now:HH:mm:ss} - {lectura}");
                Console.ForegroundColor = colorOriginal;
            }
        }

        public static void MostrarSeparador()
        {
            if (!_modoVerboso) return;

            lock (_lockConsola)
            {
                Console.WriteLine(new string('-', 80));
            }
        }

        public static void MostrarEncabezado(string titulo)
        {
            lock (_lockConsola)
            {
                var colorOriginal = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine();
                Console.WriteLine(new string('=', 80));
                Console.WriteLine($"  {titulo}");
                Console.WriteLine(new string('=', 80));
                Console.WriteLine();
                Console.ForegroundColor = colorOriginal;
            }
        }

        public static void MostrarEstadisticas(int lecturasExitosas, int totalPlcs, TimeSpan tiempoTranscurrido)
        {
            if (!_modoVerboso) return;

            lock (_lockConsola)
            {
                var colorOriginal = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\n--- Estadísticas ---");
                Console.WriteLine($"Lecturas exitosas: {lecturasExitosas}/{totalPlcs}");
                Console.WriteLine($"Tiempo: {tiempoTranscurrido.TotalSeconds:F2}s");
                Console.WriteLine($"-------------------\n");
                Console.ForegroundColor = colorOriginal;
            }
        }
    }
}
