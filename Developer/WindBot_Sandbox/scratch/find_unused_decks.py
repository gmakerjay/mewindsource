import os
import json
import re

PROJECT_ROOT = "C:\\Users\\admin\\Documents\\EDOTh"
WINDBOT_DIR = os.path.join(PROJECT_ROOT, "WindBot")
DECKS_DIR = os.path.join(WINDBOT_DIR, "Decks")
BOTS_JSON_PATH = os.path.join(WINDBOT_DIR, "bots.json")
CS_PATH = os.path.join(WINDBOT_DIR, "UnifiedIgnisExecutor.cs")

def audit_decks():
    # 1. Get all YDK files
    if not os.path.exists(DECKS_DIR):
        print("Decks directory not found.")
        return
    ydk_files = [f for f in os.listdir(DECKS_DIR) if f.endswith(".ydk")]

    # 2. Get registered decks in bots.json
    registered_deck_names = set()
    if os.path.exists(BOTS_JSON_PATH):
        try:
            with open(BOTS_JSON_PATH, "r", encoding="utf-8") as f:
                bots = json.load(f)
                for bot in bots:
                    deck = bot.get("deck")
                    if deck:
                        registered_deck_names.add(deck)
        except Exception as e:
            print(f"Error reading bots.json: {e}")

    # 3. Get C# Deck Attributes
    cs_deck_attributes = set()
    if os.path.exists(CS_PATH):
        try:
            with open(CS_PATH, "r", encoding="utf-8") as f:
                content = f.read()
                # Find [Deck("Name", "YdkFile")]
                matches = re.findall(r'\[Deck\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*\)\]', content)
                for match in matches:
                    cs_deck_attributes.add(match[0])  # deck name
                    cs_deck_attributes.add(match[1])  # ydk name prefix / deck prefix
        except Exception as e:
            print(f"Error reading UnifiedIgnisExecutor.cs: {e}")

    print(f"Total YDK files in directory: {len(ydk_files)}")
    print(f"Registered decks in bots.json: {len(registered_deck_names)}")
    print(f"Deck names in C# DeckAttributes: {len(cs_deck_attributes)}")

    # 4. Check for unreferenced YDK files
    # A YDK file is used if its base name (without AI_ prefix or .ydk extension) matches either:
    # - a registered deck name in bots.json
    # - a deck name or ydk name in C# DeckAttribute
    unreferenced_ydk = []
    for ydk in ydk_files:
        base_name = ydk[:-4]
        clean_name = base_name
        if base_name.startswith("AI_"):
            clean_name = base_name[3:]
            
        is_referenced = (
            base_name in registered_deck_names or
            clean_name in registered_deck_names or
            base_name in cs_deck_attributes or
            clean_name in cs_deck_attributes
        )
        
        if not is_referenced:
            unreferenced_ydk.append(ydk)

    print("\n--- UNREFERENCED YDK DECK FILES ---")
    print(f"Found {len(unreferenced_ydk)} unreferenced YDK files:")
    for y in sorted(unreferenced_ydk):
        print(f"  - {y}")

if __name__ == "__main__":
    audit_decks()
