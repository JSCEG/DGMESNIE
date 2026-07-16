using System.ComponentModel.DataAnnotations;

namespace NSIE.Models
{
    public class PamLoteAnalisisViewModel
    {
        public HeaderViewModel Header { get; set; }
        public PamLoteAnalisisCabecera Lote { get; set; } = new();
        public List<PamAnalisisFuenteItem> Fuentes { get; set; } = new();
        public PamAnalisisEjecucionItem EjecucionActual { get; set; }
        public PamAnalisisResumen Resumen { get; set; } = new();
        public List<PamFiltroRevisionConteo> ConteosFiltros { get; set; } = new();
        public List<PamProyectoCambiosVista> Proyectos { get; set; } = new();
        // Compatibilidad temporal con la vista anterior. La presentación nueva debe usar Proyectos.
        public List<PamCambioPropuestoVista> Cambios { get; set; } = new();
        public List<PamHallazgoVista> NoEncontrados { get; set; } = new();
        public List<PamProyectoOpcionVista> ProyectosVigentes { get; set; } = new();
        public PamAnalisisFiltro Filtro { get; set; } = new();
        public int TotalProyectos { get; set; }
        public int TotalCambios { get; set; }

        public bool AnalisisActivo =>
            string.Equals(EjecucionActual?.Estado, "Pendiente", StringComparison.OrdinalIgnoreCase)
            || string.Equals(EjecucionActual?.Estado, "Ejecutando", StringComparison.OrdinalIgnoreCase)
            || string.Equals(EjecucionActual?.Estado, "Procesando", StringComparison.OrdinalIgnoreCase);

        public int TotalPaginas => Math.Max(1,
            (int)Math.Ceiling(TotalProyectos / (double)Math.Max(1, Filtro.TamanoPagina)));

        public int RegistroDesde => TotalProyectos == 0
            ? 0
            : ((Filtro.Pagina - 1) * Filtro.TamanoPagina) + 1;

        public int RegistroHasta => Math.Min(Filtro.Pagina * Filtro.TamanoPagina, TotalProyectos);

        public int ConteoFiltroActual(string filtro) => ConteosFiltros
            .FirstOrDefault(conteo =>
                string.Equals(conteo.Filtro, filtro, StringComparison.OrdinalIgnoreCase)
                && string.Equals(conteo.EstadoRevision, Filtro.EstadoRevision, StringComparison.OrdinalIgnoreCase))
            ?.TotalProyectos ?? 0;
    }

    public sealed class PamFiltroRevisionConteo
    {
        public string EstadoRevision { get; set; }
        public string Filtro { get; set; }
        public int TotalProyectos { get; set; }
    }

    public class PamLoteAnalisisCabecera
    {
        public long LoteId { get; set; }
        public Guid LoteUid { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaCorte { get; set; }
        public string Notas { get; set; }
        public string Estado { get; set; }
        public int TotalArchivos { get; set; }
        public DateTime FechaRegistroUtc { get; set; }
        public string UsuarioNombre { get; set; }
    }

    public class PamAnalisisFuenteItem
    {
        public long LoteFuenteId { get; set; }
        public string NombreOriginal { get; set; }
        public string Extension { get; set; }
        public long TamanoBytes { get; set; }
        public string EstadoExtraccion { get; set; }
        public string MensajeExtraccion { get; set; }
        public bool FirmaValidada { get; set; }
    }

    public class PamAnalisisEjecucionItem
    {
        public long AnalisisId { get; set; }
        public string Estado { get; set; }
        public DateTime FechaSolicitudUtc { get; set; }
        public DateTime? FechaInicioUtc { get; set; }
        public DateTime? FechaFinUtc { get; set; }
        public int TotalFuentes { get; set; }
        public int FuentesProcesadas { get; set; }
        public decimal ProgresoPorcentaje { get; set; }
        public string Mensaje { get; set; }
    }

