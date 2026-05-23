# WindBot IGNIS — Comprehensive Refactor Analysis

> จัดทำ: 2026-05-23 | อ่านระบบเสร็จสมบูรณ์: UnifiedIgnisExecutor.cs (1,860 บรรทัด) + Python Sandbox 10 ไฟล์ (~2,500 บรรทัด) + Configs

---

## สารบัญ

1. [ภาพรวมระบบ](#1-ภาพรวมระบบ)
2. [การทำงานของ Cockpit — Multi-Duel Loop](#2-การทำงานของ-cockpit--multi-duel-loop)
3. [Dead Code / Unreachable Conditions](#3-dead-code--unreachable-conditions)
4. [Critical Bugs](#4-critical-bugs)
5. [High-Severity Logical Bugs](#5-high-severity-logical-bugs)
6. [Refactoring Roadmap (แบ่งเป็นเฟส)](#6-refactoring-roadmap-แบ่งเป็นเฟส)
7. [ปัญหาที่ต้องตัดสินใจก่อน Refactor](#7-ปัญหาที่ต้องตัดสินใจก่อน-refactor)
8. [Appendix: Complete Issue Register](#8-appendix-complete-issue-register)

---

## 1. ภาพรวมระบบ

```
┌───────────────────────────────────────────────────────────┐
│                     EDOPro Game Engine                     │
│  (EDOPro.exe + ocgcore.dll + script/*.lua 20,000+ ไฟล์)   │
└──────────────────────┬────────────────────────────────────┘
                       │ game state, actions
                       ▼
┌───────────────────────────────────────────────────────────┐
│                   WindBot.exe (.NET)                       │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  UnifiedIgnisExecutor.cs  (1,860 บรรทัด)           │  │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────────────┐    │  │
│  │  │ Scoring  │ │ Learning │ │ Goal/Plan Engine │    │  │
│  │  │ Engine   │ │ Pipeline │ │ (UpdateGoal,     │    │  │
│  │  │          │ │ (RL on   │ │  GetNextPlan,    │    │  │
│  │  │ Evaluate │ │  match   │ │  OnChaining)     │    │  │
│  │  │CardAction│ │  end)    │ │                  │    │  │
│  │  └──────────┘ └──────────┘ └──────────────────┘    │  │
│  └─────────────────────────────────────────────────────┘  │
└──────────────────────┬────────────────────────────────────┘
                       │ logs decisions.jsonl, match logs
                       ▼
┌───────────────────────────────────────────────────────────┐
│              WindBot_Sandbox (Python)                      │
│                                                           │
│  cockpit.py ── HTTP Dashboard (port 8000)                 │
│     │                                                     │
│     ├──▶ run_match_learning.py                             │
│     │      ├──▶ learning_sandbox.py (heuristic)            │
│     │      └──▶ q_learning.py (Q-Learning)                 │
│     │                                                      │
│     ├──▶ combo_simulator.py (Monte Carlo hands)            │
│     ├──▶ ab_tournament.py (A/B testing)                   │
│     ├──▶ optimize_registry.py (hill climbing)              │
│     └──▶ auto_role_detector.py (role from card text)      │
│                                                           │
│  shared_utils.py ── central path/registry utilities       │
└───────────────────────────────────────────────────────────┘
```

### Data Flow

```
cards_registry_<deck>.json  ──▶  UnifiedIgnisExecutor.cs
  (CardMetadata: priority, roles, q_values, ฯลฯ)

opponent_memory.json  ──▶  UnifiedIgnisExecutor.cs
  (OpponentCardMeta: times_seen, learned_danger)

UnifiedIgnisExecutor.cs  ──▶  Logs/<match>/
  (decisions.jsonl, turn_N.log, match_summary.log)

Logs/  ──▶  learning_sandbox.py  ──▶  cards_registry_<deck>.json
  (heuristic weight updates)

Logs/  ──▶  q_learning.py  ──▶  cards_registry_<deck>.json
  (Q-value Monte Carlo updates)

Sandbox registry  ──▶  (manual deploy)  ──▶  WindBot/config/
```

---

## 2. การทำงานของ Cockpit — Multi-Duel Loop

### cockpit.py คืออะไร

เป็น HTTP Web Server (Python `http.server` พอร์ต 8000) ที่รอคำสั่งจากหน้าจอ browser
มี 6 โหมดการทำงาน:

| โหมด | ฟังก์ชัน | เรียกผ่าน |
|------|----------|-----------|
| `heuristic` | optimize_registry.py (Hill Climbing) | `/api/start?mode=heuristic&deck=...` |
| `simulator` | combo_simulator.py (Monte Carlo) | `/api/start?mode=simulator&deck=...` |
| `real_match` | run_match_learning.py (Learning Pipeline) | `/api/start?mode=real_match&deck=...` |
| `ab_tournament` | ab_tournament.py (A/B Testing) | `/api/start?mode=ab_tournament&deck=...` |
| `live_duel` | เรียก WindBot.exe ตรง 2 instance | `/api/start?mode=live_duel` |
| `kill` | kill กระบวนการ | `/api/kill` |

### live_duel mode ทำงานยังไง

```
cockpit.py  ──▶  WindBot.exe P1 (deck A, hostinfo=)
              ──▶  WindBot.exe P2 (deck B, hostinfo=)
              ──▶  ทั้งคู่ connect ไป port 7911
              ──▶  รอจน process ตาย (match จบ)
              ──▶  อ่าน log จาก LIVE_LOGS_DIR
              ──▶  loop กลับไปรัน match ใหม่
```

**ข้อจำกัดปัจจุบัน:**
- P1 Hardcode ชื่อ `"IgnisBot"` (บรรทัด 111) — ไม่เปลี่ยนตาม deck
- ทั้งคู่ `hostinfo=` ว่าง — ใช้ settings เริ่มต้น
- รอแค่ process ตาย daemon thread flush ไม่ทัน (race condition)

### ถ้าจะให้ Bot เก่งขึ้นจาก Multi-Duel

แนวทางที่ระบบรองรับอยู่แล้ว:

1. **AB Tournament (`ab_tournament.py`)** — ใช้เปรียบเทียบ registry config A vs B
   - ปัญหา: อัด C# class name ผิด grammar ถ้า deck name ขึ้นต้นด้วยตัวเลข → compile fail
   - ต้องแก้ที่ `ab_tournament.py` บรรทัด 203-215

2. **Learning Pipeline (`run_match_learning.py`)** — ใช้ train bot จาก match logs
   - Step 1: `learning_sandbox.py` ปรับ heuristic weights
   - Step 2: `q_learning.py` ปรับ Q-values
   - ปัญหา: Step 1 ไม่มี `--deck` filter → process ทุก deck → cross-contamination

3. **Q-Learning (`q_learning.py`)** — Monte Carlo update Q-values
   - ใช้ได้เฉพาะการ์ดที่มี `q_values` ใน registry
   - ต้องการ `--iterations N` เพื่อทำหลายรอบ

4. **Hill Climbing (`optimize_registry.py`)** — ปรับ weights อัตโนมัติ
   - แต่ละ iteration = 3000 simulations
   - 2 decks × 300 iterations = 1.8M simulations (ช้ามาก)

5. **Manual tuning ผ่าน cockpit dashboard** — อ่าน log, snapshot registry, deploy

### สิ่งที่ยังขาด / ต้องเพิ่ม

- **Continuous Learning Loop**: ระบบไม่มีการวน loop match → learn → deploy → match ใหม่แบบอัตโนมัติ ต้องมี orchestrator script
- **Versioning**: ทุกครั้งที่ deploy registry จะเขียนทับ ไม่มี history
- **Validation**: ไม่มี unit test สำหรับ scoring engine
- **Sandbox Isolation**: learning sandbox registry กับ live registry เป็นคนละ path ต้อง deploy ด้วยตนเอง

---

## 3. Dead Code / Unreachable Conditions

### 3.1 UnifiedIgnisExecutor.cs (C# — ตัว AI)

| บรรทัด | ปัญหา | ประเภท |
|--------|-------|--------|
| 568-569 | `int delta = (outcome == "Loss") ? 1 : 0;` + `if (delta != 0)` — **ตัวแปร delta ไม่ได้ถูกใช้ต่อ** บรรทัด 570 คำนวณ priority ใหม่โดยไม่สน delta | **Dead Code** |
| 557 | `strength >= 0.5` — redundant เพราะ `strength` มีค่า 1.0 (Win) หรือ 0.5 (WeakWin) เสมอ ทำให้เงื่อนไขนี้เป็น `true` ตลอด | **Always True** |
| 557 | `meta.priority < 10` — redundant เพราะ `meta.priority < 8` ที่อยู่ต่อมา restrictive กว่า | **Always True เมื่อผ่าน < 8** |
| 291, 300 | `as System.Collections.ArrayList` — `JavaScriptSerializer` deserialize JSON arrays เป็น `object[]` ไม่ใช่ `ArrayList` → **roles และ combo_plans ไม่เคยถูก load** | **เงื่อนไขเป็น null ตลอด** |

### 3.2 Python Sandbox

| ไฟล์ | บรรทัด | ปัญหา |
|------|--------|-------|
| `cockpit.py` | 82-97 | `forward_bot_logs()` — Define ไว้แต่ **ไม่เคยถูกเรียก** ที่ไหนใน codebase |
| `learning_sandbox.py` | 4 | `import glob` — **ไม่เคยถูกใช้** |
| `auto_role_detector.py` | 4 | `import re` — **ไม่เคยถูกใช้** |
| `optimize_registry.py` | 2 | `import json` — **ไม่เคยถูกใช้** |

---

## 4. Critical Bugs

### B1: JavaScriptSerializer + ArrayList = Roles/ComboPlans หาย (UnifiedIgnisExecutor.cs:291, 300)

```csharp
object rawRoles = item["roles"] as System.Collections.ArrayList;
```

`JavaScriptSerializer` (.NET 4.x) deserialize JSON arrays เป็น `object[]` **ไม่ใช่** `ArrayList`
ดังนั้น `as ArrayList` → null เสมอ

**ผลกระทบ:**
- `roles` field ใน `CardMetadata` ว่างทุกใบ
- `combo_plans` field ใน `CardMetadata` ว่างทุกใบ
- Iron Rule #1 (`meta.roles.Contains("handtrap")`) จะเป็น `false` ตลอด → **Handtrap ไม่ถูกปิดกั้น**
- `OnDefaultSummon()` / `OnDefaultMonsterSet()` calls `HasStarterOrExtenderInHand()` → `false` ตลอด

**ต้องแก้:** เปลี่ยนเป็น `(item["roles"] as object[])?.Select(o => o.ToString()).ToList()` หรือใช้ `Newtonsoft.Json`

### B2: IsLightOrDark ใช้ Bitwise AND — อาจ Always False (UnifiedIgnisExecutor.cs:887-888)

```csharp
int attr = (int)card.Attribute;
if ((attr & 0x10) != 0 || (attr & 0x20) != 0)
```

ถ้า `Attribute` enum เป็น sequential enum (เช่น `LIGHT = 1, DARK = 2, ...`) การใช้ `&` (bitwise) จะได้ 0 เสมอ
แต่ถ้าเป็น Flags enum (เช่น `LIGHT = 0x10, DARK = 0x20`) ก็ใช้ได้

**ผลกระทบถ้าใช้ bitwise ไม่ได้:**
- `GetOpponentGraveLightDarkCount()` = 0 ตลอด
- Bystial cards ไม่มีวัน activate เพราะ `GetOpponentGraveLightDarkCount() == 0` เสมอ

### B3: AB Tournament C# Class Name Invalid (ab_tournament.py:203-215)

```python
class_name = f"{deck_name}VerAExecutor"
```

C# identifiers **ห้ามขึ้นต้นด้วยตัวเลข** ถ้า deck name = `2026_AzaYummy` → `2026_AzaYummyVerAExecutor` → compile error

### B4: bots.json — 4 entries ขาด 필필 필 필 (บรรทัด 456-478)

`Goldlord`, `Invoke`, `Kwtune`, `Labrynth` ขาด `difficulty` และ `masterRules`

---

## 5. High-Severity Logical Bugs

### 5.1 HasStarterOrExtenderInHand ไม่เช็ค "payoff" (บรรทัด 1327)

เช็คแค่ `"starter"` และ `"extender"` — มองไม่เห็น payoff cards
→ ถ้ามือมีแต่ payoff + tuner: bot จะ normal summon tuner แทนที่จะเก็บไว้ใช้ combo
→ ควรเพิ่ม `"payoff"` และ `"searcher"` ใน HasStarterOrExtenderInHand

### 5.2 CalculateTotalDangerForField ไม่นับ Hand/GY/Banished (บรรทัด 688-693)

นับเฉพาะ `MonsterZone` + `SpellZone`
→ **Handtraps, GY effects ไม่ถูกนับ** → `UpdateGoal` ประเมิน danger ต่ำเกินไป

### 5.3 Learning Pipeline ข้าม deck (run_match_learning.py:22-23)

```python
# Step 1: heuristic learning — ไม่มี --deck → process ALL decks
# Step 2: q-learning — มี --deck → process แค่ deck ที่ระบุ
```

Step 1 ไม่มี filter → cross-deck contamination

### 5.4 combo_simulator.py Brick/Starter นับเกิน (บรรทัด 66-104)

ทุกการ์ดในมือได้ credit เมื่อ combo success หรือ brick → inflate statistics
→ simulation results เชื่อถือไม่ได้

### 5.5 combo_simulator.py rescue > 500 ไม่ scale (บรรทัด 154)

500 เป็น absolute threshold: สำหรับ 1K sims = 50%, สำหรับ 100K sims = 0.5%
→ ควรเป็น percentage

### 5.6 ab_tournament.py avg_turns คำนวณผิด (บรรทัด 296)

```python
avg_turns = total_turns / max(played + ties, 1)
```
`total_turns` นับแค่ `played` matches แต่ตัวหารรวม `played + ties` → เปอร์เซ็นต์ต่ำกว่าความเป็นจริง

### 5.7 Anti-Inflation Decay + Hard Cap ทำงานสลับ (บรรทัด 596-632)

Hard Cap (priority > 8 → 8) ทำงานก่อน Anti-Inflation Decay (priority >= 9 → ลด 1)
→ Decay ไม่มีผลเพราะ Hard Cap ทำก่อน → Decay clause dead

---

## 6. Refactoring Roadmap (แบ่งเป็นเฟส)

### Phase 0 — Fix Critical Bugs (ห้ามเลื่อน)

| ลำดับ | จุด | สิ่งที่ต้องแก้ |
|-------|-----|---------------|
| 1 | `UnifiedIgnisExecutor.cs:291` | เปลี่ยน `as ArrayList` เป็น `as object[]` + `.Select(o => o.ToString()).ToList()` |
| 2 | `UnifiedIgnisExecutor.cs:300` | เหมือนกันสำหรับ combo_plans |
| 3 | `UnifiedIgnisExecutor.cs:887-888` | ตรวจสอบว่า `Attribute` enum เป็น Flags หรือไม่ ถ้าไม่ใช่ เปลี่ยนเป็น `card.Attribute == CardAttribute.Light \|\| card.Attribute == CardAttribute.Dark` |
| 4 | `ab_tournament.py:203-215` | ใส่ `@` prefix หรือ `_` prefix หน้า class name ถ้าขึ้นต้นด้วยตัวเลข |
| 5 | `bots.json:456-478` | เพิ่ม `difficulty` และ `masterRules` ให้ Goldlord, Invoke, Kwtune, Labrynth |

### Phase 1 — Fix Logical Bugs + Dead Code

| ลำดับ | จุด | สิ่งที่ต้องแก้ |
|-------|-----|---------------|
| 6 | `UnifiedIgnisExecutor.cs:568-569` | ลบบรรทัดที่สร้าง `delta` ที่ไม่ได้ใช้ (dead code) |
| 7 | `UnifiedIgnisExecutor.cs:596-632` | สลับลำดับ Anti-Inflation Decay → ก่อน Hard Cap |
| 8 | `UnifiedIgnisExecutor.cs:1327` | เพิ่ม `"payoff"`, `"searcher"`, `"combo_piece"` ใน `HasStarterOrExtenderInHand()` |
| 9 | `UnifiedIgnisExecutor.cs:688-693` | เพิ่ม `Hand`, `Graveyard`, `Banished` ใน `CalculateTotalDangerForField()` |
| 10 | `cockpit.py:82-97` | ลบ `forward_bot_logs()` หรือ implement ให้ถูกเรียก |
| 11 | `learning_sandbox.py:4` | ลบ `import glob` |
| 12 | `auto_role_detector.py:4` | ลบ `import re` |
| 13 | `optimize_registry.py:2` | ลบ `import json` |

### Phase 2 — Refactor Architecture (Major)

| ลำดับ | จุด | สิ่งที่ต้องแก้ |
|-------|-----|---------------|
| 14 | `UnifiedIgnisExecutor.cs:235-415` | แยก `LoadConfiguration` → `LoadCardRegistry()`, `LoadCardNames()`, `LoadDeckConfig()`, `LoadOpponentMemory()` |
| 15 | `UnifiedIgnisExecutor.cs:247-262` / `426-446` | Extract file path resolution → helper method |
| 16 | `UnifiedIgnisExecutor.cs:935-1273` | แยก `EvaluateCardAction()` (~340 บรรทัด) → scoring components |
| 17 | `UnifiedIgnisExecutor.cs:514-681` | แยก `ApplyRealTimeLearning()` (~170 บรรทัด) |
| 18 | `UnifiedIgnisExecutor.cs:1791-1798` | เปลี่ยน destructor → `IDisposable` pattern |
| 19 | `UnifiedIgnisExecutor.cs:18-43` | เปลี่ยน `camelCase` properties → `PascalCase` |
| 20 | `UnifiedIgnisExecutor.cs:18-43` | เปลี่ยน `ArrayList` → `List<string>`, `List<int>` |
| 21 | `UnifiedIgnisExecutor.cs:50-51` | เปลี่ยน `string` goal/plan → `enum` |
| 22 | `UnifiedIgnisExecutor.cs:39` | เปลี่ยน `string playstyle` → `enum Playstyle` |

### Phase 3 — Extract Magic Numbers + Configs

| ลำดับ | จุด | สิ่งที่ต้องแก้ |
|-------|-----|---------------|
| 23 | ทุก magic number ใน `UnifiedIgnisExecutor.cs` | สร้าง `static class Constants` หรือ config file |
| 24 | `UnifiedIgnisExecutor.cs:766-778` | เปลี่ยน hardcoded staple card IDs → config |
| 25 | `UnifiedIgnisExecutor.cs:982-1038` | เปลี่ยน inline card ID checks → strategy pattern / lookup dict |
| 26 | `UnifiedIgnisExecutor.cs:715` | Danger threshold 40.0 |
| 27 | `UnifiedIgnisExecutor.cs:1270` | Decision threshold 35.0 |
| 28 | `UnifiedIgnisExecutor.cs:1041` | Base score multiplier 10.0 |
| 29 | `UnifiedIgnisExecutor.cs:508` | Default learned danger 10.0 |
| 30 | `UnifiedIgnisExecutor.cs:667` | Natural decay rate 0.95 |

### Phase 4 — Sandbox Pipeline Improvement

| ลำดับ | จุด | สิ่งที่ต้องแก้ |
|-------|-----|---------------|
| 31 | `run_match_learning.py:22-23` | เพิ่ม `--deck` flag ให้ Step 1 หรือจำกัด scope |
| 32 | สร้าง orchestrator script | วน loop: match → learn → deploy → match ใหม่ อัตโนมัติ |
| 33 | `learning_sandbox.py:210-228` | แก้ bait_value inflation logic |
| 34 | `learning_sandbox.py:240-251` | Optimize bootstrap loop (O(cards×matches×decisions)) |
| 35 | `combo_simulator.py:154` | เปลี่ยน `rescue > 500` → percentage-based |
| 36 | `ab_tournament.py:296` | แก้ `avg_turns` denominator |
| 37 | `auto_role_detector.py:98` | เพิ่ม `"graveyard"` detection นอกเหนือจาก `"gy"` |
| 38 | `shared_utils.py:59-75` | เพิ่ม mechanism ให้ role detection re-run ได้ |
| 39 | เพิ่ม `--confirm` flag สำหรับ save to live | ป้องกัน accidental deploy |

### Phase 5 — Testing & Validation

| ลำดับ | จุด | สิ่งที่ต้องแก้ |
|-------|-----|---------------|
| 40 | สร้าง unit tests สำหรับ `EvaluateCardAction` | Mock game state → verify scoring |
| 41 | สร้าง integration tests สำหรับ learning pipeline | Mock log → verify registry output |
| 42 | Add versioning to registry deploys | ทุก deploy save snapshot + timestamp |
| 43 | Add validation on save | ตรวจสอบว่า registry data ไม่ corrupt |

---

## 7. ปัญหาที่ต้องตัดสินใจก่อน Refactor

### 7.1 JavaScriptSerializer → อะไร?

ปัจจุบันใช้ `JavaScriptSerializer` ซึ่งมีปัญหา:
- Deserialize arrays เป็น `object[]` ไม่ใช่ `ArrayList` **(CRITICAL BUG)**
- Type casting ต้องทำเองทุกครั้ง (unsafe)

**ตัวเลือก:**
1. Replace with `Newtonsoft.Json` (Json.NET) — mature, typed, popular
2. Replace with `System.Text.Json` — built-in .NET, modern
3. Custom helper wrapper ที่ manage `object[]` → `List<string>` conversion

**Recommend:** `System.Text.Json` ถ้า .NET 5+ หรือ `Newtonsoft.Json` ถ้า .NET Framework

### 7.2 Attribute Enum — Flags หรือไม่?

ถ้า `Attribute` ใน `YGOSharp.OCGWrapper.Enums` เป็น sequential enum (`LIGHT=1, DARK=2, ...`):
- `IsLightOrDark()` ใช้ `&` ผิด → always false
- ต้องเปลี่ยนเป็น `==` comparison

ต้องเปิดดู definition ของ enum ก่อนแก้

### 7.3 Learning Pipeline ควร auto-run หรือ manual?

- **Auto (continuous loop):** Cockpit มี `live_duel` mode + API → สร้าง script runner
- **Manual (current):** ต้องกด start แต่ละ step เอง

แนะนำ: Hybrid — auto loop แต่มี human-in-the-loop ก่อน deploy

### 7.4 Sandbox ↔ Live isolation strategy?

- **Current:** Sandbox path vs Live path, manual copy
- **Option:** Git-style versioning (registry commits), auto-deploy with rollback

### 7.5 ต้องการสนับสนุน multi-format หรือไม่?

- **Current:** Hardcoded zone sizes (7 monster, 8 S/T), hardcoded rule assumptions
- **Future:** ต้อง parameterize ตาม format (Rush, Speed, GOAT)

---

## 8. Appendix: Complete Issue Register

### UnifiedIgnisExecutor.cs (43 issues)

| ID | บรรทัด | Severity | ประเภท | คำอธิบาย |
|----|--------|----------|--------|----------|
| CS-01 | 18-26 | Medium | Naming | camelCase properties → ควรเป็น PascalCase |
| CS-02 | 19,25 | Medium | Type Safety | ArrayList → ควรเป็น `List<string>` |
| CS-03 | 26 | Low | Type Safety | `Dictionary<string, object>` → ควรเป็น `Dictionary<string, double>` |
| CS-04 | 39-42 | Medium | Naming | camelCase + ArrayList |
| CS-05 | 39 | Medium | Design | `playstyle` string → ควรเป็น enum |
| CS-06 | 50-51 | Medium | Design | `_currentGoal`/`_currentPlan` string → ควรเป็น enum |
| CS-07 | 69 | Low | Design | Singleton pattern — not thread safe |
| CS-08 | 93-96 | Low | Hardcoded | GUID truncation 8 chars |
| CS-09 | 98-111 | Medium | Error Handling | Log directory creation fail → silent logging loss |
| CS-10 | 142-145 | Low | Design | `Log()` ใช้ Console.WriteLine อาจไม่เห็นใน UI |
| CS-11 | 150-154 | Medium | Error Handling | Empty catch blocks (ซ้ำหลายจุด) |
| CS-12 | 177 | Low | Logic | Dedup key `"{turn}_{cardId}_{action}"` อาจข้าม legitimate re-evaluation |
| CS-13 | 182 | Medium | Bug Risk | Manual JSON string construction → injection prone |
| CS-14 | 200-211 | Low | Hardcoded | Retry config (10 retries, 100ms base) |
| CS-15 | 247-262 | Medium | Duplication | File path resolution logic ซ้ำกับ 426-446 |
| **CS-16** | **291** | **CRITICAL** | **Bug** | **`as ArrayList` → null เสมอ (roles หาย)** |
| **CS-17** | **300** | **CRITICAL** | **Bug** | **`as ArrayList` → null เสมอ (combo_plans หาย)** |
| CS-18 | 274-279 | High | Bug Risk | `(int)item["id"]` — InvalidCastException ถ้า long |
| CS-19 | 349 | High | Bug Risk | `rawDict["playstyle"]` — KeyNotFoundException |
| CS-20 | 364 | High | Bug Risk | `(int)c` — InvalidCastException ถ้า c เป็น long |
| CS-21 | 394-398 | Low | Inconsistency | ใช้ `Convert.ToInt32` ที่นี่ แต่ `(int)` cast ที่อื่น |
| CS-22 | 449 | Low | Performance | New JavaScriptSerializer ทุก call |
| CS-23 | 471 | Low | Error Handling | Backup fail → silent swallow |
| CS-24 | 508 | Low | Hardcoded | `learned_danger = 10.0` |
| CS-25 | **557** | **Low** | **Dead Code** | **`strength >= 0.5` always true** |
| CS-26 | **557** | **Low** | **Dead Code** | **`meta.priority < 10` redundant (มี < 8 ต่อ)** |
| **CS-27** | **568-569** | **Low** | **Dead Code** | **`int delta` defined but never used** |
| CS-28 | 570 | Low | Readability | Nested ternary → helper method |
| CS-29 | 574-587 | Medium | Performance | O(n²) loop on disruption |
| CS-30 | **596-632** | **High** | **Logic** | **Hard Cap ก่อน Decay → Decay dead** |
| CS-31 | 650-653 | Low | Hardcoded | Danger increment values |
| CS-32 | 667-668 | Low | Hardcoded | Decay rate 0.95, min danger 5.0 |
| CS-33 | 688-693 | High | Logic | Danger ไม่นับ Hand/GY/Banished |
| CS-34 | 715 | Medium | Hardcoded | Danger threshold 40.0 |
| **CS-35** | **887-888** | **CRITICAL** | **Bug** | **Bitwise AND — อาจ always false ถ้า enum ไม่ใช่ Flags** |
| CS-36 | 1041-1136 | Medium | Design | Magic numbers ~30+ ตัวใน scoring |
| CS-37 | 1262 | Low | Logic | `selfMonsters >= 5` → score=0 แต่ยัง compute ต่อ |
| CS-38 | 1270 | Medium | Hardcoded | Decision threshold 35.0 |
| CS-39 | **1327** | **High** | **Logic** | **HasStarterOrExtenderInHand ไม่เช็ค "payoff"** |
| CS-40 | 1542 | Low | Hardcoded | MonsterZone size 7 |
| CS-41 | 1553 | Low | Hardcoded | SpellZone size 8 |
| CS-42 | 1572 | Medium | Bug Risk | `Duel.Fields[0].Banished` — null check missing |
| CS-43 | 1791-1798 | High | Safety | Destructor accesses managed objects — unsafe |

### Python Sandbox (25 issues)

| ID | ไฟล์ | บรรทัด | Severity | คำอธิบาย |
|----|------|--------|----------|----------|
| PY-01 | cockpit.py | 82-97 | Low | `forward_bot_logs()` — never called |
| PY-02 | cockpit.py | 130-133 | Low | Race condition daemon thread vs process exit |
| PY-03 | cockpit.py | 397 | Low | `/api/progress` อ่านไฟล์ซ้ำทุก request |
| PY-04 | learning_sandbox.py | 4 | Low | `import glob` — never used |
| PY-05 | learning_sandbox.py | 210-228 | High | Bait_value inflation blunt tool |
| PY-06 | learning_sandbox.py | 235-237 | Medium | Bait decay + boost oscillation |
| PY-07 | learning_sandbox.py | 240-251 | Medium | Bootstrap O(cards×matches×decisions) |
| PY-08 | auto_role_detector.py | 4 | Low | `import re` — never used |
| PY-09 | auto_role_detector.py | 81 | Medium | `"normal summoned"` misses present tense |
| PY-10 | auto_role_detector.py | 98 | High | `"send to the gy"` misses "Graveyard" |
| PY-11 | auto_role_detector.py | 106 | Medium | `"draw" in desc` matches draw restrictions |
| PY-12 | optimize_registry.py | 2 | Low | `import json` — never used |
| PY-13 | optimize_registry.py | 113-114 | Medium | `--deck all` cross-deck mutation interference |
| PY-14 | optimize_registry.py | 127 | Medium | Equal mutation prob (risk_if_negated irrelevant) |
| PY-15 | ab_tournament.py | **203-215** | **CRITICAL** | **C# class name ขึ้นต้นตัวเลข → compile fail** |
| PY-16 | ab_tournament.py | 268-269 | Medium | Race condition: 1s sleep → false "no logs" |
| PY-17 | ab_tournament.py | **296** | **High** | **avg_turns คำนวณผิด** |
| PY-18 | combo_simulator.py | 66-104 | High | Brick/starter hit inflation |
| PY-19 | combo_simulator.py | **154** | **High** | **rescue > 500 ไม่ scale ตาม num_simulations** |
| PY-20 | combo_simulator.py | 245-249 | High | `--optimize` saves to live without confirmation |
| PY-21 | run_match_learning.py | 22-23 | High | Step 1 no `--deck` → cross-deck contamination |
| PY-22 | run_match_learning.py | 25 | Medium | Step 1 fail → silent continue |
| PY-23 | run_match_learning.py | 35 | Medium | Exit code จาก Step 2 เท่านั้น |
| PY-24 | shared_utils.py | 59-75 | Medium | Auto-role-detector runs once only |
| PY-25 | shared_utils.py | 66-72 | Medium | Auto-role-detector errors suppressed (DEVNULL) |

### Config Issues (4 issues)

| ID | ไฟล์ | บรรทัด | Severity | คำอธิบาย |
|----|------|--------|----------|----------|
| CF-01 | bots.json | 456 | **CRITICAL** | Goldlord — missing difficulty + masterRules |
| CF-02 | bots.json | 462 | **CRITICAL** | Invoke — missing difficulty + masterRules |
| CF-03 | bots.json | 468 | **CRITICAL** | Kwtune — missing difficulty + masterRules |
| CF-04 | bots.json | 474 | **CRITICAL** | Labrynth — missing difficulty + masterRules |

### Total: 72 issues (7 Critical, 11 High, 20 Medium, 34 Low)

---

## End of Document
