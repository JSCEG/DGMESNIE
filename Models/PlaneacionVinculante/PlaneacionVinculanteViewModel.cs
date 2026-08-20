using NSIE.Models;

namespace NSIE.Models.PlaneacionVinculante
{
    public sealed class PlaneacionVinculanteViewModel
    {
        public HeaderViewModel Header { get; set; } = new();
        public PlaneacionVinculanteOverview Overview { get; set; } = new();
    }

    public sealed class PlaneacionFuentesViewModel
    {
        public HeaderViewModel Header { get; set; } = new();
        public string ClaseVersion { get; set; } = "CORTE_BASE";
        public string? ErrorMessage { get; set; }
        public PvircePreviewResult? Preview { get; set; }

        public IReadOnlyList<PlaneacionVersionClassOption> ClasesVersion { get; } =
            new List<PlaneacionVersionClassOption>
            {
                new("CORTE_BASE", "Primer corte de trabajo", "Primera fotografía técnica de 2026 que se conciliará contra el PVIRCE oficial integrado al PLADESE."),
                new("BORRADOR_REVISION", "En revisión", "Corte de trabajo que todavía no constituye una publicación oficial."),
                new("PUBLICACION_OFICIAL", "PVIRCE oficial", "Versión integrada en el PLADESE publicado y respaldada por su fuente primaria."),
                new("ACTUALIZACION_OPERATIVA", "Actualización operativa", "Seguimiento posterior que no modifica la fotografía anual.")
            };
    }

    public sealed class PlaneacionVinculanteFichaViewModel
    {
        public PlaneacionVinculanteOverview Overview { get; set; } = new();
        public IReadOnlyList<PamUsuarioDestinatario> Destinatarios { get; set; } =
            Array.Empty<PamUsuarioDestinatario>();
    }

    public sealed record PlaneacionVersionClassOption(string Value, string Label, string Description);

    public sealed class PlaneacionVinculanteOverview
    {
        public DateTime FechaConsultaUtc { get; set; } = DateTime.UtcNow;
        public bool DatosOperativosDisponibles { get; set; }
        public string? AdvertenciaDatos { get; set; }

        public decimal PladeseAdicionesMw { get; set; } = 75564.000m;
        public decimal PladeseSustitucionesMw { get; set; } = 1805.000m;
        public decimal PvirceBaseMw { get; set; } = 73238.048540m;
        public decimal PvirceBaseAdicionesMw { get; set; } = 71433.020940m;
        public decimal PvirceRevisionMw { get; set; } = 81798.278540m;
        public decimal PvirceRevisionAdicionesMw { get; set; } = 79993.250940m;
        public decimal CambioPvirceMw => PvirceRevisionMw - PvirceBaseMw;

        public int PvirceBaseRegistros { get; set; } = 459;
        public int PvirceRevisionRegistros { get; set; } = 485;
        public int RegistrosAdministracion { get; set; } = 316;
        public decimal MwAdministracion { get; set; } = 43785.991m;
        public int RegistrosBaseEjecutada { get; set; } = 4;
        public decimal MwBaseEjecutada { get; set; } = 1703.700m;
        public int RegistrosLargoPlazo { get; set; } = 165;
        public decimal MwLargoPlazo { get; set; } = 36308.588m;

        public decimal PrevalenciaBase2024 { get; set; } = 55.40m;
        public decimal PrevalenciaPisoLegal { get; set; } = 54.00m;
        public decimal PrevalenciaEscenario2030 { get; set; } = 59.00m;
        public decimal EnergiaLimpiaBase2023 { get; set; } = 23.19m;
        public decimal EnergiaLimpiaBase2024 { get; set; } = 23.40m;
        public decimal MetaGeneracionLimpia2030 { get; set; } = 38.00m;
        public decimal AccesoElectricoBase2024 { get; set; } = 99.64m;
        public decimal AccesoElectricoMeta2030 { get; set; } = 99.99m;
        public int CapacidadPobrezaEnergeticaBase2024Kw { get; set; }
        public int CapacidadPobrezaEnergeticaMeta2030Kw { get; set; } = 180000;
        public decimal IntensidadEnergeticaBase2024 { get; set; } = -0.47m;
        public decimal IntensidadEnergeticaMeta2030 { get; set; } = -2.90m;

        public int CambiosAnio { get; set; } = 44;
        public int CambiosCapacidad { get; set; } = 38;
        public int FormulasCenaceRotas { get; set; } = 472;
        public int StatusMacroPendientes { get; set; } = 125;

        public int? PamProyectosVigentes { get; set; }
        public decimal? PamInversionVigenteMdp { get; set; }
        public int? RedNodos { get; set; }
        public int? RedAristas { get; set; }
        public int? SubestacionesInventario { get; set; }
        public int? ConvocatoriasActivas { get; set; }
        public decimal? ConvocatoriasMw { get; set; }
        public int? ConvocatoriasVinculadas { get; set; }
    }
}
