# -*- coding: utf-8 -*-
"""Genera las láminas .dc.html del deck del Dashboard de Planeación Energética.

Fuente de contenido: PRESENTACION_DASHBOARD_TERRITORIAL.md
Sistema de diseño: tokens --sener-* / --dash-* del proyecto DGMESNIE.

Uso:  python build.py
"""
import json
import os

OUT = os.path.dirname(os.path.abspath(__file__))

W, H = 1600, 900

GUINDA = "#9B2247"
DORADO = "#E0A12E"
DORADO_T = "#7A5D1D"
VERDE = "#0E8A6E"
CIAN = "#1E9CB8"
PAPEL = "#FAF8F5"
BLANCO = "#ffffff"
TINTA = "#1c1b1a"
GRIS = "#6F6B66"
GRISC = "#9A958E"
LINEA = "#ECE8E2"
LINEAF = "#DCD6CE"
CIRUELA = "#6B3654"
TERRA = "#9A3F27"

MONO = "'IBM Plex Mono', Consolas, monospace"
SANS = "'Noto Sans', 'Segoe UI', sans-serif"

SHELL = """<!doctype html>
<html>
<head>
  <meta charset="utf-8">
  <script src="./support.js"></script>
</head>
<body>
<x-dc>
<helmet>
  <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Noto+Sans:wght@400;600;700;800&family=IBM+Plex+Mono:wght@500;600&display=swap">
  <style>
    body {
      margin: 0;
      font-family: 'Noto Sans', 'Segoe UI', sans-serif;
      -webkit-font-smoothing: antialiased;
    }
    a { color: #9B2247; text-decoration: none; }
    a:hover { color: #8E1F40; }
    code {
      font-family: 'IBM Plex Mono', Consolas, monospace;
      font-size: .92em;
      background: rgba(155, 34, 71, .07);
      padding: 1px 5px;
    }
  </style>
</helmet>
@@ROOT@@
</x-dc>
<script data-dc-script data-props='{"$preview":{"width":1600,"height":900}}'>
class Component extends DCLogic {
  renderVals() { return {}; }
}
</script>
</body>
</html>
"""


def eyebrow(txt, color=GUINDA):
    return (
        '<div style="font-family:%s;font-size:12px;font-weight:600;letter-spacing:.2em;'
        'text-transform:uppercase;color:%s">%s</div>' % (MONO, color, txt)
    )


def title(txt, size=46, color=TINTA, mw=1220):
    return (
        '<h1 style="margin:14px 0 0;font-size:%dpx;font-weight:800;line-height:1.04;'
        'letter-spacing:-.015em;color:%s;max-width:%dpx;text-wrap:balance">%s</h1>'
        % (size, color, mw, txt)
    )


def lead(txt, mw=1140):
    return (
        '<p style="margin:16px 0 0;font-size:18px;line-height:1.5;color:%s;max-width:%dpx;'
        'text-wrap:pretty">%s</p>' % (GRIS, mw, txt)
    )


def pill(txt, color, bg=None):
    bg = bg or "rgba(0,0,0,.04)"
    return (
        '<span style="display:inline-flex;align-items:center;font-family:%s;font-size:11px;'
        'font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:%s;background:%s;'
        'border:1px solid %s;border-radius:999px;padding:4px 11px;white-space:nowrap">%s</span>'
        % (MONO, color, bg, color + "33", txt)
    )


def num_badge(n, color=GUINDA):
    return (
        '<span style="flex:none;width:30px;height:30px;border-radius:9px;background:%s;'
        'color:#fff;font-family:%s;font-size:13px;font-weight:600;display:flex;'
        'align-items:center;justify-content:center">%s</span>' % (color, MONO, n)
    )


def table(cols, rows, widths=None, align=None, fs=14):
    n = len(cols)
    widths = widths or ["1fr"] * n
    align = align or ["left"] * n
    out = ['<div style="display:grid;grid-template-columns:%s;column-gap:22px;'
           'border-top:2px solid %s">' % (" ".join(widths), TINTA)]
    for i, c in enumerate(cols):
        out.append(
            '<div style="font-family:%s;font-size:10.5px;font-weight:600;letter-spacing:.13em;'
            'text-transform:uppercase;color:%s;padding:11px 0 10px;text-align:%s">%s</div>'
            % (MONO, GRIS, align[i], c)
        )
    for r in rows:
        for i, cell in enumerate(r):
            out.append(
                '<div style="font-size:%spx;line-height:1.42;color:%s;padding:11px 0;'
                'border-top:1px solid %s;text-align:%s">%s</div>'
                % (fs, TINTA, LINEA, align[i], cell)
            )
    out.append("</div>")
    return "".join(out)


def stat(value, label, sub="", color=GUINDA, vsize=54):
    s = ('<div style="font-size:11.5px;color:%s;line-height:1.35;margin-top:4px">%s</div>'
         % (GRISC, sub)) if sub else ""
    return (
        '<div style="display:flex;flex-direction:column;border-top:3px solid %s;padding-top:14px">'
        '<div style="font-family:%s;font-size:%dpx;font-weight:600;line-height:1;color:%s;'
        'letter-spacing:-.02em">%s</div>'
        '<div style="font-size:13px;font-weight:700;color:%s;margin-top:10px;line-height:1.3">%s</div>'
        '%s</div>' % (color, MONO, vsize, color, value, TINTA, label, s)
    )


def bullets(items, color=GUINDA, fs=15, gap=11):
    li = []
    for it in items:
        li.append(
            '<li style="display:flex;gap:11px;align-items:flex-start">'
            '<span style="flex:none;width:6px;height:6px;border-radius:50%%;background:%s;'
            'margin-top:8px"></span>'
            '<span style="font-size:%spx;line-height:1.5;color:%s;text-wrap:pretty">%s</span></li>'
            % (color, fs, TINTA, it)
        )
    return ('<ul style="list-style:none;margin:0;padding:0;display:flex;flex-direction:column;'
            'gap:%dpx">%s</ul>' % (gap, "".join(li)))


def block(head, body, color=GUINDA, w=None):
    ww = ("width:%dpx;" % w) if w else "flex:1;"
    return (
        '<section style="%sdisplay:flex;flex-direction:column;gap:14px;background:%s;'
        'border:1px solid %s;border-left:3px solid %s;padding:22px 24px 24px">'
        '<div style="font-family:%s;font-size:11px;font-weight:600;letter-spacing:.16em;'
        'text-transform:uppercase;color:%s">%s</div>%s</section>'
        % (ww, BLANCO, LINEA, color, MONO, color, head, body)
    )


def slide(n, body_html, eyebrow_txt, ttl, lead_txt=None, accent=GUINDA, pad_top=52):
    head = eyebrow(eyebrow_txt, accent) + title(ttl)
    if lead_txt:
        head += lead(lead_txt)
    root = (
        '<div style="position:relative;width:%dpx;height:%dpx;background:%s;color:%s;'
        'font-family:%s;display:flex;flex-direction:column;overflow:hidden">'
        '<div style="display:flex;height:6px;flex:none">'
        '<div style="flex:6;background:%s"></div>'
        '<div style="flex:1;background:%s"></div>'
        '<div style="flex:1;background:%s"></div>'
        '<div style="flex:1;background:%s"></div>'
        '</div>'
        '<div style="flex:1;display:flex;flex-direction:column;padding:%dpx 72px 0;min-height:0">'
        '<header style="flex:none">%s</header>'
        '<div style="flex:1;display:flex;flex-direction:column;justify-content:flex-start;'
        'padding:34px 0 0;min-height:0">%s</div>'
        '</div>'
        '<footer style="flex:none;display:flex;align-items:center;gap:18px;margin:0 72px;'
        'padding:16px 0 22px;border-top:1px solid %s">'
        '<span style="font-size:11.5px;font-weight:700;color:%s">Dashboard de Planeación Energética</span>'
        '<span style="font-size:11.5px;color:%s">DGMESNIE &middot; Secretaría de Energía</span>'
        '<span style="flex:1"></span>'
        '<span style="font-family:%s;font-size:11.5px;font-weight:600;color:%s">19 AGO 2026</span>'
        '<span style="font-family:%s;font-size:11.5px;font-weight:600;color:%s">%02d / 20</span>'
        '</footer>'
        '</div>'
        % (W, H, PAPEL, TINTA, SANS,
           GUINDA, DORADO, VERDE, CIAN,
           pad_top, head, body_html,
           LINEA, TINTA, GRISC, MONO, GRISC, MONO, GUINDA, n)
    )
    return SHELL.replace("@@ROOT@@", root)


def write(name, content):
    with open(os.path.join(OUT, name), "w", encoding="utf-8") as fh:
        fh.write(content)


