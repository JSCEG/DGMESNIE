import json
import os

# 1. Leer la cadena de conexión de appsettings.json
appsettings_path = r"c:\Proyectos\1.-DGMESNIE\appsettings.json"
if not os.path.exists(appsettings_path):
    print("Error: No se encontró appsettings.json")
    exit(1)

with open(appsettings_path, 'r', encoding='utf-8') as f:
    config = json.load(f)

conn_str = config.get("ConnectionStrings", {}).get("DefaultConnection", "")
if not conn_str:
    print("Error: No se encontró la cadena de conexión DefaultConnection")
    exit(1)

print("Cadena de conexión leída correctamente.")

# 2. Intentar importar bibliotecas de SQL Server
try:
    import pyodbc
    driver = "{ODBC Driver 17 for SQL Server}"
    # Formatear la cadena de conexión para pyodbc si es necesario
    # Dapper/Entity Framework usa formato ADO.NET: "Server=tcp:...;Initial Catalog=...;User ID=...;Password=...;"
    # pyodbc puede consumirla directamente si le agregamos el DRIVER.
    pyodbc_conn_str = f"DRIVER={driver};" + conn_str
    print("Conectando con pyodbc...")
    conn = pyodbc.connect(pyodbc_conn_str, autocommit=True)
except Exception as e_odbc:
    print(f"No se pudo conectar con pyodbc: {e_odbc}")
    try:
        import pymssql
        # Extraer parámetros para pymssql
        # Server=tcp:servidorsqljavidev.database.windows.net,1433;Initial Catalog=BDPruebasSNIER;User ID=adminsql;Password=...;
        parts = {}
        for part in conn_str.split(';'):
            if '=' in part:
                k, v = part.split('=', 1)
                parts[k.strip().lower()] = v.strip()
        
        server_full = parts.get('server', '')
        # remover tcp: y el puerto
        server = server_full.replace('tcp:', '').split(',')[0]
        port = server_full.split(',')[1] if ',' in server_full else '1433'
        user = parts.get('user id', '')
        password = parts.get('password', '')
        database = parts.get('initial catalog', '')
        
        print("Conectando con pymssql...")
        import pymssql
        conn = pymssql.connect(server=server, port=port, user=user, password=password, database=database, autocommit=True)
    except Exception as e_mssql:
        print(f"No se pudo conectar con pymssql: {e_mssql}")
        print("Instale pyodbc o pymssql, o ejecute el script SQL_07_TruncateCore.sql manualmente en SSMS.")
        exit(1)

# 3. Leer y ejecutar el script SQL
sql_file_path = r"c:\Proyectos\1.-DGMESNIE\SQL_07_TruncateCore.sql"
if not os.path.exists(sql_file_path):
    print(f"Error: No se encontró {sql_file_path}")
    exit(1)

with open(sql_file_path, 'r', encoding='utf-8') as f:
    sql_script = f.read()

print("Ejecutando script de limpieza...")
cursor = conn.cursor()

# Ejecutar las sentencias divididas por bloques o de forma secuencial
# Para evitar errores con comandos GO u otros, ejecutamos en bloques lógicos.
# El script SQL_07_TruncateCore.sql no contiene GO, contiene bloques declarativos.
try:
    # Dividir el script por comentarios principales o ejecutarlo completo
    # Ejecutar las sentencias completas
    cursor.execute(sql_script)
    print("¡La base de datos canónica y staging han sido limpiadas y reestablecidas a cero!")
except Exception as e:
    print(f"Error al ejecutar el script de limpieza: {e}")
finally:
    conn.close()
