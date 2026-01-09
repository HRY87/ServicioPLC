using System;
using Microsoft.Data.SqlClient; // ✅ .NET Core compatible
using System.Threading;
using System.Threading.Tasks;
using PLCServicio.Configuracion;
using PLCServicio.Modelos;
using PLCServicio.Utilidades;
using PLCServicio.Eventos;
using PLCServicio.Mensajes;
using System.Collections.Generic;

namespace PLCServicio.Servicios
{
    public class RepositorioDatos : IDisposable
    {
        private readonly ConfiguracionDatabases _configuracion;
        private readonly Logger _logger;
        private bool _bdLocalDisponible = true;
        private bool _bdCloudDisponible = true;
        private DateTime _ultimoIntentoBdLocal = DateTime.MinValue;
        private DateTime _ultimoIntentoBdCloud = DateTime.MinValue;
        private readonly TimeSpan _intervaloReintentoConexion = TimeSpan.FromMinutes(1);

        public RepositorioDatos(ConfiguracionDatabases configuracion, Logger logger)
        {
            _configuracion = configuracion ?? throw new ArgumentNullException(nameof(configuracion));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            VerificarConexiones();
        }

        private void VerificarConexiones()
        {
            if (_configuracion.Local?.Enabled == true)
            {
                _bdLocalDisponible = VerificarConexion(_configuracion.Local.ConnectionString, "Local");
            }

            if (_configuracion.Cloud?.Enabled == true)
            {
                _bdCloudDisponible = VerificarConexion(_configuracion.Cloud.ConnectionString, "Cloud");
            }
        }

        private bool VerificarConexion(string connectionString, string nombreBd)
        {
            try
            {
                using var conexion = new SqlConnection(connectionString);
                conexion.Open();
                _logger.Informacion($"✓ Conexión a BD {nombreBd} verificada correctamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"✗ Error al verificar conexión a BD {nombreBd}", ex);
                GestorEventos.RegistrarEvento(TiposEvento.BaseDatosError, 
                    $"BD {nombreBd} no disponible: {ex.Message}");
                return false;
            }
        }

        public async Task GuardarLecturaAsync(LecturaPlc lectura, CancellationToken cancellationToken = default)
        {
            if (lectura == null)
            {
                throw new ArgumentNullException(nameof(lectura));
            }

            var tareas = new List<Task>();

            // Guardar en BD Local
            if (_configuracion.Local?.Enabled == true)
            {
                tareas.Add(GuardarEnBdAsync(lectura, _configuracion.Local, "Local", cancellationToken));
            }

            // Guardar en BD Cloud
            if (_configuracion.Cloud?.Enabled == true)
            {
                tareas.Add(GuardarEnBdAsync(lectura, _configuracion.Cloud, "Cloud", cancellationToken));
            }

            // No bloqueamos, las tareas se ejecutan en paralelo
            await Task.WhenAll(tareas).ConfigureAwait(false);
        }

        private async Task GuardarEnBdAsync(
            LecturaPlc lectura, 
            ConfiguracionBd config, 
            string nombreBd,
            CancellationToken cancellationToken)
        {
            bool bdDisponible = nombreBd == "Local" ? _bdLocalDisponible : _bdCloudDisponible;
            DateTime ultimoIntento = nombreBd == "Local" ? _ultimoIntentoBdLocal : _ultimoIntentoBdCloud;

            // Si la BD no está disponible, verificar si es momento de reintentar
            if (!bdDisponible)
            {
                if (DateTime.Now - ultimoIntento < _intervaloReintentoConexion)
                {
                    return; // No reintentar aún
                }
                
                // Intentar reconectar
                bdDisponible = VerificarConexion(config.ConnectionString, nombreBd);
                
                if (nombreBd == "Local")
                    _ultimoIntentoBdLocal = DateTime.Now;
                else
                    _ultimoIntentoBdCloud = DateTime.Now;
                
                if (!bdDisponible)
                {
                    return; // Sigue sin funcionar
                }
                
                GestorMensajes.MostrarExito($"✓ BD {nombreBd} reconectada correctamente");
            }

            try
            {
                await EjecutarInsertAsync(lectura, config.ConnectionString, cancellationToken);
            }
            catch (SqlException ex)
            {
                if (nombreBd == "Local")
                {
                    _bdLocalDisponible = false;
                    _ultimoIntentoBdLocal = DateTime.Now;
                }
                else
                {
                    _bdCloudDisponible = false;
                    _ultimoIntentoBdCloud = DateTime.Now;
                }
                
                _logger.Error($"Error al guardar en BD {nombreBd}", ex);
                
                GestorEventos.RegistrarEvento(TiposEvento.BaseDatosError, 
                    $"Error al guardar en BD {nombreBd}: {ex.Message}");
                
                GestorMensajes.MostrarError($"✗ BD {nombreBd} fuera de servicio");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error inesperado al guardar en BD {nombreBd}", ex);
            }
        }

        private async Task EjecutarInsertAsync(LecturaPlc lectura, string connectionString, CancellationToken cancellationToken)
        {
            const string query = @"
                INSERT INTO LecturasPLC 
                (PlcId, NombrePlc, FechaHoraLectura, KgHoraActual, EspesorActual, 
                 VelocidadLinea, TemperaturaProceso, ContadorProduccion, EstadoMaquina)
                VALUES 
                (@PlcId, @NombrePlc, @FechaHoraLectura, @KgHoraActual, @EspesorActual, 
                 @VelocidadLinea, @TemperaturaProceso, @ContadorProduccion, @EstadoMaquina)";

            using var conexion = new SqlConnection(connectionString);
            await conexion.OpenAsync(cancellationToken);

            using var comando = new SqlCommand(query, conexion);
            comando.Parameters.AddWithValue("@PlcId", lectura.PlcId);
            comando.Parameters.AddWithValue("@NombrePlc", lectura.NombrePlc);
            comando.Parameters.AddWithValue("@FechaHoraLectura", lectura.FechaHoraLectura);
            comando.Parameters.AddWithValue("@KgHoraActual", lectura.KgHoraActual);
            comando.Parameters.AddWithValue("@EspesorActual", lectura.EspesorActual);
            comando.Parameters.AddWithValue("@VelocidadLinea", lectura.VelocidadLinea);
            comando.Parameters.AddWithValue("@TemperaturaProceso", lectura.TemperaturaProceso);
            comando.Parameters.AddWithValue("@ContadorProduccion", lectura.ContadorProduccion);
            comando.Parameters.AddWithValue("@EstadoMaquina", lectura.EstadoMaquina);

            await comando.ExecuteNonQueryAsync(cancellationToken);
        }

        public void Dispose()
        {
            _logger.Informacion("Repositorio de datos liberado");
        }
    }
}