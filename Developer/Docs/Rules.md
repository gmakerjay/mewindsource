# WindBot IGNIS — Agentic Skill & Iron Rules v3
> เอกสารนี้ใช้แนบทุกครั้งที่สั่ง AI แก้ไขโค้ด `UnifiedIgnisExecutor.cs`
> อัปเดต: 2026-05-24 | **v3.0 — ตรงกับโค้ดจริง (3,013 บรรทัด)**

---

Folder BrainStorms คือที่เก็บ Analysis ทุกครั้งที่ผมสั่งอะไรให้สกัดออกมาเป็นไฟล์ `.md` + ลงชื่อแล้วประทับเวลาด้วย
หากผมสั่งให้ **Refactor** หรือ **Review** นั่นหมายความว่าต้องวิเคราะห์ Code Trace อย่างละเอียดทุกซอกทุกมุมเพื่อหาแนวทางพัฒนาบอทให้เก่งขึ้น และบันทึกผลลงไฟล์ทุกครั้ง

---

## PART 1 — AGENTIC SKILL (คัดลอกแนบใน Prompt ทุกครั้ง)

```
You are a surgical C# code editor for the WindBot IGNIS project.
Your role is to fix ONLY what is explicitly requested — nothing more.

=== PROJECT CONTEXT ===
File: UnifiedIgnisExecutor.cs (3,013 lines)
Namespace: ProjectIgnisAI
Class: UnifiedIgnisExecutor : DefaultExecutor, IDisposable
Language: C# (.NET Framework, WindBot game AI)
Config: cards_registry_{deck}.json, opponent_memory.json, config/decks/{deck}.json

Key methods and their ACTUAL line numbers (v3.0):
- LoadConfiguration()       → loads registry, names, deck config, opponent memory (line 279)
- SaveConfiguration()       → process-safe delta merge to disk (line 511)
- ApplyRealTimeLearning()   → post-match weight adjustment, calls SaveConfiguration (line 737)
- EvaluateCardAction()      → core scoring engine, returns bool (line 1509)
- OnCardAction()            → routes registered card actions (line 1960)
- OnDefaultActivate()       → fallback for unregistered card activations (line 1982)
- OnDefaultSummon()         → fallback for unregistered normal summons (line 2034)
- OnDefaultSpSummon()       → fallback for unregistered special summons (line 2091)
- OnDefaultSpellSet()       → fallback for spell/trap setting (line 2125)
- OnDefaultMonsterSet()     → fallback for monster setting face-down (line 2181)
- OnDefaultRepos()          → reposition logic (line 2152)
- UpdateGoal()              → sets _currentGoal based on board state (called in OnNewTurn)
- GetNextPlan()             → combo plan branching PlanA→B→C→A (line 2801)
- OnChaining()              → tracks opponent disruptions & Impermanence columns (line 2808)
- OnChainEnd()              → triggers learning when LP hits 0 (line 2891)
- OnNewTurn()               → resets plan/columns, periodic save every 3 turns (line 2287)
- OnBattle()                → battle phase decision engine (line 2421)
- OnSelectCard()            → card selection for costs/targets (line 2624)
- OnSelectPlace()           → spell/trap zone placement avoiding Impermanence (line 2753)
- IsLethalOnBoard()         → checks if bot can win this turn (line 80)
- CheckSpellTrapWillBeNegated() → Naturia Beast / Impermanence column check (line 1392)
- CalculateCardDanger()     → danger score for opponent cards (line ~1100)

=== SURGICAL EDIT RULES ===
1. Fix ONLY the lines or method explicitly named in the request.
2. Do NOT refactor, rename, reorder, or restructure any surrounding code.
3. Do NOT add new using statements unless explicitly told to.
4. Do NOT change method signatures.
5. Do NOT remove existing Log(), LogToTurn(), or LogToMatch() calls.
6. Do NOT alter any method not mentioned in the request.
7. If the fix requires touching more than 20 lines, STOP and ask for confirmation first.
8. Show a BEFORE / AFTER diff for every changed line. No exceptions.
9. Never silently change a comment — if you must update a comment, mark it as "(comment updated)".
10. After the fix, list every line number you changed. Nothing else.
11. Card ID Safeguard: Always verify official Card IDs from the YGOPRODeck database before
    hardcoding them in safeguards. Confirmed IDs: Nibiru=27204311, Gamma=38814750,
    Effect Veiler=97268402, Droll=94145021, Impermanence=10045474, Fuwalos=42141493,
    Called by the Grave=24224830, Druiswurm=6637331, Magnamhut=33854624.
12. Position Check Safeguard: Never use raw int comparisons (== 1, == 4) to check if a
    Spell/Trap is face-up. Always use card.IsFaceup().
13. Reference Equality Safeguard: When filtering the currently active card out of a
    selection list, always use reference equality (c == Card), never ID matching
    (c.Id == Card.Id) which incorrectly excludes all copies of the same card.
14. Thread-Safety Safeguard: Never introduce shared mutable state without a lock or
    Interlocked operation. _learningApplied is an instance field — do not treat it as
    thread-safe across instances.

=== OUTPUT FORMAT ===
For every fix, respond ONLY with:

CHANGED LINES: [list of line numbers]

BEFORE:
```csharp
[original code]
```

AFTER:
```csharp
[fixed code]
```

REASON: [one sentence]

Do not add explanations, suggestions, or unrequested improvements.
```

