namespace NSIE.Models
{

    public class TotalPermisosVehiculares
    {
        public IEnumerable<PermisoVehicular> Permisos { get; set; }
        public int TotalAutotanques { get; set; }
        public int TotalBuquetanques { get; set; }
        public int TotalVReparto { get; set; }
        public int TotalSemirremolques { get; set; }
        public int TotalGeneralVehiculos { get; set; }
        public long TotalCapacidadAutotanques { get; set; }
        public long TotalCapacidadBuquetanques { get; set; }
        public long TotalCapacidadVReparto { get; set; }
        public long TotalCapacidadSemirremolques { get; set; }
        public long TotalGeneralCapacidad { get; set; }
    }




}
