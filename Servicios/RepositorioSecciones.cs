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

        public RepositorioSecciones(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<SeccionConModulos>> ObtenerTodasLasSeccionesAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var secciones = await connection.QueryAsync<SeccionConModulos>("SELECT * FROM Secciones ORDER BY Orden");
            foreach (var seccion in secciones)
            {
                var modulos = await connection.QueryAsync<Modulo>(
                    "SELECT * FROM Modulos WHERE SeccionId = @SeccionId ORDER BY Orden",
                    new { SeccionId = seccion.Id });
                seccion.Modulos = modulos.ToList();
            }

            return secciones.ToList();
        }

        public async Task<SeccionConModulos> ObtenerSeccionPorIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var seccion = await connection.QueryFirstOrDefaultAsync<SeccionConModulos>(
                "SELECT * FROM Secciones WHERE Id = @Id",
                new { Id = id });

            if (seccion != null)
            {
                var modulos = await connection.QueryAsync<Modulo>(
                    "SELECT * FROM Modulos WHERE SeccionId = @SeccionId ORDER BY Orden",
                    new { SeccionId = seccion.Id });
                seccion.Modulos = modulos.ToList();
            }

            return seccion;
        }

        public async Task CrearSeccionAsync(SeccionConModulos seccion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Secciones (Titulo, Articulos, FundamentoLegal, Descripcion, Ayuda, Objetivo, ResponsableNormativo, PublicoObjetivo, Activo, Orden)
                    VALUES (@Titulo, @Articulos, @FundamentoLegal, @Descripcion, @Ayuda, @Objetivo, @ResponsableNormativo, @PublicoObjetivo, @Activo, @Orden)";

            await connection.ExecuteAsync(sql, seccion);
        }

        public async Task ActualizarSeccionAsync(SeccionConModulos seccion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Secciones SET 
                    Titulo = @Titulo,
                    Articulos = @Articulos,
                    FundamentoLegal = @FundamentoLegal,
                    Descripcion = @Descripcion,
                    Ayuda = @Ayuda,
                    Objetivo = @Objetivo,
                    ResponsableNormativo = @ResponsableNormativo,
                    PublicoObjetivo = @PublicoObjetivo,
                    Activo = @Activo,
                    Orden = @Orden
                    WHERE Id = @Id";

            await connection.ExecuteAsync(sql, seccion);
        }

        public async Task<List<ModuloSNIER>> ObtenerModulosPorSeccionAsync(int seccionId)
        {
            Console.WriteLine($">>> ObtenerModulosPorSeccionAsync - SeccionId: {seccionId}");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = @"
                    SELECT * FROM Modulos 
                    WHERE SeccionId = @SeccionId 
                    ORDER BY Orden";

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
                    // Primero eliminar todos los módulos de la sección
                    var sqlModulos = "DELETE FROM Modulos WHERE SeccionId = @SeccionId";
                    var modulosEliminados = await connection.ExecuteAsync(sqlModulos, new { SeccionId = seccionId }, transaction);
                    Console.WriteLine($">>> Módulos eliminados: {modulosEliminados}");

                    // Luego eliminar la sección
                    var sqlSeccion = "DELETE FROM Secciones WHERE Id = @Id";
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

            var sql = @"INSERT INTO Modulos (SeccionId, Title, FundamentoLegalModulo, Roles, NombresRoles, Perfiles, Etapa,
                    JustificacionOrden, AyudaContextual, Controller, Action, [Desc], Img, Btn, ElementosUI, AyudaVista, Orden, Activo)
                    VALUES (@SeccionId, @Title, @FundamentoLegalModulo, @Roles, @NombresRoles, @Perfiles, @Etapa,
                    @JustificacionOrden, @AyudaContextual, @Controller, @Action, @Desc, @Img, @Btn, @ElementosUI, @AyudaVista, @Orden, @Activo)";

            await connection.ExecuteAsync(sql, new
            {
                modulo.SeccionId,
                modulo.Title,
                modulo.FundamentoLegalModulo,
                modulo.Roles,
                modulo.NombresRoles,
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
            });

        }

        public async Task ActualizarModuloAsync(Modulo modulo)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Modulos SET
                    Title = @Title,
                    FundamentoLegalModulo = @FundamentoLegalModulo,
                    Roles = @Roles,
                    NombresRoles = @NombresRoles,
                    Perfiles = @Perfiles,
                    Etapa = @Etapa,
                    JustificacionOrden = @JustificacionOrden,
                    AyudaContextual = @AyudaContextual,
                    Controller = @Controller,
                    Action = @Action,
                   [Desc] = @Desc,
                    Img = @Img,
                    Btn = @Btn,
                    ElementosUI = @ElementosUI,
                    AyudaVista = @AyudaVista,
                    Orden = @Orden,
                    Activo = @Activo
                    WHERE Id = @Id";

            await connection.ExecuteAsync(sql, modulo);
        }

        public async Task EliminarModuloAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("DELETE FROM Modulos WHERE Id = @Id", new { Id = id });
        }


        public async Task<Modulo> ObtenerModuloPorIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Modulos WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Modulo>(sql, new { Id = id });
        }

        public async Task<List<ModulosVista>> ObtenerVistasPorModuloIdAsync(int moduloId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    v.Id as VistaId,
                    v.ModuloId,
                    v.Titulo,
                    v.Controller,
                    v.Action,
                    v.Roles,
                    v.Perfiles,
                    v.Orden,
                    v.Activo as Activa,
                    v.EsExterno,
                    m.Title as ModuloTitle,
                    s.Titulo as SeccionTitle
                FROM Vistas v 
                INNER JOIN Modulos m ON v.ModuloId = m.Id 
                INNER JOIN Secciones s ON m.SeccionId = s.Id
                WHERE v.ModuloId = @ModuloId 
                ORDER BY v.Orden";

            return (await connection.QueryAsync<ModulosVista>(sql, new { ModuloId = moduloId })).ToList();
        }

        public async Task<ModulosVista> ObtenerModuloVistaPorIdAsync(int vistaId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    v.Id as VistaId,
                    v.ModuloId,
                    v.Titulo,
                    v.Controller,
                    v.Action,
                    v.Roles,
                    v.Perfiles,
                    v.Orden,
                    v.Activo as Activa,
                    v.EsExterno,
                    m.Title as ModuloTitle,
                    s.Titulo as SeccionTitle
                FROM Vistas v 
                INNER JOIN Modulos m ON v.ModuloId = m.Id 
                INNER JOIN Secciones s ON m.SeccionId = s.Id
                WHERE v.Id = @VistaId";

            return await connection.QueryFirstOrDefaultAsync<ModulosVista>(sql, new { VistaId = vistaId });
        }

        public async Task CrearModuloVistaAsync(ModulosVista vista)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                INSERT INTO Vistas (ModuloId, Titulo, Controller, Action, Roles, Perfiles, Orden, Activo, EsExterno)
                VALUES (@ModuloId, @Titulo, @Controller, @Action, @Roles, @Perfiles, @Orden, @Activa, @EsExterno)";

            await connection.ExecuteAsync(sql, vista);
        }

        public async Task ActualizarModuloVistaAsync(ModulosVista vista)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE Vistas SET 
                    Titulo = @Titulo,
                    Controller = @Controller,
                    Action = @Action,
                    Roles = @Roles,
                    Perfiles = @Perfiles,
                    Orden = @Orden,
                    Activo = @Activa,
                    EsExterno = @EsExterno
                WHERE Id = @VistaId";

            await connection.ExecuteAsync(sql, vista);
        }

        public async Task EliminarModuloVistaAsync(int vistaId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("DELETE FROM Vistas WHERE Id = @VistaId", new { VistaId = vistaId });
        }

        public async Task<int> ContarVistasPorModuloAsync(int moduloId)
        {
            Console.WriteLine($">>> RepositorioSecciones.ContarVistasPorModuloAsync - moduloId: {moduloId}");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = "SELECT COUNT(*) FROM Vistas WHERE ModuloId = @ModuloId";
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
                    s.Id as SeccionId,
                    s.Titulo as SeccionTitulo,
                    s.Articulos,
                    s.FundamentoLegal,
                    s.Descripcion,
                    s.Ayuda,
                    s.Objetivo,
                    s.ResponsableNormativo,
                    s.PublicoObjetivo,
                    s.Activo as SeccionActiva,
                    ISNULL(s.Orden, 1) as SeccionOrden,
                    
                    -- MÓDULO
                    m.Id as ModuloId,
                    m.SeccionId as ModuloSeccionId,
                    m.Title as ModuloTitle,
                    m.FundamentoLegalModulo,
                    m.Roles as ModuloRoles,
                    m.NombresRoles,
                    m.Perfiles as ModuloPerfiles,
                    m.Etapa,
                    m.JustificacionOrden,
                    m.AyudaContextual,
                    m.Controller as ModuloController,
                    m.Action as ModuloAction,
                    m.[Desc] as ModuloDesc,
                    m.Img,
                    m.Btn,
                    m.ElementosUI,
                    m.AyudaVista,
                    ISNULL(m.Orden, 1) as ModuloOrden,
                    m.Activo as ModuloActivo
                FROM Secciones s
                LEFT JOIN Modulos m ON s.Id = m.SeccionId
                ORDER BY ISNULL(s.Orden, 1), ISNULL(m.Orden, 1)"; // ✅ QUITAMOS WHERE s.Activo = 1

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

        // NUEVO: Método para contar módulos por sección
        public async Task<int> ContarModulosPorSeccionAsync(int seccionId)
        {
            Console.WriteLine($">>> RepositorioSecciones.ContarModulosPorSeccionAsync - seccionId: {seccionId}");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = "SELECT COUNT(*) FROM Modulos WHERE SeccionId = @SeccionId AND Activo = 1";
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
            var sql = "UPDATE Secciones SET Orden = @Orden WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = seccionId, Orden = nuevoOrden });
        }
    }

}









