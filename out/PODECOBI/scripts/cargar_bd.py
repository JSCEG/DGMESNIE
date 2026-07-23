# -*- coding: utf-8 -*-
"""
cargar_bd.py — Carga inventario maestro PODECOBI + GeoJSON del CDN al esquema dgmesnie.

Fuentes (corte 2026-07-14, vigente en disco):
  data/inventario_maestro.json   ← canónico (14 polos + sus 22 campos)
  data/remote_polos.geojson      ← polígonos del CDN sassoapps con cache-busting

Operación:
  1. Calcula SHA256 del inventario (registra PODECOBI_Corte.HashInventario).
  2. UPSERT por Numero (01..14) en PODECOBI_Polo.
  3. Reemplaza PODECOBI_Vocacion, PODECOBI_Contacto, PODECOBI_Fuente (DELETE + INSERT por PoloId).
  4. UPSERT por (PoloId, FeatureIndex) en PODECOBI_Geometria (no repara geometrías inválidas).
  5. Registra PODECOBI_Corte con versión y conteo.

Uso:
  python cargar_bd.py \
      --inventario "C:/Users/User/Documents/Codex/Documentos latex/10_Informe_PODECOBI/data/inventario_maestro.json" \
      --geojson    "C:/Users/User/Documents/Codex/Documentos latex/10_Informe_PODECOBI/data/remote_polos.geojson" \
      --version    "1.2" \
      --corte      "2026-07-14" \
      --conn       "Server=...;Database=...;User Id=...;Password=...;Encrypt=yes;..."

Conexión:
  Por argumento --conn o variable de entorno PODECOBI_CONN.
  Driver: pyodbc + Microsoft ODBC Driver 18 for SQL Server.
"""

import argparse
import hashlib
import json
import os
import re
import sys
import uuid
from datetime import datetime, timezone
from pathlib import Path

try:
    import pyodbc
except ImportError:
    sys.exit("ERROR: pyodbc no instalado. `pip install pyodbc`.")


# ─────────────────────────────────────────────────────────────────────────────
# Constantes canónicas (coinciden con Sql/ddl_podecobi.sql)
# ─────────────────────────────────────────────────────────────────────────────
SCHEMA = "dgmesnie"

MAPA_AMBITO = {
    "federal_contact": "FEDERAL",
    "state_contact":   "ESTATAL",
    "municipal_contact": "MUNICIPAL",
}

URLS_POR_NUMERO = {
    # Solo se usan como respaldo cuando el inventario no trae UrlProyectosMexico.
    # Mapeo proviene del Informe_PODECOBI_2024_2026.md (fuentes 6..19).
}


# ─────────────────────────────────────────────────────────────────────────────
# Helpers
# ─────────────────────────────────────────────────────────────────────────────
def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def parse_fecha(s):
    """'30/06/2025' → date(2025,6,30). None si vacío o mal formado."""
    if not s or not isinstance(s, str):
        return None
    s = s.strip()
    for fmt in ("%d/%m/%Y", "%Y-%m-%d", "%d-%m-%Y"):
        try:
            return datetime.strptime(s, fmt).date()
        except ValueError:
            continue
    return None


def num(s):
    if s is None or s == "":
        return None
    try:
        return float(s)
    except (TypeError, ValueError):
        return None


def s(v):
    """Trim a str o None."""
    if v is None:
        return None
    v = str(v).strip()
    return v if v else None


def feature_index_para_numero(features, numero):
    """Devuelve la lista de features cuya 'properties.Nombre' o 'numero' matchean polo."""
    out = []
    for i, feat in enumerate(features):
        props = feat.get("properties") or {}
        # El CDN sassoapps no incluye 'numero' por defecto — emparejamos por nombre manual
        out.append((i, feat))
    return out


