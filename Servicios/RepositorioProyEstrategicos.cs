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
using System.Diagnostics;

namespace NSIE.Servicios
{
    public interface IRepositorioProyEstrategicos
    {
        Task<IEnumerable<ProyectoEstrategico>> ObtenerTodosProyectosEstrategicos(int? mercadoId = null);
        Task<IEnumerable<Mercado>> ObtenerTodosMercados();
        Task<ProyectoEstrategico> ObtenerProyectoPorId(int id);
        Task ActualizarTramite(TramiteProyectoEstrategico tramite);
        Task AgregarComentario(ComentarioProyectoEstrategico comentario);
        Task AgregarProyecto(ProyectoEstrategico proyecto); // Nuevo método para agregar proyecto
        Task AgregarTramite(TramiteProyectoEstrategico tramite); // Agrega un Tramite a un RPyecto Estratégico


        // Nuevos métodos
        Task<TramiteProyectoEstrategico> ObtenerTramitePorId(int id);
        Task<IEnumerable<ComentarioProyectoEstrategico>> ObtenerComentariosPorTramiteId(int tramiteId);


    }

    //El Select SCOPE IDENTITY devuelve el ID impactado en la BD
    public class RepositorioProyEstrategicos : IRepositorioProyEstrategicos
    {

        //Cadena de Conexión a BD
        private readonly string connectionString; //Variable en la que se coloca el valor del CS por default desde el proveedor de configuraciones
        public RepositorioProyEstrategicos(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task AgregarProyecto(ProyectoEstrategico proyecto)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Obtener el nombre del mercado basado en el Mercado_ID
                var mercadoNombreQuery = "SELECT Mercado_Nombre FROM Mercados WHERE Mercado_ID = @Mercado_ID";
                proyecto.Mercado = await connection.ExecuteScalarAsync<string>(mercadoNombreQuery, new { proyecto.Mercado_ID });

                var query = @"
                    INSERT INTO ProyectosEstrategicos (NombreProyecto, Descripción, Mercado, Mercado_ID, FechaIngreso) 
                    VALUES (@NombreProyecto, @Descripción, @Mercado, @Mercado_ID, @FechaIngreso);
                    SELECT SCOPE_IDENTITY();"; // Devuelve el ID generado

                proyecto.FechaIngreso = DateTime.Now; // Establecer la fecha de ingreso a la fecha actual

                proyecto.IDProyecto = await connection.ExecuteScalarAsync<int>(query, new
                {
                    proyecto.NombreProyecto,
                    proyecto.Descripción,
                    proyecto.Mercado,
                    proyecto.Mercado_ID,
                    proyecto.FechaIngreso
                });
            }
        }


