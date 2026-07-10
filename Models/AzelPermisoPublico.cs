namespace NSIE.Models
{
    public class AzelPermisoPublico
    {
        public string? NumeroPermiso { get; set; }
        public string? EfId { get; set; }
        public string? MpoId { get; set; }
        public string? RazonSocial { get; set; }
        public DateTime? FechaOtorgamiento { get; set; }
        public double? LatitudGeo { get; set; }
        public double? LongitudGeo { get; set; }
        public string? Estatus { get; set; }
        public string? EstatusInstalacion { get; set; }
        public string? TipoPermiso { get; set; }
        public DateTime? InicioVigencia { get; set; }
        public DateTime? InicioOperaciones { get; set; }
        public double? CapacidadAutorizadaMW { get; set; }
        public double? GeneracionEstimadaAnual { get; set; }
        public string? ActividadEconomica { get; set; }
        public string? Tecnologia { get; set; }
        public string? FuenteEnergia { get; set; }
        public string? Clasificacion { get; set; }
    }
}
