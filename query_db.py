import sqlite3
import json

def query():
    db_path = r"c:\Users\admin\Documents\EDOTh\cards.cdb"
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()
    
    cur.execute("SELECT id, name, desc FROM texts LIMIT 20")
    rows = cur.fetchall()
    
    output = []
    for row in rows:
        output.append({
            "id": row[0],
            "name": row[1],
            "desc": row[2]
        })
        
    with open("query_output.json", "w", encoding="utf-8") as f:
        json.dump(output, f, ensure_ascii=False, indent=2)
        
    conn.close()

if __name__ == "__main__":
    query()
