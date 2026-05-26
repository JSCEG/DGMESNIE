using System;
using System.Collections.Generic;

namespace NSIE.Models.Gestor
{
    public class GrupoInteres
    {
        public int GrupoInteresId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Origen { get; set; }
        public string? Contacto { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }
        public string? CreadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        public string? ActualizadoPor { get; set; }

        // Relación: Proyectos/Empresas hijas
        public List<GrupoInteresProyecto> Proyectos { get; set; } = [];
    }

    public class GrupoInteresProyecto
    {
        public int ProyectoId { get; set; }
        public int GrupoInteresId { get; set; }
        public string? RazonSocial { get; set; }
        public string? NombreProyecto { get; set; }
        public string? Contacto { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }
        public string? CreadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        public string? ActualizadoPor { get; set; }
    }

    public class GrupoInteresDto
    {
        public int GrupoInteresId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Origen { get; set; }
        public string? Contacto { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public int TotalProyectos { get; set; }
    }

    public class GrupoInteresForm
    {
        public int? GrupoInteresId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Origen { get; set; }
        public string? Contacto { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        
        // Proyectos hijos enviados desde el formulario
        public List<GrupoInteresProyectoForm> Proyectos { get; set; } = [];
    }

    public class GrupoInteresProyectoForm
    {
        public int? ProyectoId { get; set; }
        public string? RazonSocial { get; set; }
        public string? NombreProyecto { get; set; }
        public string? Contacto { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
    }

    public class GruposInteresDashboardVM
    {
        public int TotalGrupos { get; set; }
        public int TotalProyectos { get; set; }
        public int TotalPaises { get; set; }
        public List<PaisDistribucionDto> PaisesDistribucion { get; set; } = [];
        public List<GrupoInteresDto> TopGruposProyectos { get; set; } = [];
    }

    public class PaisDistribucionDto
    {
        public string Pais { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
