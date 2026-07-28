#!/usr/bin/env python3
"""Build a compact, traceable CFE RGD substation catalog from the source PDF."""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import tempfile
import unicodedata
import xml.etree.ElementTree as ET
from datetime import datetime, timezone
from pathlib import Path


BANK_PATTERN = re.compile(
    r"\b(?P<bank>\d{2}-[^\s-]{3}-\d{2,3}-\d+)\b",
    re.IGNORECASE,
)
NUMBER_PATTERN = re.compile(r"\d+(?:\.\d+)?")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--pdf", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--pdftotext", default="pdftotext")
    return parser.parse_args()


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def clean(value: str) -> str:
    return re.sub(r"\s+", " ", value).strip()


def fold(value: str) -> str:
    decomposed = unicodedata.normalize("NFD", value)
    return "".join(
        character
        for character in decomposed
        if unicodedata.category(character) != "Mn"
    ).upper()


def extract_bbox(pdf: Path, pdftotext: str) -> ET.Element:
    with tempfile.TemporaryDirectory(prefix="cfe-rgd-") as directory:
        output = Path(directory) / "bbox.html"
        subprocess.run(
            [pdftotext, "-bbox-layout", str(pdf), str(output)],
            check=True,
        )
        return ET.parse(output).getroot()


def parse_records(document: ET.Element) -> list[dict[str, object]]:
    records: list[dict[str, object]] = []
    diagnostics = {
        "bankWords": 0,
        "missingRowNumber": 0,
        "missingValues": 0,
        "missingTextColumns": 0,
        "missingDivision": 0,
        "missingZone": 0,
        "missingSubstation": 0,
    }
    pages = [
        element
        for element in document.iter()
        if element.tag.rsplit("}", 1)[-1] == "page"
    ]
    for page_number, page in enumerate(pages, start=1):
        words = []
        for element in page.iter():
            if element.tag.rsplit("}", 1)[-1] != "word":
                continue
            text = clean("".join(element.itertext()))
            if not text:
                continue
            x_min = float(element.attrib["xMin"])
            x_max = float(element.attrib["xMax"])
            y_min = float(element.attrib["yMin"])
            y_max = float(element.attrib["yMax"])
            words.append(
                {
                    "text": text,
                    "folded": fold(text),
                    "xMin": x_min,
                    "xMax": x_max,
                    "xCenter": (x_min + x_max) / 2,
                    "yCenter": (y_min + y_max) / 2,
                }
            )

        header_words = None
        for candidate in words:
            if candidate["folded"] != "DIVISION":
                continue
            same_line = [
                word
                for word in words
                if abs(word["yCenter"] - candidate["yCenter"]) <= 0.75
            ]
            labels = {word["folded"] for word in same_line}
            if {"DIVISION", "ZONA", "SUBESTACION", "BANCO"} <= labels:
                header_words = same_line
                break
        if header_words is None:
            continue

        label_centers = {
            label: next(
                word["xCenter"]
                for word in header_words
                if word["folded"] == label
            )
            for label in ("DIVISION", "ZONA", "SUBESTACION", "BANCO")
        }
        division_start = label_centers["DIVISION"] - (
            label_centers["ZONA"] - label_centers["DIVISION"]
        ) / 2
        zone_start = (
            label_centers["DIVISION"] + label_centers["ZONA"]
        ) / 2
        substation_start = (
            label_centers["ZONA"] + label_centers["SUBESTACION"]
        ) / 2

        bank_words = [
            word
            for word in words
            if BANK_PATTERN.fullmatch(word["text"])
        ]
        diagnostics["bankWords"] += len(bank_words)
        for bank_word in bank_words:
            row_words = sorted(
                (
                    word
                    for word in words
                    if abs(word["yCenter"] - bank_word["yCenter"]) <= 0.75
                ),
                key=lambda word: word["xMin"],
            )
            row_number_candidates = [
                word
                for word in row_words
                if word["xCenter"] < division_start
                and re.fullmatch(r"\d+", word["text"])
            ]
            values = [
                word["text"]
                for word in row_words
                if word["xMin"] > bank_word["xMax"]
                and NUMBER_PATTERN.fullmatch(word["text"])
            ]
            if not row_number_candidates:
                diagnostics["missingRowNumber"] += 1
                continue
            if len(values) < 3:
                diagnostics["missingValues"] += 1
                continue
            division = clean(
                " ".join(
                    word["text"]
                    for word in row_words
                    if division_start <= word["xCenter"] < zone_start
                )
            )
            zone = clean(
                " ".join(
                    word["text"]
                    for word in row_words
                    if zone_start <= word["xCenter"] < substation_start
                )
            )
            substation = clean(
                " ".join(
                    word["text"]
                    for word in row_words
                    if substation_start <= word["xCenter"]
                    and word["xMax"] < bank_word["xMin"]
                )
            )
            if (
                not division
                or not zone
                or not substation
            ):
                diagnostics["missingTextColumns"] += 1
                diagnostics["missingDivision"] += int(not division)
                diagnostics["missingZone"] += int(not zone)
                diagnostics["missingSubstation"] += int(not substation)
                continue
            records.append(
                {
                    "rowNumber": int(row_number_candidates[0]["text"]),
                    "division": division,
                    "zone": zone,
                    "substation": substation,
                    "bank": bank_word["text"].upper(),
                    "atKv": float(values[0]),
                    "mtKv": float(values[1]),
                    "capacityMva": float(values[2]),
                    "sourcePage": page_number,
                }
            )
    print(json.dumps({"parseDiagnostics": diagnostics}, ensure_ascii=False))
    return records


def main() -> None:
    args = parse_args()
    pdf = args.pdf.resolve()
    if not pdf.is_file():
        raise SystemExit(f"Source PDF not found: {pdf}")

    document = extract_bbox(pdf, args.pdftotext)
    records = parse_records(document)
    if len(records) < 2_500:
        raise SystemExit(
            f"Only {len(records)} table rows were parsed; expected at least 2,500."
        )

    divisions = sorted({str(record["division"]) for record in records})
    payload = {
        "version": "CFE-RGD-2026-v1",
        "documentTitle": (
            "Valores de Corto Circuito de las Redes Generales de Distribucion 2028"
        ),
        "publicationYear": 2026,
        "horizonYear": 2028,
        "sourceFile": pdf.name,
        "sourceSha256": sha256(pdf),
        "generatedUtc": datetime.now(timezone.utc).isoformat(),
        "extractionMethod": (
            "pdftotext -bbox-layout; word-center table-column parser"
        ),
        "auditRule": (
            "Exact or controlled-alias name, compatible territory, and declared "
            "voltage present as AT or MT. Evidence remains pending human validation."
        ),
        "recordCount": len(records),
        "divisions": divisions,
        "records": records,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(
        json.dumps(payload, ensure_ascii=False, separators=(",", ":")),
        encoding="utf-8",
    )
    print(
        json.dumps(
            {
                "output": str(args.output.resolve()),
                "records": len(records),
                "divisions": len(divisions),
                "sha256": payload["sourceSha256"],
            },
            ensure_ascii=False,
        )
    )


if __name__ == "__main__":
    main()
