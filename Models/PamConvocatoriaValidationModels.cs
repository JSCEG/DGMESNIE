namespace NSIE.Models;

public static class PamConvocatoriaValidationDecisions
{
    public const string Confirmada = "confirmada";
    public const string Rechazada = "rechazada";
    public const string FaltanteConfirmada = "faltante_confirmada";
    public const string Pendiente = "pendiente";

    public static readonly IReadOnlySet<string> Permitidas =
        new HashSet<string>(
            new[]
            {
                Confirmada,
                Rechazada,
                FaltanteConfirmada,
                Pendiente
            },
            StringComparer.Ordinal);
}

public sealed class PamConvocatoriaValidationCommand
{
    public string CandidatoId { get; init; } = string.Empty;
    public string TipoElemento { get; init; } = string.Empty;
    public string Decision { get; init; } = string.Empty;
    public string Observacion { get; init; } = string.Empty;
}

public sealed class PamConvocatoriaValidationActor
{
    public int? UsuarioId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Unidad { get; init; } = string.Empty;
}

public sealed class PamConvocatoriaValidationRecord
{
    public long ValidacionId { get; init; }
    public string CandidatoId { get; init; } = string.Empty;
    public string TipoElemento { get; init; } = string.Empty;
    public string Decision { get; init; } = PamConvocatoriaValidationDecisions.Pendiente;
    public string NombreDeclarado { get; init; } = string.Empty;
    public string ClaveCatalogo { get; init; } = string.Empty;
    public string CoincidenciaCatalogo { get; init; } = string.Empty;
    public int Puntaje { get; init; }
    public double? TensionKv { get; init; }
    public string Gcr { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public string Observacion { get; init; } = string.Empty;
    public string Fuente { get; init; } = string.Empty;
    public IReadOnlyList<string> Folios { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Evidencias { get; init; } = Array.Empty<string>();
    public int? UsuarioId { get; init; }
    public string UsuarioNombre { get; init; } = string.Empty;
    public string UsuarioUnidad { get; init; } = string.Empty;
    public DateTime FechaDecisionUtc { get; init; }
    public bool Vigente { get; init; }
}

public sealed class PamConvocatoriaValidationSnapshot
{
    public DateTime GeneradoUtc { get; init; } = DateTime.UtcNow;
    public int Confirmadas { get; init; }
    public int Rechazadas { get; init; }
    public int FaltantesConfirmadas { get; init; }
    public int PendientesRegistradas { get; init; }
    public IReadOnlyList<PamConvocatoriaValidationRecord> Validaciones { get; init; } =
        Array.Empty<PamConvocatoriaValidationRecord>();
}

public sealed class PamConvocatoriaBulkValidationResult
{
    public DateTime EjecutadoUtc { get; init; } = DateTime.UtcNow;
    public string TipoElemento { get; init; } = string.Empty;
    public int CandidatosEvaluados { get; init; }
    public int CoincidenciasFirmes { get; init; }
    public int ConfirmadasNuevas { get; init; }
    public int OmitidasConDecision { get; init; }
    public IReadOnlyList<PamConvocatoriaValidationRecord> Validaciones { get; init; } =
        Array.Empty<PamConvocatoriaValidationRecord>();
}
