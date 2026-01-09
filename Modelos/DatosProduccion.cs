using System;

namespace PLCServicio.Modelos
{
    public class DatosProduccion
    {

        // =======================
        // ENERGÍA / CONSUMO
        // =======================
        public const int ADDR_TENSION_L1 = 100;
        public const int ADDR_TENSION_L2 = 102;
        public const int ADDR_TENSION_L3 = 104;

        public const int ADDR_AMPERES_L1 = 112;
        public const int ADDR_AMPERES_L2 = 114;
        public const int ADDR_AMPERES_L3 = 116;

        public const int ADDR_CONSUMO_ACTUAL_KW = 130;
        public const int ADDR_KW_TOTAL = 136;
        public const int ADDR_KW_POR_KG = 140;
        public const int ADDR_KW_DIA = 154;

        // =======================
        // PRODUCCIÓN
        // =======================
        public const int ADDR_KG_HORA_PROGRAMADO = 517;
        public const int ADDR_ESPESOR_PROGRAMADO = 519;
        public const int ADDR_ANCHO_BRUTO_PROGRAMADO = 521;
        public const int ADDR_ANCHO_NETO_PROGRAMADO = 523;
        public const int ADDR_GRAMAJE_PROGRAMADO = 527;
        public const int ADDR_METROS_POR_MIN_PROGRAMADO = 531;
        
        public const int ADDR_KG_HORA_ACTUAL = 800;
        public const int ADDR_ESPESOR_ACTUAL = 802;
        public const int ADDR_ANCHO_BRUTO_ACTUAL = 804;
        public const int ADDR_GRAMAJE_ACTUAL = 806;
        public const int ADDR_ANCHO_NETO_ACTUAL = 808;
        public const int ADDR_METROS_POR_MIN_ACTUAL = 810;
        
        // ======================= 
        // ROSCA A – PROGRAMADO
        // =======================
        public const int ADDR_ROSCA_A_GRAMA_METRO_PROG = 533;
        public const int ADDR_ROSCA_A_ESPESOR_PROG = 537;
        public const int ADDR_ROSCA_A_PORCENTAJE_PROG = 539;
        public const int ADDR_ROSCA_A_KG_HORA_PROG = 541;
        public const int ADDR_ROSCA_A_SILO1_PROG = 543;
        public const int ADDR_ROSCA_A_SILO2_PROG = 545;
        public const int ADDR_ROSCA_A_SILO3_PROG = 547;
        public const int ADDR_ROSCA_A_SILO4_PROG = 549;
        public const int ADDR_ROSCA_A_SILO5_PROG = 551;
        public const int ADDR_ROSCA_A_SILO6_PROG = 553;
        
        public const int ADDR_ROSCA_A_GRAMA_METRO_ACTUAL = 812;
        public const int ADDR_ROSCA_A_ESPESOR_ACTUAL = 816;
        public const int ADDR_ROSCA_A_PORCENTAJE_ACTUAL = 818;
        public const int ADDR_ROSCA_A_KG_HORA_ACTUAL = 820;
        public const int ADDR_ROSCA_A_SILO1_ACTUAL = 822;
        public const int ADDR_ROSCA_A_SILO2_ACTUAL = 824;
        public const int ADDR_ROSCA_A_SILO3_ACTUAL = 826;
        public const int ADDR_ROSCA_A_SILO4_ACTUAL = 828;
        public const int ADDR_ROSCA_A_SILO5_ACTUAL = 830;
        public const int ADDR_ROSCA_A_SILO6_ACTUAL = 832;
        
        public const int ADDR_ROSCA_A_DENSIDAD_SILO1 = 555;
        public const int ADDR_ROSCA_A_DENSIDAD_SILO2 = 557;
        public const int ADDR_ROSCA_A_DENSIDAD_SILO3 = 559;
        public const int ADDR_ROSCA_A_DENSIDAD_SILO4 = 561;
        public const int ADDR_ROSCA_A_DENSIDAD_SILO5 = 563;
        public const int ADDR_ROSCA_A_DENSIDAD_SILO6 = 565;
        
        public const int ADDR_ROSCA_A_TOTAL_SILO1 = 967;
        public const int ADDR_ROSCA_A_TOTAL_SILO2 = 969;
        public const int ADDR_ROSCA_A_TOTAL_SILO3 = 971;
        public const int ADDR_ROSCA_A_TOTAL_SILO4 = 973;
        public const int ADDR_ROSCA_A_TOTAL_SILO5 = 975;
        public const int ADDR_ROSCA_A_TOTAL_SILO6 = 977;
        
