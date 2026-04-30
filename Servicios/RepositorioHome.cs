using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios.Interfaces;
using System.Threading.Tasks;

namespace NSIE.Servicios
{
    // public interface IRepositorioHome
    // {
    //     Task<List<SeccionSNIER>> ObtenerSeccionesSNIER();
    //     Task<List<ModuloSNIER>> ObtenerModulosPorSeccion(int seccionId);
    // }

    public class RepositorioHome : IRepositorioHome
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;

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
                [Activa] AS [SeccionActiva],
                [Orden]
            FROM [dgmesnie].[Seccion]";

        private const string SelectModuloCompat = @"
            SELECT
                [ModuloId] AS [ModuloId],
                [SeccionId],
                [Title],
                [FundamentoLegalModulo],
                CAST(NULL AS NVARCHAR(MAX)) AS [Roles],
                CAST(NULL AS NVARCHAR(MAX)) AS [NombresRoles],
                [Perfiles],
                [Etapa],
                [JustificacionOrden],
                [AyudaContextual],
                [Controller],
                [Action],
                [Descripcion] AS [Desc],
                [Imagen] AS [Img],
                [BotonTexto] AS [Btn],
                [ElementosUI],
                [AyudaVista],
                [Orden],
                [Activo] AS [ModuloActivo],
                [EsExterno]
            FROM [dgmesnie].[Modulo]";

        public RepositorioHome(IConfiguration configuration, IWebHostEnvironment env)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _env = env;
        }

        public async Task<List<SeccionSNIER>> ObtenerSeccionesSNIER()
        {
            // if (_env.IsDevelopment())
            // {
            //     var jsonPath = Path.Combine(_env.ContentRootPath, "Insumos", "secciones_snier.json");
            //     var json = await File.ReadAllTextAsync(jsonPath);
            //     return JsonConvert.DeserializeObject<List<SeccionSNIER>>(json);
            // }

            using (var connection = new SqlConnection(_connectionString))
            {
                var query = SelectSeccionCompat + " WHERE [SeccionActiva] = 1 ORDER BY [Orden]";
                return (await connection.QueryAsync<SeccionSNIER>(query)).ToList();
            }
        }

        public async Task<List<ModuloSNIER>> ObtenerModulosPorSeccion(int seccionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = SelectModuloCompat + " WHERE [SeccionId] = @SeccionId AND [ModuloActivo] = 1 ORDER BY [Orden]";
                var modulos = await connection.QueryAsync<ModuloSNIER>(query, new { SeccionId = seccionId });
                return modulos.ToList();
            }
        }

        public async Task<List<SeccionSNIER>> ObtenerSeccionesConModulosPorRol(string rolUsuario)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
               SELECT 
                    s.[SeccionId] AS [Id],
                    s.[Titulo],
                    s.[Articulos],
                    s.[FundamentoLegal],
                    s.[Descripcion],
                    s.[Ayuda],
                    s.[Objetivo],
                    s.[ResponsableNormativo],
                    s.[PublicoObjetivo],
                    s.[Activa] AS [SeccionActiva],
                    s.[Orden],
                    m.[ModuloId] AS [ModuloId],
                    m.[SeccionId],
                    m.[Title],
                    m.[FundamentoLegalModulo],
                    CAST(r.[RolId] AS NVARCHAR(50)) AS [Roles],
                    r.[RolNombre] AS [NombresRoles],
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
                    m.[Activo] AS [ModuloActivo],
                    m.[EsExterno]
                FROM [dgmesnie].[Seccion] s
                INNER JOIN [dgmesnie].[Modulo] m ON s.[SeccionId] = m.[SeccionId]
                INNER JOIN [dgmesnie].[RolModulo] rm ON rm.[ModuloId] = m.[ModuloId] AND rm.[Activa] = 1
                INNER JOIN [dgmesnie].[Rol] r ON r.[RolId] = rm.[RolId] AND r.[RolVigente] = 1
                WHERE s.[Activa] = 1 AND m.[Activo] = 1
                  AND LOWER(r.[RolNombre]) = @RolFiltro
                ORDER BY s.[Orden], m.[Orden]";

                var lookup = new Dictionary<int, SeccionSNIER>();
                var rolFiltro = rolUsuario.Trim().ToLower();

                var result = await connection.QueryAsync<SeccionSNIER, ModuloSNIER, SeccionSNIER>(
                    query,
                    (seccion, modulo) =>
                    {
                        if (!lookup.TryGetValue(seccion.Id, out var seccionEntry))
                        {
                            seccionEntry = seccion;
                            seccionEntry.Modulos = new List<ModuloSNIER>();
                            lookup.Add(seccionEntry.Id, seccionEntry);
                        }
                        if (!seccionEntry.Modulos.Any(m => m.ModuloId == modulo.ModuloId))
                        {
                            seccionEntry.Modulos.Add(modulo);
                        }
                        return seccionEntry;
                    },
                    new { RolFiltro = rolFiltro },
                    splitOn: "ModuloId"
                );

                return lookup.Values.ToList();
            }
        }

        public async Task<List<SeccionSNIER>> ObtenerSeccionesConModulos()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
            SELECT 
                s.[SeccionId] AS [Id],
                s.[Titulo],
                s.[Articulos],
                s.[FundamentoLegal],
                s.[Descripcion],
                s.[Ayuda],
                s.[Objetivo],
                s.[ResponsableNormativo],
                s.[PublicoObjetivo],
                s.[Activa] AS [SeccionActiva],
                s.[Orden],
                m.[ModuloId] AS [ModuloId],
                m.[SeccionId],
                m.[Title],
                m.[FundamentoLegalModulo],
                CAST(NULL AS NVARCHAR(MAX)) AS [Roles],
                CAST(NULL AS NVARCHAR(MAX)) AS [NombresRoles],
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
                m.[Activo] AS [ModuloActivo],
                m.[EsExterno]
            FROM [dgmesnie].[Seccion] s
            INNER JOIN [dgmesnie].[Modulo] m ON s.[SeccionId] = m.[SeccionId]
            WHERE s.[Activa] = 1 AND m.[Activo] = 1
            ORDER BY s.[Orden], m.[Orden]";

                var lookup = new Dictionary<int, SeccionSNIER>();

                var result = await connection.QueryAsync<SeccionSNIER, ModuloSNIER, SeccionSNIER>(
                    query,
                    (seccion, modulo) =>
                    {
                        if (!lookup.TryGetValue(seccion.Id, out var seccionEntry))
                        {
                            seccionEntry = seccion;
                            seccionEntry.Modulos = new List<ModuloSNIER>();
                            lookup.Add(seccionEntry.Id, seccionEntry);
                        }
                        if (!seccionEntry.Modulos.Any(m => m.ModuloId == modulo.ModuloId))
                        {
                            seccionEntry.Modulos.Add(modulo);
                        }
                        return seccionEntry;
                    },
                    splitOn: "ModuloId"
                );

                return lookup.Values.ToList();
            }
        }
    }


}