    public class PamAnalisisResumen
    {
        public int RegistrosDetectados { get; set; }
        public int CoincidenciasExactas { get; set; }
        public int CambiosPropuestos { get; set; }
        public int AltasPropuestas { get; set; }
        public int AltasPendientes { get; set; }
        public int AltasRevisadas { get; set; }
        public int ProyectosCamposPendientes { get; set; }
        public int ProyectosCamposRevisados { get; set; }
        public int CamposPendientes { get; set; }
        public int CamposRevisados { get; set; }
        public int Observados { get; set; }
        public int ClavesNoEncontradas { get; set; }
        public int RelacionesJerarquiaValidadas { get; set; }
        public int ProyectosRevisionPendiente => AltasPendientes + ProyectosCamposPendientes;
        public int ProyectosRevisionCerrada => AltasRevisadas + ProyectosCamposRevisados;
    }

    public class PamCambioPropuestoVista
    {
        public long CambioPropuestoId { get; set; }
        public long? ProyectoId { get; set; }
        public long? ProyectoVersionBaseId { get; set; }
        public string ClaveProyecto { get; set; }
        public string NombreProyecto { get; set; }
        public string TipoCambio { get; set; }
        public string Campo { get; set; }
        public string Categoria { get; set; }
        public string ValorActual { get; set; }
        public string ValorPropuesto { get; set; }
        public decimal? Confianza { get; set; }
        public string FuenteDocumento { get; set; }
        public string UbicacionFuente { get; set; }
        public string Estado { get; set; }
        public long? CambioDecisionId { get; set; }
        public string DecisionCampo { get; set; }
        public string NotaDecisionCampo { get; set; }
        public int? UsuarioDecisionCampoId { get; set; }
        public string UsuarioDecisionCampo { get; set; }
        public DateTime? FechaDecisionCampoUtc { get; set; }
        public string EstadoDecision { get; set; }
    }

    public class PamProyectoCambiosVista
    {
        public string GrupoProyecto { get; set; }
        public long? ProyectoId { get; set; }
        public long? ProyectoVersionBaseId { get; set; }
        public string ClaveProyecto { get; set; }
        public string NombreProyecto { get; set; }
        public List<PamCambioPropuestoVista> Cambios { get; set; } = new();
        public PamAltaDatosRecibidos AltaDatos { get; set; }
        public List<PamProyectoCandidatoVista> Candidatos { get; set; } = new();
        public PamDecisionPreliminarVista Decision { get; set; }

        public List<PamCambioPropuestoVista> Prioritarios => Cambios
            .Where(cambio => cambio.Categoria == "Prioritario")
            .ToList();

        public List<PamCambioPropuestoVista> Informativos => Cambios
            .Where(cambio => cambio.Categoria == "Informativo")
            .ToList();

        public List<PamCambioPropuestoVista> Otros => Cambios
            .Where(cambio => cambio.Categoria == "Otros")
            .ToList();

        public List<PamCambioPropuestoVista> PorCompletar => Cambios
            .Where(cambio => cambio.Categoria == "Completar")
            .ToList();

        public List<PamCambioPropuestoVista> Equivalentes => Cambios
            .Where(cambio => cambio.Categoria == "Equivalente")
            .ToList();

        public int TotalCambios => Cambios.Count;
        public int TotalPrioritarios => Cambios.Count(cambio => cambio.Categoria == "Prioritario");
        public int TotalInformativos => Cambios.Count(cambio => cambio.Categoria == "Informativo");
        public int TotalOtros => Cambios.Count(cambio => cambio.Categoria == "Otros");
        public int TotalPorCompletar => Cambios.Count(cambio => cambio.Categoria == "Completar");
        public int TotalEquivalentes => Cambios.Count(cambio => cambio.Categoria == "Equivalente");
        public bool EsAlta => Cambios.Any(cambio => cambio.TipoCambio == "Alta");
        public bool TieneBaja => Cambios.Any(cambio => cambio.TipoCambio == "Baja");
        public bool TieneReactivacion => Cambios.Any(cambio => cambio.TipoCambio == "Reactivación");
        public int CamposPendientes => EsAlta ? 0 : Cambios.Count(cambio => string.IsNullOrWhiteSpace(cambio.DecisionCampo));
        public bool RevisionCamposCompleta => !EsAlta && Cambios.Count > 0 && CamposPendientes == 0;
    }

