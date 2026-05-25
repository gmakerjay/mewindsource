import sqlite3
import os
import glob
import json

ydk_ids = [
    40237840, 17132130, 101402021, 83965311, 40591390, 58288218, 9411399,
    10808715, 50720316, 66206748, 27780618, 42141493, 16605586, 101402022,
    101402023, 14558127, 24094653, 32807846, 52947044, 73628505, 81439173,
    100456010, 21143941, 24224831, 6186304, 101402062, 10045474, 78114463,
    93657021, 69394324, 23204029, 90579153, 60461804, 46759931, 101402037,
    58481573, 30757127, 63813056, 1948619, 58004362, 94145021, 87758526,
    13243125, 54757758, 24299458, 48130397, 6325660
]

workspace_dir = r"c:\Users\admin\Documents\EDOTh"
cdb_files = glob.glob(os.path.join(workspace_dir, "**/*.cdb"), recursive=True)

# Sort cdb_files: Thai databases have higher priority for Thai names
thai_cdbs = [f for f in cdb_files if "Thai" in f]
other_cdbs = [f for f in cdb_files if "Thai" not in f]
all_cdbs = thai_cdbs + other_cdbs

results = {}

for cid in ydk_ids:
    results[cid] = {
        "id": cid,
        "eng_name": None,
        "thai_name": None,
        "type": None,
        "atk": None,
        "def": None,
        "level": None
    }

for cdb in all_cdbs:
    is_thai = "Thai" in cdb
    try:
        conn = sqlite3.connect(cdb)
        cursor = conn.cursor()
        for cid in ydk_ids:
            # Query texts
            cursor.execute("SELECT name, desc FROM texts WHERE id = ?", (cid,))
            text_row = cursor.fetchone()
            if text_row:
                name, desc = text_row
                if is_thai:
                    if not results[cid]["thai_name"]:
                        results[cid]["thai_name"] = name
                else:
                    if not results[cid]["eng_name"]:
                        results[cid]["eng_name"] = name
            
            # Query datas
            cursor.execute("SELECT type, atk, def, level FROM datas WHERE id = ?", (cid,))
            data_row = cursor.fetchone()
            if data_row:
                ctype, atk, _def, level = data_row
                results[cid]["type"] = ctype
                results[cid]["atk"] = atk
                results[cid]["def"] = _def
                results[cid]["level"] = level
        conn.close()
    except Exception as e:
        print(f"Error reading {cdb}: {e}")

# Check with card_names.json for missing English names
try:
    with open(os.path.join(workspace_dir, "WindBot/config/card_names.json"), "r", encoding="utf-8") as f:
        card_names = json.load(f)
        for cid in ydk_ids:
            cid_str = str(cid)
            if cid_str in card_names:
                if not results[cid]["eng_name"]:
                    results[cid]["eng_name"] = card_names[cid_str]
except Exception as e:
    print(f"Error reading card_names.json: {e}")

# Output results nicely formatted
print(json.dumps(results, indent=2, ensure_ascii=False))