        // =======================
        // ROSCA B – PROGRAMADO
        // =======================
        public const int ADDR_ROSCA_B_GRAMA_METRO_PROG = 567;
        public const int ADDR_ROSCA_B_ESPESOR_PROG = 571;
        public const int ADDR_ROSCA_B_PORCENTAJE_PROG = 573;
        public const int ADDR_ROSCA_B_KG_HORA_PROG = 575;
        public const int ADDR_ROSCA_B_SILO1_PROG = 577;
        public const int ADDR_ROSCA_B_SILO2_PROG = 579;
        public const int ADDR_ROSCA_B_SILO3_PROG = 581;
        public const int ADDR_ROSCA_B_SILO4_PROG = 583;
        public const int ADDR_ROSCA_B_SILO5_PROG = 585;
        public const int ADDR_ROSCA_B_SILO6_PROG = 587;
        public const int ADDR_ROSCA_B_GL_PROG = 589; // ⚠️ confirmar significado

        // =======================
        // ROSCA B – ACTUAL
        // =======================
        public const int ADDR_ROSCA_B_GRAMA_METRO_ACTUAL = 846;
        public const int ADDR_ROSCA_B_ESPESOR_ACTUAL = 850;
        public const int ADDR_ROSCA_B_PORCENTAJE_ACTUAL = 852;
        public const int ADDR_ROSCA_B_KG_HORA_ACTUAL = 854;
        public const int ADDR_ROSCA_B_SILO1_ACTUAL = 856;
        public const int ADDR_ROSCA_B_SILO2_ACTUAL = 858;
        public const int ADDR_ROSCA_B_SILO3_ACTUAL = 860;
        public const int ADDR_ROSCA_B_SILO4_ACTUAL = 862;
        public const int ADDR_ROSCA_B_SILO5_ACTUAL = 864;
        public const int ADDR_ROSCA_B_SILO6_ACTUAL = 866;

        // =======================
        // ROSCA B – DENSIDADES
        // =======================
        public const int ADDR_ROSCA_B_DENSIDAD_SILO1 = 589; // ⚠️ solapado con GL
        public const int ADDR_ROSCA_B_DENSIDAD_SILO2 = 591;
        public const int ADDR_ROSCA_B_DENSIDAD_SILO3 = 593;
        public const int ADDR_ROSCA_B_DENSIDAD_SILO4 = 595;
        public const int ADDR_ROSCA_B_DENSIDAD_SILO5 = 597;
        public const int ADDR_ROSCA_B_DENSIDAD_SILO6 = 599;

        // =======================
        // ROSCA B – TOTALIZADORES
        // =======================
        public const int ADDR_ROSCA_B_TOTAL_SILO1 = 967;
        public const int ADDR_ROSCA_B_TOTAL_SILO2 = 969;
        public const int ADDR_ROSCA_B_TOTAL_SILO3 = 971;
        public const int ADDR_ROSCA_B_TOTAL_SILO4 = 973;
        public const int ADDR_ROSCA_B_TOTAL_SILO5 = 975;
        public const int ADDR_ROSCA_B_TOTAL_SILO6 = 977;

        // =======================
        // ROSCA C – PROGRAMADO
        // =======================
        public const int ADDR_ROSCA_C_GRAMA_METRO_PROG = 601;
        public const int ADDR_ROSCA_C_ESPESOR_PROG = 605;
        public const int ADDR_ROSCA_C_PORCENTAJE_PROG = 607;
        public const int ADDR_ROSCA_C_KG_HORA_PROG = 609;
        public const int ADDR_ROSCA_C_SILO1_PROG = 611;
        public const int ADDR_ROSCA_C_SILO2_PROG = 613;
        public const int ADDR_ROSCA_C_SILO3_PROG = 615;
        public const int ADDR_ROSCA_C_SILO4_PROG = 617;
        public const int ADDR_ROSCA_C_SILO5_PROG = 619;
        public const int ADDR_ROSCA_C_SILO6_PROG = 621;
        public const int ADDR_ROSCA_C_GL_PROG = 623; // ⚠️ confirmar significado

