using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Controllers;
using NSIE.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;

namespace NSIE.Servicios
{
    using NSIE.Models;

    public interface IRepositorioSecciones
    {
        Task<List<SeccionConModulos>> ObtenerTodasLasSeccionesAsync();
        Task<SeccionConModulos> ObtenerSeccionPorIdAsync(int id);
        Task CrearSeccionAsync(SeccionConModulos seccion);
        Task ActualizarSeccionAsync(SeccionConModulos seccion);
        Task EliminarSeccionAsync(int id); // ✅ Ya está declarado

        Task AgregarModuloAsync(Modulo modulo);
        Task ActualizarModuloAsync(Modulo modulo);
        Task EliminarModuloAsync(int id);

        Task<Modulo> ObtenerModuloPorIdAsync(int id);

        // Métodos para ModulosVista
        Task<List<ModulosVista>> ObtenerVistasPorModuloIdAsync(int moduloId);
        Task<ModulosVista> ObtenerModuloVistaPorIdAsync(int vistaId);
        Task CrearModuloVistaAsync(ModulosVista vista);
        Task ActualizarModuloVistaAsync(ModulosVista vista);
        Task EliminarModuloVistaAsync(int vistaId);
        Task<int> ContarVistasPorModuloAsync(int moduloId);

        // NUEVOS MÉTODOS QUE FALTAN
        Task<List<SeccionConModulos>> ObtenerSeccionesConModulosAsync();
        Task<int> ContarModulosPorSeccionAsync(int seccionId);
        Task ActualizarOrdenSeccionAsync(int seccionId, int nuevoOrden);

        // ✅ AGREGAR ESTE MÉTODO QUE FALTA
        Task<List<ModuloSNIER>> ObtenerModulosPorSeccionAsync(int seccionId);
        Task<List<Usuario>> ObtenerUsuariosVigentesAsync();
    }
    // DTOs para mapeo de Dapper en ObtenerSeccionesConModulosAsync
    public class SeccionDto
    {
        public int SeccionId { get; set; }
        public string SeccionTitulo { get; set; }
        public string Articulos { get; set; }
        public string FundamentoLegal { get; set; }
        public string Descripcion { get; set; }
        public string Ayuda { get; set; }
        public string Objetivo { get; set; }
        public string ResponsableNormativo { get; set; }
        public string PublicoObjetivo { get; set; }
        public bool SeccionActiva { get; set; }
        public int SeccionOrden { get; set; }
    }

    public class ModuloDto
    {
        public int ModuloId { get; set; }
        public int ModuloSeccionId { get; set; }
        public string ModuloTitle { get; set; }
        public string FundamentoLegalModulo { get; set; }
        public string ModuloRoles { get; set; }
        public string NombresRoles { get; set; }
        public string ModuloPerfiles { get; set; }
        public string Etapa { get; set; }
        public string JustificacionOrden { get; set; }
        public string AyudaContextual { get; set; }
        public string ModuloController { get; set; }
        public string ModuloAction { get; set; }
        public string ModuloDesc { get; set; }
        public string Img { get; set; }
        public string Btn { get; set; }
        public string ElementosUI { get; set; }
        public string AyudaVista { get; set; }
        public int ModuloOrden { get; set; }
        public bool ModuloActivo { get; set; }
    }

    public class RepositorioSecciones : IRepositorioSecciones
    {
        private readonly string _connectionString;

        private const string SelectSeccionCompat = @"
            SELECT
                [SeccionId] AS [Id],
                [Titulo],
                [Articulos],
                [FundamentoLegal],
                [Descripcion],
                [Ayuda],
                [Objetivo],
                [ResponsableNormativo],
                [PublicoObjetivo],
                [Activa] AS [Activo],
                [Orden]
            FROM [dgmesnie].[Seccion]";

