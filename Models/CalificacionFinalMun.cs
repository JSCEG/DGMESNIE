namespace NSIE.Models
{
    public class CalificacionFinalMun
    {
        // Parametros de Envío
        public List<string> Indicadores_Seleccionados { get; set; }
        public string Umbral_Seleccionado { get; set; }

        // Para el Municipio
        public List<string> Indicadores_Seleccionados_Municipio { get; set; }
        public string Umbral_Seleccionado_Municipio { get; set; }

        // Resultado
        public int ID { get; set; }
        public int EF_ID { get; set; }
        public string EF_Nombre { get; set; }
        public int MPO_ID { get; set; }
        public string Municipio_Nombre { get; set; }
        public int Documentos_completos { get; set; }
        public int Analisis_riesgo { get; set; }
        public double X_Geo { get; set; }
        public double Y_Geo { get; set; }
        public int _2023_01 { get; set; }
        public int _2023_02 { get; set; }
        public int _2023_03 { get; set; }
        public int _2023_04 { get; set; }
        public int _2023_05 { get; set; }
        public int _2023_06 { get; set; }
        public int _2023_07 { get; set; }
        public int _2023_08 { get; set; }
        public int _2023_09 { get; set; }
        public int _2023_10 { get; set; }
        public int _2023_11 { get; set; }
        public int _2023_12 { get; set; }
        public int _2023_01_m { get; set; }
        public int _2023_02_m { get; set; }
        public int _2023_03_m { get; set; }
        public int _2023_04_m { get; set; }
        public int _2023_05_m { get; set; }
        public int _2023_06_m { get; set; }
        public int _2023_07_m { get; set; }
        public int _2023_08_m { get; set; }
        public int _2023_09_m { get; set; }
        public int _2023_10_m { get; set; }
        public int _2023_11_m { get; set; }
        public int _2023_12_m { get; set; }
        public int SumaTotal { get; set; }
        public int TotalColumnas { get; set; }
        public string Factor { get; set; }
        public int Resultado { get; set; }
        public double Umbral_P1 { get; set; }
        public string ColumnasSeleccionadas_P1 { get; set; }
        public int SumaTotalMunicipio { get; set; }
        public int TotalColumnasMunicipio { get; set; }
        public string FactorMunicipio { get; set; }
        public int ResultadoMunicipio { get; set; }
        public double UmbralMunicipio_P1 { get; set; }
        public string ColumnasSeleccionadasMunicipio_P1 { get; set; }
    }
}