        // =======================
        // ROSCA C – ACTUAL
        // =======================
        public const int ADDR_ROSCA_C_GRAMA_METRO_ACTUAL = 880;
        public const int ADDR_ROSCA_C_ESPESOR_ACTUAL = 884;
        public const int ADDR_ROSCA_C_PORCENTAJE_ACTUAL = 886;
        public const int ADDR_ROSCA_C_KG_HORA_ACTUAL = 888;
        public const int ADDR_ROSCA_C_SILO1_ACTUAL = 856; // ⚠️ solapado con Rosca B
        public const int ADDR_ROSCA_C_SILO2_ACTUAL = 858;
        public const int ADDR_ROSCA_C_SILO3_ACTUAL = 860;
        public const int ADDR_ROSCA_C_SILO4_ACTUAL = 862;
        public const int ADDR_ROSCA_C_SILO5_ACTUAL = 864;
        public const int ADDR_ROSCA_C_SILO6_ACTUAL = 866;

        // =======================
        // ROSCA C – DENSIDADES
        // =======================
        public const int ADDR_ROSCA_C_DENSIDAD_SILO1 = 623;
        public const int ADDR_ROSCA_C_DENSIDAD_SILO2 = 625;
        public const int ADDR_ROSCA_C_DENSIDAD_SILO3 = 627;
        public const int ADDR_ROSCA_C_DENSIDAD_SILO4 = 629;
        public const int ADDR_ROSCA_C_DENSIDAD_SILO5 = 631;
        public const int ADDR_ROSCA_C_DENSIDAD_SILO6 = 633;

        // =======================
        // ROSCA C – TOTALIZADORES
        // =======================
        public const int ADDR_ROSCA_C_TOTAL_SILO1 = 967; // ⚠️ compartido con Rosca A/B
        public const int ADDR_ROSCA_C_TOTAL_SILO2 = 969;
        public const int ADDR_ROSCA_C_TOTAL_SILO3 = 971;
        public const int ADDR_ROSCA_C_TOTAL_SILO4 = 973;
        public const int ADDR_ROSCA_C_TOTAL_SILO5 = 975;
        public const int ADDR_ROSCA_C_TOTAL_SILO6 = 977;

        // =======================
        // ROSCA D – PROGRAMADO
        // =======================
        public const int ADDR_ROSCA_D_GRAMA_METRO_PROG = 635;
        public const int ADDR_ROSCA_D_ESPESOR_PROG = 639;
        public const int ADDR_ROSCA_D_PORCENTAJE_PROG = 641;
        public const int ADDR_ROSCA_D_KG_HORA_PROG = 643;
        public const int ADDR_ROSCA_D_SILO1_PROG = 645;
        public const int ADDR_ROSCA_D_SILO2_PROG = 647;
        public const int ADDR_ROSCA_D_SILO3_PROG = 649;
        public const int ADDR_ROSCA_D_SILO4_PROG = 651;
        public const int ADDR_ROSCA_D_SILO5_PROG = 653;
        public const int ADDR_ROSCA_D_SILO6_PROG = 655;
        public const int ADDR_ROSCA_D_GL_PROG = 657; // ⚠️ confirmar significado

        // =======================
        // ROSCA D – ACTUAL
        // =======================
        public const int ADDR_ROSCA_D_GRAMA_METRO_ACTUAL = 1027;
        public const int ADDR_ROSCA_D_ESPESOR_ACTUAL = 1031;
        public const int ADDR_ROSCA_D_PORCENTAJE_ACTUAL = 1033;
        public const int ADDR_ROSCA_D_KG_HORA_ACTUAL = 1035;
        public const int ADDR_ROSCA_D_SILO1_ACTUAL = 1037;
        public const int ADDR_ROSCA_D_SILO2_ACTUAL = 1039;
        public const int ADDR_ROSCA_D_SILO3_ACTUAL = 1041;
        public const int ADDR_ROSCA_D_SILO4_ACTUAL = 1043;
        public const int ADDR_ROSCA_D_SILO5_ACTUAL = 1045;
        public const int ADDR_ROSCA_D_SILO6_ACTUAL = 1047;

