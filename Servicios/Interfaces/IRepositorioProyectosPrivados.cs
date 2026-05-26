using System.Collections.Generic;
using System.Threading.Tasks;
using NSIE.Models.ProyectosPrivados;

namespace NSIE.Servicios.Interfaces
{
    public interface IRepositorioProyectosPrivados
    {
        // Projects
        Task<List<Proyecto>> ObtenerProyectosAsync(string buscar, string tecnologia, int? clasificacionId, int? prioridadId, int? semaforoId);
        Task<Proyecto> ObtenerProyectoPorIdAsync(int id);
        Task<int> CrearProyectoAsync(ProyectoForm form, string usuario);
        Task<bool> ActualizarProyectoAsync(ProyectoForm form, string usuario);
        Task<bool> EliminarProyectoAsync(int id);

        // Sub-resources for Detail Tabs
        Task<List<TramiteProyecto>> ObtenerTramitesPorProyectoAsync(int proyectoId);
        Task<List<BitacoraProyecto>> ObtenerBitacorasPorProyectoAsync(int proyectoId);
        Task<List<AccionSeguimiento>> ObtenerAccionesPorProyectoAsync(int proyectoId);
        Task<List<Documento>> ObtenerDocumentosPorProyectoAsync(int proyectoId);
        Task<List<HistorialProyecto>> ObtenerHistorialPorProyectoAsync(int proyectoId);

        // Meetings (Reuniones / Minutas)
        Task<List<Reunion>> ObtenerReunionesAsync();
        Task<Reunion> ObtenerReunionPorIdAsync(int id);
        Task<int> GuardarReunionMinutaAsync(ReunionForm form, string usuario);

        // Follow-up Actions (Kanban/List)
        Task<List<AccionSeguimiento>> ObtenerAccionesSeguimientoAsync(int? proyectoId, string estatus, int? semaforoId);
        Task<bool> ActualizarEstatusAccionAsync(int accionId, string estatus, string comentarios, string usuario);
        Task<int> CrearAccionAsync(int proyectoId, string titulo, string descripcion, string responsableNombre, string responsableId, DateTime? fechaCompromiso, string usuario);

        // Catalogs
        Task<List<CatalogoItem>> ObtenerCatClasificacionAsync();
        Task<List<CatalogoItem>> ObtenerCatPrioridadAsync();
        Task<List<CatalogoItem>> ObtenerCatSemaforoAsync();
        Task<List<CatalogoItem>> ObtenerCatTecnologiaAsync();
        Task<List<CatalogoItem>> ObtenerCatValoracionMinutaAsync();
        Task<List<CatalogoItem>> ObtenerCatEstatusTramiteAsync();
        Task<List<CatalogoItem>> ObtenerEmpresasAsync();
        Task<List<CatalogoItem>> ObtenerUsuariosVigentesAsync();
    }
}
