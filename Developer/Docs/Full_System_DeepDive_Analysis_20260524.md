# Full System Deep-Dive Analysis — WindBot IGNIS UnifiedIgnisExecutor

**วันที่:** 2026-05-24 23:59 ICT (UTC+7)  
**วิเคราะห์โดย:** Buffy (Codebuff AI Agent)  
**ประเภท:** Comprehensive Full System Analysis — C# Engine + Python Sandbox + JSON Configs  
**ขอบเขต:** UnifiedIgnisExecutor.cs, WindBot_Sandbox/*.py, cards_registry_*.json, deck configs, opponent memory, WindBot.exe.config, bots.json

---

## สารบัญ

1. [โครงสร้างระบบปัจจุบัน (System Architecture)](#1-โครงสร้างระบบปัจจุบัน-system-architecture)
2. [API Call Analysis — C# Engine](#2-api-call-analysis--c-engine)
3. [API Call Analysis — Python Sandbox](#3-api-call-analysis--python-sandbox)
4. [บอททำอะไรได้บ้าง (Current Capabilities)](#4-บอททำอะไรได้บ้าง-current-capabilities)
5. [บอทเก่งแค่ไหน (Performance Assessment)](#5-บอทเก่งแค่ไหน-performance-assessment)
6. [จุดบกพร่องและข้อผิดพลาด (Bugs & Issues)](#6-จุดบกพร่องและข้อผิดพลาด-bugs--issues)
7. [แนวทางเทรนเพิ่มเติม (Training Pipeline Analysis)](#7-แนวทางเทรนเพิ่มเติม-training-pipeline-analysis)
8. [แนวทางอัพเกรดเพิ่มเติม (Upgrade Paths)](#8-แนวทางอัพเกรดเพิ่มเติม-upgrade-paths)
9. [Roadmap 3 ระยะ](#9-roadmap-3-ระยะ)
10. [สรุป (Summary)](#10-สรุป-summary)

---

## 1. โครงสร้างระบบปัจจุบัน (System Architecture)

### 1.1 ภาพรวมสถาปัตยกรรม

```
┌──────────────────────────────────────────────────────────────────────┐
│                     EDOPro Game Engine (Server)                       │
│                   LAN Mode (port 7911, version 720937)                │
└──────────────────────┬───────────────────────────────────────────────┘
                       │ game state → actions (via WebSocket/stdio)
                       ▼
┌──────────────────────────────────────────────────────────────────────┐
│  WindBot.exe (.NET Framework 4.x)                                     │
│  ├── bots.json — ตัวระบุ Bot และ Deck mapping                         │
│  ├── Executors/UnifiedIgnisExecutor.dll — Compiled AI Engine          │
│  ├── Decks/*.ydk — ไฟล์เด็ค 40-60 ใบ                                  │
│  ├── config/                                                          │
│  │   ├── cards_registry.json — Registry หลัก (160 cards)              │
│  │   ├── cards_registry_2026_*.json — Registry เฉพาะเด็ค (10 ชุด)     │
│  │   ├── opponent_memory.json — ความจำคู่ต่อสู้ (เรียนรู้จาก match)     │
│  │   ├── card_names.json — ชื่อการ์ด (สำหรับ logging)                 │
│  │   ├── decks/ — **⚠️ ว่างเปล่า (ไม่มี deck configs เลย)**            │
│  ├── Logs/ — Match logs (match_summary.log, decisions.jsonl, turn_*)  │
│  └── compile_ai.bat — คอมไพล์ C# → DLL                                │
└──────────────────────┬───────────────────────────────────────────────┘
                       │ logs, registry JSON
                       ▼
┌──────────────────────────────────────────────────────────────────────┐
│  WindBot_Sandbox (Python 3.x)                                         │
│  ├── cockpit.py — Web Dashboard (port 8000) — GUI ควบคุม              │
│  ├── shared_utils.py — Utility ส่วนกลาง                               │
│  ├── auto_role_detector.py — ตรวจจับ Role การ์ดอัตโนมัติ               │
│  ├── learning_sandbox.py — Heuristic Learning Engine                   │
│  ├── q_learning.py — Q-Learning Reinforcement Trainer                  │
│  ├── run_match_learning.py — Pipeline รวม Heuristic + Q-Learning      │
│  ├── combo_simulator.py — จำลองความน่าจะเป็นมือเปิด                    │
│  ├── optimize_registry.py — Hill Climbing Weight Optimizer             │
│  ├── ab_tournament.py — A/B Tournament Tester                        │
│  ├── templates/ — HTML templates (dashboard.html, analytics.html)     │
│  └── scratch/ — สคริปต์เพื่อการทดสอบ (disconnected from pipeline)     │
└──────────────────────────────────────────────────────────────────────┘
```

### 1.2 ชั้นของระบบ (Layers)

| Layer | Technology | หน้าที่ | ไฟล์หลัก |
|-------|-----------|--------|----------|
| **Game Engine** | C++ (EDOPro) | จำลองเกม Yu-Gi-Oh! | EDOPro.exe |
| **Bot Core** | C# (.NET) | ตัดสินใจ, scoring, learning | UnifiedIgnisExecutor.cs |
| **Dashboard** | Python | UI, deploy, training triggers | cockpit.py, templates/* |
| **Training** | Python | Heuristic, Q-Learning, simulation | learning_sandbox.py, q_learning.py, combo_simulator.py |
| **Config** | JSON | Card registry, deck configs, opponent memory | cards_registry_*.json, opponent_memory.json |

### 1.3 จำนวน Deck ทั้งหมด

| หมวด | จำนวน | หมายเหตุ |
|------|:-----:|---------|
| 2026 Decks (AI_*.ydk) | 10 | AzaYummy, BrElfnote, DarkTime, EvilTwin, EyeInside, Goldlord, Hecahand, Invoke, Kwtune, Labrynth |
| AI Decks อื่นๆ | ~40+ | ABC, Altergeist, Blackwing, BlueEyes, etc. |
| Deck Configs (decks/*.json) | **0** | **⚠️ ไม่มี deck configs เลย** — ทุก deck ใช้ default playstyle="control" |

---

## 2. API Call Analysis — C# Engine

### 2.1 Executor Types ที่ลงทะเบียน (AddExecutor)

| ExecutorType | Card-specific | Fallback | สถานะ |
|:------------:|:-------------:|:--------:|:-----:|
| **Activate** | ✅ (147) | ✅ (153) | ทำงานครบ |
| **Summon** | ✅ (148) | ✅ (154) | ทำงานครบ |
| **SpSummon** | ✅ (149) | ✅ (155) | ทำงานครบ |
| **SpellSet** | ❌ | ✅ (156) | Fallback อย่างเดียว |
| **Repos** | ❌ | ✅ (157) | Fallback อย่างเดียว |
| **MonsterSet** | ❌ | ✅ (158) | Fallback อย่างเดียว |

**Analysis:** 
- **Activate**, **Summon**, **SpSummon**: มีทั้ง Card-specific (จากการ์ดใน registry) และ Fallback
- **SpellSet**, **Repos**, **MonsterSet**: Fallback อย่างเดียว — ไม่มี card-specific hooks
- **ขาด ExecutorType**: `Activate` สำหรับ Spell/Trap บน field (face-up), `Tribute` สำหรับ tribute summon — **แต่ไม่จำเป็นเพราะ WindBot จัดการให้โดยอัตโนมัติ**

### 2.2 Override Methods (Lifecycle Hooks)

| Method | Signature | Status | ถูกเรียกโดย Engine? | วิเคราะห์ |
|--------|-----------|:------:|:------------------:|-----------|
| **OnNewTurn()** | `void` | ✅ Override | ✅ ทุก turn | ทำงานถูกต้อง — เรียก ApplyRealTimeLearning(), reset plans, periodic save |
| **OnNewPhase()** | `void` | ✅ Override | ✅ ทุก phase change | ทำงานถูกต้อง — log phase |
| **OnSelectHand()** | `bool` | ✅ Override | ✅ เริ่ม match | ทำงานถูกต้อง — เลือก first/second ตาม playstyle |
| **OnBattle()** | `BattlePhaseAction` | ✅ Override | ✅ Battle Phase | ทำงาน — lethal check, backrow avoidance, safe attack scan |
| **OnSelectAttackTarget()** | `BattlePhaseAction` | ✅ Override | ✅ Replay/selection | ทำงาน — target priority, memory-based avoidance |
| **OnSelectCard()** | `IList<ClientCard>` | ✅ Override | ✅ Card selection prompts | ทำงาน — self-card filter, priority sort |
| **OnChaining()** | `void` | ✅ Override | ✅ Chain building | ทำงาน — disruption tracking, plan switching |
| **OnChainEnd()** | `void` | ✅ Override | ✅ Chain resolves | ทำงาน — ApplyRealTimeLearning(), SaveConfiguration() |

### 2.3 Lifecycle Hooks ที่ไม่ได้ Override

| Method | Signature | ประโยชน์ถ้า Override | มีใน WindBot API? |
|--------|-----------|----------------------|:-----------------:|
| **OnDraw()** | `void` | ติดตามการจั่ว, hand trap probability | ✅ (EDOPro core override) |
| **OnSummon()** | `void` | ติดตาม summon event ของผู้เล่นทั้งสองฝ่าย | ⚠️ อาจไม่มี/ไม่ใช่ virtual |
| **OnSpSummon()** | `void` | ติดตาม special summon event | ⚠️ อาจไม่มี/ไม่ใช่ virtual |
| **OnActivate()** | `void` | ติดตาม card activation event | ⚠️ อาจไม่มี/ไม่ใช่ virtual |
| **OnSet()** | `void` | ติดตาม card set event | ⚠️ อาจไม่มี/ไม่ใช่ virtual |
| **OnDeath()** | `void` | ติดตามการ์ดถูกทำลาย | ⚠️ อาจไม่มี/ไม่ใช่ virtual |
| **OnChangePosition()** | `void` | ติดตามการเปลี่ยนตำแหน่ง | ⚠️ อาจไม่มี/ไม่ใช่ virtual |

**ข้อสังเกต:** 
- **OnDraw()** มีประโยชน์มากที่สุด — สามารถใช้ estimate hand trap probability (ถ้าฝ่ายตรงข้ามจั่วแล้วไม่ activate → high hand trap chance) — **และน่าจะมีใน WindBot API จริง** (เป็น override ที่พบใน DefaultExecutor)
- ฟังก์ชันอื่น ๆ (OnSummon, OnActivate, OnDeath ฯลฯ) **อาจไม่มีอยู่จริงใน WindBot API** — รายการนี้เป็น hypothetical improvement ไม่ใช่ missed opportunity
- ปัจจุบัน tracking ทั้งหมดทำผ่าน `LogDecision()` และ `OnChaining()` ซึ่งจับได้เฉพาะ action ที่บอทเลือกเอง
- **ข้อควรระวัง:** ก่อน implement ฟังก์ชันใด ๆ ต้องตรวจสอบ WindBot API ว่า method นั้น virtual และ accessible จริง

#### 2.3.1 ฟังก์ชันที่ Override ได้จริง (confirmed)

| Method | Override แล้ว? |
|--------|:--------------:|
| `OnNewTurn()` | ✅ |
| `OnNewPhase()` | ✅ |
| `OnSelectHand()` | ✅ |
| `OnBattle()` | ✅ |
| `OnSelectAttackTarget()` | ✅ |
| `OnSelectCard()` | ✅ |
| `OnChaining()` | ✅ |
| `OnChainEnd()` | ✅ |
| `OnDraw()` | ❌ — **ควร Override** |

### 2.4 Card-Specific Safeguards (9 รายการ)

| Card ID | Card Name | Safeguard Logic | ถูกเรียก? |
|:-------:|-----------|-----------------|:---------:|
| 94145021 | Droll & Lock Bird | ❌ บล็อกในเทิร์นตัวเอง | ✅ |
| 97268402 | Effect Veiler | ฝ่ายตรงข้าม Main Phase + face-up monster | ✅ |
| 24224830 | Called by the Grave | ต้องมี monster ใน opp GY | ✅ |
| 6637331 | Druiswurm (Bystial) | ต้องมี LIGHT/DARK ใน GY | ✅ |
| 33854624 | Magnamhut (Bystial) | ต้องมี LIGHT/DARK ใน GY | ✅ |
| 10045474 | Infinite Impermanence | ต้องมี face-up monster ฝ่ายตรงข้าม | ✅ |
| 42141493 | Mulcharmy Fuwalos | ❌ บล็อกในเทิร์นตัวเอง | ✅ |
| 27204311 | Nibiru | ❌ บล็อกในเทิร์นตัวเอง | ✅ |
| 38814750 | PSY-Framegear Gamma | ต้องไม่มี monster ตัวเอง + opp monster effect | ✅ |

### 2.5 Iron Rules Audit — ครบทั้ง 7 ข้อ

| Rule | Logic | ตำแหน่ง | ผ่าน? |
|:----:|-------|:-------:|:-----:|
| #1 | Handtrap ≠ own turn | ~1085-1092 | ✅ |
| #2 | Self-chain block | ~1073-1081 | ✅ |
| #3a | Called by target check | ~1113 | ✅ |
| #3b | Bystial target check | ~1123 | ✅ |
| #3c | Imperm target check | ~1133 | ✅ |
| #4 | Fallback = false | ~1505,1591,1624 | ✅ |
| #5 | Priority Hard Cap ≤ 8 | ~667-676 | ✅ |
| #6 | OnChaining direction | ~2232-2237 | ✅ |
| #7 | GetNextPlan → PlanA | ~2214 | ✅ |

---

## 3. API Call Analysis — Python Sandbox

### 3.1 Python Scripts — Function Analysis

#### cockpit.py (Web Dashboard Server)
| Function | ถูกเรียกใช้โดย | สถานะ |
|----------|:------------:|:-----:|
| `kill_active_process()` | `do_POST(/api/kill)`, `start_training()` | ✅ |
| `get_opponent_deck_name()` | `start_training()` | ✅ |
| `read_process_output()` | Thread ใน `start_training()` | ✅ |
| `consume_stream()` | Thread ใน `run_live_duel_loop()` | ✅ |
| `run_live_duel_loop()` | Thread ใน `start_training()` | ✅ |
| `get_opponent_bots()` | `do_GET(/api/opponents)` | ✅ |
| `get_match_logs_count()` | `do_GET(/api/status)` | ✅ |
| `get_registry_card_count()` | `do_GET(/api/status)` | ✅ |
| `get_opponent_memory_count()` | `do_GET(/api/status)` | ✅ |
| `parse_match_history()` | `do_GET(/api/match_history)` | ✅ |
| `get_registry_snapshot_data()` | `do_GET(/api/registry_snapshot)` | ✅ |
| `_load_template()` | Module level | ✅ |
| `CockpitHandler.do_GET()` | HTTP GET requests | ✅ |
| `CockpitHandler.do_POST()` | HTTP POST requests | ✅ |
| `CockpitHandler.start_training()` | `do_POST(/api/train)` | ✅ |
| `CockpitHandler.deploy_config()` | `do_POST(/api/deploy)` | ✅ |
| `run_server()` | `__main__` | ✅ |

#### auto_role_detector.py
| Function | ถูกเรียกใช้โดย | สถานะ |
|----------|:------------:|:-----:|
| `query_card_details()` | `main()` | ✅ |
| `detect_roles()` | `main()` | ✅ |
| `main()` | CLI / `shared_utils.get_registry_paths()` | ✅ |

#### learning_sandbox.py
| Function | ถูกเรียกใช้โดย | สถานะ |
|----------|:------------:|:-----:|
| `extract_deck_name()` | `main()` | ✅ |
| `discover_match_dirs()` | `main()` | ✅ |
| `parse_match_outcome()` | `analyze_single_match()` | ✅ |
| `parse_decisions_jsonl()` | `analyze_single_match()` | ✅ |
| `parse_disruptions_from_logs()` | `analyze_single_match()` | ✅ |
| `analyze_single_match()` | `main()` | ✅ |
| `apply_learning()` | `main()` | ✅ |
| `main()` | CLI / `run_match_learning.py` | ✅ |

#### q_learning.py
| Function | ถูกเรียกใช้โดย | สถานะ |
|----------|:------------:|:-----:|
| `discover_deck_matches()` | `main()` | ✅ |
| `parse_match_outcome()` | `main()` | ✅ |
| `parse_decisions()` | `main()` | ✅ |
| `main()` | CLI / `run_match_learning.py` | ✅ |

#### combo_simulator.py
| Function | ถูกเรียกใช้โดย | สถานะ |
|----------|:------------:|:-----:|
| `run_simulation()` | `main()` | ✅ |
| `apply_optimization()` | `main()` | ✅ |
| `main()` | CLI / cockpit.py | ✅ |

#### optimize_registry.py
| Function | ถูกเรียกใช้โดย | สถานะ |
|----------|:------------:|:-----:|
| `run_fast_eval()` | `get_overall_score()` | ✅ |
| `get_overall_score()` | `main()` | ✅ |
| `main()` | CLI / cockpit.py | ✅ |

#### ab_tournament.py
| Function | ถูกเรียกใช้โดย | สถานะ |
|----------|:------------:|:-----:|
| `check_port_open()` | `main()` | ✅ |
| `get_new_match_log()` | `main()` | ✅ |
| `parse_match_outcome()` | `main()` | ✅ (มี 4 return values — แก้แล้ว) |
| `main()` | CLI / cockpit.py | ✅ |

#### shared_utils.py
| Function | ถูกเรียกใช้โดย | สถานะ |
|----------|:------------:|:-----:|
| `configure_utf8()` | ทุก script ที่ import | ✅ |
| `_registry_filename()` | `get_registry_paths()`, `get_sandbox_registry_path()` | ✅ |
| `get_registry_paths()` | learning_sandbox, optimize_registry, combo_simulator | ✅ |
| `get_sandbox_registry_path()` | q_learning, auto_role_detector | ✅ |
| `get_available_decks()` | learning_sandbox, optimize_registry, combo_simulator, cockpit | ✅ |
| `load_ydk_main_deck()` | auto_role_detector, optimize_registry, combo_simulator | ✅ |
| `load_registry_list()` | learning_sandbox, optimize_registry | ✅ |
| `load_registry_dict()` | combo_simulator | ✅ |
| `save_registry_list()` | ทุก script ที่ต้องการ save | ✅ |

### 3.2 Unused Imports & Variables

| ไฟล์ | รายการ | ประเภท | สถานะ |
|------|--------|--------|:-----:|
| `cockpit.py` | `REGISTRY_PATH` (บรรทัด ~17) | Unused variable | 🟢 ต่ำ |
| `auto_role_detector.py` | `import re` (บรรทัด ~4) | Unused import | 🟢 ต่ำ |
| `combo_simulator.py` | ไม่มี | ✅ Clean | 🟢 |
| `learning_sandbox.py` | `import glob` (บรรทัด ~4 ใน version เก่า) | Unused import | 🟢 ต่ำ (ไม่มีแล้วใน shared_utils) |
| `ab_tournament.py` | `import re` (บรรทัด ~9) | ถูกใช้แล้ว | ✅ |
| `shared_utils.py` | ไม่มี | ✅ Clean | 🟢 |

---

## 4. บอททำอะไรได้บ้าง (Current Capabilities)

### 4.1 Decision Engine — จุดแข็งที่บอทมี

| ความสามารถ | รายละเอียด | ระดับ |
|------------|-----------|:----:|
| **Goal-based Scoring** | ปรับคะแนนตาม 4 goals (push_lethal, survive, break_board, establish_interruptions) | 🟢 **ดี** |
| **Combo Plan Branching** | PlanA → PlanB → PlanC พร้อม backup system | 🟢 **ดี** |
| **Dead Combo Penalty** | -90.0 สำหรับการ์ด combo ที่ plan ถูก block | 🟢 **ดี** |
| **Danger Assessment** | 14+ ปัจจัย (ATK, Level, Extra Deck, Hand, GY, Banished, role, priority, Learned Danger, Staple Baseline) | 🟢 **ดีมาก** |
| **Self-Sabotage Prevention** | Iron Rules 7 ข้อ, Self-chain block, Redundant Field block | 🟢 **ดีมาก** |
| **Handtrap Safeguards** | 9 safeguards เฉพาะ (Ash, Veiler, Droll, Called by, Bystial, Imperm, Gamma, Nibiru, Fuwalos) | 🟢 **ดีมาก** |
| **Opponent Memory** | เรียนรู้ danger score จาก match history (95% decay, dynamic adjust) | 🟡 **ปานกลาง** |
| **Smart Reward Learning** | เฉพาะการ์ด key (starter/payoff/searcher) — ป้องกัน priority inflation | 🟡 **ปานกลาง** |
| **Anti-Inflation Decay** | Priority ≥ 8 ที่ไม่ได้ใช้ → ลดลง (ทำงานก่อน Hard Cap) | 🟡 **ปานกลาง** |
| **Battle Phase AI** | Lethal check, backrow avoidance, memory-based target priority, token priority | 🟡 **ปานกลาง** |
| **Resource Tracking** | Hand count, monster count, deck count, card advantage | 🟡 **ปานกลาง** |
| **Logging System** | Per-turn logs, decisions.jsonl (deduplicated), match_summary.log | 🟢 **ดี** |
| **Resource Tracking Detail** | Low deck → penalize draw/search (-50), card disadvantage → boost recovery (+15) | 🟡 **ปานกลาง** |
| **Dynamic Goal Shifting** | Lethal → survive → break_board → establish_interruptions (auto-detect) | 🟢 **ดี** |
| **Q-Value Integration** | Q-values from registry → score += qVal * 10 | 🟡 **ใหม่** |
| **Graveyard Danger** | ตรวจสอบ GY/Banished recovery threats (0.3-0.5x) | 🟡 **ใหม่** |

### 4.2 จุดอ่อน (Gaps & Weaknesses)

| จุดอ่อน | รายละเอียด | Impact | ระดับ |
|---------|-----------|:------:|:----:|
| **❌ 4 Bricked Decks** | Goldlord, Invoke, Kwtune, Labrynth registry ว่าง | 🔴 เล่นไม่ได้ | Critical |
| **❌ 0 Deck Configs** | decks/*.json ไม่มีเลย — playstyle ="control" ตายตัว ส่งผลให้ OnSelectHand() เลือกไป second เสมอ → **เสียเปรียบหนักสำหรับเด็ค aggro/combo** (BrElfnote, Kwtune, Invoke) | 🔴 ทุก deck | Critical |
| **❌ No OnDraw Override** | ไม่มี hand trap probability model | 🟡 เสียเปรียบ | High |
| **❌ No Lookahead Search** | Greedy decision — ไม่คิดถึง turn ถัดไป | 🟡 เล่นสั้น | High |
| **❌ No Continuous Learning** | Pipeline ต้อง manual trigger | 🟡 ไม่พัฒนา | High |
| **❌ Score Threshold = 35.0** | Hardcoded — ไม่ปรับตามสถานการณ์ | 🟡 ไม่ยืดหยุ่น | Medium |
| **❌ 7.3 Macro-Decision** | Partially redundant — Iron Rule #2 block roles interruption/handtrap/disruption ก่อนถึง 7.3 แต่ 7.3 ยังคงใช้ได้สำหรับ roles negate/removal ที่ Rule #2 ไม่ได้ block | 🟢 เล็กน้อย | Low |
| **❌ Learning Fragile** | ต้องการ LP==0 หรือ fallback turn≥3 | 🟡 ไม่เสถียร | High |
| **❌ _processExitRegistered Static** | Static flag — ถ้ามีหลาย instances ข้อมูลหาย | 🟢 เล็กน้อย | Low |
| **❌ IsLethalOnBoard Main2** | ไม่ครอบคลุม Main Phase 2 | 🟡 พลาด lethal | Medium |
| **❌ Duplicated Parse Logic** | learning_sandbox, q_learning, ab_tournament มี parse_match_outcome ต่างกัน | 🟢 ไม่กระทบ | Low |
| **❌ No Registry Versioning** | ไม่มี snapshot ก่อน deploy | 🟡 เสี่ยง data loss | Medium |
| **❌ Bait Value Bootstrap** | bait=0 → 1 ถ้าเล่นใน Win (อาจไม่ถูกต้อง) | 🟢 เล็กน้อย | Low |
| **❌ OnSelectCard Priority Logic** | Kwtune scoped archetype boost — เฉพาะ deck เดียว | 🟢 เล็กน้อย | Low |

---

## 5. บอทเก่งแค่ไหน (Performance Assessment)

### 5.1 Win Rate โดยประมาณ (Estimated)

| สถานการณ์ | Win Rate โดยประมาณ | เหตุผล |
|-----------|:------------------:|--------|
| **เด็คที่ registry ครบ** (6 decks: AzaYummy, BrElfnote, DarkTime, EvilTwin, EyeInside, Hecahand) | **25-35%** | Goal-based scoring + safeguards + learning |
| **เด็คที่ registry ว่าง** (4 decks: Goldlord, Invoke, Kwtune, Labrynth) | **0-5%** | ไม่เล่นการ์ดหลัก → loss ทุก match |
| **เด็ค registry ครบ + learning เล่น match จริง** | **30-40%** | Learning เริ่มปรับ priority |
| **เทียบกับ WindBot ปกติ (DefaultExecutor)** | **ใกล้เคียง** | IGNIS มี goal/scoring ดีกว่า แต่ Complex เกินบางจังหวะ |

### 5.2 จุดที่บอททำได้ดีกว่าค่าเฉลี่ย

1. **การป้องกัน self-sabotage** — Iron Rules + 9 safeguards → ไม่เล่น handtrap ในเทิร์นตัวเอง, ไม่ chain ใส่การ์ดตัวเอง
2. **การประเมินอันตราย** — 14+ ปัจจัย + opponent memory → เลือกเป้าหมาย negation/removal ได้ดี
3. **การจัดการทรัพยากร** — Resource tracking + low deck penalty → ป้องกัน deckout
4. **Battle Phase safety** — ตรวจ backrow, ตรวจ lethal, priority token → ไม่ตีมั่ว
5. **Learning adaptation** — Smart Reward (เฉพาะ key cards) + Anti-Inflation Decay → ป้องกัน priority inflation

### 5.3 จุดที่บอทแย่กว่าค่าเฉลี่ย

1. **Choke Point Awareness** — ไม่มี deck configs = ไม่มี choke_points → ไม่รู้ว่าควร protect การ์ดไหน
2. **Plan Switching** — PlanA→PlanB→PlanC มีแต่ไม่มี data ว่า card ไหน belong to plan ไหน (combo_plans ใน registry เป็น generic "PlanA")
3. **Continuous Improvement** — Learning ต้อง manual trigger → ไม่พัฒนาเอง
4. **Hand Trap Prediction** — ไม่รู้ว่าฝ่ายตรงข้ามมี hand trap กี่ใบ → เปิดเล่นเสี่ยง
5. **Lookahead** — ตัดสินใจแบบ greedy → ไม่คิดถึง next turn state

---

## 6. จุดบกพร่องและข้อผิดพลาด (Bugs & Issues)

### 🔴 Critical (ต้องแก้ไขทันที)

| # | ปัญหา | ไฟล์/ตำแหน่ง | รายละเอียด |
|:-:|-------|-------------|------------|
| **C1** | **4 Bricked Decks — Registry ว่าง** | `WindBot/config/cards_registry_2026_Goldlord.json`, `Invoke.json`, `Kwtune.json`, `Labrynth.json` | ทั้ง 4 ไฟล์ = copy-paste ของ cards_registry.json (160 cards) — การ์ดหลักของเด็คไม่มีใน registry → OnDefaultActivate → Iron Rule #4 = false → บอทไม่เล่นการ์ดหลัก |
| **C2** | **0 Deck Configs** | `WindBot/config/decks/` | Directory ว่างเปล่า — ทุก deck ใช้ default (playstyle="control", goals/weaknesses/choke_points ว่าง) → OnSelectHand() เลือกไป second เสมอ → choke_points ไม่มี → disruption tracking ไม่รู้ choke point |
| **C3** | **Learning Precondition Fragile** | `ApplyRealTimeLearning()` ~L550 | ต้องการ `botLP == 0 || oppLP == 0` เพื่อทำ priority updates ถ้า match จบด้วย timeout/disconnect/surrender (ไม่ใช่ LP=0) → `_learningApplied = true` เซ็ตแล้วแต่ **learning ไร้ผล** — priority ไม่เปลี่ยนแปลง และจะไม่ถูกเรียกอีกเพราะ `_learningApplied` ป้องกันการเรียกซ้ำ ปัจจุบัน fallback คือ turn≥3 ซึ่งไม่สัมพันธ์กับ match outcome |

### 🟡 High Priority

| # | ปัญหา | ไฟล์/ตำแหน่ง | รายละเอียด |
|:-:|-------|-------------|------------|
| **H1** | **No OnDraw Override** | `UnifiedIgnisExecutor.cs` | ไม่มี override OnDraw() → ไม่รู้ว่าฝ่ายตรงข้ามจั่วได้อะไร → ไม่มี hand trap probability model (ดูเพิ่มเติมใน 2.3.1) |
| **H2** | **Score Threshold คงที่ 35.0** | `EvaluateCardAction()` ~L1480 | `bool decision = score > 35.0;` — ถ้าเกมใกล้จบ (LP ต่ำ) ควรลด threshold |
| **H3** | **Pipeline ต้อง Manual Trigger** | `run_match_learning.py` | ไม่มี auto-loop → learning หลัง match ต้อง manual รัน |
| **H4** | **Static ProcessExit Flag** | `_processExitRegistered` ~L152, `_currentInstance` ~L88 | Static instance — instance หลังสุดถูก save, instance แรกหาย |
| **H5** | **IsLethalOnBoard ไม่ครอบคลุม Main2** | `IsLethalOnBoard()` ~L64 | ตรวจเฉพาะ `Main1 || Battle` → lethal ใน Main2 (หลังตีกลับ) ไม่ถูกตรวจจับ |
| **H6** | **playstyle="control" Default เสียหายต่อ Aggro Decks** | `OnSelectHand()` ~L1851 | decks/*.json ว่าง → ทุก deck ใช้ playstyle="control" → ใน `OnSelectHand()` ระบบเลือก second เสมอ (เพราะ control ต้องการตอบโต้) แต่เด็ค aggro/combo (BrElfnote, Invoke, Kwtune) ต้องการไป first |

### 🟢 Medium-Low Priority

| # | ปัญหา | ไฟล์/ตำแหน่ง | รายละเอียด |
|:-:|-------|-------------|------------|
| **M1** | **Macro-Decision 7.3 Dead Code** | ~L1955 | Iron Rule #2 block roles interruption/handtrap/disruption → 7.3 เหลือแค่ negate/removal roles |
| **M2** | **Duplicated Parse Logic** | learning_sandbox vs q_learning vs ab_tournament | 3 ไฟล์มีฟังก์ชัน parse_match_outcome() ต่างกัน — ควร refactor ไปไว้ shared_utils |
| **M3** | **OnDefaultRepos Raw Position Values** | ~L1671-1698 | ใช้ `(int)CardPosition.FaceDownDefence` แทน `card.IsDefense()` |
| **M4** | **No Registry Versioning** | — | ทุกครั้งที่ deploy → registry เดิมถูกเขียนทับ — ไม่มี rollback |
| **M5** | **Bait Value Bootstrap** | `ApplyRealTimeLearning()` ~L565 | ถ้า card ถูกเล่นใน Win → bait=1 (อาจไม่ถูกต้องถ้าการ์ดนั้นไม่ใช่ bait card) |
| **M6** | **OnDefaultSpellSet Penalty Main1** | ~L1644-1660 | Trap/Quick-Play ใน Main1 มีแค่ -30 penalty — ควร block แทน (-500) |

---

## 7. แนวทางเทรนเพิ่มเติม (Training Pipeline Analysis)

### 7.1 Training Pipeline ปัจจุบัน

```
Step 1: เล่น match (manual หรือ cockpit live_duel)
    ↓
Step 2: run_match_learning.py (manual trigger หรือผ่าน cockpit)
    ├── learning_sandbox.py (Heuristic Updates)
    │   ├── Win → priority +1 (เฉพาะ starter/payoff/searcher)
    │   ├── Loss → priority -1
    │   ├── Disrupted → risk_if_negated +1
    │   ├── Bait value inflation (non-starter/payoff)
    │   └── Anti-Inflation Decay (priority ≥ 8 → -1)
    └── q_learning.py (Q-Value Updates)
        ├── Episodic Monte Carlo Return
        ├── Q(s,a) = Q(s,a) + α * (G_t - Q(s,a))
        └── Clamp [-2.0, 2.0]
    ↓
Step 3: Deploy ผ่าน cockpit (manual)
    └── shutil.copy2(sandbox_reg, live_reg)
    └── compile_ai.bat
```

### 7.2 จุดที่ Pipeline ยังบกพร่อง

| จุดบกพร่อง | รายละเอียด | แนวทางแก้ |
|------------|-----------|-----------|
| **❌ ไม่มี auto-trigger** | หลัง match จบ ต้อง manual รัน learning | เพิ่ม auto-trigger ใน OnChainEnd() หรือ cockpit auto-learning loop |
| **❌ No validation** | ไม่มีการเช็คว่า learning ทำให้บอทดีขึ้นจริง | ใช้ ab_tournament.py อัตโนมัติ หลัง learning |
| **❌ No versioning** | Registry ถูก overwrite — ไม่มี rollback | เพิ่ม backup snapshot ก่อน deploy |
| **❌ Learning single-match** | วิเคราะห์ทีละ match — ไม่ aggregate ข้าม match | เพิ่ม batch learning (หลาย match พร้อมกัน) |
| **❌ No cross-deck learning** | แต่ละ deck registry แยกกัน — ไม่ share knowledge | เพิ่ม transfer learning ข้าม deck |

### 7.3 แนวทางปรับปรุง Pipeline

#### ระยะใกล้ (วันนี้-1 สัปดาห์)

1. **Auto-trigger learning หลัง match จบ**
   - เพิ่ม `cockpit.py` auto-loop mode
   - หรือเพิ่ม post-match hook ใน C# → เรียก Python script

2. **สร้าง Deck Configs**
   - รัน `auto_role_detector.py` สำหรับทุก deck
   - เพิ่ม JSON config (playstyle, choke_points, weaknesses, goals) สำหรับทุก deck

3. **Version Registry ก่อน deploy**
   - `config/registry_backups/cards_registry_{deck}_{timestamp}.json`
   - Keep last 10 versions

#### ระยะกลาง (1-2 สัปดาห์)

4. **Batch Learning** — อ่านหลาย match log พร้อมกัน คำนวณ aggregate adjustment

5. **A/B Validation Loop** — หลัง learning → รัน tournament (registry A vs registry B) → auto-accept ถ้า B ดีกว่า

6. **Dashboard Enhancement** — เพิ่ม win rate graph, priority distribution, learning history

#### ระยะยาว (1 เดือน+)

7. **Continuous Learning Loop**
   ```
   while True:
       play_match()
       run_learning()
       validate_with_ab()
       if B beats A:
           deploy()
           log("improved!")
       else:
           rollback()
           log("regression detected - reverting")
   ```

8. **Cross-Deck Transfer Learning**
   - สร้าง base knowledge (handtrap danger, staple usage) ที่ share ข้าม deck
   - เฉพาะ deck-specific roles (starter, combo_piece) ที่แยกกัน

---

## 8. แนวทางอัพเกรดเพิ่มเติม (Upgrade Paths)

### 8.1 Quick Wins (1-2 ชม.)

| # | การปรับปรุง | Impact | Effort | ไฟล์ |
|:-:|------------|:------:|:------:|------|
| 1 | รัน `auto_role_detector.py` ครบ 4 deck | 🔴 สูงมาก | ⚡ 30 นาที | CLI |
| 2 | สร้าง deck configs JSON | 🔴 สูง | 🕐 1 ชม. | `WindBot/config/decks/` |
| 3 | แก้ OnDraw override — track hand count | 🟡 กลาง | 🕐 30 นาที | `UnifiedIgnisExecutor.cs` |
| 4 | Refactor parse_match_outcome → shared_utils | 🟢 ต่ำ | 🕐 30 นาที | Python scripts |
| 5 | Dynamic Score Threshold | 🟡 กลาง | 🕐 1 ชม. | `EvaluateCardAction()` |

### 8.2 Mid-Term (1-2 สัปดาห์)

| # | การปรับปรุง | Impact | Effort |
|:-:|------------|:------:|:------:|
| 6 | **Hand Trap Probability Model** | 🟢 สูง | 🕐 5 ชม. |
| 7 | **Battle Phase AI Enhancement** | 🟢 สูง | 🕐 4 ชม. |
| 8 | **Continuous Learning Loop** | 🟢 สูง | 📅 8 ชม. |
| 9 | **Registry Versioning** | 🟡 กลาง | 🕐 2 ชม. |
| 10 | **A/B Validation Auto-Accept** | 🟡 กลาง | 🕐 3 ชม. |

### 8.3 Advanced (1 เดือน+)

| # | การปรับปรุง | Impact | Effort |
|:-:|------------|:------:|:------:|
| 11 | **1-Turn Lookahead Search** | 🔴 สูงมาก | 📅 10 ชม. |
| 12 | **Chain Optimization Priority** (should I chain Ash or Veiler first?) | 🟡 กลาง | 🕐 3 ชม. |
| 13 | **MCTS Integration** for combo planning | 🔴 สูงมาก | 📅 20+ ชม. |
| 14 | **Full Opponent Adaptation** (ระบุ archetype, predict plays) | 🔴 สูงมาก | 📅 15+ ชม. |
| 15 | **Dashboard Analytics Enhancement** | 🟡 กลาง | 📅 5 ชม. |

### 8.4 รายละเอียดแนวทางอัพเกรดที่แนะนำ

#### 8.4.1 Hand Trap Probability Model

```csharp
// ใน OnDraw() override
public override void OnDraw(int player)
{
    // ถ้าฝ่ายตรงข้ามจั่วขึ้นมา 5 ใบแล้วยังไม่ activate
    // → มี probability สูงที่มือมี hand trap
    if (player == 1 && Duel.Turn == 1 && !opponentHasActivated)
    {
        _opponentHandTrapProb = EstimateHandTrapProbability(opponentHandCount);
    }
}

private bool ShouldPlayAroundHandTraps()
{
    return _opponentHandTrapProb > 0.35; // > 35% → ควรมี bait ก่อน play starter
}
```

**Logic:**
- ถ้าฝ่ายตรงข้ามจั่ว 5 (หรือ 6) ใบ + ไม่ activate → hand trap prob = 40-60%
- ถ้าฝ่ายตรงข้ามใช้ Maxx "C" → prob = 70% (มี hand trap เพิ่ม)
- ถ้า pass turn โดยไม่ทำอะไร → prob = 20% (อาจเป็น brick)

#### 8.4.2 Battle Phase AI Enhancement

**ปัจจุบัน:**
- Safe attack check → priority weakest → direct attack if possible
- Memory-based avoidance

**ควรเพิ่ม:**
- **Lethal sequencing** — รู้ว่าควรตีโปสเตอร์ไหนก่อน-หลัง (attack order optimization)
- **Battle trap baiting** — ถ้าสงสัยว่า opp มี battle trap → ตีด้วย monster ที่เล็กที่สุดก่อน
- **Chain blocking** — ควร attack เมื่อไหร่เพื่อ block opp จาก chain
- **Main Phase 2 awareness** — ถ้า lethal ใน Main2 → ไม่ต้อง battle แต่ทำอย่างอื่นก่อน

#### 8.4.3 1-Turn Lookahead Search

```csharp
private double EvaluateLookahead(ClientCard cardToPlay, ExecutorType type)
{
    double currentScore = EvaluateCardAction(cardToPlay, meta, type);
    
    // Simulate next turn state
    SimulatedState nextState = SimulatePlay(cardToPlay);
    
    // Evaluate hand cards in next state
    foreach (var handCard in nextState.Hand)
    {
        currentScore += EvaluateCardAction(handCard, _cardRegistry[handCard.Id], type) * 0.5;
    }
    
    return currentScore;
}
```

**Key insight:** Lookahead แค่ 1 turn ก็ทำให้บอทเล่นดีขึ้นมาก — เช่น เล่น Pot of Desires ก่อน (draw 2) = ได้ resource เพิ่ม → เล่นต่อได้

---

## 9. Roadmap 3 ระยะ

### ระยะที่ 1 — กู้ชีพ 🚨 (1-2 วัน)

| Task | Priority | Status |
|------|:--------:|:------:|
| ✅ Fix 4 bricked registries (Goldlord, Invoke, Kwtune, Labrynth) | 🔴 P0 | ⏳ ยังไม่ทำ |
| ✅ Fix ab_tournament.py crash | 🔴 P0 | ✅ แก้แล้ว |
| ✅ Fix Learning Fragile (auto-save) | 🔴 P1 | ⏳ ยังไม่ทำ |
| ✅ สร้าง deck configs สำหรับ 10 decks | 🔴 P1 | ⏳ ยังไม่ทำ |
| ✅ รัน `auto_role_detector.py` สำหรับทุก deck | 🔴 P1 | ⏳ ยังไม่ทำ |
| ✅ ทดสอบ 4 decks ที่กู้ชีพ — ตรวจสอบว่าเล่นได้ | 🔴 P1 | ⏳ ยังไม่ทำ |

### ระยะที่ 2 — อัพเกรด 🚀 (1-2 สัปดาห์)

| Task | Priority | Win Rate เป้าหมาย |
|------|:--------:|:-----------------:|
| ✅ Hand Trap Probability Model | 🟡 P2 | +5% |
| ✅ Battle Phase AI Enhancement | 🟡 P2 | +5% |
| ✅ Dynamic Score Threshold | 🟡 P2 | +3% |
| ✅ Continuous Learning Loop | 🟢 P3 | +5% |
| ✅ Registry Versioning | 🟢 P3 | protection |
| ✅ A/B Validation Loop | 🟢 P3 | +3% |
| **รวมหลังระยะ 2** | | **30-45% → 50-60%** |

### ระยะที่ 3 — Advanced 🧠 (1-3 เดือน)

| Task | Priority | Win Rate เป้าหมาย |
|------|:--------:|:-----------------:|
| ✅ Lookahead Search (1-2 turns) | 🔴 P0 | +10% |
| ✅ Chain Optimization | 🟡 P2 | +5% |
| ✅ MCTS Combo Planning | 🔴 P0 | +10% |
| ✅ Dashboard Analytics | 🟢 P3 | monitoring |
| ✅ Full Opponent Adaptation | 🔴 P1 | +5% |
| **รวมหลังระยะ 3** | | **50-60% → 70-85%** |

---

## 10. สรุป (Summary)

### 10.1 สถานะปัจจุบัน

```
┌─────────────────────────────────────────────────────────────────────┐
│  BOT STRENGTH: 25-35% Win Rate (ถ้า registry ครบ)                  │
│                 0-5%  Win Rate (4 decks bricked)                    │
│                                                                     │
│  ✅ Foundation แข็งแรง: Goal scoring, Iron Rules, Danger Assessment │
│  ❌ Critical Bugs: 4 bricked decks, 0 deck configs                  │
│  🟡 Medium: No hand trap model, no continuous learning              │
│  🟢 Future: Full potential 70-85% with Lookahead + MCTS            │
└─────────────────────────────────────────────────────────────────────┘
```

### 10.2 สิ่งสำคัญที่สุดที่ต้องทำทันที

1. **รัน `auto_role_detector.py` สำหรับ Goldlord, Invoke, Kwtune, Labrynth** — กู้ชีพ 4 decks ที่เล่นไม่ได้
2. **สร้าง deck configs JSON** — ให้ choke_points, weaknesses, goals สำหรับทุก deck
3. **เพิ่ม OnDraw override + Hand Trap Probability Model** — ป้องกันบอทเล่นผิดจังหวะ
4. **ทำให้ Continuous Learning Loop** — auto learn → auto deploy → auto validate

### 10.3 ข้อควรระวัง

- **⚠️ ห้ามแก้ไข Iron Rules โดยไม่ได้รับอนุมัติ** — 7 ข้อนี้ป้องกัน self-sabotage
- **⚠️ Card ID Mismatches** — Nibiru (27204311), Gamma (38814750), Called by (24224830) ตรวจสอบ ID ให้ถูกต้อง
- **⚠️ Reference Equality** — OnSelectCard ใช้ `c == Card` (object reference) ไม่ใช่ `c.Id == Card.Id`
- **⚠️ ห้ามใช้ Raw Values** สำหรับตำแหน่ง — ใช้ `card.IsFaceup()` แทน `card.Position == 1`

---

*รายงานนี้จัดทำโดย **Buffy** (Codebuff AI Agent)*  
*ประทับเวลา: 2026-05-24 23:59 ICT*  
*วิเคราะห์จาก: UnifiedIgnisExecutor.cs (C#), WindBot_Sandbox/*.py (Python), cards_registry_*.json, deck configs, opponent_memory.json, bots.json, WindBot.exe.config, system.conf*
