namespace NSIE.Models
{
    /// <summary>
    /// Semáforo de licitación acordado en la minuta 2026-07-09:
    /// cinco estatus con color institucional + "Por clasificar" para NULL.
    /// </summary>
    public static class PamSemaforo
    {
        public static readonly string[] Orden =
        {
            "En operación", "En ejecución", "Concursado", "En concurso", "Por concursar", "Sin iniciar", "Por clasificar"
        };

        public static string Clase(string estatus) => (estatus ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "en operación" => "is-enoperacion",
            "en ejecución" => "is-ejecucion",
            "concursado" => "is-concursado",
            "en concurso" => "is-enconcurso",
            "por concursar" => "is-porconcursar",
            "sin iniciar" => "is-sininiciar",
            _ => "is-porclasificar"
        };
    }

    /// <summary>
    /// Interpreta las fechas de la cartera PAM en sus múltiples formatos:
    /// ISO (2026-03-15), mes-año (dic-27), día-mes-año (30-dic-27), y textos
    /// multietapa ("Etapa 1: sep-27 Etapa 2: jul-27") de los que toma la más tardía.
    /// Devuelve null para textos no fechables ("Pausado", "Por definir").
    /// </summary>
    public static class PamFechaParser
    {
        private static readonly Dictionary<string, int> Meses = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ene"] = 1, ["enero"] = 1, ["feb"] = 2, ["febrero"] = 2, ["mar"] = 3, ["marzo"] = 3,
            ["abr"] = 4, ["abril"] = 4, ["may"] = 5, ["mayo"] = 5, ["jun"] = 6, ["junio"] = 6,
            ["jul"] = 7, ["julio"] = 7, ["ago"] = 8, ["agosto"] = 8, ["sep"] = 9, ["sept"] = 9, ["septiembre"] = 9,
            ["oct"] = 10, ["octubre"] = 10, ["nov"] = 11, ["noviembre"] = 11, ["dic"] = 12, ["diciembre"] = 12
        };

