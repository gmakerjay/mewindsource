# Changelog - 2026-05-25: Thai Language & Client Configuration Restoration

## 1. Issue Description
* **Symptom:** 
  - The EDOPro client UI reverted to the default state (English language, default skin "none", sound/music disabled, default nickname "Player"). It behaved as if EDOPro was downloaded completely brand new.
  - Card text rendering was corrupted (e.g. "Diabellstar the Black Witch" showed vast blank spaces with only English words/numbers visible, like `( ) 1 "Diabellstar the Black Witch" : / "Sinful Spoils" 1`).
* **Root Cause:**
  - In commit `aec0bcc` (feat: EDOTh WindBot system refactoring and enhancements), the `config/system.conf` file was accidentally emptied (0 bytes).
  - When the EDOPro client ran next, it detected the empty config file and regenerated it with default English configurations and the default Japanese font (`NotoSansJP-Regular.otf`).
  - Because the card databases inside `./repositories/delta-bagooska` contained Thai translations but EDOPro was running in English with a font that had no Thai glyph support, all Thai characters rendered as empty whitespaces.

## 2. Actions Taken
* **Restored `config/system.conf`:**
  - Written the original settings back:
    - `language = Thai`
    - `textfont = fonts/tahoma.ttf 15`
    - `numfont = fonts/tahoma.ttf`
    - `skin = Burning`
    - Sound and music restored (volumes set to 5 and 13 respectively)
* **Re-applied and Verified Translations:**
  - Ran `apply_translations.py` to regenerate the translation database files and deployed them to:
    - `config/languages/Thai/`
    - `config/languages/Thai/repositories/delta-bagooska/`
    - `WindBot/`
  - Ran `verify_translations.py` to verify all 106 expected translated card IDs. All checks passed with 0 issues.
* **Process Conflict Safety:**
  - Verified that the `EDOPro.exe` process was fully closed before writing `config/system.conf` to prevent the client from overwriting it on exit.
