namespace NSIE.Models
{
    /// <summary>
    /// Clase PronosticoPozo
    /// 
    /// TABLA SQL: [dbo].[Pronostico_Pozos]
    /// 
    /// DESCRIPCIÓN:
    /// Modelo que representa los pronósticos de pozos cargados de forma masiva desde Excel.
    /// Esta tabla es el corazón de la Inteligencia (INSUMO 1.1) del SIIL.
    /// 
    /// PROPÓSITO:
    /// - Recibe cargas masivas de datos de pronósticos
    /// - Soporta mapas de calor mediante FormacionGeologica
    /// - Vincula con Registro_Muestras mediante IdInterno (FK)
    /// 
    /// CAMPOS CRÍTICOS:
    /// - FormacionGeologica: VITAL para análisis geológico y mapas de calor
    /// - PosibilidadIntervalo: Categorización de riesgo/oportunidad
    /// - IdInsumo: Trazabilidad de carga masiva
    /// </summary>
    public class PronosticoPozo
    {
        // ============================================================
        // IDENTIFICADORES
        // ============================================================

        /// <summary>Identificador único interno (PK, IDENTITY)</summary>
        public int IdInterno { get; set; }

        /// <summary>ID externo del Excel (ej. P6.0137)</summary>
        public string IdPozo_Externo { get; set; }

        // ============================================================
        // INFORMACIÓN DEL POZO
        // ============================================================

        /// <summary>Campo al que pertenece el pozo (ej. Costero)</summary>
        public string Campo { get; set; }

        /// <summary>Nombre del pozo (ej. Costero 1)</summary>
        public string Pozo { get; set; }

        /// <summary>Estado actual del pozo</summary>
        public string EstadoPozo { get; set; }

        // ============================================================
        // INFORMACIÓN GEOLÓGICA (CRÍTICA PARA MAPAS DE CALOR)
        // ============================================================

        /// <summary>
        /// Formación geológica / Edad
        /// 
        /// ⚠️ CAMPO VITAL: Usado para:
        /// - Análisis estratigráfico
        /// - Generación de mapas de calor
        /// - Correlación geológica
        /// </summary>
        public string FormacionGeologica { get; set; }

        // ============================================================
        // POSIBILIDAD E INTERVALO
        // ============================================================

        /// <summary>
        /// Categoría de posibilidad/probabilidad
        /// Valores esperados: '1. Muy alto', '2. Alto', '3. Medio', '4. Bajo', etc.
        /// </summary>
        public string PosibilidadIntervalo { get; set; }

        /// <summary>Rango de concentración de litio</summary>
        public string RangoLi { get; set; }

        // ============================================================
        // TRAZABILIDAD
        // ============================================================

        /// <summary>
        /// Identificador del insumo de donde proviene la carga
        /// Formato sugerido: Insumo_YYYYMMDD (ej. Insumo_202502)
        /// Permite auditar y re-procesar cargas
        /// </summary>
        public string IdInsumo { get; set; }

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

        /// <summary>Registros de muestras relacionados con este pronóstico</summary>
        public List<RegistroMuestra> RegistrosMuestras { get; set; } = new List<RegistroMuestra>();
    }
}