# ── 01 · Portada ─────────────────────────────────────────────────────────────
portada_root = (
    '<div style="position:relative;width:%dpx;height:%dpx;background:%s;color:%s;'
    'font-family:%s;display:flex;flex-direction:column;overflow:hidden">'
    '<div style="display:flex;height:10px;flex:none">'
    '<div style="flex:6;background:%s"></div>'
    '<div style="flex:1;background:%s"></div>'
    '<div style="flex:1;background:%s"></div>'
    '<div style="flex:1;background:%s"></div>'
    '</div>'
    '<div style="flex:1;display:flex;flex-direction:column;padding:52px 72px 0">'
    '<div style="display:flex;align-items:flex-start;gap:26px">'
    '<div style="flex:1"></div>'
    '<img src="./logo_gob.png" alt="Gobierno de México" style="height:50px;width:auto">'
    '<div style="width:1px;height:50px;background:%s"></div>'
    '<img src="./logo_sener.png" alt="Secretaría de Energía" style="height:42px;width:auto;margin-top:4px">'
    '</div>'
    '<div style="flex:1;display:flex;flex-direction:column;justify-content:center;max-width:1260px">'
    '%s'
    '<h1 style="margin:20px 0 0;font-size:74px;font-weight:800;line-height:1;'
    'letter-spacing:-.025em;color:%s">Dashboard de<br>Planeación Energética</h1>'
    '<p style="margin:26px 0 0;font-size:23px;line-height:1.4;color:%s;max-width:980px;'
    'text-wrap:pretty">Mapas, análisis territorial geoespacial y reportes. Qué hace hoy, '
    'cómo se usa y qué sigue pendiente.</p>'
    '<div style="display:flex;gap:10px;margin-top:32px;flex-wrap:wrap">%s%s%s</div>'
    '</div>'
    '<div style="display:grid;grid-template-columns:repeat(4,1fr);gap:34px;padding:0 0 30px">'
    '%s%s%s%s</div>'
    '</div>'
    '<footer style="flex:none;display:flex;align-items:center;gap:18px;margin:0 72px;'
    'padding:16px 0 24px;border-top:1px solid %s">'
    '<span style="font-size:12px;font-weight:700;color:%s">Dirección General de Modernización y '
    'Evaluación del Sistema Nacional de Información Energética</span>'
    '<span style="flex:1"></span>'
    '<span style="font-family:%s;font-size:12px;font-weight:600;color:%s">01 / 20</span>'
    '</footer>'
    '</div>'
    % (W, H, PAPEL, TINTA, SANS,
       GUINDA, DORADO, VERDE, CIAN, LINEAF,
       eyebrow("Sesión de presentación &middot; 20 de agosto de 2026"),
       TINTA, GRIS,
       pill("/DashboardProyectos/Index", GUINDA, BLANCO),
       pill("Corte 19 AGO 2026", GRIS, BLANCO),
       pill("Uso interno DGMESNIE", DORADO_T, BLANCO),
       stat("39", "Capas registradas", "15 en el panel principal", GUINDA, 50),
       stat("281", "Proyectos PAM / PAMRNT", "133 con asociación de red firme", CIRUELA, 50),
       stat("1,060", "Permisos CRE georreferenciados", "generación eléctrica", CIAN, 50),
       stat("4", "Formatos de salida", "PDF, PDF 16:9, Excel y PNG", VERDE, 50),
       LINEA, GRIS, MONO, GUINDA)
)
write("Main.dc.html", SHELL.replace("@@ROOT@@", portada_root))


# ── 02 · Qué es ──────────────────────────────────────────────────────────────
def pregunta(n, q, a, color):
    return (
        '<section style="flex:1;display:flex;flex-direction:column;gap:16px;'
        'border-top:3px solid %s;padding-top:18px">'
        '<div style="display:flex;align-items:center;gap:12px">%s'
        '<span style="font-family:%s;font-size:11px;font-weight:600;letter-spacing:.16em;'
        'text-transform:uppercase;color:%s">Pregunta</span></div>'
        '<div style="font-size:24px;font-weight:800;line-height:1.22;color:%s;'
        'text-wrap:balance">%s</div>'
        '<div style="font-size:15px;line-height:1.55;color:%s;text-wrap:pretty">%s</div>'
        '</section>' % (color, num_badge(n, color), MONO, color, TINTA, q, GRIS, a)
    )

body02 = (
    '<div style="display:flex;gap:36px;align-items:stretch">%s%s%s</div>'
    '<div style="margin-top:44px;padding:20px 24px;background:%s;border:1px solid %s;'
    'border-left:3px solid %s;font-size:15px;line-height:1.55;color:%s;max-width:1290px">'
    '<strong style="color:%s">Referencia de interacción:</strong> el Atlas SEN de Batu Energy '
    '&mdash; mapa como espacio principal, capas en un panel compacto, gráfica en bandeja inferior. '
    '<strong style="color:%s">Diferencia DGMESNIE:</strong> la separación obligatoria por '
    'naturaleza del dato, que se explica en la lámina siguiente.</div>'
    % (
        pregunta(1, "¿Qué hay en un territorio?",
                 "Infraestructura eléctrica, permisos, proyectos, áreas protegidas, pueblos "
                 "indígenas, patrimonio y núcleos agrarios.", GUINDA),
        pregunta(2, "¿Cómo se comporta ese territorio?",
                 "Demanda y pronóstico horario, usuarios y consumo por división CFE, "
                 "generación distribuida.", CIAN),
        pregunta(3, "¿Cómo lo documento?",
                 "Un reporte territorial exportable a PDF, PDF 16:9, Excel y PNG, con envío "
                 "por correo institucional.", VERDE),
        BLANCO, LINEA, DORADO, TINTA, GUINDA, GUINDA,
    )
)
write("QueEs.dc.html", slide(
    2, body02, "Lámina 01", "El mapa es el espacio principal.",
    "Todo lo demás &mdash; capas, indicadores, fichas y reportes &mdash; se acomoda alrededor de él."))


# ── 03 · El principio que ordena todo ────────────────────────────────────────
nat_rows = [
    [num_badge(1, GUINDA), "<strong>Inventario institucional</strong>",
     "Subestaciones y líneas conciliadas en base de datos"],
    [num_badge(2, CIAN), "<strong>Fuente pública automatizada</strong>",
     "Demanda CENACE, divisiones tarifarias CFE"],
    [num_badge(3, GRIS), "<strong>Referencia secundaria</strong>",
     "Subestaciones de distribución OSM, licencia ODbL"],
    [num_badge(4, CIRUELA), "<strong>Planeación</strong>",
     "PAM / PAMRNT, generación privada planeada"],
    [num_badge(5, DORADO), "<strong>Evidencia pendiente de validación</strong>",
     "Vacíos de red de la 2ª convocatoria"],
]
body03 = (
    '<div style="display:flex;gap:56px;align-items:flex-start">'
    '<div style="flex:1.35">%s</div>'
    '<div style="width:430px;display:flex;flex-direction:column;gap:22px">'
    '<div style="background:%s;border:1px solid %s;border-left:3px solid %s;padding:24px">'
    '<div style="font-family:%s;font-size:11px;font-weight:600;letter-spacing:.16em;'
    'text-transform:uppercase;color:%s">Consecuencia práctica</div>'
    '<div style="font-size:20px;font-weight:800;line-height:1.3;color:%s;margin-top:14px;'
    'text-wrap:balance">Ninguna coincidencia automática se convierte en dato validado.</div>'
    '<div style="font-size:14.5px;line-height:1.55;color:%s;margin-top:14px">Se publica como '
    '<em>sugerida y trazable</em>, con su puntaje, su origen y su fecha de corte. La validación '
    'la firma una persona y queda en bitácora.</div></div>'
    '<div style="font-size:15px;line-height:1.6;color:%s;padding-left:2px">Una central, un permiso '
    'y un proyecto planeado <strong style="color:%s">no son lo mismo</strong> y por eso nunca se '
    'funden en una sola capa del mapa.</div>'
    '</div></div>'
    % (table(["#", "Naturaleza del dato", "Ejemplo en el tablero"], nat_rows,
             widths=["44px", "1fr", "1.15fr"], fs=15),
       BLANCO, LINEA, GUINDA, MONO, GUINDA, TINTA, GRIS, TINTA, GUINDA)
)
write("Principio.dc.html", slide(
    3, body03, "Lámina 02", "Cinco naturalezas de dato que no se mezclan."))


# ── 04 · Anatomía de la pantalla ─────────────────────────────────────────────
def wf_box(label, style):
    return ('<div style="%s;display:flex;align-items:center;justify-content:center;'
            'font-family:%s;font-size:9.5px;font-weight:600;letter-spacing:.08em;'
            'text-transform:uppercase;text-align:center;line-height:1.35;padding:4px">%s</div>'
            % (style, MONO, label))

