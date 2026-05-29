using NSIE.Models.Gestor;

namespace NSIE.Servicios.Interfaces
{
    public interface IRepositorioGestor
    {
        // ── Actividades (Padre) ────────────────────────────────
        Task<List<GestorActividad>> ObtenerActividadesAsync(int? usuarioId = null);
        Task<GestorActividad?> ObtenerActividadPorIdAsync(int actividadId);
        Task<int> CrearActividadAsync(GestorActividadForm form, int? usuarioId);
        Task ActualizarActividadAsync(GestorActividadForm form, int? usuarioId);
        Task EliminarActividadAsync(int actividadId);

        // ── Temas (Hijo) ───────────────────────────────────────
        Task<List<GestorTema>> ObtenerTemasAsync(int? actividadId = null, int? usuarioId = null);
        Task<GestorTema?> ObtenerTemaPorIdAsync(int temaId);
        Task<int> CrearTemaAsync(GestorTemaForm form, int? usuarioId);
        Task ActualizarTemaAsync(GestorTemaForm form, int? usuarioId);
        Task EliminarTemaAsync(int temaId);

        // ── Usuarios (para selects de responsable) ────────────
        Task<List<GestorUsuarioDto>> ObtenerUsuariosVigentesAsync();
        Task<GestorUsuarioDto?> ObtenerUsuarioVigentePorIdAsync(int idUsuario);
    }
}