        // Obtiene los Mercados Vigentes
        public async Task<IEnumerable<Mercado>> ObtenerTodosMercados()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT Mercado_ID, Mercado_Nombre FROM Mercados WHERE Mercado_Vigente = 1";
                return await connection.QueryAsync<Mercado>(query);
            }
        }

        public async Task<IEnumerable<ProyectoEstrategico>> ObtenerTodosProyectosEstrategicos(int? mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = mercadoId.HasValue
                    ? @"
                SELECT * FROM ProyectosEstrategicos WHERE Mercado_ID = @Mercado_ID ORDER BY FechaIngreso DESC;
                SELECT * FROM TramitesProyectosEstrategicos WHERE IDProyecto IN (SELECT IDProyecto FROM ProyectosEstrategicos WHERE Mercado_ID = @Mercado_ID);"
                    : @"
                SELECT * FROM ProyectosEstrategicos ORDER BY FechaIngreso DESC;
                SELECT * FROM TramitesProyectosEstrategicos WHERE IDProyecto IN (SELECT IDProyecto FROM ProyectosEstrategicos);";

                using (var multi = await connection.QueryMultipleAsync(query, new { Mercado_ID = mercadoId }))
                {
                    var proyectos = multi.Read<ProyectoEstrategico>().ToList();
                    var tramites = multi.Read<TramiteProyectoEstrategico>().ToList();

                    foreach (var proyecto in proyectos)
                    {
                        proyecto.Tramites = tramites.Where(t => t.IDProyecto == proyecto.IDProyecto).ToList();
                    }

                    return proyectos;
                }
            }
        }

        public async Task<ProyectoEstrategico> ObtenerProyectoPorId(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT * FROM ProyectosEstrategicos WHERE IDProyecto = @IDProyecto;
                      SELECT * FROM TramitesProyectosEstrategicos WHERE IDProyecto = @IDProyecto;
                      SELECT * FROM ComentariosProyectosEstrategicos WHERE IDTramite IN 
                      (SELECT IDTramite FROM TramitesProyectosEstrategicos WHERE IDProyecto = @IDProyecto);";

                using (var multi = await connection.QueryMultipleAsync(query, new { IDProyecto = id }))
                {
                    var proyecto = await multi.ReadFirstOrDefaultAsync<ProyectoEstrategico>();
                    if (proyecto != null)
                    {
                        proyecto.Tramites = (await multi.ReadAsync<TramiteProyectoEstrategico>()).AsList();
                        var comentarios = await multi.ReadAsync<ComentarioProyectoEstrategico>();

                        foreach (var tramite in proyecto.Tramites)
                        {
                            tramite.Comentarios = comentarios
                                .Where(c => c.IDTramite == tramite.IDTramite)
                                .ToList();
                        }
                    }
                    return proyecto;
                }
            }
        }




        //Agrega uno ó más Tramite a un Proyecto
        public async Task AgregarTramite(TramiteProyectoEstrategico tramite)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            INSERT INTO TramitesProyectosEstrategicos 
            (IDProyecto, Ubicación, RazonSocial, Tramite, Municipio, FechaIngreso, NumeroFolio, FechaRevisionJuridica, ComentariosRevisionJuridica, FechaSesionEstimado, ComentarioTramite, Responsables, TramiteIngresado, AnalisisEvaluacion, AutorizadoParaPleno, PermisoOtorgado, Estatus) 
            VALUES 
            (@IDProyecto, @Ubicación, @RazonSocial, @Tramite, @Municipio, @FechaIngreso, @NumeroFolio, @FechaRevisionJuridica, @ComentariosRevisionJuridica, @FechaSesionEstimado, @ComentarioTramite, @Responsables, @TramiteIngresado, @AnalisisEvaluacion, @AutorizadoParaPleno, @PermisoOtorgado, @Estatus)";

                await connection.ExecuteAsync(query, new
                {
                    tramite.IDProyecto,
                    tramite.Ubicación,
                    tramite.RazonSocial,
                    tramite.Tramite,
                    tramite.Municipio,
                    FechaIngreso = tramite.FechaIngreso.ToString("yyyy-MM-dd"),
                    tramite.NumeroFolio,
                    FechaRevisionJuridica = tramite.FechaRevisionJuridica?.ToString("yyyy-MM-dd"), // Nullable DateTime
                    tramite.ComentariosRevisionJuridica,
                    tramite.FechaSesionEstimado,
                    tramite.ComentarioTramite,
                    tramite.Responsables,
                    tramite.TramiteIngresado,
                    tramite.AnalisisEvaluacion,
                    tramite.AutorizadoParaPleno,
                    tramite.PermisoOtorgado,
                    tramite.Estatus
                });
            }
        }







        public async Task ActualizarTramite(TramiteProyectoEstrategico tramite)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"UPDATE TramitesProyectosEstrategicos 
                      SET Ubicación = @Ubicación, RazonSocial = @RazonSocial, Tramite = @Tramite, 
                          Municipio = @Municipio, FechaIngreso = @FechaIngreso, NumeroFolio = @NumeroFolio,
                          FechaRevisionJuridica = @FechaRevisionJuridica, FechaSesionEstimado = @FechaSesionEstimado,
                          ComentarioTramite = @ComentarioTramite, Responsables = @Responsables,
                          TramiteIngresado = @TramiteIngresado, AnalisisEvaluacion = @AnalisisEvaluacion,
                          AutorizadoParaPleno = @AutorizadoParaPleno, PermisoOtorgado = @PermisoOtorgado,
                          Estatus = @Estatus
                      WHERE IDTramite = @IDTramite";

                await connection.ExecuteAsync(query, tramite);
            }
        }




        public async Task AgregarComentario(ComentarioProyectoEstrategico comentario)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"INSERT INTO ComentariosProyectosEstrategicos (IDTramite, ComentarioNuevo, FechaComentario, IDUsuario) 
                      VALUES (@IDTramite, @ComentarioNuevo, @FechaComentario, @IdUsuario)";

                await connection.ExecuteAsync(query, new
                {
                    comentario.IDTramite,
                    comentario.ComentarioNuevo,
                    comentario.FechaComentario,
                    comentario.IdUsuario
                });
            }
        }

        public async Task<TramiteProyectoEstrategico> ObtenerTramitePorId(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM TramitesProyectosEstrategicos WHERE IDTramite = @IDTramite";
                return await connection.QueryFirstOrDefaultAsync<TramiteProyectoEstrategico>(query, new { IDTramite = id });
            }
        }

        public async Task<IEnumerable<ComentarioProyectoEstrategico>> ObtenerComentariosPorTramiteId(int tramiteId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT c.*, u.Nombre AS NombreUsuario 
                            FROM ComentariosProyectosEstrategicos c
                            JOIN Usuario u ON c.IDUsuario = u.IdUsuario
                            WHERE c.IDTramite = @IDTramite";
                return await connection.QueryAsync<ComentarioProyectoEstrategico>(query, new { IDTramite = tramiteId });
            }
        }

    }

}











