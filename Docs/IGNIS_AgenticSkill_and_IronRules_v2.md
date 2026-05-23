# WindBot IGNIS — Agentic Skill & Iron Rules v2
> เอกสารนี้ใช้แนบทุกครั้งที่สั่ง AI แก้ไขโค้ด `UnifiedIgnisExecutor.cs`
> อัปเดต: 2026-05-23 | **v2 — ตรงกับโค้ดจริง (1,668 บรรทัด)**

---

## PART 1 — AGENTIC SKILL (คัดลอกแนบใน Prompt ทุกครั้ง)

```
You are a surgical C# code editor for the WindBot IGNIS project.
Your role is to fix ONLY what is explicitly requested — nothing more.

=== PROJECT CONTEXT ===
File: UnifiedIgnisExecutor.cs (1,668 lines)
Namespace: ProjectIgnisAI
Class: UnifiedIgnisExecutor : DefaultExecutor
Language: C# (.NET Framework, WindBot game AI)
Config: cards_registry.json, opponent_memory.json, deck/*.json

Key methods you must know:
- EvaluateCardAction()  → core scoring engine, returns bool (line 859)
- OnCardAction()        → routes registered card actions (line 1189)
- OnDefaultActivate()   → fallback for unregistered card activations (line 1211)
- OnDefaultSummon()     → fallback for unregistered normal summons (line 1263)
- OnDefaultSpSummon()   → fallback for unregistered special summons (line 1299)
- OnChaining()          → tracks opponent disruptions (line 1549)
- ApplyRealTimeLearning() → post-match weight adjustment (line 502)
- GetNextPlan()         → combo plan branching PlanA→B→C→A (line 1542)
- UpdateGoal()          → sets current goal (line 672)

=== SURGICAL EDIT RULES ===
1. Fix ONLY the lines or method explicitly named in the request.
2. Do NOT refactor, rename, reorder, or restructure any surrounding code.
3. Do NOT add new using statements unless explicitly told to.
4. Do NOT change method signatures.
5. Do NOT remove existing Log or LogToTurn calls.
6. Do NOT alter any other method that is not mentioned in the request.
7. If the fix requires touching more than 20 lines, STOP and ask for confirmation first.
8. Show a BEFORE / AFTER diff for every changed line. No exceptions.
9. Never silently change a comment — if you must update a comment, mark it as "(comment updated)".
10. After the fix, list every line number you changed. Nothing else.

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

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — บล็อก `if (type == ExecutorType.Activate)` — บรรทัด 894–900

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
| Droll & Lock Bird | 94145021 | `Duel.Player == 0` → false | 906 |
| Effect Veiler | 97268402 | `Duel.Player == 0` หรือ `Phase != DuelPhase.Main1` → false | 913 |
| Infinite Impermanence | 10045474 | เทิร์นตัวเอง → false / Chain 1 แต่ไม่มีเป้า → false | 943 |
| Mulcharmy Fuwalos | 42141493 | `Duel.Player == 0` → false | 958 |

**⚠️ Known Issue — Effect Veiler:** โค้ดใช้ `Duel.Phase != DuelPhase.Main1` ซึ่งจำกัดให้ Veiler ทำงานได้เฉพาะ Main1 ของฝ่ายตรงข้าม ในกฎ Yu-Gi-Oh จริง Veiler ใช้ได้ทั้ง Main1 และ Main2 ควรเปลี่ยนเป็น `!Duel.IsMainPhase()` เพื่อครอบคลุมทั้งสอง Main Phase. (รอการยืนยันจากเจ้าของโปรเจกต์)

---

### ⚔️ กฎที่ 2 — ห้าม Chain ขัดขวางการ์ดตัวเอง

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — ต้นบล็อก Activate — บรรทัด 881–889

```csharp
// IRON RULE #2 — DO NOT REMOVE OR MODIFY
if (lastChainCard != null && lastChainCard.Controller == 0)
{
    if (meta.roles.Contains("interruption") || meta.roles.Contains("handtrap") || meta.roles.Contains("disruption"))
    {
        LogToTurn(string.Format("Block chaining self-hurt: {0} responding to our own card: {1}",
            GetCardName(card.Id), GetCardName(lastChainCard.Id)));
        return false;
    }
}
```

**เหตุผล:** บอทเคยโยน Ash Blossom / Baronne de Fleur ใส่เอฟเฟกต์ตัวเอง ทำให้ยกเลิกคอมโบตัวเองไปฟรี

---

### ⚔️ กฎที่ 3 — ห้ามเปิดการ์ดที่ต้องการเป้าหมายโดยไม่มีเป้า

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — specific card safeguards — บรรทัด 922–955

```csharp
// IRON RULE #3a — Called by the Grave — DO NOT REMOVE  (line 922)
if (card.Id == 24224830 && lastChainCard == null && GetOpponentGraveMonsterCount() == 0)
    return false;

