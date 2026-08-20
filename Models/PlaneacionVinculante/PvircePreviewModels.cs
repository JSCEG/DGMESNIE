namespace NSIE.Models.PlaneacionVinculante
{
    public sealed class PvircePreviewResult
    {
        public string Archivo { get; set; } = string.Empty;
        public string HashSha256 { get; set; } = string.Empty;
        public string Hoja { get; set; } = string.Empty;
        public string RangoUsado { get; set; } = string.Empty;
        public string TituloDetectado { get; set; } = string.Empty;
        public int FilaEncabezados { get; set; }
        public int TotalFilasFisicas { get; set; }
        public int TotalRegistros { get; set; }
        public int FilasIgnoradasSinNombre { get; set; }
        public int TotalColumnas { get; set; }
        public int TotalFormulas { get; set; }
        public int FormulasConReferenciaRota { get; set; }
        public int NombresUnicos { get; set; }
        public int? HorizonteInicio { get; set; }
        public int? HorizonteFin { get; set; }
        public int? AnioMinimo { get; set; }
        public int? AnioMaximo { get; set; }
        public decimal CapacidadInstaladaMw { get; set; }
        public decimal? CapacidadInterconexionMw { get; set; }
        public decimal AdicionesMw { get; set; }
        public decimal SustitucionesMw { get; set; }
        public int FilasCapacidadDiferente { get; set; }
        public decimal? DiferenciaInstaladaInterconexionMw { get; set; }
        public bool MetadatosComentariosNormalizados { get; set; }
        public bool PuedeRegistrarComoCorte { get; set; }
        public bool PuedeMarcarseValidado { get; set; }
        public List<PvircePreviewColumn> Columnas { get; set; } = new();
        public List<PvircePreviewYearSummary> ResumenAnual { get; set; } = new();
        public List<PvircePreviewDuplicateName> NombresDuplicados { get; set; } = new();
        public List<PvircePreviewIssue> Incidencias { get; set; } = new();
        public List<PvircePreviewRow> Muestra { get; set; } = new();
    }

    public sealed class PvircePreviewColumn
    {
        public int Numero { get; set; }
        public string Letra { get; set; } = string.Empty;
        public string Encabezado { get; set; } = string.Empty;
        public string EncabezadoNormalizado { get; set; } = string.Empty;
        public int FilasConValor { get; set; }
        public int Formulas { get; set; }
    }

    public sealed class PvircePreviewYearSummary
    {
        public int Anio { get; set; }
        public int Registros { get; set; }
        public decimal CapacidadInstaladaMw { get; set; }
        public decimal AdicionesMw { get; set; }
        public decimal SustitucionesMw { get; set; }
    }

    public sealed class PvircePreviewDuplicateName
    {
        public string Nombre { get; set; } = string.Empty;
        public int Repeticiones { get; set; }
        public List<int> Filas { get; set; } = new();
    }

    public sealed class PvircePreviewIssue
    {
        public string Severidad { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public List<int> FilasEjemplo { get; set; } = new();
    }

    public sealed class PvircePreviewRow
    {
        public int Fila { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int? Anio { get; set; }
        public string Movimiento { get; set; } = string.Empty;
        public string TipoVf { get; set; } = string.Empty;
        public string Gcr { get; set; } = string.Empty;
        public string Estatus { get; set; } = string.Empty;
        public decimal? CapacidadInstaladaMw { get; set; }
        public decimal? CapacidadInterconexionMw { get; set; }
    }
}
