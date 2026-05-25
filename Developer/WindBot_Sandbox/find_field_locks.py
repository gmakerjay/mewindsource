import os
import re
import sqlite3
import json

PROJECT_ROOT = r"c:\Users\admin\Documents\EDOTh"
SCRIPT_DIR = os.path.join(PROJECT_ROOT, "script")
DB_PATH = os.path.join(PROJECT_ROOT, "cards.cdb")

# Regex to find effect declarations: e1 = Effect.CreateEffect(c)
effect_decl_re = re.compile(r"(\w+)\s*=\s*Effect\.CreateEffect")

def get_card_info(card_id):
    if not os.path.exists(DB_PATH):
        return "Unknown Card", "Database missing"
    try:
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        cursor.execute("SELECT name, desc FROM texts WHERE id = ?", (card_id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            name = row[0].decode('utf-8', errors='ignore') if isinstance(row[0], bytes) else row[0]
            desc = row[1].decode('utf-8', errors='ignore') if isinstance(row[1], bytes) else row[1]
            return name, desc
    except Exception as e:
        return f"Error: {str(e)}", ""
    return "Unknown Card", ""

def main():
    print("Starting precise audit of 22,358 Lua files for field-wide attack locks...")
    locks = []
    
    for root, dirs, files in os.walk(SCRIPT_DIR):
        for file in files:
            if not file.endswith(".lua") or not file.startswith("c"):
                continue
                
            try:
                card_id_str = file[1:-4]
                if not card_id_str.isdigit():
                    continue
                card_id = int(card_id_str)
            except ValueError:
                continue
                
            file_path = os.path.join(root, file)
            try:
                with open(file_path, "r", encoding="utf-8", errors="ignore") as f:
                    content = f.read()
                
                # Check for effect variables
                effect_vars = effect_decl_re.findall(content)
                if not effect_vars:
                    continue
                
                is_lock = False
                lock_type = None
                for ev in set(effect_vars):
                    # Check if SetType has EFFECT_TYPE_FIELD on this variable
                    type_pat = rf"{ev}\s*:\s*SetType\s*\([^)]*EFFECT_TYPE_FIELD[^)]*\)"
                    
                    # Check if SetCode has cannot attack code page
                    code_pat_ann = rf"{ev}\s*:\s*SetCode\s*\([^)]*EFFECT_CANNOT_ATTACK_ANNOUNCE[^)]*\)"
                    code_pat_atk = rf"{ev}\s*:\s*SetCode\s*\([^)]*EFFECT_CANNOT_ATTACK[^)]*\)"
                    code_pat_dir = rf"{ev}\s*:\s*SetCode\s*\([^)]*EFFECT_CANNOT_DIRECT_ATTACK[^)]*\)"
                    
                    if re.search(type_pat, content):
                        if re.search(code_pat_ann, content):
                            is_lock = True
                            lock_type = "EFFECT_CANNOT_ATTACK_ANNOUNCE"
                            break
                        elif re.search(code_pat_atk, content):
                            is_lock = True
                            lock_type = "EFFECT_CANNOT_ATTACK"
                            break
                        elif re.search(code_pat_dir, content):
                            is_lock = True
                            lock_type = "EFFECT_CANNOT_DIRECT_ATTACK"
                            break
                
                if is_lock:
                    name, desc = get_card_info(card_id)
                    if name != "Unknown Card":
                        locks.append({
                            "id": card_id,
                            "name": name,
                            "desc": desc,
                            "lock_type": lock_type,
                            "file": os.path.relpath(file_path, PROJECT_ROOT)
                        })
            except Exception as e:
                pass
                
    print(f"Audit complete! Found {len(locks)} cards with field-wide attack-blocking effects.")
    
    # Save JSON data
    json_path = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", "precise_attack_locks.json")
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(locks, f, indent=2, ensure_ascii=False)
        
    # Write Markdown Report
    md_path = os.path.join(PROJECT_ROOT, "attack_locks_report.md")
    with open(md_path, "w", encoding="utf-8") as f:
        f.write("# Yu-Gi-Oh! Attack-Blocking Card Audit Report\n\n")
        f.write(f"This report lists all **{len(locks)}** cards in the database that implement field-wide attack locking or restriction effects. These cards are detected recursively from the EDOPro card scripts.\n\n")
        f.write("| Card ID | Name | Lock Type | Description |\n")
        f.write("| --- | --- | --- | --- |\n")
        for l in locks:
            clean_desc = l["desc"].replace("\n", " ").replace("|", "\\|")
            f.write(f"| {l['id']} | **{l['name']}** | `{l['lock_type']}` | {clean_desc} |\n")
            
    print(f"Detailed Markdown report saved to: {md_path}")
    print(f"JSON registry saved to: {json_path}")

if __name__ == "__main__":
    main()
