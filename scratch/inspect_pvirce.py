import openpyxl
import os

file_path = r"c:\Proyectos\1.-DGMESNIE\Insumos\pvirce.xlsx"

if not os.path.exists(file_path):
    print(f"Error: File not found at {file_path}")
    exit(1)

print(f"Inspecting file: {file_path}")
wb = openpyxl.load_workbook(file_path, read_only=True)
print(f"Sheets: {wb.sheetnames}")

# Read first sheet
sheet = wb.active
print(f"Active Sheet Name: {sheet.title}")

print("\nFirst 10 rows:")
for r_idx, row in enumerate(sheet.iter_rows(max_row=10, values_only=True), start=1):
    print(f"Row {r_idx}: {row}")