        private const string SelectModuloCompat = @"
            SELECT
                m.[ModuloId] AS [Id],
                m.[SeccionId],
                m.[Title],
                m.[FundamentoLegalModulo],
                (
                    SELECT STRING_AGG(CAST(rm.[RolId] AS NVARCHAR(20)), ',')
                    FROM [dgmesnie].[RolModulo] rm
                    WHERE rm.[ModuloId] = m.[ModuloId]
                        AND rm.[Activa] = 1
                ) AS [Roles],
                (
                    SELECT STRING_AGG(r.[RolNombre], ', ')
                    FROM [dgmesnie].[RolModulo] rm
                    INNER JOIN [dgmesnie].[Rol] r ON r.[RolId] = rm.[RolId]
                    WHERE rm.[ModuloId] = m.[ModuloId]
                        AND rm.[Activa] = 1
                ) AS [NombresRoles],
                m.[Perfiles],
                m.[Etapa],
                m.[JustificacionOrden],
                m.[AyudaContextual],
                m.[Controller],
                m.[Action],
                m.[Descripcion] AS [Desc],
                m.[Imagen] AS [Img],
                m.[BotonTexto] AS [Btn],
                m.[ElementosUI],
                m.[AyudaVista],
                m.[Orden],
                m.[Activo]
            FROM [dgmesnie].[Modulo] m";

        public RepositorioSecciones(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<SeccionConModulos>> ObtenerTodasLasSeccionesAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var secciones = await connection.QueryAsync<SeccionConModulos>(SelectSeccionCompat + " ORDER BY [Orden]");
            foreach (var seccion in secciones)
            {
                var modulos = await connection.QueryAsync<Modulo>(
                    SelectModuloCompat + " WHERE [SeccionId] = @SeccionId ORDER BY [Orden]",
                    new { SeccionId = seccion.Id });
                seccion.Modulos = modulos.ToList();
            }

            return secciones.ToList();
        }

        public async Task<SeccionConModulos> ObtenerSeccionPorIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var seccion = await connection.QueryFirstOrDefaultAsync<SeccionConModulos>(
                SelectSeccionCompat + " WHERE [SeccionId] = @Id",
                new { Id = id });

            if (seccion != null)
            {
                var modulos = await connection.QueryAsync<Modulo>(
                    SelectModuloCompat + " WHERE [SeccionId] = @SeccionId ORDER BY [Orden]",
                    new { SeccionId = seccion.Id });
                seccion.Modulos = modulos.ToList();
                
                foreach (var modulo in seccion.Modulos)
                {
                    var vistas = await connection.QueryAsync<VistaSNIER>(
                        "SELECT [VistaId], [ModuloId], [Titulo], [Controller], [Action], [Perfiles], [Orden], [Activa], [EsExterno] FROM [dgmesnie].[Vista] WHERE [ModuloId] = @ModuloId ORDER BY [Orden]",
                        new { ModuloId = modulo.Id });
                    modulo.Vistas = vistas.ToList();
                }
            }

            return seccion;
        }

        public async Task CrearSeccionAsync(SeccionConModulos seccion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO [dgmesnie].[Seccion]
                        ([Titulo], [Articulos], [FundamentoLegal], [Descripcion], [Ayuda], [Objetivo], [ResponsableNormativo], [PublicoObjetivo], [Activa], [Orden])
                        VALUES (@Titulo, @Articulos, @FundamentoLegal, @Descripcion, @Ayuda, @Objetivo, @ResponsableNormativo, @PublicoObjetivo, @Activo, @Orden)";

            await connection.ExecuteAsync(sql, seccion);
        }

        public async Task ActualizarSeccionAsync(SeccionConModulos seccion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE [dgmesnie].[Seccion] SET 
                        [Titulo] = @Titulo,
                        [Articulos] = @Articulos,
                        [FundamentoLegal] = @FundamentoLegal,
                        [Descripcion] = @Descripcion,
                        [Ayuda] = @Ayuda,
                        [Objetivo] = @Objetivo,
                        [ResponsableNormativo] = @ResponsableNormativo,
                        [PublicoObjetivo] = @PublicoObjetivo,
                        [Activa] = @Activo,
                        [Orden] = @Orden
                        WHERE [SeccionId] = @Id";

            await connection.ExecuteAsync(sql, seccion);
        }

