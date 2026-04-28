namespace NSIE.Models
{
    public class ExpendioAutorizadoElectricidad
    {
        public string NumeroPermiso { get; set; }
        public string EfId { get; set; }
        public string MpoId { get; set; }
        public string Expediente { get; set; }
        public string RazonSocial { get; set; }
        public DateTime FechaDeOtorgamiento { get; set; }
        public float LatitudGeo { get; set; }
        public float LongitudGeo { get; set; }
        public string Calle { get; set; }
        public string Colonia { get; set; }
        public string CodigoPostal { get; set; }
        public string Estatus { get; set; }
        public string Rfc { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string Marca { get; set; }
        public string TipoPermiso { get; set; }
        public DateTime InicioOperaciones { get; set; }
        public string CapacidadInstalacion { get; set; }
        public string VigenciaAnos { get; set; }
        public string NumeroSENER { get; set; }
        public string SubTipo { get; set; }
        public string SiglasTipo { get; set; }
        public string Otorgamiento { get; set; }
        public DateTime FechaAcuse { get; set; }
        public string EstatusSAT { get; set; }
        public string Subestatus { get; set; }
        public DateTime SuspensionInicio { get; set; }
        public DateTime SuspensionFin { get; set; }
        public string NumeroTanques { get; set; }
        public string CapacidadLitros { get; set; }
        public string NumeroUnidades { get; set; }
        public string NumeroDeCentralesDeGuarda { get; set; }
        public string DomicilioDeGuarda { get; set; }
        public string SuministroRecepcion { get; set; }
        public string PermisoSuministro { get; set; }
        public string CompartenTanques { get; set; }
        public string Modificacion { get; set; }
        public string Asociacion { get; set; }
        public string EfNombre { get; set; }

        //De electricidad
        public string Planta { get; set; }


        public string Combustible { get; set; }

        public string FuenteEnergia { get; set; }





    }

}
