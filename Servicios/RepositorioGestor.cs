using Microsoft.Data.SqlClient;
using NSIE.Models.Gestor;
using NSIE.Servicios.Interfaces;
using System.Data;

namespace NSIE.Servicios
{
    public class RepositorioGestor : IRepositorioGestor
    {
        private readonly string _conn;
        private readonly ILogger<RepositorioGestor> _logger;

        public RepositorioGestor(IConfiguration config, ILogger<RepositorioGestor> logger)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        // ── helpers ──────────────────────────────────────────────────────────
        private static string CalcularSemaforo(string estatus, DateTime? fechaCompromiso, bool bloqueada, DateTime? fechaUltimaAct)
        {
            if (bloqueada) return "gris";
            if (string.Equals(estatus, "Concluida", StringComparison.OrdinalIgnoreCase)) return "verde";
            if (string.Equals(estatus, "Concluido", StringComparison.OrdinalIgnoreCase)) return "verde";
            var hoy = DateTime.Today;
            if (fechaCompromiso.HasValue && fechaCompromiso.Value.Date < hoy) return "rojo";
            if (fechaCompromiso.HasValue && (fechaCompromiso.Value.Date - hoy).TotalDays <= 7) return "amarillo";
            if (fechaUltimaAct.HasValue && (hoy - fechaUltimaAct.Value.Date).TotalDays > 14) return "gris";
            return "verde";
        }

        // ── Usuarios ─────────────────────────────────────────────────────────
        public async Task<List<GestorUsuarioDto>> ObtenerUsuariosVigentesAsync()
        {
            const string sql = @"
                SELECT IdUsuario, Nombre, Correo, Cargo
                FROM [dgmesnie].[Usuario]
                WHERE Vigente = 1
                ORDER BY Nombre";

            var result = new List<GestorUsuarioDto>();
            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(sql, cn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                result.Add(new GestorUsuarioDto
                {
                    IdUsuario = rd.GetInt32(0),
                    Nombre = rd.GetString(1),
                    Correo = rd.IsDBNull(2) ? null : rd.GetString(2),
                    Cargo = rd.IsDBNull(3) ? null : rd.GetString(3)
                });
            }
            return result;
        }

        public async Task<GestorUsuarioDto?> ObtenerUsuarioVigentePorIdAsync(int idUsuario)
        {
            const string sql = @"
                SELECT IdUsuario, Nombre, Correo, Cargo
                FROM [dgmesnie].[Usuario]
                WHERE IdUsuario = @idUsuario AND Vigente = 1";

            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
            await using var rd = await cmd.ExecuteReaderAsync();

            if (!await rd.ReadAsync())
                return null;

            return new GestorUsuarioDto
            {
                IdUsuario = rd.GetInt32(0),
                Nombre = rd.GetString(1),
                Correo = rd.IsDBNull(2) ? null : rd.GetString(2),
                Cargo = rd.IsDBNull(3) ? null : rd.GetString(3)
            };
        }

