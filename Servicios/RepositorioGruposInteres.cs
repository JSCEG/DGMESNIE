using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSIE.Models.Gestor;
using NSIE.Servicios.Interfaces;

namespace NSIE.Servicios
{
    public class RepositorioGruposInteres : IRepositorioGruposInteres
    {
        private readonly string _connStr;
        private readonly ILogger<RepositorioGruposInteres> _logger;

        public RepositorioGruposInteres(IConfiguration config, ILogger<RepositorioGruposInteres> logger)
        {
            _connStr = config.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public async Task<List<GrupoInteresDto>> ObtenerTodosAsync()
        {
            const string sql = @"
                SELECT g.GrupoInteresId, g.Nombre, g.Origen, g.Contacto, g.Correo, g.Telefono,
                       (SELECT COUNT(*) 
                        FROM dgmesnie.GrupoInteresProyecto p 
                        WHERE p.GrupoInteresId = g.GrupoInteresId AND p.Activo = 1) AS TotalProyectos
                FROM dgmesnie.GrupoInteres g
                WHERE g.Activo = 1
                ORDER BY g.Nombre";

            try
            {
                using IDbConnection db = new SqlConnection(_connStr);
                var list = await db.QueryAsync<GrupoInteresDto>(sql);
                return list.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los Grupos de Interés.");
                throw;
            }
        }

        public async Task<GrupoInteres?> ObtenerPorIdAsync(int id)
        {
            const string sqlGEI = @"
                SELECT GrupoInteresId, Nombre, Origen, Contacto, Correo, Telefono, Activo, CreadoEn, CreadoPor, ActualizadoEn, ActualizadoPor
                FROM dgmesnie.GrupoInteres
                WHERE GrupoInteresId = @id AND Activo = 1";

            const string sqlProyectos = @"
                SELECT ProyectoId, GrupoInteresId, RazonSocial, NombreProyecto, Contacto, Correo, Telefono, Activo, CreadoEn, CreadoPor, ActualizadoEn, ActualizadoPor
                FROM dgmesnie.GrupoInteresProyecto
                WHERE GrupoInteresId = @id AND Activo = 1";

            try
            {
                using IDbConnection db = new SqlConnection(_connStr);
                var gei = await db.QueryFirstOrDefaultAsync<GrupoInteres>(sqlGEI, new { id });
                if (gei != null)
                {
                    var proyectos = await db.QueryAsync<GrupoInteresProyecto>(sqlProyectos, new { id });
                    gei.Proyectos = proyectos.ToList();
                }
                return gei;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el Grupo de Interés por ID {Id}.", id);
                throw;
            }
        }

        public async Task<int> CrearAsync(GrupoInteresForm form, int? usuarioId)
        {
            const string sqlInsertGEI = @"
                INSERT INTO dgmesnie.GrupoInteres (Nombre, Origen, Contacto, Correo, Telefono, CreadoPor, CreadoEn)
                VALUES (@Nombre, @Origen, @Contacto, @Correo, @Telefono, @CreadoPor, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() as int);";

            const string sqlInsertProyecto = @"
                INSERT INTO dgmesnie.GrupoInteresProyecto (GrupoInteresId, RazonSocial, NombreProyecto, Contacto, Correo, Telefono, CreadoPor, CreadoEn)
                VALUES (@GrupoInteresId, @RazonSocial, @NombreProyecto, @Contacto, @Correo, @Telefono, @CreadoPor, SYSUTCDATETIME());";

            using var connection = new SqlConnection(_connStr);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var username = usuarioId?.ToString();
                
                // Insertar GEI
                var geiId = await connection.QuerySingleAsync<int>(sqlInsertGEI, new
                {
                    form.Nombre,
                    form.Origen,
                    form.Contacto,
                    form.Correo,
                    form.Telefono,
                    CreadoPor = username
                }, transaction);

                // Insertar proyectos hijos
                if (form.Proyectos != null && form.Proyectos.Any())
                {
                    foreach (var p in form.Proyectos)
                    {
                        await connection.ExecuteAsync(sqlInsertProyecto, new
                        {
                            GrupoInteresId = geiId,
                            p.RazonSocial,
                            p.NombreProyecto,
                            p.Contacto,
                            p.Correo,
                            p.Telefono,
                            CreadoPor = username
                        }, transaction);
                    }
                }

                transaction.Commit();
                return geiId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error al crear transaccionalmente el Grupo de Interés.");
                throw;
            }
        }