# ─────────────────────────────────────────────────────────────────────────────
# Carga
# ─────────────────────────────────────────────────────────────────────────────
def cargar_inventario(conn, inv_path: Path):
    data = json.loads(inv_path.read_text(encoding="utf-8"))
    if not isinstance(data, list):
        raise ValueError("inventario_maestro.json debe ser una lista")

    sha_inv = sha256_file(inv_path)
    print(f"  · {len(data)} polos leídos · SHA256={sha_inv[:12]}…")

    # UPSERT cabecera
    sql_polo = f"""
    MERGE {SCHEMA}.PODECOBI_Polo AS tgt
    USING (SELECT ? AS Numero) AS src ON tgt.Numero = src.Numero
    WHEN MATCHED THEN UPDATE SET
        NombreOficial = ?, NombreManual = ?, Estado = ?, Municipio = ?, Activo = 1,
        FechaDeclaracion = ?, UrlDeclaracion = ?, Modificacion = ?, UrlModificacion = ?,
        Convenio = ?, UrlConvenio = ?, ComiteSesion = ?,
        AreaOficialHa = ?, AreaManualRaw = ?, AreaGeojsonHa = ?, GeojsonDeltaHa = ?, GeojsonDeltaPct = ?,
        GeojsonFeatureCount = ?, GeojsonValid = ?, GeojsonValidity = ?,
        CentroidLon = ?, CentroidLat = ?,
        Etapa = ?, Subetapa = ?, FechaRevisionPublica = ?, UrlProyectosMexico = ?, AvanceManualPct = ?,
        Inversion = ?, Empleos = ?,
        DemandaElectrica = ?, DemandaElectricaNota = ?, DemandaMaxima = ?, DemandaMaximaNota = ?,
        Tension = ?, Conexion = ?,
        GasDisponibilidad = ?, GasNota = ?, Ducto = ?,
        CorteFuente = ?, Verificacion = ?, ComentarioVerificacion = ?,
        FechaActualizacion = SYSUTCDATETIME(), UsuarioActualizacion = ?
    WHEN NOT MATCHED THEN INSERT (
        Numero, NombreOficial, NombreManual, Estado, Municipio, Activo,
        FechaDeclaracion, UrlDeclaracion, Modificacion, UrlModificacion,
        Convenio, UrlConvenio, ComiteSesion,
        AreaOficialHa, AreaManualRaw, AreaGeojsonHa, GeojsonDeltaHa, GeojsonDeltaPct,
        GeojsonFeatureCount, GeojsonValid, GeojsonValidity,
        CentroidLon, CentroidLat,
        Etapa, Subetapa, FechaRevisionPublica, UrlProyectosMexico, AvanceManualPct,
        Inversion, Empleos,
        DemandaElectrica, DemandaElectricaNota, DemandaMaxima, DemandaMaximaNota,
        Tension, Conexion,
        GasDisponibilidad, GasNota, Ducto,
        CorteFuente, Verificacion, ComentarioVerificacion,
        UsuarioRegistro
    ) VALUES (
        ?, ?, ?, ?, ?, 1,
        ?, ?, ?, ?,
        ?, ?, ?,
        ?, ?, ?, ?, ?,
        ?, ?, ?,
        ?, ?,
        ?, ?, ?, ?, ?,
        ?, ?,
        ?, ?, ?, ?,
        ?, ?,
        ?, ?, ?,
        ?, ?, ?,
        ?
    );
    """

    sql_polo_id = f"SELECT {SCHEMA}.PODECOBI_Polo_PoloId_FromNumero(?) AS PoloId"
    sql_del_voc = f"DELETE FROM {SCHEMA}.PODECOBI_Vocacion WHERE PoloId = ?"
    sql_ins_voc = f"INSERT INTO {SCHEMA}.PODECOBI_Vocacion (PoloId, Vocacion, Orden) VALUES (?, ?, ?)"
    sql_del_con = f"DELETE FROM {SCHEMA}.PODECOBI_Contacto WHERE PoloId = ?"
    sql_ins_con = f"""
    INSERT INTO {SCHEMA}.PODECOBI_Contacto (PoloId, Ambito, Nombre, Cargo, Correo, Telefono, Notas)
    VALUES (?, ?, ?, ?, ?, ?, ?)
    """
    sql_del_fue = f"DELETE FROM {SCHEMA}.PODECOBI_Fuente WHERE PoloId = ?"
    sql_ins_fue = f"""
    INSERT INTO {SCHEMA}.PODECOBI_Fuente (PoloId, Tipo, Url, Fecha, Descripcion)
    VALUES (?, ?, ?, ?, ?)
    """

    usuario = os.environ.get("USERNAME") or "cargar_bd"

    with conn.cursor() as cur:
        # Necesitamos una función escalar simple para resolver PoloId desde Numero.
        # Si no existe, la creamos ad-hoc (no persistente).
        cur.execute(f"""
        IF OBJECT_ID('{SCHEMA}.PODECOBI_Polo_PoloId_FromNumero') IS NULL
        BEGIN
          EXEC('CREATE FUNCTION {SCHEMA}.PODECOBI_Polo_PoloId_FromNumero(@N VARCHAR(2)) RETURNS INT
                AS BEGIN RETURN (SELECT PoloId FROM {SCHEMA}.PODECOBI_Polo WHERE Numero = @N) END')
        END
        """)

        for item in data:
            numero = s(item.get("num"))
            if not numero:
                print(f"  ! polo sin 'num', se omite: {item.get('official_name')}")
                continue

            params = (
                numero,
                # UPDATE SET …
                s(item.get("official_name")), s(item.get("manual_name")),
                s(item.get("state")), s(item.get("municipality")),
                parse_fecha(item.get("declaration_date")), s(item.get("declaration_url")),
                s(item.get("modification")), s(item.get("modification_url")),
                s(item.get("agreement_url")) and "Convenio" or None,
                s(item.get("agreement_url")), s(item.get("committee")),
                num(item.get("official_area_ha")), s(item.get("manual_area_raw")),
                num(item.get("geojson_area_ha")), num(item.get("geojson_delta_ha")), num(item.get("geojson_delta_pct")),
                item.get("geojson_feature_count"), bool(item.get("geojson_valid")), s(item.get("geojson_validity")),
                num(item.get("centroid_lon")), num(item.get("centroid_lat")),
                s(item.get("stage")), s(item.get("substage")),
                parse_fecha(item.get("project_last_review")),
                s(item.get("project_url")), num(item.get("progress_manual_pct")),
                s(item.get("investment")), s(item.get("jobs")),
                s(item.get("electric_demand")), s(item.get("electric_demand_note")),
                s(item.get("maximum_demand")), s(item.get("maximum_demand_note")),
                s(item.get("voltage")), s(item.get("connection")),
                s(item.get("gas")), s(item.get("gas_note")), s(item.get("pipeline")),
                s(item.get("manual_cut")), s(item.get("verified_on")),
                s(item.get("comment_verification") or item.get("verification_note") or ""),
                usuario,
                # INSERT VALUES …
                numero, s(item.get("official_name")), s(item.get("manual_name")),
                s(item.get("state")), s(item.get("municipality")),
                parse_fecha(item.get("declaration_date")), s(item.get("declaration_url")),
                s(item.get("modification")), s(item.get("modification_url")),
                s(item.get("agreement_url")) and "Convenio" or None,
                s(item.get("agreement_url")), s(item.get("committee")),
                num(item.get("official_area_ha")), s(item.get("manual_area_raw")),
                num(item.get("geojson_area_ha")), num(item.get("geojson_delta_ha")), num(item.get("geojson_delta_pct")),
                item.get("geojson_feature_count"), bool(item.get("geojson_valid")), s(item.get("geojson_validity")),
                num(item.get("centroid_lon")), num(item.get("centroid_lat")),
                s(item.get("stage")), s(item.get("substage")),
                parse_fecha(item.get("project_last_review")),
                s(item.get("project_url")), num(item.get("progress_manual_pct")),
                s(item.get("investment")), s(item.get("jobs")),
                s(item.get("electric_demand")), s(item.get("electric_demand_note")),
                s(item.get("maximum_demand")), s(item.get("maximum_demand_note")),
                s(item.get("voltage")), s(item.get("connection")),
                s(item.get("gas")), s(item.get("gas_note")), s(item.get("pipeline")),
                s(item.get("manual_cut")), s(item.get("verified_on")),
                s(item.get("comment_verification") or item.get("verification_note") or ""),
                usuario,
            )
            cur.execute(sql_polo, params)

            # Resolver PoloId actual
            cur.execute(sql_polo_id, (numero,))
            row = cur.fetchone()
            if not row or not row[0]:
                raise RuntimeError(f"No se pudo resolver PoloId para Numero={numero}")
            polo_id = int(row[0])

            # Vocaciones
            cur.execute(sql_del_voc, (polo_id,))
            for i, voc in enumerate(item.get("productive_activities") or []):
                cur.execute(sql_ins_voc, (polo_id, s(voc), i))

            # Contactos (federal + estatal)
            cur.execute(sql_del_con, (polo_id,))
            for key, ambito in MAPA_AMBITO.items():
                c = item.get(key) or {}
                if isinstance(c, dict) and (c.get("nombre") or c.get("correo")):
                    cur.execute(sql_ins_con, (
                        polo_id, ambito,
                        s(c.get("nombre")), s(c.get("cargo")),
                        s(c.get("correo")), s(c.get("telefono")), s(c.get("notas"))
                    ))

            # Fuentes automáticas: declaratoria + proyectos México
            cur.execute(sql_del_fue, (polo_id,))
            if item.get("declaration_date"):
                t = "SIDOF" if "sidof" in (item.get("declaration_url") or "") else "DOF"
                cur.execute(sql_ins_fue, (
                    polo_id, t, s(item.get("declaration_url")),
                    parse_fecha(item.get("declaration_date")),
                    "Declaratoria publicada"
                ))
            if item.get("project_url"):
                cur.execute(sql_ins_fue, (
                    polo_id, "PROYECTOS_MX", s(item.get("project_url")),
                    parse_fecha(item.get("project_last_review")),
                    "Ficha oficial del polo en Proyectos México"
                ))

    return sha_inv


