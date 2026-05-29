import pandas as pd
import openpyxl
import os

input_file = r"c:\Proyectos\1.-DGMESNIE\Insumos\pvirce.xlsx"
output_file = r"c:\Proyectos\1.-DGMESNIE\Insumos\pvirce_formateado.xlsx"

if not os.path.exists(input_file):
    print(f"Error: No se encontró el archivo {input_file}")
    exit(1)

# Leer usando pandas
print(f"Leyendo {input_file}...")
df = pd.read_excel(input_file, sheet_name=0)

print("Columnas detectadas en origen:", df.columns.tolist())

# Construir el DataFrame destino con las 10 columnas en el orden esperado
# 1. ID
# 2. PreFolioRaw
# 3. FolioProyectoRaw
# 4. RFCRaw
# 5. NombreRaw
# 6. ProyectoRaw
# 7. DescripcionProyectoRaw
# 8. GrupoInteresRaw
# 9. EsHibridaRaw
# 10. TipoTecnologiaRaw

df_formatted = pd.DataFrame()

# Columna A: ID
df_formatted['IDRaw'] = range(1, len(df) + 1)

# Columna B: PreFolioRaw (usamos Status_VF)
df_formatted['PreFolioRaw'] = df['Status_VF'].fillna('')

# Columna C: FolioProyectoRaw (usamos el Nombre real del proyecto como clave única)
df_formatted['FolioProyectoRaw'] = df['Nombre real'].fillna('')

# Columna D: RFCRaw (Vacío)
df_formatted['RFCRaw'] = ''

# Columna E: NombreRaw (Promovente - Vacío o No especificado)
df_formatted['NombreRaw'] = 'No especificado'

# Columna F: ProyectoRaw (Nombre real del proyecto)
df_formatted['ProyectoRaw'] = df['Nombre real'].fillna('')

# Columna G: DescripcionProyectoRaw (Adiciones o sustituciones o capacidad MW)
df_formatted['DescripcionProyectoRaw'] = df.apply(
    lambda row: f"{row['Adiciones o sustituciones']} - {row['MW']} MW" if pd.notna(row['MW']) else str(row['Adiciones o sustituciones']),
    axis=1
)

# Columna H: GrupoInteresRaw (Vacío)
df_formatted['GrupoInteresRaw'] = ''

# Columna I: EsHibridaRaw (NO por defecto)
df_formatted['EsHibridaRaw'] = 'NO'

# Columna J: TipoTecnologiaRaw (Mapear Tipo_VF a nombres descriptivos si aplica, o usar directamente)
tech_map = {
    'FV': 'Solar Fotovoltaica',
    'FV ': 'Solar Fotovoltaica',
    'PE': 'Eólica',
    'PE ': 'Eólica',
    'TER': 'Termoeléctrica',
    'CC': 'Ciclo Combinado',
    'HID': 'Hidroeléctrica',
    'GEO': 'Geotérmica'
}
df_formatted['TipoTecnologiaRaw'] = df['Tipo_VF'].astype(str).str.strip().map(lambda x: tech_map.get(x, x))

# Guardar en nuevo Excel
print(f"Guardando archivo ordenado en {output_file}...")
df_formatted.to_excel(output_file, index=False)
print("¡Archivo formateado exitosamente!")
