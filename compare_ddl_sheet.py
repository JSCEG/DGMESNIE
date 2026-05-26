import openpyxl
import re

wb_path = r"c:\Proyectos\1.-DGMESNIE\wwwroot\documents\BD_Privados_FirmesCGPT.xlsx"
wb = openpyxl.load_workbook(wb_path, read_only=True)
sheet = wb["Tramites_Ambientales"]

# Get Excel columns
excel_cols = []
for cell in next(sheet.iter_rows(max_row=1, values_only=True)):
    if cell is not None:
        excel_cols.append(str(cell).strip())

# Read DDL columns from SQL_01_DDL.txt for table dgmesnie.Proyecto
ddl_cols = []
ddl_path = r"C:\Users\User\.gemini\antigravity\brain\a0d32448-86f7-484b-9078-54b244a9df47\scratch\SQL_01_DDL.txt"
with open(ddl_path, "r", encoding="utf-8") as f:
    ddl_content = f.read()

# Try to find dgmesnie.Proyecto columns
m = re.search(r"CREATE TABLE dgmesnie\.Proyecto \((.*?)\);", ddl_content, re.DOTALL | re.IGNORECASE)
if m:
    table_body = m.group(1)
    for line in table_body.split("\n"):
        line = line.strip()
        if line and not line.startswith("CONSTRAINT") and not line.startswith("PRIMARY KEY") and not line.startswith("FOREIGN KEY"):
            parts = line.split()
            if parts:
                col_name = parts[0].replace("[", "").replace("]", "")
                ddl_cols.append(col_name)

print("Excel Columns in Tramites_Ambientales:")
for col in excel_cols:
    # See if there's a match in ddl
    matched = False
    for ddl_col in ddl_cols:
        if ddl_col.lower() in col.lower() or col.lower() in ddl_col.lower():
            matched = ddl_col
            break
    print(f"  - {col} (DDL Match: {matched if matched else 'None'})")

print("\nAll DDL Columns for dgmesnie.Proyecto:")
print(", ".join(ddl_cols))
