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
}

public sealed class PermisosEnergeticosDatos
{
    public required string Tipo { get; init; }
    public required string Mercado { get; init; }
    public required string Fuente { get; init; }
    public DateTime? FechaCorte { get; init; }
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
}

public sealed class PermisoEnergeticoDetalle
{
    public required string Tipo { get; init; }
    public required string Mercado { get; init; }
    public required string Fuente { get; init; }
    public required string NumeroPermiso { get; init; }
    public required string Nombre { get; init; }
    public required IReadOnlyList<PermisoEnergeticoDetalleCampo> Campos { get; init; }
}

public sealed class PermisoEnergeticoDetalleCampo
{
    public required string Clave { get; init; }
    public required string Etiqueta { get; init; }
    public required string Valor { get; init; }
}