        public async Task ActualizarAsync(GrupoInteresForm form, int? usuarioId)
        {
            const string sqlUpdateGEI = @"
                UPDATE dgmesnie.GrupoInteres
                SET Nombre = @Nombre, Origen = @Origen, Contacto = @Contacto, Correo = @Correo, Telefono = @Telefono,
                    ActualizadoPor = @ActualizadoPor, ActualizadoEn = SYSUTCDATETIME()
                WHERE GrupoInteresId = @GrupoInteresId AND Activo = 1";

            const string sqlDeleteProyectos = @"
                DELETE FROM dgmesnie.GrupoInteresProyecto 
                WHERE GrupoInteresId = @GrupoInteresId";

            const string sqlInsertProyecto = @"
                INSERT INTO dgmesnie.GrupoInteresProyecto (GrupoInteresId, RazonSocial, NombreProyecto, Contacto, Correo, Telefono, CreadoPor, CreadoEn)
                VALUES (@GrupoInteresId, @RazonSocial, @NombreProyecto, @Contacto, @Correo, @Telefono, @CreadoPor, SYSUTCDATETIME());";

            using var connection = new SqlConnection(_connStr);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var username = usuarioId?.ToString();

                // Actualizar GEI
                await connection.ExecuteAsync(sqlUpdateGEI, new
                {
                    form.Nombre,
                    form.Origen,
                    form.Contacto,
                    form.Correo,
                    form.Telefono,
                    ActualizadoPor = username,
                    form.GrupoInteresId
                }, transaction);

                // Eliminar proyectos previos
                await connection.ExecuteAsync(sqlDeleteProyectos, new { form.GrupoInteresId }, transaction);

                // Insertar nuevos proyectos
                if (form.Proyectos != null && form.Proyectos.Any())
                {
                    foreach (var p in form.Proyectos)
                    {
                        await connection.ExecuteAsync(sqlInsertProyecto, new
                        {
                            form.GrupoInteresId,
                            p.RazonSocial,
                            p.NombreProyecto,
                            p.Contacto,
                            p.Correo,
                            p.Telefono,
                            CreadoPor = username
                        }, transaction);
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error al actualizar transaccionalmente el Grupo de Interés con ID {Id}.", form.GrupoInteresId);
                throw;
            }
        }

        public async Task EliminarAsync(int id)
        {
            const string sql = @"
                UPDATE dgmesnie.GrupoInteres SET Activo = 0 WHERE GrupoInteresId = @id;
                UPDATE dgmesnie.GrupoInteresProyecto SET Activo = 0 WHERE GrupoInteresId = @id;";

            try
            {
                using IDbConnection db = new SqlConnection(_connStr);
                await db.ExecuteAsync(sql, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar la eliminación lógica del Grupo de Interés con ID {Id}.", id);
                throw;
            }
        }

        public async Task<GruposInteresDashboardVM> ObtenerDashboardDataAsync()
        {
            const string sqlGrupos = "SELECT COUNT(*) FROM dgmesnie.GrupoInteres WHERE Activo = 1";
            const string sqlProyectos = "SELECT COUNT(*) FROM dgmesnie.GrupoInteresProyecto WHERE Activo = 1";
            const string sqlPaises = "SELECT COUNT(DISTINCT Origen) FROM dgmesnie.GrupoInteres WHERE Activo = 1 AND Origen IS NOT NULL AND Origen <> ''";
            
            const string sqlDist = @"
                SELECT COALESCE(NULLIF(TRIM(Origen), ''), 'No especificado') AS Pais, COUNT(*) AS Cantidad
                FROM dgmesnie.GrupoInteres
                WHERE Activo = 1
                GROUP BY Origen
                ORDER BY Cantidad DESC";

            const string sqlTop = @"
                SELECT TOP 10 g.GrupoInteresId, g.Nombre, g.Origen, g.Contacto, g.Correo, g.Telefono,
                       (SELECT COUNT(*) 
                        FROM dgmesnie.GrupoInteresProyecto p 
                        WHERE p.GrupoInteresId = g.GrupoInteresId AND p.Activo = 1) AS TotalProyectos
                FROM dgmesnie.GrupoInteres g
                WHERE g.Activo = 1
                ORDER BY TotalProyectos DESC, g.Nombre ASC";

            try
            {
                using IDbConnection db = new SqlConnection(_connStr);
                
                var vm = new GruposInteresDashboardVM
                {
                    TotalGrupos = await db.ExecuteScalarAsync<int>(sqlGrupos),
                    TotalProyectos = await db.ExecuteScalarAsync<int>(sqlProyectos),
                    TotalPaises = await db.ExecuteScalarAsync<int>(sqlPaises),
                    PaisesDistribucion = (await db.QueryAsync<PaisDistribucionDto>(sqlDist)).ToList(),
                    TopGruposProyectos = (await db.QueryAsync<GrupoInteresDto>(sqlTop)).ToList()
                };

                return vm;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos del Dashboard de Grupos de Interés.");
                throw;
            }
        }

        public async Task<List<UsuarioCorreoDto>> ObtenerUsuariosCorreoAsync()
        {
            const string sql = @"
                SELECT IdUsuario, Nombre, Correo
                FROM dgmesnie.Usuario
                WHERE Vigente = 1
                  AND Correo IS NOT NULL
                  AND LTRIM(RTRIM(Correo)) <> ''
                ORDER BY Nombre, Correo";

            try
            {
                using IDbConnection db = new SqlConnection(_connStr);
                var usuarios = await db.QueryAsync<UsuarioCorreoDto>(sql);
                return usuarios.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios vigentes con correo.");
                throw;
            }
        }
    }
}
