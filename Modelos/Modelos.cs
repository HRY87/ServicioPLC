using System;

namespace PLCServicio.Modelos
{
    public class LecturaPlc
    {
        public int PlcId { get; set; }
        public string NombrePlc { get; set; }
        public DateTime FechaHoraLectura { get; set; }
        public DatosProduccion Produccion { get; set; }
        public LecturaPlc()
        {
            FechaHoraLectura = DateTime.Now;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"╔════════════════════════ [{NombrePlc}] {FechaHoraLectura:yyyy-MM-dd HH:mm:ss} ════════════════════════╗");
            
            // PRODUCCIÓN ACTUAL vs PROGRAMADO
            sb.AppendLine($"║ PRODUCCIÓN ACTUAL");
            sb.AppendLine($"║  KgHora: {Produccion.KgHoraActual,8:F2} kg/h | Programado: {Produccion.KgHoraProgramado,8:F2} kg/h");
            sb.AppendLine($"║  Velocidad: {Produccion.MetrosPorMinActual,8:F2} m/min | Programado: {Produccion.MetrosPorMinProgramado,8:F2} m/min");
            sb.AppendLine($"║  Espesor: {Produccion.EspesorActual,8:F2} mm | Programado: {Produccion.EspesorProgramado,8:F2} mm");
            sb.AppendLine($"║  Ancho Bruto: {Produccion.AnchoBrutoActual,8:F2} mm | Programado: {Produccion.AnchoBrutoProgramado,8:F2} mm");
            sb.AppendLine($"║  Ancho Neto: {Produccion.AnchoNetoActual,8:F2} mm | Programado: {Produccion.AnchoNetoProgramado,8:F2} mm");
            sb.AppendLine($"║  Gramaje: {Produccion.GramajeActual,8:F2} g/m² | Programado: {Produccion.GramajeProgramado,8:F2} g/m²");
            
            // OP / PRODUCCIÓN
            sb.AppendLine($"║");
            sb.AppendLine($"║ PRODUCCIÓN OP {Produccion.NumeroOP}");
            sb.AppendLine($"║  Kg Producidos: {Produccion.KgProducidos,12:F2} kg | Metros: {Produccion.MetrosProducidos,12:F2} m");
            sb.AppendLine($"║  Tamaño Bobina: {Produccion.TamanoBobinaOP,12:F2} | Consumo Total: {Produccion.ConsumoTotalOP,12:F2} kW");
            
            // ROSCA A
            sb.AppendLine($"║");
            sb.AppendLine($"║ ROSCA A - ACTUAL");
            sb.AppendLine($"║  KgHora: {Produccion.RoscaA_KgHoraActual,8:F2} | Porcentaje: {Produccion.RoscaA_PorcentajeActual,8:F2}%");
            sb.AppendLine($"║  Grama/Metro: {Produccion.RoscaA_GramaMetroActual,8:F2} | Espesor: {Produccion.RoscaA_EspesorActual,8:F2} mm");
            
            // SILOS ACTUALES Y TOTALIZADORES
            sb.AppendLine($"║");
            sb.AppendLine($"║ SILOS - ACTUAL | TOTAL");
            sb.AppendLine($"║  Silo 1: {Produccion.RoscaA_Silo1Actual,8:F2} | Total: {Produccion.RoscaA_TotalSilo1,12:F2} | Densidad: {Produccion.RoscaA_DensidadSilo1,6:F2}");
            sb.AppendLine($"║  Silo 2: {Produccion.RoscaA_Silo2Actual,8:F2} | Total: {Produccion.RoscaA_TotalSilo2,12:F2} | Densidad: {Produccion.RoscaA_DensidadSilo2,6:F2}");
            sb.AppendLine($"║  Silo 3: {Produccion.RoscaA_Silo3Actual,8:F2} | Total: {Produccion.RoscaA_TotalSilo3,12:F2} | Densidad: {Produccion.RoscaA_DensidadSilo3,6:F2}");
            sb.AppendLine($"║  Silo 4: {Produccion.RoscaA_Silo4Actual,8:F2} | Total: {Produccion.RoscaA_TotalSilo4,12:F2} | Densidad: {Produccion.RoscaA_DensidadSilo4,6:F2}");
            sb.AppendLine($"║  Silo 5: {Produccion.RoscaA_Silo5Actual,8:F2} | Total: {Produccion.RoscaA_TotalSilo5,12:F2} | Densidad: {Produccion.RoscaA_DensidadSilo5,6:F2}");
            sb.AppendLine($"║  Silo 6: {Produccion.RoscaA_Silo6Actual,8:F2} | Total: {Produccion.RoscaA_TotalSilo6,12:F2} | Densidad: {Produccion.RoscaA_DensidadSilo6,6:F2}");
            
            // CONSUMO
            sb.AppendLine($"║");
            sb.AppendLine($"║ CONSUMO ELÉCTRICO");
            sb.AppendLine($"║  Amperaje L1: {Produccion.AmperesL1,8:F2} A | Consumo Actual: {Produccion.ConsumoActualKW,8:F2} kW");
            
            sb.AppendLine($"╚═══════════════════════════════════════════════════════════════════════════════════════════════════════╝");
            
            return sb.ToString();
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
