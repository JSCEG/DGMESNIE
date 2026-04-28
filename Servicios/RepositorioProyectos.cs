using NSIE.Models;

namespace NSIE.Servicios
{

    public interface IRepositorioProyectos
    {
        List<AccesoLocal> ObtenerU();
    }
    public class RepositorioProyectos : IRepositorioProyectos
    {

        public List<AccesoLocal> ObtenerU()
        {
            return new List<AccesoLocal> {
         new AccesoLocal(){
             Usuario="Javier",
             Password=1234,
             Rol=1
         },
         new AccesoLocal(){
             Usuario="Oscar",
             Password=1234,
             Rol=2
         },
         };
        }
    }
}
