using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class ProyectoModernizacionRegistro
    {
        public int ProyectoModernizacionId { get; set; }

        [Display(Name = "No.")]
        [Range(1, int.MaxValue, ErrorMessage = "El número debe ser mayor a cero.")]
        public int Numero { get; set; }

        [Display(Name = "No. original")]
        public int? NumeroOriginal { get; set; }

        [StringLength(100)]
        public string GRT { get; set; }

        [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
        [Display(Name = "Nombre del proyecto")]
        [StringLength(500)]
        public string NombreProyecto { get; set; }

        [Display(Name = "Tipo de financiamiento")]
        [StringLength(150)]
        public string TipoFinanciamiento { get; set; }

        [Display(Name = "Año de instrucción")]
        public int? AnioInstruccion { get; set; }

        [Display(Name = "Etapa del proyecto")]
        [StringLength(150)]
        public string EtapaProyecto { get; set; }

        [Display(Name = "Monto del proyecto (MDP)")]
        [Range(typeof(decimal), "0", "999999999999")]
        public decimal? MontoProyectoMdp { get; set; }

        [Display(Name = "Elementos y equipos asociados")]
        public string ElementosEquiposAsociados { get; set; }

        [Display(Name = "Fecha estimada de inicio")]
        [StringLength(100)]
        public string FechaEstimadaInicio { get; set; }

        [Display(Name = "FEO indicada en oficio SENER")]
        [StringLength(150)]
        public string FeoIndicadaOficioSener { get; set; }

        [Display(Name = "FEO factible")]
        [StringLength(150)]
        public string FeoFactible { get; set; }

        [Display(Name = "% avance de ejecución")]
        [Range(typeof(decimal), "0", "100")]
        public decimal? PorcentajeAvanceEjecucion { get; set; }

        [Display(Name = "Circunstancias que ocasionaron atrasos")]
        public string CircunstanciasAtrasos { get; set; }

        [Display(Name = "Acciones de mitigación o corrección")]
        public string AccionesMitigacionCorreccion { get; set; }

        [Display(Name = "Estado real que guarda el proyecto")]
        public string EstadoRealProyecto { get; set; }

        [Display(Name = "Comentarios sobre el nivel de priorización")]
        [StringLength(200)]
        public string ComentariosNivelPriorizacion { get; set; }

        [Display(Name = "Clave PEM")]
        [StringLength(200)]
        public string ClavePem { get; set; }

        [Display(Name = "Clasificación SENER")]
        [StringLength(200)]
        public string ClasificacionSener { get; set; }

        [Display(Name = "Fecha de programación (trimestre)")]
        [StringLength(50)]
        public string FechaProgramacionTrimestre { get; set; }

        [Display(Name = "Quincena de publicación")]
        [StringLength(50)]
        public string QuincenaPublicacion { get; set; }

        [Display(Name = "Universo presentación presidencia")]
        [StringLength(50)]
        public string UniversoPresentacionPresidencia { get; set; }

        [Display(Name = "MVA")]
        [Range(typeof(decimal), "0", "999999999999")]
        public decimal? Mva { get; set; }

        [Display(Name = "MVAR")]
        [Range(typeof(decimal), "0", "999999999999")]
        public decimal? Mvar { get; set; }

        [Display(Name = "km-C")]
        [Range(typeof(decimal), "0", "999999999999")]
        public decimal? KmC { get; set; }

        public string FuenteArchivo { get; set; }
        public DateTime FechaCarga { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; } = true;
    }
}