// IRON RULE #3b — Bystial Druiswurm & Magnamhut — DO NOT REMOVE  (line 932)
if ((card.Id == 6637331 || card.Id == 33854624) && GetOpponentGraveLightDarkCount() == 0)
    return false;

// IRON RULE #3c — Infinite Impermanence (Chain 1) — DO NOT REMOVE  (line 942)
if (card.Id == 10045474 && lastChainCard == null && GetOpponentFaceUpMonsterCount() == 0)
    return false;
```

**เหตุผล:** เปิดการ์ดโดยไม่มีเป้าหมายที่ถูกต้องทำให้ game engine บังคับเลือกเป้าหมายผิดฝั่ง หรือเปิดใช้งานไปฟรีโดยไม่มีผล

---

### ⚔️ กฎที่ 4 — Fallback ต้องเป็น false เสมอ

**ตำแหน่งในโค้ด:** `OnDefaultActivate()` (1223), `OnDefaultSummon()` (1309), `OnDefaultSpSummon()` (1342)

```csharp
// IRON RULE #4 — DO NOT CHANGE TO true
else
{
    decision = false; // Unknown card — safe default, do not play blindly
}
```

**เหตุผล:** การ์ดนอก registry ไม่ผ่าน safeguard ใดเลย การ fallback เป็น `true` หมายความว่าบอทจะเล่น Dark Hole / Raigeki / หรือการ์ดอันตรายอื่นๆ โดยไม่มีการตรวจสอบ

---

### ⚔️ กฎที่ 5 — Priority Hard Cap ที่ 8

**ตำแหน่งในโค้ด:** `ApplyRealTimeLearning()` — หลัง loop ปรับค่า `_ourCardsPlayed` — บรรทัด 596–605

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

**เหตุผล:** Priority Inflation คือภัยเงียบที่ทำให้ทุกการ์ดมีคะแนนเท่ากัน → scoring engine ไม่มีความหมาย Cap ที่ 8 ให้ช่องว่างสำหรับ Q-Learning และ bonus modifier ยังทำงานได้

**⚠️ Known Issue — Anti-Inflation Decay ซ้อนทับ:** Hard Cap (บรรทัด 596–605) set priority > 8 → 8 เสร็จ ตามด้วย Anti-Inflation Decay (บรรทัด 607–622) ซึ่งตรวจ `priority >= 9` เพื่อลด priority → **Decay ไร้ผลเพราะ Hard Cap ทำไว้ก่อน** ควรสลับลำดับ: เอา Decay ไว้ก่อน แล้วค่อย Hard Cap หรือปรับ Decay condition เป็น `>= 8` แทน

---

### ⚔️ กฎที่ 6 — OnChaining ต้องเช็คทิศให้ถูก

**ตำแหน่งในโค้ด:** `OnChaining()` — บล็อกตรวจ disruption — บรรทัด 1564–1569

```csharp
// IRON RULE #6 — Controller == 0 (our card) and player == 1 (opponent chains) — DO NOT SWAP
if (lastChain != null && lastChain.Controller == 0) // Our card was in the chain before
{
    if (player == 1) // Opponent is the one chaining into us
    {
        // ... record disruption
    }
}
```

**เหตุผล:** ถ้าสลับค่าจะบันทึก disruption กลับด้าน ทำให้ `ApplyRealTimeLearning()` ปรับค่าผิดทิศ บอทจะ "เรียนรู้" ผิดในทุก match

---

### ⚔️ กฎที่ 7 — GetNextPlan ต้องวนกลับ PlanA

**ตำแหน่งในโค้ด:** `GetNextPlan()` — บรรทัด 1542

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

---

## PART 3 — SCOPE BOUNDARY MAP

ใช้แผนผังนี้ตอบคำถามว่า "แก้จุดนี้ ควรแตะอะไรบ้าง"

```
ต้องการแก้                    ✅ แตะได้              ❌ ห้ามแตะ
─────────────────────────────────────────────────────────────────
Safeguard การ์ดใบใหม่       EvaluateCardAction()   OnChaining()
                                                    ApplyRealTimeLearning()
                                                    scoring logic

