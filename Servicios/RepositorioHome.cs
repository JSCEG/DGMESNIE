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
                var query = @"SELECT Id, Titulo, Articulos, FundamentoLegal, 
                             Descripcion, Ayuda, Objetivo, ResponsableNormativo, 
                             PublicoObjetivo FROM Secciones WHERE Activo = 1";
                return (await connection.QueryAsync<SeccionSNIER>(query)).ToList();
            }
        }

        public async Task<List<ModuloSNIER>> ObtenerModulosPorSeccion(int seccionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT * FROM Modulos WHERE SeccionId = @SeccionId AND Activo = 1";
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
                    s.Id, s.Titulo, s.Articulos, s.FundamentoLegal, s.Descripcion, s.Ayuda, s.Objetivo, 
                    s.ResponsableNormativo, s.PublicoObjetivo, s.Activo AS SeccionActiva,
                    m.Id AS ModuloId, m.SeccionId, m.Title, m.FundamentoLegalModulo, m.Roles, m.NombresRoles, 
                    m.Perfiles, m.Etapa, m.JustificacionOrden, m.AyudaContextual, m.Controller, m.Action, 
                    m.[Desc], m.Img, m.Btn, m.ElementosUI, m.AyudaVista, m.Orden, m.Activo AS ModuloActivo
                    FROM Secciones s
                    INNER JOIN Modulos m ON s.Id = m.SeccionId
                    WHERE s.Activo = 1 AND m.Activo = 1
                        AND (
                            m.NombresRoles IS NULL 
                            OR m.NombresRoles = '' 
                            OR ',' + LOWER(m.NombresRoles) + ',' LIKE @RolFiltro
                        )
                    ORDER BY s.Id, m.Orden";

                var lookup = new Dictionary<int, SeccionSNIER>();
                var rolFiltro = " %," + rolUsuario.Trim().ToLower() + ",%";

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
                        seccionEntry.Modulos.Add(modulo);
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
                s.Id, s.Titulo, s.Articulos, s.FundamentoLegal, s.Descripcion, s.Ayuda, s.Objetivo, 
                s.ResponsableNormativo, s.PublicoObjetivo, s.Activo AS SeccionActiva,
                m.Id AS ModuloId, m.SeccionId, m.Title, m.FundamentoLegalModulo, m.Roles, m.NombresRoles, 
                m.Perfiles, m.Etapa, m.JustificacionOrden, m.AyudaContextual, m.Controller, m.Action, 
                m.[Desc], m.Img, m.Btn, m.ElementosUI, m.AyudaVista, m.Orden, m.Activo AS ModuloActivo
            FROM Secciones s
            INNER JOIN Modulos m ON s.Id = m.SeccionId
            WHERE s.Activo = 1 AND m.Activo = 1
            ORDER BY s.Id, m.Orden";

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
                        seccionEntry.Modulos.Add(modulo);
                        return seccionEntry;
                    },
                    splitOn: "ModuloId"
                );

                return lookup.Values.ToList();
            }
        }
    }


}











