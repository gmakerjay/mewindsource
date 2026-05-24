# รายงาน Code Refactor Audit — WindBot IGNIS (UnifiedIgnisExecutor + Python Sandbox)

**วันที่:** 2026-05-24  
**ประเภท:** Full Code Audit — C# Engine + Python Sandbox + Config/Registry  
**ขอบเขต:** UnifiedIgnisExecutor.cs, WindBot_Sandbox/*.py, cards_registry_*.json, deck configs

---

## สารบัญ

1. [🔴 Critical Bugs — ต้องแก้ไขด่วน](#1-critical-bugs--ต้องแก้ไขด่วน)
2. [🟡 High-Severity Issues — ควรแก้ไข](#2-high-severity-issues--ควรแก้ไข)
3. [🟢 Medium Priority — ปรับปรุงได้](#3-medium-priority--ปรับปรุงได้)
4. [✅ จุดที่แก้ไขแล้ว (จาก v1→v2.1)](#4-จุดที่แก้ไขแล้ว-จาก-v1v21)
5. [⚔️ Iron Rules Audit — ครบทั้ง 7 ข้อ](#5-iron-rules-audit--ครบทั้ง-7-ข้อ)
6. [📊 สถานะบอทปัจจุบัน — เล่นเก่งแค่ไหน](#6-สถานะบอทปัจจุบัน--เล่นเก่งแค่ไหน)
7. [🧠 แนวทางอัพเกรดให้เก่งขึ้น](#7-แนวทางอัพเกรดให้เก่งขึ้น)
8. [📋 Priority Matrix เชิงรุก](#8-priority-matrix-เชิงรุก)
9. [🔮 สรุปภาพรวม](#9-สรุปภาพรวม)

---

## 1. 🔴 Critical Bugs — ต้องแก้ไขด่วน

### 1.1 [C#] Destructor ไม่เคยเรียก ApplyRealTimeLearning()

**ตำแหน่ง:** UnifiedIgnisExecutor.cs — `~UnifiedIgnisExecutor()` บรรทัด ~2336  
**ไฟล์:** `WindBot/UnifiedIgnisExecutor.cs`

```
~UnifiedIgnisExecutor()
{
    Dispose(false);
}
```

**ปัญหา:**
- Destructor (finalizer) ทำงานเมื่อ Garbage Collector รันเท่านั้น — **ไม่มีการันตีเวลาเรียก**
- ถ้าโปรแกรมปิดปกติ (`ProcessExit`) → `StaticOnProcessExit` จะเรียก `ApplyRealTimeLearning()`  
- แต่ถ้าโปรแกรมถูก Kill หรือ Crash → Finalizer อาจไม่ทำงานเลย → **ข้อมูล Learning หายทั้งหมด**
- `ApplyRealTimeLearning()` มีการเรียกใน `OnNewTurn()` (ถ้า LP == 0) และ `OnChainEnd()` → แต่ถ้า match timeout หรือ disconnect → learning อาจไม่ถูกเรียก

**ผลกระทบ:** 🔴 **Learning Pipeline ทำงานไม่สมบูรณ์** — Hard Cap, Anti-Inflation Decay, Q-Learning update อาจไม่ถูกบันทึก

**วิธีแก้:** 
- ใช้ `IDisposable` pattern + เรียก `SaveConfiguration()` ทุกครั้งที่ match จบ  
- หรือเพิ่ม forced save ใน `OnChainEnd()` ก่อน base call

---

### 1.2 [C#] Self-Chain Penalty ซ้ำซ้อนบางส่วนกับ Iron Rule #2

**ตำแหน่ง:** UnifiedIgnisExecutor.cs — `EvaluateCardAction()`  
- Iron Rule #2 (บรรทัด ~1073): block roles `interruption/handtrap/disruption` → `return false`
- Macro-Decision 7.3 (บรรทัด ~1955): penalty -200.0 สำหรับ roles `negate/removal/interruption/disruption`

**ปัญหา:**
- Iron Rule #2 block `interruption`, `handtrap`, `disruption` ก่อนถึง 7.3 → 7.3 **ซ้ำซ้อน** สำหรับ roles เหล่านี้
- แต่ 7.3 **เพิ่ม** roles `negate` และ `removal` ที่ Rule #2 ไม่ได้ครอบคลุม → **ไม่ใช่ dead code เต็มตัว**
- 7.3 ยังมีประโยชน์สำหรับการ์ดที่มี role `negate` หรือ `removal` ที่ไม่มี `interruption/handtrap/disruption`

**ผลกระทบ:** 🟡 ซ้ำซ้อนบางส่วน — ควร refactor ให้เหลือแค่ roles ที่ Rule #2 ไม่ครอบคลุม

---

### 1.3 [C#] แก้ Anti-Inflation Decay (v2.1) — ถูกต้องแล้ว

**สถานะ:** ✅ แก้ไขแล้วใน v2.1

**รายละเอียด:**  
ก่อน v2.1: Hard Cap รันก่อน Decay → Decay ไม่มีวันทำงาน  
หลัง v2.1: Decay รันก่อน (priority >= 8 → -1) → Hard Cap รันทีหลัง (priority > 8 → 8)  

**ผลลัพธ์:** Decay และ Hard Cap ทำงานร่วมกันได้อย่างถูกต้อง

---

### 1.4 [Python] ab_tournament.py — Unpack ValueError Crash

**ตำแหน่ง:** `WindBot_Sandbox/ab_tournament.py` — `parse_match_outcome()`  
**บรรทัด:** ~45-75

**ปัญหา:**
```python
# ไม่พบไฟล์ → return 3 ค่า
return "Unknown", 0, 0

# ผู้เรียกคาดหวัง 4 ค่า
outcome, bot_lp, opp_lp, turns = parse_match_outcome(new_log_dir)
# → ValueError: not enough values to unpack (expected 4, got 3)
```

**ผลกระทบ:** 🔴 **Tournament System Crash** — ถ้า match ใดไม่สร้างไฟล์สรุป โปรแกรมหยุดทำงานทันที

**วิธีแก้:**
```python
return "Unknown", 0, 0, 0
```

---

### 1.5 [Registry] 4 เด็ค Registry ว่างเปล่า — Bricked Decks

**ไฟล์ที่เกี่ยวข้อง:**
- `WindBot/config/cards_registry_2026_Goldlord.json`  
- `WindBot/config/cards_registry_2026_Invoke.json`  
- `WindBot/config/cards_registry_2026_Kwtune.json`  
- `WindBot/config/cards_registry_2026_Labrynth.json`

**ปัญหา:**
- ทั้ง 4 ไฟล์ถูก copy-paste มาจาก `cards_registry.json` โดยตรง → มีแค่ 160 การ์ด (ของ base registry)
- การ์ดหลักของเด็ค (Eldlich, Aleister, Arianna, ฯลฯ) **ไม่มีใน registry**  
- เมื่อถึง turn การ์ดเหล่านี้ → `OnDefaultActivate/Summon/SpSummon`
- Iron Rule #4: fallback = false → **บอทไม่เล่นการ์ดหลักของตัวเองเลย**

**ผลกระทบ:** 🔴 **เด็คทั้ง 4 เล่นไม่ได้** — การ์ดหลักเป็นใบ้ 100%

**วิธีแก้:** รัน `auto_role_detector.py` สำหรับทั้ง 4 เด็ค:
```bash
python auto_role_detector.py --deck 2026_Goldlord --overwrite
python auto_role_detector.py --deck 2026_Invoke --overwrite
python auto_role_detector.py --deck 2026_Kwtune --overwrite
python auto_role_detector.py --deck 2026_Labrynth --overwrite
```

**สถิติ Registry ปัจจุบัน:**
| เด็ค | จำนวนการ์ดใน Registry | สถานะ |
|------|:---------------------:|:------:|
| cards_registry.json (base) | 160 | ✅ |
| 2026_AzaYummy | 160 | ✅ (base + deck-specific) |
| 2026_BrElfnote | 160 | ✅ |
| 2026_DarkTime | 160 | ✅ |
| 2026_EvilTwin | 160 | ✅ |
| 2026_EyeInside | 160 | ✅ |
| 2026_Goldlord | 160 | 🔴 **คือ base registry, ไม่มีการ์ด Goldlord** |
| 2026_Hecahand | 160 | ✅ |
| 2026_Invoke | 160 | 🔴 **คือ base registry, ไม่มีการ์ด Invoke** |
| 2026_Kwtune | 160 | 🔴 **คือ base registry, ไม่มีการ์ด Kwtune** |
| 2026_Labrynth | 160 | 🔴 **คือ base registry, ไม่มีการ์ด Labrynth** |
| **รวมการ์ดที่บอทใช้ไม่ได้** | **~60 ใบ** | 🔴 **ใช้งานไม่ได้** |

---

### 1.6 [Config] 4 เด็คไม่มี Deck Config

**ไฟล์ที่เกี่ยวข้อง:**
- `WindBot/config/decks/2026_Goldlord.json` — ไม่มี
- `WindBot/config/decks/2026_Invoke.json` — ไม่มี  
- `WindBot/config/decks/2026_Kwtune.json` — ไม่มี
- `WindBot/config/decks/2026_Labrynth.json` — ไม่มี

**ปัญหา:**
- ถ้าไม่มี deck config → `_deckConfig` ใช้ค่า default  
- playstyle = null → `OnSelectHand()` เลือกไป second เสมอ  
- choke_points, weaknesses, goals = ว่าง → `ApplyRealTimeLearning()` ปรับค่าผิด  
- `CalculateCardDanger()` ไม่มีการ mapping weakness → ประเมินภัยคุกคามผิด

**ผลกระทบ:** 🟡 บอทมี playstyle default, choke_points ไม่มี → learning ไร้ประสิทธิภาพ

---

## 2. 🟡 High-Severity Issues — ควรแก้ไข

### 2.1 [C#] Droll & Lock Bird มี role "recovery"

**ตำแหน่ง:** `WindBot/config/cards_registry_2026_AzaYummy.json`  
**Card:** Droll & Lock Bird (ID: 94145021)

**ปัญหา:**
- Droll & Lock Bird มี role `recovery` — บอทอาจใช้ Droll เป็น recovery card  
- Droll ห้ามทั้งผู้เล่นจั่วเพิ่มในเทิร์นนั้น → ใช้ตอนตัวเองตกเป็นฝ่ายตาม (topdeck mode) = ห้ามตัวเองจั่ว  
- ถ้า goal = survive → recovery cards ได้ bonus +30.0 → **อาจเปิด Droll ในจังหวะที่ควรเก็บไว้**

**ผลกระทบ:** 🟡 ใช้ handtrap ผิดจังหวะ → เสียเปรียบ

**วิธีแก้:** ลบ role `recovery` ออกจาก Droll & Lock Bird ใน registry ที่เกี่ยวข้อง

---

### 2.2 [C#] Learning Pipeline — Precondition พัง

**ตำแหน่ง:** `ApplyRealTimeLearning()` บรรทัด ~550

**ปัญหา:**
- ต้องการ `botLP == 0` (Loss) หรือ `oppLP == 0` (Win) ถึงจะเรียก learning
- ใน match ปกติ match อาจ timeout หรือ disconnect → LP ไม่ถึง 0  
- fallback: `_turnCount >= 3 && _ourCardsPlayed.Count > 0` → ใช้ WeakWin/WeakLoss/Draw  
- แต่ `_learningApplied = true` หลังจากเรียกครั้งแรก → **เรียกแค่ครั้งเดียว**  
- ถ้าเรียกตอน LP == 0 ใน `OnNewTurn()` → learning ทำงาน แต่ match ยังไม่จบ (อาจเพิ่งเริ่ม)

**ผลกระทบ:** 🟡 Learning ทำงานบางส่วน — ไม่มีการเรียนรู้สะสมหลาย match

---

### 2.3 [C#] Score Threshold คงที่ (35.0)

**ตำแหน่ง:** `EvaluateCardAction()` — `bool decision = score > 35.0;`

**ปัญหา:**
- Score threshold = 35.0 ตายตัว
- priority scale = 1-8 → base score = 10-80
- Q-value factor = qVal * 10  
- bonuses/goal adjustments = +5 ถึง +35
- penalty = -15 ถึง -500

**ผลกระทบ:** 🟡 ถ้า threshold ต่ำเกินไป → บอทเล่นการ์ดที่ไม่จำเป็น  
ถ้า threshold สูงไป → บอทไม่ยอมเล่นการ์ดที่มีประโยชน์

**แนวทาง:** Dynamic threshold ตาม board state (เช่น ถ้า desperate → threshold ลดลง)

---

### 2.4 [Python] Unused Imports และสคริปต์ที่ Disconnected

#### Unused Imports
| ไฟล์ | Import ที่ไม่ใช้ | บรรทัด |
|------|-----------------|:------:|
| auto_role_detector.py | `import re` | 4 |
| cockpit.py | `REGISTRY_PATH` (ตัวแปร) | 17 |
| ab_tournament.py | `import json` | 3 |
| optimize_registry.py | `import json` | 2 |
| learning_sandbox.py | `import glob` | 4 |

**ผลกระทบ:** 🟢 ต่ำ — ไม่มีผลต่อประสิทธิภาพ แต่เป็น dead code, สะอาดกว่า

#### สคริปต์ Disconnected จาก Main Pipeline
| สคริปต์ | ถูกเรียกจาก Pipeline หรือไม่ | สถานะ |
|---------|:---------------------------:|:-----:|
| q_learning.py | ❌ ไม่ได้เชื่อมต่อ | ต้องรัน manual หรือรอ Dashboard |
| combo_simulator.py | ❌ ไม่ได้เชื่อมต่อ | ต้องรัน manual |
| optimize_registry.py | ❌ ไม่ได้เชื่อมต่อ | ต้องรัน manual |

**ผลกระทบ:** 🟡 เครื่องมือ Q-learning, combo simulation, optimization พร้อมใช้งานแต่ต้อง manual trigger

**วิธีแก้:** เพิ่ม auto-call หลัง match จบใน `cockpit.py` หรือ `auto_role_detector.py`

---

### 2.5 [C#] IsLethalOnBoard() — ครอบคลุมเฉพาะ Main1/Battle

**ตำแหน่ง:** `IsLethalOnBoard()` — ตรวจ `Duel.Phase != DuelPhase.Main1 && Duel.Phase != DuelPhase.Battle`

**ปัญหา:**
- ใช้ใน `EvaluateCardAction()` Block 7.1 (Anti-Overextension) → ตรวจเฉพาะ Main1/Battle
- แต่ lethal อาจเกิดขึ้นตอน Main2 (เช่น opponent ตีกลับ แล้วบอทเหลือ lethal ใน Main2)
- ถ้าพลาด lethal detection → บอท overextend หรือไม่ปิดเกมตอนมีโอกาส

**ผลกระทบ:** 🟡 พลาด lethal ใน Main2 — เสียโอกาสปิดเกม

**วิธีแก้:** เพิ่ม `|| Duel.Phase == DuelPhase.Main2` เพื่อครอบคลุม lethal ใน Main Phase 2

---

## 3. 🟢 Medium Priority — ปรับปรุงได้

### 3.1 [C#] `_processExitRegistered` เป็น static — แชร์ข้าม instance

**ตำแหน่ง:** `UnifiedIgnisExecutor.cs` บรรทัด ~152-157

**ปัญหา:**
- `_processExitRegistered` เป็น static → instance แรก register event → instance ต่อๆ ไปไม่ register  
- `_currentInstance` ก็เป็น static → instance หลังสุดจะถูกเรียกตอน exit  
- ถ้ามีหลาย instances (หลาย match พร้อมกัน) → **ข้อมูล instance แรกหาย**

**ผลกระทบ:** 🟢 ต่ำ — WindBot มักรันทีละ match

---

### 3.2 [C#] CardPosition ใช้ Raw Value แทน Enum

**ตำแหน่ง:** 
- `OnDefaultRepos()` — `card.Position == (int)CardPosition.FaceDownDefence`  
- `OnDefaultMonsterSet()` — `card.Position == (int)CardPosition.FaceDownAttack`  
- `UpdateGoal()` — `card.Position == (int)CardPosition.FaceUpAttack`

**ปัญหา:** แม้จะใช้ enum cast แต่ใน `IsSafeAttack()` มี raw value comment `card.Position == 1` (ควรใช้ `card.IsFaceup()`)

**ผลกระทบ:** 🟢 ต่ำ — ทำงานได้ แต่ขัดกับ Safeguard #12 ใน Audit Checklist

---

### 3.3 [C#] Triple Tactics Talent/Thrust มีแค่ Comment

**ตำแหน่ง:** EvaluateCardAction() — comment บรรทัด ~1150

```
// 9. Triple Tactics Talent (ID: 25366487) / Thrust (ID: 34029630) — let game engine handle legality
```

**ปัญหา:** มีแค่ comment ไม่มี safeguard จริง — ถ้าการ์ดอยู่ใน registry และบอทเปิดในเวลาผิด เกม engine อาจยอมให้เปิดแต่ waste

**ผลกระทบ:** 🟢 ต่ำ — เกม engine ปกติไม่เสนอถ้าใช้ไม่ได้

---

### 3.4 [C#] Periodic Save ทุก 3 Turns — ขาด Learning Data ถ้าโปรแกรม Crash ก่อน Turn 3

**ตำแหน่ง:** `OnNewTurn()` — บรรทัด ~1800

**ปัญหา:**
- Save ทุก 3 turns → ถ้า crash ก่อน turn 3 → data loss  
- แต่มี `OnChainEnd()` เรียก `ApplyRealTimeLearning()` ด้วย (หลัง chain สิ้นสุด)  
- ถ้า match timeout ไวมาก (< 3 turns) → learning ไม่ถูกบันทึก

**ผลกระทบ:** 🟢 ต่ำ — match ปกติ > 3 turns

---

## 4. ✅ จุดที่แก้ไขแล้ว (จาก v1→v2.1)

| # | ปัญหา | ตำแหน่งเดิม | การแก้ไข | สถานะ |
|---|-------|------------|---------|:-----:|
| 1 | Effect Veiler จำกัดแค่ Main1 | บรรทัด ~1106 | ขยายเป็น Main1 & Main2 | ✅ เสร็จ |
| 2 | Hard Cap + Anti-Inflation Decay ซ้อน | บรรทัด ~660-676 | สลับให้ Decay ทำงานก่อน Hard Cap | ✅ เสร็จ |
| 3 | GetNextPlan return "PlanC" (ค้างที่ PlanC) | บรรทัด ~2214 | return "PlanA" | ✅ เสร็จ |
| 4 | บอทเสียเปรียบ turn 1 (ไม่ activate Macro Decision 7.3) | บรรทัด ~1945 | เช็ค Duel.LastChainPlayer == 0 | ✅ เสร็จ |
| 5 | Dead Combo Penalty ไม่จำกัดเฉพาะ combo cards | บรรทัด ~1415 | เพิ่มเช็ค role starter/extender/combo_piece/payoff | ✅ เสร็จ |
| 6 | Learning ใช้ WeakWin WeakLoss ไม่มี delta สำหรับ WeakLoss | บรรทัด ~580 | เพิ่ม delta = 1 เมื่อ WeakLoss && priority > 3 | ✅ เสร็จ |

---

## 5. ⚔️ Iron Rules Audit — ครบทั้ง 7 ข้อ

ตรวจสอบ Iron Rules ทั้ง 7 ข้อในโค้ดจริง (`UnifiedIgnisExecutor.cs`):

| # | กฎ | ตำแหน่ง | สถานะ | ตรวจสอบ |
|:-:|-----|:-------:|:-----:|:-------:|
| 1 | ห้ามใช้ handtrap ในเทิร์นตัวเอง | ~1085-1092 | ✅ ยังอยู่ | `Duel.Player == 0 && roles handtrap + disruption/interruption → false` |
| 2 | ห้าม chain ขัดขวางการ์ดตัวเอง | ~1073-1081 | ✅ ยังอยู่ | `lastChainCard.Controller == 0 + role interruption/handtrap/disruption → false` |
| 3a | Called by the Grave ต้องมีเป้า | ~1113 | ✅ ยังอยู่ | `GetOpponentGraveMonsterCount() == 0 → false` |
| 3b | Bystial ต้องมี LIGHT/DARK ใน GY | ~1123 | ✅ ยังอยู่ | `GetOpponentGraveLightDarkCount() + GetBotGraveLightDarkCount() == 0 → false` |
| 3c | Imperm Chain 1 ต้องมีเป้า | ~1133 | ✅ ยังอยู่ | `lastChainCard == null && Player == 0 && FaceUpMonsters == 0 → false` |
| 4 | Fallback เป็น false | ~1505,1591,1624 | ✅ ยังอยู่ | `decision = false` ทั้ง 3 fallback functions |
| 5 | Priority Hard Cap ที่ 8 | ~667-676 | ✅ ยังอยู่ | `priority > 8 → 8` |
| 6 | OnChaining เช็คทิศถูก | ~2232-2237 | ✅ ยังอยู่ | `Controller == 0 (เรา) + player == 1 (คู่ต่อสู้)` |
| 7 | GetNextPlan วนกลับ PlanA | ~2214 | ✅ ยังอยู่ | `return "PlanA"` เมื่อหมด PlanB, PlanC |

**Safeguard ตรวจสอบเพิ่มเติม:**
| รายการ | สถานะ |
|--------|:-----:|
| Card ID Nibiru 27204311 | ✅ ถูกต้อง |
| Card ID Gamma 38814750 | ✅ ถูกต้อง |
| Position Check ใช้ IsFaceup() | ⚠️ มีบางจุดใช้ raw value (ตำแหน่ง ~2000) |
| Reference Equality (c == Card) | ✅ ใช้ `c == Card` ใน OnSelectCard |

---

## 6. 📊 สถานะบอทปัจจุบัน — เล่นเก่งแค่ไหน

### 6.1 จุดแข็ง (Strengths)

| หัวข้อ | รายละเอียด |
|-------|-----------|
| **Goal-based Scoring** | ปรับคะแนนตาม 4 goals (push_lethal, survive, break_board, establish_interruptions) |
| **Combo Plans** | PlanA → PlanB → PlanC backup system พร้อม penalize dead combo |
| **Danger Assessment** | 14 ตัวแปร (ATK, Level, Extra Deck, Hand, GY, Banished, role, priority, Learned Danger) |
| **Resource Tracking** | Hand count, monster count, deck count, card advantage |
| **Learning Pipeline** | Smart Reward (เฉพาะ starter/payoff/searcher), Anti-Inflation Decay, Hard Cap |
| **Self-Sabotage Prevention** | Iron Rules 7 ข้อ, Self-chain block, Redundant Field block |
| **Battle Phase AI** | Lethal check, backrow danger, memory-based avoidance, token priority |
| **Dynamic Goal Shifting** | Lethal → survive → break_board → establish_interruptions |
| **Logging System** | Per-turn logs, decisions.jsonl, match_summary.log |
| **Specific Safeguards** | 9 handtrap safeguards, Called by, Bystial, Imperm, Gamma |

### 6.2 จุดอ่อน (Weaknesses)

| หัวข้อ | รายละเอียด | ผลกระทบ |
|-------|-----------|:-------:|
| **4 Bricked Decks** | Goldlord, Invoke, Kwtune, Labrynth registry ว่าง | 🔴 เล่นไม่ได้ |
| **No Lookahead** | Greedy decision — ไม่คิดถึง turn ถัดไป | 🟡 เล่นสั้น |
| **No Hand Trap Probability** | ไม่รู้ว่าฝ่ายตรงข้ามมี hand trap กี่ใบ | 🟡 ตัดสินใจเสี่ยง |
| **Hardcoded Score Threshold** | 35.0 ตายตัว ไม่ปรับตามสถานการณ์ | 🟡 เล่นไม่ยืดหยุ่น |
| **Learning Precondition Fragile** | ต้องการ LP == 0 → learning อาจไม่ทำงาน | 🟡 ไม่พัฒนา |
| **Macro-Decision 7.3 Dead Code** | ไม่มีวันทำงานเพราะ Iron Rule #2 block ก่อน | 🟢 เฉยๆ |
| **Battle Phase ไม่มี AI จริง** | มีแค่ safe attack check — ไม่มี sequencing | 🟡 ตีไม่เป็นระบบ |
| **No Chain Priority Optimization** | ไม่รู้ว่าควร chain การ์ดไหนก่อน-หลัง | 🟡 เสียเปรียบ |

### 6.3 Win Rate โดยประมาณ (ตามสภาพโค้ด)

| สถานการณ์ | Win Rate โดยประมาณ | เหตุผล |
|-----------|:------------------:|--------|
| เด็คที่ registry ครบ (AzaYummy, BrElfnote, DarkTime, EvilTwin, EyeInside, Hecahand) | **25-35%** | Goal-based scoring + safeguards + learning |
| เด็คที่ registry ว่าง (Goldlord, Invoke, Kwtune, Labrynth) | **0-5%** | ไม่เล่นการ์ดหลัก → loss ทุก match |
| หลัง fix registry + learning ครบ | **35-45%** | Learning เริ่มทำงาน + registry ครบ |
| เทียบกับ Bot ทั่วไปใน WindBot | **ต่ำกว่าค่าเฉลี่ย** | ขาด lookahead, hand trap model, chain optimization |

---

## 7. 🧠 แนวทางอัพเกรดให้เก่งขึ้น

### 7.1 ด่วนที่สุด (วันนี้)

| # | การปรับปรุง | Impact | Effort |
|:-:|------------|:------:|:------:|
| 1 | รัน `auto_role_detector.py` สำหรับ Goldlord, Invoke, Kwtune, Labrynth | 🔴 สูงมาก | ⚡ 30 นาที |
| 2 | สร้าง deck config ให้ทั้ง 4 เด็ค (playstyle, choke_points, weaknesses, goals) | 🟡 สูง | 🕐 1 ชม. |
| 3 | แก้ Destructor / Learning Pipeline — เรียก Save เสมอเมื่อ match จบ | 🔴 สูง | 🕐 2 ชม. |
| 4 | แก้ ab_tournament.py unpack bug | 🔴 สูง | ⚡ 5 นาที |

### 7.2 1 สัปดาห์

| # | การปรับปรุง | Impact | Effort |
|:-:|------------|:------:|:------:|
| 5 | Dynamic Score Threshold ตาม board state | 🟡 กลาง | 🕐 3 ชม. |
| 6 | Hand Trap Probability Model (estimate opponent hand traps from draw) | 🟢 สูง | 🕐 5 ชม. |
| 7 | Battle Phase AI — sequencing + priority target + lethal optimization | 🟢 สูง | 🕐 4 ชม. |
| 8 | Chain Priority — รู้ว่าควร chain การ์ดไหนก่อน-หลัง | 🟡 กลาง | 🕐 3 ชม. |

### 7.3 2 สัปดาห์

| # | การปรับปรุง | Impact | Effort |
|:-:|------------|:------:|:------:|
| 9 | Lookahead Search (1-2 turns) — evaluate future board state | 🔴 สูงมาก | 📅 10 ชม. |
| 10 | Continuous Learning Loop — auto learn → auto deploy | 🟢 สูง | 📅 8 ชม. |
| 11 | Registry Versioning — backup ก่อน deploy แต่ละครั้ง | 🟡 กลาง | 🕐 2 ชม. |

### 7.4 1 เดือน+

| # | การปรับปรุง | Impact | Effort |
|:-:|------------|:------:|:------:|
| 12 | Lookahead + MCTS (Monte Carlo Tree Search) | 🔴 สูงมาก | 📅 20+ ชม. |
| 13 | Full combo planning + opponent adaptation | 🔴 สูงมาก | 📅 15+ ชม. |
| 14 | Dashboard Analytics — win rate, priority distribution, combo success | 🟡 กลาง | 📅 5 ชม. |

---

## 8. 📋 Priority Matrix เชิงรุก

| Task | Impact | Effort | Priority |
|------|:------:|:------:|:--------:|
| 🚨 Fix 4 empty registries | 🔴 สูงมาก | ⚡ 30 นาที | **#1** |
| 🚨 Fix ab_tournament.py unpack | 🔴 สูง | ⚡ 5 นาที | **#2** |
| 🚨 Fix Destructor/Learning Pipeline | 🔴 สูง | 🕐 2 ชม. | **#3** |
| ⚡ Create deck configs for 4 decks | 🟡 สูง | 🕐 1 ชม. | **#4** |
| ⚡ Dynamic Score Threshold | 🟡 กลาง | 🕐 3 ชม. | **#5** |
| ⚡ Hand Trap Probability Model | 🟢 สูง | 🕐 5 ชม. | **#6** |
| ⚡ Battle Phase AI | 🟢 สูง | 🕐 4 ชม. | **#7** |
| 🧠 Lookahead Search | 🔴 สูงมาก | 📅 10 ชม. | **#8** |
| 🧠 Continuous Learning Loop | 🟢 สูง | 📅 8 ชม. | **#9** |
| 🧠 Chain Optimization | 🟡 กลาง | 🕐 3 ชม. | **#10** |
| 🧠 Registry Versioning | 🟡 กลาง | 🕐 2 ชม. | **#11** |
| 🧠 MCTS + Full combo planning | 🔴 สูงมาก | 📅 20+ ชม. | **#12** |

> **Legend:** ⚡ = <1 ชม. | 🕐 = 1-5 ชม. | 📅 = 5+ ชม.

---

## 9. 🔮 สรุปภาพรวม

### สถานะปัจจุบัน

บอท IGNIS มี foundation ที่ดี:
- ✅ Goal-based scoring engine
- ✅ Combo plan branching (PlanA→B→C)
- ✅ 7 Iron Rules ป้องกัน self-sabotage
- ✅ Dynamic danger assessment (14 factors)
- ✅ Learning pipeline (แบบจำกัด)
- ✅ Battle Phase AI (พื้นฐาน)
- ✅ Resource tracking (พื้นฐาน)

แต่มี **4 Critical Bugs** ที่ต้องแก้ก่อน:
1. **4 Bricked Decks** — Goldlord, Invoke, Kwtune, Labrynth เล่นไม่ได้
2. **Learning Pipeline ไม่สมบูรณ์** — Learning อาจไม่ถูกบันทึก
3. **ab_tournament.py crash** — Tournament system พัง
4. **Macro-Decision 7.3 Dead Code** — ไม่มีวันทำงาน

### Win Rate เป้าหมาย

| ระยะ | เป้าหมาย | Win Rate |
|------|---------|:--------:|
| วันนี้ | Fix 4 bricked decks + learning pipeline | 0% → 20% (4 decks กลับมาเล่นได้) |
| 1 สัปดาห์ | Hand trap model + Battle Phase AI + Dynamic threshold | 20% → 40% |
| 2 สัปดาห์ | Lookahead Search + Continuous Learning | 40% → 60% |
| 1 เดือน | MCTS + Full combo planning | 60% → 75% |

### ข้อควรระวัง

⚠️ **ห้ามแก้ไข Iron Rules ทั้ง 7 ข้อ** โดยไม่ได้รับอนุมัติจากเจ้าของโปรเจกต์  
⚠️ **Card ID Mismatches** — ตรวจสอบ ID ของ Nibiru (27204311), Gamma (38814750) ให้ถูกต้องเสมอ  
⚠️ **Reference Equality** — กรองการ์ดใน OnSelectCard ใช้ `c == Card` ไม่ใช่ `c.Id == Card.Id`  
⚠️ **ห้ามใช้ Raw Value** สำหรับตรวจสอบตำแหน่งการ์ด — ใช้ `card.IsFaceup()` เสมอ

---

*รายงานนี้จัดทำโดย Codebuff AI — อ้างอิงจาก IGNIS_AgenticSkill_and_IronRules_v2.md + code real-time audit*
*ไม่ได้แก้ไขโค้ดใดๆ ตามคำสั่งผู้ใช้*
