import openpyxl
import json
import datetime

# Helper to serialize datetime to ISO string
def date_serializer(obj):
    if isinstance(obj, (datetime.datetime, datetime.date)):
        return obj.isoformat()
    raise TypeError(f"Type {type(obj)} not serializable")

wb_path = r"c:\Proyectos\1.-DGMESNIE\wwwroot\documents\BD_Privados_FirmesCGPT.xlsx"
wb = openpyxl.load_workbook(wb_path, data_only=True)

# 1. Read classification catalog if available, or we will use standard catalog seed
# 2. Read Tramites_Ambientales sheet
sheet_ta = wb["Tramites_Ambientales"]
rows_ta = list(sheet_ta.iter_rows(values_only=True))
headers_ta = [str(cell).strip() for cell in rows_ta[0] if cell is not None]

projects = []
companies = set()
technologies = set()

# Columns indexes map
col_idx = {name: idx for idx, name in enumerate(headers_ta)}

for row in rows_ta[1:]:
    # Filter empty rows
    if row[0] is None and row[1] is None:
        continue
    
    p = {}
    for name, idx in col_idx.items():
        if idx < len(row):
            val = row[idx]
            p[name] = val
        else:
            p[name] = None
            
    projects.append(p)
    
    # Collect companies
    empresa_name = p.get("Grupo Económico") or p.get("Promovente")
    if empresa_name:
        companies.add(str(empresa_name).strip())
        
    promovente = p.get("Promovente")
    if promovente:
        companies.add(str(promovente).strip())

    # Collect technologies (Tipo)
    tech = p.get("Tipo")
    if tech:
        technologies.add(str(tech).strip())

# 3. Read Bitacora sheet
sheet_bit = wb["Bitácora"]
rows_bit = list(sheet_bit.iter_rows(values_only=True))
headers_bit = [str(cell).strip() for cell in rows_bit[0] if cell is not None]

bitacoras = []
meetings = {} # Key: (fecha, minuta) -> details

col_idx_bit = {name: idx for idx, name in enumerate(headers_bit)}

for row in rows_bit[1:]:
    if row[0] is None and row[2] is None:
        continue
    
    b = {}
    for name, idx in col_idx_bit.items():
        if idx < len(row):
            val = row[idx]
            b[name] = val
        else:
            b[name] = None
    
    bitacoras.append(b)

data = {
    "projects": projects,
    "bitacoras": bitacoras,
    "companies": list(companies),
    "technologies": list(technologies)
}

# Save as JSON
with open(r"c:\Proyectos\1.-DGMESNIE\excel_data.json", "w", encoding="utf-8") as f:
    json.dump(data, f, default=date_serializer, indent=4, ensure_ascii=False)

print("Successfully dumped Excel data to excel_data.json")
print(f"Total projects: {len(projects)}")
print(f"Total bitacora entries: {len(bitacoras)}")
print(f"Total unique company names: {len(companies)}")
print(f"Total technologies: {len(technologies)}")
