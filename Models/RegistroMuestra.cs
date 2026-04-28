#nullable enable annotations

using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;

namespace NSIE.Models
{
    /// <summary>
    /// Clase RegistroMuestra
    /// 
    /// TABLA SQL: [dbo].[Registro_Muestras]
    /// 
    /// DESCRIPCIÓN:
    /// Modelo que representa los registros de muestras operacionales del Formulario 1.
    /// Soporta dos tipos de fuentes: Arcilla (Barrenación) y Salmuera (Pozos).
    /// Esta tabla es el corazón de la Operación (INSUMO 1.2) del SIIL.
    /// 
    /// PROPÓSITO:
    /// - Almacena datos del Formulario 1 (cálculo de ID único: ARC-BRR-2026/01/23...)
    /// - Vincula operación con inteligencia mediante IdPozo_Pronostico_FK
    /// - Soporta campos condicionales según Fuente/Origen
    /// 
    /// TIPOS DE DATOS SOPORTADOS:
    /// 1. Arcilla - Barrenación (Azimut, Inclinación, Largo, Diámetro, RQD, FamiliaRoca)
    /// 2. Salmuera (ProfundidadPozo, pH, Conductividad, Temperatura)
    /// </summary>
    public class RegistroMuestra
    {
        // ============================================================
        // IDENTIFICADOR ÚNICO (PK)
        // ============================================================

        /// <summary>
        /// Clave primaria generada por el calculadora del Formulario 1
        /// Formato: {Fuente}-{Origen}-{YYYY/MM/DD} (ej. ARC-BRR-2026/01/23...)
        /// 
        /// IMPORTANTE: Este ID debe ser único y generado por lógica en C#
        /// No es auto-generado por la BD
        /// </summary>
        public string? IdMuestra { get; set; }

        // ============================================================
        // CONTEXTO GENERAL
        // ============================================================

        /// <summary>Identificador del proyecto asociado</summary>
        /// [Required(ErrorMessage = "El ID del Proyecto es obligatorio.")]
        [RegularExpression(@"^[A-Za-z0-9\-_\/]+$", 
            ErrorMessage = "Formato inválido en Id Proyecto.")]
        [StringLength(50)]
        public string IdProyecto { get; set; }

        /// <summary>Institución responsable del registro</summary>
        public string Institucion { get; set; }

        /// <summary>Email del responsable del registro</summary>
        public string ResponsableRegistro { get; set; }

        /// <summary>Email del responsable del registro</summary>
        [Write(false)]
        public string? NombreResponsableRegistro { get; set; }

        /// <summary>Fecha y hora del registro operacional</summary>
        public DateTime FechaRegistro { get; set; }

        // ============================================================
        // UBICACIÓN GEOGRÁFICA Y ADMINISTRATIVA
        // ============================================================

        /// <summary>Estado de México donde se ubicó la muestra</summary>
        [Required(ErrorMessage = "Debe seleccionar un estado.")]
        public string Estado { get; set; }

        /// <summary>Municipio donde se ubicó la muestra</summary>
        [Required(ErrorMessage = "Debe seleccionar un municipio.")]
        public string Municipio { get; set; }

        /// <summary>Coordenada de latitud (precisión: 6 decimales = ~0.1 metros)</summary>
        [Range(14, 33, ErrorMessage = "Latitud fuera de rango válido para México.")]
        public decimal Latitud { get; set; }

        /// <summary>Coordenada de longitud (precisión: 6 decimales = ~0.1 metros)</summary>
        [Range(-118, -86, ErrorMessage = "Longitud fuera de rango válido para México.")]
        public decimal Longitud { get; set; }

        // ============================================================
        // CLASIFICACIÓN (SOURCE 1: Fuente/Origen)
        // ============================================================

        /// <summary>
        /// Tipo de fuente de la muestra
        /// Valores: 'Arcilla' o 'Salmuera'
        /// 
        /// CONDICIONA LOS CAMPOS A CAPTURAR:
        /// - Arcilla → Datos de Barrenación
        /// - Salmuera → Datos de Pozo
        /// </summary>
        public string Fuente { get; set; }

