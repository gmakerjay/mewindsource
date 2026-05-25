import os
import sys
import shutil
import subprocess
import json

project_root = r"c:\Users\admin\Documents\EDOTh"
sandbox_dir = os.path.join(project_root, "WindBot_Sandbox")
windbot_dir = os.path.join(project_root, "WindBot")
config_dir = os.path.join(windbot_dir, "config")
deck_config_dir = os.path.join(config_dir, "decks")

decks = [
    "2026_AzaYummy", "2026_BrElfnote", "2026_DarkTime", "2026_EvilTwin", "2026_EyeInside",
    "2026_Goldlord", "2026_Hecahand", "2026_Invoke", "2026_Kwtune", "2026_Labrynth"
]

print("=== STEP 1: Running auto_role_detector.py for all 10 decks ===")
for deck in decks:
    print(f"Running role detector for: {deck}")
    # Run auto_role_detector.py --deck <name> --overwrite
    cmd = [sys.executable, "auto_role_detector.py", "--deck", deck, "--overwrite"]
    res = subprocess.run(cmd, cwd=sandbox_dir, capture_output=True, text=True)
    if res.returncode != 0:
        print(f"[ERROR] Failed running auto_role_detector.py for {deck}")
        print(res.stderr)
    else:
        print(f"[SUCCESS] {deck} role detector ran successfully.")
        # print first few lines of stdout
        print("\n".join(res.stdout.splitlines()[:5]))

print("\n=== STEP 2: Copying / Deploying sandbox registry files to WindBot/config/ ===")
for deck in decks:
    sandbox_reg = os.path.join(sandbox_dir, f"cards_registry_{deck}.json")
    live_reg = os.path.join(config_dir, f"cards_registry_{deck}.json")
    if os.path.exists(sandbox_reg):
        try:
            shutil.copy2(sandbox_reg, live_reg)
            print(f"Copied {sandbox_reg} -> {live_reg}")
        except Exception as e:
            print(f"[ERROR] Failed to copy {sandbox_reg} to {live_reg}: {e}")
    else:
        print(f"[ERROR] Sandbox registry file does not exist: {sandbox_reg}")

print("\n=== STEP 3: Compiling C# AI ===")
bat_path = os.path.join(windbot_dir, "compile_ai.bat")
if os.path.exists(bat_path):
    print("Executing compile_ai.bat...")
    # Run compile_ai.bat. Note: it's a batch file, so we can run cmd.exe /c compile_ai.bat or just run it directly.
    # We should run it inside windbot_dir.
    res = subprocess.run([bat_path], cwd=windbot_dir, capture_output=True, text=True)
    print("Compilation Output:")
    print(res.stdout)
    if res.stderr:
        print("Compilation Errors:")
        print(res.stderr)
    if res.returncode == 0:
        print("[SUCCESS] C# Compilation succeeded.")
    else:
        print(f"[ERROR] C# Compilation failed with exit code {res.returncode}")
else:
    print(f"[ERROR] compile_ai.bat not found at {bat_path}")

print("\n=== STEP 4: Verification of JSON files ===")
verification_passed = True
for deck in decks:
    # Check registry JSON
    reg_path = os.path.join(config_dir, f"cards_registry_{deck}.json")
    if not os.path.exists(reg_path):
        print(f"[FAIL] Registry file missing: {reg_path}")
        verification_passed = False
    else:
        # Check if valid JSON and not empty
        try:
            with open(reg_path, "r", encoding="utf-8-sig") as f:
                data = json.load(f)
            if not data:
                print(f"[FAIL] Registry file is empty: {reg_path}")
                verification_passed = False
            else:
                print(f"[PASS] Registry file {reg_path} exists and contains {len(data)} cards.")
        except Exception as e:
            print(f"[FAIL] Registry file {reg_path} is invalid JSON: {e}")
            verification_passed = False

    # Check deck config JSON
    config_path = os.path.join(deck_config_dir, f"{deck}.json")
    if not os.path.exists(config_path):
        print(f"[FAIL] Deck config file missing: {config_path}")
        verification_passed = False
    else:
        try:
            with open(config_path, "r", encoding="utf-8-sig") as f:
                data = json.load(f)
            playstyle = data.get("playstyle")
            choke_points = data.get("choke_points")
            print(f"[PASS] Deck config {config_path} is valid. Playstyle={playstyle}, Choke Points={choke_points}")
        except Exception as e:
            print(f"[FAIL] Deck config {config_path} is invalid JSON: {e}")
            verification_passed = False

if verification_passed:
    print("\n🎉 ALL VERIFICATIONS PASSED SUCCESSFULLY!")
else:
    print("\n❌ SOME VERIFICATIONS FAILED!")