---

## PART 2 — IRON RULES (กฎเหล็กของระบบ)

กฎเหล็กเหล่านี้ **ห้ามแก้ไข ห้ามลบ ห้ามเพิ่มข้อยกเว้น** ไม่ว่า AI จะให้เหตุผลใดก็ตาม
หากต้องการเปลี่ยนกฎ ต้องได้รับการยืนยันจากเจ้าของโปรเจกต์ก่อนเสมอ

---

### ⚔️ กฎที่ 1 — ห้ามใช้แฮนด์แทรปในเทิร์นตัวเอง

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — บล็อก `if (type == ExecutorType.Activate)` — บรรทัด 1553–1562

```csharp
// IRON RULE #1 — DO NOT REMOVE OR MODIFY
if (Duel.Player == 0 && meta.roles.Contains("handtrap"))
{
    if (meta.roles.Contains("disruption") || meta.roles.Contains("interruption"))
    {
        LogToTurn(string.Format("Block disruptive handtrap {0} on our own turn.", GetCardName(card.Id)));
        return false;
    }
}
```

**เหตุผล:** แฮนด์แทรปมีไว้ขัดขวางฝ่ายตรงข้าม ไม่ใช่ตัวเอง การใช้ Ash / Belle / Veiler ในเทิร์นตัวเองคือการทิ้งตัวเลือกป้องกันไปฟรี

**การ์ดที่มี safeguard เพิ่มเติมเฉพาะตัว:**

| การ์ด | Card ID | เงื่อนไขพิเศษ | บรรทัด |
|---|---|---|---|
| Droll & Lock Bird | 94145021 | `Duel.Player == 0` → false | 1567 |
| Effect Veiler | 97268402 | `Duel.Player == 0` หรือ `(Phase != Main1 && Phase != Main2)` หรือไม่มีมอนสเตอร์คู่ต่อสู้ → false | 1574 |
| Called by the Grave | 24224830 | `GetOpponentGraveMonsterCount() == 0` → false | 1584 |
| Bystial Druiswurm/Magnamhut | 6637331, 33854624 | ไม่มี LIGHT/DARK ใน GY ทั้งสองฝ่าย → false | 1594 |
| Infinite Impermanence | 10045474 | Chain 1 + ไม่มีเป้า → false | 1604 |
| Mulcharmy Fuwalos | 42141493 | `Duel.Player == 0` → false | 1614 |
| Nibiru, the Primal Being | 27204311 | `Duel.Player == 0` → false | 1622 |
| PSY-Framegear Gamma | 38814750 | `ourMonCount > 0` (ควบคุมมอนสเตอร์อยู่) → false | 1632 |

---

### ⚔️ กฎที่ 2 — ห้าม Chain ขัดขวางการ์ดตัวเอง

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — บล็อก Activate — บรรทัด 1543–1551

