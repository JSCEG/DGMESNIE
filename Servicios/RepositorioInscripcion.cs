using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using NSIE.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace NSIE.Servicios
{
    public interface IRepositorioInscripcion
    {
        Task<List<ModalidadInscripcion>> ObtenerModalidadesAsync();
    }

    //El Select SCOPE IDENTITY devuelve el ID impactado en la BD
    public class RepositorioInscripcion : IRepositorioInscripcion
    {

        //Cadena de Conexión a BD
        private readonly string connectionString; //Variable en la que se coloca el valor del CS por default desde el proveedor de configuraciones
        public RepositorioInscripcion(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ModalidadInscripcion>> ObtenerModalidadesAsync()
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = "SELECT Nombre_Modalidad, Categoria FROM ModalidadesInscripcion";
                var modalidades = await cn.QueryAsync<ModalidadInscripcion>(query);
                return modalidades.AsList();
            }
        }

    }

}
