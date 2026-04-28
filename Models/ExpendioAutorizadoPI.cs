namespace NSIE.Models
{
    public class ExpendioAutorizadoPI_Infra
    {
        public string NumeroPermiso { get; set; }
        public string RazonSocial { get; set; }
        public double LatitudGeo { get; set; }
        public double LongitudGeo { get; set; }
        //Clasificación
        public string Clasificacion { get; set; }
        public string Nombre { get; set; }
        public string InicioOperaciones { get; set; }
        public string LocalidadSalidaMex { get; set; }
        public string PuntoSalidaEua { get; set; }
        public string LocalidadSalidaEua { get; set; }
        public string GasoductoMex { get; set; }
        public string GasoductoEua { get; set; }

        public string Consorcio { get; set; }
        public string Capacidad_Mmpcd { get; set; }
        public string Diametro { get; set; }
        public string Referencia { get; set; }


        // Anterior
        public string EfId { get; set; }
        public string MpoId { get; set; }
        public string NumeroDeExpediente { get; set; }
        public DateTime FechaOtorgamiento { get; set; }
        public string Direccion { get; set; }
        public string Estatus { get; set; }
        public string RFC { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string EstatusInstalacion { get; set; }
        public string TipoPermiso { get; set; }
        public DateTime InicioVigencia { get; set; }
        public double CapacidadAutorizadaMW { get; set; }
        public double GeneracionEstimadaAnual { get; set; }
        public double InversionEstimadaMdls { get; set; }
        public string EnergeticoPrimario { get; set; }
        public string ActividadEconomica { get; set; }
        public string Tecnologia { get; set; }
        public string EmpresaLider { get; set; }
        public string PaisDeOrigen { get; set; }
        public string Subasta { get; set; }

        // 

        public string Planta { get; set; }
        public string Combustible { get; set; }
        public string FuenteEnergia { get; set; }


    }

}
