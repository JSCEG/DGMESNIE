namespace NSIE.Models.Gestor
{
    public class GestorActividad
    {
        public int ActividadId { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Actividad { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public string Prioridad { get; set; } = "Media";
        public string Estatus { get; set; } = "Activo";
        public int? ResponsablePrincipalId { get; set; }
        public string? ResponsableNombre { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCompromiso { get; set; }
        public int AvanceGeneral { get; set; }
        public string? LigaSharePoint { get; set; }
        public string? ComentariosEjecutivos { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; } = true;

        // Corresponsables (JOIN from Gestor_Corresponsables)
        public List<GestorUsuarioDto> Corresponsables { get; set; } = [];

        // Semáforo calculado
        public string Semaforo { get; set; } = "gris";

        // Temas relacionados (calculado)
        public int TotalTemas { get; set; }
        public int TemasConcluidos { get; set; }
    }

    public class GestorTema
    {
        public int TemaId { get; set; }
        public string Clave { get; set; } = string.Empty;
        public int ActividadId { get; set; }
        public string? ActividadNombre { get; set; }
        public string Tema { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? ResponsableId { get; set; }
        public string? ResponsableNombre { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCompromiso { get; set; }
        public string Estatus { get; set; } = "Pendiente";
        public string Prioridad { get; set; } = "Media";
        public int Avance { get; set; }
        public bool Bloqueada { get; set; }
        public string? MotivoBloqueo { get; set; }
        public string? EvidenciaUrl { get; set; }
        public string? Comentarios { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; } = true;

        // Corresponsables
        public List<GestorUsuarioDto> Corresponsables { get; set; } = [];

        // Semáforo calculado
        public string Semaforo { get; set; } = "gris";

        // Etapas de este tema
        public List<GestorEtapa> Etapas { get; set; } = [];
    }

    public class GestorEtapa
    {
        public int EtapaId { get; set; }
        public int TemaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int ResponsableId { get; set; }
        public string? ResponsableNombre { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCompromiso { get; set; }
        public int Avance { get; set; }
        public string Estatus { get; set; } = "Pendiente";
        public int Orden { get; set; }
        public List<GestorUsuarioDto> Corresponsables { get; set; } = [];
    }

    public class GestorUsuarioDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Correo { get; set; }
        public string? Cargo { get; set; }
    }

    public class GestorDashboardVM
    {
        public int TotalActividades { get; set; }
        public int TotalTemas { get; set; }
        public int TotalConcluidos { get; set; }
        public int TotalPorVencer { get; set; }
        public int TotalVencidos { get; set; }
        public int TotalBloqueados { get; set; }
        public double AvanceGlobal { get; set; }
        public List<GestorActividad> Actividades { get; set; } = [];
        public List<GestorTema> Temas { get; set; } = [];
    }

    public class GestorActividadForm
    {
        public int? ActividadId { get; set; }
        public string Actividad { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public string Prioridad { get; set; } = "Media";
        public string Estatus { get; set; } = "Activo";
        public int? ResponsablePrincipalId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCompromiso { get; set; }
        public string? LigaSharePoint { get; set; }
        public string? ComentariosEjecutivos { get; set; }
        public List<int> CorresponsablesIds { get; set; } = [];
    }

    public class GestorTemaForm
    {
        public int? TemaId { get; set; }
        public int ActividadId { get; set; }
        public string Tema { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? ResponsableId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCompromiso { get; set; }
        public string Estatus { get; set; } = "Pendiente";
        public string Prioridad { get; set; } = "Media";
        public int Avance { get; set; }
        public bool Bloqueada { get; set; }
        public string? MotivoBloqueo { get; set; }
        public string? EvidenciaUrl { get; set; }
        public string? Comentarios { get; set; }
        public List<int> CorresponsablesIds { get; set; } = [];
        public List<int> NotificarUsuariosIds { get; set; } = [];

        // Etapas de este tema
        public List<GestorEtapaForm> Etapas { get; set; } = [];
    }

    public class GestorEtapaForm
    {
        public int? EtapaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int ResponsableId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCompromiso { get; set; }
        public int Avance { get; set; }
        public string Estatus { get; set; } = "Pendiente";
        public int Orden { get; set; }
        public List<int> CorresponsablesIds { get; set; } = [];
    }
}
