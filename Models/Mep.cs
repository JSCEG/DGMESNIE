using System.ComponentModel.DataAnnotations;
namespace NSIE.Models
{
    public class Mep
    {
        //Para Cargar Archivos
        public IFormFile File { get; set; }



        public IEnumerable<Indicador1_Clase> Indicador1 { get; set; }
        public IEnumerable<Indicador2_Clase> Indicador2 { get; set; }
        public IEnumerable<Indicador3_Clase> Indicador3 { get; set; }
        public IEnumerable<Indicador4_Clase> Indicador4 { get; set; }
        public IEnumerable<Coordenadasx> ObtenerMapa { get; set; }
        public IEnumerable<Expendios> ObtenerExpendios { get; set; }


        //Petroliferos
        public IEnumerable<Consulta_SolicitudesE> ObtenerTotalSolicitudes_EvaluarPetroliferos { get; set; }
        public IEnumerable<Consulta_SolicitudesE> ObtenerSolicitudes_MapaPetroliferos { get; set; }
        public IEnumerable<CalificacionFinal> Calcula_CFExpendiosxEF { get; set; }
        public IEnumerable<CalificacionFinal> Resultados { get; set; }

        //Datos para Obtener la calificación Final
        [Required(ErrorMessage = "El Campo {0} es obligatorio, no puede estar vacío,ni ser 0 ni mayor a 1")]
        public float umbralSeleccionado { get; set; }
        public bool chkI1 { get; set; }
        public bool chkI2 { get; set; }
        public bool chkI3 { get; set; }
        public bool chkI4 { get; set; }
        public bool chkI5 { get; set; }
        public bool chkI6 { get; set; }
        public bool chkI7 { get; set; }
        public bool chkI8 { get; set; }
        public bool chkI9 { get; set; }
        public bool chkI10 { get; set; }
        public bool chkI11 { get; set; }
        public bool chkI12 { get; set; }
        public Indicadores Indicadores { get; internal set; }
        public Indicadores ColoresMIEF { get; internal set; }
    }
}
