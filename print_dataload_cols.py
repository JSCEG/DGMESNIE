with open(r"C:\Users\User\.gemini\antigravity\brain\a0d32448-86f7-484b-9078-54b244a9df47\scratch\SQL_03_DataLoad.txt", "r", encoding="utf-8") as f:
    lines = f.readlines()

for line in lines:
    if "INSERT INTO dgmesnie.Proyecto" in line:
        print("Length of line:", len(line))
        print("Line content:")
        print(line[:800])
        # Find where columns list ends
        idx = line.find("VALUES")
        if idx != -1:
            cols_str = line[:idx]
            print("\nColumns:")
            print(cols_str)
        break
