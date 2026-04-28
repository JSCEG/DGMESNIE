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
    public interface IRepositorioIndicadores
    {

        //Mecanismo Especial
        // Método para obtener todas las solicitudes
        Task<IEnumerable<LogEvaluacionesCRE>> Obtener_SolicitudesD();

        // Método para actualizar la calificación de una solicitud
        Task ActualizarCalificacion(int id, decimal calificacion, string comentario, string nombreEvaluador, DateTime fechaYHoraEvaluacion);

        //0.-Grafico de Totales por EF en el SEM de Infraestructura
        Task<IEnumerable<PermisosPorEntidad>> ObtenerPermisosPorEntidad();

        //1.-Obtiene los indicadores a nivel nacional cuando se carga la página del Mecanismo
        Task<Indicadores> Obtener_Indicadores(); //Petrolíferos
        Task<Indicadores> Obtener_Indicadores_GLP(); //GLP
        Task<Indicadores> Obtener_Indicadores_Electricidad(); //Electricidad


        //2.-Obtiene los totales de las solicitudes a evaluar y de los permisos autorizados
        Task<IEnumerable<Consulta_SolicitudesE>> ObtenerTotalSolicitudes_EvaluarPetroliferos();//Petrolíferos
        Task<IEnumerable<Consulta_SolicitudesE>> ObtenerTotalSolicitudes_Evaluar_GLP();//Gas LP
        Task<IEnumerable<Consulta_SolicitudesE>> ObtenerTotalSolicitudes_Evaluar_Electricidad();//Electricidad


        //3.-Obtiene las solicitudes a evaluar
        Task<IEnumerable<Consulta_SolicitudesE>> ObtenerSolicitudes_MapaPetroliferos();//Petrolíferos
        Task<IEnumerable<Consulta_SolicitudesE>> ObtenerSolicitudes_Mapa_GLP();//GLP
        Task<IEnumerable<Consulta_SolicitudesE>> ObtenerSolicitudes_Mapa_Electricidad();//Electricidad




        //4.-Evaluaciones anivel Nacional EF
        //Totales de Tarjetas a nivel EF
        Task<IEnumerable<CalificacionFinal>> A_Calificacion_Nacional(CalificacionFinal calificacionFinal);//Petrolíferos
        Task<IEnumerable<CalificacionFinal>> A_Calificacion_Nacional_GLP(CalificacionFinal calificacionFinal);//Gas L.P.
        Task<IEnumerable<CalificacionFinal>> A_Calificacion_Nacional_Electricidad(CalificacionFinal calificacionFinal);//Electricidad

        //Totales Categorías a nivel EF
        Task<IEnumerable<CalificacionFinal>> B_Calificacion_Total(CalificacionFinal calificacionFinal);
        Task<IEnumerable<CalificacionFinal>> B_Calificacion_Total_GLP(CalificacionFinal calificacionFinal);
        Task<IEnumerable<CalificacionFinal>> B_Calificacion_Total_Electricidad(CalificacionFinal calificacionFinal);
        //Tabla y Grafico a Neivel EF
        Task<IEnumerable<CalificacionFinal>> D_Calificacion_por_EF(CalificacionFinal calificacionFinal);
        Task<IEnumerable<CalificacionFinal>> D_Calificacion_por_EF_GLP(CalificacionFinal calificacionFinal);
        Task<IEnumerable<CalificacionFinal>> D_Calificacion_por_EF_Electricidad(CalificacionFinal calificacionFinal);

        //5.-Evaluación a Nivel Municipal
        Task<IEnumerable<ResultadoMunicipios>> J_Salida_EFMUN_Aprobados_NAL(ResultadoMunicipios resultadoMunicipio);
        Task<IEnumerable<ResultadoMunicipios>> J_Salida_EFMUN_Aprobados_NAL_GLP(ResultadoMunicipios resultadoMunicipio);
        Task<IEnumerable<ResultadoMunicipios>> J_Salida_EFMUN_Aprobados_NAL_Electricidad(ResultadoMunicipios resultadoMunicipio);


        //6.-Colores de Los Mapas MunicipalesEvaluados
        Task<List<Color>> Obtener_Indicadores_Colores();
        Task<List<Color>> Obtener_Indicadores_Colores_GLP();
        Task<List<Color>> Obtener_Indicadores_Colores_Electricidad();


        //7.-Vista Detalle Resultado a Nivel EF
        Task<DetalleCalificacion_EF> D_Detalle_Calificacion_por_EF(CalificacionFinal calificacionFinal);
        Task<DetalleCalificacion_EF> D_Detalle_Calificacion_por_EF_GLP(CalificacionFinal calificacionFinal);
        Task<DetalleCalificacion_EF> D_Detalle_Calificacion_por_EF_Electricidad(CalificacionFinal calificacionFinal);



        //8.-Campos Visibles de los Mapas y las Solcicitudes
        //Consultas Mapa /Publicas e Internas
        Task<IEnumerable<ExpendioAutorizado>> ObtenerExpendiosAutorizados();
        Task<List<string>> ObtenerCamposVisibles(int rolId, int mercadoId);
        Task<IEnumerable<ExpendioAutorizadoGLP>> ObtenerExpendiosAutorizadosGLP();
        //Mapas de Infraestructura
        Task<IEnumerable<ExpendioAutorizadoGLP>> ObtenerExpendiosAutorizadosGLP_Infra();
        Task<IEnumerable<ExpendioAutorizadoGN>> ObtenerExpendiosAutorizadosGN_Infra();

        Task<List<string>> ObtenerCamposVisiblesGLP_Infra(int rolId, int mercadoId);
        Task<IEnumerable<ExpendioAutorizado>> ObtenerExpendiosAutorizadosPET_Infra();

        Task<List<string>> ObtenerCamposVisiblesPET_Infra(int rolId, int mercadoId);
        //Gas Natural
        Task<List<string>> ObtenerCamposVisiblesGN_Infra(int rolId, int mercadoId);



        Task<List<string>> ObtenerCamposVisiblesGLP(int rolId, int mercadoId);

        //RyS
        Task<List<string>> ObtenerCamposVisiblesRyS_Infra(int rolId, int mercadoId);
        Task<IEnumerable<ExpendioAutorizadoRyS_Infra>> ObtenerExpendiosAutorizadosRyS_Infra();


        //CG
        Task<List<string>> ObtenerCamposVisiblesCG_Infra(int rolId, int mercadoId);
        Task<IEnumerable<ExpendioAutorizadoCG_Infra>> ObtenerExpendiosAutorizadosCG_Infra();

        //PI
        Task<List<string>> ObtenerCamposVisiblesPI_Infra(int rolId, int mercadoId);
        Task<IEnumerable<ExpendioAutorizadoPI_Infra>> ObtenerExpendiosAutorizadosPI_Infra();
        //Electricidad
        Task<List<string>> ObtenerCamposVisiblesElectricidad_Infra(int rolId, int mercadoId);
        Task<IEnumerable<ExpendioAutorizadoElectricidad>> ObtenerExpendiosAutorizadosElectricidad();
        Task<List<string>> ObtenerCamposVisiblesElectricidad(int rolId, int mercadoId);
        Task<IEnumerable<ExpendioAutorizadoElectricidad_Infra>> ObtenerExpendiosAutorizadosElectricidad_Infra();

        //9.-Obtiene los totales de los grupos de ineteres "GIE" de los permisos autorizados y solicitados
        Task<IEnumerable<Consulta_GIE>> ObtenerGIE_Petroliferos();//Petrolíferos
        Task<IEnumerable<Consulta_GIE>> ObtenerGIE_Petroliferos_A();//Petrolíferos
        Task<IEnumerable<Consulta_GIE>> ObtenerGIE_Petroliferos_S();//Petrolíferos


        Task<IEnumerable<Consulta_GIE>> ObtenerGIE_GLP();//GLP       
        Task<IEnumerable<Consulta_GIE>> ObtenerGIE_GLP_A();//GLP     
        Task<IEnumerable<Consulta_GIE>> ObtenerGIE_GLP_S();//GLP

        //9.1-Detalle del GIE al hacer Click , búsqueda por Razón Social
        Task<(List<Consulta_GIExRazonSocial> Permisos, List<Consulta_GIExRazonSocial> Solicitudes)> ObtenerGIEPorRazonSocial(string razonSocial);
        Task<(List<ExpendioAutorizadoGLP> Permisos, List<Consulta_GIExRazonSocial> Solicitudes)> ObtenerGIEPorRazonSocialGLP(string razonSocial);

        //10.-3KM    
        Task<Consulta_SolicitudesE> ObtenerSolicitudPorId(int Id);
        Task<Consulta_SolicitudesE> ObtenerSolicitudPorIdGLP(int Id);




        //11.-Precios de GLP
        Task<IEnumerable<PreciosGLP>> ObtenerPreciosGLP(int year, int month, int week, string priceType);





        Task Crear(Indicador2_Clase indicador2_Clase);
        Task<bool> Existe(string nombre, string email);
        Task<Expendios> ObtenerExpendioPorNumeroPermiso(string NumeroPermiso);

        //Tablero de Permisos de Electricidad
        Task<IEnumerable<DatosDashboard_PE>> ObtenerDatosPorPermisoYAnio(string numeroPermiso);
        Task<IEnumerable<Expendios>> ObtenerExpendios();
        //Task<IEnumerable<Coordenadasx>> ObtenerMapa();
        Task<IEnumerable<Indicador1_Clase>> Obtener_I1();
        Task<IEnumerable<Indicador2_Clase>> Obtener_I2();
        Task<IEnumerable<Indicador3_Clase>> Obtener_I3();
        Task<IEnumerable<Indicador4_Clase>> Obtener_I4();

        //Metodo salidas por EF

        // Municipios
        Task<IEnumerable<CalificacionFinal>> C_Calificacion_por_MUN(CalificacionFinal calificacionFinal);
        Task<IEnumerable<CalificacionFinal>> B_Calificacion_Total_por_MUN(CalificacionFinal calificacionFinal);
        Task<DetalleCalificacion_EF> Detalle_MUN(CalificacionFinal calificacionFinal);



        //Pre-Calificación de Solcitud Pública
        Task<IEnumerable<CalificacionFinal>> PRE_Calificacion_Solicitud_Pública(CalificacionFinal calificacionFinal);

        Task devuelveBitacoraRegistro(LogEvaluaciones datos);
        Task devuelveBitacoraRegistroCRE(LogEvaluacionesCRE datos);
        Task<Isocrona> GetIsocronaTotales(Isocrona isocrona);



        ///
        Task<int> InsertarSolicitudesBulk(List<CargaSolicitudExpendio> solicitudes);
        Task<IEnumerable<ExpendioAutorizado>> ObtenerExpendiosAutorizadosPET_Infra_SP(ExpendioAutorizado expendioAutorizado);

        //Obtiene el Tablero de Electricidad


    }

    //El Select SCOPE IDENTITY devuelve el ID impactado en la BD
    public class RepositorioIndicadores : IRepositorioIndicadores
    {

        //Cadena de Conexión a BD
        private readonly string connectionString; //Variable en la que se coloca el valor del CS por default desde el proveedor de configuraciones
        public RepositorioIndicadores(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        //LogEvaluaciones CRE
        public async Task<IEnumerable<LogEvaluacionesCRE>> Obtener_SolicitudesD()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT  [IdLog], [IdUsuario], [FechaYHora], [Mercado], [Año], [Pregunta1], [Pregunta2], [Pregunta3], " +
                            "[Pregunta4], [Pregunta5], [Pregunta6], [Pregunta7], [Entidad_Federativa], [Municipio], [Coordenadas], [Evaluacion], " +
                            "[Turno], [NumerodeExpediente], [FechaYHoraED], [NombreEvaluaED], [CalificacionED], [ComentariosED] " +
                            "FROM [dbo].[LogEvaluacionesCRE]";
                return await connection.QueryAsync<LogEvaluacionesCRE>(query);
            }
        }

        public async Task ActualizarCalificacion(int id, decimal calificacion, string comentario, string nombreEvaluador, DateTime fechaYHoraEvaluacion)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "UPDATE [dbo].[LogEvaluacionesCRE] SET CalificacionED = @Calificacion, ComentariosED = @Comentario, " +
                            "NombreEvaluaED = @NombreEvaluador, FechaYHoraED = @FechaYHoraEvaluacion WHERE IdLog = @Id";
                await connection.ExecuteAsync(query, new
                {
                    Id = id,
                    Calificacion = calificacion,
                    Comentario = comentario,
                    NombreEvaluador = nombreEvaluador,
                    FechaYHoraEvaluacion = fechaYHoraEvaluacion
                });
            }
        }







        //0.-Grafico totale sInfraestructuras
        public async Task<IEnumerable<PermisosPorEntidad>> ObtenerPermisosPorEntidad()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                SELECT 
                    EF.EF_Nombre AS EfNombre,
                    ISNULL(EP.TotalPermisos, 0) AS TotalPetroliferos,
                    ISNULL(EG.TotalPermisos, 0) AS TotalElectricidad,
                    ISNULL(GN.TotalPermisos, 0) AS TotalGasNatural,
                    ISNULL(GLP.TotalPermisos, 0) AS TotalGasLP
                FROM [dbo].[Entidades_Federativas] AS EF
                LEFT JOIN (
                    SELECT 
                        E.EfId,
                        COUNT(E.NumeroPermiso) AS TotalPermisos
                    FROM [dbo].[vExpendios_autorizado_mapa] AS E
                    GROUP BY E.EfId
                ) AS EP ON EF.EF_ID = EP.EfId
                LEFT JOIN (
                    SELECT 
                        SUBSTRING(G.EfId, CHARINDEX(' ', G.EfId) + 1, LEN(G.EfId)) AS EfId,
                        COUNT(G.NumeroPermiso) AS TotalPermisos
                    FROM [dbo].[vElectricidad_autorizado_mapa] AS G
                    GROUP BY SUBSTRING(G.EfId, CHARINDEX(' ', G.EfId) + 1, LEN(G.EfId))
                ) AS EG ON EF.EF_Nombre = EG.EfId
                LEFT JOIN (
                    SELECT 
                        SUBSTRING(GN.EfId, CHARINDEX('-', GN.EfId) + 1, LEN(GN.EfId)) AS EfId,
                        COUNT(GN.NumeroPermiso) AS TotalPermisos
                    FROM [dbo].[vGasNatural_autorizado_mapa] AS GN
                    GROUP BY SUBSTRING(GN.EfId, CHARINDEX('-', GN.EfId) + 1, LEN(GN.EfId))
                ) AS GN ON EF.EF_Nombre = GN.EfId
                LEFT JOIN (
                    SELECT 
                        G.EfId,
                        COUNT(G.NumeroPermiso) AS TotalPermisos
                    FROM [dbo].[vGasLP_autorizado_mapa] AS G
                    GROUP BY G.EfId
                ) AS GLP ON EF.EF_ID = GLP.EfId
                ORDER BY EF.EF_Nombre ASC";

                return await connection.QueryAsync<PermisosPorEntidad>(query);
            }
        }


        //1.-Metodo para obtener los indicadores en el Mecanismo de Evaluación
        //Petríferos
        public async Task<Indicadores> Obtener_Indicadores()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = @"
									EXEC [dbo].[spObtener_I1];
									EXEC [dbo].[spObtener_I2];
									EXEC [dbo].[spObtener_I3];
									EXEC [dbo].[spObtener_I4];
									EXEC [dbo].[spObtener_I5];
									EXEC [dbo].[spObtener_I6];
									EXEC [dbo].[spObtener_I7];
									EXEC [dbo].[spObtener_I8];
									EXEC [dbo].[spObtener_I9];
									EXEC [dbo].[spObtener_I10];
									EXEC [dbo].[spObtener_I11];
									EXEC [dbo].[spObtener_I12];    
								";

                    await connection.OpenAsync();

                    using (var multi = await connection.QueryMultipleAsync(query))
                    {
                        var datosIndicadores = new Indicadores
                        {
                            I1 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I2 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I3 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I4 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I5 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I6 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I7 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I8 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I9 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I10 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I11 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I12 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false)

                        };

                        return datosIndicadores;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejar la excepción de SQL aquí
                Console.WriteLine("Error de SQL: " + ex.Message);
                // Puedes lanzar una excepción personalizada, realizar alguna acción de manejo de errores o retornar un valor predeterminado
                throw;
            }
        }
        //GLP
        public async Task<Indicadores> Obtener_Indicadores_GLP()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = @"
									EXEC [dbo].[spObtener_I1_GLP];
									EXEC [dbo].[spObtener_I2_GLP];
									EXEC [dbo].[spObtener_I3_GLP];
									EXEC [dbo].[spObtener_I4_GLP];
									EXEC [dbo].[spObtener_I5_GLP];
									EXEC [dbo].[spObtener_I6_GLP];
									EXEC [dbo].[spObtener_I7_GLP];
									EXEC [dbo].[spObtener_I8_GLP];
									EXEC [dbo].[spObtener_I9_GLP];
									EXEC [dbo].[spObtener_I10_GLP];
									EXEC [dbo].[spObtener_I11_GLP];
									EXEC [dbo].[spObtener_I12_GLP];
									EXEC [dbo].[spObtener_I13_GLP];
									EXEC [dbo].[spObtener_I14_GLP];
									
								";

                    await connection.OpenAsync();

                    using (var multi = await connection.QueryMultipleAsync(query))
                    {
                        var datosIndicadores = new Indicadores
                        {
                            I1 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I2 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I3 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I4 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I5 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I6 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I7 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I8 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I9 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I10 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I11 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I12 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I13 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I14 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false)
                            //I15 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false)
                        };

                        return datosIndicadores;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejar la excepción de SQL aquí
                Console.WriteLine("Error de SQL: " + ex.Message);
                // Puedes lanzar una excepción personalizada, realizar alguna acción de manejo de errores o retornar un valor predeterminado
                throw;
            }
        }
        //Electricidad
        public async Task<Indicadores> Obtener_Indicadores_Electricidad()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = @"
									EXEC [dbo].[spObtener_I1_Electricidad];
									EXEC [dbo].[spObtener_I2_Electricidad];
									EXEC [dbo].[spObtener_I3_Electricidad];
									EXEC [dbo].[spObtener_I4_Electricidad];
									EXEC [dbo].[spObtener_I5_Electricidad];
									EXEC [dbo].[spObtener_I6_Electricidad];
									EXEC [dbo].[spObtener_I7_Electricidad];
									EXEC [dbo].[spObtener_I8_Electricidad];
									EXEC [dbo].[spObtener_I9_Electricidad];
									EXEC [dbo].[spObtener_I10_Electricidad];
									EXEC [dbo].[spObtener_I11_Electricidad];
									EXEC [dbo].[spObtener_I12_Electricidad];
									EXEC [dbo].[spObtener_I13_Electricidad];
									EXEC [dbo].[spObtener_I14_Electricidad];
									
								";

                    await connection.OpenAsync();

                    using (var multi = await connection.QueryMultipleAsync(query))
                    {
                        var datosIndicadores = new Indicadores
                        {
                            I1 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I2 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I3 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I4 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I5 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I6 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I7 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I8 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I9 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I10 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I11 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I12 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I13 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false),
                            I14 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false)
                            //I15 = await multi.ReadAsync<Indicadores>().ConfigureAwait(false)
                        };

                        return datosIndicadores;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejar la excepción de SQL aquí
                Console.WriteLine("Error de SQL: " + ex.Message);
                // Puedes lanzar una excepción personalizada, realizar alguna acción de manejo de errores o retornar un valor predeterminado
                throw;
            }
        }


        //2.-Total de las Solicitudes a Evaluar y expendios
        //Petríferos
        public async Task<IEnumerable<Consulta_SolicitudesE>> ObtenerTotalSolicitudes_EvaluarPetroliferos()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<Consulta_SolicitudesE>(
                    @"

						SELECT top 1 (SELECT  count(*)  FROM [dbo].[Solicitudes_Expendio]) as Total_de_Solicitudes_a_Evaluar
						,(SELECT Count(*)  FROM [dbo].[Calificacion_Solicitudes_Petroleo_EF] where [Documentos_completos] =1) as [Con_Documentos_Completos]
						,(SELECT Count(*)  FROM [dbo].[Calificacion_Solicitudes_Petroleo_EF] where [Analisis_riesgo] =1) as [Con_Analisis_de_Riesgo]
						,(SELECT count(*)  FROM [dbo].[Expendios]) as Total_de_Permisos_de_Expendio,
                         (SELECT MAX([FechaOtorgamiento]) FROM [dbo].[Expendios_autorizado]) AS Fecha_Actualizacion
					  FROM [dbo].[Calificacion_Solicitudes_Petroleo_EF]

					");

                return coordenadas;
            }
        }
        //GLP
        public async Task<IEnumerable<Consulta_SolicitudesE>> ObtenerTotalSolicitudes_Evaluar_GLP()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<Consulta_SolicitudesE>(
                    @"

						SELECT top 1 (SELECT  count(*)  FROM [dbo].[Solicitudes_Expendios_GLP]) as Total_de_Solicitudes_a_Evaluar
						,(SELECT Count(*)  FROM [dbo].[Calificacion_Solicitudes_GASLP_EF] where [Documentos_completos] =1) as [Con_Documentos_Completos]
						,(SELECT Count(*)  FROM [dbo].[Calificacion_Solicitudes_GASLP_EF] where [Analisis_riesgo] =1) as [Con_Analisis_de_Riesgo]
						,(SELECT count(*)  FROM [dbo].[vGasLP_autorizado]) as Total_de_Permisos_de_Expendio,
                        (SELECT MAX([FechaOtorgamiento]) FROM [dbo].[GasLP_autorizado]) AS Fecha_Actualizacion
					  FROM [dbo].[Calificacion_Solicitudes_GASLP_EF]

					");

                return coordenadas;
            }
        }
        //Electricidad
        public async Task<IEnumerable<Consulta_SolicitudesE>> ObtenerTotalSolicitudes_Evaluar_Electricidad()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<Consulta_SolicitudesE>(
                    @"

						SELECT top 1 (SELECT  count(*)  FROM [dbo].[Solicitudes_Expendio_Electricidad]) as Total_de_Solicitudes_a_Evaluar
						,(SELECT Count(*)  FROM [dbo].[Calificacion_Solicitudes_Electricidad_EF] where [Documentos_completos] =1) as [Con_Documentos_Completos]
						,(SELECT Count(*)  FROM [dbo].[Calificacion_Solicitudes_Electricidad_EF] where [Analisis_riesgo] =1) as [Con_Analisis_de_Riesgo]
						,(SELECT count(*)  FROM [dbo].[vElectricidad_autorizado]) as Total_de_Permisos_de_Expendio

					  FROM [dbo].[Calificacion_Solicitudes_Electricidad_EF]

					");

                return coordenadas;
            }
        }

        //3.-Obtiene las solicitudes a evaluar
        //Petríferos
        public async Task<IEnumerable<Consulta_SolicitudesE>> ObtenerSolicitudes_MapaPetroliferos()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<Consulta_SolicitudesE>(
                    @"
					SELECT  [ID]
					  ,[EF_ID]
					  ,[EF_Nombre]
					  ,[MPO_ID]
	 				  ,[Municipio_Nombre]
    				  ,Case [Documentos_completos]
						when 1 then 'SI'
						else 'NO' END as [Documentos_completos]
					  ,Case [Analisis_riesgo]
						when 1 then 'SI'
						else 'NO' END as [Analisis_riesgo]
					  ,[X_Geo] as Latitud_GEO
					  ,[Y_Geo] as Longitud_GEO
	    			  ,Turno
		 			  ,[Marca_solicitada]
					  ,[Expediente]
					  ,[Razon_social]
				       FROM [dbo].[Calificacion_Solicitudes_Petroleo_EF]

					");

                return coordenadas;
            }
        }
        //GLP
        public async Task<IEnumerable<Consulta_SolicitudesE>> ObtenerSolicitudes_Mapa_GLP()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<Consulta_SolicitudesE>(
                    @"
					SELECT  [ID]
					  ,[EF_ID]
					  ,[EF_Nombre]
					  ,[MPO_ID]
	 				  ,[Municipio_Nombre]
    				  ,Case [Documentos_completos]
						when 1 then 'SI'
						else 'NO' END as [Documentos_completos]
					  ,Case [Analisis_riesgo]
						when 1 then 'SI'
						else 'NO' END as [Analisis_riesgo]
					  ,[X_Geo] as Latitud_GEO
					  ,[Y_Geo] as Longitud_GEO
	    			  ,Turno
		 			  ,[Marca_solicitada]
					  ,[Expediente]
					  ,[Razon_social]
				       FROM [dbo].[Calificacion_Solicitudes_GASLP_EF]

					");

                return coordenadas;
            }
        }
        //Electricidad
        public async Task<IEnumerable<Consulta_SolicitudesE>> ObtenerSolicitudes_Mapa_Electricidad()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<Consulta_SolicitudesE>(
                    @"
					SELECT  [ID]
					  ,[EF_ID]
					  ,[EF_Nombre]
					  ,[MPO_ID]
	 				  ,[Municipio_Nombre]
    				  ,Case [Documentos_completos]
						when 1 then 'SI'
						else 'NO' END as [Documentos_completos]
					  ,Case [Analisis_riesgo]
						when 1 then 'SI'
						else 'NO' END as [Analisis_riesgo]
					  ,[X_Geo] as Latitud_GEO
					  ,[Y_Geo] as Longitud_GEO
	    			  ,Turno
		 			  ,[Marca_solicitada]
					  ,[Expediente]
					  ,[Razon_social]
				       FROM [dbo].[Calificacion_Solicitudes_Electricidad_EF]

					");

                return coordenadas;
            }
        }


        //4.-Evualuacionesa Nivel EF
        //4.1- Tarjeta de Totales
        //Petrolíferos 
        public async Task<IEnumerable<CalificacionFinal>> A_Calificacion_Nacional(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Se implementa aquí la lógica para obtener los parámetros de los checkboxes
            // y el umbral y pasarlos al stored procedure

            // Se obtienen los nombres de los checkboxes seleccionados
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;

            // Se crea una cadena de texto con los nombres de los checkboxes seleccionados
            var checkboxValuesString = string.Join(",", checkboxValues);

            //Se crea un parámetro para el umbral
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);

            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
				EXEC [dbo].[E_Total_Nacional] @CheckboxValues, @Umbral",
                new { CheckboxValues = checkboxValuesString, Umbral = calificacionFinal.Umbral_Seleccionado });

            return detalle;
        }
        //GLP
        public async Task<IEnumerable<CalificacionFinal>> A_Calificacion_Nacional_GLP(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Se implementa aquí la lógica para obtener los parámetros de los checkboxes
            // y el umbral y pasarlos al stored procedure

            // Se obtienen los nombres de los checkboxes seleccionados
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;

            // Se crea una cadena de texto con los nombres de los checkboxes seleccionados
            var checkboxValuesString = string.Join(",", checkboxValues);

            //Se crea un parámetro para el umbral
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);

            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
				EXEC [dbo].[Tarjeta_Total_Nacional_GLP] @CheckboxValues, @Umbral",
                new { CheckboxValues = checkboxValuesString, Umbral = calificacionFinal.Umbral_Seleccionado });

            return detalle;
        }

        //Electricidad
        public async Task<IEnumerable<CalificacionFinal>> A_Calificacion_Nacional_Electricidad(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Se implementa aquí la lógica para obtener los parámetros de los checkboxes
            // y el umbral y pasarlos al stored procedure

            // Se obtienen los nombres de los checkboxes seleccionados
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;

            // Se crea una cadena de texto con los nombres de los checkboxes seleccionados
            var checkboxValuesString = string.Join(",", checkboxValues);

            //Se crea un parámetro para el umbral
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);

            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
				EXEC [dbo].[Tarjeta_Total_Nacional_Electricidad] @CheckboxValues, @Umbral",
                new { CheckboxValues = checkboxValuesString, Umbral = calificacionFinal.Umbral_Seleccionado });

            return detalle;
        }

        //4.2-Categorías Totales a EF
        //Petrolíferos
        public async Task<IEnumerable<CalificacionFinal>> B_Calificacion_Total(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de los checkboxes
            // y el umbral y pasarlos al stored procedure

            // Ejemplo: obtén los nombres de los checkboxes seleccionados
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;

            // Ejemplo: crea una cadena de texto con los nombres de los checkboxes seleccionados
            var checkboxValuesString = string.Join(",", checkboxValues);

            // Ejemplo: crea un parámetro para el umbral
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);

            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
            EXEC B_Calificacion_Total @CheckboxValues, @Umbral",
                new { CheckboxValues = checkboxValuesString, Umbral = calificacionFinal.Umbral_Seleccionado });

            return detalle;
        }
        //GLP
        public async Task<IEnumerable<CalificacionFinal>> B_Calificacion_Total_GLP(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de los checkboxes
            // y el umbral y pasarlos al stored procedure

            // Ejemplo: obtén los nombres de los checkboxes seleccionados
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;

            // Ejemplo: crea una cadena de texto con los nombres de los checkboxes seleccionados
            var checkboxValuesString = string.Join(",", checkboxValues);

            // Ejemplo: crea un parámetro para el umbral
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);

            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
				EXEC B_Categorias_Total_EF @CheckboxValues, @Umbral",
                new { CheckboxValues = checkboxValuesString, Umbral = calificacionFinal.Umbral_Seleccionado });

            return detalle;
        }
        //Electricidad
        public async Task<IEnumerable<CalificacionFinal>> B_Calificacion_Total_Electricidad(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de los checkboxes
            // y el umbral y pasarlos al stored procedure

            // Ejemplo: obtén los nombres de los checkboxes seleccionados
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;

            // Ejemplo: crea una cadena de texto con los nombres de los checkboxes seleccionados
            var checkboxValuesString = string.Join(",", checkboxValues);

            // Ejemplo: crea un parámetro para el umbral
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);

            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
				EXEC B_Categorias_Total_EF_Electricidad @CheckboxValues, @Umbral",
                new { CheckboxValues = checkboxValuesString, Umbral = calificacionFinal.Umbral_Seleccionado });

            return detalle;
        }




        //4.3-Grafico y Tablas de Resultados a Nivel EF
        //Petrolíferos
        public async Task<IEnumerable<CalificacionFinal>> D_Calificacion_por_EF(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de los checkboxes
            // y el umbral y pasarlos al stored procedure

            // Ejemplo: obtén los nombres de los checkboxes seleccionados
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;

            // Ejemplo: crea una cadena de texto con los nombres de los checkboxes seleccionados
            var checkboxValuesString = string.Join(",", checkboxValues);

            // Ejemplo: crea un parámetro para el umbral
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);

            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
					 EXEC D_Calificacion_por_EF @CheckboxValues, @Umbral",
                new { CheckboxValues = checkboxValuesString, Umbral = calificacionFinal.Umbral_Seleccionado });

            return detalle;
        }
        //GLP
        public async Task<IEnumerable<CalificacionFinal>> D_Calificacion_por_EF_GLP(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de los checkboxes
            // y el umbral y pasarlos al stored procedure

            // Ejemplo: obtén los nombres de los checkboxes seleccionados
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;

            // Ejemplo: crea una cadena de texto con los nombres de los checkboxes seleccionados
            var checkboxValuesString = string.Join(",", checkboxValues);

            // Ejemplo: crea un parámetro para el umbral
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);

            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
					 EXEC [D_TablayGrafico_por_EF_GLP] @CheckboxValues, @Umbral",
                new { CheckboxValues = checkboxValuesString, Umbral = calificacionFinal.Umbral_Seleccionado });

            return detalle;
        }
        //Electricidad
        public async Task<IEnumerable<CalificacionFinal>> D_Calificacion_por_EF_Electricidad(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de los checkboxes
            // y el umbral y pasarlos al stored procedure

            // Ejemplo: obtén los nombres de los checkboxes seleccionados
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;

            // Ejemplo: crea una cadena de texto con los nombres de los checkboxes seleccionados
            var checkboxValuesString = string.Join(",", checkboxValues);

            // Ejemplo: crea un parámetro para el umbral
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);

            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
					 EXEC [D_TablayGrafico_por_EF_Electricidad] @CheckboxValues, @Umbral",
                new { CheckboxValues = checkboxValuesString, Umbral = calificacionFinal.Umbral_Seleccionado });

            return detalle;
        }



        //5.-Evaluación a Nivel Municipal (Botón Ejecutar)
        //Petrolíferos
        public async Task<IEnumerable<ResultadoMunicipios>> J_Salida_EFMUN_Aprobados_NAL(ResultadoMunicipios resultadoMunicipio)

        {
            using var connection = new SqlConnection(connectionString);

            //Los valores sellaman igual desde el AJAX

            // Ejemplo: obtén los nombres de los checkboxes seleccionados
            var Indicadores_Seleccionados_Nal = resultadoMunicipio.Indicadores_Seleccionados_Nal;
            var Indicadores_Seleccionados_NalString = string.Join(",", Indicadores_Seleccionados_Nal);
            var Indicadores_Seleccionados_Mun = resultadoMunicipio.Indicadores_Seleccionados_Mun;
            var Indicadores_Seleccionados_MunString = string.Join(",", Indicadores_Seleccionados_Mun);
            var Umbral = new SqlParameter("@Umbral", resultadoMunicipio.Umbral_Seleccionado_Nal);
            var UmbralMunicipio = new SqlParameter("@UmbralMunicipio", resultadoMunicipio.Umbral_Seleccionado_Mun);

            var detalle = await connection.QueryAsync<ResultadoMunicipios>(
                         @"
					 EXEC J_Salida_EFMUN_Aprobados_NAL_Detalle @Indicadores_Seleccionados_Nal, @Umbral,@Indicadores_Seleccionados_Mun, @UmbralMunicipio

				",
                new
                {

                    Indicadores_Seleccionados_Nal = Indicadores_Seleccionados_NalString,
                    Umbral = resultadoMunicipio.Umbral_Seleccionado_Nal,
                    Indicadores_Seleccionados_Mun = Indicadores_Seleccionados_MunString,
                    UmbralMunicipio = resultadoMunicipio.Umbral_Seleccionado_Mun

                });

            return detalle;

        }
        //GLP
        public async Task<IEnumerable<ResultadoMunicipios>> J_Salida_EFMUN_Aprobados_NAL_GLP(ResultadoMunicipios resultadoMunicipio)

        {
            using var connection = new SqlConnection(connectionString);

            //Los valores sellaman igual desde el AJAX

            // Ejemplo: obtén los nombres de los checkboxes seleccionados
            var Indicadores_Seleccionados_Nal = resultadoMunicipio.Indicadores_Seleccionados_Nal;
            var Indicadores_Seleccionados_NalString = string.Join(",", Indicadores_Seleccionados_Nal);
            var Indicadores_Seleccionados_Mun = resultadoMunicipio.Indicadores_Seleccionados_Mun;
            var Indicadores_Seleccionados_MunString = string.Join(",", Indicadores_Seleccionados_Mun);
            var Umbral = new SqlParameter("@Umbral", resultadoMunicipio.Umbral_Seleccionado_Nal);
            var UmbralMunicipio = new SqlParameter("@UmbralMunicipio", resultadoMunicipio.Umbral_Seleccionado_Mun);

            var detalle = await connection.QueryAsync<ResultadoMunicipios>(
                         @"
					      EXEC J_Salida_EFMUN_Aprobados_NAL_Detalle_GLP @Indicadores_Seleccionados_Nal, @Umbral,@Indicadores_Seleccionados_Mun, @UmbralMunicipio
				",
                new
                {

                    Indicadores_Seleccionados_Nal = Indicadores_Seleccionados_NalString,
                    Umbral = resultadoMunicipio.Umbral_Seleccionado_Nal,
                    Indicadores_Seleccionados_Mun = Indicadores_Seleccionados_MunString,
                    UmbralMunicipio = resultadoMunicipio.Umbral_Seleccionado_Mun

                });

            return detalle;

        }
        //Electricidad
        public async Task<IEnumerable<ResultadoMunicipios>> J_Salida_EFMUN_Aprobados_NAL_Electricidad(ResultadoMunicipios resultadoMunicipio)

        {
            using var connection = new SqlConnection(connectionString);

            //Los valores sellaman igual desde el AJAX

            // Ejemplo: obtén los nombres de los checkboxes seleccionados
            var Indicadores_Seleccionados_Nal = resultadoMunicipio.Indicadores_Seleccionados_Nal;
            var Indicadores_Seleccionados_NalString = string.Join(",", Indicadores_Seleccionados_Nal);
            var Indicadores_Seleccionados_Mun = resultadoMunicipio.Indicadores_Seleccionados_Mun;
            var Indicadores_Seleccionados_MunString = string.Join(",", Indicadores_Seleccionados_Mun);
            var Umbral = new SqlParameter("@Umbral", resultadoMunicipio.Umbral_Seleccionado_Nal);
            var UmbralMunicipio = new SqlParameter("@UmbralMunicipio", resultadoMunicipio.Umbral_Seleccionado_Mun);

            var detalle = await connection.QueryAsync<ResultadoMunicipios>(
                         @"
					      EXEC J_Salida_EFMUN_Aprobados_NAL_Detalle_Electricidad @Indicadores_Seleccionados_Nal, @Umbral,@Indicadores_Seleccionados_Mun, @UmbralMunicipio
				",
                new
                {

                    Indicadores_Seleccionados_Nal = Indicadores_Seleccionados_NalString,
                    Umbral = resultadoMunicipio.Umbral_Seleccionado_Nal,
                    Indicadores_Seleccionados_Mun = Indicadores_Seleccionados_MunString,
                    UmbralMunicipio = resultadoMunicipio.Umbral_Seleccionado_Mun

                });

            return detalle;

        }




        //6.-Obtiene los Colores de los Municipios ya Evaluados
        //A)Petrolíferos
        public async Task<List<Color>> Obtener_Indicadores_Colores()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "EXEC [dbo].[ObtenerColoresMunicipio];";
                    await connection.OpenAsync();
                    var coloresIndicadores = await connection.QueryAsync<Color>(query);
                    return coloresIndicadores.ToList();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }
        //B)Gas L.P.
        public async Task<List<Color>> Obtener_Indicadores_Colores_GLP()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "EXEC [dbo].[ObtenerColoresMunicipio_GLP];";
                    await connection.OpenAsync();
                    var coloresIndicadores = await connection.QueryAsync<Color>(query);
                    return coloresIndicadores.ToList();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }
        //C)Electricidad
        public async Task<List<Color>> Obtener_Indicadores_Colores_Electricidad()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "EXEC [dbo].[ObtenerColoresMunicipio_Electricidad];";
                    await connection.OpenAsync();
                    var coloresIndicadores = await connection.QueryAsync<Color>(query);
                    return coloresIndicadores.ToList();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }







        //7.-Vista Detalle Resultado a Nivel EF
        //Salida Ef por ID
        //A)Petrolíferos
        public async Task<DetalleCalificacion_EF> D_Detalle_Calificacion_por_EF(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;
            var checkboxValuesString = string.Join(",", checkboxValues.Select(x => x.ToString()));
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);
            var idefParameter = new SqlParameter("@EF_ID", calificacionFinal.efId);
            var query = @"
                EXEC D_Detalle_Calificacion_por_EF @CheckboxValues, @Umbral, @EF_ID;
                EXEC [dbo].[CAL_XEF] @CheckboxValues, @Umbral, @EF_ID;
                EXEC B_Calificacion_Total_por_EF @CheckboxValues, @Umbral, @EF_ID;
				EXEC D_Detalle_Calificacion_por_MUNEF @CheckboxValues, @Umbral, @EF_ID;
				EXEC [dbo].[ObtieneExpendiosporEFID] @EF_ID;
					";

            using var multi = await connection.QueryMultipleAsync(query, new
            {
                CheckboxValues = checkboxValuesString,
                Umbral = calificacionFinal.Umbral_Seleccionado,
                EF_ID = calificacionFinal.efId
            });

            var detalle = multi.Read<CalificacionFinal>(); // Lee los resultados del primer stored procedure
            var resultadoSegundoSP = multi.Read<CalificacionFinal_EF>(); // Lee los resultados del segundo stored procedure
            var resultadoB_Calificacion_Total_por_EF = multi.Read<Parametros_EF>(); // Lee los resultados del segundo stored procedure
            var resultadoD_Detalle_Calificacion_por_MUNEF = multi.Read<DetalleCalificacionEF_Municipio>(); // Lee los resultados del segundo stored procedure
            var resultadoAutorizados = multi.Read<CalificacionFinal_EF>(); // Lee los resultados del segundo stored procedure

            var detalleCalificacion = new DetalleCalificacion_EF
            {
                Totales_EF = detalle,
                Detalle_EF = resultadoSegundoSP,
                Parametros_EF = resultadoB_Calificacion_Total_por_EF,
                Totales_EF_MUN = resultadoD_Detalle_Calificacion_por_MUNEF,
                Permisos_Autorizados = resultadoAutorizados
            };

            return detalleCalificacion;
        }
        //B)GAS L.P.
        public async Task<DetalleCalificacion_EF> D_Detalle_Calificacion_por_EF_GLP(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;
            var checkboxValuesString = string.Join(",", checkboxValues.Select(x => x.ToString()));
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);
            var idefParameter = new SqlParameter("@EF_ID", calificacionFinal.efId);
            var query = @"
                EXEC D_Detalle_Calificacion_por_EF_GLP @CheckboxValues, @Umbral, @EF_ID;
                EXEC [dbo].[CAL_XEF_GLP] @CheckboxValues, @Umbral, @EF_ID;
                EXEC B_Calificacion_Total_por_EF_GLP @CheckboxValues, @Umbral, @EF_ID;
				EXEC D_Detalle_Calificacion_por_MUNEF_GLP @CheckboxValues, @Umbral, @EF_ID;
				EXEC [dbo].[ObtieneExpendiosporEFID_GLP] @EF_ID;
					";

            using var multi = await connection.QueryMultipleAsync(query, new
            {
                CheckboxValues = checkboxValuesString,
                Umbral = calificacionFinal.Umbral_Seleccionado,
                EF_ID = calificacionFinal.efId
            });

            var detalle = multi.Read<CalificacionFinal>(); // Lee los resultados del primer stored procedure
            var resultadoSegundoSP = multi.Read<CalificacionFinal_EF>(); // Lee los resultados del segundo stored procedure
            var resultadoB_Calificacion_Total_por_EF = multi.Read<Parametros_EF>(); // Lee los resultados del segundo stored procedure
            var resultadoD_Detalle_Calificacion_por_MUNEF = multi.Read<DetalleCalificacionEF_Municipio>(); // Lee los resultados del segundo stored procedure
            var resultadoAutorizados = multi.Read<CalificacionFinal_EF>(); // Lee los resultados del segundo stored procedure

            var detalleCalificacion = new DetalleCalificacion_EF
            {
                Totales_EF = detalle,
                Detalle_EF = resultadoSegundoSP,
                Parametros_EF = resultadoB_Calificacion_Total_por_EF,
                Totales_EF_MUN = resultadoD_Detalle_Calificacion_por_MUNEF,
                Permisos_Autorizados = resultadoAutorizados
            };

            return detalleCalificacion;
        }
        //C)Electricidad
        public async Task<DetalleCalificacion_EF> D_Detalle_Calificacion_por_EF_Electricidad(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados;
            var checkboxValuesString = string.Join(",", checkboxValues.Select(x => x.ToString()));
            var umbralParameter = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);
            var idefParameter = new SqlParameter("@EF_ID", calificacionFinal.efId);
            var query = @"
                EXEC D_Detalle_Calificacion_por_EF_Electricidad @CheckboxValues, @Umbral, @EF_ID;
                EXEC [dbo].[CAL_XEF_Electricidad] @CheckboxValues, @Umbral, @EF_ID;
                EXEC B_Calificacion_Total_por_EF_Electricidad @CheckboxValues, @Umbral, @EF_ID;
				EXEC D_Detalle_Calificacion_por_MUNEF_Electricidad @CheckboxValues, @Umbral, @EF_ID;
				EXEC [dbo].[ObtieneExpendiosporEFID_Electricidad] @EF_ID;
					";

            using var multi = await connection.QueryMultipleAsync(query, new
            {
                CheckboxValues = checkboxValuesString,
                Umbral = calificacionFinal.Umbral_Seleccionado,
                EF_ID = calificacionFinal.efId
            });

            var detalle = multi.Read<CalificacionFinal>(); // Lee los resultados del primer stored procedure
            var resultadoSegundoSP = multi.Read<CalificacionFinal_EF>(); // Lee los resultados del segundo stored procedure
            var resultadoB_Calificacion_Total_por_EF = multi.Read<Parametros_EF>(); // Lee los resultados del segundo stored procedure
            var resultadoD_Detalle_Calificacion_por_MUNEF = multi.Read<DetalleCalificacionEF_Municipio>(); // Lee los resultados del segundo stored procedure
            var resultadoAutorizados = multi.Read<CalificacionFinal_EF>(); // Lee los resultados del segundo stored procedure

            var detalleCalificacion = new DetalleCalificacion_EF
            {
                Totales_EF = detalle,
                Detalle_EF = resultadoSegundoSP,
                Parametros_EF = resultadoB_Calificacion_Total_por_EF,
                Totales_EF_MUN = resultadoD_Detalle_Calificacion_por_MUNEF,
                Permisos_Autorizados = resultadoAutorizados
            };

            return detalleCalificacion;
        }


        //8.-Campos Visibles

        //A)Petrolíferos
        //Obtiene el atributo del cada coordenada si es visible o no
        public async Task<List<string>> ObtenerCamposVisibles(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo 
					  FROM [dbo].[CamposMapas] 
					  WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='vExpendios_autorizado'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }
        //Mapa de Infraestructura
        public async Task<List<string>> ObtenerCamposVisiblesPET_Infra(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo 
					  FROM [dbo].[CamposMapas] 
					  WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='vExpendios_autorizado_mapa'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }
        //B)Gas L.P.
        //Obtiene el atributo del cada coordenada si es visible o no
        public async Task<List<string>> ObtenerCamposVisiblesGLP(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo 
					  FROM [dbo].[CamposMapas] 
					  WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='vGasLP_autorizado'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }
        //Mapa de Infraestructura
        public async Task<List<string>> ObtenerCamposVisiblesGLP_Infra(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo 
					  FROM [dbo].[CamposMapas] 
					  WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='vGasLP_autorizado_mapa'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }
        //C)Gas Natural
        //Obtiene el atributo del cada coordenada si es visible o no en el mapa de Infraestructura
        public async Task<List<string>> ObtenerCamposVisiblesGN_Infra(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo 
                            FROM [dbo].[CamposMapas] 
                            WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='vGasNatural_autorizado_mapa'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }



        //D)Electricidad
        //Obtiene el atributo del cada coordenada si es visible o no
        public async Task<List<string>> ObtenerCamposVisiblesElectricidad(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo 
					  FROM [dbo].[CamposMapas] 
					  WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='vElectricidad_autorizado'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }

        //
        public async Task<List<string>> ObtenerCamposVisiblesElectricidad_Infra(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo 
                            FROM [dbo].[CamposMapas] 
                            WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='vElectricidad_autorizado_mapa'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }

        public async Task<List<string>> ObtenerCamposVisiblesRyS_Infra(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo
                            FROM [dbo].[CamposMapas] 
                            WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='Centrales_de_compresion'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }


        public async Task<List<string>> ObtenerCamposVisiblesCG_Infra(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo
                            FROM [dbo].[CamposMapas] 
                            WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='vComplejos_procesadoresGas'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }

        public async Task<List<string>> ObtenerCamposVisiblesPI_Infra(int rolId, int mercadoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var campos = await connection.QueryAsync<string>(
                    @"SELECT Nombre_Campo
                            FROM [dbo].[CamposMapas] 
                            WHERE Rol_ID = @RolId AND Mercado_ID = @MercadoId AND Visible = 1 AND REPLACE([Tabla], ' ', '')='vPuntos_internacion'",
                    new { RolId = rolId, MercadoId = mercadoId }
                );

                return campos.ToList();
            }
        }





        //9.-Atributos de los Mapas de Expendios y Centrales de Generación
        //A)Petrolíferos
        //Obtener Los datos de las coordenadas de los Permisos de Expendio al Público
        public async Task<IEnumerable<ExpendioAutorizado>> ObtenerExpendiosAutorizados()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizado>(
                    @"SELECT
				
					E.[NumeroPermiso]
						  ,E.[EfId]
						  ,E.[MpoId]
						  ,E.[NumeroDeExpediente]
						  ,E.[RazonSocial]
						  ,E.[FechaOtorgamiento]
						  ,E.[LatitudGeo]
						  ,E.[LongitudGeo]
						  ,E.[CalleNumEs]
						  ,E.[ColoniaEs]
						  ,E.[CodigoPostal]
						  ,E.[Estatus]
						  ,E.[Rfc]
						  ,E.[FechaRecepcion]
						  ,E.[EstatusInstalacion]
						  ,E.[CausaSuspension]
						  ,E.[Marca]
						  ,E.[TipoPermiso]
						  ,E.[InicioVigencia]
						  ,E.[TerminoVigencia]
						  ,E.[InicioOperaciones]
						  ,E.[CapacidadAutorizadaBarriles]
						  ,E.[InversionEstimada]
						  ,E.[Productos]
						  ,E.[Comentarios]
						  ,E.[TipoPersona]
						  ,E.[NumeroDeEstacionesDeServicio]
						  ,E.[TipoDeEstacion]
						  ,E.[FechaDeAcuse]
						  ,E.[FechaEntregaEstadosFinancieros]
						  ,E.[Propietario]
						  ,E.[CapacidadMaximaDeLaBomba]
						  ,E.[CapacidadOperativaReal]
						  ,E.[ServicioDeRegadera]
						  ,E.[ServicioDeRestaurante]
						  ,E.[ServicioDeSanitario]
						  ,E.[OtrosServicios]
						  ,E.[TiendaDeConveniencia]
						  ,E.[NumeroDeModulosDespachadores]
						  ,E.[TipoDeEstacionId]
						  ,E.[TipoDePersona]
						  ,E.[TipoDePermiso]
						  ,E.[EstadoDePermiso]
						  ,E.[EstatusDeLaInstalacion]
						  ,E.[ImagenCorporativa]
						  ,E.CausaSuspencionInstalacionId
						  ,EF.EF_Nombre AS EfNombre
					  FROM [dbo].[vExpendios_autorizado]AS E 
					 LEFT OUTER JOIN [dbo].[Entidades_Federativas] AS EF ON E.[EfId] = EF.EF_ID;

								 ");

                return coordenadas;
            }
        }
        //B)Gas L.P.
        //Obtener Los datos de las coordenadas de los Permisos de Expendio al Público
        public async Task<IEnumerable<ExpendioAutorizadoGLP>> ObtenerExpendiosAutorizadosGLP()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizadoGLP>(
                    @"SELECT
							   G.[NumeroPermiso]
							  ,G.[EfId]
							  ,G.[MpoId]
							  ,G.[Expediente]
							  ,G.[RazonSocial]
							  ,G.[FechaDeOtorgamiento]
							  ,G.[LatitudGeo]
							  ,G.[LongitudGeo]
							  ,G.[Calle]
							  ,G.[Colonia]
							  ,G.[CodigoPostal]
							  ,G.[Estatus]
							  ,G.[Rfc]
							  ,G.[FechaRecepcion]
							  ,G.[Marca]
							  ,G.[TipoPermiso]
							  ,G.[inicioOperaciones]
							  ,G.[CapacidadInstalacion]
							  ,G.[VigenciaAnos]
							  ,G.[NumeroSENER]
							  ,G.[SubTipo]
							  ,G.[SiglasTipo]
							  ,G.[Otorgamiento]
							  ,G.[FechaAcuse]
							  ,G.[EstatusSAT]
							  ,G.[Subestatus]
							  ,G.[SuspensionInicio]
							  ,G.[SuspensionFin]
							  ,G.[NumeroTanques]
							  ,G.[CapacidadLitros]
							  ,G.[NumeroUnidades]
							  ,G.[NumeroDeCentralesDeGuarda]
							  ,G.[DomicilioDeGuarda]
							  ,G.[SuministroRecepcion]
							  ,G.[PermisoSuministro]
							  ,G.[CompartenTanques]
							  ,G.[Modificacion]
							  ,G.[Asociacion]
							  ,G.[Gie]
							  ,EF.EF_Nombre AS EfNombre
						  FROM [dbo].[vGasLP_autorizado]AS G 
											 LEFT OUTER JOIN [dbo].[Entidades_Federativas] AS EF ON G.[EfId] = EF.EF_ID;

								 ");

                return coordenadas;
            }
        }
        //Mapa de Infraestructura
        public async Task<IEnumerable<ExpendioAutorizadoGLP>> ObtenerExpendiosAutorizadosGLP_Infra()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizadoGLP>(
                    @"
                    SELECT
							   G.[NumeroPermiso]
							  ,G.[EfId]
							  ,G.[MpoId]
							  ,G.[Expediente]
							  ,G.[RazonSocial]
							  ,G.[FechaDeOtorgamiento]
							  ,G.[LatitudGeo]
							  ,G.[LongitudGeo]
							  ,G.[Calle]
							  ,G.[Colonia]
							  ,G.[CodigoPostal]
							  ,G.[Estatus]
							  ,G.[Rfc]
							  ,G.[FechaRecepcion]
							  ,G.[Marca]
							  ,G.[TipoPermiso]
							  ,G.[inicioOperaciones]
							  ,G.[CapacidadInstalacion]
							  ,G.[VigenciaAnos]
							  ,G.[NumeroSENER]
							  ,G.[SubTipo]
							  ,G.[SiglasTipo]
							  ,G.[Otorgamiento]
							  ,G.[FechaAcuse]
							  ,G.[EstatusSAT]
							  ,G.[Subestatus]
							  ,G.[SuspensionInicio]
							  ,G.[SuspensionFin]
							  ,G.[NumeroTanques]
							  ,G.[CapacidadLitros]
							  ,G.[NumeroUnidades]
							  ,G.[NumeroDeCentralesDeGuarda]
							  ,G.[DomicilioDeGuarda]
							  ,G.[SuministroRecepcion]
							  ,G.[PermisoSuministro]
							  ,G.[CompartenTanques]
							  ,G.[Modificacion]
							  ,G.[Asociacion]
							  ,G.[Gie]
							  ,EF.EF_Nombre AS EfNombre
						  FROM [dbo].[vGasLP_autorizado_mapa]AS G 
											 LEFT OUTER JOIN [dbo].[Entidades_Federativas] AS EF ON G.[EfId] = EF.EF_ID;

								 ");

                return coordenadas;
            }
        }


        public async Task<IEnumerable<ExpendioAutorizadoGN>> ObtenerExpendiosAutorizadosGN_Infra()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizadoGN>(
                    @"
                    SELECT 
    [NumeroPermiso],
    [RazonSocial],
    -- Eliminar el prefijo numérico y el guion de EfId
    SUBSTRING([EfId], CHARINDEX('-', [EfId]) + 1, LEN([EfId])) AS [EfId],
    [MpoId],
    [LatitudGeo],
    [LongitudGeo],
    [Estatus],
    [FechaOtorgamiento],
    [EstatusInstalacion],
    [ColoniaEs],
    [CalleNumEs],
    [CodigoPostal],
    [InversionEstimada],
    [PermisoSuministrador],
    [PermisoDistribuidor],
    [Comentarios],
    [NumeroDeExpediente],
    [Rfc],
    [TipoPermiso],
    [Suministrador],
    [Distribuidor],
    [Interconexión],
    [Compresores],
    [NumeroDespachadores],
    [CapacidadDiseño],
    [Cilindros]
FROM [dbo].[vGasNatural_autorizado_mapa];

								 ");

                return coordenadas;
            }
        }


        public async Task<IEnumerable<ExpendioAutorizado>> ObtenerExpendiosAutorizadosPET_Infra()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizado>(
                    @"
                    SELECT
								E.[NumeroPermiso]
						  ,E.[EfId]
						  ,E.[MpoId]
						  ,E.[NumeroDeExpediente]
						  ,E.[RazonSocial]
						  ,E.[FechaOtorgamiento]
						  ,E.[LatitudGeo]
						  ,E.[LongitudGeo]
						  ,E.[CalleNumEs]
						  ,E.[ColoniaEs]
						  ,E.[CodigoPostal]
						  ,E.[Estatus]
						  ,E.[Rfc]
						  ,E.[FechaRecepcion]
						  ,E.[EstatusInstalacion]
						  ,E.[CausaSuspension]
						  ,E.[Marca]
						  ,E.[TipoPermiso]
						  ,E.[InicioVigencia]
						  ,E.[TerminoVigencia]
						  ,E.[InicioOperaciones]
						  ,E.[CapacidadAutorizadaBarriles]
						  ,E.[InversionEstimada]
						  ,E.[Productos]
						  ,E.[Comentarios]
						  ,E.[TipoPersona]
						  ,E.[NumeroDeEstacionesDeServicio]
						  ,E.[TipoDeEstacion]
						  ,E.[FechaDeAcuse]
						  ,E.[FechaEntregaEstadosFinancieros]
						  ,E.[Propietario]
						  ,E.[CapacidadMaximaDeLaBomba]
						  ,E.[CapacidadOperativaReal]
						  ,E.[ServicioDeRegadera]
						  ,E.[ServicioDeRestaurante]
						  ,E.[ServicioDeSanitario]
						  ,E.[OtrosServicios]
						  ,E.[TiendaDeConveniencia]
						  ,E.[NumeroDeModulosDespachadores]
						  ,E.[TipoDeEstacionId]
						  ,E.[TipoDePersona]
						  ,E.[TipoDePermiso]
						  ,E.[EstadoDePermiso]
						  ,E.[EstatusDeLaInstalacion]
						  ,E.[ImagenCorporativa]
						  ,E.CausaSuspencionInstalacionId
						  ,EF.EF_Nombre AS EfNombre
					  FROM [dbo].[vExpendios_autorizado_mapa]AS E 
					 LEFT OUTER JOIN [dbo].[Entidades_Federativas] AS EF ON E.[EfId] = EF.EF_ID;

								 ");

                return coordenadas;
            }
        }

        //C)Electricidad
        //Obtener Los datos de las coordenadas de los Permisos de Expendio al Público
        public async Task<IEnumerable<ExpendioAutorizadoElectricidad>> ObtenerExpendiosAutorizadosElectricidad()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizadoElectricidad>(
                    @"
                    SELECT
							   G.[NumeroPermiso]
							  ,G.[EfId]
							  ,G.[MpoId]
							  ,G.[Expediente]
							  ,G.[RazonSocial]
							  ,G.[FechaDeOtorgamiento]
							  ,G.[LatitudGeo]
							  ,G.[LongitudGeo]
							  ,G.[Calle]
							  ,G.[Colonia]
							  ,G.[CodigoPostal]
							  ,G.[Estatus]
							  ,G.[Rfc]
							  ,G.[FechaRecepcion]
							  ,G.[Marca]
							  ,G.[TipoPermiso]
							  ,G.[inicioOperaciones]
							  ,G.[CapacidadInstalacion]
							  ,G.[VigenciaAnos]
							  ,G.[NumeroSENER]
							  ,G.[SubTipo]
							  ,G.[SiglasTipo]
							  ,G.[Otorgamiento]
							  ,G.[FechaAcuse]
							  ,G.[EstatusSAT]
							  ,G.[Subestatus]
							  ,G.[SuspensionInicio]
							  ,G.[SuspensionFin]
							  ,G.[NumeroTanques]
							  ,G.[CapacidadLitros]
							  ,G.[NumeroUnidades]
							  ,G.[NumeroDeCentralesDeGuarda]
							  ,G.[DomicilioDeGuarda]
							  ,G.[SuministroRecepcion]
							  ,G.[PermisoSuministro]
							  ,G.[CompartenTanques]
							  ,G.[Modificacion]
							  ,G.[Asociacion]
							  ,G.[Gie]
							  ,EF.EF_Nombre AS EfNombre
						  FROM [dbo].[vElectricidad_autorizado]AS G 
											 LEFT OUTER JOIN [dbo].[Entidades_Federativas] AS EF ON G.[EfId] = EF.EF_ID;

								 ");

                return coordenadas;
            }
        }

        //Mapas de Infraestructura de RyS
        public async Task<IEnumerable<ExpendioAutorizadoRyS_Infra>> ObtenerExpendiosAutorizadosRyS_Infra()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizadoRyS_Infra>(
                    @"

					  SELECT [Nombre] as [NumeroPermiso]                             
                              ,[Nombre] as [RazonSocial]
                              ,[Latitud] as [LatitudGeo]
                              ,[Longitud] as [LongitudGeo]
                              ,'RyS' as [Clasificacion]
                           FROM [dbo].[Centrales_de_compresion]

								 ");

                return coordenadas;
            }
        }


        //MapasCG
        public async Task<IEnumerable<ExpendioAutorizadoCG_Infra>> ObtenerExpendiosAutorizadosCG_Infra()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizadoCG_Infra>(
                    @"
                                    SELECT 
                                    [NumeroDePermiso] as [NumeroPermiso]
                                        ,[Nombre] as [RazonSocial]
                                        ,[Latitud] as [LatitudGeo]
                                        ,[Longitud] as [LongitudGeo]
                                        ,'CG' as [Clasificacion]
                                        ,[Comentarios]
                                        ,[Inicio_operaciones]  as InicioOperaciones            
                                        ,[Número de Plantas de endulzamiento de Gas] as NPDG
                                        ,[Capacidad de endulzamiento de Gas (Mmpcd)] as CED
                                        ,[Número de Plantas de recuperación de Líquidos] as NPRL
                                        ,[Capacidad de recuperación de Líquidos (Mmpcd)] as CRL
                                        ,[Número de Plantas de fraccionamiento de Líquidos] as NPFL
                                        ,[Capacidad de fraccionamiento de Líquidos (Mbd)] as CFL
                                        ,[Número de Plantas de producción de Azufre] as NPPA
                                        ,[Capacidad de Producción de Azufre (Td)] as CPA
                                        ,[Licenciador]
                                    FROM [dbo].[vComplejos_procesadoresGas]
								 ");

                return coordenadas;
            }
        }

        //Mapa de Puntos de Internación 
        public async Task<IEnumerable<ExpendioAutorizadoPI_Infra>> ObtenerExpendiosAutorizadosPI_Infra()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizadoPI_Infra>(
                    @"
                                   SELECT 
                                        [NumeroPermiso]
                                        ,[Permisionario] as [RazonSocial]
                                        ,[Latitud]  as [LatitudGeo]
                                        ,[Longitud] as [LongitudGeo]
                                        ,'PI' as [Clasificacion]
                                        ,[Nombre] 
                                        ,[InicioOperaciones]
                                        ,[LocalidadSalidaMex]
                                        ,[PuntoSalidaEua]
                                        ,[LocalidadSalidaEua]
                                        ,[GasoductoMex]
                                        ,[GasoductoEua]
                                        ,[TipoDePermiso]
                                        ,[Consorcio]
                                        ,[Capacidad_Mmpcd]
                                        ,[Diámetro] as Diametro
                                        ,[Referencia]
                                    FROM [dbo].[vPuntos_internacion]
								 ");

                return coordenadas;
            }
        }




        //Mapas de Infraestructura de Electricidad
        public async Task<IEnumerable<ExpendioAutorizadoElectricidad_Infra>> ObtenerExpendiosAutorizadosElectricidad_Infra()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<ExpendioAutorizadoElectricidad_Infra>(
                    @"
                        SELECT 
    [NumeroPermiso],
    -- Eliminar el prefijo numérico de EfId
    SUBSTRING([EfId], CHARINDEX(' ', [EfId]) + 1, LEN([EfId])) AS [EfId],
    [MpoId],
    [NumeroDeExpediente],
    [RazonSocial],
    [FechaOtorgamiento],
    [LatitudGeo],
    [LongitudGeo],
    [Dirección] AS Direccion,
    [Estatus],
    [RFC],
    [FechaRecepcion],
    [EstatusInstalacion],
    [TipoPermiso],
    [InicioVigencia],
    [InicioOperaciones],
    [CapacidadAutorizadaMW],
    [Generación_estimada_anual] AS [GeneracionEstimadaAnual],
    [Inversion_estimada_mdls] AS InversionEstimadaMdls,
    [Energetico_primario] AS EnergeticoPrimario,
    [Actividad_economica] AS ActividadEconomica,
    [Tecnología] AS [Tecnologia],
    [EmpresaLíder] AS [EmpresaLider],
    [PaísDeOrigen] AS PaisDeOrigen,
    [Subasta],
    [Planta],
    [Combustible],
    [FuenteEnergía] AS [FuenteEnergia],
    [Comentarios],
    [Clasifica_Menú] AS [Clasificacion],
    [Tipo_Empresa] as TipoEmpresa
FROM [dbo].[vElectricidad_autorizado_mapa];

								 ");

                return coordenadas;
            }
        }




        //10.-Carga las Solicitudes
        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        //A)Petrolíferos
        public async Task<int> InsertarSolicitudesBulk(List<CargaSolicitudExpendio> solicitudes)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "dbo.Carga_Solicitudes_Expendio";
                    // Aquí, mapea las columnas del objeto SolicitudExpendio con las columnas de la tabla, si es necesario.
                    try
                    {
                        DataTable dataTable = ConvertToDataTable(solicitudes);
                        await bulkCopy.WriteToServerAsync(dataTable);

                        return solicitudes.Count;
                    }
                    catch (Exception ex)
                    {
                        // Manejar la excepción como lo consideres necesario
                        throw;
                    }
                }
            }
        }






        //Obtener Los datos de las coordenadas de los Permisos de Expendio al Público
        public async Task<IEnumerable<Expendios>> ObtenerExpendios()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var coordenadas = await connection.QueryAsync<Expendios>(
                    @"SELECT
					  [NumeroPermiso]
					  ,[EF_ID]
					  ,[MPO_ID]
					  ,[Razón_social]
					  ,[FechaOtorgamiento]
					  ,[Latitud_GEO]
					  ,[Longitud_GEO]
					  ,[Calle_num_ES]
					  ,[Colonia_ES]
					  ,[Codigo_Postal]
					  ,[Estatus]
					  ,[RFC]
					  ,[FechaRecepcion]
					  ,[Estatus_instalacion]
					  ,[Causa_suspension]
					  ,[Numero_de_Expediente]
					  ,[Marca]
					  ,[Comentarios]
				  FROM [dbo].[Expendios]");

                return coordenadas;
            }
        }







        //11.-Obtiene la relacion de Grupos de Ineteres por Razón Social de las Solicitudes a Evaluar
        //Petríferos
        public async Task<IEnumerable<Consulta_GIE>> ObtenerGIE_Petroliferos()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "EXEC [dbo].[spGIE_PET];";
                    await connection.OpenAsync();
                    var gie = await connection.QueryAsync<Consulta_GIE>(query);
                    return gie.ToList();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Consulta_GIE>> ObtenerGIE_Petroliferos_A()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "EXEC [dbo].[spGIE_PET_A];";
                    await connection.OpenAsync();
                    var gie = await connection.QueryAsync<Consulta_GIE>(query);
                    return gie.ToList();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Consulta_GIE>> ObtenerGIE_Petroliferos_S()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "EXEC [dbo].[spGIE_PET_S];";
                    await connection.OpenAsync();
                    var gie = await connection.QueryAsync<Consulta_GIE>(query);
                    return gie.ToList();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }


        //GLP

        public async Task<IEnumerable<Consulta_GIE>> ObtenerGIE_GLP()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "EXEC [dbo].[spGIE_GLP];";
                    await connection.OpenAsync();
                    var gie = await connection.QueryAsync<Consulta_GIE>(query);
                    return gie.ToList();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Consulta_GIE>> ObtenerGIE_GLP_A()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "EXEC [dbo].[spGIE_GLP_A];";
                    await connection.OpenAsync();
                    var gie = await connection.QueryAsync<Consulta_GIE>(query);
                    return gie.ToList();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Consulta_GIE>> ObtenerGIE_GLP_S()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "EXEC [dbo].[spGIE_GLP_S];";
                    await connection.OpenAsync();
                    var gie = await connection.QueryAsync<Consulta_GIE>(query);
                    return gie.ToList();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }













        //11.1-Obtiene el GIE por Razón Social y lo devuelve al Mapa
        //A)Petrolíferos
        public async Task<(List<Consulta_GIExRazonSocial> Permisos, List<Consulta_GIExRazonSocial> Solicitudes)> ObtenerGIEPorRazonSocial(string razonSocial)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = @"
                EXEC [dbo].[usp_BuscarGIE_RazonSocial_Permisos] @RazonSocial;
                EXEC [dbo].[usp_BuscarGIE_RazonSocial_Solicitudes] @RazonSocial;
            ";
                    await connection.OpenAsync();

                    using var multi = await connection.QueryMultipleAsync(query, new { RazonSocial = razonSocial });

                    var permisos = multi.Read<Consulta_GIExRazonSocial>().ToList();
                    var solicitudes = multi.Read<Consulta_GIExRazonSocial>().ToList();

                    return (Permisos: permisos, Solicitudes: solicitudes);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }
        //B)GLP
        public async Task<(List<ExpendioAutorizadoGLP> Permisos, List<Consulta_GIExRazonSocial> Solicitudes)> ObtenerGIEPorRazonSocialGLP(string razonSocial)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = @"
                EXEC [dbo].[usp_BuscarGIE_RazonSocial_Permisos_GLP] @RazonSocial;
                EXEC [dbo].[usp_BuscarGIE_RazonSocial_Solicitudes_GLP] @RazonSocial;
            ";
                    await connection.OpenAsync();

                    using var multi = await connection.QueryMultipleAsync(query, new { RazonSocial = razonSocial });

                    var permisos = multi.Read<ExpendioAutorizadoGLP>().ToList();
                    var solicitudes = multi.Read<Consulta_GIExRazonSocial>().ToList();

                    return (Permisos: permisos, Solicitudes: solicitudes);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de SQL: " + ex.Message);
                throw;
            }
        }




        //12.-Solicitud a 3km
        //Obtiene la solicitud a evaluar
        //Petríferos
        public async Task<Consulta_SolicitudesE> ObtenerSolicitudPorId(int Id)

        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var solicitud = await connection.QuerySingleOrDefaultAsync<Consulta_SolicitudesE>(
                       @"
                    SELECT  [ID]
                      ,[EF_ID]
                      ,[EF_Nombre]
                      ,[MPO_ID]
                      ,[Municipio_Nombre]
                      ,Case [Documentos_completos]
                        when 1 then 'SI'
                        else 'NO' END as [Documentos_completos]
                      ,Case [Analisis_riesgo]
                        when 1 then 'SI'
                        else 'NO' END as [Analisis_riesgo]
                      ,[X_Geo] as Latitud_GEO
                      ,[Y_Geo] as Longitud_GEO
                      ,Turno
                      ,[Marca_solicitada]
                      ,[Expediente]
                      ,[Razon_social]
                       FROM [dbo].[Calificacion_Solicitudes_Petroleo_EF]
                    WHERE [ID] = @Id
                    ", new { Id = Id });


                return solicitud;
            }
        }

        //GLP
        public async Task<Consulta_SolicitudesE> ObtenerSolicitudPorIdGLP(int Id)

        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var solicitud = await connection.QuerySingleOrDefaultAsync<Consulta_SolicitudesE>(
                       @"
                        SELECT  [ID]
                      ,[EF_ID]
                      ,[EF_Nombre]
                      ,[MPO_ID]
                      ,[Municipio_Nombre]
                      ,Case [Documentos_completos]
                        when 1 then 'SI'
                        else 'NO' END as [Documentos_completos]
                      ,Case [Analisis_riesgo]
                        when 1 then 'SI'
                        else 'NO' END as [Analisis_riesgo]
                      ,[X_Geo] as Latitud_GEO
                      ,[Y_Geo] as Longitud_GEO
                      ,Turno
                      ,[Marca_solicitada]
                      ,[Expediente]
                      ,[Razon_social]
                       FROM [dbo].[Calificacion_Solicitudes_GASLP_EF]
                    WHERE [ID] = @Id
                    ", new { Id = Id });


                return solicitud;
            }
        }




        public async Task Crear(Indicador2_Clase indicador2_Clase)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QueryAsync(
               $@"
            INSERT INTO [dbo].[Usuarios]
                    ([Usuario]
                    ,[Email]
                    ,[EmailNormalizado]
                    ,[PasswordHash])
            VALUES
                    (@Usuario, @Email, @EmailNormalizado, @PasswordHash);

                    ", indicador2_Clase);
            // indicador2_Clase.id = id;
        }
        //Método para verificar registros duplicados
        #region  Método de Usuarios Duplicados
        public async Task<bool> Existe(string usuario, string email)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>($@"
                                                                                  Select 1
                                                                                  From [dbo].[Usuarios]
                                                                                  where Usuario = @Usuario and Email=@Email",

                                                                          new { usuario, email });
            return existe == 1;
        }
        #endregion


        //Indicador I1
        public async Task<IEnumerable<Indicador1_Clase>> Obtener_I1()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Indicador1_Clase>(
                               $@"
	SELECT
I2.[EF_ID]

      ,I2.[EF_Nombre]
	 -- ,C2.[EF_Nombre]
      ,I2.[2015] as Año1
	  ,CASE C2.[2015]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año1

      ,I2.[2016] as Año2
	  ,CASE C2.[2016]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año2

      ,I2.[2017] as Año3
	  ,CASE C2.[2017]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año3
      ,I2.[2018] as Año4
	  ,CASE C2.[2018]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año4
      ,I2.[2019] as Año5
	  ,CASE C2.[2019]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año5

      ,I2.[2020] as Año6
	  ,CASE C2.[2020]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año6

      ,I2.[2021] as Año7
      ,CASE C2.[2021]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año7
	  ,I2.[2022] as Año8
	  ,CASE C2.[2022]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año8
      ,I2.[2023] as Año9
	  ,CASE C2.[2023]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año9
      ,I2.[2024] as Año10
	  ,CASE C2.[2024]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año10
      ,I2.[2025] as Año11
	  ,CASE C2.[2025]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año11
      ,I2.[2026] as Año12
	  ,CASE C2.[2026]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año12
      ,I2.[2027] as Año13
	  ,CASE C2.[2027]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año13
      ,I2.[2028] as Año14
	  ,CASE C2.[2028]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año14
      ,I2.[2029] as Año15
	  ,CASE C2.[2029]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año15
      ,I2.[2030] as Año16
	  ,CASE C2.[2030]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año16
  FROM [dbo].[Indicador01_EF] as I2 left outer join  [dbo].[Indicador01_salida_EF] as C2 on (I2.EF_ID=C2.EF_ID)

									");

        }
        //Indicador I2
        public async Task<IEnumerable<Indicador2_Clase>> Obtener_I2()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Indicador2_Clase>(
                               $@"
				/****** Script for SelectTopNRows command from SSMS  ******/
				SELECT

					  I2.[EF_Nombre]
				,I2.[EF_ID]
					 -- ,C2.[EF_Nombre]
					  ,I2.[2015] as Año1
					  ,CASE C2.[2015]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año1

					  ,I2.[2016] as Año2
					  ,CASE C2.[2016]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año2

					  ,I2.[2017] as Año3
					  ,CASE C2.[2017]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año3
					  ,I2.[2018] as Año4
					  ,CASE C2.[2018]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año4
					  ,I2.[2019] as Año5
					  ,CASE C2.[2019]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año5

					  ,I2.[2020] as Año6
					  ,CASE C2.[2020]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año6

					  ,I2.[2021] as Año7
					  ,CASE C2.[2021]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año7
					  ,I2.[2022] as Año8
					  ,CASE C2.[2022]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año8
					  ,I2.[2023] as Año9
					  ,CASE C2.[2023]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año9
					  ,I2.[2024] as Año10
					  ,CASE C2.[2024]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año10
					  ,I2.[2025] as Año11
					  ,CASE C2.[2025]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año11
					  ,I2.[2026] as Año12
					  ,CASE C2.[2026]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año12
					  ,I2.[2027] as Año13
					  ,CASE C2.[2027]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año13
					  ,I2.[2028] as Año14
					  ,CASE C2.[2028]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año14
					  ,I2.[2029] as Año15
					  ,CASE C2.[2029]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año15
					  ,I2.[2030] as Año16
					  ,CASE C2.[2030]
							When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
							ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
							END as C_Año16
				  FROM [dbo].[Indicador02] as I2 left outer join  [dbo].[Indicador02_salida] as C2 on (I2.EF_ID=C2.EF_ID)



									");

        }
        //Indicador I3
        public async Task<IEnumerable<Indicador3_Clase>> Obtener_I3()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Indicador3_Clase>(
                               $@"
		SELECT
I2.[EF_ID]

      ,I2.[EF_Nombre]
	 -- ,C2.[EF_Nombre]
      ,I2.[2015] as Año1
	  ,CASE C2.[2015]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año1

      ,I2.[2016] as Año2
	  ,CASE C2.[2016]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año2

      ,I2.[2017] as Año3
	  ,CASE C2.[2017]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año3
      ,I2.[2018] as Año4
	  ,CASE C2.[2018]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año4
      ,I2.[2019] as Año5
	  ,CASE C2.[2019]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año5

      ,I2.[2020] as Año6
	  ,CASE C2.[2020]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año6

      ,I2.[2021] as Año7
      ,CASE C2.[2021]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año7
	  ,I2.[2022] as Año8
	  ,CASE C2.[2022]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año8
      ,I2.[2023] as Año9
	  ,CASE C2.[2023]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año9
      ,I2.[2024] as Año10
	  ,CASE C2.[2024]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año10
      ,I2.[2025] as Año11
	  ,CASE C2.[2025]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año11
      ,I2.[2026] as Año12
	  ,CASE C2.[2026]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año12
      ,I2.[2027] as Año13
	  ,CASE C2.[2027]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año13
      ,I2.[2028] as Año14
	  ,CASE C2.[2028]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año14
      ,I2.[2029] as Año15
	  ,CASE C2.[2029]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año15
      ,I2.[2030] as Año16
	  ,CASE C2.[2030]
			When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
			ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
			END as C_Año16
  FROM [dbo].[Indicador03_EF] as I2 left outer join  [dbo].[Indicador03_salida_EF] as C2 on (I2.EF_ID=C2.EF_ID)


									");

        }
        //Indicador I4
        public async Task<IEnumerable<Indicador4_Clase>> Obtener_I4()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Indicador4_Clase>(
                               $@"
										SELECT
								I2.[EF_ID]

									  ,I2.[EF_Nombre]
									 -- ,C2.[EF_Nombre]
									  ,I2.[2015] as Año1
									  ,CASE C2.[2015]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año1

									  ,I2.[2016] as Año2
									  ,CASE C2.[2016]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año2

									  ,I2.[2017] as Año3
									  ,CASE C2.[2017]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año3
									  ,I2.[2018] as Año4
									  ,CASE C2.[2018]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año4
									  ,I2.[2019] as Año5
									  ,CASE C2.[2019]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año5

									  ,I2.[2020] as Año6
									  ,CASE C2.[2020]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año6

									  ,I2.[2021] as Año7
									  ,CASE C2.[2021]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año7
									  ,I2.[2022] as Año8
									  ,CASE C2.[2022]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año8
									  ,I2.[2023] as Año9
									  ,CASE C2.[2023]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año9
									  ,I2.[2024] as Año10
									  ,CASE C2.[2024]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año10
									  ,I2.[2025] as Año11
									  ,CASE C2.[2025]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año11
									  ,I2.[2026] as Año12
									  ,CASE C2.[2026]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año12
									  ,I2.[2027] as Año13
									  ,CASE C2.[2027]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año13
									  ,I2.[2028] as Año14
									  ,CASE C2.[2028]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año14
									  ,I2.[2029] as Año15
									  ,CASE C2.[2029]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año15
									  ,I2.[2030] as Año16
									  ,CASE C2.[2030]
											When -1 Then 'background-color:  #9f2241;border-color: #4c1922;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											When 0 Then 'background-color: #ffe600;border-color: #bd9865;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:#4c1922; text-align: center;'
											ELSE 'background-color:  #235b4e;border-color: #13322b;border-style:solid;border-width:1px;font-family:Montserrat, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;text-align:left;vertical-align:bottom;word-break:normal; color:whitesmoke;text-align: center;'
											END as C_Año16
									FROM [dbo].[Indicador04_EF] as I2 left outer join  [dbo].[Indicador04_salida_EF] as C2 on (I2.EF_ID=C2.EF_ID)


									");

        }






        #region  Busca Expendio

        public async Task<Expendios> ObtenerExpendioPorNumeroPermiso(string NumeroPermiso)
        {
            using var connection = new SqlConnection(connectionString);
            var detalle = await connection.QuerySingleOrDefaultAsync<Expendios>(
                @"
                  	 SELECT  
                           ISNULL([NumeroPermiso], 'S/D') as [NumeroPermiso]
                          ,ISNULL([EF_ID], 'S/D') as [EF_ID]
                          ,ISNULL([MPO_ID], 'S/D') as [MPO_ID]
                          ,ISNULL([Razón_social], 'S/D') as [Razón_social]
                          , [FechaOtorgamiento] as [FechaOtorgamiento]
                          ,ISNULL([Latitud_GEO], 'S/D') as [Latitud_GEO]
                          ,ISNULL([Longitud_GEO], 'S/D') as [Longitud_GEO]
                          ,ISNULL([Calle_num_ES], 'S/D') as [Calle_num_ES]
                          ,ISNULL([Colonia_ES], 'S/D') as [Colonia_ES]
                          ,ISNULL([Codigo_Postal], 'S/D') as [Codigo_Postal]
                          ,ISNULL([Estatus], 'S/D') as [Estatus]
                          ,ISNULL([RFC], 'S/D') as [RFC]
                          , [FechaRecepcion] as [FechaRecepcion] 
                          ,ISNULL([Estatus_instalacion], 'S/D') as [Estatus_instalacion]
	                       ,'S/D' as [Subestatus]
                          ,ISNULL([Causa_suspension], 'S/D') as [Causa_suspension]
                          ,ISNULL([Numero_de_Expediente], 'S/D') as [Numero_de_Expediente]
                          ,ISNULL([Marca],'S/D') as Marca
                          ,ISNULL([Comentarios], 'S/D') as [Comentarios]
                          ,ISNULL([Tipo_permiso], 'S/D') as [Tipo_permiso]
                          ,'Petrolíferos' as [Mercado]
                      FROM [dbo].[Expendios]
                        WHERE NumeroPermiso = @NumeroPermiso
                      Union all
					         SELECT  
                           ISNULL([NumeroPermiso],'S/D') as [NumeroPermiso]
                          ,ISNULL([EfId],'S/D') as [EF_ID]
                          ,ISNULL([MpoId],'S/D') as [MPO_ID]
                          ,ISNULL([RazonSocial],'S/D') as [Razón_social]
                          ,[FechaOtorgamiento] as [FechaOtorgamiento]
                          ,ISNULL([LatitudGeo],'S/D') as [Latitud_GEO]
                          ,ISNULL([LongitudGeo],'S/D') as [Longitud_GEO]
                          ,ISNULL([CalleNumEs],'S/D') as [Calle_num_ES]
                          ,ISNULL([ColoniaEs],'S/D') as [Colonia_ES]
                          ,ISNULL([CodigoPostal],'S/D') as [Codigo_Postal]
                          ,ISNULL([Estatus],'S/D') as [Estatus]
                          ,ISNULL([RFC],'S/D') as [RFC]
                          ,[FechaRecepcion] as [FechaRecepcion] 
                          ,ISNULL([EstatusInstalacion],'S/D') as [Estatus_instalacion]
	                      ,'S/D' as [Subestatus]
                          ,ISNULL([CausaSuspension],'S/D') as [Causa_suspension]
                          ,ISNULL([NumeroDeExpediente],'S/D') as [Numero_de_Expediente]
                          ,isnull([Marca],'S/D') as Marca
                          ,ISNULL([Comentarios],'S/D') as [Comentarios]
                          ,ISNULL([TipoPermiso],'S/D') as [Tipo_permiso]
                          ,'Petrolíferos' as [Mercado]
                      FROM [dbo].[vExpendios_autorizado_mapa]
                        WHERE NumeroPermiso = @NumeroPermiso And [TipoPermiso] not like 'Expendios'
                      Union all
           SELECT  
                        ISNULL([NumeroPermiso],'S/D') as [NumeroPermiso],
                        ISNULL([EfId],'S/D') as [EF_ID],
                        ISNULL([MpoId],'S/D') as [MPO_ID],
                        ISNULL([RazonSocial],'S/D') as [Razón_social],
                        [FechaDeOtorgamiento]  as [FechaOtorgamiento],
                        ISNULL([LatitudGeo],'S/D') as [Latitud_GEO],
                        ISNULL([LongitudGeo],'S/D') as [Longitud_GEO],
                        ISNULL([Calle],'S/D') as [Calle_num_ES],
                        ISNULL([Colonia],'S/D') as [Colonia_ES],
                        ISNULL([CodigoPostal],'S/D') as [Codigo_Postal],
                        ISNULL([Estatus],'S/D') as [Estatus],
                        ISNULL([RFC],'S/D') as [RFC],
                         [FechaRecepcion]  as [FechaRecepcion],
                        'S/D' as [Estatus_instalacion],
                        ISNULL([Subestatus],'S/D') as [Subestatus],
                        'S/D' as [Causa_suspension],
                        ISNULL([Expediente],'S/D') as [Numero_de_Expediente],
                        'S/D' as [Marca],
                        'S/D' as Comentarios,
                        ISNULL([TipoPermiso],'S/D') as [Tipo_permiso]
                        ,'GLP' as [Mercado]
                    FROM [dbo].[vGasLP_autorizado_mapa]
WHERE NumeroPermiso = @NumeroPermiso
			  Union all
					     SELECT  
                           ISNULL([NumeroPermiso],'S/D') as [NumeroPermiso]
                          ,ISNULL([EfId],'S/D') as [EF_ID]
                          ,ISNULL([MpoId],'S/D') as [MPO_ID]
                          ,ISNULL([RazonSocial],'S/D') as [Razón_social]
                          ,[FechaOtorgamiento] as [FechaOtorgamiento]
                          ,[LatitudGeo] as [Latitud_GEO]
                          ,[LongitudGeo] as [Longitud_GEO]
                          ,ISNULL([CalleNumEs],'S/D') as [Calle_num_ES]
                          ,ISNULL([ColoniaEs],'S/D') as [Colonia_ES]
                          ,ISNULL([CodigoPostal],'S/D') as [Codigo_Postal]
                          ,ISNULL([Estatus],'S/D') as [Estatus]
                          ,ISNULL([RFC],'S/D') as [RFC]
                           ,CAST('9999-12-31' AS DATETIME) as [FechaRecepcion]
                          ,ISNULL([EstatusInstalacion],'S/D') as [Estatus_instalacion]
	                      ,'S/D' as [Subestatus]
                           ,'S/D' as [Causa_suspension]
                          ,ISNULL([NumeroDeExpediente],'S/D') as [Numero_de_Expediente]
                          ,'S/D' as Marca
                          ,ISNULL([Comentarios],'S/D') as [Comentarios]
                          ,ISNULL([TipoPermiso],'S/D') as [Tipo_permiso]
                          ,'GN' as [Mercado]
                      FROM [dbo].[vGasNatural_autorizado_mapa]
                        WHERE NumeroPermiso = @NumeroPermiso 
							  Union all
					     SELECT  
                           ISNULL([NumeroPermiso],'S/D') as [NumeroPermiso]
                          ,ISNULL([EfId],'S/D') as [EF_ID]
                          ,ISNULL([MpoId],'S/D') as [MPO_ID]
                          ,ISNULL([RazonSocial],'S/D') as [Razón_social]
                          ,[FechaOtorgamiento] as [FechaOtorgamiento]
                          ,[LatitudGeo] as [Latitud_GEO]
                          ,[LongitudGeo] as [Longitud_GEO]
                          ,ISNULL([Dirección],'S/D')  as [Calle_num_ES]
                          ,'S/D' as [Colonia_ES]
                         ,'S/D' as [Codigo_Postal]
                          ,ISNULL([Estatus],'S/D') as [Estatus]
                          ,ISNULL([RFC],'S/D') as [RFC]
                          ,CAST('9999-12-31' AS DATETIME) as [FechaRecepcion]
                          ,ISNULL([EstatusInstalacion],'S/D') as [Estatus_instalacion]
	                      ,'S/D' as [Subestatus]
                           ,'S/D' as [Causa_suspension]
                          ,ISNULL([NumeroDeExpediente],'S/D') as [Numero_de_Expediente]
                          ,'S/D' as Marca
                          ,ISNULL([Comentarios],'S/D') as [Comentarios]
                          ,ISNULL([TipoPermiso],'S/D') as [Tipo_permiso]
                          ,'Electricidad' as [Mercado]
                      FROM [dbo].[vElectricidad_autorizado_mapa]
                        WHERE NumeroPermiso = @NumeroPermiso 
                ",
                new { NumeroPermiso });

            return detalle;
        }
        #endregion

        #region Dashboard Detalle PE
        public async Task<IEnumerable<DatosDashboard_PE>> ObtenerDatosPorPermisoYAnio(string numeroPermiso)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
                 WITH DatosTrimestrales AS (
                                            SELECT 
                                                [Periodo],
                                                [Trimestre], -- Trimestre como int
                                                [NumeroPermiso],
                                                [Generacion_bruta_o_importacion_gwh] AS GeneracionBrutaOImportacionGWh,
                                                [Generacion_neta_gwh] AS GeneracionNetaGWh,
                                                [Factor_planta_reportado] AS FactorPlantaReportado,
                                                [factor_planta_calculado] AS FactorPlantaCalculado,
                                                [Emisiones_co2] AS EmisionesCO2,
                                                [Consumo_agua] AS ConsumoAgua
                                            FROM [dbo].[vPermisosElectricidad_consolidado_Trimestral]
                                            WHERE NumeroPermiso = @NumeroPermiso
                                        )
                                        SELECT 
                                            Periodo,
                                            NULL AS Trimestre, -- Datos anuales no tienen trimestre
                                            NumeroPermiso,
                                            AVG(GeneracionBrutaOImportacionGWh) AS GeneracionBrutaOImportacionGWh,
                                            AVG(GeneracionNetaGWh) AS GeneracionNetaGWh,
                                            AVG(FactorPlantaReportado) AS FactorPlantaReportado,
                                            AVG(FactorPlantaCalculado) AS FactorPlantaCalculado,
                                            SUM(EmisionesCO2) AS EmisionesCO2,
                                            SUM(ConsumoAgua) AS ConsumoAgua
                                        FROM DatosTrimestrales
                                        GROUP BY Periodo, NumeroPermiso

                                        UNION ALL

                                        SELECT 
                                            Periodo,
                                            Trimestre,
                                            NumeroPermiso,
                                            GeneracionBrutaOImportacionGWh,
                                            GeneracionNetaGWh,
                                            FactorPlantaReportado,
                                            FactorPlantaCalculado,
                                            EmisionesCO2,
                                            ConsumoAgua
                                        FROM DatosTrimestrales
                                        ORDER BY Periodo, Trimestre;
                        ";
            return await connection.QueryAsync<DatosDashboard_PE>(query, new { NumeroPermiso = numeroPermiso });
        }

        #endregion

        //Precios de Gas LP
        public async Task<IEnumerable<PreciosGLP>> ObtenerPreciosGLP(int year, int month, int week, string priceType)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@Ano", year, DbType.Int32);
                parameters.Add("@Mes", month, DbType.Int32);
                parameters.Add("@Semana", week, DbType.Int32);
                parameters.Add("@Tipo", priceType, DbType.String);

                var precios = await connection.QueryAsync<PreciosGLP>(
                    "spGetPreciosGLP",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return precios;
            }
        }





        /// <summary>
        /// Este es el Store que devuelve la Calificación Final a nivel Resumen
        /// </summary>
        /// <param name="calificacionFinal por Entidad Federativa"></param>
        /// <returns></returns>

        #region Municipios

        /// <summary>
        /// Este es el Stor que devuelve la Calificación Final a nivel Resumen
        /// </summary>
        /// <param name="calificacionFinal por Municipio"></param>
        /// <returns></returns>

        public async Task<IEnumerable<CalificacionFinal>> C_Calificacion_por_MUN(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Asignando los Parametroos
            var checkboxValues_EF = calificacionFinal.Indicadores_Seleccionados;
            var checkboxValues_MUN = calificacionFinal.Indicadores_Seleccionados_Municipio;
            var checkboxValuesString_EF = string.Join(",", checkboxValues_EF);
            var checkboxValuesString_MUN = string.Join(",", checkboxValues_MUN);
            var umbralParameter_EF = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);
            var umbralParameter_MUN = new SqlParameter("@UmbralMunicipio", calificacionFinal.Umbral_Seleccionado_Municipio);
            var efid = new SqlParameter("@EF_ID_Parameter", calificacionFinal.EF_ID);
            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
					 EXEC [C_Calificacion_por_MUN]  @CheckboxValues_EF ,@Umbral ,@EF_ID_Parameter, @CheckboxValues_MUN, @UmbralMunicipio",
                      new
                      {
                          CheckboxValues_EF = checkboxValuesString_EF,
                          Umbral = calificacionFinal.Umbral_Seleccionado,
                          EF_ID_Parameter = calificacionFinal.EF_ID,
                          CheckboxValues_MUN = checkboxValuesString_MUN,
                          UmbralMunicipio = calificacionFinal.Umbral_Seleccionado_Municipio
                      });

            return detalle;
        }



        /// <summary>
        /// Este es el Stor que devuelve la Calificación Final a nivel Resumen
        /// </summary>
        /// <param name="calificacionFinal por Municipio"></param>
        /// <returns></returns>

        public async Task<IEnumerable<CalificacionFinal>> B_Calificacion_Total_por_MUN(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Asignando los Parametroos
            var checkboxValues_EF = calificacionFinal.Indicadores_Seleccionados;
            var checkboxValues_MUN = calificacionFinal.Indicadores_Seleccionados_Municipio;
            var checkboxValuesString_EF = string.Join(",", checkboxValues_EF);
            var checkboxValuesString_MUN = string.Join(",", checkboxValues_MUN);
            var umbralParameter_EF = new SqlParameter("@Umbral", calificacionFinal.Umbral_Seleccionado);
            var umbralParameter_MUN = new SqlParameter("@UmbralMunicipio", calificacionFinal.Umbral_Seleccionado_Municipio);
            var efid = new SqlParameter("@EF_ID_Parameter", calificacionFinal.EF_ID);
            var detalle = await connection.QueryAsync<CalificacionFinal>(
                @"
					 EXEC [B_Calificacion_Total_por_MUN]  @CheckboxValues_EF ,@Umbral ,@EF_ID_Parameter, @CheckboxValues_MUN, @UmbralMunicipio",
                      new
                      {
                          CheckboxValues_EF = checkboxValuesString_EF,
                          Umbral = calificacionFinal.Umbral_Seleccionado,
                          EF_ID_Parameter = calificacionFinal.EF_ID,
                          CheckboxValues_MUN = checkboxValuesString_MUN,
                          UmbralMunicipio = calificacionFinal.Umbral_Seleccionado_Municipio
                      });

            return detalle;
        }



        //Detalle por Municipio
        public async Task<DetalleCalificacion_EF> Detalle_MUN(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            var umbralParameter = new SqlParameter("@UmbralMunicipio", calificacionFinal.Umbral_Seleccionado_Municipio);
            var idefParameter = new SqlParameter("@EF_ID", calificacionFinal.efId);
            var id_municipio = new SqlParameter("@MPO_ID_Parameter", calificacionFinal.MPO_ID);
            var umbral_ef = new SqlParameter(" @Umbral", calificacionFinal.Umbral_P1);
            var checkboxValues_EF = calificacionFinal.ColumnasSeleccionadas_P1;
            var checkboxValuesString_EF = string.Join(",", checkboxValues_EF);
            var checkboxValues = calificacionFinal.Indicadores_Seleccionados_Municipio;
            var checkboxValuesString = string.Join(",", checkboxValues.Select(x => x.ToString()));

            //(var Parameter = new SqlParameter("@EF_ID", calificacionFinal.efId)

            //            EXEC D_Detalle_Calificacion_por_EF @CheckboxValues, @Umbral, @EF_ID;
            //            EXEC [dbo].[CAL_XEF] @CheckboxValues, @Umbral, @EF_ID;
            //            EXEC B_Calificacion_Total_por_EF @CheckboxValues, @Umbral, @EF_ID;
            //EXEC D_Detalle_Calificacion_por_MUNEF @CheckboxValues, @Umbral, @EF_ID;
            //EXEC [dbo].[ObtieneExpendiosporEFID] @EF_ID;

            var query = @"
				EXEC [dbo].[E_Calificacion_Solicitudes_MUN]  @CheckboxValues_EF,@Umbral,@EF_ID, @CheckboxValues, @UmbralMunicipio, @MPO_ID_Parameter;
				EXEC [dbo].[ObtieneExpendiosporEFID] @EF_ID;
					";

            using var multi = await connection.QueryMultipleAsync(query, new
            {
                CheckboxValues_EF = checkboxValuesString_EF,
                Umbral = calificacionFinal.Umbral_P1,
                EF_ID = calificacionFinal.efId,
                CheckboxValues = checkboxValuesString,
                UmbralMunicipio = calificacionFinal.Umbral_Seleccionado_Municipio,
                MPO_ID_Parameter = calificacionFinal.MPO_ID
            });

            var detalle_municipio = multi.Read<CalificacionFinal_EF>(); // Lee los resultados del primer stored procedure
            var resultadoAutorizados = multi.Read<CalificacionFinal_EF>(); // Lee los resultados del segundo stored procedure
            //var resultadoSegundoSP = multi.Read<CalificacionFinal_EF>(); // Lee los resultados del segundo stored procedure
            //var resultadoB_Calificacion_Total_por_EF = multi.Read<Parametros_EF>(); // Lee los resultados del segundo stored procedure
            //var resultadoD_Detalle_Calificacion_por_MUNEF = multi.Read<DetalleCalificacionEF_Municipio>(); // Lee los resultados del segundo stored procedure

            var detalleCalificacion = new DetalleCalificacion_EF
            {
                Detalle_MUN = detalle_municipio,
                Permisos_Autorizados = resultadoAutorizados
                //Totales_EF = detalle,
                //Parametros_EF = resultadoB_Calificacion_Total_por_EF,
                //Totales_EF_MUN = resultadoD_Detalle_Calificacion_por_MUNEF,
            };

            return detalleCalificacion;
        }

        #endregion











        #region PRE-Evaluación de la Valificación Pública de Solicitudes de Expendio al Público Petrolíferos
        //public async Task<IEnumerable<CalificacionFinal>> PRE_Calificacion_Solicitud_Pública(CalificacionFinal calificacionFinal)
        //{
        //    using var connection = new SqlConnection(connectionString);

        //    // Implementa aquí la lógica para obtener los parámetros de la vista al stored procedure
        //    var parameters = new
        //    {
        //        cve_ent = calificacionFinal.cve_ent,
        //        cve_mun = calificacionFinal.cve_mun,
        //        yearSelect = calificacionFinal.yearSelect,
        //        mercadoSelect = calificacionFinal.mercadoSelect
        //    };

        //    var resultado = await connection.QueryAsync<CalificacionFinal>(
        //        "[dbo].[Evaluacion_Publica_Expendio_Petroliferos] @cve_ent, @cve_mun, @yearSelect,@mercadoSelect",
        //        parameters

        //    );

        //    return resultado;
        //}
        public async Task<IEnumerable<CalificacionFinal>> PRE_Calificacion_Solicitud_Pública(CalificacionFinal calificacionFinal)
        {
            using var connection = new SqlConnection(connectionString);

            // Implementa aquí la lógica para obtener los parámetros de la vista al stored procedure
            var parameters = new
            {
                cve_ent = calificacionFinal.cve_ent,
                cve_mun = calificacionFinal.cve_mun,
                yearSelect = calificacionFinal.yearSelect,
                mercadoSelect = calificacionFinal.mercadoSelect
            };

            IEnumerable<CalificacionFinal> resultado;

            switch (parameters.mercadoSelect.ToLower())
            {
                case "petroliferos":
                    resultado = await connection.QueryAsync<CalificacionFinal>(
                        "[dbo].[Evaluacion_Publica_Expendio_Petroliferos] @cve_ent, @cve_mun, @yearSelect",
                        parameters
                    );
                    break;

                case "gas_lp":
                    resultado = await connection.QueryAsync<CalificacionFinal>(
                      "[dbo].[Evaluacion_Publica_Expendio_GASLP] @cve_ent, @cve_mun, @yearSelect",
                        parameters
                    );
                    break;

                case "electricidad":
                    resultado = await connection.QueryAsync<CalificacionFinal>(
                        "[dbo].[Evaluacion_Publica_Expendio_Petroliferos] @cve_ent, @cve_mun, @yearSelect",
                        parameters
                    );
                    break;

                default:
                    throw new ArgumentException("Mercado no reconocido");
            }

            return resultado;
        }

        public async Task devuelveBitacoraRegistro(LogEvaluaciones datos)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                Console.WriteLine($"Id: {datos.IdLog}, fecha: {datos.FechaYHora}, pregunta: {datos.Pregunta1}, coordenada: {datos.Coordenadas}");
                var id = await connection.QueryAsync(
                    @"INSERT INTO [dbo].[LogEvaluaciones] 
                        ([IdUsuario], [FechaYHora], [Mercado], [Año], [Pregunta1], [Pregunta2], [Pregunta3], [Pregunta4], [Pregunta5], [Pregunta6], [Pregunta7], [Entidad_Federativa], [Municipio], [Coordenadas], [Evaluacion]) 
                        VALUES 
                        ( @IdUsuario, @FechaYHora, @Mercado, @Año, @Pregunta1, @Pregunta2, @Pregunta3, @Pregunta4, @Pregunta5, @Pregunta6, @Pregunta7, @Entidad_Federativa, @Municipio, @Coordenadas, @Evaluacion)",
                    new
                    {

                        IdUsuario = datos.IdUsuario,
                        FechaYHora = datos.FechaYHora,
                        Mercado = datos.Mercado,
                        Año = datos.Año,
                        Pregunta1 = datos.Pregunta1,
                        Pregunta2 = datos.Pregunta2,
                        Pregunta3 = datos.Pregunta3,
                        Pregunta4 = datos.Pregunta4,
                        Pregunta5 = datos.Pregunta5,
                        Pregunta6 = datos.Pregunta6,
                        Pregunta7 = datos.Pregunta7,
                        Entidad_Federativa = datos.Entidad_Federativa,
                        Municipio = datos.Municipio,
                        Coordenadas = datos.Coordenadas,
                        Evaluacion = datos.Evaluacion
                    }
                );
            }
            catch (Exception ex)
            {
                // Loguear la excepción para facilitar la depuración
                Console.WriteLine($"Error al insertar datos en la base de datos: {ex.Message}");
                // Puedes lanzar la excepción nuevamente si deseas propagarla hacia arriba
                throw;
            }
        }


        public async Task devuelveBitacoraRegistroCRE(LogEvaluacionesCRE datos)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                Console.WriteLine($"Id: {datos.IdLog}, fecha: {datos.FechaYHora}, pregunta: {datos.Pregunta1}, coordenada: {datos.Coordenadas}");
                var id = await connection.QueryAsync(
                    @"INSERT INTO [dbo].[LogEvaluacionesCRE] 
                        ([IdUsuario], [FechaYHora], [Mercado], [Año], [Pregunta1], [Pregunta2], [Pregunta3], [Pregunta4], [Pregunta5], [Pregunta6], [Pregunta7], [Entidad_Federativa], [Municipio], [Coordenadas], [Evaluacion], [Turno],[NumerodeExpediente]) 
                        VALUES 
                        ( @IdUsuario, @FechaYHora, @Mercado, @Año, @Pregunta1, @Pregunta2, @Pregunta3, @Pregunta4, @Pregunta5, @Pregunta6, @Pregunta7, @Entidad_Federativa, @Municipio, @Coordenadas, @Evaluacion, @Turno, @NumerodeExpediente)",
                    new
                    {

                        IdUsuario = datos.IdUsuario,
                        FechaYHora = datos.FechaYHora,
                        Mercado = datos.Mercado,
                        Año = datos.Año,
                        Pregunta1 = datos.Pregunta1,
                        Pregunta2 = datos.Pregunta2,
                        Pregunta3 = datos.Pregunta3,
                        Pregunta4 = datos.Pregunta4,
                        Pregunta5 = datos.Pregunta5,
                        Pregunta6 = datos.Pregunta6,
                        Pregunta7 = datos.Pregunta7,
                        Entidad_Federativa = datos.Entidad_Federativa,
                        Municipio = datos.Municipio,
                        Coordenadas = datos.Coordenadas,
                        Evaluacion = datos.Evaluacion,
                        Turno = datos.Turno,
                        NumerodeExpediente = datos.NumerodeExpediente
                    }
                );
            }
            catch (Exception ex)
            {
                // Loguear la excepción para facilitar la depuración
                Console.WriteLine($"Error al insertar datos en la base de datos: {ex.Message}");
                // Puedes lanzar la excepción nuevamente si deseas propagarla hacia arriba
                throw;
            }
        }

        #endregion

        //Isocrona
        public async Task<Isocrona> GetIsocronaTotales(Isocrona isocrona)
        {
            using var connection = new SqlConnection(connectionString);
            // Imprimir los valores de las coordenadas en la consola
            Console.WriteLine($"X1: {isocrona.x1}, Y1: {isocrona.y1}, X2: {isocrona.x2}, Y2: {isocrona.y2}");

            // Implementa aquí la lógica para obtener los parámetros de la vista al stored procedure
            var parameters = new
            {
                X1 = isocrona.x1,
                X2 = isocrona.x2,
                Y1 = isocrona.y1,
                Y2 = isocrona.y2

            };


            Isocrona resultado = new Isocrona();

            if (isocrona.mercadoSelect.ToLower() == "petroliferos")
            {
                using var multi = await connection.QueryMultipleAsync("[dbo].[GetIsocrona] @X1, @Y1, @X2, @Y2", parameters);

                var totales = await multi.ReadFirstOrDefaultAsync();
                if (totales != null)
                {
                    resultado.TotalAutomoviles = totales.TotalAutomoviles;
                    resultado.TotalMotos = totales.TotalMotos;
                }

                resultado.ExpendiosAutorizadosCP = (await multi.ReadAsync<ExpendioAutorizadoCP>()).ToList();
            }
            //If mas Casos GLP
            if (isocrona.mercadoSelect.ToLower() == "gas_lp")
            {
                using var multi = await connection.QueryMultipleAsync("[dbo].[GetIsocronaGLP] @X1, @Y1, @X2, @Y2", parameters);

                var totales = await multi.ReadFirstOrDefaultAsync();
                if (totales != null)
                {
                    resultado.TotalAutomoviles = totales.TotalAutomoviles;
                    resultado.TotalMotos = totales.TotalMotos;
                }

                resultado.ExpendiosAutorizadosCP = (await multi.ReadAsync<ExpendioAutorizadoCP>()).ToList();
            }
            return resultado;
        }







        //Para Cargar solo Los del aEF de Petroliferos en las Solicitud Pública
        public async Task<IEnumerable<ExpendioAutorizado>> ObtenerExpendiosAutorizadosPET_Infra_SP(ExpendioAutorizado expendioAutorizado)
        {
            using var connection = new SqlConnection(connectionString);

            var parameters = new
            {
                cve_ent = expendioAutorizado.cve_ent,
                cve_mun = expendioAutorizado.cve_mun,
                yearSelect = expendioAutorizado.yearSelect,
                mercadoSelect = expendioAutorizado.mercadoSelect
            };

            // Inicializar resultado con una lista vacía
            IEnumerable<ExpendioAutorizado> resultado = new List<ExpendioAutorizado>();

            if (parameters.mercadoSelect.ToLower() == "petroliferos")
            {
                string sqlQuery = @"
            
            SELECT
								E.[NumeroPermiso]
						  ,E.[EfId]
						  ,E.[MpoId]
						  ,E.[NumeroDeExpediente]
						  ,E.[RazonSocial]
						  ,E.[FechaOtorgamiento]
						  ,E.[LatitudGeo]
						  ,E.[LongitudGeo]
						  ,E.[CalleNumEs]
						  ,E.[ColoniaEs]
						  ,E.[CodigoPostal]
						  ,E.[Estatus]
						  ,E.[Rfc]
						  ,E.[FechaRecepcion]
						  ,E.[EstatusInstalacion]
						  ,E.[CausaSuspension]
						  ,E.[Marca]
						  ,E.[TipoPermiso]
						  ,E.[InicioVigencia]
						  ,E.[TerminoVigencia]
						  ,E.[InicioOperaciones]
						  ,E.[CapacidadAutorizadaBarriles]
						  ,E.[InversionEstimada]
						  ,E.[Productos]
						  ,E.[Comentarios]
						  ,E.[TipoPersona]
						  ,E.[NumeroDeEstacionesDeServicio]
						  ,E.[TipoDeEstacion]
						  ,E.[FechaDeAcuse]
						  ,E.[FechaEntregaEstadosFinancieros]
						  ,E.[Propietario]
						  ,E.[CapacidadMaximaDeLaBomba]
						  ,E.[CapacidadOperativaReal]
						  ,E.[ServicioDeRegadera]
						  ,E.[ServicioDeRestaurante]
						  ,E.[ServicioDeSanitario]
						  ,E.[OtrosServicios]
						  ,E.[TiendaDeConveniencia]
						  ,E.[NumeroDeModulosDespachadores]
						  ,E.[TipoDeEstacionId]
						  ,E.[TipoDePersona]
						  ,E.[TipoDePermiso]
						  ,E.[EstadoDePermiso]
						  ,E.[EstatusDeLaInstalacion]
						  ,E.[ImagenCorporativa]
						  ,E.CausaSuspencionInstalacionId
						  ,EF.EF_Nombre AS EfNombre
					  FROM [dbo].[vExpendios_autorizado_mapa]AS E 
					 LEFT OUTER JOIN [dbo].[Entidades_Federativas] AS EF ON E.[EfId] = EF.EF_ID;
                    WHERE E.EfId = @cve_ent           
            ";

                resultado = await connection.QueryAsync<ExpendioAutorizado>(sqlQuery, parameters);
            }
            // ... [manejo de otros casos si es necesario] ...

            return resultado;
        }



    }


}