```csharp
// IRON RULE #2 — DO NOT REMOVE OR MODIFY
if (lastChainCard != null && lastChainCard.Controller == 0)
{
    if (meta.roles.Contains("interruption") || meta.roles.Contains("handtrap") ||
        meta.roles.Contains("disruption") || meta.roles.Contains("negate") || meta.roles.Contains("removal"))
    {
        LogToTurn(string.Format("Block chaining self-hurt: {0} (ID: {1}) responding to our own card: {2} (ID: {3})",
            GetCardName(card.Id), card.Id, GetCardName(lastChainCard.Id), lastChainCard.Id));
        return false;
    }
}
```

**เหตุผล:** บอทเคยโยน Ash Blossom / Baronne de Fleur ใส่เอฟเฟกต์ตัวเอง ทำให้ยกเลิกคอมโบตัวเองไปฟรี

---

### ⚔️ กฎที่ 3 — ห้ามเปิดการ์ดที่ต้องการเป้าหมายโดยไม่มีเป้า

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — specific card safeguards — บรรทัด 1584–1611

```csharp
// IRON RULE #3a — Called by the Grave
if (card.Id == 24224830)
{
    if (GetOpponentGraveMonsterCount() == 0) { return false; }
}

// IRON RULE #3b — Bystial Druiswurm & Magnamhut
if (card.Id == 6637331 || card.Id == 33854624)
{
    if (GetOpponentGraveLightDarkCount() + GetBotGraveLightDarkCount() == 0) { return false; }
}

// IRON RULE #3c — Infinite Impermanence (no target)
if (card.Id == 10045474)
{
    if (GetOpponentFaceUpMonsterCount() == 0) { return false; }
}
```

**เหตุผล:** เปิดการ์ดโดยไม่มีเป้าหมายที่ถูกต้องทำให้ game engine บังคับเลือกเป้าหมายผิดฝั่ง หรือใช้การ์ดไปฟรีโดยไม่มีผล

---

### ⚔️ กฎที่ 4 — Fallback ต้องเป็น false เสมอ

**ตำแหน่งในโค้ด:** `OnDefaultActivate()` (1994), `OnDefaultSummon()` (2080), `OnDefaultSpSummon()` (2113)

```csharp
// IRON RULE #4 — DO NOT CHANGE TO true
else
{
    decision = false; // Unknown card — safe default, do not play blindly
}
```

**เหตุผล:** การ์ดนอก registry ไม่ผ่าน safeguard ใดเลย fallback เป็น `true` หมายความว่าบอทจะเล่น Dark Hole / Raigeki โดยไม่มีการตรวจสอบ

---

### ⚔️ กฎที่ 5 — Priority Hard Cap ที่ 8

**ตำแหน่งในโค้ด:** `ApplyRealTimeLearning()` — หลัง Anti-Inflation Decay loop — บรรทัด 895–904

```csharp
// IRON RULE #5 — DO NOT REMOVE OR RAISE THE CAP
foreach (var kvpCap in _cardRegistry)
{
    if (kvpCap.Value.priority > 8)
    {
        LogToMatch(string.Format("  Hard Cap: Card {0} ({1}) priority capped from {2} to 8",
            kvpCap.Key, GetCardName(kvpCap.Key), kvpCap.Value.priority));
        kvpCap.Value.priority = 8;
    }
}
```

**เหตุผล:** Priority Inflation ทำให้ทุกการ์ดมีคะแนนเท่ากัน → scoring engine ไม่มีความหมาย Cap ที่ 8 เปิดช่องให้ Q-Value และ bonus modifier ยังทำงานได้

**⚠️ หมายเหตุ:** Anti-Inflation Decay (บรรทัด 849–862) ต้องรันก่อน Hard Cap เสมอ — ลำดับนี้ถูกต้องแล้ว ห้ามสลับ

---

### ⚔️ กฎที่ 6 — OnChaining ต้องเช็คทิศให้ถูก

**ตำแหน่งในโค้ด:** `OnChaining()` — บล็อกตรวจ disruption — บรรทัด 2840–2885

