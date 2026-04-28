
using System;
using System.Collections.Generic;

namespace NSIE.Models
{
    public class ProyectoFOTEASE
    {
        public class EstadisticasProyectos
        {
            public int ProyectosTerminados { get; set; }
            public int ProyectosEnCurso { get; set; }
            public int ProyectosCancelados { get; set; }

            public int Total => ProyectosTerminados + ProyectosEnCurso + ProyectosCancelados;

            public int PorcentajeTerminados => Total > 0 ? (ProyectosTerminados * 100 / Total) : 0;
            public int PorcentajeEnCurso => Total > 0 ? (ProyectosEnCurso * 100 / Total) : 0;
            public int PorcentajeCancelados => Total > 0 ? (ProyectosCancelados * 100 / Total) : 0;
        }

        public class ProyectoGantt
        {
            public string Id { get; set; }
            public string Nombre { get; set; }
            public DateTime Inicio { get; set; }
            public DateTime Fin { get; set; }
            public double Progreso { get; set; } // De 0 a 1
            public string[] Dependencias { get; set; } = new string[0];
        }

    }
    public class DashboardViewModel
    {
        public ProyectoFOTEASE.EstadisticasProyectos Estadisticas { get; set; }
        public List<ProyectoFOTEASE.ProyectoGantt> ProyectosGantt { get; set; }
        public string NombreUsuario { get; set; }
        public string RolUsuario { get; set; }
        public int IdUsuario { get; set; }
    }
}