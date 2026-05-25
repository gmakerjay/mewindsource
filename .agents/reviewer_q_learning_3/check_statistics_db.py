import sqlite3

def check_db():
    db_path = r"c:\Users\admin\Documents\EDOTh\scratch\statistics.db"
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    print("=== Matches Schema ===")
    cursor.execute("PRAGMA table_info(matches)")
    for col in cursor.fetchall():
        print(col)
        
    print("\n=== Decisions Schema ===")
    cursor.execute("PRAGMA table_info(decisions)")
    for col in cursor.fetchall():
        print(col)

    print("\n=== Matches Row Count ===")
    cursor.execute("SELECT COUNT(*) FROM matches")
    print("Matches Count:", cursor.fetchone()[0])
    
    print("\n=== Decisions Row Count ===")
    cursor.execute("SELECT COUNT(*) FROM decisions")
    print("Decisions Count:", cursor.fetchone()[0])
    
    print("\n=== Mock Match Record ===")
    cursor.execute("SELECT * FROM matches WHERE deck_self='2026_EvilTwin'")
    for row in cursor.fetchall():
        print(row)
        
    print("\n=== Mock Decision Record ===")
    cursor.execute("SELECT * FROM decisions WHERE card_id=6637331")
    for row in cursor.fetchall():
        print(row)
        
    conn.close()

if __name__ == "__main__":
    check_db()