        // =======================
        // ROSCA D – DENSIDADES
        // =======================
        public const int ADDR_ROSCA_D_DENSIDAD_SILO1 = 657;
        public const int ADDR_ROSCA_D_DENSIDAD_SILO2 = 659;
        public const int ADDR_ROSCA_D_DENSIDAD_SILO3 = 661;
        public const int ADDR_ROSCA_D_DENSIDAD_SILO4 = 663;
        public const int ADDR_ROSCA_D_DENSIDAD_SILO5 = 665;
        public const int ADDR_ROSCA_D_DENSIDAD_SILO6 = 667;

        // =======================
        // ROSCA D – TOTALIZADORES
        // =======================
        public const int ADDR_ROSCA_D_TOTAL_SILO1 = 1003;
        public const int ADDR_ROSCA_D_TOTAL_SILO2 = 1005;
        public const int ADDR_ROSCA_D_TOTAL_SILO3 = 1007;
        public const int ADDR_ROSCA_D_TOTAL_SILO4 = 1009;
        public const int ADDR_ROSCA_D_TOTAL_SILO5 = 1011;
        public const int ADDR_ROSCA_D_TOTAL_SILO6 = 1013;

        // =======================
        // ROSCA E – PROGRAMADO
        // =======================
        public const int ADDR_ROSCA_E_GRAMA_METRO_PROG = 669;
        public const int ADDR_ROSCA_E_ESPESOR_PROG = 673;
        public const int ADDR_ROSCA_E_PORCENTAJE_PROG = 675;
        public const int ADDR_ROSCA_E_KG_HORA_PROG = 677;
        public const int ADDR_ROSCA_E_SILO1_PROG = 679;
        public const int ADDR_ROSCA_E_SILO2_PROG = 681;
        public const int ADDR_ROSCA_E_SILO3_PROG = 683;
        public const int ADDR_ROSCA_E_SILO4_PROG = 685;
        public const int ADDR_ROSCA_E_SILO5_PROG = 687;
        public const int ADDR_ROSCA_E_SILO6_PROG = 689;
        public const int ADDR_ROSCA_E_GL_PROG = 691; // ⚠️ confirmar significado

        // =======================
        // ROSCA E – ACTUAL
        // =======================
        public const int ADDR_ROSCA_E_GRAMA_METRO_ACTUAL = 1061;
        public const int ADDR_ROSCA_E_ESPESOR_ACTUAL = 1065;
        public const int ADDR_ROSCA_E_PORCENTAJE_ACTUAL = 1067;
        public const int ADDR_ROSCA_E_KG_HORA_ACTUAL = 1069;
        public const int ADDR_ROSCA_E_SILO1_ACTUAL = 1071;
        public const int ADDR_ROSCA_E_SILO2_ACTUAL = 1073;
        public const int ADDR_ROSCA_E_SILO3_ACTUAL = 1075;
        public const int ADDR_ROSCA_E_SILO4_ACTUAL = 1077;
        public const int ADDR_ROSCA_E_SILO5_ACTUAL = 1079;
        public const int ADDR_ROSCA_E_SILO6_ACTUAL = 1081;

        // =======================
        // ROSCA E – DENSIDADES
        // =======================
        public const int ADDR_ROSCA_E_DENSIDAD_SILO1 = 691;
        public const int ADDR_ROSCA_E_DENSIDAD_SILO2 = 693;
        public const int ADDR_ROSCA_E_DENSIDAD_SILO3 = 695;
        public const int ADDR_ROSCA_E_DENSIDAD_SILO4 = 697;
        public const int ADDR_ROSCA_E_DENSIDAD_SILO5 = 699;
        public const int ADDR_ROSCA_E_DENSIDAD_SILO6 = 701;

        // =======================
        // ROSCA E – TOTALIZADORES
        // =======================
        public const int ADDR_ROSCA_E_TOTAL_SILO1 = 1015;
        public const int ADDR_ROSCA_E_TOTAL_SILO2 = 1017;
        public const int ADDR_ROSCA_E_TOTAL_SILO3 = 1019;
        public const int ADDR_ROSCA_E_TOTAL_SILO4 = 1021;
        public const int ADDR_ROSCA_E_TOTAL_SILO5 = 1023;
        public const int ADDR_ROSCA_E_TOTAL_SILO6 = 1025;

