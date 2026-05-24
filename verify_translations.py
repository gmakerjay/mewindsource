import sqlite3
import json
import os

REPO_DIR = r"c:\Users\admin\Documents\EDOTh\repositories\delta-bagooska"

def verify():
    # Load list of expected cards
    with open("untranslated_repo_cards.json", "r", encoding="utf-8") as f:
        untranslated_data = json.load(f)
        
    expected_ids = set(untranslated_data.keys())
    print(f"Verifying {len(expected_ids)} translated card IDs...")
    
    issues = []
    verified_count = 0
    
    # Check databases in submodule repo
    for card_id, records in untranslated_data.items():
        cdb_name = records[0]["cdb"]
        cdb_path = os.path.join(REPO_DIR, cdb_name)
        
        if not os.path.exists(cdb_path):
            issues.append(f"Database missing: {cdb_path}")
            continue
            
        conn = sqlite3.connect(cdb_path)
        cur = conn.cursor()
        
        cur.execute("SELECT name, desc FROM texts WHERE id = ?", (card_id,))
        row = cur.fetchone()
        if not row:
            issues.append(f"Card ID {card_id} missing from texts in {cdb_name}")
            conn.close()
            continue
            
        name, desc = row
        
        # Check if description has Thai characters
        has_thai = any('\u0e00' <= char <= '\u0e7f' for char in desc or '')
        if not has_thai:
            issues.append(f"Card ID {card_id} in {cdb_name} does not contain Thai text in description.")
            
        # Check for forbidden words
        if "คาถา" in (desc or ""):
            issues.append(f"Card ID {card_id} uses forbidden word 'คาถา' in description.")
        if "กัปดัก" in (desc or ""):
            issues.append(f"Card ID {card_id} uses typo word 'กัปดัก' in description.")
            
        conn.close()
        verified_count += 1
        
    # Check deployed databases
    deployed_dirs = {
        "Thai_Language_Folder": r"c:\Users\admin\Documents\EDOTh\config\languages\Thai",
        "WindBot": r"c:\Users\admin\Documents\EDOTh\WindBot"
    }
    
    cdb_names = ["cards.delta.cdb", "prerelease-betb.cdb", "prerelease-cori.cdb", "release-blzd.cdb"]
    
    for dir_name, dir_path in deployed_dirs.items():
        for cdb_name in cdb_names:
            path = os.path.join(dir_path, cdb_name)
            if not os.path.exists(path):
                issues.append(f"Deployed CDB missing: {path}")
                continue
                
            # Verify card IDs from this database
            conn = sqlite3.connect(path)
            cur = conn.cursor()
            
            for card_id, records in untranslated_data.items():
                if records[0]["cdb"] != cdb_name:
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
        r"c:\Users\admin\Documents\EDOTh\expansions\release-blzd.cdb"
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