    /// <summary>
    /// Lectura tipada del registro completo recibido para una propuesta de alta.
    /// Los campos adicionales conservan cualquier columna que traiga un layout futuro.
    /// </summary>
    public sealed class PamAltaDatosRecibidos
    {
        public long CambioPropuestoId { get; set; }
        public string ClaveProyecto { get; set; }
        public string NombreProyecto { get; set; }
        public string GRT { get; set; }
        public string EtapaProyecto { get; set; }
        public string FechaNecesaria { get; set; }
        public string FeoFactible { get; set; }
        public decimal? MontoProyectoMdp { get; set; }
        public string EstadoRealProyecto { get; set; }
        public int? AnioInstruccion { get; set; }
        public string FuenteDocumento { get; set; }
        public string UbicacionFuente { get; set; }
        public decimal? Confianza { get; set; }
        public List<PamCampoRecibidoVista> CamposAdicionales { get; set; } = new();
        public string ClaveFamiliaProbable { get; set; }
        public int TotalClavesMismoNombre { get; set; } = 1;
        public List<string> ClavesMismoNombre { get; set; } = new();
        public bool EsFamiliaProbable => TotalClavesMismoNombre > 1;
    }

    public sealed class PamCampoRecibidoVista
    {
        public string Etiqueta { get; set; }
        public string Valor { get; set; }
    }

    public sealed class PamProyectoCandidatoVista
    {
        public long ProyectoId { get; set; }
        public long ProyectoVersionId { get; set; }
        public string ClaveProyecto { get; set; }
        public string NombreProyecto { get; set; }
        public string GRT { get; set; }
        public string EtapaProyecto { get; set; }
        public decimal Score { get; set; }
        public string Motivo { get; set; }
        public List<string> Coincidencias { get; set; } = new();
        public bool CoincidenciaClaveExacta { get; set; }
        public bool CoincidenciaNombreExacta { get; set; }
        public string ClasificacionSugerida { get; set; }
    }

    /// <summary>
    /// Snapshot de la clasificación humana. No representa una aplicación a la cartera.
    /// </summary>
    public sealed class PamDecisionPreliminarVista
    {
        public long RevisionId { get; set; }
        public string Clasificacion { get; set; }
        public long? ProyectoRelacionadoId { get; set; }
        public string ProyectoRelacionadoClave { get; set; }
        public string ProyectoRelacionadoNombre { get; set; }
        public string Nota { get; set; }
        public int? UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public DateTime? FechaDecisionUtc { get; set; }
        public string VersionDecision { get; set; }
        public string Estado { get; set; }
    }

    public sealed class PamGuardarDecisionPreliminarInput
    {
        [Range(1, long.MaxValue, ErrorMessage = "La propuesta de alta no es válida. Recarga la página e intenta nuevamente.")]
        public long CambioPropuestoId { get; set; }

        [Required(ErrorMessage = "Selecciona cómo debe tratarse este registro.")]
        [StringLength(40, ErrorMessage = "La clasificación seleccionada no es válida.")]
        public string Clasificacion { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Selecciona un proyecto vigente relacionado válido.")]
        public long? ProyectoRelacionadoId { get; set; }

        [StringLength(1000)]
        public string? Nota { get; set; }

        [Required(ErrorMessage = "La versión de la revisión es obligatoria. Recarga la página.")]
        [StringLength(18, ErrorMessage = "La versión de la revisión no es válida. Recarga la página.")]
        [RegularExpression(@"^(?:0x)?[0-9A-Fa-f]{16}$", ErrorMessage = "La versión de la revisión no es válida. Recarga la página.")]
        public string VersionDecision { get; set; }

        // Contexto de retorno para conservar filtros y paginación después del POST/Redirect/GET.
        public int PaginaRetorno { get; set; } = 1;
        public string? CategoriaRetorno { get; set; }
        public string? BusquedaRetorno { get; set; }
        public string? TipoRetorno { get; set; }
    }

    public sealed class PamReabrirDecisionPreliminarInput
    {
        public long CambioPropuestoId { get; set; }

        public long RevisionId { get; set; }

        [Required(ErrorMessage = "La versión de la revisión es obligatoria. Recarga la página.")]
        [StringLength(18, ErrorMessage = "La versión de la revisión no es válida. Recarga la página.")]
        [RegularExpression(@"^(?:0x)?[0-9A-Fa-f]{16}$", ErrorMessage = "La versión de la revisión no es válida. Recarga la página.")]
        public string VersionDecision { get; set; }

