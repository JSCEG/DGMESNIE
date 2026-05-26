using System.Collections.Generic;
using System.Threading.Tasks;
using NSIE.Models.Gestor;

namespace NSIE.Servicios.Interfaces
{
    public interface IRepositorioGruposInteres
    {
        Task<List<GrupoInteresDto>> ObtenerTodosAsync();
        Task<GrupoInteres?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(GrupoInteresForm form, int? usuarioId);
        Task ActualizarAsync(GrupoInteresForm form, int? usuarioId);
        Task EliminarAsync(int id);
        Task<GruposInteresDashboardVM> ObtenerDashboardDataAsync();
    }
}
