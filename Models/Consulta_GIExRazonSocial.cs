using Microsoft.AspNetCore.Mvc;
using NSIE.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class Consulta_GIExRazonSocial
    {
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
        public string RFC { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string EstatusInstalacion { get; set; }
        public string CausaSuspension { get; set; }
        public string Marca { get; set; }
        public string TipoPermiso { get; set; }
        public DateTime InicioVigencia { get; set; }
        public DateTime TerminoVigencia { get; set; }
        public DateTime InicioOperaciones { get; set; }
        public string CapacidadAutorizadaBarriles { get; set; }
        public string InversionEstimada { get; set; }
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
        public string Id_sol { get; set; }
        public string EfId_sol { get; set; }
        public string EfNombre_sol { get; set; }
        public string MpoId_sol { get; set; }
        public string MunicipioNombre_sol { get; set; }
        public string DocumentosCompletos_sol { get; set; }
        public string AnalisisRiesgo_sol { get; set; }
        public float XGeo_sol { get; set; }
        public float YGeo_sol { get; set; }
        public string Turno_sol { get; set; }
        public string MarcaSolicitada_sol { get; set; }
        public string Expediente_sol { get; set; }
        public string RazonSocial_sol { get; set; }
        public DateTime FechaSolicitud_sol { get; set; }
        public string Tipo { get; set; }
    }

}