def cargar_geometria(conn, geo_path: Path, source_url: str):
    if not geo_path.exists():
        print(f"  ! GeoJSON no encontrado, se omite geometría: {geo_path}")
        return
    fc = json.loads(geo_path.read_text(encoding="utf-8"))
    features = fc.get("features") or []
    if not features:
        print("  ! GeoJSON sin features")
        return

    # Limpia geometrías previas: la asignación por (PoloId, FeatureIndex) puede cambiar
    # entre cortes del CDN si los ids rotan o se reordenan. Borramos todo y reinsertamos
    # de forma idempotente bajo el mismo corte.
    with conn.cursor() as cur:
        cur.execute(f"DELETE FROM {SCHEMA}.PODECOBI_Geometria")

    # Mapa nombre-oficial → Numero (recorremos BD) — solo como respaldo
    sql_nombre = f"SELECT Numero, NombreOficial, NombreManual FROM {SCHEMA}.PODECOBI_Polo"
    mapa = {}
    with conn.cursor() as cur:
        cur.execute(sql_nombre)
        for r in cur.fetchall():
            num_, oficial, manual = r[0], r[1], r[2]
            for k in (oficial, manual):
                if k:
                    mapa[norm(k)] = num_

    sql_ins_geom = f"""
    MERGE {SCHEMA}.PODECOBI_Geometria AS tgt
    USING (SELECT ? AS PoloId, ? AS FeatureIndex) AS src
      ON tgt.PoloId = src.PoloId AND tgt.FeatureIndex = src.FeatureIndex
    WHEN MATCHED THEN UPDATE SET
        GeometryType = ?, GeometryJson = ?, PropertiesJson = ?,
        IsValid = ?, ValidityNote = ?, AreaHa = ?, PerimetroM = ?,
        SourceUrl = ?, Sha256 = ?, FechaDescarga = ?
    WHEN NOT MATCHED THEN INSERT (
        PoloId, FeatureIndex, GeometryType, GeometryJson, PropertiesJson,
        IsValid, ValidityNote, AreaHa, PerimetroM,
        SourceUrl, Sha256, FechaDescarga
    ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);
    """

    sha_geo = sha256_file(geo_path)
    ahora = datetime.now(timezone.utc)
    matched = unmatched = 0
    with conn.cursor() as cur:
        for idx, feat in enumerate(features):
            num_ = match_feature_numero(feat, mapa)
            if not num_:
                unmatched += 1
                continue
            cur.execute(f"SELECT {SCHEMA}.PODECOBI_Polo_PoloId_FromNumero(?)", (num_,))
            row = cur.fetchone()
            if not row or not row[0]:
                unmatched += 1
                continue
            polo_id = int(row[0])

            geom = feat.get("geometry") or {}
            props = feat.get("properties") or {}
            gtipo = s(geom.get("type"))
            gjson = json.dumps(geom, ensure_ascii=False)
            pjson = json.dumps(props, ensure_ascii=False)
            area, perim, valid, note = area_perim_desde_props(props, gtipo)
            sha_feat = hashlib.sha256(gjson.encode("utf-8")).hexdigest()

            cur.execute(sql_ins_geom, (
                polo_id, idx,
                gtipo, gjson, pjson,
                bool(valid) if valid is not None else None, s(note), area, perim,
                source_url, sha_feat, ahora,
                polo_id, idx, gtipo, gjson, pjson,
                bool(valid) if valid is not None else None, s(note), area, perim,
                source_url, sha_feat, ahora,
            ))
            matched += 1

    print(f"  · geometría: {matched} features cargadas, {unmatched} sin match")


