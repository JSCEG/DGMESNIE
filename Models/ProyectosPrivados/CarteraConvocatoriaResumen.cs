namespace NSIE.Models.ProyectosPrivados;

/// <summary>Ficha ejecutiva de la cartera Mixtos II: universo registrado y agregados de los proyectos firmes.</summary>
public sealed class CarteraConvocatoriaResumenViewModel
{
    public string SourceVersion { get; set; } = "";
    public CarteraConvocatoriaCarga? LatestImport { get; set; }
    public DateTime? CutoffDate { get; set; }
    public List<NSIE.Models.PamUsuarioDestinatario> Recipients { get; set; } = new();

    // Universo registrado
    public int Total { get; set; }
    public int Firm { get; set; }
    public int Review { get; set; }
    public int Rejected { get; set; }
    public decimal MwUniverso { get; set; }
    public decimal MwFirme { get; set; }
    public List<ResumenConteo> EstatusUniverso { get; set; } = new();
    public List<ResumenConteo> Origenes { get; set; } = new();
    public int Preseleccionados { get; set; }
    public int AntecedenteMixtosI { get; set; }
    public int AntecedenteCvp2 { get; set; }
    public int Consorcios { get; set; }

    // Firmes por gerencia, tecnología y modalidad
    public List<ResumenGcr> Gcr { get; set; } = new();
    public List<ResumenConteo> Tecnologias { get; set; } = new();
    public List<ResumenConteo> Modalidades { get; set; } = new();
    public List<ResumenConteo> GruposInteres { get; set; } = new();
    public List<ResumenConteo> Tensiones { get; set; } = new();
    public List<ResumenConteo> Subestaciones { get; set; } = new();
    public List<ResumenConteo> Entidades { get; set; } = new();

    // Entradas en operación (año → proyectos, MW, MW por GCR)
    public List<ResumenAnio> Operacion { get; set; } = new();
    public int PorEtapas { get; set; }

    // Trámites y estudios
    public List<ResumenConteo> ValidacionCenace { get; set; } = new();
    public List<ResumenConteo> Permisos { get; set; } = new();
    public List<ResumenConteo> Pagos { get; set; } = new();
    public List<ResumenConteo> Registro { get; set; } = new();
    public List<ResumenConteo> SolicitudPermiso { get; set; } = new();
    public int ConEstudiosInterconexion { get; set; }

    // Almacenamiento
    public int ConSae { get; set; }
    public decimal SaeMw { get; set; }
    public decimal SaeMwh { get; set; }

    // Costos
    public int ConFichaCfe { get; set; }
    public decimal CostoRedMdd { get; set; }
    public decimal CostoRedMdp { get; set; }
    public decimal MwConFichaCfe { get; set; }
    public int Obras { get; set; }
    public decimal? TipoCambio { get; set; }
    public int ConInversionDeclarada { get; set; }
    public decimal InversionDeclarada { get; set; }
    public int ConCapex { get; set; }
    public decimal CapexTotal { get; set; }
    public decimal CapexSae { get; set; }

    // Evaluaciones
    public int ConEvaluacionSocial { get; set; }
    public int ConFactibilidad { get; set; }
    public int ConInteresCfe { get; set; }
    public List<ResumenConteo> RiesgoSocial { get; set; } = new();
    public List<ResumenConteo> EstatusEvis { get; set; } = new();
    public List<ResumenConteo> EstatusMia { get; set; } = new();
    public List<ResumenConteo> Factibilidad { get; set; } = new();
    public List<ResumenConteo> Clasificacion { get; set; } = new();

    // Clúster y excluyentes (marcas CFE)
    public bool MarcasDisponibles { get; set; }
    public int ConCluster { get; set; }
    public int ConExcluyente { get; set; }
    public List<ResumenConteo> Clusters { get; set; } = new();

    public List<ResumenProyecto> TopProyectos { get; set; } = new();
    public List<CarteraConvocatoriaRequerimientoSistema> Requerimientos { get; set; } = new();
    // Listas para revisión: válidos no considerados, riesgo social alto e híbridos.
    public List<ResumenProyecto> ValidosNoConsiderados { get; set; } = new();
    public List<ResumenProyecto> RiesgoSocialAlto { get; set; } = new();
    public List<ResumenProyecto> Hibridos { get; set; } = new();
    // Complementos visuales: grupos excluyentes, inversión por tecnología y listas top para llenar las láminas.
    public List<ResumenConteo> GruposExcluyentes { get; set; } = new();
    public List<ResumenConteo> InversionPorTecnologia { get; set; } = new();
    public List<ResumenProyecto> TopInversion { get; set; } = new();
    public List<ResumenProyecto> TopCostoRed { get; set; } = new();
}

public sealed class ResumenConteo
{
    public string Name { get; set; } = "";
    public int Projects { get; set; }
    public decimal Mw { get; set; }
    public decimal Amount { get; set; }
    public string Detail { get; set; } = "";
}

public sealed class ResumenGcr
{
    public string Region { get; set; } = "";
    public int Universo { get; set; }
    public int Projects { get; set; }
    public decimal Mw { get; set; }
    public decimal Share { get; set; }
    public Dictionary<string, decimal> MwPorTecnologia { get; set; } = new();
    public Dictionary<string, decimal> MwPorAnio { get; set; } = new();
    public int ConFichaCfe { get; set; }
    public decimal CostoMdd { get; set; }
    public decimal CostoMdp { get; set; }
    public int Obras { get; set; }
    public int ConSocial { get; set; }
    public int ConFactibilidad { get; set; }
    public int ConInteres { get; set; }
    public int ConCluster { get; set; }
    public int ConSae { get; set; }
    public decimal SaeMw { get; set; }
    public decimal InversionDeclarada { get; set; }
    public List<ResumenConteo> Modalidades { get; set; } = new();
}

public sealed class ResumenAnio
{
    public string Anio { get; set; } = "";
    public int Projects { get; set; }
    public decimal Mw { get; set; }
    public Dictionary<string, decimal> MwPorGcr { get; set; } = new();
    public Dictionary<string, int> ProyectosPorGcr { get; set; } = new();
}

public sealed class ResumenProyecto
{
    public string Folio { get; set; } = "";
    public string Name { get; set; } = "";
    public string Region { get; set; } = "";
    public string Technology { get; set; } = "";
    public decimal Mw { get; set; }
    public string Operacion { get; set; } = "";
    public bool FichaCfe { get; set; }
    public decimal? CostoMdd { get; set; }
    public string? Cluster { get; set; }
    public string Factibilidad { get; set; } = "";
    public string Modalidad { get; set; } = "";
    public string Sae { get; set; } = "";
    public string Entidad { get; set; } = "";
    public string Estatus { get; set; } = "";
    public string RiesgoSocial { get; set; } = "";
    public string EstatusEvis { get; set; } = "";
    public string EstatusMia { get; set; } = "";
    public string RiesgoAmbiental { get; set; } = "";
    public string Observacion { get; set; } = "";
    // Verificación del cruce entre la evaluación social (homologada por nombre) y la solicitud del folio.
    public string Cruce { get; set; } = "";
    // Antecedentes del expediente para los registros no incluidos (observación del área, homologación, duplicidad, evaluaciones).
    public string Antecedentes { get; set; } = "";
    public decimal? Inversion { get; set; }
}
