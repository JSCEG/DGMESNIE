import os

workspace = r"c:\Proyectos\1.-DGMESNIE"
exclude_dirs = { "node_modules", "bin", "obj", ".vs", ".git" }
search_term = "340026100031426"

matches = []

for root, dirs, files in os.walk(workspace):
    # filter exclude dirs in-place
    dirs[:] = [d for d in dirs if d not in exclude_dirs]
    for file in files:
        if file.endswith((".js", ".json", ".sql", ".md", ".cs", ".cshtml", ".txt")):
            file_path = os.path.join(root, file)
            try:
                with open(file_path, "r", encoding="utf-8", errors="ignore") as f:
                    content = f.read()
                    if search_term in content:
                        matches.append(file_path)
            except Exception as e:
                pass

print("Matches found in files:")
for m in matches:
    print(" -", m)
