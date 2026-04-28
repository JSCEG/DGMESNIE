namespace NSIE.Models
{
    public class CalificacionFinal_EF
    {


        public int ID { get; set; }
        public int EF_ID { get; set; }
        public string EF_Nombre { get; set; }
        public int MPO_ID { get; set; }
        public string Municipio_Nombre { get; set; }
        public int Documentos_completos { get; set; }
        public int Analisis_riesgo { get; set; }
        public string Documentos_completos_text { get; set; }
        public string Analisis_riesgo_text { get; set; }
        public float X_Geo { get; set; }
        public float Y_Geo { get; set; }
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
        public int SumaTotal { get; set; }
        public int TotalColumnas { get; set; }
        public decimal Factor { get; set; }
        public string Resultado { get; set; }
        public decimal Umbral_P1 { get; set; }
        public string ColumnasSeleccionadas_P1 { get; set; }
        //Faltantes
        public string Turno { get; set; }
        public string Marca_solicitada { get; set; }
        public string Expediente { get; set; }
        public string Razon_social { get; set; }



        //EXPENDIOS AUTORIZADOS

        public string NumeroPermiso { get; set; }
        // public int EF_ID { get; set; }
        // public int MPO_ID { get; set; }
        public string Razón_social { get; set; }
        public string RazónSocial { get; set; }
        public DateTime FechaOtorgamiento { get; set; }
        public decimal Latitud_GEO { get; set; }
        public decimal Longitud_GEO { get; set; }
        public string Calle_num_ES { get; set; }
        public string Colonia_ES { get; set; }
        public string Codigo_Postal { get; set; }
        public string Estatus { get; set; }
        public string RFC { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string Estatus_instalacion { get; set; }
        public string Causa_suspension { get; set; }
        public string Numero_de_Expediente { get; set; }
        public string Marca { get; set; }
        public string Comentarios { get; set; }


        //Resultado de Municipios

        public int _2023_01_m { get; set; }
        public int _2023_02_m { get; set; }
        public int _2023_03_m { get; set; }
        public int _2023_04_m { get; set; }
        public int _2023_05_m { get; set; }
        public int _2023_06_m { get; set; }
        public int _2023_08_m { get; set; }

        //      public double Factor { get; set; }

        public int SumaTotalMunicipio { get; set; }
        public int TotalColumnasMunicipio { get; set; }
        public double FactorMunicipio { get; set; }
        public string ResultadoMunicipio { get; set; }
        public decimal UmbralMunicipio_P1 { get; set; }
        public string ColumnasSeleccionadasMunicipio_P1 { get; set; }


        //


    }
}
