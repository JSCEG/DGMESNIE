using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NSIE.Models.ProyectosPrivados;
using NSIE.Servicios.Interfaces;

namespace NSIE.Servicios
{
    public class RepositorioProyectosPrivados : IRepositorioProyectosPrivados
    {
        private readonly string _conn;

        public RepositorioProyectosPrivados(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection Connection => new SqlConnection(_conn);

        // ── Projects ─────────────────────────────────────────────────────────
        public async Task<List<Proyecto>> ObtenerProyectosAsync(string buscar, string tecnologia, int? clasificacionId, int? prioridadId, int? semaforoId)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT 
                        p.*, 
                        e.Nombre AS EmpresaNombre,
                        cc.Nombre AS ClasificacionNombre,
                        cp.Nombre AS PrioridadNombre,
                        cs.Nombre AS SemaforoNombre,
                        cs.ColorHex AS SemaforoColorHex
                    FROM dgmesnie.Proyecto p
                    LEFT JOIN dgmesnie.Empresa e ON e.EmpresaId = p.EmpresaId
                    LEFT JOIN dgmesnie.CatClasificacion cc ON cc.ClasificacionId = p.ClasificacionId
                    LEFT JOIN dgmesnie.CatPrioridad cp ON cp.PrioridadId = p.PrioridadId
                    LEFT JOIN dgmesnie.CatSemaforo cs ON cs.SemaforoId = p.SemaforoId
                    WHERE p.Activo = 1";

                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    sql += " AND (p.Nombre LIKE @Buscar OR p.Promovente LIKE @Buscar OR p.GrupoEconomico LIKE @Buscar OR p.NumeroPermiso LIKE @Buscar OR p.EntidadFederativa LIKE @Buscar OR p.Municipio LIKE @Buscar)";
                    parameters.Add("Buscar", $"%{buscar.Trim()}%");
                }

                if (!string.IsNullOrWhiteSpace(tecnologia))
                {
                    sql += " AND p.Tipo = @Tecnologia";
                    parameters.Add("Tecnologia", tecnologia.Trim());
                }

                if (clasificacionId.HasValue)
                {
                    sql += " AND p.ClasificacionId = @ClasificacionId";
                    parameters.Add("ClasificacionId", clasificacionId.Value);
                }

                if (prioridadId.HasValue)
                {
                    sql += " AND p.PrioridadId = @PrioridadId";
                    parameters.Add("PrioridadId", prioridadId.Value);
                }

                if (semaforoId.HasValue)
                {
                    sql += " AND p.SemaforoId = @SemaforoId";
                    parameters.Add("SemaforoId", semaforoId.Value);
                }

                sql += " ORDER BY p.Nombre";

