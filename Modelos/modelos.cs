using System;

namespace PLCServicio.Modelos
{
    public class LecturaPlc
    {
        public int PlcId { get; set; }
        public string NombrePlc { get; set; }
        public DateTime FechaHoraLectura { get; set; }
        
        // Datos de producción
        public float KgHoraActual { get; set; }
        public float EspesorActual { get; set; }
        public float VelocidadLinea { get; set; }
        public float TemperaturaProceso { get; set; }
        public int ContadorProduccion { get; set; }
        public bool EstadoMaquina { get; set; }
        
        // 👉 AGREGAR AQUÍ MÁS PROPIEDADES SEGÚN TUS NECESIDADES:
        // public float Presion { get; set; }
        // public float Humedad { get; set; }
        // public int AlarmasActivas { get; set; }
        // public float ConsumoEnergia { get; set; }
        // etc...

        public LecturaPlc()
        {
            FechaHoraLectura = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{NombrePlc}] {FechaHoraLectura:yyyy-MM-dd HH:mm:ss} - " +
                   $"KgHora: {KgHoraActual:F2}, Espesor: {EspesorActual:F2}mm, " +
                   $"Velocidad: {VelocidadLinea:F2}m/min, Temp: {TemperaturaProceso:F1}°C, " +
                   $"Contador: {ContadorProduccion}, Estado: {(EstadoMaquina ? "ON" : "OFF")}";
        }
    }

    public class PlcDato
    {
        public string Nombre { get; set; }
        public int Posicion { get; set; }
        public string Tipo { get; set; }
        public object Valor { get; set; }
        public DateTime FechaHoraLectura { get; set; }

        public PlcDato()
        {
            FechaHoraLectura = DateTime.Now;
        }
    }

    public class EstadoPlc
    {
        public int PlcId { get; set; }
        public string Nombre { get; set; }
        public bool Conectado { get; set; }
        public DateTime UltimaLectura { get; set; }
        public DateTime UltimaConexion { get; set; }
        public int IntentosReconexion { get; set; }
        public string MensajeEstado { get; set; }

        public EstadoPlc()
        {
            Conectado = false;
            IntentosReconexion = 0;
            MensajeEstado = "Inicializando...";
        }
    }
}
