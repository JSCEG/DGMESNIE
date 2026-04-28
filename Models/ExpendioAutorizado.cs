namespace NSIE.Models
{
    public class ExpendioAutorizado
    {

        //Campos establecidos para la Calificación Pública

        public string cve_ent { get; set; }
        public string cve_mun { get; set; }
        public string yearSelect { get; set; }
        public string mercadoSelect { get; set; }
        public string resultado { get; set; }
        public string ResultadoFinal { get; set; }
        //
        public string NumeroPermiso { get; set; }
        public string EfId { get; set; }
        public string MpoId { get; set; }
        public string NumeroDeExpediente { get; set; }
        public string RazonSocial { get; set; }
        public DateTime FechaOtorgamiento { get; set; }
        public float LatitudGeo { get; set; }
        public float LongitudGeo { get; set; }
        public string CalleNumEs { get; set; }
        public string ColoniaEs { get; set; }
        public string CodigoPostal { get; set; }
        public string Estatus { get; set; }
        public string Rfc { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string EstatusInstalacion { get; set; }
        public string CausaSuspension { get; set; }
        public string Marca { get; set; }
        public string TipoPermiso { get; set; }
        public DateTime InicioVigencia { get; set; }
        public DateTime TerminoVigencia { get; set; }
        public DateTime InicioOperaciones { get; set; }
        public string CapacidadAutorizadaBarriles { get; set; }
        public float InversionEstimada { get; set; }
        public string Productos { get; set; }
        public string Comentarios { get; set; }
        public string TipoPersona { get; set; }
        public string NumeroDeEstacionesDeServicio { get; set; }
        public string TipoDeEstacion { get; set; }
        public DateTime FechaDeAcuse { get; set; }
        public DateTime FechaEntregaEstadosFinancieros { get; set; }
        public string Propietario { get; set; }
        public string CapacidadMaximaDeLaBomba { get; set; }
        public string CapacidadOperativaReal { get; set; }
        public string ServicioDeRegadera { get; set; }
        public string ServicioDeRestaurante { get; set; }
        public string ServicioDeSanitario { get; set; }
        public string OtrosServicios { get; set; }
        public string TiendaDeConveniencia { get; set; }
        public string NumeroDeModulosDespachadores { get; set; }
        public string TipoDeEstacionId { get; set; }
        public string TipoDePersona { get; set; }
        public string TipoDePermiso { get; set; }
        public string EstadoDePermiso { get; set; }
        public string EstatusDeLaInstalacion { get; set; }
        public string ImagenCorporativa { get; set; }
        public string CausaSuspencionInstalacionId { get; set; }
        public string EfNombre { get; set; }


        // Nuevas propiedades para precios
        // Nuevas propiedades para precios con valores predeterminados
        public float PrecioRegular { get; set; } = 26.07f;
        public float PrecioPremium { get; set; } = 28.15f;
        public float PrecioDiesel { get; set; } = 25.60f;
    }

}