        /// <summary>Última fecha reconocible del texto (la relevante cuando el proyecto completo está listo).</summary>
        public static DateTime? Parsear(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;
            var t = System.Text.RegularExpressions.Regex.Replace(texto, @"[\r\n\t]+", " ").ToLowerInvariant();
            DateTime? maxima = null;

            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(t, @"(\d{4})-(\d{1,2})-(\d{1,2})"))
                Acumular(ref maxima, Construir(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value)));

            // día-mes-año o mes-año con mes textual: 30-dic-27, dic-27, dic/27, dic de 2027
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                t, @"(?:(\d{1,2})[\s/\-]+)?([a-záéí]{3,10})[\s/\-]+(?:de[\s/\-]+)?(\d{2,4})"))
            {
                if (!Meses.TryGetValue(m.Groups[2].Value, out var mes)) continue;
                var anio = int.Parse(m.Groups[3].Value);
                if (anio < 100) anio += 2000;
                var dia = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 1;
                Acumular(ref maxima, Construir(anio, mes, dia));
            }

            return maxima;
        }

        private static DateTime? Construir(int anio, int mes, int dia)
        {
            if (anio is < 2000 or > 2100 || mes is < 1 or > 12) return null;
            dia = Math.Clamp(dia, 1, DateTime.DaysInMonth(anio, mes));
            return new DateTime(anio, mes, dia);
        }

        private static void Acumular(ref DateTime? maxima, DateTime? candidata)
        {
            if (candidata.HasValue && (!maxima.HasValue || candidata.Value > maxima.Value))
                maxima = candidata;
        }
    }

    /// <summary>
    /// Empalme fecha necesaria ↔ FEO factible de un proyecto de transmisión (minuta 2026-07-09).
    /// Retraso de la red frente a cuando se necesita = riesgo de vertimientos de generación.
    /// </summary>
    public class PamEmpalme
    {
        public string FechaNecesariaTexto { get; set; }
        public string FeoFactibleTexto { get; set; }
        public DateTime? FechaNecesaria { get; set; }
        public DateTime? FeoFactible { get; set; }

        public bool Evaluable => FechaNecesaria.HasValue && FeoFactible.HasValue;
        public int DeltaMeses => Evaluable
            ? (FeoFactible.Value.Year - FechaNecesaria.Value.Year) * 12 + (FeoFactible.Value.Month - FechaNecesaria.Value.Month)
            : 0;

        public string Nivel => !Evaluable ? "Sin evaluar"
            : DeltaMeses <= 0 ? "A tiempo"
            : DeltaMeses <= 6 ? "Holgura ajustada"
            : "Riesgo de empalme";

        public string Clase => Nivel switch
        {
            "A tiempo" => "is-ok",
            "Holgura ajustada" => "is-warn",
            "Riesgo de empalme" => "is-critico",
            _ => "is-sindato"
        };

        public string Resumen => !Evaluable
            ? "No hay fechas comparables para evaluar el empalme."
            : DeltaMeses <= 0
                ? $"La red estaría lista {Math.Abs(DeltaMeses)} mes(es) antes de necesitarse."
                : $"La red entraría {DeltaMeses} mes(es) después de la fecha necesaria.";
    }

    /// <summary>Fase constructiva detectada en la etapa del proyecto o en relaciones 'Fase de'.</summary>
    public class PamFaseProyecto
    {
        public string Etiqueta { get; set; }
        public string EstatusTexto { get; set; }
        public string EstatusSemaforo { get; set; }
        public string Feo { get; set; }
        public int? Numero { get; set; }
        public string EstatusClase => PamSemaforo.Clase(EstatusSemaforo);
    }

    /// <summary>
    /// Interpreta textos multietapa del Informe Pormenorizado
    /// ("Etapa 1: En Concurso Etapa 2: ..." o "Fases 1 y 2 en Ejecución/Construcción").
    /// Devuelve lista vacía para proyectos de fase única.
    /// </summary>
    public static class PamFasesParser
    {
        public static List<PamFaseProyecto> Desde(string etapaProyecto) => Desde(etapaProyecto, null);

        public static List<PamFaseProyecto> Desde(string etapaProyecto, string feoFactible)
        {
            var fases = new List<PamFaseProyecto>();
            if (string.IsNullOrWhiteSpace(etapaProyecto)) return fases;

            var texto = Normalizar(etapaProyecto);

            var plural = System.Text.RegularExpressions.Regex.Match(
                texto, @"^Fases\s+(\d+)\s*y\s*(\d+)\s+en\s+(.+)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (plural.Success)
            {
                var estatusPlural = plural.Groups[3].Value.Trim();
                fases.Add(Crear($"Fase {plural.Groups[1].Value}", estatusPlural, int.Parse(plural.Groups[1].Value)));
                fases.Add(Crear($"Fase {plural.Groups[2].Value}", estatusPlural, int.Parse(plural.Groups[2].Value)));
            }
            else
            {
                var marcas = System.Text.RegularExpressions.Regex.Matches(
                    texto, @"(Etapa|Fase)\s*(\d+)\s*:\s*",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (marcas.Count == 0) return fases;

                for (var i = 0; i < marcas.Count; i++)
                {
                    var inicio = marcas[i].Index + marcas[i].Length;
                    var fin = i + 1 < marcas.Count ? marcas[i + 1].Index : texto.Length;
                    var estatus = texto[inicio..fin].Trim();
                    if (string.IsNullOrWhiteSpace(estatus)) continue;
                    fases.Add(Crear($"{Capitalizar(marcas[i].Groups[1].Value)} {marcas[i].Groups[2].Value}",
                        estatus, int.Parse(marcas[i].Groups[2].Value)));
                }
            }

            AsociarFeo(fases, feoFactible);
            return fases;
        }

        /// <summary>Extrae la FEO factible por fase y la asigna a cada fase por orden o por número.</summary>
        private static void AsociarFeo(List<PamFaseProyecto> fases, string feoFactible)
        {
            if (fases.Count == 0 || string.IsNullOrWhiteSpace(feoFactible)) return;

            var texto = Normalizar(feoFactible);
            var marcas = System.Text.RegularExpressions.Regex.Matches(
                texto, @"(?:Etapa|Fase)[^:]*?:\s*",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var items = new List<(int? Numero, string Valor)>();
            for (var i = 0; i < marcas.Count; i++)
            {
                var inicio = marcas[i].Index + marcas[i].Length;
                var fin = i + 1 < marcas.Count ? marcas[i + 1].Index : texto.Length;
                var valor = texto[inicio..fin].Trim().TrimEnd(',', ';');
                if (string.IsNullOrWhiteSpace(valor)) continue;
                var num = System.Text.RegularExpressions.Regex.Match(marcas[i].Value, @"\d+");
                items.Add((num.Success ? int.Parse(num.Value) : (int?)null, valor));
            }

            if (items.Count == 0) return;

            if (items.Count == fases.Count)
            {
                for (var i = 0; i < fases.Count; i++) fases[i].Feo = items[i].Valor;
                return;
            }

            // Conteos distintos (p. ej. una fase con sub-etapas): match por número principal.
            foreach (var fase in fases)
            {
                var coincidencia = items.FirstOrDefault(x => x.Numero == fase.Numero);
                if (coincidencia.Valor != null) fase.Feo = coincidencia.Valor;
            }
        }

        private static string Normalizar(string valor)
        {
            var texto = System.Text.RegularExpressions.Regex.Replace(valor, @"[\r\n\t]+", " ");
            return System.Text.RegularExpressions.Regex.Replace(texto, @"\s{2,}", " ").Trim();
        }

        private static PamFaseProyecto Crear(string etiqueta, string estatusTexto, int? numero) => new()
        {
            Etiqueta = etiqueta,
            EstatusTexto = estatusTexto,
            EstatusSemaforo = MapearSemaforo(estatusTexto),
            Numero = numero
        };

        private static string Capitalizar(string valor) =>
            string.IsNullOrEmpty(valor) ? valor : char.ToUpperInvariant(valor[0]) + valor[1..].ToLowerInvariant();

        private static string MapearSemaforo(string estatus)
        {
            var valor = (estatus ?? string.Empty).Trim().ToLowerInvariant();
            if (valor.Contains("operaci")) return "En operación";
            if (valor.Contains("ejecuci") || valor.Contains("construcci") || valor.Contains("estudios previos")) return "En ejecución";
            if (valor.Contains("concursado")) return "Concursado";
            if (valor.Contains("en concurso")) return "En concurso";
            if (valor.Contains("por concursar") || valor.Contains("priorizaci")) return "Por concursar";
            if (valor.Contains("sin iniciar") || valor.Contains("identificado") || valor.Contains("instruido")) return "Sin iniciar";
            return "Por clasificar";
        }
    }

    public class PamrntProyectosIdentificadosViewModel
    {
        public HeaderViewModel Header { get; set; }
        public string Fuente { get; set; }
        public string VersionDatos { get; set; }
        public bool CruceBaseDisponible { get; set; }
        public string AvisoCruce { get; set; }
        public bool PuedeActualizarFuentes { get; set; }
        public List<PamrntProyectoIdentificado> Proyectos { get; set; } = new();
        public int TotalPam { get; set; }
        public int TotalPamrnt { get; set; }
        public int TotalVigentes { get; set; }
        public int TotalCancelados { get; set; }
        public int TotalRecientes { get; set; }
        public int TotalFiltrado { get; set; }
        public decimal TotalKmC { get; set; }
        public decimal TotalMva { get; set; }
        public decimal TotalMvar { get; set; }
        public decimal TotalInversionVigente { get; set; }
        public int ProyectosConMetricas { get; set; }
        public PamDashboardFiltro Filtro { get; set; } = new();
        public Dictionary<string, int> ConteosEstatus { get; set; } = new();
        public Dictionary<string, int> ConteosEmpalme { get; set; } = new();
        public int EmpalmeConteo(string nivel) => ConteosEmpalme.TryGetValue(nivel, out var n) ? n : 0;
        public List<string> EtapasDisponibles { get; set; } = new();
        public List<string> RegionesDisponibles { get; set; } = new();
        public List<string> TiposDisponibles { get; set; } = new();
        public List<string> FuentesDisponibles { get; set; } = new();

        public int TotalProyectos => Proyectos.Count;
        public int TotalIdentidades => TotalPam + TotalPamrnt;
        public decimal InversionTotalMdp => Proyectos
            .Where(x => string.Equals(x.OrigenPrograma, "PAMRNT", StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.InversionMdp ?? 0);
        public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(TotalFiltrado / (double)Filtro.TamanoPagina));
        public int RegistroDesde => TotalFiltrado == 0 ? 0 : ((Filtro.Pagina - 1) * Filtro.TamanoPagina) + 1;
        public int RegistroHasta => Math.Min(Filtro.Pagina * Filtro.TamanoPagina, TotalFiltrado);
    }

    public class PamDashboardFiltro
    {
        public string Universo { get; set; } = "vigentes";
        public string Busqueda { get; set; }
        public string Origen { get; set; }
        public string Etapa { get; set; }
        public string Region { get; set; }
        public string Tipo { get; set; }
        public string Fuente { get; set; }
        public string Estatus { get; set; }
        public string Empalme { get; set; }
        public string Equipo { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 25;

        public void Normalizar()
        {
            var universos = new[] { "vigentes", "pam", "pamrnt", "cancelados", "recientes", "todos" };
            Universo = universos.Contains(Universo?.ToLowerInvariant()) ? Universo.ToLowerInvariant() : "vigentes";
            var estatusValidos = new[] { "En operación", "En ejecución", "Concursado", "En concurso", "Por concursar", "Sin iniciar", "Por clasificar" };
            Estatus = estatusValidos.FirstOrDefault(e => string.Equals(e, Estatus?.Trim(), StringComparison.OrdinalIgnoreCase));
            var empalmeValidos = new[] { "A tiempo", "Holgura ajustada", "Riesgo de empalme", "Sin evaluar" };
            Empalme = empalmeValidos.FirstOrDefault(e => string.Equals(e, Empalme?.Trim(), StringComparison.OrdinalIgnoreCase));
            var equipoValidos = new[] { "lineas", "transformacion", "compensacion" };
            Equipo = equipoValidos.FirstOrDefault(e => string.Equals(e, Equipo?.Trim().ToLowerInvariant()));
            Pagina = Math.Max(1, Pagina);
            TamanoPagina = new[] { 10, 25, 50, 100 }.Contains(TamanoPagina) ? TamanoPagina : 25;
        }
    }

    public class PamrntProyectoIdentificado
    {
        public int? Prioridad { get; set; }
        public string Gcr { get; set; }
        public string ClavePem { get; set; }
        public string ClavesAlternas { get; set; }
        public string Proyecto { get; set; }
        public string FechaNecesaria { get; set; }
        public int EjercicioPlaneacion { get; set; }
        public string ZonaAtendida { get; set; }
        public decimal? InversionMdp { get; set; }
        public bool FichaDisponible { get; set; }
        public long ProyectoId { get; set; }
        public string TipoProyecto { get; set; }
        public string OrigenPrograma { get; set; }
        public string EtapaProyecto { get; set; }
        public string EstadoVigenciaCartera { get; set; }
        public string EstatusLicitacion { get; set; }
        public int NumeroVersion { get; set; }
        public string FuenteDocumento { get; set; }
        public DateTime? FechaCorte { get; set; }
        public string FuenteUbicacion { get; set; }
        public DateTime? UltimoCambioUtc { get; set; }
        public string ClavePadre { get; set; }
        public string NombrePadre { get; set; }
        public bool EsAltaOCambioReciente => NumeroVersion > 1 || string.Equals(OrigenPrograma, "PAMRNT", StringComparison.OrdinalIgnoreCase);
        public string EstatusSemaforo => string.IsNullOrWhiteSpace(EstatusLicitacion) ? "Por clasificar" : EstatusLicitacion.Trim();
        public string EstatusSemaforoClase => PamSemaforo.Clase(EstatusLicitacion);
    }

    public class PamImpactoRegional
    {
        public string Region { get; set; }
        public int TotalProyectos { get; set; }
        public decimal InversionRegion { get; set; }
        public decimal KmCRegion { get; set; }
        public decimal MvaRegion { get; set; }
        public decimal MvarRegion { get; set; }
        public decimal InversionProyecto { get; set; }
        public decimal? KmCProyecto { get; set; }
        public decimal? MvaProyecto { get; set; }
        public decimal? MvarProyecto { get; set; }

        public decimal PorcentajeInversion => InversionRegion > 0 ? 100m * InversionProyecto / InversionRegion : 0;
        public decimal PorcentajeKmC => KmCRegion > 0 ? 100m * (KmCProyecto ?? 0) / KmCRegion : 0;
        public decimal PorcentajeMva => MvaRegion > 0 ? 100m * (MvaProyecto ?? 0) / MvaRegion : 0;
        public bool TieneDatos => TotalProyectos > 0 && InversionRegion > 0;
    }

    public class PamrntFichaProyectoViewModel
    {
        public HeaderViewModel Header { get; set; }
        public PamrntProyectoIdentificado Proyecto { get; set; }
        public PamrntFichaProyecto Ficha { get; set; }
        public PamProyectoDetalleViewModel Detalle { get; set; }
        public PamImpactoRegional ImpactoRegional { get; set; }
        public List<PamrntProyectoIdentificado> ContextoCartera { get; set; } = new();
        public List<PamrntProyectoIdentificado> ProyectosRegion { get; set; } = new();
        public bool FichaEnriquecida { get; set; }
        public bool EsPamrntIdentificado => string.Equals(Proyecto?.OrigenPrograma, "PAMRNT", StringComparison.OrdinalIgnoreCase);
        /// <summary>Contexto de panorama: los 8 PAMRNT para identificados, o los proyectos de la GCR para PAM.</summary>
        public List<PamrntProyectoIdentificado> Panorama => EsPamrntIdentificado ? ContextoCartera : ProyectosRegion;
        public IReadOnlyList<PamrntFichaRecurso> Recursos => Ficha?.Recursos ?? new List<PamrntFichaRecurso>();
        public IReadOnlyList<PamrntFichaRecurso> Diagramas => Recursos
            .Where(x => x.Aplica && (x.EsDiagramaUnifilar || x.EsGeoespacial || x.EsC7U))
            .OrderBy(x => x.Orden)
            .ToList();
        public bool TieneDiagramaUnifilar => Recursos.Any(x => x.Aplica && x.EsDiagramaUnifilar && !string.IsNullOrWhiteSpace(x.Url));
        public bool TieneGeoespacial => Recursos.Any(x => x.Aplica && x.EsGeoespacial && !string.IsNullOrWhiteSpace(x.Url));
        public bool TieneC7U => Recursos.Any(x => x.Aplica && x.EsC7U && !string.IsNullOrWhiteSpace(x.Url));
        public bool TieneRecursosVisuales => TieneDiagramaUnifilar || TieneGeoespacial || TieneC7U;
        public List<PamFaseProyecto> Fases => PamFasesParser.Desde(Detalle?.Actual?.EtapaProyecto, Detalle?.Actual?.FeoFactible);
        public PamEmpalme Empalme => new()
        {
            FechaNecesariaTexto = Detalle?.Actual?.FechaNecesaria,
            FeoFactibleTexto = Detalle?.Actual?.FeoFactible,
            FechaNecesaria = PamFechaParser.Parsear(Detalle?.Actual?.FechaNecesaria),
            FeoFactible = PamFechaParser.Parsear(Detalle?.Actual?.FeoFactible)
        };
    }

    public class PamProyectoDetalleViewModel
    {
        public HeaderViewModel Header { get; set; }
        public PamProyectoDetalleActual Actual { get; set; }
        public List<PamProyectoVersionResumen> Historial { get; set; } = new();
        public List<PamCambioDetalle> Cambios { get; set; } = new();
        public List<PamFuenteDetalle> Fuentes { get; set; } = new();
        public List<PamRelacionDetalle> Relaciones { get; set; } = new();
        public bool FichaDisponible => Actual?.ProyectoId > 0;
    }

    public class PamProyectoDetalleActual
    {
        public long ProyectoId { get; set; }
        public Guid ProyectoUid { get; set; }
        public int? ProyectoModernizacionIdOrigen { get; set; }
        public long ProyectoVersionId { get; set; }
        public int NumeroVersion { get; set; }
        public string OrigenPrograma { get; set; }
        public string Programa { get; set; }
        public int? AnioPrograma { get; set; }
        public string TipoProyecto { get; set; }
        public string EstadoVigenciaCartera { get; set; }
        public string ClaveProyecto { get; set; }
        public string ClavesAlternas { get; set; }
        public string NombreProyecto { get; set; }
        public string GRT { get; set; }
        public string EtapaProyecto { get; set; }
        public string EstatusLicitacion { get; set; }
        public string TipoFinanciamiento { get; set; }
        public int? AnioInstruccion { get; set; }
        public decimal? MontoProyectoMdp { get; set; }
        public decimal? PorcentajeAvanceEjecucion { get; set; }
        public string ElementosEquiposAsociados { get; set; }
        public string FechaEstimadaInicio { get; set; }
        public string FeoIndicadaOficioSener { get; set; }
        public string FeoFactible { get; set; }
        public string FechaNecesaria { get; set; }
        public string ZonaAtendida { get; set; }
        public int? PrioridadPrograma { get; set; }
        public string EstadoRealProyecto { get; set; }
        public string CircunstanciasAtrasos { get; set; }
        public string AccionesMitigacionCorreccion { get; set; }
        public string ComentariosNivelPriorizacion { get; set; }
        public string ClasificacionSener { get; set; }
        public decimal? Mva { get; set; }
        public decimal? Mvar { get; set; }
        public decimal? KmC { get; set; }
        public DateTime VigenteDesde { get; set; }
        public string FuenteDocumento { get; set; }
        public DateTime FechaCorte { get; set; }
        public string FuenteUbicacion { get; set; }
    }

    public class PamProyectoVersionResumen
    {
        public long ProyectoVersionId { get; set; }
        public int NumeroVersion { get; set; }
        public bool EsVersionVigente { get; set; }
        public DateTime VigenteDesde { get; set; }
        public DateTime? VigenteHasta { get; set; }
        public string ClaveProyecto { get; set; }
        public string NombreProyecto { get; set; }
        public string EtapaProyecto { get; set; }
        public string EstadoVigenciaCartera { get; set; }
        public decimal? MontoProyectoMdp { get; set; }
        public string MotivoCambio { get; set; }
        public string FuenteDocumento { get; set; }
        public DateTime FechaCorte { get; set; }
        public DateTime FechaRegistroUtc { get; set; }
        public string UsuarioRegistro { get; set; }
    }

    public class PamCambioDetalle
    {
        public long CambioId { get; set; }
        public string TipoCambio { get; set; }
        public string Campo { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public DateTime FechaRegistroUtc { get; set; }
        public string UsuarioRegistro { get; set; }
        public string TipoCarga { get; set; }
        public string FuenteDocumento { get; set; }
        public DateTime FechaCorte { get; set; }

        public string CampoEtiqueta => PamCambioFormato.EtiquetaCampo(Campo);
        public string AntesLimpio => PamCambioFormato.LimpiarValor(ValorAnterior);
        public string DespuesLimpio => PamCambioFormato.LimpiarValor(ValorNuevo);
        public bool EsAlta => string.Equals(TipoCambio, "Alta", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Limpia valores JSON ({"value":X}) y traduce nombres de campo a etiquetas legibles para el anexo de trazabilidad.</summary>
    public static class PamCambioFormato
    {
        public static string LimpiarValor(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return "∅";
            var v = valor.Trim();
            if (v.StartsWith("{"))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(v);
                    if (doc.RootElement.TryGetProperty("value", out var val))
                        return val.ValueKind == System.Text.Json.JsonValueKind.Null ? "∅" : val.ToString();
                }
                catch { /* no era JSON válido: se muestra crudo */ }
            }
            return v;
        }

        public static string EtiquetaCampo(string campo) => campo switch
        {
            "NombreProyecto" => "Nombre del proyecto",
            "GRT" => "Región de transmisión",
            "EtapaProyecto" => "Etapa / estatus",
            "EstatusLicitacion" => "Estatus de licitación",
            "FechaNecesaria" => "Fecha necesaria",
            "FeoFactible" => "FEO factible",
            "FeoIndicadaOficioSener" => "FEO indicada (oficio)",
            "MontoProyectoMdp" => "Inversión (MDP)",
            "EstadoRealProyecto" => "Estado reportado",
            "EstadoVigenciaCartera" => "Vigencia en cartera",
            "AnioInstruccion" => "Año de instrucción",
            "TipoFinanciamiento" => "Financiamiento",
            "PrioridadPrograma" => "Prioridad",
            "ZonaAtendida" => "Zona atendida",
            "Mva" => "Transformación (MVA)",
            "Mvar" => "Compensación (MVAr)",
            "KmC" => "Transmisión (km-C)",
            null or "" or "Registro completo" => "Registro completo",
            _ => campo
        };
    }

    public class PamFuenteDetalle
    {
        public long FuenteId { get; set; }
        public string NombreDocumento { get; set; }
        public string TipoDocumento { get; set; }
        public DateTime? FechaDocumento { get; set; }
        public DateTime FechaCorte { get; set; }
        public string RutaArchivo { get; set; }
        public string HojaPaginaSeccion { get; set; }
        public string VersionDocumento { get; set; }
        public string HashSha256 { get; set; }
        public string Observaciones { get; set; }
    }

    public class PamRelacionDetalle
    {
        public long ProyectoRelacionVersionId { get; set; }
        public string Direccion { get; set; }
        public long ProyectoRelacionadoId { get; set; }
        public string ClaveRelacionada { get; set; }
        public string NombreRelacionado { get; set; }
        public string TipoRelacion { get; set; }
        public string EstadoValidacion { get; set; }
        public DateTime VigenteDesde { get; set; }
        public DateTime? VigenteHasta { get; set; }
        public bool EsRelacionVigente { get; set; }
        public string FuenteDocumento { get; set; }
        public string HojaPaginaSeccion { get; set; }
        public string Observaciones { get; set; }
    }

    public class PamrntFichaProyecto
    {
        public string ClavePem { get; set; }
        public string Titulo { get; set; }
        public string TipoFicha { get; set; }
        public string ResumenEjecutivo { get; set; }
        public string AlternativaSeleccionada { get; set; }
        public string RecomendacionEjecutiva { get; set; }
        public decimal InversionMdp { get; set; }
        public string FechaNecesaria { get; set; }
        public string FechaFactible { get; set; }
        public decimal RelacionBeneficioCosto { get; set; }
        public int TotalObras { get; set; }
        public string CorredorPrincipal { get; set; }
        public string Fuente { get; set; }
        public List<string> Estados { get; set; } = new();
        public List<PamrntMetricaProyecto> MetasFisicas { get; set; } = new();
        public List<string> PendientesValidacion { get; set; } = new();
        public List<string> PuntosClave { get; set; } = new();
        public string DiagnosticoOperativo { get; set; }
        public string PronosticoDemanda { get; set; }
        public decimal? CrecimientoDemandaPorcentaje { get; set; }
        public string DemandaMaxima { get; set; }
        public decimal? BeneficioNetoUsdMiles { get; set; }
        public string NotaEvaluacionEconomica { get; set; }
        public string ConclusionEjecutiva { get; set; }
        public List<PamrntRiesgoProyecto> Riesgos { get; set; } = new();
        public List<PamrntAlternativaProyecto> Alternativas { get; set; } = new();
        public List<PamrntComparacionProyecto> Comparativa { get; set; } = new();
        public List<string> ProximosPasos { get; set; } = new();
        public List<PamrntFichaRecurso> Recursos { get; set; } = new();
    }

    public class PamrntFichaRecurso
    {
        public long RecursoId { get; set; }
        public long ProyectoId { get; set; }
        public string TipoRecurso { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Url { get; set; }
        public string AltText { get; set; }
        public int Orden { get; set; }
        public bool Aplica { get; set; } = true;
        public bool EsDiagramaUnifilar => string.Equals(TipoRecurso, "DiagramaUnifilar", StringComparison.OrdinalIgnoreCase);
        public bool EsGeoespacial => string.Equals(TipoRecurso, "Geoespacial", StringComparison.OrdinalIgnoreCase);
        public bool EsC7U => string.Equals(TipoRecurso, "C7U", StringComparison.OrdinalIgnoreCase);
    }

    public class PamrntMetricaProyecto
    {
        public string Concepto { get; set; }
        public string Valor { get; set; }
        public string Unidad { get; set; }
    }

    public class PamrntRiesgoProyecto
    {
        public string Riesgo { get; set; }
        public string Impacto { get; set; }
        public string Mitigacion { get; set; }
    }

    public class PamrntAlternativaProyecto
    {
        public string Nombre { get; set; }
        public string Tecnologia { get; set; }
        public decimal InversionMdp { get; set; }
        public string Alcance { get; set; }
        public string Ventaja { get; set; }
        public bool Recomendada { get; set; }
    }

    public class PamrntComparacionProyecto
    {
        public string Criterio { get; set; }
        public string Alternativa1 { get; set; }
        public string Alternativa2 { get; set; }
        public string Favorable { get; set; }
    }
}