wireframe = (
    '<div style="position:relative;width:100%%;height:392px;background:%s;border:1px solid %s;'
    'display:flex;flex-direction:column;overflow:hidden">'
    '<div style="flex:none;height:44px;display:flex;align-items:center;gap:8px;padding:0 12px;'
    'background:%s;border-bottom:1px solid %s">'
    '<span style="font-size:11px;font-weight:800;color:%s">Dashboard de Planeación Energética</span>'
    '<span style="flex:1"></span>%s</div>'
    '<div style="flex:1;display:flex;position:relative;background:#EEF1EE">'
    '<div style="position:absolute;inset:0;background:'
    'repeating-linear-gradient(0deg,rgba(0,0,0,.05) 0 1px,transparent 1px 34px),'
    'repeating-linear-gradient(90deg,rgba(0,0,0,.05) 0 1px,transparent 1px 34px)"></div>'
    '%s%s%s%s</div></div>'
    % (BLANCO, LINEAF, BLANCO, LINEA, GUINDA,
       "".join('<span style="width:22px;height:22px;border:1px solid %s;border-radius:6px;'
               'background:%s"></span>' % (LINEAF, PAPEL) for _ in range(9)),
       wf_box("Panel<br>de capas",
              "position:absolute;left:14px;top:14px;width:168px;height:236px;background:%s;"
              "border:1px solid %s;color:%s" % (BLANCO, LINEAF, GUINDA)),
       wf_box("Leyenda",
              "position:absolute;left:14px;bottom:14px;width:168px;height:62px;background:%s;"
              "border:1px solid %s;color:%s" % (BLANCO, LINEAF, GRIS)),
       wf_box("Panel de detalle",
              "position:absolute;right:14px;top:14px;width:186px;height:214px;background:%s;"
              "border:1px solid %s;color:%s" % (BLANCO, LINEAF, CIRUELA)),
       wf_box("Bandeja de indicadores &middot; demanda, consumo, generación distribuida",
              "position:absolute;left:200px;right:14px;bottom:14px;height:82px;background:%s;"
              "border:1px solid %s;color:%s" % (BLANCO, LINEAF, CIAN)))
)

zonas = [
    ["Barra superior", "Capas &middot; Análisis &middot; Datos &middot; Detalle &middot; Leyenda "
     "&middot; Medir &middot; Limpiar &middot; Actualizar &middot; Nuevo análisis &middot; "
     "Pantalla completa"],
    ["Panel de capas", "Catálogo agrupado, con interruptor, conteo, búsqueda y filtro "
     "<strong>por capa</strong>"],
    ["Leyenda", "Plegable; separa lo que entró al análisis de lo que sólo está dibujado"],
    ["Bandeja inferior", "Gráfica contextual del indicador activo"],
    ["Panel de detalle", "Ficha del elemento seleccionado, con fuente y fecha de corte"],
    ["Buscador", "Búsqueda transversal por capa, entidad, tecnología y gerencia"],
]
body04 = (
    '<div style="display:flex;gap:48px;align-items:flex-start">'
    '<div style="flex:1.05">%s'
    '<div style="font-size:12px;color:%s;margin-top:12px;font-family:%s">Esquema de zonas '
    '&middot; no es captura de pantalla</div></div>'
    '<div style="flex:1">%s'
    '<div style="margin-top:22px;padding:16px 18px;background:%s;border:1px solid %s;'
    'border-left:3px solid %s;font-size:14.5px;line-height:1.55;color:%s">'
    'Arranque deliberadamente ligero: <strong>mapa sin teselas, sólo límites estatales</strong>, '
    'ninguna capa pesada encendida. Todo lo demás es carga diferida.</div>'
    '</div></div>'
    % (wireframe, GRISC, MONO,
       table(["Zona", "Función"], [["<strong>%s</strong>" % z[0], z[1]] for z in zonas],
             widths=["150px", "1fr"], fs=13.5),
       BLANCO, LINEA, VERDE, TINTA)
)
write("Anatomia.dc.html", slide(4, body04, "Lámina 03", "Anatomía de la pantalla."))


# ── 05 · Capas: operación y mercado ──────────────────────────────────────────
INTEG = pill("Integrada", VERDE, "rgba(14,138,110,.08)")
MAPGRAF = pill("Mapa + gráfica", CIAN, "rgba(30,156,184,.08)")

cap_op = [
    ["01", "Centrales eléctricas &middot; infraestructura", INTEG, "Inventario DGMESNIE (CDN)",
     "Activos físicos públicos y privados. <strong>No equivale a permisos</strong>"],
    ["02", "Permisos CRE &middot; generación eléctrica", INTEG,
     "BD <code>vElectricidad_autorizado_mapa</code>",
     "1,060 permisos georreferenciados; se excluyen 27 de importación/exportación"],
    ["03", "Gerencias de control CENACE", INTEG, "CENACE/CFE + geometría DGMESNIE",
     "Contexto regional de operación"],
    ["04", "Líneas de transmisión", INTEG, "Grafo eléctrico en BD",
     "Aristas con tensión y extremos resueltos"],
    ["05", "Subestaciones de transmisión", INTEG, "Inventario conciliado en BD",
     "Nodos clasificados como transmisión"],
    ["06", "Subestaciones de distribución", INTEG, "Inventario conciliado en BD",
     "Subtransmisión, distribución e indeterminado"],
    ["07", "Demanda y pronóstico &middot; CENACE", MAPGRAF, "CENACE, compilado por Atlas SEN",
     "9 regiones más el agregado SIN, resolución horaria"],
    ["08", "Divisiones CFE &middot; usuarios y consumo", MAPGRAF, "DOF + INEGI + memorias CNE",
     "17 divisiones, serie 2022&ndash;2026"],
    ["09", "Generación distribuida &middot; CNE", MAPGRAF, "Estadísticas CNE/CRE",
     "Capacidad y contratos por entidad"],
]
kv = [("400 kV", "#00A9CE"), ("230 kV", "#F2C94C"), ("161&ndash;138 kV", "#3DAA5D"),
      ("115&ndash;60 kV", "#C84FA3"), ("44&ndash;13.2 kV", "#FFFFFF"), ("menor a 13.2 kV", "#F28C38")]
kv_html = "".join(
    '<div style="display:flex;align-items:center;gap:8px">'
    '<span style="width:26px;height:5px;background:%s;border:1px solid %s"></span>'
    '<span style="font-family:%s;font-size:11.5px;font-weight:600;color:%s">%s</span></div>'
    % (c, LINEAF, MONO, TINTA, n) for n, c in kv)

body05 = (
    '%s'
    '<div style="display:flex;gap:26px;align-items:center;margin-top:26px;padding:16px 22px;'
    'background:%s;border:1px solid %s">'
    '<div style="font-family:%s;font-size:10.5px;font-weight:600;letter-spacing:.14em;'
    'text-transform:uppercase;color:%s;width:150px;line-height:1.4">Código cromático<br>'
    'de tensión &middot; DACG/DOF</div>%s'
    '<span style="flex:1"></span>'
    '<div style="font-size:12.5px;color:%s;max-width:330px;line-height:1.45">El estado topológico '
    'se expresa con trazo continuo o discontinuo, nunca sustituyendo el color de tensión.</div>'
    '</div>'
    % (table(["#", "Capa", "Estado", "Fuente principal", "Alcance"], cap_op,
             widths=["34px", "1.15fr", "142px", "1fr", "1.25fr"], fs=13),
       BLANCO, LINEA, MONO, GRIS, kv_html, GRIS)
)
write("CapasOperacion.dc.html", slide(
    5, body05, "Lámina 04 &middot; Capas", "Operación y mercado &middot; bloque Eléctrico.",
    "El panel crece por subsector: a futuro caben Gas natural, Petrolíferos o Gasolinas sin "
    "mezclar indicadores incompatibles.", accent=CIAN))


# ── 06 · Capas: planeación y expansión ───────────────────────────────────────
cap_pl = [
    ["01", "Proyectos PAM / PAMRNT", INTEG,
     "Catálogo completo más asociación sugerida con la red eléctrica"],
    ["02", "Generación privada planeada &middot; 1ª convocatoria",
     pill("Referencia secundaria", GRIS, "rgba(111,107,102,.08)"),
     "18 proyectos ligados a 20 permisos; algunas ubicaciones son aproximadas"],
    ["03", "Vacíos de red &middot; 2ª convocatoria", INTEG,
     "Coincidencias propuestas, <strong>fuera del grafo oficial</strong> mientras no se validen"],
    ["04", "Cartera institucional de convocatorias",
     pill("Dataset interno", CIAN, "rgba(30,156,184,.08)"),
     "<code>dgmesnie.CarteraConvocatoriaProyecto</code>. Sólo se dibujan los registros con "
     "coordenada válida; el resto vive en datos y reportes"],
    ["05", "Transmisión planeada",
     pill("Pendiente de contrato de datos", DORADO_T, "rgba(224,161,46,.12)"),
     "Obras futuras, separadas de la red existente"],
]
body06 = (
    '%s<div style="display:flex;gap:24px;margin-top:34px">%s%s</div>'
    % (table(["#", "Capa", "Estado", "Alcance"], cap_pl,
             widths=["34px", "1.15fr", "236px", "1.5fr"], fs=14),
       block("Lo que la capa PAM permite hacer",
             bullets(["Filtrar por etapa, licitación y gerencia de control.",
                      "Abrir la ficha ejecutiva del proyecto desde el mapa.",
                      "Correr el análisis territorial con la geometría del proyecto o de su GCR.",
                      "Ver el subgrafo eléctrico alrededor del proyecto."], CIRUELA, 14.5, 9),
             CIRUELA),
       block("Lo que la capa PAM no hace",
             bullets(["No inventa coordenadas cuando el expediente no las tiene.",
                      "No convierte una coincidencia de texto en ubicación validada.",
                      "No incorpora al grafo oficial los marcadores regionales.",
                      "No mezcla evidencia de convocatorias con el inventario institucional."],
                     DORADO, 14.5, 9),
             DORADO))
)
write("CapasPlaneacion.dc.html", slide(
    6, body06, "Lámina 05 &middot; Capas", "Planeación y expansión.", accent=CIRUELA))


