using System.Collections.Generic;
using System.Threading.Tasks;
using NSIE.Models;

namespace NSIE.Servicios.Interfaces
{
    public interface IRepositorioSIIL
    {
        Task<List<T>> ObtenerTodos<T>(string nombreTabla) where T : class;
        Task<T> ObtenerPorId<T>(string nombreTabla, int id) where T : class;
        Task<int> Insertar<T>(string nombreTabla, T objeto) where T : class;
        Task<bool> Actualizar<T>(string nombreTabla, T objeto) where T : class;
        Task<bool> Eliminar<T>(string nombreTabla, int id) where T : class;
        Task<List<T>> ObtenerPorFiltro<T>(string nombreTabla, string condicion, Dictionary<string, object> parametros) where T : class;
        Task<bool> ExisteBarreno(string barrenoId);
        Task<bool> BuscarBarreno();
        Task<IEnumerable<string>> ObtenerBarrenos();


        
        //USUARIO Y ROLES
        Task<UserViewModel> ObtenerUsuarioPorCorreo(string email);
        Task<UserViewModel> ObtenerRolPorUsuarioId(int id);

        //UBICACIÓN
        Task<IEnumerable<Estado>> ObtenerTodos();
        Task<IEnumerable<Municipio>> ObtenerMunicipiosPorEstado(int estadoId);

        /// <summary>
        /// Genera el identificador único para Registro_Muestras según la fórmula:
        /// [CÓDIGO_FUENTE] - [CÓDIGO_ORIGEN] - [TIMESTAMP]
        /// 
        /// Ejemplos:
        /// - Salmuera → SLM-SLM-2026/01/23 06:21
        /// - Arcilla + Prospectiva → ARC-PRS-2026/01/23 06:38
        /// - Arcilla + Barrenación → ARC-BRR-2026/01/23 06:39
        /// 
        /// Este ID es la PK de Registro_Muestras y FK en tablas de resultados.
        /// </summary>
        /// <param name="fuente">"Arcilla" o "Salmuera"</param>
        /// <param name="origen">"Prospectiva" o "Barrenación" (ignorado si fuente=Salmuera)</param>
        /// <param name="fecha">Fecha y hora del registro (formato: yyyy/MM/dd HH:mm)</param>
        /// <returns>ID único en formato: XXX-XXX-YYYY/MM/DD HH:mm</returns>
        string GenerarIdMuestra(string fuente, string origen, DateTime fecha);
    }
}
