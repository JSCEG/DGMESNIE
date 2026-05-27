namespace NSIE.Models.Gestor
{
    public class GestorTema
    {
        public int TemaId { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Tema { get; set; } = string.Empty;
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

        // Semáforo calculado en repositorio/controller
        public string Semaforo { get; set; } = "gris";

        // Actividades relacionadas (calculado)
        public int TotalActividades { get; set; }
        public int ActividadesConcluidas { get; set; }
    }

    public class GestorActividad
    {
        public int ActividadId { get; set; }
        public string Clave { get; set; } = string.Empty;
        public int TemaId { get; set; }
        public string? TemaNombre { get; set; }
        public string Actividad { get; set; } = string.Empty;
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
        public int TotalTemas { get; set; }
        public int TotalActividades { get; set; }
        public int TotalConcluidas { get; set; }
        public int TotalPorVencer { get; set; }
        public int TotalVencidas { get; set; }
        public int TotalBloqueadas { get; set; }
        public double AvanceGlobal { get; set; }
        public List<GestorTema> Temas { get; set; } = [];
        public List<GestorActividad> Actividades { get; set; } = [];
    }

    public class GestorTemaForm
    {
        public int? TemaId { get; set; }
        public string Tema { get; set; } = string.Empty;
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

    public class GestorActividadForm
    {
        public int? ActividadId { get; set; }
        public int TemaId { get; set; }
        public string Actividad { get; set; } = string.Empty;
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
    }
}