# ── 07 · Contexto territorial y catálogo de análisis ─────────────────────────
catalogo = [
    "Ductos de importación y ductos integrados a SISTRANGAS.",
    "Presas.",
    "Permisos de gas natural, Gas LP y petrolíferos.",
    "ANP federales y estatales; conservación voluntaria; humedales RAMSAR.",
    "Atlas de pueblos indígenas, lenguas, localidades y regiones indígenas, Ruta Wixárika.",
    "Sitios y zonas arqueológicas, zonas históricas.",
    "Núcleos agrarios (RAN).",
]
body07 = (
    '<div style="display:flex;gap:52px;align-items:flex-start">'
    '<div style="width:430px;display:flex;flex-direction:column;gap:26px">'
    '<div>%s</div>'
    '<div style="display:grid;grid-template-columns:repeat(3,1fr);gap:22px">%s%s%s</div>'
    '</div>'
    '<div style="flex:1">'
    '<div style="font-family:%s;font-size:11px;font-weight:600;letter-spacing:.16em;'
    'text-transform:uppercase;color:%s;margin-bottom:16px">Fuera del panel &middot; '
    'catálogo de análisis</div>%s'
    '<div style="margin-top:22px;padding:16px 18px;background:%s;border:1px solid %s;'
    'border-left:3px solid %s;font-size:14.5px;line-height:1.55;color:%s">Estas capas se cargan '
    '<strong>únicamente</strong> cuando el usuario define una cobertura o las selecciona '
    'expresamente. Mantenerlas fuera del panel es lo que impide saturar el mapa.</div>'
    '</div></div>'
    % (table(["Contexto territorial", "Estado"],
             [["<strong>Estados</strong>", pill("Activa al inicio", VERDE, "rgba(14,138,110,.08)")],
              ["<strong>Municipios</strong>", pill("Diferida &middot; ~26 MB", DORADO_T,
                                                   "rgba(224,161,46,.12)")],
              ["<strong>PODECOBI</strong>", pill("Integrada", VERDE, "rgba(14,138,110,.08)")]],
             widths=["1fr", "196px"], fs=15),
       stat("39", "Capas registradas", "", GUINDA, 42),
       stat("15", "En el panel principal", "", CIAN, 42),
       stat("29", "Habilitadas para análisis", "", VERDE, 42),
       MONO, GRIS, bullets(catalogo, GRIS, 15, 12), BLANCO, LINEA, CIAN, TINTA)
)
write("Contexto.dc.html", slide(
    7, body07, "Lámina 06 &middot; Capas", "Contexto territorial y catálogo de análisis.",
    accent=VERDE))


# ── 08 · Bandeja de indicadores ──────────────────────────────────────────────
spark = (
    '<svg viewBox="0 0 260 84" style="width:100%%;height:84px" role="img" '
    'aria-label="Series horarias de demanda, generación y pronóstico">'
    '<polyline points="4,64 30,58 56,48 82,40 108,30 134,24 160,26 186,34 212,44 238,52 256,58" '
    'fill="none" stroke="%s" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"/>'
    '<polyline points="4,72 30,68 56,60 82,52 108,44 134,38 160,40 186,46 212,56 238,64 256,70" '
    'fill="none" stroke="%s" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>'
    '<polyline points="4,60 30,54 56,44 82,36 108,27 134,21 160,23 186,31 212,41 238,49 256,55" '
    'fill="none" stroke="%s" stroke-width="1.6" stroke-dasharray="5 4" stroke-linecap="round"/>'
    '<line x1="134" y1="10" x2="134" y2="80" stroke="%s" stroke-width="1.4"/></svg>'
    % (CIAN, VERDE, GRISC, GUINDA)
)
coro_cells = ["#E8F5EC", "#B8DFC7", "#72BE91", "#2F9667", "#0B5D3B", "#B8DFC7",
              "#72BE91", "#0B5D3B", "#2F9667", "#E8F5EC", "#72BE91", "#2F9667"]
coro = ('<div style="display:grid;grid-template-columns:repeat(6,1fr);gap:5px;height:84px">%s</div>'
        % "".join('<div style="background:%s;border:1px solid rgba(0,0,0,.06)"></div>' % c
                  for c in coro_cells))
barras_gd = ('<div style="display:flex;align-items:flex-end;gap:12px;height:84px">%s</div>'
             % "".join('<div style="flex:1;height:%d%%;background:%s"></div>' % (v, DORADO)
                       for v in [26, 38, 52, 61, 74, 84]))


def modulo(letra, ttl, chart, items, color):
    return (
        '<section style="flex:1;display:flex;flex-direction:column;gap:16px;background:%s;'
        'border:1px solid %s;border-top:3px solid %s;padding:22px 24px 26px">'
        '<div style="display:flex;align-items:center;gap:11px">%s'
        '<span style="font-size:18px;font-weight:800;color:%s;line-height:1.25">%s</span></div>'
        '%s%s</section>'
        % (BLANCO, LINEA, color, num_badge(letra, color), TINTA, ttl, chart,
           bullets(items, color, 13.5, 8))
    )

body08 = (
    '<div style="display:flex;gap:26px;align-items:stretch">%s%s%s</div>'
    '<div style="margin-top:30px;padding:18px 22px;background:%s;border:1px solid %s;'
    'border-left:3px solid %s;font-size:15px;line-height:1.55;color:%s">Los tres comparten la '
    'misma mecánica: <strong>seleccionar en el mapa actualiza al mismo tiempo mapa, ficha y '
    'gráfica</strong>. Demanda y consumo están validados en navegador.</div>'
    % (modulo("A", "Demanda y pronóstico", spark,
              ["Tres series: demanda, generación y pronóstico.",
               "Línea vertical de la hora actual.",
               "Tarjetas de demanda, generación y balance.",
               "Selección nacional o por gerencia de control."], CIAN),
       modulo("B", "Usuarios y consumo CFE", coro,
              ["Selector Usuarios | Consumo | Intensidad.",
               "Color estable por división: el mismo en mapa, leyenda, ficha y gráfica.",
               "Intensidad en kWh por usuario.",
               "Serie 2022&ndash;2026; 2025 es el último año completo."], VERDE),
       modulo("C", "Generación distribuida", barras_gd,
              ["Capacidad y contratos acumulados por entidad.",
               "Evolución anual por rango de tamaño.",
               "Comparación estatal y línea de tiempo.",
               "Fuente: estadísticas CNE/CRE."], DORADO),
       BLANCO, LINEA, GUINDA, TINTA)
)
write("Indicadores.dc.html", slide(
    8, body08, "Lámina 07", "La bandeja de indicadores.", accent=CIAN))


# ── 09 · Grafo eléctrico ─────────────────────────────────────────────────────
grafo_svg = (
    '<svg viewBox="0 0 430 250" style="width:100%%;height:250px" role="img" '
    'aria-label="Nodos y aristas del grafo eléctrico">'
    '<line x1="60" y1="60" x2="180" y2="45" stroke="#00A9CE" stroke-width="3"/>'
    '<line x1="180" y1="45" x2="300" y2="80" stroke="#00A9CE" stroke-width="3"/>'
    '<line x1="180" y1="45" x2="165" y2="150" stroke="#F2C94C" stroke-width="2.4"/>'
    '<line x1="165" y1="150" x2="300" y2="80" stroke="#F2C94C" stroke-width="2.4"/>'
    '<line x1="165" y1="150" x2="80" y2="205" stroke="%s" stroke-width="2.2" stroke-dasharray="7 5"/>'
    '<line x1="300" y1="80" x2="380" y2="180" stroke="%s" stroke-width="2.2" stroke-dasharray="7 5"/>'
    '<circle cx="60" cy="60" r="10" fill="%s"/>'
    '<circle cx="180" cy="45" r="12" fill="%s"/>'
    '<circle cx="300" cy="80" r="11" fill="%s"/>'
    '<circle cx="165" cy="150" r="10" fill="%s"/>'
    '<circle cx="80" cy="205" r="8" fill="%s" stroke="%s" stroke-width="2"/>'
    '<circle cx="380" cy="180" r="8" fill="%s" stroke="%s" stroke-width="2"/>'
    '<text x="60" y="38" font-family="%s" font-size="11" fill="%s" text-anchor="middle">nodo real</text>'
    '<text x="80" y="230" font-family="%s" font-size="11" fill="%s" text-anchor="middle">nodo virtual</text>'
    '<text x="380" y="205" font-family="%s" font-size="11" fill="%s" text-anchor="middle">nodo virtual</text>'
    '</svg>' % (GRISC, GRISC, GUINDA, GUINDA, GUINDA, GUINDA, BLANCO, GRISC, BLANCO, GRISC,
                MONO, GRIS, MONO, GRISC, MONO, GRISC)
)
api_items = ["Resumen", "Json", "Nodos", "Nodos/{id}/Vecinos", "Ruta &middot; ruta mínima",
             "Revisiones", "GeoJson", "Pam/{proyectoId} &middot; subgrafo", "Simulacion"]
