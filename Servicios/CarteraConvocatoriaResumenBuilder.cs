using System.Globalization;
using System.Text.RegularExpressions;
using NSIE.Models.ProyectosPrivados;
using static NSIE.Models.ProyectosPrivados.ConvocatoriaFichaCampos;

namespace NSIE.Servicios;

/// <summary>
/// Agrega la cartera Mixtos II (universo y proyectos firmes) a partir de la cartera vigente, el último
/// expediente de cada folio y las marcas de clúster de CFE. Sólo lectura; no altera datos.
/// </summary>
public static class CarteraConvocatoriaResumenBuilder
{
    private const string Cat = "CATALOGO DE PROYECTOS";
    private static readonly TextInfo Text = CultureInfo.GetCultureInfo("es-MX").TextInfo;

    public static CarteraConvocatoriaResumenViewModel Build(
        CarteraConvocatoriaDatos cartera,
        List<CarteraConvocatoriaExpediente> dossiers,
        List<CarteraConvocatoriaMarca> marks)
    {
        var byFolio = dossiers.GroupBy(d => d.Folio, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var markByFolio = marks.GroupBy(m => m.Folio, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var all = cartera.Projects.Where(p => p.Source == "Mixtos II").ToList();
        var firm = all.Where(p => p.Consideration == "firme").ToList();
        CarteraConvocatoriaExpediente? D(CarteraConvocatoriaProyecto p) => byFolio.GetValueOrDefault(p.Folio);

        var model = new CarteraConvocatoriaResumenViewModel
        {
            SourceVersion = cartera.SourceVersion,
            LatestImport = cartera.LatestImport,
            CutoffDate = cartera.LatestImport?.CutoffDate ?? dossiers.FirstOrDefault()?.CutoffDate,
            Total = all.Count,
            Firm = firm.Count,
            Review = all.Count(p => p.Consideration == "revision"),
            Rejected = all.Count(p => p.Consideration == "no-va"),
            MwUniverso = all.Sum(p => p.Mw),
            MwFirme = firm.Sum(p => p.Mw),
            MarcasDisponibles = marks.Count > 0,
            TipoCambio = dossiers.Select(d => d.ExchangeRate).FirstOrDefault(r => r is > 0),
            Requerimientos = dossiers.Select(d => d.SystemRequirements).FirstOrDefault(r => r.Count > 0) ?? new()
        };

        // ── Universo ──
        model.EstatusUniverso = Count(all, p => p.Consideration switch { "firme" => "Considerados", "revision" => "En seguimiento (registro vigente)", _ => Text.ToTitleCase(Clean(p.UniverseStatus, "No considerados").ToLowerInvariant()) });
        model.Origenes = Count(firm, p => { var o = Clean(D(p).Valor("Origen", 1, Cat), "Sin origen registrado"); return o == "0" ? "Sin origen registrado" : o; });
        model.Preseleccionados = firm.Count(p => { var v = D(p).Valor("Preseleccionado", 1, Cat); return Tiene(v) && !EsNo(v); });
        model.AntecedenteMixtosI = firm.Count(p => EsSi(D(p).Valor("¿Estaba Mixtos I?", 1, Cat)));
        model.AntecedenteCvp2 = firm.Count(p => EsSi(D(p).Valor("¿Estaba CVP2?", 1, Cat)));
        model.Consorcios = firm.Count(p => EsSi(D(p).Valor("¿La solicitud es presentada por un consorcio?")));

        // ── Clasificaciones de los firmes ──
        model.Tecnologias = Count(firm, p => Tech(p.Technology));
        model.Modalidades = Count(firm, p => Modalidad(D(p)));
        model.GruposInteres = Count(firm, p => Clean(p.InterestGroup ?? D(p).Valor("Grupo de Interés"), "No informado")).Take(8).ToList();
        model.Tensiones = Count(firm, p => Tension(p, D(p)));
        model.Subestaciones = Count(firm, p => Clean(p.Substation, "No informada")).Take(10).ToList();
        model.Entidades = Count(firm, p => Text.ToTitleCase(Clean(p.State, "No informada").ToLowerInvariant())).ToList();
        model.PorEtapas = firm.Count(p => EsSi(D(p).Valor("Proyecto por etapas")));

        // ── Entrada en operación ──
        var years = firm.Select(p => (Project: p, Year: Year(D(p)))).ToList();
        model.Operacion = years.GroupBy(x => x.Year).OrderBy(g => g.Key == "Sin fecha" ? 1 : 0).ThenBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new ResumenAnio
            {
                Anio = g.Key,
                Projects = g.Count(),
                Mw = g.Sum(x => x.Project.Mw),
                MwPorGcr = g.GroupBy(x => Gcr(x.Project)).ToDictionary(r => r.Key, r => r.Sum(x => x.Project.Mw)),
                ProyectosPorGcr = g.GroupBy(x => Gcr(x.Project)).ToDictionary(r => r.Key, r => r.Count())
            }).ToList();

        // ── Trámites ──
        model.ValidacionCenace = Count(firm, p => Clean(D(p).Valor("Estatus Validación CENACE"), "Sin registro"));
        model.Permisos = Count(firm, p => Clean(D(p).Valor("Permiso Generación"), "Sin registro"));
        model.Pagos = Count(firm, p => Clean(D(p).Valor("Estatus de Pago"), "Sin registro"));
        model.Registro = Count(firm, p => Clean(D(p).Valor("Registro"), "Sin registro"));
        model.SolicitudPermiso = Count(firm, p => Clean(D(p).Valor("Estatus de la solicitud de permiso (Sin información / Presentada)"), "Sin registro"));
        model.ConEstudiosInterconexion = firm.Count(p => EsSi(D(p).Valor("Has Estudios Interconexión")));

        // ── Almacenamiento ──
        // Una potencia de SAE mayor al doble de la central (o a 1,000 MW) es un error de captura: se aparta para revisión.
        static bool SaeFueraDeRango(CarteraConvocatoriaProyecto p, (decimal Mw, decimal Hours) sae) => sae.Mw > Math.Max(p.Mw * 2m, 50m) || sae.Mw > 1000m;
        var saeRevisar = new List<CarteraConvocatoriaProyecto>();
        foreach (var p in firm)
        {
            var sae = Sae(D(p));
            if (sae.Mw <= 0) continue;
            if (SaeFueraDeRango(p, sae)) { saeRevisar.Add(p); continue; }
            model.ConSae++;
            model.SaeMw += sae.Mw;
            model.SaeMwh += sae.Mw * sae.Hours;
        }

        // ── Costos ──
        var conCfe = firm.Where(p => p.NetworkCostUsd.HasValue || p.NetworkCostMxn.HasValue).ToList();
        model.ConFichaCfe = conCfe.Count;
        model.CostoRedMdd = conCfe.Sum(p => p.NetworkCostUsd ?? 0);
        model.CostoRedMdp = conCfe.Sum(p => p.NetworkCostMxn ?? 0);
        model.MwConFichaCfe = conCfe.Sum(p => p.Mw);
        model.Obras = conCfe.Sum(p => p.WorksCount ?? 0);
        foreach (var p in firm)
        {
            var inversion = NumeroValor(D(p).Valor("Monto de inversión total del proyecto"));
            if (inversion is > 0) { model.ConInversionDeclarada++; model.InversionDeclarada += inversion.Value; }
            var capex = NumeroValor(D(p).Valor("CapEx total del proyecto"));
            if (capex is > 0) { model.ConCapex++; model.CapexTotal += capex.Value; model.CapexSae += NumeroValor(D(p).Valor("CapEx total del SAE")) ?? 0; }
        }

        // ── Evaluaciones ──
        bool Has(CarteraConvocatoriaProyecto p, string sheet) => D(p)?.Records.Any(r => r.Sheet == sheet) == true;
        model.ConEvaluacionSocial = firm.Count(p => Has(p, "BD_IMPACTO_SOCIAL"));
        model.ConFactibilidad = firm.Count(p => Has(p, "BD_FACTIBILIDAD"));
        model.ConInteresCfe = firm.Count(p => Has(p, "BD_INTERES_CFE"));
        model.RiesgoSocial = Count(firm, p => Clean(D(p).Valor("RIESGO SOCIAL", 1, Cat), "Sin evaluación"));
        model.EstatusEvis = Count(firm, p => Clean(D(p).Valor("ESTATUS EVIS / MISSE", 1, Cat), "Sin evaluación"));
        model.EstatusMia = Count(firm, p => Clean(D(p).Valor("ESTATUS MIA", 1, Cat), "Sin evaluación"));
        model.Factibilidad = Count(firm, p => Factible(D(p).Valor("¿FACTIBLE?", 1, Cat) ?? p.TechnicalViability));
        model.Clasificacion = Count(firm, p => Clean(D(p).Valor("CLASIFICACIÓN", 1, Cat) ?? p.AnalysisClassification, "Sin clasificación"));

        // ── Marcas CFE ──
        model.ConCluster = firm.Count(p => Tiene(markByFolio.GetValueOrDefault(p.Folio)?.Cluster));
        model.ConExcluyente = firm.Count(p => { var m = markByFolio.GetValueOrDefault(p.Folio); return Tiene(m?.Excluyente1) || Tiene(m?.Excluyente2); });
        model.Clusters = Count(firm.Where(p => Tiene(markByFolio.GetValueOrDefault(p.Folio)?.Cluster)).ToList(), p => markByFolio[p.Folio].Cluster!.Trim());

        // ── Por gerencia ──
        model.Gcr = firm.GroupBy(Gcr).Select(g =>
        {
            var row = new ResumenGcr
            {
                Region = g.Key,
                Universo = all.Count(p => Gcr(p) == g.Key),
                Projects = g.Count(),
                Mw = g.Sum(p => p.Mw),
                Share = model.MwFirme > 0 ? 100m * g.Sum(p => p.Mw) / model.MwFirme : 0,
                MwPorTecnologia = g.GroupBy(p => Tech(p.Technology)).ToDictionary(t => t.Key, t => t.Sum(p => p.Mw)),
                MwPorAnio = g.GroupBy(p => Year(D(p))).ToDictionary(y => y.Key, y => y.Sum(p => p.Mw)),
                ConFichaCfe = g.Count(p => p.NetworkCostUsd.HasValue || p.NetworkCostMxn.HasValue),
                CostoMdd = g.Sum(p => p.NetworkCostUsd ?? 0),
                CostoMdp = g.Sum(p => p.NetworkCostMxn ?? 0),
                Obras = g.Sum(p => p.WorksCount ?? 0),
                ConSocial = g.Count(p => Has(p, "BD_IMPACTO_SOCIAL")),
                ConFactibilidad = g.Count(p => Has(p, "BD_FACTIBILIDAD")),
                ConInteres = g.Count(p => Has(p, "BD_INTERES_CFE")),
                ConCluster = g.Count(p => Tiene(markByFolio.GetValueOrDefault(p.Folio)?.Cluster)),
                Modalidades = Count(g.ToList(), p => Modalidad(D(p)))
            };
            foreach (var p in g)
            {
                var sae = Sae(D(p));
                if (sae.Mw > 0 && !SaeFueraDeRango(p, sae)) { row.ConSae++; row.SaeMw += sae.Mw; }
                row.InversionDeclarada += NumeroValor(D(p).Valor("Monto de inversión total del proyecto")) ?? 0;
            }
            return row;
        }).OrderByDescending(r => r.Mw).ToList();

        // ── Fichas resumidas por proyecto ──
        ResumenProyecto Proyecto(CarteraConvocatoriaProyecto p)
        {
            var d = D(p);
            var sae = Sae(d);
            var duplicado = EsSi(d.Valor("Duplicado", 1, Cat)) || Tiene(p.DuplicateGroup);
            var estatus = p.Consideration == "firme" ? "Considerado"
                : p.Consideration == "revision" ? (duplicado ? "Registro vigente · duplicado · en seguimiento" : "Registro vigente · en seguimiento")
                : Text.ToTitleCase(Clean(p.UniverseStatus, "No considerado").ToLowerInvariant());
            return new ResumenProyecto
            {
                Folio = p.Folio,
                Name = p.Name,
                Region = Gcr(p),
                Technology = Tech(p.Technology),
                Mw = p.Mw,
                Operacion = Fecha(d.Valor("Fecha Prevista para entrada en operación comercial"), "No informada"),
                FichaCfe = p.NetworkCostUsd.HasValue || p.NetworkCostMxn.HasValue,
                CostoMdd = p.NetworkCostUsd,
                Cluster = markByFolio.GetValueOrDefault(p.Folio)?.Cluster,
                Factibilidad = Factible(d.Valor("¿FACTIBLE?", 1, Cat) ?? p.TechnicalViability),
                Modalidad = Modalidad(d),
                Sae = sae.Mw > 0 ? $"{sae.Mw:N0} MW / {sae.Hours:N0} h" : "Sin SAE",
                Entidad = Text.ToTitleCase(Clean(p.State, "No informada").ToLowerInvariant()) + (Tiene(p.Municipality) ? " · " + p.Municipality!.Trim() : ""),
                Estatus = estatus,
                RiesgoSocial = Clean(d.Valor("RIESGO SOCIAL", 1, Cat), "Sin evaluación"),
                EstatusEvis = Clean(d.Valor("ESTATUS EVIS / MISSE", 1, Cat), "Sin registro"),
                EstatusMia = Clean(d.Valor("ESTATUS MIA", 1, Cat), "Sin registro"),
                RiesgoAmbiental = Clean(d.Valor("RIESGO AMBIENTAL", 1, Cat), "Sin evaluación"),
                Observacion = Clean(d.Valor("OBSERVACIÓN DEL ÁREA", 1, Cat) ?? p.TechnicalAnalysis, ""),
                Cruce = CruceSocial(p, d),
                Antecedentes = Antecedentes(p, d),
                Inversion = NumeroValor(d.Valor("Monto de inversión total del proyecto"))
            };
        }

        // Antecedentes registrados en el expediente de un registro no incluido: sin criterio de exclusión documentado,
        // se listan los hechos que constan en el corte para mantenerlo en seguimiento.
        string Antecedentes(CarteraConvocatoriaProyecto p, CarteraConvocatoriaExpediente? d)
        {
            var notas = new List<string>();
            var obs = Clean(d.Valor("OBSERVACIÓN DEL ÁREA", 1, Cat) ?? p.TechnicalAnalysis, "");
            if (obs != "") notas.Add("Observación del área: " + obs.TrimEnd('.'));
            var llave = (d.Valor("FOLIO CONSIDERADO (llave única)", 1, Cat) ?? p.CanonicalFolio ?? "").Trim();
            if (Tiene(llave) && !llave.Equals(p.Folio, StringComparison.OrdinalIgnoreCase))
            {
                var canon = all.FirstOrDefault(x => x.Folio.Equals(llave, StringComparison.OrdinalIgnoreCase));
                notas.Add(canon is null
                    ? $"Homologado con el folio {llave}"
                    : $"Homologado con el folio {llave} ({canon.Mw:N0} MW · {Text.ToTitleCase(Clean(canon.InterestGroup ?? canon.Company, "promovente no informado").ToLowerInvariant())}), considerado");
            }
            var dup = (d.Valor("FOLIO(S) DUPLICADO(S)", 1, Cat) ?? p.DuplicateGroup ?? "").Trim();
            if (EsSi(d.Valor("Duplicado", 1, Cat)) || Tiene(p.DuplicateGroup))
            {
                var otro = all.FirstOrDefault(x => x.Folio.Equals(dup, StringComparison.OrdinalIgnoreCase));
                notas.Add(!Tiene(dup) ? "Duplicado de otro folio" : otro?.Consideration == "no-va" ? $"Duplicado del folio {dup} (desechado)" : $"Duplicado del folio {dup}");
            }
            var pre = d.Valor("Preseleccionado", 1, Cat);
            if (Tiene(pre) && !EsNo(pre)) notas.Add("Preseleccionado");
            if (EsSi(d.Valor("¿Estaba Mixtos I?", 1, Cat))) notas.Add("Antecedente en Mixtos I");
            if (EsSi(d.Valor("¿Estaba CVP2?", 1, Cat))) notas.Add("Antecedente en CVP2");
            var fac = Factible(d.Valor("¿FACTIBLE?", 1, Cat) ?? p.TechnicalViability);
            if (fac != "Sin evaluación") notas.Add("Factibilidad: " + fac.ToLowerInvariant());
            var riesgo = Clean(d.Valor("RIESGO SOCIAL", 1, Cat), "");
            if (riesgo != "") notas.Add("Riesgo social " + riesgo.ToLowerInvariant());
            var prevencion = d.Valor("Prevención 1 - Motivo");
            if (Tiene(prevencion) && !EsNoAplica(prevencion)) notas.Add("Solicitud de estudios con prevención");
            if (notas.Count == 0) notas.Add("Sin observaciones ni evaluaciones registradas en el corte");
            return string.Join(" · ", notas);
        }
        model.TopProyectos = firm.OrderByDescending(p => p.Mw).Take(12).Select(Proyecto).ToList();
        model.SaeRevisar = saeRevisar.OrderByDescending(p => p.Mw).Select(Proyecto).ToList();
        model.TopCostoRed = conCfe.OrderByDescending(p => p.NetworkCostUsd ?? 0).ThenByDescending(p => p.Mw).Take(8).Select(Proyecto).ToList();
        var conInversion = firm.Select(p => (Project: p, Inversion: NumeroValor(D(p).Valor("Monto de inversión total del proyecto")) ?? 0)).Where(x => x.Inversion > 0).ToList();
        model.TopInversion = conInversion.OrderByDescending(x => x.Inversion).Take(8).Select(x => Proyecto(x.Project)).ToList();
        model.InversionPorTecnologia = conInversion.GroupBy(x => Tech(x.Project.Technology))
            .Select(g => new ResumenConteo { Name = g.Key, Projects = g.Count(), Mw = g.Sum(x => x.Project.Mw), Amount = g.Sum(x => x.Inversion) })
            .OrderByDescending(x => x.Amount).ToList();
        model.GruposExcluyentes = firm.Select(p => (Project: p, Mark: markByFolio.GetValueOrDefault(p.Folio))).Where(x => Tiene(x.Mark?.Excluyente1))
            .GroupBy(x => x.Mark!.Excluyente1!.Trim())
            .Select(g => new ResumenConteo
            {
                Name = g.Key, Projects = g.Count(), Mw = g.Sum(x => x.Project.Mw),
                Detail = string.Join(" · ", g.GroupBy(x => Gcr(x.Project)).OrderByDescending(r => r.Count()).Select(r => r.Key))
            })
            .OrderByDescending(x => x.Mw).Take(8).ToList();
        model.ValidosNoConsiderados = all.Where(p => p.Consideration == "revision").OrderByDescending(p => p.Mw).Select(Proyecto).ToList();
        model.RiesgoSocialAlto = firm.Where(p => (D(p).Valor("RIESGO SOCIAL", 1, Cat) ?? "").Trim().StartsWith("ALTO", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Mw).Select(Proyecto).ToList();
        model.Hibridos = firm.Where(p => Tech(p.Technology) == "Híbrida").OrderByDescending(p => p.Mw).Select(Proyecto).ToList();

        return model;
    }

    // La evaluación social se homologó por nombre de proyecto; se confirma que nombre, entidad y
    // municipio del registro social coincidan con la solicitud del folio antes de atribuirle el riesgo.
    private static string CruceSocial(CarteraConvocatoriaProyecto p, CarteraConvocatoriaExpediente? d)
    {
        var social = d?.Records.FirstOrDefault(r => r.Sheet == "BD_IMPACTO_SOCIAL");
        if (social == null) return "Riesgo del catálogo; sin registro en la evaluación social";
        string S(string name) => social.Fields.FirstOrDefault(f => string.Equals(f.Name.Trim(), name, StringComparison.OrdinalIgnoreCase))?.Value ?? "";
        static string N(string? v) => Regex.Replace(new string((v ?? "").Normalize(System.Text.NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).ToUpperInvariant(), @"[^A-Z0-9]+", " ").Trim();
        // Nombres: mismas palabras en otro orden ("PV HUICHAPAN (GUALUMBOS)" vs "HUICHAPAN (PV GUALUMBOS)") cuentan como coincidencia.
        static HashSet<string> Tokens(string v) => N(v).Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        static bool Similar(string a, string b)
        {
            var x = N(a); var y = N(b);
            if (x == y || (x.Length > 4 && y.Length > 4 && (x.Contains(y) || y.Contains(x)))) return true;
            var ta = Tokens(a); var tb = Tokens(b);
            if (ta.Count == 0 || tb.Count == 0) return false;
            var common = ta.Intersect(tb).Count();
            return common >= Math.Max(1, (int)Math.Ceiling(0.7 * Math.Min(ta.Count, tb.Count)));
        }
        // Municipios: tolera erratas de una o dos letras ("Calpulapan" / "Calpulalpan").
        static bool SameMunicipio(string a, string b)
        {
            var x = N(a); var y = N(b);
            if (x == y) return true;
            if (x.Length >= 6 && y.Length >= 6 && x[..5] == y[..5] && Math.Abs(x.Length - y.Length) <= 2) return true;
            return false;
        }
        var nombreOk = Similar(S("Proyecto"), p.Name) || Similar(S("NOMBRE HOMOLOGADO"), p.Name);
        var entidadOk = N(S("Entidad Federativa")) == N(p.State);
        var municipioOk = SameMunicipio(S("Municipio/Alcaldía"), p.Municipality ?? "");
        var confianza = S("CONFIANZA").Trim();
        var partes = new List<string>();
        partes.Add(nombreOk ? "Nombre coincide" : "Nombre distinto: " + S("Proyecto"));
        partes.Add(entidadOk ? "entidad coincide" : "entidad distinta");
        partes.Add(municipioOk ? "municipio coincide" : "municipio distinto (" + S("Municipio/Alcaldía") + ")");
        if (Tiene(confianza) && confianza != "—") partes.Add("confianza " + confianza.ToLowerInvariant());
        var texto = string.Join(" · ", partes);
        return nombreOk && entidadOk && municipioOk ? "Coincide nombre, entidad y municipio" + (Tiene(confianza) && confianza != "—" ? " · confianza " + confianza.ToLowerInvariant() : "") : texto;
    }

    private static List<ResumenConteo> Count(List<CarteraConvocatoriaProyecto> projects, Func<CarteraConvocatoriaProyecto, string> key) =>
        projects.GroupBy(key).Select(g => new ResumenConteo { Name = g.Key, Projects = g.Count(), Mw = g.Sum(p => p.Mw) })
            .OrderByDescending(x => x.Mw).ThenByDescending(x => x.Projects).ToList();

    private static string Clean(string? value, string fallback) => Tiene(value) ? value!.Trim() : fallback;

    private static string Gcr(CarteraConvocatoriaProyecto p) => Tiene(p.Region) ? Text.ToTitleCase(p.Region.Trim().ToLowerInvariant()) : "Sin gerencia";

    private static string Tech(string? technology)
    {
        if (!Tiene(technology)) return "No informada";
        var t = technology!.Trim().ToUpperInvariant();
        if (t.Contains("FOTOVOLTA") || t.Contains("SOLAR")) return "Fotovoltaica";
        if (t.Contains("EÓLIC") || t.Contains("EOLIC")) return "Eólica";
        if (t.Contains("HÍBRID") || t.Contains("HIBRID")) return "Híbrida";
        if (t.Contains("TERMO")) return "Termosolar";
        if (t.Contains("BOMBEO") || t.Contains("HIDR")) return "Hidroeléctrica / rebombeo";
        if (t.Contains("ALMACEN") || t.Contains("BATER")) return "Almacenamiento";
        return Text.ToTitleCase(t.ToLowerInvariant());
    }

    private static string Modalidad(CarteraConvocatoriaExpediente? d)
    {
        var value = d.Valor("Modalidad de asociación con CFE en el Esquema de Desarrollo Mixto");
        return Tiene(value) ? Text.ToTitleCase(value!.Trim().ToLowerInvariant()) : "No informada";
    }

    private static string Tension(CarteraConvocatoriaProyecto p, CarteraConvocatoriaExpediente? d)
    {
        var declared = d.Valor("¿A qué nivel de tensión se interconectaría?");
        if (Tiene(declared)) return Regex.IsMatch(declared!, @"\d") ? Regex.Replace(declared!.Trim(), @"\s*k?v$", " kV", RegexOptions.IgnoreCase) : declared!.Trim();
        var match = Regex.Match(p.Substation ?? "", @"@\s*([\d.,]+)\s*KV", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value + " kV" : "No informada";
    }

    private static string Year(CarteraConvocatoriaExpediente? d)
    {
        var fecha = FechaValor(d.Valor("Fecha Prevista para entrada en operación comercial"));
        return fecha.HasValue ? fecha.Value.Year.ToString(CultureInfo.InvariantCulture) : "Sin fecha";
    }

    private static string Factible(string? value)
    {
        var v = (value ?? "").Trim().ToUpperInvariant();
        return v switch
        {
            "1" or "SI" or "SÍ" => "Factible",
            "X" => "Con restricciones de red",
            "DESECHADO" => "Desechado",
            "" or "NONE" => "Sin evaluación",
            _ => Text.ToTitleCase(v.ToLowerInvariant())
        };
    }

    // "27 MW / 3" → (27, 3); catálogo "ALMACENAMIENTO SAE" como respaldo.
    private static (decimal Mw, decimal Hours) Sae(CarteraConvocatoriaExpediente? d)
    {
        var text = d.Valor("Potencia instalada del SAE (MW) y horas de almacenamiento");
        if (!Tiene(text)) text = d.Valor("¿Cuál es la potencia instalada del SAE y horas de almacenamiento?");
        if (!Tiene(text)) text = d.Valor("ALMACENAMIENTO SAE", 1, Cat);
        if (!Tiene(text)) return (0, 0);
        var mw = Regex.Match(text!, @"([\d.,]+)\s*MW", RegexOptions.IgnoreCase);
        var hours = Regex.Match(text!, @"/\s*([\d.,]+)");
        decimal Parse(Match m) => m.Success && decimal.TryParse(m.Groups[1].Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : 0;
        return (Parse(mw), Parse(hours));
    }
}
