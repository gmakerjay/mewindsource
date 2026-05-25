import sqlite3
import json
import os
import sys

REPO_DIR = r"c:\Users\admin\Documents\EDOTh\repositories\delta-bagooska"

def verify():
    # Load list of expected cards from apply_translations
    sys.path.append(r"c:\Users\admin\Documents\EDOTh")
    from apply_translations import CUSTOM_TRANSLATIONS
    
    official_translations = {}
    if os.path.exists("official_thai_matches.json"):
        with open("official_thai_matches.json", "r", encoding="utf-8") as f:
            official_translations = json.load(f)
            
    all_translations = {}
    all_translations.update(CUSTOM_TRANSLATIONS)
    all_translations.update(official_translations)
    
    expected_ids = set(all_translations.keys())
    print(f"Verifying {len(expected_ids)} translated card IDs...")
    
    issues = []
    verified_count = 0
    
    # Map card IDs to databases dynamically
    card_to_cdb = {}
    repo_cdbs = [f for f in os.listdir(REPO_DIR) if f.endswith(".cdb")]
    for card_id in all_translations:
        for cdb_name in repo_cdbs:
            cdb_path = os.path.join(REPO_DIR, cdb_name)
            try:
                conn = sqlite3.connect(cdb_path)
                cur = conn.cursor()
                cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='texts'")
                if cur.fetchone():
                    cur.execute("SELECT id FROM texts WHERE id = ?", (card_id,))
                    if cur.fetchone():
                        card_to_cdb[card_id] = cdb_name
                        conn.close()
                        break
                conn.close()
            except:
                pass
                
    # Check databases in submodule repo
    for card_id in sorted(expected_ids):
        cdb_name = card_to_cdb.get(card_id)
        if not cdb_name:
            issues.append(f"Card ID {card_id} not found in any repository database")
            continue
            
        cdb_path = os.path.join(REPO_DIR, cdb_name)
        try:
            conn = sqlite3.connect(cdb_path)
            cur = conn.cursor()
            cur.execute("SELECT name, desc FROM texts WHERE id = ?", (card_id,))
            row = cur.fetchone()
            if not row:
                issues.append(f"Card ID {card_id} missing from texts in {cdb_name}")
            else:
                name, desc = row
                has_thai = any('\u0e00' <= char <= '\u0e7f' for char in desc or '')
                if not has_thai:
                    issues.append(f"Card ID {card_id} in {cdb_name} does not contain Thai text.")
                if "คาถา" in (desc or ""):
                    issues.append(f"Card ID {card_id} uses forbidden word 'คาถา' in description.")
                if "กัปดัก" in (desc or ""):
                    issues.append(f"Card ID {card_id} uses typo word 'กัปดัก' in description.")
            conn.close()
        except Exception as e:
            issues.append(f"Error checking Card ID {card_id} in {cdb_name}: {e}")
        verified_count += 1
        
    # Check deployed databases
    deployed_dirs = {
        "Thai_Language_Folder_Root": r"c:\Users\admin\Documents\EDOTh\config\languages\Thai",
        "Thai_Language_Folder_Nested": r"c:\Users\admin\Documents\EDOTh\config\languages\Thai\repositories\delta-bagooska",
        "WindBot": r"c:\Users\admin\Documents\EDOTh\WindBot"
    }
    
    cdb_names = ["cards.delta.cdb", "prerelease-betb.cdb", "prerelease-cori.cdb", "release-blzd.cdb", "prerelease-lpg2.cdb"]
    
    for dir_name, dir_path in deployed_dirs.items():
        for cdb_name in cdb_names:
            path = os.path.join(dir_path, cdb_name)
            if not os.path.exists(path):
                issues.append(f"Deployed CDB missing: {path}")
                continue
                
            # Verify card IDs from this database
            conn = sqlite3.connect(path)
            cur = conn.cursor()
            
            for card_id, expected_cdb in card_to_cdb.items():
                if expected_cdb != cdb_name:
                    continue
                    
                cur.execute("SELECT name, desc FROM texts WHERE id = ?", (card_id,))
                row = cur.fetchone()
                if not row:
                    issues.append(f"Card ID {card_id} missing in deployed cdb: {path}")
                    continue
                    
                name, desc = row
                has_thai = any('\u0e00' <= char <= '\u0e7f' for char in desc or '')
                if not has_thai:
                    issues.append(f"Card ID {card_id} in deployed {path} has no Thai text.")
                    
                if "คาถา" in (desc or ""):
                    issues.append(f"Card ID {card_id} in deployed {path} uses forbidden word 'คาถา' in description.")
                if "กัปดัก" in (desc or ""):
                    issues.append(f"Card ID {card_id} in deployed {path} uses typo word 'กัปดัก' in description.")
                    
            conn.close()
            print(f"Verified deployed database: {path}")

    # Check cleanup paths
    cleanup_paths = [
        r"c:\Users\admin\Documents\EDOTh\cards.delta.cdb",
        r"c:\Users\admin\Documents\EDOTh\expansions\cards.delta.cdb",
        r"c:\Users\admin\Documents\EDOTh\expansions\prerelease-betb.cdb",
        r"c:\Users\admin\Documents\EDOTh\expansions\prerelease-cori.cdb",
        r"c:\Users\admin\Documents\EDOTh\expansions\release-blzd.cdb",
        r"c:\Users\admin\Documents\EDOTh\expansions\prerelease-lpg2.cdb"
    ]
    
    for path in cleanup_paths:
        if os.path.exists(path):
            issues.append(f"Redundant database copy still exists: {path}")
        else:
            print(f"Verified redundant database copy is absent: {path}")

    # Summary
    report = []
    report.append(f"Verification Results:")
    report.append(f"Expected IDs: {len(expected_ids)}")
    report.append(f"Verified IDs: {verified_count}")
    report.append(f"Issues Found: {len(issues)}")
    if issues:
        report.append("\nIssues detail:")
        for issue in issues:
            report.append(f"- {issue}")
    else:
        report.append("\nAll checks passed! No issues found.")
        
    with open("verification_report.txt", "w", encoding="utf-8") as f_out:
        f_out.write("\n".join(report))
        
    print(f"Verification report written to verification_report.txt. Issues count: {len(issues)}")

if __name__ == "__main__":
    verify()