api_html = ('<div style="display:flex;flex-wrap:wrap;gap:7px">%s</div>'
            % "".join('<span style="font-family:%s;font-size:11.5px;font-weight:600;color:%s;'
                      'background:%s;border:1px solid %s;padding:5px 10px">%s</span>'
                      % (MONO, TINTA, PAPEL, LINEAF, a) for a in api_items))
param_rows = [["Radio de búsqueda", "5 km"], ["Tolerancia de extremo", "3 km"],
              ["Fusión de nodos virtuales", "0.1 km"], ["Confianza alta", "85 pts"],
              ["Umbral de revisión", "70 pts"], ["Margen de ambigüedad", "10 pts"]]
body09 = (
    '<div style="display:flex;gap:46px;align-items:flex-start">'
    '<div style="width:430px">%s'
    '<div style="font-size:12px;color:%s;font-family:%s;margin-top:4px;line-height:1.45">Aristas por '
    'color de tensión; trazo discontinuo = extremo sin resolver</div></div>'
    '<div style="flex:1;display:flex;flex-direction:column;gap:20px">%s'
    '<div style="display:flex;gap:24px;align-items:flex-start">'
    '<div style="width:310px">%s</div>'
    '<div style="flex:1">'
    '<div style="font-family:%s;font-size:11px;font-weight:600;letter-spacing:.16em;'
    'text-transform:uppercase;color:%s;margin-bottom:12px">API del grafo</div>%s</div>'
    '</div></div></div>'
    % (grafo_svg, GRISC, MONO,
       bullets([
           "Cada subestación catalogada es un <strong>nodo real</strong>; cada extremo de línea sin "
           "resolver queda como <strong>nodo virtual</strong>, y dos virtuales sólo se fusionan si "
           "la tensión es compatible y están a 100 m o menos.",
           "Cada tramo es una arista con su geometría completa. Estados: <code>conectada</code>, "
           "<code>parcial</code>, <code>sin_resolver</code>.",
           "El cruce visual de dos líneas <strong>no crea conectividad</strong>.",
           "Persistencia versionada en base de datos; sólo una versión activa a la vez.",
       ], GUINDA, 15, 11),
       table(["Parámetro", "Valor"], param_rows, widths=["1fr", "84px"],
             align=["left", "right"], fs=13),
       MONO, GRIS, api_html)
)
write("Grafo.dc.html", slide(
    9, body09, "Lámina 08", "La red es un grafo consultable, no un dibujo.",
    "Versión <code>RED-GRAFO-v1.2</code>. La simulación reconstruye una versión candidata y la "
    "compara contra la activa: informa promociones y regresiones sin publicar nada."))


# ── 10 · Análisis geoespacial ────────────────────────────────────────────────
def paso(n, ttl, cuerpo, extra, color):
    return (
        '<section style="flex:1;display:flex;flex-direction:column;gap:15px;background:%s;'
        'border:1px solid %s;border-top:3px solid %s;padding:24px 26px 28px">'
        '<div style="display:flex;align-items:center;gap:12px">%s'
        '<span style="font-family:%s;font-size:11px;font-weight:600;letter-spacing:.16em;'
        'text-transform:uppercase;color:%s">Paso %s</span></div>'
        '<div style="font-size:21px;font-weight:800;line-height:1.25;color:%s">%s</div>'
        '%s%s</section>'
        % (BLANCO, LINEA, color, num_badge(n, color), MONO, color, n, TINTA, ttl, cuerpo, extra)
    )

modos_html = ('<div style="display:flex;flex-wrap:wrap;gap:7px">%s</div>'
              % "".join(pill(m, GUINDA, PAPEL) for m in
                        ["Clic en el mapa", "Coordenadas lat / lng", "Dirección o lugar",
                         "Atajo territorial"]))
radios_html = ('<div style="display:flex;gap:6px">%s</div>'
               % "".join('<span style="flex:1;text-align:center;font-family:%s;font-size:12px;'
                         'font-weight:600;color:%s;background:%s;border:1px solid %s;'
                         'padding:7px 0">%s</span>'
                         % (MONO, TINTA, PAPEL, LINEAF, r)
                         for r in ["0", "1", "5", "10", "25", "50 km"]))
presets_html = ('<div style="display:flex;flex-wrap:wrap;gap:7px">%s</div>'
                % "".join(pill(p, VERDE, PAPEL) for p in
                          ["Red eléctrica", "Ambiental", "Social", "Patrimonio", "Ductos"]))

body10 = (
    '<div style="display:flex;gap:26px;align-items:stretch">%s%s%s</div>'
    '<div style="display:flex;gap:24px;margin-top:28px">'
    '<div style="flex:1;padding:18px 22px;background:%s;border:1px solid %s;border-left:3px solid %s;'
    'font-size:14.5px;line-height:1.55;color:%s"><strong>Opcional, fuera de la secuencia:</strong> '
    'cargar un dataset propio (CSV, Google Sheets o GeoJSON) para que participe del mismo '
    'análisis.</div>'
    '<div style="flex:1;padding:18px 22px;background:%s;border:1px solid %s;border-left:3px solid %s;'
    'font-size:14.5px;line-height:1.55;color:%s"><strong>Salidas:</strong> conteo por capa, resumen '
    'agregado, ficha por elemento y alimentación directa del reporte territorial.</div>'
    '</div>'
    % (paso(1, "Ubicar el punto de análisis",
            '<div style="font-size:14px;line-height:1.5;color:%s">Cuatro modos. El atajo territorial '
            'analiza el polígono completo de un estado, un municipio o una gerencia.</div>' % GRIS,
            modos_html, GUINDA),
       paso(2, "Definir la distancia",
            '<div style="font-size:14px;line-height:1.5;color:%s">En un atajo territorial, buffer '
            '0 km conserva el límite oficial; un valor mayor amplía todo su contorno.</div>' % GRIS,
            radios_html, CIAN),
       paso(3, "Elegir qué buscar alrededor",
            '<div style="font-size:14px;line-height:1.5;color:%s">Presets temáticos o selección '
            'manual de capas, agrupadas por categoría y con conteo por capa.</div>' % GRIS,
            presets_html, VERDE),
       BLANCO, LINEA, DORADO, TINTA, BLANCO, LINEA, GUINDA, TINTA)
)
write("Analisis.dc.html", slide(
    10, body10, "Lámina 09", "Análisis geoespacial: un flujo de tres pasos."))


# ── 11 · Rendimiento ─────────────────────────────────────────────────────────
costos = [
    ("RAN &middot; núcleos agrarios", 91.3, "#9A3F27"),
    ("Localidades indígenas", 35.4, "#C0552E"),
    ("ANP 2025", 21.6, "#C0552E"),
    ("Regiones indígenas", 12.3, "#C0552E"),
    ("Lenguas indígenas", 10.5, "#C0552E"),
    ("Humedales RAMSAR", 8.2, "#E0A12E"),
    ("ANP estatales", 7.9, "#E0A12E"),
    ("Atlas de pueblos indígenas", 6.1, "#E0A12E"),
    ("Permisos &middot; petrolíferos", 5.5, "#E0A12E"),
    ("Líneas de transmisión", 3.2, "#E0A12E"),
    ("Centrales eléctricas", 2.9, "#E0A12E"),
    ("Divisiones tarifarias", 1.9, "#0E8A6E"),
    ("Permisos &middot; eléctricos", 0.9, "#0E8A6E"),
    ("Subestaciones de transmisión", 0.6, "#0E8A6E"),
]
barras = "".join(
    '<div style="display:flex;align-items:center;gap:14px">'
    '<span style="width:240px;font-size:13.5px;color:%s;line-height:1.3">%s</span>'
    '<span style="flex:1;height:12px;background:%s;position:relative">'
    '<span style="position:absolute;left:0;top:0;bottom:0;width:%.1f%%;background:%s"></span></span>'
    '<span style="width:76px;text-align:right;font-family:%s;font-size:12.5px;font-weight:600;'
    'color:%s">%s MB</span></div>'
    % (TINTA, n, PAPEL, (mb / 91.3) * 100, c, MONO, TINTA, mb) for n, mb, c in costos)

