using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;

namespace NSIE.Models
{
    public class BarrenacionViewModel
    {
        // =========================
        // CONTEXTO
        // =========================

        [Required(ErrorMessage = "Debe seleccionar la litología local.")]
        public string LitologiaLocal { get; set; }

        [Range(-10000, 10000, ErrorMessage = "La anomalía gravimétrica debe estar entre -10000 y 10000.")]
        public decimal? AnomaliaGravimetrica { get; set; }
        
        [Required(ErrorMessage = "Debe registrar anomalía 1.")]
        public string Anomalia1 { get; set; }

        [Required(ErrorMessage = "Debe registrar anomalía 2.")]
        public string Anomalia2 { get; set; }

        [Required(ErrorMessage = "Debe registrar anomalía 3.")]
        public string Anomalia3 { get; set; }

        [Required(ErrorMessage = "Debe seleccionar la accesibilidad del sitio.")]
        public string Accesibilidad { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el tipo de terreno.")]
        public string TipoTerreno { get; set; }


        // =========================
        // BARRENO
        // =========================

        [Required(ErrorMessage = "El ID del barreno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El ID del barreno no debe superar los 50 caracteres.")]
        public string BarrenoID { get; set; }

        [Required(ErrorMessage = "Debe registrar a la empresa perforista.")]
        [StringLength(100, ErrorMessage = "El nombre del perforista no debe superar los 100 caracteres.")]
        public string Perforista { get; set; }

        [Required(ErrorMessage = "Debe registrar al responsable de la perforación.")]
        [StringLength(100, ErrorMessage = "El nombre del responsable no debe superar los 100 caracteres.")]
        public string Responsable { get; set; }


        // =========================
        // COORDENADAS
        // =========================

        [Required(ErrorMessage = "Debe registrar la latitud.")]
        [Range(14.5, 32.7, ErrorMessage = "La latitud debe estar dentro del territorio nacional.")]
        public decimal Latitud { get; set; }

        [Required(ErrorMessage = "Debe registrar la longitud.")]
        [Range(-118.5, -86.7, ErrorMessage = "La longitud debe estar dentro del territorio nacional.")]
        public decimal Longitud { get; set; }

        [Range(0, 10000, ErrorMessage = "La altitud debe estar entre 0 y 10000 metros.")]
        public decimal? Altitud { get; set; }

        [Range(0, 360, ErrorMessage = "El azimut debe estar entre 0° y 360°.")]
        public int? Azimut { get; set; }

        [Range(-90, 0, ErrorMessage = "La inclinación debe estar entre -90° y 0°.")]
        public int? Inclinacion { get; set; }


        // =========================
        // PROGRAMA
        // =========================

        [Required(ErrorMessage = "Debe seleccionar el tipo de barrenación.")]
        public string TipoBarrenacion { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaFinalizacion { get; set; }

        [Required(ErrorMessage = "Debe registrar la longitud perforada.")]
        [Range(0.01, 10000, ErrorMessage = "La longitud perforada debe ser mayor a 0.")]
        public decimal? LongitudPerforada { get; set; }


        // =========================
        // NÚCLEO
        // =========================

        [Range(0.01, 10000, ErrorMessage = "La longitud recuperada debe ser mayor a 0.")]
        public decimal? LongitudRecuperada { get; set; }

        [Range(1, 1000, ErrorMessage = "El diámetro debe ser mayor a 0.")]
        public int? Diametro { get; set; }

        [Range(1, 1000, ErrorMessage = "Debe registrar al menos una caja.")]
        public int? NumeroCajas { get; set; }

        [Required(ErrorMessage = "Debe registrar el nombre de las cajas.")]
        [StringLength(200, ErrorMessage = "El nombre de las cajas no debe superar los 200 caracteres.")]
        public string NombreCajas { get; set; }

        [StringLength(500, ErrorMessage = "La lista de RQD es demasiado larga.")]
        public string RQD { get; set; }

        [Range(0, 100, ErrorMessage = "El TCR debe estar entre 0 y 100.")]
        public decimal? TCR { get; set; }

        [Range(1, 1000, ErrorMessage = "Debe registrar al menos un intervalo.")]
        public int? Intervalos { get; set; }

        [Required(ErrorMessage = "Debe registrar la descripción de intervalos.")]
        [StringLength(500, ErrorMessage = "La descripción de intervalos es demasiado larga.")]
        public string IntervalosInteres { get; set; }


        // =========================
        // ARCHIVOS
        // =========================
        // [Required(ErrorMessage = "Debe anexar el archivo de descripción del núcleo.")]
        public IFormFile ArchivoDescripcionNucleo { get; set; }

        // [Required(ErrorMessage = "Debe anexar al menos una fotografía del núcleo.")]
        public List<IFormFile> FotografiasNucleo { get; set; }


        // =========================
        // NOTAS
        // =========================
        [Required(ErrorMessage = "Debe registrar observaciones generales.")]
        [StringLength(1000, ErrorMessage = "Las observaciones no deben superar los 1000 caracteres.")]
        public string Observaciones { get; set; }
    }

    public class Barrenacion
    {
        public int Id { get; set; }

        // CONTEXTO
        public string LitologiaLocal { get; set; }
        public decimal? AnomaliaGravimetrica { get; set; }

        public string? Anomalia1 { get; set; }
        public string? Anomalia2 { get; set; }
        public string? Anomalia3 { get; set; }

        public string Accesibilidad { get; set; }
        public string TipoTerreno { get; set; }

        // BARRENO
        public string BarrenoID { get; set; }
        public string? Perforista { get; set; }
        public string? Responsable { get; set; }

        // COORDENADAS
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public decimal? Altitud { get; set; }

        public int? Azimut { get; set; }
        public int? Inclinacion { get; set; }

        // PROGRAMA
        public string TipoBarrenacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

        public decimal? LongitudPerforada { get; set; }

        // NUCLEO
        public decimal? LongitudRecuperada { get; set; }
        public int? Diametro { get; set; }
        public int? NumeroCajas { get; set; }
        public string? NombreCajas { get; set; }

        public string RQD { get; set; }
        public decimal? TCR { get; set; }

        public int? Intervalos { get; set; }
        public string? IntervalosInteres { get; set; }

        // ARCHIVOS
        public string? ArchivoDescripcionRuta { get; set; }

        // NOTAS
        public string? Observaciones { get; set; }

        // AUDITORIA
        public DateTime FechaCreacion { get; set; }
    }

    // FOTOS
    public class BarrenacionFoto
    {
        public int Id { get; set; }

        public int BarrenacionId { get; set; }

        public string RutaFoto { get; set; }

        [Write(false)]
        public DateTime? FechaSubida { get; set; }
    }
}