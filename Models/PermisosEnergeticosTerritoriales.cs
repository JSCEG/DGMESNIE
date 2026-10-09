namespace NSIE.Models;

public sealed class PermisoEnergeticoRegistro
{
    public string Nombre { get; init; } = string.Empty;
    public string NumeroPermiso { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public string Estatus { get; init; } = string.Empty;
    public string TipoPermiso { get; init; } = string.Empty;
    public string Tecnologia { get; init; } = string.Empty;
    public string Clasificacion { get; init; } = string.Empty;
    public double? Capacidad { get; init; }
    public string UnidadCapacidad { get; init; } = string.Empty;
    public string CapacidadTexto { get; init; } = string.Empty;
    public DateTime? FechaOtorgamiento { get; init; }
    public DateTime? FechaOperacion { get; init; }
    public double Latitud { get; init; }
    public double Longitud { get; init; }
    public bool EsVigente { get; init; }
    public string Comentarios { get; init; } = string.Empty;
    public string EntidadNombre { get; init; } = string.Empty;
    public string MunicipioNombre { get; init; } = string.Empty;
}

public sealed class PermisosEnergeticosDatos
{
    public required string Tipo { get; init; }
    public required string Mercado { get; init; }
    public required string Fuente { get; init; }
    public DateTime? FechaCorte { get; init; }
    public int SinUbicacion { get; init; }
    public required IReadOnlyList<PermisoEnergeticoRegistro> Registros { get; init; }
}

public sealed class PermisosEnergeticosGeoJson
{
    public string Type { get; init; } = "FeatureCollection";
    public required PermisosEnergeticosMeta Meta { get; init; }
    public required IReadOnlyList<PermisoEnergeticoFeature> Features { get; init; }
}

public sealed class PermisosEnergeticosMeta
{
    public required string Tipo { get; init; }
    public required string Mercado { get; init; }
    public required string Fuente { get; init; }
    public DateTime? FechaCorte { get; init; }
    public int Conteo { get; init; }
    public int SinUbicacion { get; init; }
}

public sealed class PermisoEnergeticoFeature
{
    public string Type { get; init; } = "Feature";
    public required PermisoEnergeticoPoint Geometry { get; init; }
    public required PermisoEnergeticoProperties Properties { get; init; }
}

public sealed class PermisoEnergeticoPoint
{
    public string Type { get; init; } = "Point";
    public required double[] Coordinates { get; init; }
}

public sealed class PermisoEnergeticoProperties
{
    public required string Nombre { get; init; }
    public required string NumeroPermiso { get; init; }
    public required string Mercado { get; init; }
    public required string Entidad { get; init; }
    public required string Municipio { get; init; }
    public required string Estatus { get; init; }
    public required string TipoPermiso { get; init; }
    public required string Tecnologia { get; init; }
    public required string Clasificacion { get; init; }
    public double? Capacidad { get; init; }
    public required string UnidadCapacidad { get; init; }
    public required string CapacidadTexto { get; init; }
    public DateTime? FechaOtorgamiento { get; init; }
    public DateTime? FechaOperacion { get; init; }
    public required string Fuente { get; init; }
    public bool Vigente { get; init; } = true;
    // exacta | calle | municipio (según Comentarios de la carga CNE)
    public string Precision { get; init; } = "exacta";
    public string EntidadNombre { get; init; } = string.Empty;
    public string MunicipioNombre { get; init; } = string.Empty;
}

public sealed class PermisoEnergeticoDetalle
{
    public required string Tipo { get; init; }
    public required string Mercado { get; init; }
    public required string Fuente { get; init; }
    public required string NumeroPermiso { get; init; }
    public required string Nombre { get; init; }
    public DateTime? FechaCorte { get; init; }
    public DateTime FechaConsulta { get; init; }
    public required string NivelAcceso { get; init; }
    public bool EsDetalleInstitucional { get; init; }
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public string Estatus { get; init; } = string.Empty;
    public double? Latitud { get; init; }
    public double? Longitud { get; init; }
    public required IReadOnlyList<PermisoEnergeticoDetalleCampo> Campos { get; init; }
}

public sealed class PermisoEnergeticoDetalleCampo
{
    public required string Clave { get; init; }
    public required string Etiqueta { get; init; }
    public required string Categoria { get; init; }
    public required string Valor { get; init; }
}

public sealed record PermisoEnergeticoContextoAcceso(
    bool EsInstitucional,
    int RolId,
    int MercadoId,
    string NivelAcceso)
{
    public static PermisoEnergeticoContextoAcceso Publico { get; } =
        new(false, 0, 0, "Consulta externa");
}

public sealed class PermisoEnergeticoFichaViewModel
{
    public required PermisoEnergeticoDetalle Detalle { get; init; }
    public List<PamUsuarioDestinatario> Destinatarios { get; init; } = new();
}

public sealed class PermisoEnergeticoEnviarFichaInput
{
    public string Tipo { get; set; } = string.Empty;
    public string NumeroPermiso { get; set; } = string.Empty;
    public string Formato { get; set; } = "pdf";
    public string ArchivoBase64 { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public List<int> UsuarioIds { get; set; } = new();
    public string MensajeAdicional { get; set; } = string.Empty;
}

public sealed class PermisoEnergeticoBusquedaItem
{
    public string Tipo { get; init; } = string.Empty;
    public string NumeroPermiso { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public string TipoPermiso { get; init; } = string.Empty;
    public string Estatus { get; init; } = string.Empty;
    public double? Latitud { get; init; }
    public double? Longitud { get; init; }
    public bool Vigente { get; init; } = true;
}

public sealed class PermisoEnergeticoBusquedaConteo
{
    public string Tipo { get; init; } = string.Empty;
    public int Total { get; init; }
}

public sealed class PermisosEnergeticosBusqueda
{
    public required string Termino { get; init; }
    public int Total { get; init; }
    public required IReadOnlyList<PermisoEnergeticoBusquedaConteo> PorTipo { get; init; }
    public required IReadOnlyList<PermisoEnergeticoBusquedaItem> Resultados { get; init; }
}

public sealed class PermisoSinUbicacion
{
    public string NumeroPermiso { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public string Estatus { get; init; } = string.Empty;
    public bool EsVigente { get; init; }
}

public sealed class ActualizacionPermisosCne
{
    public bool Disponible { get; init; }
    public DateTime? General { get; init; }
    public string Fuente { get; init; } = "CNE";
    public Dictionary<string, DateTime?> PorTabla { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