reglas = [
    "Carga diferida por capa y caché en el backend.",
    "Bandeja gráfica montada sólo cuando se solicita.",
    "Puntos masivos en Canvas o agrupación controlada; las subestaciones del grafo se conservan "
    "como elementos individuales.",
    "Geometrías generalizadas en vista nacional y detalle completo al acercarse.",
    "Series horarias separadas de las geometrías para no redibujar el mapa.",
]
body11 = (
    '<div style="display:flex;gap:52px;align-items:flex-start">'
    '<div style="flex:1.2;display:flex;flex-direction:column;gap:9px">%s</div>'
    '<div style="width:440px;display:flex;flex-direction:column;gap:22px">%s'
    '<div style="padding:18px 22px;background:%s;border:1px solid %s;border-left:3px solid %s;'
    'font-size:14.5px;line-height:1.55;color:%s">El costo está <strong>medido</strong>, no supuesto, '
    'y se advierte al usuario antes de correr el análisis. Por eso el tablero arranca sin teselas y '
    'con una sola capa encendida.</div></div></div>'
    % (barras, block("Reglas de rendimiento", bullets(reglas, VERDE, 14, 10), VERDE),
       BLANCO, LINEA, DORADO, TINTA)
)
write("Rendimiento.dc.html", slide(
    11, body11, "Lámina 10", "Rendimiento: decisiones explícitas, no supuestos.", accent=VERDE))


# ── 12 · El reporte territorial ──────────────────────────────────────────────
flujo = [("Portada", "logotipos, metadatos y objetivo", GUINDA),
         ("Índice navegable", "numerado, con cifra por sección", CIRUELA),
         ("Tira de KPIs", "métricas del análisis", CIAN),
         ("Perfil eléctrico", "sección 01, con gráficas", VERDE),
         ("Una sección por capa", "tabla, gráfica y fuente", DORADO),
         ("Cierre", "trazabilidad y fecha de corte", GRIS)]
flujo_html = "".join(
    '<div style="flex:1;display:flex;flex-direction:column;gap:9px;border-top:3px solid %s;'
    'padding-top:14px">'
    '<span style="font-family:%s;font-size:10.5px;font-weight:600;letter-spacing:.12em;'
    'text-transform:uppercase;color:%s">%02d</span>'
    '<span style="font-size:16px;font-weight:800;color:%s;line-height:1.25">%s</span>'
    '<span style="font-size:12.5px;color:%s;line-height:1.4">%s</span></div>'
    % (c, MONO, c, i + 1, TINTA, n, GRIS, d) for i, (n, d, c) in enumerate(flujo))

body12 = (
    '<div style="display:flex;gap:20px;align-items:stretch">%s</div>'
    '<div style="display:flex;gap:24px;margin-top:38px">%s%s</div>'
    % (flujo_html,
       block("Lo que el reporte no hace",
             '<div style="font-size:15px;line-height:1.6;color:%s">Las gráficas se '
             '<strong>omiten</strong> cuando la capa no fue analizada o no produjo elementos. '
             'El documento no finge cobertura: si una capa no entró al análisis, no aparece como '
             'si lo hubiera hecho.</div>' % TINTA, DORADO),
       block("Contexto que se integra automáticamente",
             bullets(["Indicadores INEGI de la entidad y el municipio analizados.",
                      "Histórico tarifario territorial por división CFE.",
                      "Precios de combustibles del área analizada."], CIAN, 14.5, 9),
             CIAN))
)
write("Reporte.dc.html", slide(
    12, body12, "Lámina 11", "El reporte territorial se compone con lo que se analizó.",
    "Se arma en el navegador a partir del análisis vigente; no es una plantilla fija.",
    accent=CIRUELA))


# ── 13 · Exportación ─────────────────────────────────────────────────────────
ICON = {
    "pdf": '<path d="M12 3v12"/><path d="m7 10 5 5 5-5"/><path d="M4 21h16"/>',
    "wide": '<rect x="2" y="3" width="20" height="14" rx="2"/><path d="M8 21h8M12 17v4"/>',
    "xls": '<rect x="3" y="3" width="18" height="18" rx="2"/><path d="M3 9h18M3 15h18M9 3v18"/>',
    "png": '<rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="9" cy="9" r="2"/>'
           '<path d="m21 15-4.5-4.5L6 21"/>',
    "book": '<path d="M2 4h7a3 3 0 0 1 3 3v13a2.5 2.5 0 0 0-2.5-2.5H2z"/>'
            '<path d="M22 4h-7a3 3 0 0 0-3 3v13a2.5 2.5 0 0 1 2.5-2.5H22z"/>',
    "mark": '<path d="M12 21s-6-4.35-6-9.9a6 6 0 0 1 12 0c0 5.55-6 9.9-6 9.9z"/>'
            '<circle cx="12" cy="11" r="2.2"/>',
    "mail": '<rect x="2" y="4" width="20" height="16" rx="2"/><path d="m3 6 9 7 9-7"/>',
}


def salida(icon, ttl, desc, color):
    return (
        '<section style="display:flex;flex-direction:column;gap:12px;background:%s;'
        'border:1px solid %s;border-top:3px solid %s;padding:20px 22px 22px">'
        '<span style="width:34px;height:34px;border-radius:9px;background:%s1A;color:%s;'
        'display:flex;align-items:center;justify-content:center">'
        '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" '
        'stroke-width="2" stroke-linecap="round" stroke-linejoin="round">%s</svg></span>'
        '<span style="font-size:17px;font-weight:800;color:%s;line-height:1.25">%s</span>'
        '<span style="font-size:13.5px;line-height:1.5;color:%s">%s</span></section>'
        % (BLANCO, LINEA, color, color, color, ICON[icon], TINTA, ttl, GRIS, desc)
    )

body13 = (
    '<div style="display:grid;grid-template-columns:repeat(4,1fr);gap:20px">%s%s%s%s</div>'
    '<div style="display:grid;grid-template-columns:repeat(3,1fr);gap:20px;margin-top:20px">%s%s%s</div>'
    '<div style="margin-top:26px;padding:16px 22px;background:%s;border:1px solid %s;'
    'font-size:14px;line-height:1.55;color:%s">El nombre del archivo incorpora el objetivo del '
    'análisis. Las columnas de las tablas del PDF se reparten <strong>midiendo el contenido</strong>, '
    'no por proporción fija.</div>'
    % (salida("pdf", "PDF", "Informe vertical carta, generado desde el propio HTML, con "
                            "portadillas de sección.", GUINDA),
       salida("wide", "PDF 16:9", "Versión horizontal, tipo presentación.", CIRUELA),
       salida("xls", "Excel", "Métricas, análisis y datasets en hojas separadas.", VERDE),
       salida("png", "PNG", "Sólo la imagen del mapa, en alta resolución.", CIAN),
       salida("book", "Modo libro", "El informe se lee pasando hojas carta medidas, no "
                                    "desplazando.", DORADO),
       salida("mark", "Marca de agua", "Logotipo SENER y fecha sobre el mapa exportado.", GRIS),
       salida("mail", "Envío por correo", "Plantilla institucional, con destinatarios "
                                          "controlados.", GUINDA),
       BLANCO, LINEA, TINTA)
)
write("Exportacion.dc.html", slide(
    13, body13, "Lámina 12", "Siete formas de sacar el análisis del tablero.", accent=VERDE))


# ── 14 · PAM en el mapa ──────────────────────────────────────────────────────
reglas_gcr = [
    "Permite localizar, consultar y abrir la ficha del proyecto.",
    "Muestra la leyenda <code>Referencia regional GCR &middot; no es coordenada oficial</code>.",
    "Resalta el polígono completo de la gerencia al seleccionarlo.",
    "Ejecuta el análisis con la geometría de la GCR y buffer 0.",
    "No se persiste en <code>PAMProyectoUbicacion</code>.",
    "No participa como nodo del grafo eléctrico.",
    "Nunca se presenta como ubicación validada.",
]
body14 = (
    '<div style="display:flex;gap:52px;align-items:flex-start">'
    '<div style="width:470px;display:flex;flex-direction:column;gap:28px">'
    '<div style="display:grid;grid-template-columns:repeat(3,1fr);gap:22px">%s%s%s</div>'
    '<div style="padding:18px 22px;background:%s;border:1px solid %s;border-left:3px solid %s;'
    'font-size:14.5px;line-height:1.55;color:%s">Si el campo GCR trae varias regiones '
    '(<code>CE / OR / OC</code>) se analiza la unión. <code>Varias</code> es cobertura '
    'multirregional. Un valor desconocido <strong>no</strong> se sustituye por el territorio '
    'nacional.</div>'
    '<div style="display:flex;flex-wrap:wrap;gap:7px;align-items:center">'
    '<span style="font-family:%s;font-size:10.5px;font-weight:600;letter-spacing:.14em;'
    'text-transform:uppercase;color:%s;width:100%%">Filtros propios de la capa</span>%s%s%s</div>'
    '</div><div style="flex:1">%s</div></div>'
    % (stat("281", "Proyectos en el catálogo", "PAM / PAMRNT", CIRUELA, 46),
       stat("133", "Con asociación de red", "confianza alta", VERDE, 46),
       stat("148", "Representados por su GCR", "sin geometría precisa", DORADO, 46),
       BLANCO, LINEA, CIAN, TINTA, MONO, GRIS,
       pill("Etapa", GUINDA, PAPEL), pill("Licitación", GUINDA, PAPEL), pill("GCR", GUINDA, PAPEL),
       block("Qué hace &mdash; y qué no hace &mdash; el marcador regional",
             bullets(reglas_gcr, CIRUELA, 14.5, 10) +
             '<div style="font-size:13.5px;line-height:1.5;color:%s;margin-top:4px">El marcador se '
             'genera <strong>sólo en el navegador</strong>, en el centroide de la gerencia.</div>'
             % GRIS, CIRUELA))
)
write("PamMapa.dc.html", slide(
    14, body14, "Lámina 13 &middot; PAM", "El PAM en el mapa, con corte al 26 de julio de 2026.",
    accent=CIRUELA))


