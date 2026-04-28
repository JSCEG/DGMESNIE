namespace NSIE.Models
{
    public class PermisoVehicular
    {
        public string Permiso { get; set; }
        public string TIPO_DE_VEHICULO { get; set; }
        public string ID_CRE { get; set; }
        public string NUMERO_ECONOMICO { get; set; }
        public string MARCA_DEL_RECIPIENTE { get; set; }
        public int CAPACIDAD_DEL_RECIPIENTE_LITROS { get; set; }
        public string NUMERO_DE_SERIE_DEL_RECIPIENTE { get; set; }
        public string NUMERO_DE_PLACA_DEL_RECIPIENTE { get; set; }

        //PARA DISTRIBUCIÓN
        public string MARCA { get; set; }
        public string MODELO { get; set; }
        public string PLACAS { get; set; }

        //PARA AMBOS
        public DateTime FECHA { get; set; }
    }


}
