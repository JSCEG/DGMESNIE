namespace NSIE.Models
{
    public class ExpendioAutorizadoRyS_Infra
    {
        public string NumeroPermiso { get; set; }
        public string EfId { get; set; }
        public string MpoId { get; set; }
        public string NumeroDeExpediente { get; set; }
        public string RazonSocial { get; set; }
        public DateTime FechaOtorgamiento { get; set; }
        public double LatitudGeo { get; set; }
        public double LongitudGeo { get; set; }
        public string Direccion { get; set; }
        public string Estatus { get; set; }
        public string RFC { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string EstatusInstalacion { get; set; }
        public string TipoPermiso { get; set; }
        public DateTime InicioVigencia { get; set; }
        public DateTime InicioOperaciones { get; set; }
        public double CapacidadAutorizadaMW { get; set; }
        public double GeneracionEstimadaAnual { get; set; }
        public double InversionEstimadaMdls { get; set; }
        public string EnergeticoPrimario { get; set; }
        public string ActividadEconomica { get; set; }
        public string Tecnologia { get; set; }
        public string EmpresaLider { get; set; }
        public string PaisDeOrigen { get; set; }
        public string Subasta { get; set; }
        public string Comentarios { get; set; }

        // 

        public string Planta { get; set; }
        public string Combustible { get; set; }
        public string FuenteEnergia { get; set; }

        //Clasificación
        public string Clasificacion { get; set; }

    }

}