def norm(s):
    return re.sub(r"\s+", " ", (s or "").strip().lower())


# ── Mapeo explícito CDN sassoapps → Numero PODECOBI ────────────────────────
# El GeoJSON del CDN usa nombres de proyecto / anuncio, no siempre los jurídicos.
# Esta tabla es la ÚNICA fuente de verdad para la asignación de features a polos.
# Mantener alineada con la columna "Correspondencia entre el anuncio y los nombres
# jurídicos" del informe PODECOBI vigente.
MAPEO_CDN = {
    # id del feature en CDN (ID_00000..ID_00014) → Numero del polo
    "ID_00000": "11",   # Centro Logístico e Industrial de Durango (CLID) → Durango
    "ID_00001": "05",   # Puerta Logística del Bajío → Guanajuato
    "ID_00002": "11",   # Chetumal I → Chetumal, Quintana Roo
    "ID_00003": "07",   # San José Chiapa & Nopalucan (Cd. Audi) → Puebla (Futura Capital)
    "ID_00004": "13",   # Topolobampo → Sinaloa
    "ID_00005": "14",   # Xicotencatl II → Altamira, Tamaulipas (mismo complejo)
    "ID_00006": "03",   # Reserva Zapotlán/AIFA → Hidalgo
    "ID_00007": "02",   # San Jerónimo → Chihuahua
    "ID_00008": "10",   # Parque Industrial Bajío → Michoacán
    "ID_00009": "01",   # Seybaplaya I → Campeche
    "ID_00010": "09",   # Tuxpan → Veracruz
    "ID_00011": "04",   # CLAT Neza Bicentenario → Nezahualcóyotl, EdoMéx
    "ID_00012": "12",   # Hermosillo (parte 1, Sonora) → Hermosillo
    "ID_00013": "12",   # Hermosillo (parte 2) → Hermosillo (representación en dos entidades)
    "ID_00014": "14",   # Altamira → Tamaulipas
    # Por nombre (respaldo si el id cambia entre cortes del CDN)
    "centro logistico e industrial de durango": "11",
    "puerta logistica del bajio": "05",
    "chetumal i": "11",
    "chetumal": "11",
    "san jose chiapa": "04",
    "futura capital": "04",
    "topolobampo": "14",
    "xicotencatl ii": "09",
    "reserva zapotlan": "03",
    "san jeronimo": "02",
    "parque industrial bajio": "06",
    "seybaplaya i": "01",
    "tuxpan": "09",
    "clat neza bicentenario": "08",
    "nezahualcoyotl": "08",
    "hermosillo": "13",
    "altamira": "07",
}