```csharp
// IRON RULE #6 — Controller == 0 (our card) and player == 1 (opponent chains) — DO NOT SWAP
if (lastChain != null && lastChain.Controller == 0)
{
    if (player == 1)
    {
        // record disruption relationship
    }
}
```

**เหตุผล:** ถ้าสลับค่าจะบันทึก disruption กลับด้าน ทำให้ `ApplyRealTimeLearning()` ปรับค่าผิดทิศในทุก match

---

### ⚔️ กฎที่ 7 — GetNextPlan ต้องวนกลับ PlanA

**ตำแหน่งในโค้ด:** `GetNextPlan()` — บรรทัด 2801–2805

```csharp
// IRON RULE #7 — return "PlanA" not "PlanC" — DO NOT CHANGE
private string GetNextPlan(string current)
{
    if (current == "PlanA") return "PlanB";
    if (current == "PlanB") return "PlanC";
    return "PlanA"; // Reset to PlanA when all plans exhausted
}
```

**เหตุผล:** ถ้า return "PlanC" บอทจะติดอยู่ที่ PlanC ตลอดเกม ไม่มีวันปรับแผนใหม่

> **หมายเหตุ (BUG 3):** GetNextPlan() วนกลับ PlanA โดยไม่ตรวจว่า PlanA ถูก block ไปแล้วหรือยัง — นี่คือ known design trade-off ที่ยอมรับได้ หากต้องการแก้ให้ผ่านการยืนยันก่อน

---

### ⚔️ กฎที่ 8 — _learningApplied ต้องเช็คก่อน SaveConfiguration เสมอ

**ตำแหน่งในโค้ด:** `ApplyRealTimeLearning()` — บรรทัด 737–748

```csharp
// IRON RULE #8 — DO NOT REMOVE THIS GUARD
if (_learningApplied) return;
// ...
_learningApplied = true;
```

**เหตุผล:** มี 4 จุดที่เรียก ApplyRealTimeLearning ได้ (OnNewTurn, OnChainEnd, Dispose, StaticOnProcessExit) — guard นี้ป้องกัน learning data ถูกนับซ้ำหรือ SaveConfiguration ถูกเรียกหลายรอบในเกมเดียว

---

## PART 3 — SCOPE BOUNDARY MAP

ใช้แผนผังนี้ตอบคำถามว่า "แก้จุดนี้ ควรแตะอะไรบ้าง"

```
ต้องการแก้                      ✅ แตะได้                    ❌ ห้ามแตะ
──────────────────────────────────────────────────────────────────────────
Safeguard การ์ดใบใหม่          EvaluateCardAction()         OnChaining()
                                                             ApplyRealTimeLearning()
                                                             scoring logic

เพิ่มการ์ดลง registry          cards_registry_{deck}.json   *.cs ทุกไฟล์

แก้ scoring weight              EvaluateCardAction()         Iron Rules ทั้ง 8 ข้อ
                                (บรรทัด score += ...)

แก้ Learning logic              ApplyRealTimeLearning()      EvaluateCardAction()
                                SaveConfiguration()           OnChaining()

แก้ combo plan                  GetNextPlan()                scoring logic
                                _currentPlan fields          Iron Rules

แก้ fallback behavior           OnDefaultActivate()          EvaluateCardAction()
                                OnDefaultSummon()             Iron Rules
                                OnDefaultSpSummon()

แก้ battle decision             OnBattle()                   EvaluateCardAction()
                                OnSelectAttackTarget()        Iron Rules

แก้ column avoidance            OnSelectPlace()              OnChaining()
                                _opponentNegatedColumns       Iron Rules

แก้ card selection cost         OnSelectCard()               Iron Rules
```

---

## PART 4 — AUDIT CHECKLIST

ใช้รายการนี้หลังให้ AI แก้โค้ดทุกครั้ง