# ── 15 · Reglas de asociación PAM–red ────────────────────────────────────────
norm = ["Mayúsculas y sin acentos.",
        "Puntuación y guiones homologados; espacios repetidos colapsados.",
        "Prefijos removidos: <code>SE</code>, <code>S.E.</code>, <code>SUBESTACIÓN</code>, "
        "<code>LT</code>, <code>LÍNEA DE TRANSMISIÓN</code>.",
        "Comparación por <strong>palabra completa</strong>, nunca por subcadena.",
        "Alias de gerencia resueltos: BC, BS, MG, NE, NO, NT, OC, OR, CE, PE."]
evid_html = ('<div style="display:flex;flex-wrap:wrap;gap:7px">%s</div>'
             % "".join(pill(e, CIAN, PAPEL) for e in
                       ["Nombre del elemento", "Gerencia de control", "Nivel de tensión",
                        "Número de circuitos", "Longitud", "Homónimos",
                        "Especificidad del nombre", "Conectividad línea&ndash;subestación"]))
pub = ["Una coordenada o geometría manual validada gana sobre cualquier automatismo.",
       "Si el proyecto ya tiene ubicación validada, no se publica una alternativa.",
       "La caída de un GeoJSON no bloquea el catálogo: queda el respaldo por GCR.",
       "<strong>Una coincidencia de texto nunca se convierte en <code>Validada = 1</code>.</strong>",
       "Toda revisión conserva usuario, fecha y observaciones en <code>PAMProyectoUbicacion</code>."]
pub_html = "".join(
    '<li style="display:flex;gap:12px;align-items:flex-start">%s'
    '<span style="font-size:14.5px;line-height:1.5;color:%s">%s</span></li>'
    % (num_badge(i + 1, GUINDA), TINTA, p) for i, p in enumerate(pub))

body15 = (
    '<div style="display:flex;gap:34px;align-items:stretch">%s'
    '<div style="flex:1;display:flex;flex-direction:column;gap:20px">%s%s</div></div>'
    % (block("1 &middot; Normalización previa", bullets(norm, CIAN, 14, 9), CIAN),
       block("2 &middot; Evidencia que suma puntaje", evid_html, VERDE),
       block("3 &middot; Reglas de publicación, en orden",
             '<ul style="list-style:none;margin:0;padding:0;display:flex;flex-direction:column;'
             'gap:11px">%s</ul>' % pub_html, GUINDA))
)
write("PamReglas.dc.html", slide(
    15, body15, "Lámina 14 &middot; PAM", "Cómo se asocia un proyecto con la red.",
    "Versión de reglas <code>PAM-RED-v1.0</code>. La salida es una asociación sugerida y trazable, "
    "nunca una validación."))


# ── 16 · Segunda Convocatoria ────────────────────────────────────────────────
niveles = [
    [pill("A", VERDE, "rgba(14,138,110,.10)"),
     "Mismo folio o identidad de proyecto, con geometría explícita",
     "Candidato geométrico de evidencia alta, <strong>pendiente de validación</strong>"],
    [pill("B", CIAN, "rgba(30,156,184,.10)"),
     "Misma subestación o línea, GCR y tensión compatibles, con nodo catalogado",
     "Refuerza el elemento como candidato pendiente"],
    [pill("C", DORADO_T, "rgba(224,161,46,.14)"),
     "Referencia nominal consistente, elemento ausente o ambiguo",
     "Entra al diagnóstico de cobertura para revisión"],
    [pill("D", TERRA, "rgba(154,63,39,.10)"),
     "Cercanía visual o cruce aparente de una línea",
     "<strong>No crea asociación</strong>"],
]
body16 = (
    '<div style="display:flex;gap:44px;align-items:flex-start">'
    '<div style="flex:1.25">%s</div>'
    '<div style="width:430px;display:flex;flex-direction:column;gap:20px">%s%s</div></div>'
    % (table(["Nivel", "Coincidencia requerida", "Resultado permitido"], niveles,
             widths=["72px", "1fr", "1fr"], fs=14),
       block("La trampa que se evita",
             '<div style="font-size:14.5px;line-height:1.6;color:%s">La geometría de la subestación '
             '<strong>del proyecto privado</strong> no se interpreta como coordenada de la '
             'subestación de la red: se usa para contrastar la distancia declarada. Así no se '
             'publica como infraestructura de CFE o CENACE una elevadora o colectora '
             'particular.</div>' % TINTA, DORADO),
       block("Mesa de revisión en el propio tablero",
             bullets(["Tabla paginada con Mapa, Confirmar y Rechazar con observación.",
                      "Confirmación en lote de los casos firmes.",
                      "Bitácora persistente del dictamen humano, con autorización DGMESNIE.",
                      "Fuente <code>conv2_part</code>, filtrada por la última decisión "
                      "<code>Continúa</code>. No se mezcla con GAT Mixto."], GUINDA, 14, 9),
             GUINDA))
)
write("Conv2.dc.html", slide(
    16, body16, "Lámina 15 &middot; PAM",
    "La Segunda Convocatoria entra como evidencia, no como verdad.", accent=DORADO))


# ── 17 · Ficha ejecutiva PAM ─────────────────────────────────────────────────
laminas = [
    ("01", "Portada", GUINDA), ("02", "Índice", GUINDA), ("02B", "Índice de figuras", GUINDA),
    ("04", "Decisión ejecutiva", CIRUELA), ("05", "Panorama", CIRUELA),
    ("06", "Diagnóstico operativo", CIRUELA),
    ("06B", "Ubicación y RNT", CIAN), ("06C", "Análisis territorial", CIAN),
    ("06C-2", "Perfil energético", CIAN), ("07", "Pronóstico de demanda", CIAN),
    ("08", "Riesgos y mitigación", DORADO), ("09", "Alternativas técnicas", DORADO),
    ("10", "Comparativa trazable", DORADO), ("11", "Evaluación económica", DORADO),
    ("12", "Figuras y diagramas", GRIS),
    ("13", "Fases y alineación", VERDE), ("13A", "Datos de licitación", VERDE),
    ("13B", "Avance físico y financiero", VERDE),
    ("15", "Conclusión ejecutiva", GUINDA), ("16", "Aplicación y trazabilidad", GUINDA),
    ("18", "Historial de versiones", GRIS), ("19", "Línea de tiempo", GRIS),
]
chips = "".join(
    '<div style="display:flex;flex-direction:column;gap:7px;background:%s;border:1px solid %s;'
    'border-top:3px solid %s;padding:13px 14px 15px">'
    '<span style="font-family:%s;font-size:11px;font-weight:600;color:%s;letter-spacing:.08em">%s</span>'
    '<span style="font-size:13px;font-weight:700;color:%s;line-height:1.3">%s</span></div>'
    % (BLANCO, LINEA, c, MONO, c, n, TINTA, t) for n, t, c in laminas)

body17 = (
    '<div style="display:grid;grid-template-columns:repeat(6,1fr);gap:13px">%s</div>'
    '<div style="display:flex;gap:24px;margin-top:30px">'
    '<div style="flex:1;padding:18px 22px;background:%s;border:1px solid %s;border-left:3px solid %s;'
    'font-size:14.5px;line-height:1.55;color:%s">Se abre desde el mapa o desde '
    '<code>/InformePormenorizado/ProyectosIdentificados</code>, se envía por correo desde la propia '
    'ficha y reutiliza exactamente las mismas fuentes territoriales del dashboard.</div>'
    '<div style="flex:1;padding:18px 22px;background:%s;border:1px solid %s;border-left:3px solid %s;'
    'font-size:14.5px;line-height:1.55;color:%s">No es una pantalla de formulario: es una '
    '<strong>presentación que se proyecta</strong>. Cada lámina responde una pregunta de la mesa '
    'de decisión, en el orden en que se hace.</div></div>'
    % (chips, BLANCO, LINEA, CIRUELA, TINTA, BLANCO, LINEA, GUINDA, TINTA)
)
write("FichaPam.dc.html", slide(
    17, body17, "Lámina 16 &middot; PAM", "La ficha ejecutiva es un deck, no un formulario.",
    accent=CIRUELA))


