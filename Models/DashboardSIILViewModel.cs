namespace NSIE.Models
{
    /// <summary>
    /// Modelo de vista para el Dashboard principal de SIIL
    /// Contiene KPIs y métricas derivadas de pronósticos y resultados de laboratorio
    /// </summary>
    public class DashboardSIILViewModel
    {
        /// <summary>
        /// Total de muestras físicas registradas en el Catálogo Nacional
        /// Basado en: Fuente [5] - Total de registros en Registro_Muestras
        /// </summary>
        public int TotalMuestras { get; set; }

        /// <summary>
        /// Total de pozos con potencial Alto/Muy Alto según pronósticos
        /// Basado en: Fuente [3] - Pozos existentes con PosibilidadIntervalo = "1. Muy alto" o "2. Alto"
        /// </summary>
        public int PozosPrioritarios { get; set; }

        /// <summary>
        /// Total de muestras con cálculo de Litio Probable completado
        /// Basado en: Fuente [4] - Muestras con resultados analíticos finalizados
        /// </summary>
        public int MuestrasAnalizadas { get; set; }

        /// <summary>
        /// Nombre del usuario actual (para personalización del dashboard)
        /// </summary>
        public string NombreUsuario { get; set; }

        /// <summary>
        /// Rol del usuario actual
        /// </summary>
        public string RolUsuario { get; set; }

        /// <summary>
        /// Lista de pronósticos de pozos (para dropdown en formulario)
        /// </summary>
        public List<PronosticoPozo> PronosticosPozos { get; set; } = new List<PronosticoPozo>();
    }
}
