using System.Collections.Generic;
using System;
namespace NSIE.Models
{

    public class RolesUsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public int Rol_ID { get; set; }
        public int Mercado_ID { get; set; }


        public int RolUsuario_Vigente { get; set; }
        public int RolUsuario_QuienRegistro { get; set; }
        public DateTime RolUsuario_FechaMod { get; set; }

        public string RolUsuario_Comentarios { get; set; }
        // Otros campos que necesites actualizar en la tabla Roles_Usuarios
    }

}