        public async Task<List<ModuloSNIER>> ObtenerModulosPorSeccionAsync(int seccionId)
        {
            Console.WriteLine($">>> ObtenerModulosPorSeccionAsync - SeccionId: {seccionId}");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = @"
                    SELECT
                        [ModuloId] AS [ModuloId],
                        [SeccionId] AS [SeccionId],
                        [Title] AS [Title],
                        [FundamentoLegalModulo] AS [FundamentoLegalModulo],
                        CAST(NULL AS NVARCHAR(MAX)) AS [Roles],
                        [Perfiles] AS [Perfiles],
                        [Etapa] AS [Etapa],
                        [JustificacionOrden] AS [JustificacionOrden],
                        [AyudaContextual] AS [AyudaContextual],
                        [Controller] AS [Controller],
                        [Action] AS [Action],
                        [Descripcion] AS [Desc],
                        [Imagen] AS [Img],
                        [BotonTexto] AS [Btn],
                        [ElementosUI] AS [ElementosUI],
                        [AyudaVista] AS [AyudaVista],
                        [Orden] AS [Orden],
                        [Activo] AS [Activo],
                        [EsExterno] AS [EsExterno]
                    FROM [dgmesnie].[Modulo]
                    WHERE [SeccionId] = @SeccionId 
                    ORDER BY [Orden]";

                var modulos = await connection.QueryAsync<ModuloSNIER>(sql, new { SeccionId = seccionId });

                Console.WriteLine($">>> Módulos encontrados: {modulos.Count()}");
                return modulos.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> ERROR en ObtenerModulosPorSeccionAsync: {ex.Message}");
                throw;
            }
        }

        public async Task EliminarSeccionAsync(int seccionId)
        {
            Console.WriteLine($">>> EliminarSeccionAsync - ID: {seccionId}");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();

                try
                {
                    var sqlOverrides = @"
                        DELETE uvo
                        FROM [dgmesnie].[UsuarioVistaOverride] uvo
                        INNER JOIN [dgmesnie].[Vista] v ON v.[VistaId] = uvo.[VistaId]
                        INNER JOIN [dgmesnie].[Modulo] m ON m.[ModuloId] = v.[ModuloId]
                        WHERE m.[SeccionId] = @SeccionId;";
                    await connection.ExecuteAsync(sqlOverrides, new { SeccionId = seccionId }, transaction);

                    var sqlRolVista = @"
                        DELETE rv
                        FROM [dgmesnie].[RolVista] rv
                        INNER JOIN [dgmesnie].[Vista] v ON v.[VistaId] = rv.[VistaId]
                        INNER JOIN [dgmesnie].[Modulo] m ON m.[ModuloId] = v.[ModuloId]
                        WHERE m.[SeccionId] = @SeccionId;";
                    await connection.ExecuteAsync(sqlRolVista, new { SeccionId = seccionId }, transaction);

                    var sqlVistas = @"
                        DELETE v
                        FROM [dgmesnie].[Vista] v
                        INNER JOIN [dgmesnie].[Modulo] m ON m.[ModuloId] = v.[ModuloId]
                        WHERE m.[SeccionId] = @SeccionId;";
                    await connection.ExecuteAsync(sqlVistas, new { SeccionId = seccionId }, transaction);

                    var sqlRolModulo = "DELETE FROM [dgmesnie].[RolModulo] WHERE [ModuloId] IN (SELECT [ModuloId] FROM [dgmesnie].[Modulo] WHERE [SeccionId] = @SeccionId)";
                    await connection.ExecuteAsync(sqlRolModulo, new { SeccionId = seccionId }, transaction);

                    var sqlModulos = "DELETE FROM [dgmesnie].[Modulo] WHERE [SeccionId] = @SeccionId";
                    var modulosEliminados = await connection.ExecuteAsync(sqlModulos, new { SeccionId = seccionId }, transaction);
                    Console.WriteLine($">>> Módulos eliminados: {modulosEliminados}");

                    var sqlRolSeccion = "DELETE FROM [dgmesnie].[RolSeccion] WHERE [SeccionId] = @SeccionId";
                    await connection.ExecuteAsync(sqlRolSeccion, new { SeccionId = seccionId }, transaction);

                    // Luego eliminar la sección
                    var sqlSeccion = "DELETE FROM [dgmesnie].[Seccion] WHERE [SeccionId] = @Id";
                    var seccionEliminada = await connection.ExecuteAsync(sqlSeccion, new { Id = seccionId }, transaction);
                    Console.WriteLine($">>> Sección eliminada: {seccionEliminada}");

                    if (seccionEliminada == 0)
                    {
                        throw new Exception("No se pudo eliminar la sección de la base de datos");
                    }

                    transaction.Commit();
                    Console.WriteLine($">>> Eliminación completada exitosamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($">>> ERROR en transacción: {ex.Message}");
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> ERROR en EliminarSeccionAsync: {ex.Message}");
                throw;
            }
        }

        public async Task AgregarModuloAsync(Modulo modulo)
        {
            Console.WriteLine(">>> Insertando módulo con título: " + modulo.Title);
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            var sql = @"INSERT INTO [dgmesnie].[Modulo]
                    (
                        [SeccionId], [Title], [FundamentoLegalModulo], [Perfiles], [Etapa],
                        [JustificacionOrden], [AyudaContextual], [Controller], [Action], [Descripcion],
                        [Imagen], [BotonTexto], [ElementosUI], [AyudaVista], [Orden], [Activo]
                    )
                    OUTPUT INSERTED.[ModuloId]
                    VALUES
                    (
                        @SeccionId, @Title, @FundamentoLegalModulo, @Perfiles, @Etapa,
                        @JustificacionOrden, @AyudaContextual, @Controller, @Action, @Desc,
                        @Img, @Btn, @ElementosUI, @AyudaVista, @Orden, @Activo
                    )";

            var moduloId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                modulo.SeccionId,
                modulo.Title,
                modulo.FundamentoLegalModulo,
                modulo.Perfiles,
                modulo.Etapa,
                modulo.JustificacionOrden,
                modulo.AyudaContextual,
                modulo.Controller,
                modulo.Action,
                modulo.Desc,
                modulo.Img,
                modulo.Btn,
                modulo.ElementosUI,
                modulo.AyudaVista,
                modulo.Orden,
                modulo.Activo
            }, transaction);

            await SyncRolModuloAsync(connection, transaction, moduloId, modulo.Roles);
            await transaction.CommitAsync();

        }

        public async Task ActualizarModuloAsync(Modulo modulo)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            var sql = @"UPDATE [dgmesnie].[Modulo] SET
                    [Title] = @Title,
                    [FundamentoLegalModulo] = @FundamentoLegalModulo,
                    [Perfiles] = @Perfiles,
                    [Etapa] = @Etapa,
                    [JustificacionOrden] = @JustificacionOrden,
                    [AyudaContextual] = @AyudaContextual,
                    [Controller] = @Controller,
                    [Action] = @Action,
                    [Descripcion] = @Desc,
                    [Imagen] = @Img,
                    [BotonTexto] = @Btn,
                    [ElementosUI] = @ElementosUI,
                    [AyudaVista] = @AyudaVista,
                    [Orden] = @Orden,
                    [Activo] = @Activo
                    WHERE [ModuloId] = @Id";

            await connection.ExecuteAsync(sql, modulo, transaction);
            await SyncRolModuloAsync(connection, transaction, modulo.Id, modulo.Roles);
            await transaction.CommitAsync();
        }

        public async Task EliminarModuloAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await connection.ExecuteAsync(@"
                    DELETE uvo
                    FROM [dgmesnie].[UsuarioVistaOverride] uvo
                    INNER JOIN [dgmesnie].[Vista] v ON v.[VistaId] = uvo.[VistaId]
                    WHERE v.[ModuloId] = @Id;",
                    new { Id = id }, transaction);

                await connection.ExecuteAsync(@"
                    DELETE rv
                    FROM [dgmesnie].[RolVista] rv
                    INNER JOIN [dgmesnie].[Vista] v ON v.[VistaId] = rv.[VistaId]
                    WHERE v.[ModuloId] = @Id;",
                    new { Id = id }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM [dgmesnie].[Vista] WHERE [ModuloId] = @Id;",
                    new { Id = id }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM [dgmesnie].[RolModulo] WHERE [ModuloId] = @Id;",
                    new { Id = id }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM [dgmesnie].[Modulo] WHERE [ModuloId] = @Id;",
                    new { Id = id }, transaction);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<Modulo> ObtenerModuloPorIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = SelectModuloCompat + " WHERE [ModuloId] = @Id";
            return await connection.QueryFirstOrDefaultAsync<Modulo>(sql, new { Id = id });
        }

        public async Task<List<ModulosVista>> ObtenerVistasPorModuloIdAsync(int moduloId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    v.[VistaId] as VistaId,
                    v.ModuloId,
                    v.[Titulo],
                    v.[Controller],
                    v.[Action],
                    CAST(NULL AS NVARCHAR(150)) as Roles,
                    v.[Perfiles],
                    v.[Orden],
                    v.[Activa] as Activa,
                    v.[EsExterno],
                    m.[Title] as ModuloTitle,
                    s.[Titulo] as SeccionTitle
                FROM [dgmesnie].[Vista] v 
                INNER JOIN [dgmesnie].[Modulo] m ON v.[ModuloId] = m.[ModuloId]
                INNER JOIN [dgmesnie].[Seccion] s ON m.[SeccionId] = s.[SeccionId]
                WHERE v.[ModuloId] = @ModuloId 
                ORDER BY v.[Orden]";

            return (await connection.QueryAsync<ModulosVista>(sql, new { ModuloId = moduloId })).ToList();
        }

        public async Task<ModulosVista> ObtenerModuloVistaPorIdAsync(int vistaId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    v.[VistaId] as VistaId,
                    v.[ModuloId],
                    v.[Titulo],
                    v.[Controller],
                    v.[Action],
                    CAST(NULL AS NVARCHAR(150)) as Roles,
                    v.[Perfiles],
                    v.[Orden],
                    v.[Activa] as Activa,
                    v.[EsExterno],
                    m.[Title] as ModuloTitle,
                    s.[Titulo] as SeccionTitle
                FROM [dgmesnie].[Vista] v 
                INNER JOIN [dgmesnie].[Modulo] m ON v.[ModuloId] = m.[ModuloId]
                INNER JOIN [dgmesnie].[Seccion] s ON m.[SeccionId] = s.[SeccionId]
                WHERE v.[VistaId] = @VistaId";

            return await connection.QueryFirstOrDefaultAsync<ModulosVista>(sql, new { VistaId = vistaId });
        }

        public async Task CrearModuloVistaAsync(ModulosVista vista)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                INSERT INTO [dgmesnie].[Vista] ([ModuloId], [Titulo], [Controller], [Action], [Perfiles], [Orden], [Activa], [EsExterno])
                VALUES (@ModuloId, @Titulo, @Controller, @Action, @Perfiles, @Orden, @Activa, @EsExterno)";

            await connection.ExecuteAsync(sql, vista);
        }

        public async Task ActualizarModuloVistaAsync(ModulosVista vista)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE [dgmesnie].[Vista] SET 
                    [Titulo] = @Titulo,
                    [Controller] = @Controller,
                    [Action] = @Action,
                    [Perfiles] = @Perfiles,
                    [Orden] = @Orden,
                    [Activa] = @Activa,
                    [EsExterno] = @EsExterno
                WHERE [VistaId] = @VistaId";

            await connection.ExecuteAsync(sql, vista);
        }

        public async Task EliminarModuloVistaAsync(int vistaId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await connection.ExecuteAsync(
                    "DELETE FROM [dgmesnie].[UsuarioVistaOverride] WHERE [VistaId] = @VistaId;",
                    new { VistaId = vistaId }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM [dgmesnie].[RolVista] WHERE [VistaId] = @VistaId;",
                    new { VistaId = vistaId }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM [dgmesnie].[Vista] WHERE [VistaId] = @VistaId;",
                    new { VistaId = vistaId }, transaction);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> ContarVistasPorModuloAsync(int moduloId)
        {
            Console.WriteLine($">>> RepositorioSecciones.ContarVistasPorModuloAsync - moduloId: {moduloId}");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = "SELECT COUNT(*) FROM [dgmesnie].[Vista] WHERE [ModuloId] = @ModuloId";
                Console.WriteLine($">>> SQL: {sql}");

                var count = await connection.QuerySingleAsync<int>(sql, new { ModuloId = moduloId });
                Console.WriteLine($">>> Resultado query: {count}");

                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> Error en ContarVistasPorModuloAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<SeccionConModulos>> ObtenerSeccionesConModulosAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    -- SECCIÓN
                    s.[SeccionId] as SeccionId,
                    s.[Titulo] as SeccionTitulo,
                    s.[Articulos],
                    s.[FundamentoLegal],
                    s.[Descripcion],
                    s.[Ayuda],
                    s.[Objetivo],
                    s.[ResponsableNormativo],
                    s.[PublicoObjetivo],
                    s.[Activa] as SeccionActiva,
                    ISNULL(s.[Orden], 1) as SeccionOrden,
                    
                    -- MÓDULO
                    m.[ModuloId] as ModuloId,
                    m.[SeccionId] as ModuloSeccionId,
                    m.[Title] as ModuloTitle,
                    m.[FundamentoLegalModulo],
                    (
                        SELECT STRING_AGG(CAST(rm.[RolId] AS NVARCHAR(20)), ',')
                        FROM [dgmesnie].[RolModulo] rm
                        WHERE rm.[ModuloId] = m.[ModuloId]
                            AND rm.[Activa] = 1
                    ) as ModuloRoles,
                    (
                        SELECT STRING_AGG(r.[RolNombre], ', ')
                        FROM [dgmesnie].[RolModulo] rm
                        INNER JOIN [dgmesnie].[Rol] r ON r.[RolId] = rm.[RolId]
                        WHERE rm.[ModuloId] = m.[ModuloId]
                            AND rm.[Activa] = 1
                    ) as NombresRoles,
                    m.[Perfiles] as ModuloPerfiles,
                    m.[Etapa],
                    m.[JustificacionOrden],
                    m.[AyudaContextual],
                    m.[Controller] as ModuloController,
                    m.[Action] as ModuloAction,
                    m.[Descripcion] as ModuloDesc,
                    m.[Imagen] as Img,
                    m.[BotonTexto] as Btn,
                    m.[ElementosUI],
                    m.[AyudaVista],
                    ISNULL(m.[Orden], 1) as ModuloOrden,
                    m.[Activo] as ModuloActivo
                FROM [dgmesnie].[Seccion] s
                LEFT JOIN [dgmesnie].[Modulo] m ON s.[SeccionId] = m.[SeccionId]
                ORDER BY ISNULL(s.[Orden], 1), ISNULL(m.[Orden], 1)";

            var seccionesDict = new Dictionary<int, SeccionConModulos>();

            await connection.QueryAsync<SeccionDto, ModuloDto, SeccionConModulos>(
                sql,
                (seccionDto, moduloDto) =>
                {
                    // Convertir DTO a modelo real
                    var seccion = new SeccionConModulos
                    {
                        Id = seccionDto.SeccionId,
                        Titulo = seccionDto.SeccionTitulo,
                        Articulos = seccionDto.Articulos,
                        FundamentoLegal = seccionDto.FundamentoLegal,
                        Descripcion = seccionDto.Descripcion,
                        Ayuda = seccionDto.Ayuda,
                        Objetivo = seccionDto.Objetivo,
                        ResponsableNormativo = seccionDto.ResponsableNormativo,
                        PublicoObjetivo = seccionDto.PublicoObjetivo,
                        Activo = seccionDto.SeccionActiva,
                        Orden = seccionDto.SeccionOrden
                    };

                    if (!seccionesDict.TryGetValue(seccion.Id, out var seccionExistente))
                    {
                        seccionExistente = seccion;
                        seccionExistente.Modulos = new List<Modulo>();
                        seccionesDict[seccion.Id] = seccionExistente;
                    }

                    // Solo agregar módulo si existe
                    if (moduloDto != null && moduloDto.ModuloId > 0)
                    {
                        var modulo = new Modulo
                        {
                            Id = moduloDto.ModuloId,
                            SeccionId = moduloDto.ModuloSeccionId,
                            Title = moduloDto.ModuloTitle,
                            FundamentoLegalModulo = moduloDto.FundamentoLegalModulo,
                            Roles = moduloDto.ModuloRoles,
                            NombresRoles = moduloDto.NombresRoles,
                            Perfiles = moduloDto.ModuloPerfiles,
                            Etapa = moduloDto.Etapa,
                            JustificacionOrden = moduloDto.JustificacionOrden,
                            AyudaContextual = moduloDto.AyudaContextual,
                            Controller = moduloDto.ModuloController,
                            Action = moduloDto.ModuloAction,
                            Desc = moduloDto.ModuloDesc,
                            Img = moduloDto.Img,
                            Btn = moduloDto.Btn,
                            ElementosUI = moduloDto.ElementosUI,
                            AyudaVista = moduloDto.AyudaVista,
                            Orden = moduloDto.ModuloOrden,
                            Activo = moduloDto.ModuloActivo
                        };

                        // No duplicar módulos
                        if (!seccionExistente.Modulos.Any(m => m.Id == modulo.Id))
                        {
                            seccionExistente.Modulos.Add(modulo);
                        }
                    }

                    return seccionExistente;
                },
                splitOn: "ModuloId"
            );

            return seccionesDict.Values.OrderBy(s => s.Orden).ToList();
        }

        private static async Task SyncRolModuloAsync(SqlConnection connection, SqlTransaction transaction, int moduloId, string? roles)
        {
            await connection.ExecuteAsync(
                "DELETE FROM [dgmesnie].[RolModulo] WHERE [ModuloId] = @ModuloId;",
                new { ModuloId = moduloId },
                transaction);

            var roleIds = (roles ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => int.TryParse(value, out var roleId) ? roleId : (int?)null)
                .Where(roleId => roleId.HasValue)
                .Select(roleId => roleId!.Value)
                .Distinct()
                .ToList();

            if (!roleIds.Any())
            {
                return;
            }

            const string insertRolesSql = @"
                INSERT INTO [dgmesnie].[RolModulo] ([RolId], [ModuloId], [MercadoId], [Activa])
                VALUES (@RolId, @ModuloId, NULL, 1);";

            foreach (var roleId in roleIds)
            {
                await connection.ExecuteAsync(
                    insertRolesSql,
                    new { RolId = roleId, ModuloId = moduloId },
                    transaction);
            }
        }

        // NUEVO: Método para contar módulos por sección
        public async Task<int> ContarModulosPorSeccionAsync(int seccionId)
        {
            Console.WriteLine($">>> RepositorioSecciones.ContarModulosPorSeccionAsync - seccionId: {seccionId}");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = "SELECT COUNT(*) FROM [dgmesnie].[Modulo] WHERE [SeccionId] = @SeccionId AND [Activo] = 1";
                Console.WriteLine($">>> SQL: {sql}");

                var count = await connection.QuerySingleAsync<int>(sql, new { SeccionId = seccionId });
                Console.WriteLine($">>> Resultado query: {count}");

                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> Error en ContarModulosPorSeccionAsync: {ex.Message}");
                throw;
            }
        }

        // NUEVO: Método para actualizar orden de sección
        public async Task ActualizarOrdenSeccionAsync(int seccionId, int nuevoOrden)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE [dgmesnie].[Seccion] SET [Orden] = @Orden WHERE [SeccionId] = @Id";
            await connection.ExecuteAsync(sql, new { Id = seccionId, Orden = nuevoOrden });
        }

        public async Task<List<Usuario>> ObtenerUsuariosVigentesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT IdUsuario, Nombre FROM [dgmesnie].[Usuario] WHERE Vigente = 1 ORDER BY Nombre";
            var usuarios = await connection.QueryAsync<Usuario>(sql);
            return usuarios.ToList();
        }
    }
}









