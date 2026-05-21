using NSIE.Models.Gestor;

namespace NSIE.Servicios.Interfaces
{
    public interface IRepositorioGestor
    {
        // ── Temas ─────────────────────────────────────────────
        Task<List<GestorTema>> ObtenerTemasAsync();
        Task<GestorTema?> ObtenerTemaPorIdAsync(int temaId);
        Task<int> CrearTemaAsync(GestorTemaForm form, int? usuarioId);
        Task ActualizarTemaAsync(GestorTemaForm form, int? usuarioId);
        Task EliminarTemaAsync(int temaId);

        // ── Actividades ───────────────────────────────────────
        Task<List<GestorActividad>> ObtenerActividadesAsync(int? temaId = null);
        Task<GestorActividad?> ObtenerActividadPorIdAsync(int actividadId);
        Task<int> CrearActividadAsync(GestorActividadForm form, int? usuarioId);
        Task ActualizarActividadAsync(GestorActividadForm form, int? usuarioId);
        Task EliminarActividadAsync(int actividadId);

        // ── Usuarios (para selects de responsable) ────────────
        Task<List<GestorUsuarioDto>> ObtenerUsuariosVigentesAsync();
    }
}
