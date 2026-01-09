using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace PLCServicio.Configuracion
{
    public class ConfiguracionManager
    {
        public int IntervaloLecturaSegundos { get; set; }
        public int IntervaloReconexionSegundos { get; set; }
        public int TimeoutLecturaSegundos { get; set; }
        public int MaximoReintentos { get; set; }
        public ConfiguracionDatabases Databases { get; set; }
        public List<ConfiguracionPlc> Plcs { get; set; }
        public Dictionary<string, MapeoDato> MapeosDatos { get; set; }

        public static ConfiguracionManager CargarConfiguracion(string rutaArchivo = "Configuracion/appsettings.json")
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                {
                    throw new FileNotFoundException($"No se encontró el archivo de configuración: {rutaArchivo}");
                }

                var json = File.ReadAllText(rutaArchivo);
                var opciones = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var configuracion = JsonSerializer.Deserialize<ConfiguracionManager>(json, opciones);
                
                if (configuracion == null)
                {
                    throw new InvalidOperationException("Error al deserializar la configuración");
                }

                ValidarConfiguracion(configuracion);
                return configuracion;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al cargar la configuración: {ex.Message}", ex);
            }
        }

        private static void ValidarConfiguracion(ConfiguracionManager config)
        {
            if (config.IntervaloLecturaSegundos <= 0)
                throw new ArgumentException("IntervaloLecturaSegundos debe ser mayor a 0");

            if (config.IntervaloReconexionSegundos <= 0)
                throw new ArgumentException("IntervaloReconexionSegundos debe ser mayor a 0");

            if (config.Plcs == null || config.Plcs.Count == 0)
                throw new ArgumentException("Debe configurar al menos un PLC");

            if (config.MapeosDatos == null || config.MapeosDatos.Count == 0)
                throw new ArgumentException("Debe configurar al menos un mapeo de datos");
        }
    }

    public class ConfiguracionDatabases
    {
        public ConfiguracionBd Local { get; set; }
        public ConfiguracionBd Cloud { get; set; }
    }

    public class ConfiguracionBd
    {
        public bool Enabled { get; set; }
        public string ConnectionString { get; set; }
    }

    public class ConfiguracionPlc
    {
        public string Nombre { get; set; }
        public string Ip { get; set; }
        public int Puerto { get; set; }
        public int Id { get; set; }
        public bool Habilitada { get; set; }
        public string TipoConexion { get; set; }
        public string Descripcion { get; set; }
    }

    public class MapeoDato
    {
        public int Posicion { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
    }
}