def match_feature_numero(feat, mapa):
    """Mapea un feature del CDN a su Numero PODECOBI. Prioriza id del feature;
    si no hay match, intenta por nombre normalizado."""
    props = feat.get("properties") or {}

    # 1) Match por id del feature (CDN usa 'ID_00000'..'ID_00014')
    fid = str(props.get("id") or props.get("Id") or props.get("ID") or "").strip()
    if fid and fid in MAPEO_CDN:
        return MAPEO_CDN[fid]

    # 2) Match por nombre normalizado (con fallback de caracteres mal codificados)
    for key in ("nombre", "Nombre", "NOMBRE", "name", "Name", "polo", "PODECOBI"):
        v = props.get(key)
        if v:
            # Intento 1: normal tal cual
            n = mapa.get(norm(v))
            if n: return n
            # Intento 2: reparar mojibake latin1→utf8 (común cuando se sirve CP1252)
            try:
                repaired = v.encode("latin1").decode("utf-8")
                n = mapa.get(norm(repaired))
                if n: return n
            except (UnicodeEncodeError, UnicodeDecodeError):
                pass
            # Intento 3: coincidencia parcial por tokens clave
            tokens = set(re.findall(r"[a-záéíóúñ]+", norm(v)))
            for polo_nombre, num in mapa.items():
                polo_tokens = set(re.findall(r"[a-záéíóúñ]+", polo_nombre))
                if polo_tokens and tokens and polo_tokens.issubset(tokens):
                    return num
    return None


