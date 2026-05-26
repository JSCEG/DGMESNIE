ddl_path = r"C:\Users\User\.gemini\antigravity\brain\a0d32448-86f7-484b-9078-54b244a9df47\scratch\SQL_01_DDL.txt"
with open(ddl_path, "r", encoding="utf-8") as f:
    lines = f.readlines()

in_table = False
for line in lines:
    if "CREATE TABLE dgmesnie.Proyecto" in line:
        in_table = True
    if in_table:
        print(line, end="")
        if "CONSTRAINT" in line and ");" in line:
            # wait, end of table
            pass
        if in_table and line.strip() == ");":
            in_table = False
            break
        if in_table and line.strip() == "GO":
            in_table = False
            break