        public int PaginaRetorno { get; set; } = 1;
        public string? CategoriaRetorno { get; set; }
        public string? BusquedaRetorno { get; set; }
        public string? TipoRetorno { get; set; }
    }

    public sealed class PamGuardarHallazgoPreliminarInput
    {
        [Range(1, long.MaxValue, ErrorMessage = "El hallazgo no es válido. Recarga la página e intenta nuevamente.")]
        public long HallazgoId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "La revisión no es válida. Recarga la página e intenta nuevamente.")]
        public long RevisionId { get; set; }

        [Required(ErrorMessage = "Selecciona cómo debe tratarse la cancelación reportada.")]
        [StringLength(40)]
        public string Clasificacion { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Selecciona un proyecto vigente válido.")]
        public long? ProyectoRelacionadoId { get; set; }

        [StringLength(1000)]
        public string? Nota { get; set; }

        [Required(ErrorMessage = "La versión de la revisión es obligatoria. Recarga la página.")]
        [StringLength(18)]
        [RegularExpression(@"^(?:0x)?[0-9A-Fa-f]{16}$", ErrorMessage = "La versión de la revisión no es válida. Recarga la página.")]
        public string VersionDecision { get; set; }

        public int PaginaRetorno { get; set; } = 1;
        public string? CategoriaRetorno { get; set; }
        public string? BusquedaRetorno { get; set; }
        public string? TipoRetorno { get; set; }
        public string? EstadoRevisionRetorno { get; set; }
    }

    public sealed class PamGuardarDecisionesCampoInput
    {
        [Range(1, long.MaxValue, ErrorMessage = "El proyecto de la revisión no es válido. Recarga la página.")]
        public long ProyectoVersionBaseId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "La revisión no contiene campos para clasificar.")]
        public List<PamDecisionCampoInput> Cambios { get; set; } = new();

        [StringLength(1000)]
        public string? NotaProyecto { get; set; }

