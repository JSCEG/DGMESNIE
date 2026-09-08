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

        // Cartera estratégica y segunda convocatoria
        Task SincronizarCarteraConvocatoriaAsync(CarteraConvocatoriaSeed seed, string usuario);
        Task<CarteraConvocatoriaImportResult> GuardarCargaCarteraConvocatoriaAsync(CarteraConvocatoriaImportDocument document, string usuario);
        Task<CarteraConvocatoriaDatos> ObtenerCarteraConvocatoriaAsync();
        Task<CarteraConvocatoriaExpediente?> ObtenerExpedienteConvocatoriaAsync(string folio);
        Task<int> CompletarExpedientesConvocatoriaAsync(CarteraConvocatoriaImportDocument document, string usuario, bool aplicar = false);
        Task<List<CarteraConvocatoriaOperacionProgramada>> ObtenerProgramaOperacionConvocatoriaAsync();
        Task<List<CarteraConvocatoriaExpediente>> ObtenerExpedientesConvocatoriaAsync();
        Task<int> GuardarMarcasConvocatoriaAsync(CarteraConvocatoriaMarcasDocument document, string usuario);
        Task<CarteraConvocatoriaMarca?> ObtenerMarcaConvocatoriaAsync(string folio);
        Task<List<CarteraConvocatoriaMarca>> ObtenerMarcasConvocatoriaAsync();
        Task<(int Guardados, int Firmes, int Descartados, int Revision)> GuardarSeleccionConvocatoriaAsync(CarteraConvocatoriaSeleccionDocument document, string usuario);
        Task<CarteraConvocatoriaSeleccion?> ObtenerSeleccionConvocatoriaAsync(string folio);
        Task<List<CarteraConvocatoriaSeleccion>> ObtenerSeleccionesConvocatoriaAsync();
        Task<List<CarteraConvocatoriaSeleccionCarga>> ObtenerSeleccionCargasConvocatoriaAsync();
        Task<string?> ObtenerUrlKmlCarteraConvocatoriaAsync(string folio, string tipo);
        Task<bool> ActualizarEstadoConvocatoriaAsync(string folio, string estado, string usuario);
        Task<CarteraConvocatoriaComentario?> AgregarComentarioConvocatoriaAsync(AgregarComentarioConvocatoriaRequest request, string usuario);
        Task<bool> ActualizarPrioridadConvocatoriaAsync(string folio, int prioridad, string usuario);
        Task<long> CrearProyectoConvocatoriaAsync(CrearProyectoConvocatoriaRequest request, string usuario);

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