        // =======================
        // OP / PRODUCCIÓN GENERAL
        // =======================
        public const int ADDR_NUMERO_OP = 30000;          // String (16)
        public const int ADDR_ESTADO_OP = 30023;          // Word (entero corto)
        public const int ADDR_KG_POR_METRO_OP = 30017;
        public const int ADDR_TAMANO_BOBINA_OP = 30019;
        public const int ADDR_KG_PRODUCIDOS = 30037;
        public const int ADDR_METROS_PRODUCIDOS = 30053;
        public const int ADDR_CONSUMO_TOTAL_OP = 30059;
        public const int ADDR_RECORTES_OP = 30061;          // Float (confirmar)

       // ==================================================
        // PRODUCCIÓN – PROGRAMADO
        // ==================================================
        public double KgHoraProgramado { get; set; }
        public double EspesorProgramado { get; set; }
        public double AnchoBrutoProgramado { get; set; }
        public double AnchoNetoProgramado { get; set; }
        public double GramajeProgramado { get; set; }
        public double MetrosPorMinProgramado { get; set; }

        // ==================================================
        // PRODUCCIÓN – ACTUAL
        // ==================================================
        public double KgHoraActual { get; set; }
        public double EspesorActual { get; set; }
        public double AnchoBrutoActual { get; set; }
        public double AnchoNetoActual { get; set; }
        public double GramajeActual { get; set; }
        public double MetrosPorMinActual { get; set; }

        // ==================================================
        // ROSCAS – ACTUAL (A..E)
        // ==================================================

        // Rosca A
        public double RoscaA_GramaMetroActual { get; set; }
        public double RoscaA_EspesorActual { get; set; }
        public double RoscaA_PorcentajeActual { get; set; }
        public double RoscaA_KgHoraActual { get; set; }
        public double RoscaA_Silo1Actual { get; set; }
        public double RoscaA_Silo2Actual { get; set; }
        public double RoscaA_Silo3Actual { get; set; }
        public double RoscaA_Silo4Actual { get; set; }
        public double RoscaA_Silo5Actual { get; set; }
        public double RoscaA_Silo6Actual { get; set; }

        // Rosca B
        public double RoscaB_GramaMetroActual { get; set; }
        public double RoscaB_EspesorActual { get; set; }
        public double RoscaB_PorcentajeActual { get; set; }
        public double RoscaB_KgHoraActual { get; set; }
        public double RoscaB_Silo1Actual { get; set; }
        public double RoscaB_Silo2Actual { get; set; }
        public double RoscaB_Silo3Actual { get; set; }
        public double RoscaB_Silo4Actual { get; set; }
        public double RoscaB_Silo5Actual { get; set; }
        public double RoscaB_Silo6Actual { get; set; }

        // Rosca C
        public double RoscaC_GramaMetroActual { get; set; }
        public double RoscaC_EspesorActual { get; set; }
        public double RoscaC_PorcentajeActual { get; set; }
        public double RoscaC_KgHoraActual { get; set; }
        public double RoscaC_Silo1Actual { get; set; }
        public double RoscaC_Silo2Actual { get; set; }
        public double RoscaC_Silo3Actual { get; set; }
        public double RoscaC_Silo4Actual { get; set; }
        public double RoscaC_Silo5Actual { get; set; }
        public double RoscaC_Silo6Actual { get; set; }

        // Rosca D
        public double RoscaD_GramaMetroActual { get; set; }
        public double RoscaD_EspesorActual { get; set; }
        public double RoscaD_PorcentajeActual { get; set; }
        public double RoscaD_KgHoraActual { get; set; }
        public double RoscaD_Silo1Actual { get; set; }
        public double RoscaD_Silo2Actual { get; set; }
        public double RoscaD_Silo3Actual { get; set; }
        public double RoscaD_Silo4Actual { get; set; }
        public double RoscaD_Silo5Actual { get; set; }
        public double RoscaD_Silo6Actual { get; set; }

        // Rosca E
        public double RoscaE_GramaMetroActual { get; set; }
        public double RoscaE_EspesorActual { get; set; }
        public double RoscaE_PorcentajeActual { get; set; }
        public double RoscaE_KgHoraActual { get; set; }
        public double RoscaE_Silo1Actual { get; set; }
        public double RoscaE_Silo2Actual { get; set; }
        public double RoscaE_Silo3Actual { get; set; }
        public double RoscaE_Silo4Actual { get; set; }
        public double RoscaE_Silo5Actual { get; set; }
        public double RoscaE_Silo6Actual { get; set; }

