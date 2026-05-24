import os
import re
import time

CHANGELOG_DIR = r"c:\Users\admin\Documents\EDOTh\Changelogs"

def get_changelog_info():
    files = [f for f in os.listdir(CHANGELOG_DIR) if f.startswith("changelog_20260524_")]
    results = []
    
    for f in files:
        path = os.path.join(CHANGELOG_DIR, f)
        with open(path, "r", encoding="utf-8") as file:
            content = file.read()
            
        # Try to find timestamp in content
        ts_match = re.findall(r"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{2}:\d{2}", content)
        if ts_match:
            ts = ts_match[0]
        else:
            # Fallback to modification time
            mtime = os.path.getmtime(path)
            ts = time.strftime("%Y-%m-%dT%H:%M:%S+07:00", time.localtime(mtime))
            
        # Clean title
        lines = content.splitlines()
        title = lines[0].strip("# ").strip() if lines else "Changelog Section"
        
        results.append({
            "timestamp": ts,
            "filename": f,
            "title": title,
            "content": content,
            "path": path
        })
        
    # Sort chronologically by timestamp
    results.sort(key=lambda x: x["timestamp"])
    return results

def consolidate():
    changelogs = get_changelog_info()
    
    consolidated_content = []
    consolidated_content.append("# Consolidated Changelog - 24 พฤษภาคม 2026")
    consolidated_content.append(f"**รวบรวมเมื่อ**: {time.strftime('%Y-%m-%dT%H:%M:%S+07:00')}")
    consolidated_content.append("\n---\n")
    
    print(f"Found {len(changelogs)} changelog files for 2026-05-24:")
    for idx, cl in enumerate(changelogs, 1):
        print(f"{idx}. {cl['timestamp']} | {cl['filename']} | {cl['title']}")
        
        # Add section header
        consolidated_content.append(f"## [{idx}] {cl['title']}")
        consolidated_content.append(f"**เวลาบันทึก**: {cl['timestamp']}")
        consolidated_content.append(f"**ไฟล์เดิม**: `{cl['filename']}`")
        consolidated_content.append("\n")
        
        # Add content but strip the first H1 if present to avoid nesting headers awkwardly
        body = cl["content"]
        # Remove the first H1 header line if it matches the title
        lines = body.splitlines()
        if lines and lines[0].strip().startswith("#"):
            body = "\n".join(lines[1:])
            
        consolidated_content.append(body.strip())
        consolidated_content.append("\n\n---\n\n")
        
    output_path = os.path.join(CHANGELOG_DIR, "changelog_20260524.md")
    with open(output_path, "w", encoding="utf-8") as f_out:
        f_out.write("\n".join(consolidated_content))
    print(f"Successfully consolidated into: {output_path}")
    
    # Delete original files
    for cl in changelogs:
        os.remove(cl["path"])
        print(f"Deleted original file: {cl['filename']}")

if __name__ == "__main__":
    consolidate()