# ── 18 · Fuentes y trazabilidad ──────────────────────────────────────────────
fuentes = [
    ["Subestaciones de transmisión &middot; Atlas", "471",
     "Contraste con el inventario; no sustituye la base de datos"],
    ["Subestaciones de distribución &middot; OSM", "2,037",
     "Referencia secundaria, licencia ODbL"],
    ["Trazos de transmisión &middot; Atlas", "8,715",
     "Contraste geométrico y de conectividad"],
    ["Demanda CENACE", "10 series", "9 regiones cartográficas más el agregado SIN"],
    ["MDA CENACE", "108 zonas", "Backend, 24 valores horarios por zona"],
    ["Generación privada planeada", "18 proyectos", "Asociados a 20 permisos"],
    ["Divisiones tarifarias", "17", "Usuarios y energía por división"],
]
body18 = (
    '<div style="display:flex;gap:46px;align-items:flex-start">'
    '<div style="flex:1.15">%s</div>'
    '<div style="width:450px;display:flex;flex-direction:column;gap:20px">%s%s</div></div>'
    % (table(["Dataset de referencia", "Registros", "Tratamiento DGMESNIE"], fuentes,
             widths=["1fr", "112px", "1.1fr"], align=["left", "right", "left"], fs=14),
       block("Cómo se consume",
             '<div style="font-size:15px;line-height:1.6;color:%s">Atlas publica <strong>archivos '
             'JSON actualizables, no una API jurídica de permisos</strong>. El servicio los consume '
             'con <strong>caché, huella SHA-256 y estado de revisión</strong>. La capa oficial de '
             'permisos sigue saliendo de la base de datos institucional.</div>' % TINTA, GUINDA),
       block("Panel Datos &middot; 20 fuentes registradas",
             bullets(["Cartera institucional de convocatorias.",
                      "PVIRCE &middot; Programa Vinculante 2026&ndash;2040.",
                      "2ª convocatoria: particulares y general.",
                      "GAT Mixto y trámites CNE.",
                      "Producción, generación anual y participación por tecnología.",
                      "Más las fuentes que cargue el propio usuario."], CIAN, 14, 8),
             CIAN))
)
write("Fuentes.dc.html", slide(
    18, body18, "Lámina 17", "Fuentes y trazabilidad.",
    "Corte de la conexión automatizada: 29 de julio de 2026.", accent=CIAN))


# ── 19 · Pendientes ──────────────────────────────────────────────────────────
pend = [
    ["Precios MDA por zona de carga",
     pill("Backend integrado", VERDE, "rgba(14,138,110,.08)"),
     "Capa en mapa, rampa de calor, deslizador horario y curva por zona"],
    ["Transmisión planeada",
     pill("Sin contrato de datos", DORADO_T, "rgba(224,161,46,.12)"),
     "Definir la fuente oficial y separarla de la red existente"],
    ["Tour guiado institucional",
     pill("Diseñado, no implementado", DORADO_T, "rgba(224,161,46,.12)"),
     "Seis pasos, omitibles, sin autoarranque en visitas siguientes"],
    ["Diagramas unifilares CENACE",
     pill("Fuera del puntaje", GRIS, "rgba(111,107,102,.08)"),
     "Incorporarlos como evidencia de asociación"],
    ["Export PDF de la ficha PAM",
     pill("Parcial", DORADO_T, "rgba(224,161,46,.12)"),
     "Cerrar la cadena de extremo a extremo"],
]
dato = [
    "Los GeoJSON <strong>no traen un identificador institucional compartido</strong> con PAM: toda "
    "la asociación se sostiene en nombre, territorio y tensión.",
    "El catálogo de subestaciones <strong>no incluye capacidad MVA, bancos ni "
    "alimentadores</strong>.",
    "Los nombres genéricos siguen exigiendo revisión humana.",
    "La asociación por alias históricos requiere un catálogo curado adicional.",
    "Dos capas caídas en el CDN, con aviso en pantalla y sin abortar el análisis: "
    "<code>municipios_sin_electrificar</code> y <code>localidades_indigenas</code>.",
]
oper = [
    "Credenciales y perfiles de acceso por rol, al cierre del despliegue.",
    "Los 148 proyectos PAM regionales sólo mejoran con validación documental: no hay automatismo "
    "que los convierta en coordenada.",
]
body19 = (
    '<div style="display:flex;gap:40px;align-items:flex-start">'
    '<div style="flex:1.25">'
    '<div style="font-family:%s;font-size:11px;font-weight:600;letter-spacing:.16em;'
    'text-transform:uppercase;color:%s;margin-bottom:14px">Pendientes de producto</div>%s</div>'
    '<div style="width:470px;display:flex;flex-direction:column;gap:18px">%s%s</div></div>'
    % (MONO, DORADO_T,
       table(["Tema", "Estado real", "Qué falta"], pend,
             widths=["1fr", "196px", "1.15fr"], fs=13.5),
       block("Pendientes de dato y de fuente", bullets(dato, DORADO, 13.5, 9), DORADO),
       block("Deuda operativa", bullets(oper, TERRA, 13.5, 9), TERRA))
)
write("Pendientes.dc.html", slide(
    19, body19, "Lámina 18", "Qué está pendiente, y por qué.",
    "Ninguno es un problema de arquitectura: son contratos de dato, de fuente y de validación.",
    accent=DORADO))


# ── 20 · Cierre ──────────────────────────────────────────────────────────────
mensajes = [
    ("El mapa es el método, no el adorno.",
     "Ubicar, delimitar, analizar y documentar es un flujo de tres pasos que termina en un "
     "documento firmable.", GUINDA),
    ("Nada se valida solo.",
     "El tablero propone, puntúa y deja rastro. La validación es humana y queda en bitácora, con "
     "usuario, fecha y observaciones.", CIAN),
    ("Lo que falta está identificado y acotado.",
     "MDA en mapa, transmisión planeada y diagramas unifilares son trabajo de fuente, no de "
     "arquitectura.", VERDE),
]
mens_html = "".join(
    '<section style="flex:1;display:flex;flex-direction:column;gap:16px;border-top:3px solid %s;'
    'padding-top:20px">%s'
    '<div style="font-size:26px;font-weight:800;line-height:1.2;color:%s;text-wrap:balance">%s</div>'
    '<div style="font-size:15px;line-height:1.55;color:%s;text-wrap:pretty">%s</div></section>'
    % (c, num_badge(i + 1, c), TINTA, t, GRIS, d) for i, (t, d, c) in enumerate(mensajes))

body20 = (
    '<div style="display:flex;gap:40px;align-items:stretch">%s</div>'
    '<div style="display:flex;align-items:center;gap:20px;margin-top:56px;padding-top:24px;'
    'border-top:1px solid %s">'
    '<span style="font-size:14px;color:%s">Documento de respaldo: '
    '<code>PRESENTACION_DASHBOARD_TERRITORIAL.md</code></span>'
    '<span style="flex:1"></span>'
    '<img src="./logo_sener.png" alt="Secretaría de Energía" style="height:34px;width:auto">'
    '</div>' % (mens_html, LINEA, GRIS)
)
write("Cierre.dc.html", slide(
    20, body20, "Lámina 19", "Tres mensajes para llevarse de la sesión.", pad_top=60))


# ── canvas.json ──────────────────────────────────────────────────────────────
ORDER = [
    "Main.dc.html", "QueEs.dc.html", "Principio.dc.html", "Anatomia.dc.html",
    "CapasOperacion.dc.html", "CapasPlaneacion.dc.html", "Contexto.dc.html",
    "Indicadores.dc.html", "Grafo.dc.html", "Analisis.dc.html", "Rendimiento.dc.html",
    "Reporte.dc.html", "Exportacion.dc.html", "PamMapa.dc.html", "PamReglas.dc.html",
    "Conv2.dc.html", "FichaPam.dc.html", "Fuentes.dc.html", "Pendientes.dc.html",
    "Cierre.dc.html",
]
COLS, GAP_X, GAP_Y = 4, 140, 170
artboards = [{
    "file": f,
    "x": (i % COLS) * (W + GAP_X),
    "y": (i // COLS) * (H + GAP_Y),
    "w": W, "h": H, "print": "fixed",
} for i, f in enumerate(ORDER)]

canvas = {
    "artboards": artboards,
    "annotations": [
        {"id": "orden-lectura", "x": 0, "y": -150, "w": 640,
         "text": "Orden de lectura: izquierda a derecha, fila por fila.\n"
                 "20 láminas 16:9 (1600 x 900).\n"
                 "Contenido y cifras: PRESENTACION_DASHBOARD_TERRITORIAL.md"},
        {"id": "bloque-pam", "x": 3 * (W + GAP_X) + W + 60, "y": 3 * (H + GAP_Y), "w": 330,
         "text": "Bloque PAM: láminas 13 a 16.\n"
                 "Si la sesión se acorta, éste es el bloque que conviene conservar completo."},
    ],
    "launch": {"view": "canvas"},
}
with open(os.path.join(OUT, "canvas.json"), "w", encoding="utf-8") as fh:
    json.dump(canvas, fh, ensure_ascii=False, indent=2)

print("Laminas generadas: %d" % len(ORDER))
