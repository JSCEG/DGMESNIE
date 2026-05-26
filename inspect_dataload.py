with open(r"C:\Users\User\.gemini\antigravity\brain\a0d32448-86f7-484b-9078-54b244a9df47\scratch\SQL_03_DataLoad.txt", "r", encoding="utf-8") as f:
    lines = f.readlines()

print("--- Printing first few non-empty lines that contain INSERT INTO dgmesnie.Proyecto ---")
count = 0
for line in lines:
    if "INSERT INTO dgmesnie.Proyecto" in line:
        print(line.strip()[:400])
        count += 1
        if count >= 10:
            break
