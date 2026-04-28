using NSIE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSIE.Servicios.Interfaces
{
    public interface IRepositorioHome
    {
        Task<List<SeccionSNIER>> ObtenerSeccionesConModulos();
        Task<List<ModuloSNIER>> ObtenerModulosPorSeccion(int seccionId);
        Task<List<SeccionSNIER>> ObtenerSeccionesConModulosPorRol(string rolUsuario);
    }
}