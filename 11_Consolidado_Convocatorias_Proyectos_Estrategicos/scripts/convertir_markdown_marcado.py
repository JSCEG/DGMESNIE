from __future__ import annotations

import re
import sys
from pathlib import Path


def escape_latex(value: str) -> str:
    replacements = [
        ("\\", r"\textbackslash{}"),
        ("&", r"\&"),
        ("%", r"\%"),
        ("$", r"\$"),
        ("#", r"\#"),
        ("_", r"\_"),
        ("{", r"\{"),
        ("}", r"\}"),
        ("~", r"\textasciitilde{}"),
        ("^", r"\textasciicircum{}"),
        ("—", "--"),
        ("–", "--"),
    ]
    for source, target in replacements:
        value = value.replace(source, target)
    return value


def inline(value: str) -> str:
    protected: list[str] = []

    def protect(command: str) -> str:
        token = f"ZZTOKEN{len(protected)}ZZ"
        protected.append(command)
        return token

    referencias = {
        "6": r"\referenciaBase{}",
        "15": r"\referenciaPrimera{}",
        "9": r"\referenciaSegunda{}",
    }
    value = re.sub(
        r"\[cite:([^\]]+)\]",
        lambda match: protect(
            "".join(referencias.get(item.strip(), r"\referenciaFuente{}") for item in match.group(1).split(","))
        ),
        value,
    )
    value = re.sub(
        r"\[(https?://[^\]]+)\]\((https?://[^)]+)\)",
        lambda match: protect(r"\href{" + match.group(2) + r"}{\texttt{enlace}}"),
        value,
    )

    marks = {
        "***": protect(r"\marcaTercera{}"),
        "**": protect(r"\marcaSegunda{}"),
        "*": protect(r"\marcaPrimera{}"),
    }
    for source, target in marks.items():
        value = value.replace(source, target)

    value = escape_latex(value)
    for index, command in enumerate(protected):
        value = value.replace(f"ZZTOKEN{index}ZZ", command)
    return value


def split_table_row(line: str) -> list[str]:
    stripped = line.strip()
    if stripped.startswith("|"):
        stripped = stripped[1:]
    if stripped.endswith("|"):
        stripped = stripped[:-1]
    return [cell.strip() for cell in stripped.split("|")]


def is_table_separator(line: str) -> bool:
    cells = split_table_row(line)
    return bool(cells) and all(re.fullmatch(r":?-{1,}:?", cell) for cell in cells)


def is_convocatoria_point(line: str) -> bool:
    return bool(re.match(r"^(?:\d+(?:\.\d+)*\.|[IVXLCDM]+\.)\s+", line))


def table_widths(count: int) -> list[str]:
    presets = {
        2: ["0.25", "0.735"],
        3: ["0.23", "0.34", "0.39"],
        4: ["0.17", "0.24", "0.25", "0.29"],
        5: ["0.16", "0.19", "0.22", "0.25", "0.13"],
        6: ["0.14", "0.16", "0.17", "0.18", "0.18", "0.12"],
    }
    if count in presets:
        return presets[count]
    width = round(0.97 / max(count, 1), 3)
    return [str(width)] * count


def render_table(lines: list[str]) -> list[str]:
    rows = [split_table_row(line) for line in lines]
    header = rows[0]
    body = rows[2:]
    count = len(header)
    widths = table_widths(count)
    spec = "@{}" + "".join(
        ">" + r"{\raggedright\arraybackslash}p{" + width + r"\textwidth}"
        for width in widths
    ) + "@{}"
    output = [f"\\begin{{longtable}}{{{spec}}}", "\\toprule"]
    output.append(" & ".join(r"\senerth{" + inline(cell) + "}" for cell in header) + r" \\")
    output.extend(["\\midrule", "\\endfirsthead", "\\toprule"])
    output.append(" & ".join(r"\senerth{" + inline(cell) + "}" for cell in header) + r" \\")
    output.extend(["\\midrule", "\\endhead"])
    for row in body:
        row = row + [""] * (count - len(row))
        output.append(" & ".join(inline(cell) for cell in row[:count]) + r" \\")
    output.extend(["\\bottomrule", "\\end{longtable}", ""])
    return output


def convert(source: Path, destination: Path) -> None:
    lines = source.read_text(encoding="utf-8-sig").splitlines()
    output: list[str] = [
        "% Generated consolidated text; do not edit manually.",
        "% Source records are preserved in the project archive.",
        "",
    ]
    index = 0
    while index < len(lines):
        line = lines[index].rstrip()
        if not line.strip():
            output.append("")
            index += 1
            continue

        if line.startswith("|") and line.rstrip().endswith("|"):
            table = []
            while index < len(lines):
                candidate = lines[index].rstrip()
                if not (candidate.startswith("|") and candidate.endswith("|")):
                    break
                table.append(candidate)
                index += 1
            if len(table) >= 2 and is_table_separator(table[1]):
                output.extend(render_table(table))
            else:
                output.extend(inline(row) + r"\\" for row in table)
            continue

        heading = re.match(r"^(#{1,4})\s+(.*)$", line)
        if heading:
            level = len(heading.group(1))
            title = inline(heading.group(2))
            command = {1: "subsection*", 2: "subsubsection*", 3: "paragraph*", 4: "subparagraph*"}[level]
            output.append(f"\\{command}{{{title}}}")
            if level >= 3:
                output.extend(["\\mbox{}\\par", "\\nopagebreak[4]"])
            output.append("")
            index += 1
            continue

        if line.startswith("---"):
            output.extend(["\\medskip", ""])
            index += 1
            continue

        if line.startswith("- "):
            output.append("\\begin{itemize}")
            while index < len(lines) and lines[index].startswith("- "):
                output.append(r"\item " + inline(lines[index][2:].rstrip()))
                index += 1
            output.extend(["\\end{itemize}", ""])
            continue

        rendered = inline(line)
        if is_convocatoria_point(line):
            output.append(r"\puntoConvocatoria{" + rendered + "}")
        else:
            output.append(rendered)
        output.append("")
        index += 1

    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text("\n".join(output) + "\n", encoding="utf-8")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise SystemExit("Uso: convertir_markdown_marcado.py entrada.md salida.tex")
    convert(Path(sys.argv[1]), Path(sys.argv[2]))