- [ ] กฎที่ 1 ยังอยู่: `Duel.Player == 0 && meta.roles.Contains("handtrap")` → false (บรรทัด 1553)
- [ ] กฎที่ 2 ยังอยู่: `lastChainCard.Controller == 0` + roles → false (บรรทัด 1543)
- [ ] กฎที่ 3 ยังอยู่: Called by the Grave / Bystial / Imperm target check (บรรทัด 1584–1611)
- [ ] กฎที่ 4 ยังอยู่: fallback ทั้ง 3 ฟังก์ชันเป็น `false` (บรรทัด 1994, 2080, 2113)
- [ ] กฎที่ 5 ยังอยู่: Hard Cap loop `priority > 8 → 8` (บรรทัด 895–904)
- [ ] กฎที่ 6 ยังอยู่: `Controller == 0` และ `player == 1` ใน OnChaining (บรรทัด 2840–2885)
- [ ] กฎที่ 7 ยังอยู่: `GetNextPlan()` บรรทัดสุดท้าย return `"PlanA"` (บรรทัด 2805)
- [ ] กฎที่ 8 ยังอยู่: `if (_learningApplied) return;` เป็นบรรทัดแรกใน ApplyRealTimeLearning (บรรทัด 739)
- [ ] Card ID ถูกต้อง: Nibiru=27204311, Gamma=38814750, Imperm=10045474, Droll=94145021
- [ ] Position Check: ไม่ใช้ `== 1` หรือ `== 4` สำหรับ face-up check — ใช้ `card.IsFaceup()` เสมอ
- [ ] Reference Equality ใน OnSelectCard: ใช้ `c == Card` ไม่ใช่ `c.Id == Card.Id`
- [ ] Anti-Inflation Decay รันก่อน Hard Cap เสมอ (ลำดับ: Decay → Bait Decay → Hard Cap)
- [ ] ไม่มี method signature เปลี่ยน
- [ ] ไม่มี using statement ใหม่
- [ ] ไม่มี LogToTurn / LogToMatch ถูกลบ
- [ ] จำนวนบรรทัดรวมไม่ต่างจากเดิมเกิน 30 บรรทัด (ถ้าเกินให้ตรวจสอบ)

---

## APPENDIX A — Known Bugs & Issues

ปัญหาเหล่านี้พบระหว่าง Audit v2→v3 แบ่งตามความรุนแรง

### 🔴 Critical (ต้องแก้ก่อน)

| # | ปัญหา | ตำแหน่ง | ผลกระทบ | สถานะ |
|---|-------|---------|----------|-------|
| BUG-01 | ApplyRealTimeLearning() ถูกเรียกซ้ำในสภาพแวดล้อม multi-bot เพราะ `_learningApplied` ไม่ thread-safe | บรรทัด 739 | Learning data ถูกบันทึกซ้ำหรือ race condition | ยังไม่แก้ไข — ต้องใช้ `Interlocked.CompareExchange` |
| BUG-02 | `SaveConfiguration()` รันทุก 3 เทิร์น (periodic save) ก่อน learning จบ ทำให้ delta merge คำนวณผิด | บรรทัด 2312–2316 | Learning data สูญหายหรือเขียนทับด้วยค่าเก่า | ยังไม่แก้ไข — ควรข้าม periodic save ถ้า `_learningApplied == false` |

### 🟡 Logic Error

| # | ปัญหา | ตำแหน่ง | ผลกระทบ | สถานะ |
|---|-------|---------|----------|-------|
| BUG-03 | `GetNextPlan()` วนกลับ PlanA โดยไม่ตรวจว่า PlanA ถูก block ไปแล้วหรือยัง | บรรทัด 2801 | บอทอาจวนลูปกลับแผนที่โดน block แล้ว | Known trade-off — ยอมรับได้ หากต้องการแก้ให้ขออนุญาตก่อน |
| BUG-04 | `OnChainEnd()` ตรวจ `LP == 0` กลางการ resolve chain อาจเรียก learning ก่อนเกมจบสมบูรณ์ | บรรทัด 2891–2901 | Learning อาจรันขณะ game state ยังไม่ settled | ยังไม่แก้ไข |
| BUG-05 | Bait Decay ลดค่า `bait_value >= 6` ของทุกการ์ดทุกเกม ไม่สนว่าการ์ดนั้นถูกเล่นหรือเปล่า | บรรทัด 864–875 | bait_value ถดถอยเร็วเกินไป | ยังไม่แก้ไข |
| BUG-06 | `OnNewTurn()` reset `_currentPlan = "PlanA"` และ `_blockedPlans.Clear()` ทุกต้นเทิร์น | บรรทัด 2302–2304 | บอทลืมว่า PlanA/B โดน disrupt ไปแล้วในเทิร์นก่อน | Expected behavior — reset ต้นเทิร์นถือว่าถูกต้อง แต่ถ้า disruption เกิดใน End Phase ของฝ่ายตรงข้ามจะ miss |

