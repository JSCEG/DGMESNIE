namespace NSIE.Models
{

    public class GeoLocation
    {
        public string Ip { get; set; }
        public string Hostname { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Loc { get; set; } // Contiene las coordenadas "latitud,longitud"
        public string Org { get; set; }
        public string Postal { get; set; }
        public string Timezone { get; set; }

        // Método para obtener latitud y longitud como dos valores separados
        public (double Latitude, double Longitude) GetCoordinates()
        {
            var parts = Loc.Split(',');
            return (double.Parse(parts[0]), double.Parse(parts[1]));
        }
    }


}
