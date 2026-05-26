import openpyxl

wb_path = r"c:\Proyectos\1.-DGMESNIE\wwwroot\documents\BD_Privados_FirmesCGPT.xlsx"
wb = openpyxl.load_workbook(wb_path, read_only=True)

if "Bitácora" in wb.sheetnames:
    sheet = wb["Bitácora"]
    headers = [str(cell).strip() for cell in next(sheet.iter_rows(max_row=1, values_only=True)) if cell is not None]
    print("Bitacora headers:", headers)
    # Print first row of data
    for row in sheet.iter_rows(min_row=2, max_row=4, values_only=True):
        print("Row:", row)
else:
    print("Bitacora sheet not found.")