### 🟣 Design Issue

| # | ปัญหา | ตำแหน่ง | ผลกระทบ | สถานะ |
|---|-------|---------|----------|-------|
| BUG-07 | `static _currentInstance` เก็บแค่ instance ล่าสุด — `StaticOnProcessExit` บันทึกเฉพาะบอทตัวสุดท้ายใน multi-bot run | บรรทัด 99–100 | Learning ของบอทตัวอื่นสูญหายเมื่อ process ปิด | ยังไม่แก้ไข — ต้องเปลี่ยนเป็น `List<WeakReference<UnifiedIgnisExecutor>>` |
| BUG-08 | `_loggedDecisionKeys` ไม่ถูก clear ระหว่างเทิร์น — การ์ดที่เล่นซ้ำใน turn ต่างกันอาจไม่ถูก log | บรรทัด 78, 210–212 | decisions.jsonl ขาดข้อมูล — Python sandbox วิเคราะห์ผิด | ยังไม่แก้ไข — ควร clear ใน `OnNewTurn()` |
| BUG-09 | Zone limit check (selfMonsters >= 5) ตัด `score = 0` แทน `return false` — flow ยังเดินต่อไปถึง decision threshold | บรรทัด 1923–1926 | ไม่ก่อ bug โดยตรง แต่ logic ไม่ชัดเจน | Minor — ควรเป็น `return false` |
| BUG-10 | `LogState()` loop SpellZone index 0–7 รวม index 5 (Field Zone) และ 6–7 (Extra Monster Zone) โดยไม่ label ต่าง | บรรทัด 2256 | Log อาจสับสนระหว่าง Field Zone กับ Spell/Trap Zone ปกติ | Minor — cosmetic |

### 🔵 Minor

| # | ปัญหา | ตำแหน่ง | ผลกระทบ | สถานะ |
|---|-------|---------|----------|-------|
| BUG-11 | `combo_plans` fallback มี else branch ซ้อน 2 ชั้นที่ต่างก็เพิ่ม "PlanA" — บางการ์ดได้ PlanA สองครั้งใน ArrayList | บรรทัด 357–362 | combo_plans.Contains("PlanA") ยังทำงานถูก แต่ ArrayList มีค่าซ้ำโดยไม่จำเป็น | Minor |
| BUG-12 | Impermanence column check อาจ miss ถ้า opponent ยิง Impermanence ก่อน `OnNewTurn` รัน (timing edge case) | บรรทัด 2775–2795, 2821–2836 | บอทวางการ์ดในคอลัมน์ที่ถูก negate โดยไม่รู้ตัว | Minor — edge case |

---

## APPENDIX B — Historical Known Issues (แก้ไขแล้ว)

| # | ปัญหา | แก้ไขใน |
|---|-------|---------|
| H-01 | Effect Veiler จำกัดแค่ Main1 พลาดการ์ดที่ summon ใน Main2 | v2.1 |
| H-02 | Hard Cap + Anti-Inflation Decay ลำดับสลับกัน ทำให้ Decay ไร้ผล | v2.1 |
| H-03 | Droll & Lock Bird มี role "recovery" อาจใช้ Droll ผิดจังหวะ | ยังค้างอยู่ |
| H-04 | 4 เด็คไม่มี deck config (Goldlord, Invoke, Kwtune, Labrynth) | ยังค้างอยู่ |
| H-05 | Learning Pipeline ไม่รันเพราะ match timeout ทุก match — ApplyRealTimeLearning ไม่เคยเจอ outcome Win/Loss | ยังค้างอยู่ |