                return (await db.QueryAsync<Proyecto>(sql, parameters)).ToList();
            }
        }

        public async Task<Proyecto> ObtenerProyectoPorIdAsync(int id)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT 
                        p.*, 
                        e.Nombre AS EmpresaNombre,
                        cc.Nombre AS ClasificacionNombre,
                        cp.Nombre AS PrioridadNombre,
                        cs.Nombre AS SemaforoNombre,
                        cs.ColorHex AS SemaforoColorHex
                    FROM dgmesnie.Proyecto p
                    LEFT JOIN dgmesnie.Empresa e ON e.EmpresaId = p.EmpresaId
                    LEFT JOIN dgmesnie.CatClasificacion cc ON cc.ClasificacionId = p.ClasificacionId
                    LEFT JOIN dgmesnie.CatPrioridad cp ON cp.PrioridadId = p.PrioridadId
                    LEFT JOIN dgmesnie.CatSemaforo cs ON cs.SemaforoId = p.SemaforoId
                    WHERE p.ProyectoId = @Id AND p.Activo = 1";

                return await db.QueryFirstOrDefaultAsync<Proyecto>(sql, new { Id = id });
            }
        }

        public async Task<int> CrearProyectoAsync(ProyectoForm form, string usuario)
        {
            using (var db = Connection)
            {
                var sql = @"
                    INSERT INTO dgmesnie.Proyecto (
                        Status, Nombre, EmpresaId, Promovente, GrupoEconomico, Origen, Anio,
                        AdicionesSustituciones, ContratoUnidad, Tipo, Renovable, CapacidadMW, Mes,
                        GerenciaControl, RegionTransmision, EntidadFederativa, Municipio, Longitud, Latitud,
                        Firmes, PorcentajeConstruccion, FolioEvIS_MISSE, StatusEvIS_MISSE, StatusCPLI,
                        ConflictosSociales, NumeroPermiso, FechaInicioObras, FechaTerminacionObras,
                        FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE,
                        Categoria, InteresadaEnContinuar, ObservacionesUEVISPI, ResumenCaso,
                        PropuestaAtencion, RequiereAlmacenamiento, SiguientesPasos, ClasificacionId,
                        PrioridadId, SemaforoId, RazonesBreves, TramitesSemarnat, EstatusSemarnat,
                        ObservacionesSemarnat, Fuente, SemaforoPPT, FechaUltimaActualizacion,
                        FuenteUltimaActualizacion, CreadoPor, CreadoEn
                    ) VALUES (
                        @Status, @Nombre, @EmpresaId, @Promovente, @GrupoEconomico, @Origen, @Anio,
                        @AdicionesSustituciones, @ContratoUnidad, @Tipo, @Renovable, @CapacidadMW, @Mes,
                        @GerenciaControl, @RegionTransmision, @EntidadFederativa, @Municipio, @Longitud, @Latitud,
                        @Firmes, @PorcentajeConstruccion, @FolioEvIS_MISSE, @StatusEvIS_MISSE, @StatusCPLI,
                        @ConflictosSociales, @NumeroPermiso, @FechaInicioObras, @FechaTerminacionObras,
                        @FechaEntradaOperacion, @EstadoProgramaObras, @TramiteCNE, @ObservacionesCNE,
                        @Categoria, @InteresadaEnContinuar, @ObservacionesUEVISPI, @ResumenCaso,
                        @PropuestaAtencion, @RequiereAlmacenamiento, @SiguientesPasos, @ClasificacionId,
                        @PrioridadId, @SemaforoId, @RazonesBreves, @TramitesSemarnat, @EstatusSemarnat,
                        @ObservacionesSemarnat, @Fuente, @SemaforoPPT, @FechaUltimaActualizacion,
                        @FuenteUltimaActualizacion, @Usuario, GETDATE()
                    );
                    SELECT SCOPE_IDENTITY();";

                var id = await db.ExecuteScalarAsync<int>(sql, new {
                    form.Status, form.Nombre, form.EmpresaId, form.Promovente, form.GrupoEconomico, form.Origen, form.Anio,
                    form.AdicionesSustituciones, form.ContratoUnidad, form.Tipo, form.Renovable, form.CapacidadMW, form.Mes,
                    form.GerenciaControl, form.RegionTransmision, form.EntidadFederativa, form.Municipio, form.Longitud, form.Latitud,
                    form.Firmes, form.PorcentajeConstruccion, form.FolioEvIS_MISSE, form.StatusEvIS_MISSE, form.StatusCPLI,
                    form.ConflictosSociales, form.NumeroPermiso, form.FechaInicioObras, form.FechaTerminacionObras,
                    form.FechaEntradaOperacion, form.EstadoProgramaObras, form.TramiteCNE, form.ObservacionesCNE,
                    form.Categoria, form.InteresadaEnContinuar, form.ObservacionesUEVISPI, form.ResumenCaso,
                    form.PropuestaAtencion, form.RequiereAlmacenamiento, form.SiguientesPasos, form.ClasificacionId,
                    form.PrioridadId, form.SemaforoId, form.RazonesBreves, form.TramitesSemarnat, form.EstatusSemarnat,
                    form.ObservacionesSemarnat, form.Fuente, form.SemaforoPPT, form.FechaUltimaActualizacion,
                    form.FuenteUltimaActualizacion, Usuario = usuario
                });

                // Write initial JSON snapshot to Bitacora as "Carga Inicial"
                var proj = await ObtenerProyectoPorIdAsync(id);
                var snapshot = JsonConvert.SerializeObject(proj);
                await db.ExecuteAsync(@"
                    INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, SnapshotProyectoJSON, CreadoPor, CreadoEn)
                    VALUES (@ProyectoId, GETDATE(), 'CREACIÓN', 'Carga inicial del proyecto.', @Snapshot, @Usuario, GETDATE())",
                    new { ProyectoId = id, Snapshot = snapshot, Usuario = usuario });

                return id;
            }
        }

        public async Task<bool> ActualizarProyectoAsync(ProyectoForm form, string usuario)
        {
            if (!form.ProyectoId.HasValue) return false;
            var id = form.ProyectoId.Value;

            using (var db = Connection)
            {
                if (db.State != ConnectionState.Open) db.Open();
                using (var tx = db.BeginTransaction())
                {
                    try
                    {
                        // 1. Get current state from database
                        var current = await db.QueryFirstOrDefaultAsync<Proyecto>(
                            "SELECT * FROM dgmesnie.Proyecto WHERE ProyectoId = @Id AND Activo = 1",
                            new { Id = id }, tx);

                        if (current == null) return false;

                        // 2. Perform field-by-field audit comparison
                        var auditLogs = new List<dynamic>();
                        void CheckChange(string fieldName, object oldVal, object newVal)
                        {
                            var oldStr = oldVal?.ToString() ?? "";
                            var newStr = newVal?.ToString() ?? "";
                            if (oldStr != newStr)
                            {
                                auditLogs.Add(new { Campo = fieldName, Anterior = oldStr, Nuevo = newStr });
                            }
                        }

                        CheckChange("Status", current.Status, form.Status);
                        CheckChange("Nombre real", current.Nombre, form.Nombre);
                        CheckChange("EmpresaId", current.EmpresaId, form.EmpresaId);
                        CheckChange("Promovente", current.Promovente, form.Promovente);
                        CheckChange("Grupo Económico", current.GrupoEconomico, form.GrupoEconomico);
                        CheckChange("Origen", current.Origen, form.Origen);
                        CheckChange("Año", current.Anio, form.Anio);
                        CheckChange("Adiciones o sustituciones", current.AdicionesSustituciones, form.AdicionesSustituciones);
                        CheckChange("Contrato o unidad", current.ContratoUnidad, form.ContratoUnidad);
                        CheckChange("Tipo", current.Tipo, form.Tipo);
                        CheckChange("Renovable", current.Renovable, form.Renovable);
                        CheckChange("CapacidadMW", current.CapacidadMW, form.CapacidadMW);
                        CheckChange("Mes", current.Mes, form.Mes);
                        CheckChange("Gerencia de control", current.GerenciaControl, form.GerenciaControl);
                        CheckChange("Región de transmisión", current.RegionTransmision, form.RegionTransmision);
                        CheckChange("Entidad Federativa", current.EntidadFederativa, form.EntidadFederativa);
                        CheckChange("Municipio", current.Municipio, form.Municipio);
                        CheckChange("Longitud", current.Longitud, form.Longitud);
                        CheckChange("Latitud", current.Latitud, form.Latitud);
                        CheckChange("Firmes", current.Firmes, form.Firmes);
                        CheckChange("Porcentaje de Construcción", current.PorcentajeConstruccion, form.PorcentajeConstruccion);
                        CheckChange("Folio EvIS/MISSE", current.FolioEvIS_MISSE, form.FolioEvIS_MISSE);
                        CheckChange("Status EvIS/MISSE", current.StatusEvIS_MISSE, form.StatusEvIS_MISSE);
                        CheckChange("Status CPLI", current.StatusCPLI, form.StatusCPLI);
                        CheckChange("Conflictos sociales detectados", current.ConflictosSociales, form.ConflictosSociales);
                        CheckChange("Número de Permiso", current.NumeroPermiso, form.NumeroPermiso);
                        CheckChange("Inicio de Obras", current.FechaInicioObras?.ToString("yyyy-MM-dd"), form.FechaInicioObras?.ToString("yyyy-MM-dd"));
                        CheckChange("Terminación de Obras", current.FechaTerminacionObras?.ToString("yyyy-MM-dd"), form.FechaTerminacionObras?.ToString("yyyy-MM-dd"));
                        CheckChange("Entrada en Operación", current.FechaEntradaOperacion?.ToString("yyyy-MM-dd"), form.FechaEntradaOperacion?.ToString("yyyy-MM-dd"));
                        CheckChange("Estado actual del Programa de Obras", current.EstadoProgramaObras, form.EstadoProgramaObras);
                        CheckChange("Trámite en proceso con CNE", current.TramiteCNE, form.TramiteCNE);
                        CheckChange("Observaciones CNE", current.ObservacionesCNE, form.ObservacionesCNE);
                        CheckChange("Categoría", current.Categoria, form.Categoria);
                        CheckChange("Interesada en Continuar", current.InteresadaEnContinuar, form.InteresadaEnContinuar);
                        CheckChange("Observaciones UEVISPI", current.ObservacionesUEVISPI, form.ObservacionesUEVISPI);
                        CheckChange("Resumen del Caso", current.ResumenCaso, form.ResumenCaso);
                        CheckChange("Propuesta de Atención", current.PropuestaAtencion, form.PropuestaAtencion);
                        CheckChange("Requiere Almacenamiento", current.RequiereAlmacenamiento, form.RequiereAlmacenamiento);
                        CheckChange("Siguientes Pasos", current.SiguientesPasos, form.SiguientesPasos);
                        CheckChange("ClasificacionId", current.ClasificacionId, form.ClasificacionId);
                        CheckChange("PrioridadId", current.PrioridadId, form.PrioridadId);
                        CheckChange("SemaforoId", current.SemaforoId, form.SemaforoId);
                        CheckChange("Razones (Breves)", current.RazonesBreves, form.RazonesBreves);
                        CheckChange("Trámites SEMARNAT", current.TramitesSemarnat, form.TramitesSemarnat);
                        CheckChange("Estatus SEMARNAT", current.EstatusSemarnat, form.EstatusSemarnat);
                        CheckChange("Observaciones SEMARNAT", current.ObservacionesSemarnat, form.ObservacionesSemarnat);
                        CheckChange("Fuente", current.Fuente, form.Fuente);
                        CheckChange("Semáforo PPT", current.SemaforoPPT, form.SemaforoPPT);
                        CheckChange("Última actualización", current.FechaUltimaActualizacion?.ToString("yyyy-MM-dd"), form.FechaUltimaActualizacion?.ToString("yyyy-MM-dd"));
                        CheckChange("Fuente última actualización", current.FuenteUltimaActualizacion, form.FuenteUltimaActualizacion);

                        // 3. Write changed fields to HistorialProyecto
                        foreach (var log in auditLogs)
                        {
                            await db.ExecuteAsync(@"
                                INSERT INTO dgmesnie.HistorialProyecto (ProyectoId, Campo, ValorAnterior, ValorNuevo, CambiadoEn, CambiadoPor)
                                VALUES (@ProyectoId, @Campo, @Anterior, @Nuevo, GETDATE(), @Usuario)",
                                new { ProyectoId = id, Campo = log.Campo, Anterior = log.Anterior, Nuevo = log.Nuevo, Usuario = usuario }, tx);
                        }

                        // 4. Update dgmesnie.Proyecto
                        var updateSql = @"
                            UPDATE dgmesnie.Proyecto SET
                                Status = @Status, Nombre = @Nombre, EmpresaId = @EmpresaId, Promovente = @Promovente,
                                GrupoEconomico = @GrupoEconomico, Origen = @Origen, Anio = @Anio,
                                AdicionesSustituciones = @AdicionesSustituciones, ContratoUnidad = @ContratoUnidad,
                                Tipo = @Tipo, Renovable = @Renovable, CapacidadMW = @CapacidadMW, Mes = @Mes,
                                GerenciaControl = @GerenciaControl, RegionTransmision = @RegionTransmision,
                                EntidadFederativa = @EntidadFederativa, Municipio = @Municipio, Longitud = @Longitud, Latitud = @Latitud,
                                Firmes = @Firmes, PorcentajeConstruccion = @PorcentajeConstruccion, FolioEvIS_MISSE = @FolioEvIS_MISSE,
                                StatusEvIS_MISSE = @StatusEvIS_MISSE, StatusCPLI = @StatusCPLI, ConflictosSociales = @ConflictosSociales,
                                NumeroPermiso = @NumeroPermiso, FechaInicioObras = @FechaInicioObras, FechaTerminacionObras = @FechaTerminacionObras,
                                FechaEntradaOperacion = @FechaEntradaOperacion, EstadoProgramaObras = @EstadoProgramaObras,
                                TramiteCNE = @TramiteCNE, ObservacionesCNE = @ObservacionesCNE, Categoria = @Categoria,
                                InteresadaEnContinuar = @InteresadaEnContinuar, ObservacionesUEVISPI = @ObservacionesUEVISPI,
                                ResumenCaso = @ResumenCaso, PropuestaAtencion = @PropuestaAtencion, RequiereAlmacenamiento = @RequiereAlmacenamiento,
                                SiguientesPasos = @SiguientesPasos, ClasificacionId = @ClasificacionId, PrioridadId = @PrioridadId,
                                SemaforoId = @SemaforoId, RazonesBreves = @RazonesBreves, TramitesSemarnat = @TramitesSemarnat,
                                EstatusSemarnat = @EstatusSemarnat, ObservacionesSemarnat = @ObservacionesSemarnat, Fuente = @Fuente,
                                SemaforoPPT = @SemaforoPPT, FechaUltimaActualizacion = @FechaUltimaActualizacion,
                                FuenteUltimaActualizacion = @FuenteUltimaActualizacion, ActualizadoEn = GETDATE(), ActualizadoPor = @Usuario
                            WHERE ProyectoId = @ProyectoId";

                        await db.ExecuteAsync(updateSql, new {
                            form.Status, form.Nombre, form.EmpresaId, form.Promovente, form.GrupoEconomico, form.Origen, form.Anio,
                            form.AdicionesSustituciones, form.ContratoUnidad, form.Tipo, form.Renovable, form.CapacidadMW, form.Mes,
                            form.GerenciaControl, form.RegionTransmision, form.EntidadFederativa, form.Municipio, form.Longitud, form.Latitud,
                            form.Firmes, form.PorcentajeConstruccion, form.FolioEvIS_MISSE, form.StatusEvIS_MISSE, form.StatusCPLI,
                            form.ConflictosSociales, form.NumeroPermiso, form.FechaInicioObras, form.FechaTerminacionObras,
                            form.FechaEntradaOperacion, form.EstadoProgramaObras, form.TramiteCNE, form.ObservacionesCNE,
                            form.Categoria, form.InteresadaEnContinuar, form.ObservacionesUEVISPI, form.ResumenCaso,
                            form.PropuestaAtencion, form.RequiereAlmacenamiento, form.SiguientesPasos, form.ClasificacionId,
                            form.PrioridadId, form.SemaforoId, form.RazonesBreves, form.TramitesSemarnat, form.EstatusSemarnat,
                            form.ObservacionesSemarnat, form.Fuente, form.SemaforoPPT, form.FechaUltimaActualizacion,
                            form.FuenteUltimaActualizacion, Usuario = usuario, ProyectoId = id
                        }, tx);

                        // 5. Generate a Bitacora entry for this Direct Update containing the JSON Snapshot
                        var updatedProj = await db.QueryFirstOrDefaultAsync<Proyecto>(
                            "SELECT * FROM dgmesnie.Proyecto WHERE ProyectoId = @Id", new { Id = id }, tx);
                        var snapshot = JsonConvert.SerializeObject(updatedProj);

                        await db.ExecuteAsync(@"
                            INSERT INTO dgmesnie.BitacoraProyecto (ProyectoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, SnapshotProyectoJSON, CreadoPor, CreadoEn)
                            VALUES (@ProyectoId, GETDATE(), 'EDICIÓN DIRECTA', 'Actualización manual de datos generales.', @Snapshot, @Usuario, GETDATE())",
                            new { ProyectoId = id, Snapshot = snapshot, Usuario = usuario }, tx);

                        tx.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> EliminarProyectoAsync(int id)
        {
            using (var db = Connection)
            {
                var sql = "UPDATE dgmesnie.Proyecto SET Activo = 0 WHERE ProyectoId = @Id";
                var rows = await db.ExecuteAsync(sql, new { Id = id });
                return rows > 0;
            }
        }

        // ── Sub-resources for Detail Tabs ────────────────────────────────────
        public async Task<List<TramiteProyecto>> ObtenerTramitesPorProyectoAsync(int proyectoId)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT t.*, c.Nombre AS EstatusTramiteNombre
                    FROM dgmesnie.TramiteProyecto t
                    LEFT JOIN dgmesnie.CatEstatusTramite c ON c.EstatusTramiteId = t.EstatusTramiteId
                    WHERE t.ProyectoId = @ProyectoId AND t.Activo = 1
                    ORDER BY t.FechaIngreso DESC";
                return (await db.QueryAsync<TramiteProyecto>(sql, new { ProyectoId = proyectoId })).ToList();
            }
        }

        public async Task<List<BitacoraProyecto>> ObtenerBitacorasPorProyectoAsync(int proyectoId)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT b.*, r.Titulo AS ReunionTitulo, r.FechaReunion AS ReunionFecha, cv.Nombre AS ValoracionMinutaNombre
                    FROM dgmesnie.BitacoraProyecto b
                    LEFT JOIN dgmesnie.Reunion r ON r.ReunionId = b.ReunionId
                    LEFT JOIN dgmesnie.CatValoracionMinuta cv ON cv.ValoracionMinutaId = b.ValoracionMinutaId
                    WHERE b.ProyectoId = @ProyectoId
                    ORDER BY b.FechaEvento DESC";
                return (await db.QueryAsync<BitacoraProyecto>(sql, new { ProyectoId = proyectoId })).ToList();
            }
        }

        public async Task<List<AccionSeguimiento>> ObtenerAccionesPorProyectoAsync(int proyectoId)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT a.*, cs.Nombre AS SemaforoNombre, cs.ColorHex AS SemaforoColorHex
                    FROM dgmesnie.AccionSeguimiento a
                    LEFT JOIN dgmesnie.CatSemaforo cs ON cs.SemaforoId = a.SemaforoId
                    WHERE a.ProyectoId = @ProyectoId
                    ORDER BY a.FechaCompromiso DESC";
                return (await db.QueryAsync<AccionSeguimiento>(sql, new { ProyectoId = proyectoId })).ToList();
            }
        }

        public async Task<List<Documento>> ObtenerDocumentosPorProyectoAsync(int proyectoId)
        {
            using (var db = Connection)
            {
                // Retrieve documents linked directly or via Bitacora entries of this project
                var sql = @"
                    SELECT DISTINCT d.*, ct.Nombre AS TipoDocumentoNombre
                    FROM dgmesnie.Documento d
                    JOIN dgmesnie.CatTipoDocumento ct ON ct.TipoDocumentoId = d.TipoDocumentoId
                    LEFT JOIN dgmesnie.BitacoraProyecto b ON b.DocumentoId = d.DocumentoId
                    LEFT JOIN dgmesnie.Reunion r ON r.DocumentoId = d.DocumentoId
                    WHERE (b.ProyectoId = @ProyectoId OR r.ReunionId IN (SELECT ReunionId FROM dgmesnie.BitacoraProyecto WHERE ProyectoId = @ProyectoId))
                      AND d.Activo = 1
                    ORDER BY d.FechaDocumento DESC";
                return (await db.QueryAsync<Documento>(sql, new { ProyectoId = proyectoId })).ToList();
            }
        }

        public async Task<List<HistorialProyecto>> ObtenerHistorialPorProyectoAsync(int proyectoId)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT h.*, p.Nombre AS ProyectoNombre
                    FROM dgmesnie.HistorialProyecto h
                    JOIN dgmesnie.Proyecto p ON p.ProyectoId = h.ProyectoId
                    WHERE h.ProyectoId = @ProyectoId
                    ORDER BY h.CambiadoEn DESC";
                return (await db.QueryAsync<HistorialProyecto>(sql, new { ProyectoId = proyectoId })).ToList();
            }
        }

        // ── Reuniones / Minutas ──────────────────────────────────────────────
        public async Task<List<Reunion>> ObtenerReunionesAsync()
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT r.*, d.Titulo AS DocumentoTitulo, d.SharePointUrl AS DocumentoSharePointUrl
                    FROM dgmesnie.Reunion r
                    LEFT JOIN dgmesnie.Documento d ON d.DocumentoId = r.DocumentoId
                    ORDER BY r.FechaReunion DESC";
                return (await db.QueryAsync<Reunion>(sql)).ToList();
            }
        }

        public async Task<Reunion> ObtenerReunionPorIdAsync(int id)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT r.*, d.Titulo AS DocumentoTitulo, d.SharePointUrl AS DocumentoSharePointUrl
                    FROM dgmesnie.Reunion r
                    LEFT JOIN dgmesnie.Documento d ON d.DocumentoId = r.DocumentoId
                    WHERE r.ReunionId = @Id";
                return await db.QueryFirstOrDefaultAsync<Reunion>(sql, new { Id = id });
            }
        }

        public async Task<int> GuardarReunionMinutaAsync(ReunionForm form, string usuario)
        {
            using (var db = Connection)
            {
                if (db.State != ConnectionState.Open) db.Open();
                using (var tx = db.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert SharePoint Document metadata if provided
                        int? docId = null;
                        if (!string.IsNullOrWhiteSpace(form.SharePointUrl))
                        {
                            var filename = string.IsNullOrWhiteSpace(form.Titulo) ? "Minuta.pdf" : $"{form.Titulo}.pdf";
                            docId = await db.ExecuteScalarAsync<int>(@"
                                INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, SubidoPor, SubidoEn)
                                VALUES (1, @Titulo, @Url, @Filename, @Fecha, @Usuario, GETDATE());
                                SELECT SCOPE_IDENTITY();",
                                new { Titulo = form.Titulo, Url = form.SharePointUrl, Filename = filename, Fecha = form.FechaReunion, Usuario = usuario }, tx);
                        }

                        // 2. Insert Reunion
                        var reunionId = await db.ExecuteScalarAsync<int>(@"
                            INSERT INTO dgmesnie.Reunion (Titulo, FechaReunion, Modalidad, Lugar, Objetivo, Asistentes, DocumentoId, CreadoEn, CreadoPor)
                            VALUES (@Titulo, @Fecha, @Modalidad, @Lugar, @Objetivo, @Asistentes, @DocId, GETDATE(), @Usuario);
                            SELECT SCOPE_IDENTITY();",
                            new { form.Titulo, Fecha = form.FechaReunion, form.Modalidad, form.Lugar, form.Objetivo, form.Asistentes, DocId = docId, Usuario = usuario }, tx);

                        // 3. Loop through associated projects and process their changes & agreements
                        foreach (var ent in form.ProyectoEntradas)
                        {
                            // A. Fetch current state of project before update
                            var current = await db.QueryFirstOrDefaultAsync<Proyecto>(
                                "SELECT * FROM dgmesnie.Proyecto WHERE ProyectoId = @Id", new { Id = ent.ProyectoId }, tx);

                            if (current == null) continue;

                            // B. Save a Snapshot JSON of the project BEFORE applying changes
                            var snapshot = JsonConvert.SerializeObject(current);

                            // C. Insert BitacoraProyecto
                            var bitacoraId = await db.ExecuteScalarAsync<int>(@"
                                INSERT INTO dgmesnie.BitacoraProyecto (
                                    ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionMinutaId, ValoracionTexto,
                                    ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, SnapshotProyectoJSON,
                                    ProcesadoPor, CreadoPor, CreadoEn
                                ) VALUES (
                                    @ProyectoId, @ReunionId, @DocId, @Fecha, @ValoracionId, @ValoracionTexto,
                                    @ResumenAcuerdos, @Compromisos, @Riesgos, @Snapshot, @Usuario, @Usuario, GETDATE()
                                );
                                SELECT SCOPE_IDENTITY();",
                                new {
                                    ProyectoId = ent.ProyectoId, ReunionId = reunionId, DocId = docId, Fecha = form.FechaReunion,
                                    ValoracionId = ent.ValoracionMinutaId, ValoracionTexto = ent.ValoracionTexto,
                                    ResumenAcuerdos = ent.ResumenAcuerdos, Compromisos = ent.CompromisosSiguientesPasos,
                                    Riesgos = ent.RiesgosObservaciones, Snapshot = snapshot, Usuario = usuario
                                }, tx);

                            // D. Apply project updates directly to dgmesnie.Proyecto
                            var auditLogs = new List<dynamic>();
                            void CheckChange(string fieldName, object oldVal, object newVal)
                            {
                                var oldStr = oldVal?.ToString() ?? "";
                                var newStr = newVal?.ToString() ?? "";
                                if (oldStr != newStr)
                                {
                                    auditLogs.Add(new { Campo = fieldName, Anterior = oldStr, Nuevo = newStr });
                                }
                            }

                            CheckChange("Status", current.Status, ent.Status);
                            CheckChange("CapacidadMW", current.CapacidadMW, ent.CapacidadMW);
                            CheckChange("Categoría", current.Categoria, ent.Categoria);
                            CheckChange("ClasificacionId", current.ClasificacionId, ent.ClasificacionId);
                            CheckChange("PrioridadId", current.PrioridadId, ent.PrioridadId);
                            CheckChange("SemaforoId", current.SemaforoId, ent.SemaforoId);
                            CheckChange("Siguientes Pasos", current.SiguientesPasos, ent.SiguientesPasos);

                            // E. Write project history
                            foreach (var log in auditLogs)
                            {
                                await db.ExecuteAsync(@"
                                    INSERT INTO dgmesnie.HistorialProyecto (ProyectoId, BitacoraProyectoId, DocumentoId, Campo, ValorAnterior, ValorNuevo, MotivoCambio, CambiadoEn, CambiadoPor)
                                    VALUES (@ProyectoId, @BitacoraId, @DocId, @Campo, @Anterior, @Nuevo, @Motivo, GETDATE(), @Usuario)",
                                    new { ProyectoId = ent.ProyectoId, BitacoraId = bitacoraId, DocId = docId, Campo = log.Campo, Anterior = log.Anterior, Nuevo = log.Nuevo, Motivo = $"Reunión: {form.Titulo}", Usuario = usuario }, tx);
                            }

                            // F. Apply direct project update
                            await db.ExecuteAsync(@"
                                UPDATE dgmesnie.Proyecto SET
                                    Status = ISNULL(@Status, Status),
                                    CapacidadMW = ISNULL(@CapacidadMW, CapacidadMW),
                                    Categoria = ISNULL(@Categoria, Categoria),
                                    ClasificacionId = ISNULL(@ClasificacionId, ClasificacionId),
                                    PrioridadId = ISNULL(@PrioridadId, PrioridadId),
                                    SemaforoId = ISNULL(@SemaforoId, SemaforoId),
                                    SiguientesPasos = ISNULL(@SiguientesPasos, SiguientesPasos),
                                    FechaUltimaActualizacion = @Fecha,
                                    FuenteUltimaActualizacion = @Fuente,
                                    ActualizadoEn = GETDATE(),
                                    ActualizadoPor = @Usuario
                                WHERE ProyectoId = @ProyectoId",
                                new {
                                    ProyectoId = ent.ProyectoId, Status = ent.Status, CapacidadMW = ent.CapacidadMW,
                                    Categoria = ent.Categoria, ClasificacionId = ent.ClasificacionId, PrioridadId = ent.PrioridadId,
                                    SemaforoId = ent.SemaforoId, SiguientesPasos = ent.SiguientesPasos, Fecha = form.FechaReunion,
                                    Fuente = $"Reunión: {form.Titulo}", Usuario = usuario
                                }, tx);

                            // G. Insert spawned follow-up actions (Acciones de seguimiento)
                            foreach (var acc in ent.AccionesAInsertar)
                            {
                                if (string.IsNullOrWhiteSpace(acc.Titulo)) continue;

                                // Resolve initial semaphore based on compromise date rules
                                int semId = 3; // Verde (SemaforoId = 3 by default)
                                if (acc.FechaCompromiso.HasValue)
                                {
                                    var days = (acc.FechaCompromiso.Value.Date - DateTime.Today.Date).TotalDays;
                                    if (days < 0) semId = 1; // Rojo (SemaforoId = 1)
                                    else if (days <= 7) semId = 2; // Amarillo (SemaforoId = 2)
                                }

                                await db.ExecuteAsync(@"
                                    INSERT INTO dgmesnie.AccionSeguimiento (
                                        ProyectoId, BitacoraProyectoId, Titulo, Descripcion, ResponsableUsuarioId,
                                        ResponsableNombre, FechaCompromiso, Estatus, SemaforoId, CreadoEn, CreadoPor
                                    ) VALUES (
                                        @ProyectoId, @BitacoraId, @Titulo, @Descripcion, @RespId,
                                        @RespNombre, @FechaCompromiso, 'Pendiente', @SemaforoId, GETDATE(), @Usuario
                                    )",
                                    new {
                                        ProyectoId = ent.ProyectoId, BitacoraId = bitacoraId, Titulo = acc.Titulo,
                                        Descripcion = acc.Descripcion, RespId = acc.ResponsableUsuarioId,
                                        RespNombre = acc.ResponsableNombre, FechaCompromiso = acc.FechaCompromiso,
                                        SemaforoId = semId, Usuario = usuario
                                    }, tx);
                            }
                        }

                        tx.Commit();
                        return reunionId;
                    }
                    catch (Exception)
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // ── Follow-up Actions ────────────────────────────────────────────────
        public async Task<List<AccionSeguimiento>> ObtenerAccionesSeguimientoAsync(int? proyectoId, string estatus, int? semaforoId)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT a.*, p.Nombre AS ProyectoNombre, cs.Nombre AS SemaforoNombre, cs.ColorHex AS SemaforoColorHex
                    FROM dgmesnie.AccionSeguimiento a
                    JOIN dgmesnie.Proyecto p ON p.ProyectoId = a.ProyectoId
                    LEFT JOIN dgmesnie.CatSemaforo cs ON cs.SemaforoId = a.SemaforoId
                    WHERE 1 = 1";

                var parameters = new DynamicParameters();

                if (proyectoId.HasValue)
                {
                    sql += " AND a.ProyectoId = @ProyectoId";
                    parameters.Add("ProyectoId", proyectoId.Value);
                }

                if (!string.IsNullOrWhiteSpace(estatus))
                {
                    sql += " AND a.Estatus = @Estatus";
                    parameters.Add("Estatus", estatus.Trim());
                }

                if (semaforoId.HasValue)
                {
                    sql += " AND a.SemaforoId = @SemaforoId";
                    parameters.Add("SemaforoId", semaforoId.Value);
                }

                sql += " ORDER BY a.FechaCompromiso";

                var list = (await db.QueryAsync<AccionSeguimiento>(sql, parameters)).ToList();

                // Dynamically recalculate semaphore colors if the action is not closed (Closed / Cerrada)
                var today = DateTime.Today;
                foreach (var item in list)
                {
                    if (string.Equals(item.Estatus, "Cerrada", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(item.Estatus, "Cancelada", StringComparison.OrdinalIgnoreCase))
                    {
                        // Ensure closed/cancelled actions show as Verde or neutral gray,
                        // Cerrada -> Green (#00B050)
                        if (string.Equals(item.Estatus, "Cerrada", StringComparison.OrdinalIgnoreCase) && item.SemaforoColorHex != "#00B050")
                        {
                            item.SemaforoColorHex = "#00B050";
                            item.SemaforoNombre = "Verde";
                        }
                    }
                    else if (item.FechaCompromiso.HasValue)
                    {
                        var limit = item.FechaCompromiso.Value.Date;
                        if (limit < today)
                        {
                            item.SemaforoColorHex = "#C00000"; // Rojo
                            item.SemaforoNombre = "Rojo";
                        }
                        else if ((limit - today).TotalDays <= 7)
                        {
                            item.SemaforoColorHex = "#FFC000"; // Amarillo
                            item.SemaforoNombre = "Amarillo";
                        }
                        else
                        {
                            item.SemaforoColorHex = "#00B050"; // Verde
                            item.SemaforoNombre = "Verde";
                        }
                    }
                }

                return list;
            }
        }

        public async Task<bool> ActualizarEstatusAccionAsync(int accionId, string estatus, string comentarios, string usuario)
        {
            using (var db = Connection)
            {
                // Resolve correct semaphore color based on target status
                int? semId = null;
                if (string.Equals(estatus, "Cerrada", StringComparison.OrdinalIgnoreCase))
                {
                    semId = 3; // Verde (SemaforoId = 3)
                }

                var sql = @"
                    UPDATE dgmesnie.AccionSeguimiento SET
                        Estatus = @Estatus,
                        Comentarios = ISNULL(@Comentarios, Comentarios),
                        FechaCierre = @FechaCierre,
                        SemaforoId = ISNULL(@SemaforoId, SemaforoId),
                        ActualizadoEn = GETDATE(),
                        ActualizadoPor = @Usuario
                    WHERE AccionId = @AccionId";

                var rows = await db.ExecuteAsync(sql, new {
                    AccionId = accionId,
                    Estatus = estatus,
                    Comentarios = comentarios,
                    FechaCierre = string.Equals(estatus, "Cerrada", StringComparison.OrdinalIgnoreCase) ? (object)DateTime.Now : DBNull.Value,
                    SemaforoId = semId,
                    Usuario = usuario
                });

                return rows > 0;
            }
        }

        public async Task<int> CrearAccionAsync(int proyectoId, string titulo, string descripcion, string responsableNombre, string responsableId, DateTime? fechaCompromiso, string usuario)
        {
            using (var db = Connection)
            {
                // Resolve semaphore based on dates
                int semId = 3; // Verde (SemaforoId = 3)
                if (fechaCompromiso.HasValue)
                {
                    var days = (fechaCompromiso.Value.Date - DateTime.Today.Date).TotalDays;
                    if (days < 0) semId = 1; // Rojo (SemaforoId = 1)
                    else if (days <= 7) semId = 2; // Amarillo (SemaforoId = 2)
                }

                var sql = @"
                    INSERT INTO dgmesnie.AccionSeguimiento (
                        ProyectoId, Titulo, Descripcion, ResponsableUsuarioId, ResponsableNombre,
                        FechaCompromiso, Estatus, SemaforoId, CreadoEn, CreadoPor
                    ) VALUES (
                        @ProyectoId, @Titulo, @Descripcion, @RespId, @RespNombre,
                        @FechaCompromiso, 'Pendiente', @SemaforoId, GETDATE(), @Usuario
                    );
                    SELECT SCOPE_IDENTITY();";

                return await db.ExecuteScalarAsync<int>(sql, new {
                    ProyectoId = proyectoId, Titulo = titulo, Descripcion = descripcion,
                    RespId = responsableId, RespNombre = responsableNombre, FechaCompromiso = fechaCompromiso,
                    SemaforoId = semId, Usuario = usuario
                });
            }
        }

        // ── Catalogs ─────────────────────────────────────────────────────────
        public async Task<List<CatalogoItem>> ObtenerCatClasificacionAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT ClasificacionId AS Id, Nombre FROM dgmesnie.CatClasificacion WHERE Activo = 1 ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatPrioridadAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT PrioridadId AS Id, Nombre FROM dgmesnie.CatPrioridad WHERE Activo = 1 ORDER BY Orden";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatSemaforoAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT SemaforoId AS Id, Nombre FROM dgmesnie.CatSemaforo WHERE Activo = 1 ORDER BY SemaforoId";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatTecnologiaAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT TecnologiaId AS Id, Nombre FROM dgmesnie.CatTecnologia WHERE Activo = 1 ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatValoracionMinutaAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT ValoracionMinutaId AS Id, Nombre FROM dgmesnie.CatValoracionMinuta ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatEstatusTramiteAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT EstatusTramiteId AS Id, Nombre FROM dgmesnie.CatEstatusTramite WHERE Activo = 1 ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerEmpresasAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT EmpresaId AS Id, Nombre FROM dgmesnie.Empresa WHERE Activo = 1 ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerUsuariosVigentesAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT IdUsuario AS Id, Nombre FROM dgmesnie.Usuario WHERE Vigente = 1 ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }
    }
}