        // ── Corresponsables (helpers privados) ───────────────────────────────
        private async Task<List<GestorUsuarioDto>> CargarCorresponsablesAsync(SqlConnection cn, int? temaId, int? actividadId)
        {
            const string sql = @"
                SELECT u.IdUsuario, u.Nombre, u.Correo, u.Cargo
                FROM [dgmesnie].[Gestor_Corresponsables] gc
                JOIN [dgmesnie].[Usuario] u ON u.IdUsuario = gc.IdUsuario
                WHERE (@temaId IS NULL OR gc.TemaId = @temaId)
                  AND (@actividadId IS NULL OR gc.ActividadId = @actividadId)";

            var result = new List<GestorUsuarioDto>();
            await using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@temaId", (object?)temaId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@actividadId", (object?)actividadId ?? DBNull.Value);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                result.Add(new GestorUsuarioDto
                {
                    IdUsuario = rd.GetInt32(0),
                    Nombre = rd.GetString(1),
                    Correo = rd.IsDBNull(2) ? null : rd.GetString(2),
                    Cargo = rd.IsDBNull(3) ? null : rd.GetString(3)
                });
            }
            return result;
        }

        private async Task SincronizarCorresponsablesAsync(SqlConnection cn, SqlTransaction tx,
            int? temaId, int? actividadId, List<int> nuevosIds)
        {
            // Borrar los existentes para este padre
            const string del = @"
                DELETE FROM [dgmesnie].[Gestor_Corresponsables]
                WHERE (@temaId IS NULL OR TemaId = @temaId)
                  AND (@actividadId IS NULL OR ActividadId = @actividadId)";
            await using var cmdDel = new SqlCommand(del, cn, tx);
            cmdDel.Parameters.AddWithValue("@temaId", (object?)temaId ?? DBNull.Value);
            cmdDel.Parameters.AddWithValue("@actividadId", (object?)actividadId ?? DBNull.Value);
            await cmdDel.ExecuteNonQueryAsync();

            // Insertar los nuevos
            foreach (var uid in nuevosIds.Distinct())
            {
                const string ins = @"
                    INSERT INTO [dgmesnie].[Gestor_Corresponsables] (TemaId, ActividadId, IdUsuario)
                    VALUES (@temaId, @actividadId, @uid)";
                await using var cmdIns = new SqlCommand(ins, cn, tx);
                cmdIns.Parameters.AddWithValue("@temaId", (object?)temaId ?? DBNull.Value);
                cmdIns.Parameters.AddWithValue("@actividadId", (object?)actividadId ?? DBNull.Value);
                cmdIns.Parameters.AddWithValue("@uid", uid);
                await cmdIns.ExecuteNonQueryAsync();
            }
        }

        private async Task<List<GestorEtapa>> CargarEtapasAsync(SqlConnection cn, int temaId)
        {
            const string sql = @"
                SELECT e.EtapaId, e.TemaId, e.Nombre, e.ResponsableId,
                       u.Nombre AS ResponsableNombre,
                       e.FechaInicio, e.FechaCompromiso, e.Avance, e.Estatus, e.Orden
                FROM [dgmesnie].[Gestor_Temas_Etapas] e
                LEFT JOIN [dgmesnie].[Usuario] u ON u.IdUsuario = e.ResponsableId
                WHERE e.TemaId = @temaId AND e.Activo = 1
                ORDER BY e.Orden, e.EtapaId";

            var result = new List<GestorEtapa>();
            await using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@temaId", temaId);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                result.Add(new GestorEtapa
                {
                    EtapaId = rd.GetInt32(0),
                    TemaId = rd.GetInt32(1),
                    Nombre = rd.GetString(2),
                    ResponsableId = rd.GetInt32(3),
                    ResponsableNombre = rd.IsDBNull(4) ? null : rd.GetString(4),
                    FechaInicio = rd.IsDBNull(5) ? null : rd.GetDateTime(5),
                    FechaCompromiso = rd.IsDBNull(6) ? null : rd.GetDateTime(6),
                    Avance = rd.GetInt32(7),
                    Estatus = rd.GetString(8),
                    Orden = rd.GetInt32(9)
                });
            }
            return result;
        }

        private async Task SincronizarEtapasAsync(SqlConnection cn, SqlTransaction tx, int temaId, List<GestorEtapaForm> stages)
        {
            var existingIds = new List<int>();
            if (stages != null)
            {
                existingIds = stages.Where(e => e.EtapaId.HasValue).Select(e => e.EtapaId!.Value).ToList();
            }

            if (existingIds.Any())
            {
                const string del = "UPDATE [dgmesnie].[Gestor_Temas_Etapas] SET Activo = 0 WHERE TemaId = @temaId AND EtapaId NOT IN ({0})";
                var inClause = string.Join(",", existingIds);
                var formattedDel = string.Format(del, inClause);
                await using var cmdDel = new SqlCommand(formattedDel, cn, tx);
                cmdDel.Parameters.AddWithValue("@temaId", temaId);
                await cmdDel.ExecuteNonQueryAsync();
            }
            else
            {
                const string delAll = "UPDATE [dgmesnie].[Gestor_Temas_Etapas] SET Activo = 0 WHERE TemaId = @temaId";
                await using var cmdDel = new SqlCommand(delAll, cn, tx);
                cmdDel.Parameters.AddWithValue("@temaId", temaId);
                await cmdDel.ExecuteNonQueryAsync();
            }

            if (stages == null || !stages.Any())
            {
                return;
            }

            var orderedStages = stages.OrderBy(s => s.Orden).ToList();
            for (int i = 0; i < orderedStages.Count; i++)
            {
                var stg = orderedStages[i];
                if (string.IsNullOrWhiteSpace(stg.Nombre))
                    throw new ArgumentException($"La Etapa {i + 1} debe tener un nombre.");
                if (stg.ResponsableId <= 0)
                    throw new ArgumentException($"La Etapa {i + 1} ({stg.Nombre}) debe tener un responsable asignado.");
                if (!stg.FechaCompromiso.HasValue)
                    throw new ArgumentException($"La Etapa {i + 1} ({stg.Nombre}) debe tener una fecha compromiso.");

                if (stg.FechaInicio.HasValue && stg.FechaCompromiso.HasValue && stg.FechaInicio.Value.Date > stg.FechaCompromiso.Value.Date)
                {
                    throw new ArgumentException($"La fecha de inicio de la Etapa {i + 1} ({stg.Nombre}) no puede ser posterior a su propia fecha compromiso.");
                }

                if (i > 0)
                {
                    var prev = orderedStages[i - 1];
                    if (stg.FechaInicio.HasValue && prev.FechaCompromiso.HasValue && stg.FechaInicio.Value.Date < prev.FechaCompromiso.Value.Date)
                    {
                        throw new InvalidOperationException($"La fecha de inicio de la Etapa {i + 1} ({stg.Nombre}) no puede ser anterior a la fecha compromiso de la Etapa {i} ({prev.Nombre}).");
                    }
                    if (stg.Estatus != "Pendiente" && prev.Estatus != "Concluida")
                    {
                        throw new InvalidOperationException($"La Etapa {i + 1} ({stg.Nombre}) no puede iniciar (en proceso/concluida) si la Etapa {i} ({prev.Nombre}) no está Concluida.");
                    }
                }
            }

            int orden = 1;
            foreach (var stg in stages)
            {
                if (stg.EtapaId.HasValue && stg.EtapaId.Value > 0)
                {
                    const string upd = @"
                        UPDATE [dgmesnie].[Gestor_Temas_Etapas] SET
                            Nombre = @nombre, ResponsableId = @respId,
                            FechaInicio = @inicio, FechaCompromiso = @comp,
                            Avance = @avance, Estatus = @estatus, Orden = @orden, Activo = 1
                        WHERE EtapaId = @etapaId AND TemaId = @temaId";
                    await using var cmdUpd = new SqlCommand(upd, cn, tx);
                    cmdUpd.Parameters.AddWithValue("@nombre", stg.Nombre);
                    cmdUpd.Parameters.AddWithValue("@respId", stg.ResponsableId);
                    cmdUpd.Parameters.AddWithValue("@inicio", (object?)stg.FechaInicio ?? DBNull.Value);
                    cmdUpd.Parameters.AddWithValue("@comp", (object?)stg.FechaCompromiso ?? DBNull.Value);
                    cmdUpd.Parameters.AddWithValue("@avance", stg.Avance);
                    cmdUpd.Parameters.AddWithValue("@estatus", stg.Estatus);
                    cmdUpd.Parameters.AddWithValue("@orden", orden++);
                    cmdUpd.Parameters.AddWithValue("@etapaId", stg.EtapaId.Value);
                    cmdUpd.Parameters.AddWithValue("@temaId", temaId);
                    await cmdUpd.ExecuteNonQueryAsync();
                }
                else
                {
                    const string ins = @"
                        INSERT INTO [dgmesnie].[Gestor_Temas_Etapas]
                            (TemaId, Nombre, ResponsableId, FechaInicio, FechaCompromiso, Avance, Estatus, Orden, Activo)
                        VALUES
                            (@temaId, @nombre, @respId, @inicio, @comp, @avance, @estatus, @orden, 1)";
                    await using var cmdIns = new SqlCommand(ins, cn, tx);
                    cmdIns.Parameters.AddWithValue("@temaId", temaId);
                    cmdIns.Parameters.AddWithValue("@nombre", stg.Nombre);
                    cmdIns.Parameters.AddWithValue("@respId", stg.ResponsableId);
                    cmdIns.Parameters.AddWithValue("@inicio", (object?)stg.FechaInicio ?? DBNull.Value);
                    cmdIns.Parameters.AddWithValue("@comp", (object?)stg.FechaCompromiso ?? DBNull.Value);
                    cmdIns.Parameters.AddWithValue("@avance", stg.Avance);
                    cmdIns.Parameters.AddWithValue("@estatus", stg.Estatus);
                    cmdIns.Parameters.AddWithValue("@orden", orden++);
                    await cmdIns.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task RecalcularTemaAgregadosAsync(SqlConnection cn, SqlTransaction tx, int temaId)
        {
            const string sqlStg = @"
                SELECT ResponsableId, FechaInicio, FechaCompromiso, Avance, Estatus, Orden
                FROM [dgmesnie].[Gestor_Temas_Etapas]
                WHERE TemaId = @temaId AND Activo = 1
                ORDER BY Orden, EtapaId";

            var stages = new List<GestorEtapa>();
            await using (var cmd = new SqlCommand(sqlStg, cn, tx))
            {
                cmd.Parameters.AddWithValue("@temaId", temaId);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    stages.Add(new GestorEtapa
                    {
                        ResponsableId = rd.GetInt32(0),
                        FechaInicio = rd.IsDBNull(1) ? null : rd.GetDateTime(1),
                        FechaCompromiso = rd.IsDBNull(2) ? null : rd.GetDateTime(2),
                        Avance = rd.GetInt32(3),
                        Estatus = rd.GetString(4),
                        Orden = rd.GetInt32(5)
                    });
                }
            }

            if (!stages.Any()) return;

            double sumAvance = stages.Sum(s => s.Avance);
            int overallAvance = (int)Math.Round(sumAvance / stages.Count);

            DateTime? overallInicio = stages.Where(s => s.FechaInicio.HasValue).Min(s => s.FechaInicio);
            DateTime? overallCompromiso = stages.Where(s => s.FechaCompromiso.HasValue).Max(s => s.FechaCompromiso);

            var activeStage = stages.FirstOrDefault(s => s.Avance < 100) ?? stages.Last();
            int overallResponsableId = activeStage.ResponsableId;

            string overallEstatus = "En proceso";
            if (stages.All(s => s.Avance == 100 || s.Estatus == "Concluida"))
            {
                overallEstatus = "Concluida";
            }
            else if (stages.All(s => s.Avance == 0 && s.Estatus == "Pendiente"))
            {
                overallEstatus = "Pendiente";
            }
            else
            {
                if (activeStage.FechaCompromiso.HasValue && activeStage.FechaCompromiso.Value.Date < DateTime.Today)
                {
                    overallEstatus = "Vencida";
                }
            }

            const string updTema = @"
                UPDATE [dgmesnie].[Gestor_Temas] SET
                    ResponsableId = @respId,
                    FechaInicio = @inicio,
                    FechaCompromiso = @comp,
                    Avance = @avance,
                    Estatus = @estatus,
                    FechaUltimaActualizacion = GETDATE()
                WHERE TemaId = @temaId";

            await using var cmdUpdTema = new SqlCommand(updTema, cn, tx);
            cmdUpdTema.Parameters.AddWithValue("@respId", overallResponsableId);
            cmdUpdTema.Parameters.AddWithValue("@inicio", (object?)overallInicio ?? DBNull.Value);
            cmdUpdTema.Parameters.AddWithValue("@comp", (object?)overallCompromiso ?? DBNull.Value);
            cmdUpdTema.Parameters.AddWithValue("@avance", overallAvance);
            cmdUpdTema.Parameters.AddWithValue("@estatus", overallEstatus);
            cmdUpdTema.Parameters.AddWithValue("@temaId", temaId);
            await cmdUpdTema.ExecuteNonQueryAsync();
        }

        // ── Actividades (Padre) ──────────────────────────────────────────────
        public async Task<List<GestorActividad>> ObtenerActividadesAsync()
        {
            const string sql = @"
                SELECT a.ActividadId, a.Clave, a.Actividad, a.Descripcion, a.Categoria,
                       a.Prioridad, a.Estatus, a.ResponsablePrincipalId,
                       u.Nombre AS ResponsableNombre,
                       a.FechaInicio, a.FechaCompromiso, a.AvanceGeneral,
                       a.LigaSharePoint, a.ComentariosEjecutivos,
                       a.FechaUltimaActualizacion, a.FechaCreacion
                FROM [dgmesnie].[Gestor_Actividades] a
                LEFT JOIN [dgmesnie].[Usuario] u ON u.IdUsuario = a.ResponsablePrincipalId
                WHERE a.Activo = 1
                ORDER BY a.ActividadId";

            var acts = new List<GestorActividad>();
            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(sql, cn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var a = MapActividad(rd);
                a.Semaforo = CalcularSemaforo(a.Estatus, a.FechaCompromiso, false, a.FechaUltimaActualizacion);
                acts.Add(a);
            }
            rd.Close();

            // Corresponsables
            foreach (var a in acts)
                a.Corresponsables = await CargarCorresponsablesAsync(cn, null, a.ActividadId);

            return acts;
        }

        public async Task<GestorActividad?> ObtenerActividadPorIdAsync(int actividadId)
        {
            const string sql = @"
                SELECT a.ActividadId, a.Clave, a.Actividad, a.Descripcion, a.Categoria,
                       a.Prioridad, a.Estatus, a.ResponsablePrincipalId,
                       u.Nombre AS ResponsableNombre,
                       a.FechaInicio, a.FechaCompromiso, a.AvanceGeneral,
                       a.LigaSharePoint, a.ComentariosEjecutivos,
                       a.FechaUltimaActualizacion, a.FechaCreacion
                FROM [dgmesnie].[Gestor_Actividades] a
                LEFT JOIN [dgmesnie].[Usuario] u ON u.IdUsuario = a.ResponsablePrincipalId
                WHERE a.ActividadId = @id AND a.Activo = 1";

            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", actividadId);
            await using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;
            var a = MapActividad(rd);
            rd.Close();
            a.Corresponsables = await CargarCorresponsablesAsync(cn, null, actividadId);
            a.Semaforo = CalcularSemaforo(a.Estatus, a.FechaCompromiso, false, a.FechaUltimaActualizacion);
            return a;
        }

        public async Task<int> CrearActividadAsync(GestorActividadForm form, int? usuarioId)
        {
            const string sql = @"
                DECLARE @n INT;
                SELECT @n = ISNULL(MAX(ActividadId),0)+1 FROM [dgmesnie].[Gestor_Actividades];
                INSERT INTO [dgmesnie].[Gestor_Actividades]
                    (Clave,Actividad,Descripcion,Categoria,Prioridad,Estatus,
                     ResponsablePrincipalId,FechaInicio,FechaCompromiso,AvanceGeneral,
                     LigaSharePoint,ComentariosEjecutivos,CreadoPor,
                     FechaUltimaActualizacion,FechaCreacion)
                VALUES
                    ('A-'+RIGHT('000'+CAST(@n AS NVARCHAR),3),@act,@desc,@cat,@pri,@est,
                     @resp,@inicio,@comp,0,@liga,@coment,@creador,
                     GETDATE(),GETDATE());
                SELECT SCOPE_IDENTITY();";

            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
            try
            {
                await using var cmd = new SqlCommand(sql, cn, tx);
                AddActividadParams(cmd, form);
                cmd.Parameters.AddWithValue("@creador", (object?)usuarioId ?? DBNull.Value);
                var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                await SincronizarCorresponsablesAsync(cn, tx, null, id, form.CorresponsablesIds);
                await tx.CommitAsync();
                return id;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task ActualizarActividadAsync(GestorActividadForm form, int? usuarioId)
        {
            const string sql = @"
                UPDATE [dgmesnie].[Gestor_Actividades] SET
                    Actividad = @act, Descripcion = @desc, Categoria = @cat,
                    Prioridad = @pri, Estatus = @est,
                    ResponsablePrincipalId = @resp,
                    FechaInicio = @inicio, FechaCompromiso = @comp,
                    LigaSharePoint = @liga, ComentariosEjecutivos = @coment,
                    FechaUltimaActualizacion = GETDATE()
                WHERE ActividadId = @actividadId AND Activo = 1";

            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
            try
            {
                await using var cmd = new SqlCommand(sql, cn, tx);
                AddActividadParams(cmd, form);
                cmd.Parameters.AddWithValue("@actividadId", form.ActividadId!.Value);
                await cmd.ExecuteNonQueryAsync();
                await SincronizarCorresponsablesAsync(cn, tx, null, form.ActividadId.Value, form.CorresponsablesIds);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task EliminarActividadAsync(int actividadId)
        {
            const string sql = @"
                UPDATE [dgmesnie].[Gestor_Actividades] SET Activo = 0 WHERE ActividadId = @id;
                UPDATE [dgmesnie].[Gestor_Temas] SET Activo = 0 WHERE ActividadId = @id;";
            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", actividadId);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── Temas (Hijo) ─────────────────────────────────────────────────────
        public async Task<List<GestorTema>> ObtenerTemasAsync(int? actividadId = null)
        {
            const string sql = @"
                SELECT t.TemaId, t.Clave, t.ActividadId, a.Actividad AS ActividadNombre,
                       t.Tema, t.Descripcion, t.ResponsableId,
                       u.Nombre AS ResponsableNombre,
                       t.FechaInicio, t.FechaCompromiso,
                       t.Estatus, t.Prioridad, t.Avance,
                       t.Bloqueada, t.MotivoBloqueO, t.EvidenciaUrl, t.Comentarios,
                       t.FechaUltimaActualizacion, t.FechaCreacion
                FROM [dgmesnie].[Gestor_Temas] t
                JOIN [dgmesnie].[Gestor_Actividades] a ON a.ActividadId = t.ActividadId
                LEFT JOIN [dgmesnie].[Usuario] u ON u.IdUsuario = t.ResponsableId
                WHERE t.Activo = 1
                  AND (@actividadId IS NULL OR t.ActividadId = @actividadId)
                ORDER BY t.ActividadId, t.TemaId";

            var temas = new List<GestorTema>();
            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@actividadId", (object?)actividadId ?? DBNull.Value);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var t = MapTema(rd);
                t.Semaforo = CalcularSemaforo(t.Estatus, t.FechaCompromiso, t.Bloqueada, t.FechaUltimaActualizacion);
                temas.Add(t);
            }
            rd.Close();

            foreach (var t in temas)
            {
                t.Corresponsables = await CargarCorresponsablesAsync(cn, t.TemaId, null);
                t.Etapas = await CargarEtapasAsync(cn, t.TemaId);
            }

            return temas;
        }

        public async Task<GestorTema?> ObtenerTemaPorIdAsync(int temaId)
        {
            const string sql = @"
                SELECT t.TemaId, t.Clave, t.ActividadId, a.Actividad AS ActividadNombre,
                       t.Tema, t.Descripcion, t.ResponsableId,
                       u.Nombre AS ResponsableNombre,
                       t.FechaInicio, t.FechaCompromiso,
                       t.Estatus, t.Prioridad, t.Avance,
                       t.Bloqueada, t.MotivoBloqueO, t.EvidenciaUrl, t.Comentarios,
                       t.FechaUltimaActualizacion, t.FechaCreacion
                FROM [dgmesnie].[Gestor_Temas] t
                JOIN [dgmesnie].[Gestor_Actividades] a ON a.ActividadId = t.ActividadId
                LEFT JOIN [dgmesnie].[Usuario] u ON u.IdUsuario = t.ResponsableId
                WHERE t.TemaId = @id AND t.Activo = 1";

            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", temaId);
            await using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;
            var t = MapTema(rd);
            rd.Close();
            t.Corresponsables = await CargarCorresponsablesAsync(cn, temaId, null);
            t.Etapas = await CargarEtapasAsync(cn, temaId);
            t.Semaforo = CalcularSemaforo(t.Estatus, t.FechaCompromiso, t.Bloqueada, t.FechaUltimaActualizacion);
            return t;
        }

        public async Task<int> CrearTemaAsync(GestorTemaForm form, int? usuarioId)
        {
            const string sql = @"
                DECLARE @n INT;
                SELECT @n = ISNULL(MAX(TemaId),0)+1 FROM [dgmesnie].[Gestor_Temas];
                INSERT INTO [dgmesnie].[Gestor_Temas]
                    (Clave,ActividadId,Tema,Descripcion,ResponsableId,
                     FechaInicio,FechaCompromiso,Estatus,Prioridad,Avance,
                     Bloqueada,MotivoBloqueO,EvidenciaUrl,Comentarios,CreadoPor,
                     FechaUltimaActualizacion,FechaCreacion)
                VALUES
                    ('T-'+RIGHT('000'+CAST(@n AS NVARCHAR),3),@actividadId,@tema,@desc,@resp,
                     @inicio,@comp,@est,@pri,@avance,
                     @bloq,@motivo,@evi,@coment,@creador,
                     GETDATE(),GETDATE());
                SELECT SCOPE_IDENTITY();";

            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
            try
            {
                await using var cmd = new SqlCommand(sql, cn, tx);
                AddTemaParams(cmd, form);
                cmd.Parameters.AddWithValue("@creador", (object?)usuarioId ?? DBNull.Value);
                var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                await SincronizarCorresponsablesAsync(cn, tx, id, null, form.CorresponsablesIds);

                var stages = form.Etapas;
                if (stages == null || !stages.Any())
                {
                    stages = new List<GestorEtapaForm>
                    {
                        new GestorEtapaForm
                        {
                            Nombre = "Etapa Inicial",
                            ResponsableId = form.ResponsableId ?? 0,
                            FechaInicio = form.FechaInicio,
                            FechaCompromiso = form.FechaCompromiso,
                            Avance = form.Avance,
                            Estatus = form.Estatus
                        }
                    };
                }
                await SincronizarEtapasAsync(cn, tx, id, stages);
                await RecalcularTemaAgregadosAsync(cn, tx, id);

                await tx.CommitAsync();
                return id;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task ActualizarTemaAsync(GestorTemaForm form, int? usuarioId)
        {
            const string sql = @"
                UPDATE [dgmesnie].[Gestor_Temas] SET
                    ActividadId = @actividadId, Tema = @tema, Descripcion = @desc,
                    ResponsableId = @resp, FechaInicio = @inicio, FechaCompromiso = @comp,
                    Estatus = @est, Prioridad = @pri, Avance = @avance,
                    Bloqueada = @bloq, MotivoBloqueO = @motivo,
                    EvidenciaUrl = @evi, Comentarios = @coment,
                    FechaUltimaActualizacion = GETDATE()
                WHERE TemaId = @temaId AND Activo = 1";

            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
            try
            {
                await using var cmd = new SqlCommand(sql, cn, tx);
                AddTemaParams(cmd, form);
                cmd.Parameters.AddWithValue("@temaId", form.TemaId!.Value);
                await cmd.ExecuteNonQueryAsync();
                await SincronizarCorresponsablesAsync(cn, tx, form.TemaId.Value, null, form.CorresponsablesIds);

                if (form.Etapas != null && form.Etapas.Any())
                {
                    await SincronizarEtapasAsync(cn, tx, form.TemaId.Value, form.Etapas);
                    await RecalcularTemaAgregadosAsync(cn, tx, form.TemaId.Value);
                }
                else
                {
                    const string checkStg = "SELECT COUNT(*) FROM [dgmesnie].[Gestor_Temas_Etapas] WHERE TemaId = @temaId AND Activo = 1";
                    int count = 0;
                    await using (var cmdCheck = new SqlCommand(checkStg, cn, tx))
                    {
                        cmdCheck.Parameters.AddWithValue("@temaId", form.TemaId.Value);
                        count = Convert.ToInt32(await cmdCheck.ExecuteScalarAsync());
                    }
                    if (count == 0)
                    {
                        var defaultStages = new List<GestorEtapaForm>
                        {
                            new GestorEtapaForm
                            {
                                Nombre = "Etapa Inicial",
                                ResponsableId = form.ResponsableId ?? 0,
                                FechaInicio = form.FechaInicio,
                                FechaCompromiso = form.FechaCompromiso,
                                Avance = form.Avance,
                                Estatus = form.Estatus
                            }
                        };
                        await SincronizarEtapasAsync(cn, tx, form.TemaId.Value, defaultStages);
                        await RecalcularTemaAgregadosAsync(cn, tx, form.TemaId.Value);
                    }
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task EliminarTemaAsync(int temaId)
        {
            const string sql = "UPDATE [dgmesnie].[Gestor_Temas] SET Activo = 0 WHERE TemaId = @id";
            await using var cn = new SqlConnection(_conn);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", temaId);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── Mappers ───────────────────────────────────────────────────────────
        private static GestorActividad MapActividad(SqlDataReader rd) => new()
        {
            ActividadId = rd.GetInt32(rd.GetOrdinal("ActividadId")),
            Clave = rd.GetString(rd.GetOrdinal("Clave")),
            Actividad = rd.GetString(rd.GetOrdinal("Actividad")),
            Descripcion = rd.IsDBNull(rd.GetOrdinal("Descripcion")) ? null : rd.GetString(rd.GetOrdinal("Descripcion")),
            Categoria = rd.IsDBNull(rd.GetOrdinal("Categoria")) ? null : rd.GetString(rd.GetOrdinal("Categoria")),
            Prioridad = rd.GetString(rd.GetOrdinal("Prioridad")),
            Estatus = rd.GetString(rd.GetOrdinal("Estatus")),
            ResponsablePrincipalId = rd.IsDBNull(rd.GetOrdinal("ResponsablePrincipalId")) ? null : rd.GetInt32(rd.GetOrdinal("ResponsablePrincipalId")),
            ResponsableNombre = rd.IsDBNull(rd.GetOrdinal("ResponsableNombre")) ? null : rd.GetString(rd.GetOrdinal("ResponsableNombre")),
            FechaInicio = rd.IsDBNull(rd.GetOrdinal("FechaInicio")) ? null : rd.GetDateTime(rd.GetOrdinal("FechaInicio")),
            FechaCompromiso = rd.IsDBNull(rd.GetOrdinal("FechaCompromiso")) ? null : rd.GetDateTime(rd.GetOrdinal("FechaCompromiso")),
            AvanceGeneral = rd.GetInt32(rd.GetOrdinal("AvanceGeneral")),
            LigaSharePoint = rd.IsDBNull(rd.GetOrdinal("LigaSharePoint")) ? null : rd.GetString(rd.GetOrdinal("LigaSharePoint")),
            ComentariosEjecutivos = rd.IsDBNull(rd.GetOrdinal("ComentariosEjecutivos")) ? null : rd.GetString(rd.GetOrdinal("ComentariosEjecutivos")),
            FechaUltimaActualizacion = rd.GetDateTime(rd.GetOrdinal("FechaUltimaActualizacion")),
            FechaCreacion = rd.GetDateTime(rd.GetOrdinal("FechaCreacion"))
        };

        private static GestorTema MapTema(SqlDataReader rd) => new()
        {
            TemaId = rd.GetInt32(rd.GetOrdinal("TemaId")),
            Clave = rd.GetString(rd.GetOrdinal("Clave")),
            ActividadId = rd.GetInt32(rd.GetOrdinal("ActividadId")),
            ActividadNombre = rd.IsDBNull(rd.GetOrdinal("ActividadNombre")) ? null : rd.GetString(rd.GetOrdinal("ActividadNombre")),
            Tema = rd.GetString(rd.GetOrdinal("Tema")),
            Descripcion = rd.IsDBNull(rd.GetOrdinal("Descripcion")) ? null : rd.GetString(rd.GetOrdinal("Descripcion")),
            ResponsableId = rd.IsDBNull(rd.GetOrdinal("ResponsableId")) ? null : rd.GetInt32(rd.GetOrdinal("ResponsableId")),
            ResponsableNombre = rd.IsDBNull(rd.GetOrdinal("ResponsableNombre")) ? null : rd.GetString(rd.GetOrdinal("ResponsableNombre")),
            FechaInicio = rd.IsDBNull(rd.GetOrdinal("FechaInicio")) ? null : rd.GetDateTime(rd.GetOrdinal("FechaInicio")),
            FechaCompromiso = rd.IsDBNull(rd.GetOrdinal("FechaCompromiso")) ? null : rd.GetDateTime(rd.GetOrdinal("FechaCompromiso")),
            Estatus = rd.GetString(rd.GetOrdinal("Estatus")),
            Prioridad = rd.GetString(rd.GetOrdinal("Prioridad")),
            Avance = rd.GetInt32(rd.GetOrdinal("Avance")),
            Bloqueada = rd.GetBoolean(rd.GetOrdinal("Bloqueada")),
            MotivoBloqueo = rd.IsDBNull(rd.GetOrdinal("MotivoBloqueO")) ? null : rd.GetString(rd.GetOrdinal("MotivoBloqueO")),
            EvidenciaUrl = rd.IsDBNull(rd.GetOrdinal("EvidenciaUrl")) ? null : rd.GetString(rd.GetOrdinal("EvidenciaUrl")),
            Comentarios = rd.IsDBNull(rd.GetOrdinal("Comentarios")) ? null : rd.GetString(rd.GetOrdinal("Comentarios")),
            FechaUltimaActualizacion = rd.GetDateTime(rd.GetOrdinal("FechaUltimaActualizacion")),
            FechaCreacion = rd.GetDateTime(rd.GetOrdinal("FechaCreacion"))
        };

        // ── Param helpers ─────────────────────────────────────────────────────
        private static void AddActividadParams(SqlCommand cmd, GestorActividadForm f)
        {
            cmd.Parameters.AddWithValue("@act", f.Actividad);
            cmd.Parameters.AddWithValue("@desc", (object?)f.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cat", (object?)f.Categoria ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pri", f.Prioridad);
            cmd.Parameters.AddWithValue("@est", f.Estatus);
            cmd.Parameters.AddWithValue("@resp", (object?)f.ResponsablePrincipalId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@inicio", (object?)f.FechaInicio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@comp", (object?)f.FechaCompromiso ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@liga", (object?)f.LigaSharePoint ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@coment", (object?)f.ComentariosEjecutivos ?? DBNull.Value);
        }

        private static void AddTemaParams(SqlCommand cmd, GestorTemaForm f)
        {
            cmd.Parameters.AddWithValue("@actividadId", f.ActividadId);
            cmd.Parameters.AddWithValue("@tema", f.Tema);
            cmd.Parameters.AddWithValue("@desc", (object?)f.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@resp", (object?)f.ResponsableId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@inicio", (object?)f.FechaInicio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@comp", (object?)f.FechaCompromiso ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@est", f.Estatus);
            cmd.Parameters.AddWithValue("@pri", f.Prioridad);
            cmd.Parameters.AddWithValue("@avance", f.Avance);
            cmd.Parameters.AddWithValue("@bloq", f.Bloqueada);
            cmd.Parameters.AddWithValue("@motivo", (object?)f.MotivoBloqueo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@evi", (object?)f.EvidenciaUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@coment", (object?)f.Comentarios ?? DBNull.Value);
        }
    }
}