        /// <summary>
        /// Tipo de origen de la muestra
        /// Valores: 'Prospectiva' o 'Barrenación'
        /// 
        /// Para Arcilla:
        /// - Prospectiva: búsqueda inicial
        /// - Barrenación: perforación profunda
        /// 
        /// Para Salmuera:
        /// - Prospectiva: muestreo inicial
        /// - Barrenación: extracción profunda
        /// </summary>
        public string Origen { get; set; }

        // ============================================================
        // DATOS CONDICIONALES: ARCILLA - BARRENACIÓN
        // ============================================================

        /// <summary>Ángulo de azimut de la perforación (NULL si no aplica)</summary>
        public decimal? Azimut { get; set; }

        /// <summary>Ángulo de inclinación de la perforación (NULL si no aplica)</summary>
        public decimal? Inclinacion { get; set; }

        /// <summary>Largo de la muestra en metros (NULL si no aplica)</summary>
        public decimal? Largo { get; set; }

        /// <summary>Diámetro de la muestra en milímetros (NULL si no aplica)</summary>
        public decimal? Diametro { get; set; }

        /// <summary>
        /// Calidad de Roca (Rock Quality Designation)
        /// Valores esperados: '0%-25%', '25%-50%', '50%-75%', '75%-100%'
        /// NULL si no aplica (no es Arcilla-Barrenación)
        /// </summary>
        public string? RQD { get; set; }

        /// <summary>Familia de roca identificada (ej. Granito, Basalto, etc.). NULL si no aplica</summary>
        public string? FamiliaRoca { get; set; }

        /// <summary>ID del barreno al que pertenece la muestra</summary>
        public string BarrenoID { get; set; }

        // ============================================================
        // DATOS CONDICIONALES: SALMUERA - POZO
        // ============================================================

        /// <summary>Profundidad del pozo en metros (NULL si no aplica)</summary>
        public decimal? ProfundidadPozo { get; set; }

        /// <summary>pH de la solución acuosa (rango típico: 0-14). NULL si no aplica</summary>
        public decimal? PH { get; set; }

        /// <summary>Conductividad eléctrica en µS/cm (micro-Siemens por centímetro). NULL si no aplica</summary>
        public decimal? Conductividad { get; set; }

        /// <summary>Temperatura de la salmuera en °C. NULL si no aplica</summary>
        public decimal? Temperatura { get; set; }

        // ============================================================
        // VINCULACIÓN CON INTELIGENCIA (FOREIGN KEY)
        // ============================================================

        /// <summary>
        /// Foreign Key a Pronostico_Pozos.IdInterno
        /// 
        /// ⭐ LA CLAVE DE LA INTEGRACIÓN ⭐
        /// 
        /// PERMITE:
        /// - Vincular cada muestra operacional con su pronóstico
        /// - Validar si la operación coincide con la inteligencia
        /// - Generar reportes de correlación Inteligencia-Operación
        /// 
        /// NULL si no hay pronóstico asociado (operación autónoma)
        /// </summary>
        public int? IdPozo_Pronostico_FK { get; set; }

        // ============================================================
        // AUDITORÍA
        // ============================================================

        /// <summary>Fecha y hora de creación del registro (DEFAULT: GETDATE())</summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>Fecha y hora de última actualización (DEFAULT: GETDATE())</summary>
        public DateTime FechaActualizacion { get; set; }

        // ============================================================
        // NAVEGACIÓN (para relaciones con Entity Framework)
        // ============================================================

        /// <summary>Referencia al pronóstico relacionado (si existe FK)</summary>
        [Write(false)]
        public PronosticoPozo? PronosticoRelacionado { get; set; }
    }

    public class Municipio
    {
        public int MunicipioID { get; set; }
        public string Municipio_Nombre { get; set; }
        public int EF_ID { get; set; }
    }

    public class Estado
    {
        public int EF_ID { get; set; }
        public string EF_Nombre { get; set; }
    }
}
