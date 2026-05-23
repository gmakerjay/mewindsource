"""
shared_utils.py — Shared utilities for WindBot_Sandbox training scripts.

Centralizes path constants, registry I/O, deck file loading, and other
commonly used helpers that were previously duplicated across:
  optimize_registry.py, combo_simulator.py, learning_sandbox.py,
  auto_role_detector.py, q_learning.py, ab_tournament.py, cockpit.py
"""

import os
import sys
import json
import shutil
import subprocess


# ─── Path Constants ──────────────────────────────────────────────────────────

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.dirname(SCRIPT_DIR)
WINDBOT_DIR = os.path.join(PROJECT_ROOT, "WindBot")
DECKS_DIR = os.path.join(WINDBOT_DIR, "Decks")
LIVE_CONFIG_DIR = os.path.join(WINDBOT_DIR, "config")
LIVE_LOGS_DIR = os.path.join(WINDBOT_DIR, "Logs")
OPP_MEMORY_PATH = os.path.join(LIVE_CONFIG_DIR, "opponent_memory.json")


# ─── Console Encoding ───────────────────────────────────────────────────────

def configure_utf8():
    """Reconfigure stdout to UTF-8 (safe no-op on older Python builds)."""
    try:
        sys.stdout.reconfigure(encoding='utf-8')
    except AttributeError:
        pass


# ─── Registry Path Resolution ───────────────────────────────────────────────

def _registry_filename(deck_name):
    """Return the registry JSON filename for a given deck name."""
    if not deck_name or deck_name == "all":
        return "cards_registry.json"
    return f"cards_registry_{deck_name}.json"


def get_registry_paths(deck_name, auto_init=True):
    """
    Return (sandbox_path, live_path) for the given deck's registry file.

    If ``auto_init`` is True and the deck-specific sandbox registry does not
    exist, it will be initialised from the default ``cards_registry.json``
    and the auto-role-detector will be invoked automatically.
    """
    reg_file = _registry_filename(deck_name)
    sandbox_path = os.path.join(SCRIPT_DIR, reg_file)
    live_path = os.path.join(LIVE_CONFIG_DIR, reg_file)

    if auto_init and deck_name and deck_name != "all" and not os.path.exists(sandbox_path):
        default_sandbox = os.path.join(SCRIPT_DIR, "cards_registry.json")
        if os.path.exists(default_sandbox):
            try:
                shutil.copy2(default_sandbox, sandbox_path)
                print(f"Initialized deck-specific registry: {sandbox_path}")
                # Run auto role detector
                detector_script = os.path.join(SCRIPT_DIR, "auto_role_detector.py")
                if os.path.exists(detector_script):
                    subprocess.run(
                        [sys.executable, detector_script, "--deck", deck_name],
                        stdout=subprocess.DEVNULL,
                        stderr=subprocess.DEVNULL,
                    )
                    print(f"Auto-populated roles for deck: {deck_name}")
            except Exception as e:
                print(f"Warning: Failed to copy default registry or run auto-role-detector: {e}")

    return sandbox_path, live_path


def get_sandbox_registry_path(deck_name):
    """Return only the sandbox registry path (no auto-initialisation)."""
    return os.path.join(SCRIPT_DIR, _registry_filename(deck_name))


# ─── Deck Discovery ─────────────────────────────────────────────────────────

def get_available_decks(ai_only=True):
    """
    Return a sorted list of deck names found in ``WindBot/Decks/``.

    When *ai_only* is True (default) only ``AI_*.ydk`` files are returned.
    When False, all ``.ydk`` files are included (with ``AI_`` prefix stripped
    where present).
    """
    if not os.path.exists(DECKS_DIR):
        return []
    decks = []
    for f in os.listdir(DECKS_DIR):
        if f.startswith("AI_") and f.endswith(".ydk"):
            decks.append(f[3:-4])
        elif not ai_only and f.endswith(".ydk"):
            decks.append(f[:-4])
    return sorted(decks)


def load_ydk_main_deck(deck_name, unique=False):
    """
    Parse the main-deck card IDs from the ``.ydk`` file for *deck_name*.

    Looks for ``AI_<deck_name>.ydk`` first, then ``<deck_name>.ydk``.
    Returns a list of integer card IDs.  When *unique* is True the list is
    deduplicated (useful for role detection where we only need distinct IDs).
    """
    path = os.path.join(DECKS_DIR, f"AI_{deck_name}.ydk")
    if not os.path.exists(path):
        path = os.path.join(DECKS_DIR, f"{deck_name}.ydk")
    if not os.path.exists(path):
        return []

    main_deck = []
    in_main = False
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#created"):
                continue
            if line == "#main":
                in_main = True
                continue
            if line.startswith("#extra") or line.startswith("!side"):
                in_main = False
                continue
            if in_main:
                try:
                    main_deck.append(int(line))
                except ValueError:
                    pass

    if unique:
        return list(set(main_deck))
    return main_deck


# ─── Registry I/O ───────────────────────────────────────────────────────────

def load_registry_list(path):
    """
    Load a card registry JSON file and return it as a **list** of card dicts.

    Returns an empty list if the file does not exist.
    """
    if not os.path.exists(path):
        return []
    with open(path, "r", encoding="utf-8-sig") as f:
        return json.load(f)


def load_registry_dict(path):
    """
    Load a card registry JSON file and return it as a **dict** keyed by card ID.

    Returns an empty dict if the file does not exist.
    """
    if not os.path.exists(path):
        return {}
    with open(path, "r", encoding="utf-8-sig") as f:
        data = json.load(f)
        return {card["id"]: card for card in data}


def save_registry_list(data, path):
    """
    Save a card registry to *path* as a JSON list.

    *data* may be either a ``list`` of card dicts or a ``dict`` keyed by
    card ID (in which case ``.values()`` is used automatically).
    """
    import tempfile
    if isinstance(data, dict):
        data = list(data.values())

    dir_name = os.path.dirname(path)
    if dir_name and not os.path.exists(dir_name):
        os.makedirs(dir_name)

    fd, temp_path = tempfile.mkstemp(dir=dir_name, prefix="tmp_registry_", suffix=".json")
    try:
        with os.fdopen(fd, "w", encoding="utf-8-sig") as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        os.replace(temp_path, path)
    except Exception as e:
        if os.path.exists(temp_path):
            try:
                os.remove(temp_path)
            except Exception:
                pass
        raise e