        // ==================================================
        // ROSCAS – PROGRAMADO (A..E)
        // ==================================================
        // Rosca A programado
        public double RoscaA_GramaMetroProgramado { get; set; }
        public double RoscaA_EspesorProgramado { get; set; }
        public double RoscaA_PorcentajeProgramado { get; set; }
        public double RoscaA_KgHoraProgramado { get; set; }
        public double RoscaA_Silo1Programado { get; set; }
        public double RoscaA_Silo2Programado { get; set; }
        public double RoscaA_Silo3Programado { get; set; }
        public double RoscaA_Silo4Programado { get; set; }
        public double RoscaA_Silo5Programado { get; set; }
        public double RoscaA_Silo6Programado { get; set; }

        // Rosca B programado
        public double RoscaB_GramaMetroProgramado { get; set; }
        public double RoscaB_EspesorProgramado { get; set; }
        public double RoscaB_PorcentajeProgramado { get; set; }
        public double RoscaB_KgHoraProgramado { get; set; }
        public double RoscaB_Silo1Programado { get; set; }
        public double RoscaB_Silo2Programado { get; set; }
        public double RoscaB_Silo3Programado { get; set; }
        public double RoscaB_Silo4Programado { get; set; }
        public double RoscaB_Silo5Programado { get; set; }
        public double RoscaB_Silo6Programado { get; set; }

        // Rosca C programado
        public double RoscaC_GramaMetroProgramado { get; set; }
        public double RoscaC_EspesorProgramado { get; set; }
        public double RoscaC_PorcentajeProgramado { get; set; }
        public double RoscaC_KgHoraProgramado { get; set; }
        public double RoscaC_Silo1Programado { get; set; }
        public double RoscaC_Silo2Programado { get; set; }
        public double RoscaC_Silo3Programado { get; set; }
        public double RoscaC_Silo4Programado { get; set; }
        public double RoscaC_Silo5Programado { get; set; }
        public double RoscaC_Silo6Programado { get; set; }

        // Rosca D programado
        public double RoscaD_GramaMetroProgramado { get; set; }
        public double RoscaD_EspesorProgramado { get; set; }
        public double RoscaD_PorcentajeProgramado { get; set; }
        public double RoscaD_KgHoraProgramado { get; set; }
        public double RoscaD_Silo1Programado { get; set; }
        public double RoscaD_Silo2Programado { get; set; }
        public double RoscaD_Silo3Programado { get; set; }
        public double RoscaD_Silo4Programado { get; set; }
        public double RoscaD_Silo5Programado { get; set; }
        public double RoscaD_Silo6Programado { get; set; }

        // Rosca E programado
        public double RoscaE_GramaMetroProgramado { get; set; }
        public double RoscaE_EspesorProgramado { get; set; }
        public double RoscaE_PorcentajeProgramado { get; set; }
        public double RoscaE_KgHoraProgramado { get; set; }
        public double RoscaE_Silo1Programado { get; set; }
        public double RoscaE_Silo2Programado { get; set; }
        public double RoscaE_Silo3Programado { get; set; }
        public double RoscaE_Silo4Programado { get; set; }
        public double RoscaE_Silo5Programado { get; set; }
        public double RoscaE_Silo6Programado { get; set; }

        // ==================================================
        // ROSCAS – DENSIDADES (A..E)
        // ==================================================
        public double RoscaA_DensidadSilo1 { get; set; }
        public double RoscaA_DensidadSilo2 { get; set; }
        public double RoscaA_DensidadSilo3 { get; set; }
        public double RoscaA_DensidadSilo4 { get; set; }
        public double RoscaA_DensidadSilo5 { get; set; }
        public double RoscaA_DensidadSilo6 { get; set; }

        public double RoscaB_DensidadSilo1 { get; set; }
        public double RoscaB_DensidadSilo2 { get; set; }
        public double RoscaB_DensidadSilo3 { get; set; }
        public double RoscaB_DensidadSilo4 { get; set; }
        public double RoscaB_DensidadSilo5 { get; set; }
        public double RoscaB_DensidadSilo6 { get; set; }

        public double RoscaC_DensidadSilo1 { get; set; }
        public double RoscaC_DensidadSilo2 { get; set; }
        public double RoscaC_DensidadSilo3 { get; set; }
        public double RoscaC_DensidadSilo4 { get; set; }
        public double RoscaC_DensidadSilo5 { get; set; }
        public double RoscaC_DensidadSilo6 { get; set; }

