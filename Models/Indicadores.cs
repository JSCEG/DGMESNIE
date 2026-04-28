namespace NSIE.Models
{
    public class Indicadores
    {
        public IEnumerable<Indicadores> I1;
        public IEnumerable<Indicadores> I2;
        public IEnumerable<Indicadores> I3;
        public IEnumerable<Indicadores> I4;
        public IEnumerable<Indicadores> I5;
        public IEnumerable<Indicadores> I6;
        public IEnumerable<Indicadores> I7;
        public IEnumerable<Indicadores> I8;
        public IEnumerable<Indicadores> I9;
        public IEnumerable<Indicadores> I10;
        public IEnumerable<Indicadores> I11;
        public IEnumerable<Indicadores> I12;
        public IEnumerable<Indicadores> I13;
        public IEnumerable<Indicadores> I14;
        public IEnumerable<Indicadores> I15;
        public IEnumerable<Indicadores> ICM;
        public IEnumerable<Indicadores> IC;


        //Mapas de Colores
        public IEnumerable<Indicadores> MI1;
        public IEnumerable<Indicadores> MI2;
        public IEnumerable<Indicadores> MI3;
        public IEnumerable<Indicadores> MI4;
        public IEnumerable<Indicadores> MI5;
        public IEnumerable<Indicadores> MI6;
        public IEnumerable<Indicadores> MI7;
        public IEnumerable<Indicadores> MI8;
        public IEnumerable<Indicadores> MI9;
        public IEnumerable<Indicadores> MI10;
        public IEnumerable<Indicadores> MI11;
        public IEnumerable<Indicadores> MI12;


        //Datos       
        public string EF_ID { get; set; }
        public string EF_Nombre { get; set; }

        //Del Shape de Mapas IEF
        public string qgs_fid { get; set; }
        public byte[] geom { get; set; }
        public string cvegeo { get; set; }
        public string cve_ent { get; set; }
        public string cve_mun { get; set; }
        public string nomgeo { get; set; }

        public float Año1 { get; set; }
        public float Año2 { get; set; }
        public float Año3 { get; set; }
        public float Año4 { get; set; }
        public float Año5 { get; set; }
        public float Año6 { get; set; }
        public float Año7 { get; set; }
        public float Año8 { get; set; }
        public float Año9 { get; set; }
        public float Año10 { get; set; }
        public float Año11 { get; set; }
        public float Año12 { get; set; }
        public float Año13 { get; set; }
        public float Año14 { get; set; }
        public float Año15 { get; set; }
        public float Año16 { get; set; }


        //Colores de la Tabla y Mapa
        public string C_Año1 { get; set; }
        public string C_Año2 { get; set; }
        public string C_Año3 { get; set; }
        public string C_Año4 { get; set; }
        public string C_Año5 { get; set; }
        public string C_Año6 { get; set; }
        public string C_Año7 { get; set; }
        public string C_Año8 { get; set; }
        public string C_Año9 { get; set; }
        public string C_Año10 { get; set; }
        public string C_Año11 { get; set; }
        public string C_Año12 { get; set; }
        public string C_Año13 { get; set; }
        public string C_Año14 { get; set; }
        public string C_Año15 { get; set; }
        public string C_Año16 { get; set; }
        public string C { get; set; }



        //Datos de los Colores de los Indicadores año actual
        public int QgsFid { get; set; }
        public string CveGeo { get; set; }
        public string CveEnt { get; set; }
        public string nombre_entidad { get; set; }
        public string CveMun { get; set; }
        public string nombre_mun { get; set; }
        public int I1_E { get; set; }
        public int I1_M { get; set; }
        public int I2_E { get; set; }
        public int I2_M { get; set; }
        public int I3_E { get; set; }
        public int I3_M { get; set; }
        public int I4_E { get; set; }
        public int I4_M { get; set; }
        public int I5_E { get; set; }
        public int I5_M { get; set; }
        public int I6_E { get; set; }
        public int I6_M { get; set; }
        public int I8_E { get; set; }
        public int I8_M { get; set; }
        public int I7_E { get; set; }
        public int I9_E { get; set; }
        public int I10_E { get; set; }
        public int I11_E { get; set; }
        public int I12_E { get; set; }


    }
}
