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
                t.Corresponsables = await CargarCorresponsablesAsync(cn, t.TemaId, null);

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