        public int PaginaRetorno { get; set; } = 1;
        public string? CategoriaRetorno { get; set; }
        public string? BusquedaRetorno { get; set; }
        public string? TipoRetorno { get; set; }
        public string? EstadoRevisionRetorno { get; set; }
    }

    public sealed class PamDecisionCampoInput
    {
        [Range(1, long.MaxValue, ErrorMessage = "Uno de los cambios no es válido. Recarga la página.")]
        public long CambioPropuestoId { get; set; }

        [Required(ErrorMessage = "Selecciona una decisión para cada campo.")]
        [StringLength(20)]
        public string Decision { get; set; }

        public long? UltimaDecisionId { get; set; }
    }

    public class PamHallazgoVista
    {
        public long HallazgoId { get; set; }
        public long? CambioPropuestoId { get; set; }
        public long? RevisionId { get; set; }
        public string ClaveRecibida { get; set; }
        public string NombreRecibido { get; set; }
        public string FuenteDocumento { get; set; }
        public string UbicacionFuente { get; set; }
        public string Motivo { get; set; }
        public decimal? Confianza { get; set; }
        public string Clasificacion { get; set; }
        public string EstadoRevision { get; set; }
        public string ClasificacionPreliminar { get; set; }
        public long? ProyectoRelacionadoId { get; set; }
        public string ProyectoRelacionadoClave { get; set; }
        public string ProyectoRelacionadoNombre { get; set; }
        public string NotaPreliminar { get; set; }
        public string VersionDecision { get; set; }

        public bool EsCancelacion => string.Equals(
            Clasificacion, "Cancelación reportada", StringComparison.OrdinalIgnoreCase);

        public bool EstaRevisado => string.Equals(
            EstadoRevision, "Revisada", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class PamProyectoOpcionVista
    {
        public long ProyectoId { get; set; }
        public string ClaveProyecto { get; set; }
        public string NombreProyecto { get; set; }
    }

    public sealed class PamPreparacionAplicacionViewModel
    {
        public HeaderViewModel Header { get; set; }
        public PamLoteAnalisisCabecera Lote { get; set; } = new();
        public long AnalisisId { get; set; }
        public List<PamPreparacionAplicacionItem> Acciones { get; set; } = new();
        public List<PamPreparacionAplicacionItem> Pendientes { get; set; } = new();
        public PamPreparacionCamposResumen ResumenCampos { get; set; } = new();
        public List<PamPreparacionCampoGrupo> GruposCampos { get; set; } = new();
        public List<PamPreparacionValidacionItem> Validaciones { get; set; } = new();
        public List<PamPreparacionCambioCampoItem> CamposAplicables { get; set; } = new();
        public PamPaqueteCierreVista PaqueteCierre { get; set; } = new();
        public PamAplicacionPreflightViewModel Aplicacion { get; set; } = new();

        public int TotalAltas => Acciones.Count(item => item.TipoAccion == "Alta vigente");
        public int TotalVinculos => Acciones.Count(item => item.TipoAccion == "Vincular clave");
        public int TotalAntecedentesCancelados => Acciones.Count(item => item.TipoAccion == "Antecedente cancelado");
        public int TotalCancelacionesVinculadas => Acciones.Count(item => item.TipoAccion == "Cancelar existente");
        public int TotalDescartados => Acciones.Count(item => item.TipoAccion == "No aplicar");
        public int TotalPendientes => Pendientes.Count + ResumenCampos.Pendientes;
        public int TotalBloqueos => Validaciones.Count(item => item.Severidad == "Bloqueo");
        public int TotalAdvertencias => Validaciones.Count(item => item.Severidad == "Advertencia");
        public bool ListaParaConfirmar => TotalPendientes == 0;
        public bool ListaParaCongelar => ListaParaConfirmar && TotalBloqueos == 0;
        public bool TienePaqueteVigente => PaqueteCierre?.PaqueteAplicacionId > 0 && PaqueteCierre.EsVigente;
        public bool PaqueteAplicado => string.Equals(PaqueteCierre?.EstadoActual, "Aplicado", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class PamPreparacionAplicacionItem
    {
        public long RevisionId { get; set; }
        public string ClaveRecibida { get; set; }
        public string NombreRecibido { get; set; }
        public string EstadoRevision { get; set; }
        public string ClasificacionPreliminar { get; set; }
        public string TipoAccion { get; set; }
        public long? ProyectoRelacionadoId { get; set; }
        public string ProyectoRelacionadoClave { get; set; }
        public string ProyectoRelacionadoNombre { get; set; }
        public string FuenteDocumento { get; set; }
        public string UbicacionFuente { get; set; }
        public bool EsCancelacion { get; set; }
    }

    public sealed class PamPreparacionCamposResumen
    {
        public int Total { get; set; }
        public int Aplicar { get; set; }
        public int MantenerSql { get; set; }
        public int Omitir { get; set; }
        public int Pendientes { get; set; }
        public int ProyectosPendientes { get; set; }
        public int ProyectosRevisados { get; set; }
    }

    public sealed class PamPreparacionCambioCampoItem
    {
        public long CambioPropuestoId { get; set; }
        public long ProyectoVersionBaseId { get; set; }
        public string ClaveProyecto { get; set; }
        public string NombreProyecto { get; set; }
        public string Campo { get; set; }
        public string ValorActual { get; set; }
        public string ValorPropuesto { get; set; }
        public string DecisionCampo { get; set; }
        public string FuenteDocumento { get; set; }
        public string UbicacionFuente { get; set; }
    }

    public sealed class PamPreparacionCampoGrupo
    {
        public string Campo { get; set; }
        public string NivelRiesgo { get; set; }
        public int Total { get; set; }
        public int Aplicar { get; set; }
        public int MantenerSql { get; set; }
        public int Omitir { get; set; }
    }

    public sealed class PamPreparacionValidacionItem
    {
        public string Severidad { get; set; }
        public string Codigo { get; set; }
        public string Titulo { get; set; }
        public string Detalle { get; set; }
        public int Total { get; set; }
    }

    public sealed class PamPaqueteCierreVista
    {
        public long? PaqueteAplicacionId { get; set; }
        public Guid? PaqueteUid { get; set; }
        public string HashActual { get; set; }
        public string HashContenido { get; set; }
        public string EstadoActual { get; set; }
        public int TotalDetalles { get; set; }
        public int TotalCambiosCampo { get; set; }
        public int TotalAccionesClave { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime? FechaCreacionUtc { get; set; }
        public bool EsVigente { get; set; }
    }

    public sealed class PamCongelarPaqueteCierreInput
    {
        [Required(ErrorMessage = "Debes confirmar que revisaste el resumen final.")]
        public bool Confirmado { get; set; }

        [StringLength(1000)]
        public string? Nota { get; set; }
    }

    public sealed class PamPaqueteCierreResultado
    {
        public long PaqueteAplicacionId { get; set; }
        public Guid PaqueteUid { get; set; }
        public string HashContenido { get; set; }
        public bool Existente { get; set; }
        public int TotalDetalles { get; set; }
    }

    public sealed class PamAplicacionPreflightViewModel
    {
        public int TotalCambiosCampo { get; set; }
        public int TotalVersionesNuevas { get; set; }
        public int TotalClavesAlternas { get; set; }
        public int TotalAltasVigentes { get; set; }
        public int TotalAntecedentes { get; set; }
        public int TotalProyectosNuevos => TotalAltasVigentes + TotalAntecedentes;
        public int TotalFuentes { get; set; }
        public int TotalResultados { get; set; }
        public List<PamPreparacionValidacionItem> Validaciones { get; set; } = new();
        public int TotalBloqueos => Validaciones.Count(item => item.Severidad == "Bloqueo");
        public int TotalAdvertencias => Validaciones.Count(item => item.Severidad == "Advertencia");
        public bool YaAplicado { get; set; }
        public bool PuedeAplicar { get; set; }
        public string FraseConfirmacion { get; set; }
    }

    public sealed class PamAplicarPaqueteInput
    {
        [Required]
        public long PaqueteAplicacionId { get; set; }

        [Required(ErrorMessage = "Escribe la frase de confirmación mostrada.")]
        [StringLength(80)]
        public string Confirmacion { get; set; }

        [Required(ErrorMessage = "Debes confirmar la aplicación definitiva.")]
        public bool Confirmado { get; set; }
    }

    public sealed class PamAplicacionResultadoFinal
    {
        public long PaqueteAplicacionId { get; set; }
        public Guid PaqueteUid { get; set; }
        public bool Existente { get; set; }
        public int TotalVersionesNuevas { get; set; }
        public int TotalProyectosNuevos { get; set; }
        public int TotalClavesAlternas { get; set; }
        public int TotalCambios { get; set; }
        public int TotalResultados { get; set; }
    }

    public class PamAnalisisFiltro
    {
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 10;
        public string Categoria { get; set; }
        public string Busqueda { get; set; }
        // Se conserva para que los enlaces antiguos no fallen durante la transición de la vista.
        public string Tipo { get; set; }
        public string EstadoRevision { get; set; } = "Pendiente";
        public long? Cambio { get; set; }

        public void Normalizar()
        {
            Pagina = Math.Clamp(Pagina, 1, 100000);
            TamanoPagina = 10;
            Categoria = NormalizarCategoria(Categoria);
            Busqueda = string.IsNullOrWhiteSpace(Busqueda)
                ? null
                : Busqueda.Trim()[..Math.Min(Busqueda.Trim().Length, 200)];
            Tipo = string.IsNullOrWhiteSpace(Tipo) ? null : Tipo.Trim();
            EstadoRevision = NormalizarEstadoRevision(EstadoRevision);
            if (Cambio <= 0) Cambio = null;
        }

        private static string NormalizarCategoria(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria)) return "Prioritario";
            return categoria.Trim().ToUpperInvariant() switch
            {
                "PRIORITARIO" => "Prioritario",
                "INFORMATIVO" => "Informativo",
                "COMPLETAR" => "Completar",
                "EQUIVALENTE" => "Equivalente",
                "OTROS" => "Otros",
                "TODOS" => "Todos",
                _ => "Prioritario"
            };
        }

        private static string NormalizarEstadoRevision(string estado)
        {
            return estado?.Trim().ToUpperInvariant() switch
            {
                "REVISADA" => "Revisada",
                "TODAS" => "Todas",
                _ => "Pendiente"
            };
        }
    }
}