def area_perim_desde_props(props, geom_type):
    """Lee campos calculados que el script extract_inputs.py ya dejó en properties."""
    area = num(props.get("area_ha_geodesic") or props.get("area_ha"))
    perim = num(props.get("perimeter_m_geodesic") or props.get("perimetro_m"))
    valid = props.get("is_valid")
    note = props.get("validity")
    if valid is not None:
        valid = bool(valid)
    return area, perim, valid, note


def registrar_corte(conn, fecha_corte, version, sha_inv, polo_count):
    sql = f"""
    IF NOT EXISTS (SELECT 1 FROM {SCHEMA}.PODECOBI_Corte WHERE FechaCorte = ?)
    INSERT INTO {SCHEMA}.PODECOBI_Corte (FechaCorte, VersionInforme, PoloCount, HashInventario, CargadoPor, Notas)
    VALUES (?, ?, ?, ?, ?, ?);
    """
    with conn.cursor() as cur:
        cur.execute(sql, (
            fecha_corte,
            fecha_corte, version, polo_count, sha_inv,
            os.environ.get("USERNAME") or "cargar_bd",
            f"Carga automatica · {datetime.now(timezone.utc).isoformat()}"
        ))


# ─────────────────────────────────────────────────────────────────────────────
# Main
# ─────────────────────────────────────────────────────────────────────────────
def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--inventario", required=True, type=Path)
    ap.add_argument("--geojson", required=True, type=Path)
    ap.add_argument("--corte", required=True, help="Fecha de corte ISO YYYY-MM-DD")
    ap.add_argument("--version", default="1.2")
    ap.add_argument("--source-url", default="https://cdn.sassoapps.com/dgmesnie/podecobis/14_PODECOBIS_02072026.geojson")
    ap.add_argument("--conn", default=os.environ.get("PODECOBI_CONN"))
    args = ap.parse_args()

    if not args.conn:
        sys.exit("ERROR: --conn o variable PODECOBI_CONN requerida.")

    print(f"[cargar_bd] inventario: {args.inventario}")
    print(f"[cargar_bd] geojson:    {args.geojson}")
    print(f"[cargar_bd] corte:      {args.corte}  v{args.version}")

    with pyodbc.connect(args.conn, autocommit=False) as conn:
        print("[1/3] cargando polos + vocaciones + contactos + fuentes…")
        sha_inv = cargar_inventario(conn, args.inventario)
        conn.commit()

        print("[2/3] cargando geometría del CDN…")
        cargar_geometria(conn, args.geojson, args.source_url)
        conn.commit()

        print("[3/3] registrando corte…")
        with conn.cursor() as cur:
            cur.execute(f"SELECT COUNT(*) FROM {SCHEMA}.PODECOBI_Polo WHERE Activo = 1")
            polo_count = int(cur.fetchone()[0])
        registrar_corte(conn, parse_fecha(args.corte) or datetime.utcnow().date(),
                        args.version, sha_inv, polo_count)
        conn.commit()

    print(f"[cargar_bd] OK · {polo_count} polos · corte {args.corte} v{args.version}")


if __name__ == "__main__":
    main()