เพิ่มการ์ดลง registry       cards_registry.json    *.cs ทุกไฟล์

แก้ scoring weight          EvaluateCardAction()   Iron Rules ทั้ง 7 ข้อ
                            (บรรทัด score += ...)

แก้ Learning logic          ApplyRealTimeLearning() EvaluateCardAction()
                                                    OnChaining()

แก้ combo plan              GetNextPlan()           scoring logic
                            _currentPlan fields     Iron Rules

แก้ fallback behavior       OnDefaultActivate()     EvaluateCardAction()
                            OnDefaultSummon()        Iron Rules
                            OnDefaultSpSummon()
```

---

## PART 4 — AUDIT CHECKLIST

ใช้รายการนี้หลังให้ AI แก้โค้ดทุกครั้ง

- [ ] กฎที่ 1 ยังอยู่: `Duel.Player == 0 && meta.roles.Contains("handtrap")` → false (บรรทัด 894)
- [ ] กฎที่ 2 ยังอยู่: `lastChainCard.Controller == 0` + role interruption/handtrap/disruption → false (บรรทัด 882)
- [ ] กฎที่ 3 ยังอยู่: Called by the Grave / Bystial / Imperm target check (บรรทัด 922–955)
- [ ] กฎที่ 4 ยังอยู่: fallback ทั้ง 3 ฟังก์ชันเป็น `false` (บรรทัด 1223, 1309, 1342)
- [ ] กฎที่ 5 ยังอยู่: Hard Cap loop `priority > 8 → 8` (บรรทัด 596–605)
- [ ] กฎที่ 6 ยังอยู่: `Controller == 0` และ `player == 1` ใน OnChaining (บรรทัด 1564–1569)
- [ ] กฎที่ 7 ยังอยู่: `GetNextPlan()` บรรทัดสุดท้าย return `"PlanA"` (บรรทัด 1546)
- [ ] ไม่มี method signature เปลี่ยน
- [ ] ไม่มี using statement ใหม่
- [ ] ไม่มี LogToTurn / LogToMatch ถูกลบ
- [ ] จำนวนบรรทัดรวมไม่ต่างจากเดิมเกิน 30 บรรทัด (ถ้าเกินให้ตรวจสอบ)
- [ ] **⚠️ ตรวจสอบ Anti-Inflation Decay: ถ้าแก้ไขให้สลับลำดับกับ Hard Cap หรือปรับ condition**

---

## APPENDIX: Known Code Issues (ไม่ใช่ Iron Rules — แค่บันทึก)

ปัญหาเหล่านี้ถูกพบระหว่าง Audit v1→v2 แต่ **ไม่ใช่ Iron Rules** — สามารถแก้ไขได้โดยไม่ต้องขออนุญาตเจ้าของโปรเจกต์:

| # | ปัญหา | ตำแหน่ง | ผลกระทบ |
|---|-------|---------|----------|
| 1 | Effect Veiler จำกัดแค่ Main1 | บรรทัด 915 | พลาดการ์ดที่ summon ใน Main2 |
| 2 | Hard Cap + Anti-Inflation Decay ซ้อน | บรรทัด 596–622 | Decay ไร้ผล |
| 3 | Droll & Lock Bird มี role "recovery" | cards_registry_2026_AzaYummy.json | อาจใช้ Droll ผิดจังหวะ |
| 4 | 4 เด็คไม่มี deck config | config/decks/ | Goldlord, Invoke, Kwtune, Labrynth |
| 5 | Learning Pipeline ไม่เคยทำงาน* | ApplyRealTimeLearning() | Hard Cap ไม่เคยถูกเรียก ดูหมายเหตุด้านล่าง |

> ***หมายเหตุข้อ 5:** Learning Pipeline ต้องการ outcome = Win/Loss ถึงจะปรับค่า ถ้า match timeout (ทุก match ที่ผ่านมา) → ไม่มีการเรียนรู้เกิดขึ้น ไม่เกี่ยวกับ Iron Rules โดยตรง แต่เป็น precondition ที่ทำให้ Iron Rules #5 ไม่มีผล*
