using Microsoft.Data.SqlClient;

namespace NSIE.Models
{

    public class ListaModel

    {

        //Listado de Usuarios
        public List<ListaUsuarios> ListaU = new();

        public void GetOn()
        {
            try
            { //Se colecta de la BD
                string connectionString = "Data Source=DESKTOP-9F04CH6;Initial Catalog=cre-db-2;Integrated Security=True";
                using SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string sql = "SELECT [id],[Usuario],[Email],[EmailNormalizado],[PasswordHash] FROM[cre - db - 2].[dbo].[Usuarios]";
                SqlCommand cmd = new SqlCommand(sql, connection);
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            ListaUsuarios usuarios = new();



                            usuarios.Usuario = reader.GetString(1);
                            usuarios.Email = reader.GetString(2);
                            usuarios.EmailNormalizado = reader.GetString(3);
                            usuarios.PasswordHash = reader.GetString(4);

                            ListaU.Add(usuarios);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //
            }
        }

    }


    public class ListaUsuarios
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Email { get; set; }
        public string EmailNormalizado { get; set; }
        public string PasswordHash { get; set; }
    }
}