        public double RoscaD_DensidadSilo1 { get; set; }
        public double RoscaD_DensidadSilo2 { get; set; }
        public double RoscaD_DensidadSilo3 { get; set; }
        public double RoscaD_DensidadSilo4 { get; set; }
        public double RoscaD_DensidadSilo5 { get; set; }
        public double RoscaD_DensidadSilo6 { get; set; }

        public double RoscaE_DensidadSilo1 { get; set; }
        public double RoscaE_DensidadSilo2 { get; set; }
        public double RoscaE_DensidadSilo3 { get; set; }
        public double RoscaE_DensidadSilo4 { get; set; }
        public double RoscaE_DensidadSilo5 { get; set; }
        public double RoscaE_DensidadSilo6 { get; set; }

        // ==================================================
        // ROSCAS – TOTALIZADORES (KG) (A..E)
        // ==================================================
        public double RoscaA_TotalSilo1 { get; set; }
        public double RoscaA_TotalSilo2 { get; set; }
        public double RoscaA_TotalSilo3 { get; set; }
        public double RoscaA_TotalSilo4 { get; set; }
        public double RoscaA_TotalSilo5 { get; set; }
        public double RoscaA_TotalSilo6 { get; set; }

        public double RoscaB_TotalSilo1 { get; set; }
        public double RoscaB_TotalSilo2 { get; set; }
        public double RoscaB_TotalSilo3 { get; set; }
        public double RoscaB_TotalSilo4 { get; set; }
        public double RoscaB_TotalSilo5 { get; set; }
        public double RoscaB_TotalSilo6 { get; set; }

        public double RoscaC_TotalSilo1 { get; set; }
        public double RoscaC_TotalSilo2 { get; set; }
        public double RoscaC_TotalSilo3 { get; set; }
        public double RoscaC_TotalSilo4 { get; set; }
        public double RoscaC_TotalSilo5 { get; set; }
        public double RoscaC_TotalSilo6 { get; set; }

        public double RoscaD_TotalSilo1 { get; set; }
        public double RoscaD_TotalSilo2 { get; set; }
        public double RoscaD_TotalSilo3 { get; set; }
        public double RoscaD_TotalSilo4 { get; set; }
        public double RoscaD_TotalSilo5 { get; set; }
        public double RoscaD_TotalSilo6 { get; set; }

        public double RoscaE_TotalSilo1 { get; set; }
        public double RoscaE_TotalSilo2 { get; set; }
        public double RoscaE_TotalSilo3 { get; set; }
        public double RoscaE_TotalSilo4 { get; set; }
        public double RoscaE_TotalSilo5 { get; set; }
        public double RoscaE_TotalSilo6 { get; set; }

        // ==================================================
        // CONSUMO / ENERGÍA
        // ==================================================
        public double TensionL1 { get; set; }
        public double TensionL2 { get; set; }
        public double TensionL3 { get; set; }
        public double AmperesL1 { get; set; }
        public double AmperesL2 { get; set; }
        public double AmperesL3 { get; set; }
        public double ConsumoActualKW { get; set; }
        public double KWTotal { get; set; }
        public double KWPorKg { get; set; }
        public double KWDia { get; set; }

        // ==================================================
        // OPERACIÓN / PRODUCCIÓN GENERAL (OP)
        // ==================================================
        public string NumeroOP { get; set; }
        public int EstadoOP { get; set; }
        public double KgPorMetroOP { get; set; }
        public double TamanoBobinaOP { get; set; }
        public double RecortesOP { get; set; }

        public double KgProducidos { get; set; }
        public double MetrosProducidos { get; set; }
        public double ConsumoTotalOP { get; set; }

        // ==================================================
        // DATOS DE PEDIDO / MÁQUINA
        // ==================================================
        public string MaquinaOcupada { get; set; }
        public string NombreMaquina { get; set; }
        public string Pedido { get; set; }
        public string PedidoIniciado { get; set; }
        public double PorcentajeB { get; set; }
        public double PorcentajeC { get; set; }
        public string PrevisionTerminar { get; set; }
        public string NombreReceta { get; set; }
        public string EstadoPedido { get; set; }
        public double TamanoBobina { get; set; }
        public string TiempoTotal { get; set; }
    }
}