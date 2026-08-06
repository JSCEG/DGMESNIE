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
        private sealed class EstadoConvocatoriaRow
        {
            public long Id { get; set; }
            public string Estado { get; set; } = string.Empty;
        }

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
                        p.ProyectoId,
                        p.Status,
                        p.NombreOficial AS Nombre,
                        p.NombreCorto,
                        p.NombreNormalizado,
                        p.Descripcion,
                        p.TecnologiaId,
                        p.ClasificacionId,
                        p.PrioridadId,
                        p.SemaforoId,
                        p.Renovable,
                        p.CapacidadMW,
                        p.Activo,
                        p.CreadoEn,
                        p.CreadoPor,
                        p.ActualizadoEn,
                        p.ActualizadoPor,
                        (SELECT TOP 1 a.RazonSocial FROM core.ProyectoActor pa JOIN core.Actor a ON a.ActorId = pa.ActorId WHERE pa.ProyectoId = p.ProyectoId AND pa.TipoActorId = 1) AS Promovente,
                        (SELECT TOP 1 g.Nombre FROM core.ProyectoActor pa JOIN core.Actor a ON a.ActorId = pa.ActorId LEFT JOIN core.GrupoInteresEconomico g ON g.GrupoEconomicoId = a.GrupoEconomicoId WHERE pa.ProyectoId = p.ProyectoId AND pa.TipoActorId = 1) AS GrupoEconomico,
                        (SELECT TOP 1 e.Nombre FROM core.CatEntidadFederativa e JOIN core.ProyectoUbicacion u ON u.EntidadFederativaId = e.EntidadFederativaId WHERE u.ProyectoId = p.ProyectoId) AS EntidadFederativa,
                        (SELECT TOP 1 m.Nombre FROM core.CatMunicipio m JOIN core.ProyectoUbicacion u ON u.MunicipioId = m.MunicipioId WHERE u.ProyectoId = p.ProyectoId) AS Municipio,
                        ct.Nombre AS Tipo,
                        cc.Nombre AS ClasificacionNombre,
                        cp.Nombre AS PrioridadNombre,
                        cs.Nombre AS SemaforoNombre,
                        cs.ColorHex AS SemaforoColorHex
                    FROM core.Proyecto p
                    LEFT JOIN core.CatTecnologia ct ON ct.TecnologiaId = p.TecnologiaId
                    LEFT JOIN core.CatClasificacion cc ON cc.ClasificacionId = p.ClasificacionId
                    LEFT JOIN core.CatPrioridad cp ON cp.PrioridadId = p.PrioridadId
                    LEFT JOIN core.CatSemaforo cs ON cs.SemaforoId = p.SemaforoId
                    WHERE p.Activo = 1";

                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    sql += " AND (p.NombreOficial LIKE @Buscar OR p.NombreCorto LIKE @Buscar OR p.NombreNormalizado LIKE @Buscar)";
                    parameters.Add("Buscar", $"%{buscar.Trim()}%");
                }

                if (!string.IsNullOrWhiteSpace(tecnologia))
                {
                    sql += " AND ct.Nombre = @Tecnologia";
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

                sql += " ORDER BY p.NombreOficial";

                return (await db.QueryAsync<Proyecto>(sql, parameters)).ToList();
            }
        }

        public async Task<Proyecto> ObtenerProyectoPorIdAsync(int id)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT 
                        p.ProyectoId,
                        p.Status,
                        p.NombreOficial AS Nombre,
                        p.NombreCorto,
                        p.NombreNormalizado,
                        p.Descripcion,
                        p.TecnologiaId,
                        p.ClasificacionId,
                        p.PrioridadId,
                        p.SemaforoId,
                        p.Renovable,
                        p.CapacidadMW,
                        p.Activo,
                        p.CreadoEn,
                        p.CreadoPor,
                        p.ActualizadoEn,
                        p.ActualizadoPor,
                        (SELECT TOP 1 a.RazonSocial FROM core.ProyectoActor pa JOIN core.Actor a ON a.ActorId = pa.ActorId WHERE pa.ProyectoId = p.ProyectoId AND pa.TipoActorId = 1) AS Promovente,
                        (SELECT TOP 1 g.Nombre FROM core.ProyectoActor pa JOIN core.Actor a ON a.ActorId = pa.ActorId LEFT JOIN core.GrupoInteresEconomico g ON g.GrupoEconomicoId = a.GrupoEconomicoId WHERE pa.ProyectoId = p.ProyectoId AND pa.TipoActorId = 1) AS GrupoEconomico,
                        (SELECT TOP 1 e.Nombre FROM core.CatEntidadFederativa e JOIN core.ProyectoUbicacion u ON u.EntidadFederativaId = e.EntidadFederativaId WHERE u.ProyectoId = p.ProyectoId) AS EntidadFederativa,
                        (SELECT TOP 1 m.Nombre FROM core.CatMunicipio m JOIN core.ProyectoUbicacion u ON u.MunicipioId = m.MunicipioId WHERE u.ProyectoId = p.ProyectoId) AS Municipio,
                        (SELECT TOP 1 a.ActorId FROM core.ProyectoActor pa JOIN core.Actor a ON a.ActorId = pa.ActorId WHERE pa.ProyectoId = p.ProyectoId AND pa.TipoActorId = 1) AS EmpresaId,
                        ct.Nombre AS Tipo,
                        cc.Nombre AS ClasificacionNombre,
                        cp.Nombre AS PrioridadNombre,
                        cs.Nombre AS SemaforoNombre,
                        cs.ColorHex AS SemaforoColorHex
                    FROM core.Proyecto p
                    LEFT JOIN core.CatTecnologia ct ON ct.TecnologiaId = p.TecnologiaId
                    LEFT JOIN core.CatClasificacion cc ON cc.ClasificacionId = p.ClasificacionId
                    LEFT JOIN core.CatPrioridad cp ON cp.PrioridadId = p.PrioridadId
                    LEFT JOIN core.CatSemaforo cs ON cs.SemaforoId = p.SemaforoId
                    WHERE p.ProyectoId = @Id AND p.Activo = 1";

                return await db.QueryFirstOrDefaultAsync<Proyecto>(sql, new { Id = id });
            }
        }

        public async Task<int> CrearProyectoAsync(ProyectoForm form, string usuario)
        {
            using (var db = Connection)
            {
                if (db.State != ConnectionState.Open) db.Open();
                using (var tx = db.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert core.Proyecto
                        var sqlProy = @"
                            INSERT INTO core.Proyecto (
                                Status, NombreOficial, NombreCorto, TecnologiaId, EstatusProyectoId, NivelMadurezId, ClasificacionId, PrioridadId, SemaforoId, Renovable, CapacidadMW, Activo, CreadoEn, CreadoPor
                            ) VALUES (
                                @Status, @Nombre, @NombreCorto, (SELECT TOP 1 TecnologiaId FROM core.CatTecnologia WHERE Nombre = @Tipo), 3, 1, @ClasificacionId, @PrioridadId, @SemaforoId, @Renovable, @CapacidadMW, 1, GETDATE(), @Usuario
                            );
                            SELECT SCOPE_IDENTITY();";

                        var pId = await db.ExecuteScalarAsync<int>(sqlProy, new {
                            form.Status,
                            form.Nombre,
                            NombreCorto = IngestionService.NormalizeProjectName(form.Nombre),
                            form.Tipo,
                            form.ClasificacionId,
                            form.PrioridadId,
                            form.SemaforoId,
                            form.Renovable,
                            form.CapacidadMW,
                            Usuario = usuario
                        }, tx);

                        // 2. Insert core.ProyectoIdentificador (Initial manual ID)
                        await db.ExecuteAsync(@"
                            INSERT INTO core.ProyectoIdentificador (ProyectoId, OrigenDatosId, ClaveExterna, NombreEnOrigen)
                            VALUES (@ProyectoId, 1, @ClaveExterna, @NombreEnOrigen)
                        ", new {
                            ProyectoId = pId,
                            ClaveExterna = form.NumeroPermiso ?? $"MANUAL-{pId}",
                            NombreEnOrigen = form.Nombre
                        }, tx);

                        // 3. Associate with Empresa/Actor if provided
                        if (form.EmpresaId.HasValue)
                        {
                            await db.ExecuteAsync(@"
                                INSERT INTO core.ProyectoActor (ProyectoId, ActorId, TipoActorId)
                                VALUES (@ProyectoId, @ActorId, 1) -- 1 = Promovente
                            ", new { ProyectoId = pId, ActorId = form.EmpresaId.Value }, tx);
                        }

                        // 4. Insert core.ProyectoUbicacion
                        int entId = 99; // No Especificado por defecto
                        int munId = 99001;
                        if (!string.IsNullOrWhiteSpace(form.EntidadFederativa))
                        {
                            var eRes = await db.QueryFirstOrDefaultAsync<int?>(@"
                                SELECT EntidadFederativaId FROM core.CatEntidadFederativa WHERE Nombre LIKE @Nombre
                            ", new { Nombre = $"%{form.EntidadFederativa}%" }, tx);
                            if (eRes.HasValue)
                            {
                                entId = eRes.Value;
                                var mRes = await db.QueryFirstOrDefaultAsync<int?>(@"
                                    SELECT MunicipioId FROM core.CatMunicipio WHERE EntidadFederativaId = @entId
                                ", new { entId }, tx);
                                if (mRes.HasValue) munId = mRes.Value;
                            }
                        }

                        await db.ExecuteAsync(@"
                            INSERT INTO core.ProyectoUbicacion (ProyectoId, EntidadFederativaId, MunicipioId, Localidad, EsPrincipal, RegionTransmision)
                            VALUES (@ProyectoId, @EntidadFederativaId, @MunicipioId, @Localidad, 1, @RegionTransmision)
                        ", new {
                            ProyectoId = pId,
                            EntidadFederativaId = entId,
                            MunicipioId = munId,
                            Localidad = form.Municipio,
                            form.RegionTransmision
                        }, tx);

                        // 5. Insert core.ProyectoDatosTecnicos
                        await db.ExecuteAsync(@"
                            INSERT INTO core.ProyectoDatosTecnicos (ProyectoId, CapacidadInstaladaMW, AlmacenamientoBess, EsHibrido, ComentariosTecnicos)
                            VALUES (@ProyectoId, @Capacidad, @Bess, @Bess, @Comentarios)
                        ", new {
                            ProyectoId = pId,
                            Capacidad = form.CapacidadMW ?? 0.0m,
                            Bess = form.RequiereAlmacenamiento == "SÍ" || form.RequiereAlmacenamiento == "SI" ? 1 : 0,
                            Comentarios = form.EstadoProgramaObras
                        }, tx);

                        // 6. Insert core.ProyectoDatosFinancieros
                        await db.ExecuteAsync(@"
                            INSERT INTO core.ProyectoDatosFinancieros (ProyectoId, MonedaId, FuenteFinanciamiento, NombreEPC)
                            VALUES (@ProyectoId, 1, @Fuente, @EPC)
                        ", new { ProyectoId = pId, Fuente = form.Fuente, EPC = form.Promovente }, tx);

                        // 7. Write initial JSON snapshot to Bitacora
                        var proj = await db.QueryFirstOrDefaultAsync<Proyecto>(
                            "SELECT * FROM core.Proyecto WHERE ProyectoId = @Id", new { Id = pId }, tx);
                        var snapshot = JsonConvert.SerializeObject(proj);

                        await db.ExecuteAsync(@"
                            INSERT INTO core.BitacoraProyecto (ProyectoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, SnapshotProyectoJSON, CreadoPor, CreadoEn)
                            VALUES (@ProyectoId, GETDATE(), 'CREACIÓN', 'Carga inicial del proyecto.', @Snapshot, @Usuario, GETDATE())",
                            new { ProyectoId = pId, Snapshot = snapshot, Usuario = usuario }, tx);

                        tx.Commit();
                        return pId;
                    }
                    catch (Exception)
                    {
                        tx.Rollback();
                        throw;
                    }
                }
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
                        var current = await db.QueryFirstOrDefaultAsync<Proyecto>(@"
                            SELECT 
                                p.*, 
                                ct.Nombre AS Tipo,
                                cc.Nombre AS ClasificacionNombre,
                                cp.Nombre AS PrioridadNombre,
                                cs.Nombre AS SemaforoNombre,
                                (SELECT TOP 1 a.RazonSocial FROM core.ProyectoActor pa JOIN core.Actor a ON a.ActorId = pa.ActorId WHERE pa.ProyectoId = p.ProyectoId AND pa.TipoActorId = 1) AS Promovente
                            FROM core.Proyecto p
                            LEFT JOIN core.CatTecnologia ct ON ct.TecnologiaId = p.TecnologiaId
                            LEFT JOIN core.CatClasificacion cc ON cc.ClasificacionId = p.ClasificacionId
                            LEFT JOIN core.CatPrioridad cp ON cp.PrioridadId = p.PrioridadId
                            LEFT JOIN core.CatSemaforo cs ON cs.SemaforoId = p.SemaforoId
                            WHERE p.ProyectoId = @Id AND p.Activo = 1",
                            new { Id = id }, tx);

                        if (current == null) return false;

                        // 2. Audit check
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
                        CheckChange("Nombre oficial", current.Nombre, form.Nombre);
                        CheckChange("ClasificacionId", current.ClasificacionId, form.ClasificacionId);
                        CheckChange("PrioridadId", current.PrioridadId, form.PrioridadId);
                        CheckChange("SemaforoId", current.SemaforoId, form.SemaforoId);
                        CheckChange("CapacidadMW", current.CapacidadMW, form.CapacidadMW);

                        foreach (var log in auditLogs)
                        {
                            await db.ExecuteAsync(@"
                                INSERT INTO core.HistorialProyecto (ProyectoId, Campo, ValorAnterior, ValorNuevo, CambiadoEn, CambiadoPor)
                                VALUES (@ProyectoId, @Campo, @Anterior, @Nuevo, GETDATE(), @Usuario)",
                                new { ProyectoId = id, Campo = log.Campo, Anterior = log.Anterior, Nuevo = log.Nuevo, Usuario = usuario }, tx);
                        }

                        // 3. Update core.Proyecto
                        await db.ExecuteAsync(@"
                            UPDATE core.Proyecto SET
                                Status = @Status,
                                NombreOficial = @Nombre,
                                TecnologiaId = (SELECT TOP 1 TecnologiaId FROM core.CatTecnologia WHERE Nombre = @Tipo),
                                ClasificacionId = @ClasificacionId,
                                PrioridadId = @PrioridadId,
                                SemaforoId = @SemaforoId,
                                Renovable = @Renovable,
                                CapacidadMW = @CapacidadMW,
                                ActualizadoEn = GETDATE(),
                                ActualizadoPor = @Usuario
                            WHERE ProyectoId = @ProyectoId",
                            new {
                                form.Status, form.Nombre, form.Tipo, form.ClasificacionId, form.PrioridadId, form.SemaforoId, form.Renovable, form.CapacidadMW, Usuario = usuario, ProyectoId = id
                            }, tx);

                        // 4. Update core.ProyectoDatosTecnicos
                        await db.ExecuteAsync(@"
                            UPDATE core.ProyectoDatosTecnicos SET
                                CapacidadInstaladaMW = ISNULL(@Capacidad, CapacidadInstaladaMW),
                                AlmacenamientoBess = @Bess,
                                EsHibrido = @Bess
                            WHERE ProyectoId = @ProyectoId",
                            new { ProyectoId = id, Capacidad = form.CapacidadMW, Bess = form.RequiereAlmacenamiento == "SÍ" || form.RequiereAlmacenamiento == "SI" ? 1 : 0 }, tx);

                        // 5. Update Actor connection
                        if (form.EmpresaId.HasValue)
                        {
                            await db.ExecuteAsync("DELETE FROM core.ProyectoActor WHERE ProyectoId = @ProyectoId AND TipoActorId = 1", new { ProyectoId = id }, tx);
                            await db.ExecuteAsync(@"
                                INSERT INTO core.ProyectoActor (ProyectoId, ActorId, TipoActorId)
                                VALUES (@ProyectoId, @ActorId, 1)
                            ", new { ProyectoId = id, ActorId = form.EmpresaId.Value }, tx);
                        }

                        // 6. Snapshot Bitacora
                        var updatedProj = await db.QueryFirstOrDefaultAsync<Proyecto>("SELECT * FROM core.Proyecto WHERE ProyectoId = @Id", new { Id = id }, tx);
                        var snapshot = JsonConvert.SerializeObject(updatedProj);

                        await db.ExecuteAsync(@"
                            INSERT INTO core.BitacoraProyecto (ProyectoId, FechaEvento, ValoracionTexto, ResumenAcuerdos, SnapshotProyectoJSON, CreadoPor, CreadoEn)
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
                var sql = "UPDATE core.Proyecto SET Activo = 0 WHERE ProyectoId = @Id";
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
                    SELECT 
                        t.TramiteId AS TramiteProyectoId,
                        t.ProyectoId,
                        ct.Nombre AS TipoTramite,
                        t.Folio,
                        t.EstatusTramiteId,
                        ce.Nombre AS EstatusTramiteNombre,
                        t.FechaIngreso,
                        t.FechaResolucion,
                        t.FechaVencimiento,
                        t.Observaciones,
                        ca.Acronimo AS Autoridad,
                        t.DocumentoUrl AS Fuente,
                        t.Activo
                    FROM core.ProyectoTramite t
                    LEFT JOIN core.CatTipoTramite ct ON ct.TipoTramiteId = t.TipoTramiteId
                    LEFT JOIN core.CatEstatusTramite ce ON ce.EstatusTramiteId = t.EstatusTramiteId
                    LEFT JOIN core.CatAutoridad ca ON ca.AutoridadId = t.AutoridadId
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
                    FROM core.BitacoraProyecto b
                    LEFT JOIN core.Reunion r ON r.ReunionId = b.ReunionId
                    LEFT JOIN core.CatValoracionMinuta cv ON cv.ValoracionMinutaId = b.ValoracionMinutaId
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
                    FROM core.AccionSeguimiento a
                    LEFT JOIN core.CatSemaforo cs ON cs.SemaforoId = a.SemaforoId
                    WHERE a.ProyectoId = @ProyectoId
                    ORDER BY a.FechaCompromiso DESC";
                return (await db.QueryAsync<AccionSeguimiento>(sql, new { ProyectoId = proyectoId })).ToList();
            }
        }

        public async Task<List<Documento>> ObtenerDocumentosPorProyectoAsync(int proyectoId)
        {
            using (var db = Connection)
            {
                var sql = @"
                    SELECT DISTINCT d.*, ct.Nombre AS TipoDocumentoNombre
                    FROM core.Documento d
                    JOIN core.CatTipoDocumento ct ON ct.TipoDocumentoId = d.TipoDocumentoId
                    LEFT JOIN core.BitacoraProyecto b ON b.DocumentoId = d.DocumentoId
                    LEFT JOIN core.Reunion r ON r.DocumentoId = d.DocumentoId
                    WHERE (b.ProyectoId = @ProyectoId OR r.ReunionId IN (SELECT ReunionId FROM core.BitacoraProyecto WHERE ProyectoId = @ProyectoId))
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
                    SELECT h.*, p.NombreOficial AS ProyectoNombre
                    FROM core.HistorialProyecto h
                    JOIN core.Proyecto p ON p.ProyectoId = h.ProyectoId
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
                    FROM core.Reunion r
                    LEFT JOIN core.Documento d ON d.DocumentoId = r.DocumentoId
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
                    FROM core.Reunion r
                    LEFT JOIN core.Documento d ON d.DocumentoId = r.DocumentoId
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
                        int? docId = null;
                        if (!string.IsNullOrWhiteSpace(form.SharePointUrl))
                        {
                            var filename = string.IsNullOrWhiteSpace(form.Titulo) ? "Minuta.pdf" : $"{form.Titulo}.pdf";
                            docId = await db.ExecuteScalarAsync<int>(@"
                                INSERT INTO core.Documento (TipoDocumentoId, Titulo, SharePointUrl, NombreArchivo, FechaDocumento, SubidoPor, SubidoEn)
                                VALUES (1, @Titulo, @Url, @Filename, @Fecha, @Usuario, GETDATE());
                                SELECT SCOPE_IDENTITY();",
                                new { Titulo = form.Titulo, Url = form.SharePointUrl, Filename = filename, Fecha = form.FechaReunion, Usuario = usuario }, tx);
                        }

                        var reunionId = await db.ExecuteScalarAsync<int>(@"
                            INSERT INTO core.Reunion (Titulo, FechaReunion, Modalidad, Lugar, Objetivo, Asistentes, DocumentoId, CreadoEn, CreadoPor)
                            VALUES (@Titulo, @Fecha, @Modalidad, @Lugar, @Objetivo, @Asistentes, @DocId, GETDATE(), @Usuario);
                            SELECT SCOPE_IDENTITY();",
                            new { form.Titulo, Fecha = form.FechaReunion, form.Modalidad, form.Lugar, form.Objetivo, form.Asistentes, DocId = docId, Usuario = usuario }, tx);

                        foreach (var ent in form.ProyectoEntradas)
                        {
                            var current = await db.QueryFirstOrDefaultAsync<Proyecto>(
                                "SELECT * FROM core.Proyecto WHERE ProyectoId = @Id", new { Id = ent.ProyectoId }, tx);

                            if (current == null) continue;

                            var snapshot = JsonConvert.SerializeObject(current);

                            var bitacoraId = await db.ExecuteScalarAsync<int>(@"
                                INSERT INTO core.BitacoraProyecto (
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

                            // Apply direct project update
                            await db.ExecuteAsync(@"
                                UPDATE core.Proyecto SET
                                    Status = ISNULL(@Status, Status),
                                    CapacidadMW = ISNULL(@CapacidadMW, CapacidadMW),
                                    ClasificacionId = ISNULL(@ClasificacionId, ClasificacionId),
                                    PrioridadId = ISNULL(@PrioridadId, PrioridadId),
                                    SemaforoId = ISNULL(@SemaforoId, SemaforoId),
                                    ActualizadoEn = GETDATE(),
                                    ActualizadoPor = @Usuario
                                WHERE ProyectoId = @ProyectoId",
                                new {
                                    ProyectoId = ent.ProyectoId, Status = ent.Status, CapacidadMW = ent.CapacidadMW,
                                    ClasificacionId = ent.ClasificacionId, PrioridadId = ent.PrioridadId,
                                    SemaforoId = ent.SemaforoId, Usuario = usuario
                                }, tx);

                            foreach (var acc in ent.AccionesAInsertar)
                            {
                                if (string.IsNullOrWhiteSpace(acc.Titulo)) continue;

                                int semId = 3; 
                                if (acc.FechaCompromiso.HasValue)
                                {
                                    var days = (acc.FechaCompromiso.Value.Date - DateTime.Today.Date).TotalDays;
                                    if (days < 0) semId = 1; 
                                    else if (days <= 7) semId = 2; 
                                }

                                await db.ExecuteAsync(@"
                                    INSERT INTO core.AccionSeguimiento (
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
                    SELECT a.*, p.NombreOficial AS ProyectoNombre, cs.Nombre AS SemaforoNombre, cs.ColorHex AS SemaforoColorHex
                    FROM core.AccionSeguimiento a
                    JOIN core.Proyecto p ON p.ProyectoId = a.ProyectoId
                    LEFT JOIN core.CatSemaforo cs ON cs.SemaforoId = a.SemaforoId
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
                var today = DateTime.Today;
                foreach (var item in list)
                {
                    if (string.Equals(item.Estatus, "Cerrada", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(item.Estatus, "Cancelada", StringComparison.OrdinalIgnoreCase))
                    {
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
                            item.SemaforoColorHex = "#C00000"; 
                            item.SemaforoNombre = "Rojo";
                        }
                        else if ((limit - today).TotalDays <= 7)
                        {
                            item.SemaforoColorHex = "#FFC000"; 
                            item.SemaforoNombre = "Amarillo";
                        }
                        else
                        {
                            item.SemaforoColorHex = "#00B050"; 
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
                int? semId = null;
                if (string.Equals(estatus, "Cerrada", StringComparison.OrdinalIgnoreCase))
                {
                    semId = 3; 
                }

                var sql = @"
                    UPDATE core.AccionSeguimiento SET
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
                int semId = 3; 
                if (fechaCompromiso.HasValue)
                {
                    var days = (fechaCompromiso.Value.Date - DateTime.Today.Date).TotalDays;
                    if (days < 0) semId = 1; 
                    else if (days <= 7) semId = 2; 
                }

                var sql = @"
                    INSERT INTO core.AccionSeguimiento (
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

        // ── Cartera estratégica y segunda convocatoria ─────────────────────
        public async Task SincronizarCarteraConvocatoriaAsync(CarteraConvocatoriaSeed seed, string usuario)
        {
            if (seed?.Projects == null || seed.Projects.Count == 0) return;

            using (var db = Connection)
            {
                db.Open();
                using (var tx = db.BeginTransaction())
                {
                    try
                    {
                        const string upsert = @"
MERGE dgmesnie.CarteraConvocatoriaProyecto WITH (HOLDLOCK) AS target
USING (SELECT @Folio AS Folio) AS source
ON target.Folio = source.Folio
WHEN MATCHED THEN UPDATE SET
    Nombre = @Name,
    Tipo = @Type,
    GerenciaControl = @Region,
    EntidadFederativa = @State,
    Tecnologia = @Technology,
    RazonSocial = @Company,
    GrupoInteres = @InterestGroup,
    Subestacion = @Substation,
    PuntoInterconexion = @InterconnectionPoint,
    CapacidadMw = @Mw,
    OrdenPrelacion = @Rank,
    FuentePrioridad = @PrioritySource,
    ClasificacionAnalisis = @AnalysisClassification,
    ViabilidadTecnica = @TechnicalViability,
    AnalisisTecnico = @TechnicalAnalysis,
    FechaFirmaProyecto = @ProjectSignedAt,
    GrupoDuplicado = @DuplicateGroup,
    Fuente = @Source,
    FilaFuente = @SourceRow,
    VersionFuente = @SourceVersion,
    Activo = 1,
    ActualizadoUtc = SYSUTCDATETIME(),
    ActualizadoPor = @Usuario
WHEN NOT MATCHED THEN INSERT
(
    Folio, Nombre, Tipo, GerenciaControl, EntidadFederativa,
    Tecnologia, RazonSocial, GrupoInteres, Subestacion,
    PuntoInterconexion, CapacidadMw, OrdenPrelacion, Prioridad,
    FuentePrioridad, EstadoSeguimiento, ClasificacionAnalisis,
    ViabilidadTecnica, AnalisisTecnico, FechaFirmaProyecto,
    GrupoDuplicado, Fuente, FilaFuente,
    VersionFuente, CreadoPor
)
VALUES
(
    @Folio, @Name, @Type, @Region, @State,
    @Technology, @Company, @InterestGroup, @Substation,
    @InterconnectionPoint, @Mw, @Rank, @Priority,
    @PrioritySource, @Decision, @AnalysisClassification,
    @TechnicalViability, @TechnicalAnalysis, @ProjectSignedAt,
    @DuplicateGroup, @Source, @SourceRow,
    @SourceVersion, @Usuario
);";

                        foreach (var project in seed.Projects)
                        {
                            var decision = NormalizarEstadoConvocatoria(project.Decision);
                            await db.ExecuteAsync(upsert, new
                            {
                                project.Folio,
                                project.Name,
                                project.Type,
                                project.Region,
                                project.State,
                                project.Technology,
                                project.Company,
                                project.InterestGroup,
                                project.Substation,
                                project.InterconnectionPoint,
                                project.Mw,
                                project.Rank,
                                Priority = Math.Clamp(project.Priority, 1, 4),
                                project.PrioritySource,
                                Decision = decision,
                                project.AnalysisClassification,
                                project.TechnicalViability,
                                project.TechnicalAnalysis,
                                project.ProjectSignedAt,
                                project.DuplicateGroup,
                                project.Source,
                                project.SourceRow,
                                seed.SourceVersion,
                                Usuario = usuario
                            }, tx);
                        }

                        await db.ExecuteAsync(@"
UPDATE dgmesnie.CarteraConvocatoriaProyecto
SET Activo = 0,
    ActualizadoUtc = SYSUTCDATETIME(),
    ActualizadoPor = @Usuario
WHERE Activo = 1
  AND Fuente IN (N'Estratégicos', N'Particulares 2')
  AND ISNULL(VersionFuente, N'') <> @SourceVersion;", new
                        {
                            seed.SourceVersion,
                            Usuario = usuario
                        }, tx);

                        const string insertSeedNote = @"
DECLARE @ProyectoCarteraId BIGINT =
(
    SELECT ProyectoCarteraId
    FROM dgmesnie.CarteraConvocatoriaProyecto
    WHERE Folio = @Folio
);
IF @ProyectoCarteraId IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1
       FROM dgmesnie.CarteraConvocatoriaComentario
       WHERE OrigenClave = @OrigenClave
          OR
          (
              ProyectoCarteraId = @ProyectoCarteraId
              AND Sesion = @Session
              AND FechaSesion = @Date
              AND Comentario = @Text
          )
   )
BEGIN
    INSERT dgmesnie.CarteraConvocatoriaComentario
    (
        ProyectoCarteraId, Sesion, FechaSesion, Comentario,
        UsuarioRegistro, OrigenClave
    )
    VALUES
    (
        @ProyectoCarteraId, @Session, @Date, @Text,
        @Usuario, @OrigenClave
    );
END;";

                        for (var index = 0; index < seed.Notes.Count; index++)
                        {
                            var note = seed.Notes[index];
                            if (string.IsNullOrWhiteSpace(note.Folio) ||
                                string.IsNullOrWhiteSpace(note.Text)) continue;

                            await db.ExecuteAsync(insertSeedNote, new
                            {
                                note.Folio,
                                Session = string.IsNullOrWhiteSpace(note.Session)
                                    ? "Sesión de trabajo"
                                    : note.Session.Trim(),
                                Date = note.Date == default ? DateTime.Today : note.Date.Date,
                                Text = note.Text.Trim(),
                                Usuario = "Importación inicial",
                                OrigenClave = $"seed:cartera:{note.Folio}:{note.Date:yyyyMMdd}:{index + 1}"
                            }, tx);
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<CarteraConvocatoriaDatos> ObtenerCarteraConvocatoriaAsync()
        {
            using (var db = Connection)
            {
                const string projectsSql = @"
SELECT
    ProyectoCarteraId AS ProjectId,
    Folio,
    Nombre AS Name,
    Tipo AS Type,
    GerenciaControl AS Region,
    EntidadFederativa AS State,
    Tecnologia AS Technology,
    RazonSocial AS Company,
    GrupoInteres AS InterestGroup,
    Subestacion,
    PuntoInterconexion AS InterconnectionPoint,
    CapacidadMw AS Mw,
    OrdenPrelacion AS Rank,
    Prioridad AS Priority,
    FuentePrioridad AS PrioritySource,
    EstadoSeguimiento AS Decision,
    ClasificacionAnalisis AS AnalysisClassification,
    ViabilidadTecnica AS TechnicalViability,
    AnalisisTecnico AS TechnicalAnalysis,
    FechaFirmaProyecto AS ProjectSignedAt,
    GrupoDuplicado AS DuplicateGroup,
    Fuente AS Source,
    FilaFuente AS SourceRow,
    ActualizadoUtc AS UpdatedAt,
    ActualizadoPor AS UpdatedBy
FROM dgmesnie.CarteraConvocatoriaProyecto
WHERE Activo = 1
ORDER BY OrdenPrelacion, Folio;";

                const string notesSql = @"
SELECT
    c.ComentarioId AS CommentId,
    p.Folio,
    c.Sesion AS Session,
    c.FechaSesion AS Date,
    c.Comentario AS Text,
    c.UsuarioRegistro AS [User],
    c.FechaRegistroUtc AS CreatedAt
FROM dgmesnie.CarteraConvocatoriaComentario c
INNER JOIN dgmesnie.CarteraConvocatoriaProyecto p
    ON p.ProyectoCarteraId = c.ProyectoCarteraId
WHERE p.Activo = 1
ORDER BY c.FechaSesion DESC, c.ComentarioId DESC;";

                var projects = (await db.QueryAsync<CarteraConvocatoriaProyecto>(projectsSql)).ToList();
                var notes = (await db.QueryAsync<CarteraConvocatoriaComentario>(notesSql)).ToList();
                var sourceVersion = await db.QueryFirstOrDefaultAsync<string>(@"
SELECT TOP (1) VersionFuente
FROM dgmesnie.CarteraConvocatoriaProyecto
WHERE Activo = 1 AND VersionFuente IS NOT NULL
ORDER BY ActualizadoUtc DESC, ProyectoCarteraId DESC;");

                var latest = notes.FirstOrDefault();
                return new CarteraConvocatoriaDatos
                {
                    SourceVersion = sourceVersion ?? "bd-cartera-convocatoria-v1",
                    Source = new
                    {
                        database = "dgmesnie.CarteraConvocatoriaProyecto",
                        rows = projects.Count
                    },
                    Projects = projects,
                    Notes = notes,
                    Session = latest == null
                        ? new CarteraConvocatoriaSesion()
                        : new CarteraConvocatoriaSesion
                        {
                            Name = latest.Session,
                            Date = latest.Date.ToString("yyyy-MM-dd")
                        }
                };
            }
        }

        public async Task<bool> ActualizarEstadoConvocatoriaAsync(string folio, string estado, string usuario)
        {
            var normalized = NormalizarEstadoConvocatoria(estado);
            using (var db = Connection)
            {
                db.Open();
                using (var tx = db.BeginTransaction())
                {
                    try
                    {
                        var current = await db.QueryFirstOrDefaultAsync<EstadoConvocatoriaRow>(@"
SELECT ProyectoCarteraId AS Id, EstadoSeguimiento AS Estado
FROM dgmesnie.CarteraConvocatoriaProyecto WITH (UPDLOCK, HOLDLOCK)
WHERE Folio = @Folio AND Activo = 1;", new { Folio = folio.Trim() }, tx);

                        if (current == null || current.Id <= 0)
                        {
                            tx.Rollback();
                            return false;
                        }

                        if (!string.Equals(current.Estado, normalized, StringComparison.OrdinalIgnoreCase))
                        {
                            await db.ExecuteAsync(@"
UPDATE dgmesnie.CarteraConvocatoriaProyecto
SET EstadoSeguimiento = @Estado,
    ActualizadoUtc = SYSUTCDATETIME(),
    ActualizadoPor = @Usuario
WHERE ProyectoCarteraId = @Id;

INSERT dgmesnie.CarteraConvocatoriaEstadoHistorial
(
    ProyectoCarteraId, EstadoAnterior, EstadoNuevo, UsuarioRegistro
)
VALUES
(
    @Id, @Anterior, @Estado, @Usuario
);", new
                            {
                                current.Id,
                                Anterior = current.Estado,
                                Estado = normalized,
                                Usuario = usuario
                            }, tx);
                        }

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<CarteraConvocatoriaComentario?> AgregarComentarioConvocatoriaAsync(
            AgregarComentarioConvocatoriaRequest request,
            string usuario)
        {
            using (var db = Connection)
            {
                const string sql = @"
DECLARE @ProyectoCarteraId BIGINT =
(
    SELECT ProyectoCarteraId
    FROM dgmesnie.CarteraConvocatoriaProyecto
    WHERE Folio = @Folio AND Activo = 1
);

IF @ProyectoCarteraId IS NOT NULL
BEGIN
    INSERT dgmesnie.CarteraConvocatoriaComentario
    (
        ProyectoCarteraId, Sesion, FechaSesion, Comentario, UsuarioRegistro
    )
    OUTPUT
        inserted.ComentarioId AS CommentId,
        @Folio AS Folio,
        inserted.Sesion AS Session,
        inserted.FechaSesion AS Date,
        inserted.Comentario AS Text,
        inserted.UsuarioRegistro AS [User],
        inserted.FechaRegistroUtc AS CreatedAt
    VALUES
    (
        @ProyectoCarteraId, @Sesion, @Fecha, @Comentario, @Usuario
    );
END;";

                return await db.QueryFirstOrDefaultAsync<CarteraConvocatoriaComentario>(sql, new
                {
                    Folio = request.Folio.Trim(),
                    Sesion = request.Sesion.Trim(),
                    Fecha = request.Fecha.Date,
                    Comentario = request.Comentario.Trim(),
                    Usuario = usuario
                });
            }
        }

        public async Task<bool> ActualizarPrioridadConvocatoriaAsync(string folio, int prioridad, string usuario)
        {
            using (var db = Connection)
            {
                var rows = await db.ExecuteAsync(@"
UPDATE dgmesnie.CarteraConvocatoriaProyecto
SET Prioridad = @Prioridad,
    FuentePrioridad = N'Asignación registrada en plataforma',
    ActualizadoUtc = SYSUTCDATETIME(),
    ActualizadoPor = @Usuario
WHERE Folio = @Folio AND Activo = 1;", new
                {
                    Folio = folio.Trim(),
                    Prioridad = prioridad,
                    Usuario = usuario
                });
                return rows > 0;
            }
        }

        public async Task<long> CrearProyectoConvocatoriaAsync(CrearProyectoConvocatoriaRequest request, string usuario)
        {
            using (var db = Connection)
            {
                db.Open();
                using (var tx = db.BeginTransaction())
                {
                    try
                    {
                        var nextRank = await db.ExecuteScalarAsync<int>(@"
SELECT ISNULL(MAX(OrdenPrelacion), 0) + 1
FROM dgmesnie.CarteraConvocatoriaProyecto WITH (UPDLOCK, HOLDLOCK);", transaction: tx);

                        var id = await db.ExecuteScalarAsync<long>(@"
INSERT dgmesnie.CarteraConvocatoriaProyecto
(
    Folio, Nombre, Tipo, GerenciaControl, EntidadFederativa,
    Tecnologia, RazonSocial, GrupoInteres, PuntoInterconexion,
    CapacidadMw, OrdenPrelacion, Prioridad, FuentePrioridad,
    EstadoSeguimiento, Fuente, VersionFuente, CreadoPor
)
VALUES
(
    @Folio, @Nombre, @Tipo, @Gerencia, @Entidad,
    @Tecnologia, @RazonSocial, @GrupoInteres, @PuntoInterconexion,
    @CapacidadMw, @Orden, @Prioridad, N'Alta manual en plataforma',
    N'revision', N'Captura manual', N'bd-manual-v1', @Usuario
);
SELECT CONVERT(BIGINT, SCOPE_IDENTITY());", new
                        {
                            Folio = request.Folio.Trim(),
                            Nombre = request.Nombre.Trim(),
                            Tipo = request.Tipo.Trim(),
                            Gerencia = request.Gerencia.Trim(),
                            Entidad = request.Entidad.Trim(),
                            Tecnologia = request.Tecnologia?.Trim(),
                            RazonSocial = request.RazonSocial?.Trim(),
                            GrupoInteres = request.GrupoInteres?.Trim(),
                            PuntoInterconexion = request.PuntoInterconexion?.Trim(),
                            request.CapacidadMw,
                            Orden = nextRank,
                            request.Prioridad,
                            Usuario = usuario
                        }, tx);

                        tx.Commit();
                        return id;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private static string NormalizarEstadoConvocatoria(string? estado)
        {
            return (estado ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "va" => "continua",
                "continua" => "continua",
                "continúa" => "continua",
                "no-va" => "no-continua",
                "no continua" => "no-continua",
                "no continúa" => "no-continua",
                "no-continua" => "no-continua",
                "suspendido" => "no-continua",
                "suspendida" => "no-continua",
                _ => "revision"
            };
        }

        // ── Catalogs ─────────────────────────────────────────────────────────
        public async Task<List<CatalogoItem>> ObtenerCatClasificacionAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT ClasificacionId AS Id, Nombre FROM core.CatClasificacion WHERE Activo = 1 ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatPrioridadAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT PrioridadId AS Id, Nombre FROM core.CatPrioridad WHERE Activo = 1 ORDER BY Orden";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatSemaforoAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT SemaforoId AS Id, Nombre FROM core.CatSemaforo WHERE Activo = 1 ORDER BY SemaforoId";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatTecnologiaAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT TecnologiaId AS Id, Nombre FROM core.CatTecnologia WHERE Activo = 1 ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatValoracionMinutaAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT ValoracionMinutaId AS Id, Nombre FROM core.CatValoracionMinuta ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerCatEstatusTramiteAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT EstatusTramiteId AS Id, Nombre FROM core.CatEstatusTramite WHERE Activo = 1 ORDER BY Nombre";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerEmpresasAsync()
        {
            using (var db = Connection)
            {
                var sql = "SELECT ActorId AS Id, RazonSocial AS Nombre FROM core.Actor WHERE Activo = 1 ORDER BY RazonSocial";
                return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
            }
        }

        public async Task<List<CatalogoItem>> ObtenerUsuariosVigentesAsync()
        {
            using (var db = Connection)
            {
                // Mantiene compatibilidad con la consulta de usuarios del sistema
                var sql = "SELECT UsuarioId AS Id, Nombre FROM dgmesnie.Usuario WHERE Activo = 1 ORDER BY Nombre";
                try {
                    return (await db.QueryAsync<CatalogoItem>(sql)).ToList();
                } catch {
                    // Fallback si no existía la tabla de usuarios
                    return new List<CatalogoItem> { new CatalogoItem { Id = 1, Nombre = "Administrador" } };
                }
            }
        }
    }
}
