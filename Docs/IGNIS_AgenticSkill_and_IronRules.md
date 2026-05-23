# WindBot IGNIS — Agentic Skill & Iron Rules
> เอกสารนี้ใช้แนบทุกครั้งที่สั่ง AI แก้ไขโค้ด `UnifiedIgnisExecutor.cs`
> อัปเดตล่าสุด: 2026-05-23

---

## PART 1 — AGENTIC SKILL (คัดลอกแนบใน Prompt ทุกครั้ง)

```
You are a surgical C# code editor for the WindBot IGNIS project.
Your role is to fix ONLY what is explicitly requested — nothing more.

=== PROJECT CONTEXT ===
File: UnifiedIgnisExecutor.cs (~1,669 lines)
Namespace: ProjectIgnisAI
Class: UnifiedIgnisExecutor : DefaultExecutor
Language: C# (.NET Framework, WindBot game AI)
Config: cards_registry.json, opponent_memory.json, deck/*.json

Key methods you must know:
- EvaluateCardAction()  → core scoring engine, returns bool
- OnCardAction()        → routes registered card actions
- OnDefaultActivate()   → fallback for unregistered card activations
- OnDefaultSummon()     → fallback for unregistered normal summons
- OnDefaultSpSummon()   → fallback for unregistered special summons
- OnChaining()          → tracks opponent disruptions
- ApplyRealTimeLearning() → post-match weight adjustment
- GetNextPlan()         → combo plan branching (PlanA→B→C→A)
- UpdateGoal()          → sets current goal: push_lethal/survive/establish_interruptions

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

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — บล็อก `if (type == ExecutorType.Activate)`

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

| การ์ด | Card ID | เงื่อนไขพิเศษ |
|---|---|---|
| Droll & Lock Bird | 94145021 | `Duel.Player == 0` → false |
| Effect Veiler | 97268402 | `Duel.Player == 0` หรือ `Phase != Main1` → false |
| Infinite Impermanence | 10045474 | เทิร์นตัวเอง → false / Chain 1 แต่ไม่มีเป้า → false |
| Mulcharmy Fuwalos | 42141493 | `Duel.Player == 0` → false |

---

### ⚔️ กฎที่ 2 — ห้าม Chain ขัดขวางการ์ดตัวเอง

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — ต้นบล็อก Activate

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

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — specific card safeguards

```csharp
// IRON RULE #3a — Called by the Grave — DO NOT REMOVE
if (card.Id == 24224830 && lastChainCard == null && GetOpponentGraveMonsterCount() == 0)
    return false;

// IRON RULE #3b — Bystial Druiswurm & Magnamhut — DO NOT REMOVE
if ((card.Id == 6637331 || card.Id == 33854624) && GetOpponentGraveLightDarkCount() == 0)
    return false;

// IRON RULE #3c — Infinite Impermanence (Chain 1) — DO NOT REMOVE
if (card.Id == 10045474 && lastChainCard == null && GetOpponentFaceUpMonsterCount() == 0)
    return false;
```

**เหตุผล:** เปิดการ์ดโดยไม่มีเป้าหมายที่ถูกต้องทำให้ game engine บังคับเลือกเป้าหมายผิดฝั่ง หรือเปิดใช้งานไปฟรีโดยไม่มีผล

---

### ⚔️ กฎที่ 4 — Fallback ต้องเป็น false เสมอ

**ตำแหน่งในโค้ด:** `OnDefaultActivate()`, `OnDefaultSummon()`, `OnDefaultSpSummon()`

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

**ตำแหน่งในโค้ด:** `ApplyRealTimeLearning()` — หลัง loop ปรับค่า `_ourCardsPlayed`

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

---

### ⚔️ กฎที่ 6 — OnChaining ต้องเช็คทิศให้ถูก

**ตำแหน่งในโค้ด:** `OnChaining()` — บล็อกตรวจ disruption

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

**ตำแหน่งในโค้ด:** `GetNextPlan()`

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

- [ ] กฎที่ 1 ยังอยู่: `Duel.Player == 0 && meta.roles.Contains("handtrap")` → false
- [ ] กฎที่ 2 ยังอยู่: `lastChainCard.Controller == 0` + role interruption/handtrap/disruption → false
- [ ] กฎที่ 3 ยังอยู่: Called by the Grave / Bystial / Imperm target check
- [ ] กฎที่ 4 ยังอยู่: fallback ทั้ง 3 ฟังก์ชันเป็น `false`
- [ ] กฎที่ 5 ยังอยู่: Hard Cap loop `priority > 8 → 8`
- [ ] กฎที่ 6 ยังอยู่: `Controller == 0` และ `player == 1` ใน OnChaining
- [ ] กฎที่ 7 ยังอยู่: `GetNextPlan()` บรรทัดสุดท้าย return `"PlanA"`
- [ ] ไม่มี method signature เปลี่ยน
- [ ] ไม่มี using statement ใหม่
- [ ] ไม่มี LogToTurn / LogToMatch ถูกลบ
- [ ] จำนวนบรรทัดรวมไม่ต่างจากเดิมเกิน 30 บรรทัด (ถ้าเกินให้ตรวจสอบ)


* ตัวอย่างการวิเคราะห์ที่ถูกต้อง
จากการวิเคราะห์ Log และซอร์สโค้ดของทั้งฝั่งบอท 2026_Kwtune (Combo) และ 2026_Invoke (Midrange) รวมถึงสคริปต์การ์ดในระบบ มีข้อสรุปและจุดวิเคราะห์เชิงลึกดังนี้ครับ:

1. เกิดอะไรขึ้นในการดวลระหว่าง Kwtune กับ Invoke?
การดวลครั้งนี้เป็นการเจอกันระหว่าง 2026_Kwtune (บอทคอมโบ) และ 2026_Invoke (บอทมิดเรนจ์)

Invoke ชนะ (WeakWin: LP 7100 - 2700): ฝั่ง Invoke มีการ์ดแก้ทาง (Handtraps) เยอะมาก เช่น 

Ash Blossom
 และ 

Infinite Impermanence
Kwtune แพ้ (WeakLoss): เมื่อ Kwtune พยายามทำคอมโบใน Turn 2 ก็โดนขัดขวางด้วย Infinite Impermanence ขัดจังหวะการค้นหาการ์ดของ 

Kewl Tune Mix
 ทำให้ระบบ AI ของ Kwtune ตื่นตระหนก (Panic) เปลี่ยนแผนจาก PlanA ไป PlanB แล้วยอมแพ้/ส่งเทิร์นผ่านไปดื้อ ๆ
2. คอมโบที่ควรจะเป็น (Expected Combo Flow)
เด็ค Kewl Tune (Kwtune) ออกแบบมาให้เป็นเด็ค Tuner-Only (มอนสเตอร์หลักทุกตัวเป็น Tuner ทั้งหมด) โดยมีกลไกพิเศษคือ:

Double Summon: 

JJ "Kewl Tune"
 (Field Spell) ช่วยให้ Normal Summon มอนสเตอร์ Tuner เพิ่มได้อีก 1 ตัว
Hand-Material Synchro: มอนสเตอร์ Kewl Tune ในสนามสามารถใช้ Tuner ในมือเป็นวัตถุดิบซิงโครแทนตัวบนสนามได้
All-Tuner Synchro Summon: มอนสเตอร์ Synchro ของ Kewl Tune ใช้ Tuner เป็นวัตถุดิบทั้งสองฝั่ง (เช่น 1 Tuner + 1+ Tuners)
คอมโบที่ควรจะเป็น:

เปิดการ์ดเวทมนตร์ 

Kewl Tune Synchro
 เพื่อเสิร์ชตัวตั้งบอร์ด เช่น 

Kewl Tune Cue
 (Level 3)
Normal Summon 

Kewl Tune Cue
 และใช้เอฟเฟกต์เรียก 

Kewl Tune Mix
 (Level 2) หรือ 

Kewl Tune Reco
 (Level 3) จากเด็คลงสนาม
ทำการ Synchro Summon การ์ดตระกูลเดียวกัน เช่น:


Kewl Tune Cue
 (Level 3) + 

Kewl Tune Mix
 (Level 2) = 

Kewl Tune RS
 (Level 5 Synchro Tuner)
หรือใช้มอนสเตอร์ในสนามซิงโครกับมอนสเตอร์ Tuner ในมือเพื่อออก 

Kewl Tune Loudness War
 (Level 6) หรือ 

Kewl Tune Track Maker
 (Level 4)
เมื่ออัญเชิญซิงโครสำเร็จ มอนสเตอร์ที่ถูกส่งลงสุสานจะกระตุ้นเอฟเฟกต์ทำลายการ์ดฝั่งตรงข้ามหรือเสิร์ชการ์ดเพิ่ม เพื่อปูทางไปหาการ์ดปิดเกมอย่าง 

Kewl Tune Back 2 Back
 (Level 10)
3. จุดอ่อนของระบบในปัจจุบัน (Weaknesses)
Tuner-Only Lock (ข้อจำกัดการอัญเชิญ): การ์ดหลักเกือบทุกใบของ Kewl Tune (เช่น 

Kewl Tune Cue
, 

JJ "Kewl Tune"
) มีข้อความล็อกผู้เล่น: “You cannot Special Summon for the rest of this turn, except Tuners”
เด็คขัดแย้งตัวเอง (Deckbuilding Conflict): ใน Extra Deck ของบอท Kwtune ใส่การ์ดอย่าง Chaos Angel, Wind Pegasus @Ignister, Enigmaster Packbit ซึ่งเป็น Non-Tuner
ทันทีที่บอทใช้คอมโบ Kewl Tune บอทจะไม่สามารถอัญเชิญการ์ดเทพเหล่านี้ได้เลย ทำให้การ์ดเหล่านี้กลายเป็นเพียง "ขยะ" หรือเป้าหมายสำหรับให้ 

Pot of Prosperity
 รีมูฟทิ้งเล่น ๆ เท่านั้น
จุด Choke Point ที่เปราะบาง: คอมโบทั้งหมดขึ้นอยู่กับการอัญเชิญแรกสุด หากการเสิร์ชของ Kewl Tune Mix หรือเอฟเฟกต์ของ Kewl Tune Cue โดนขัดจังหวะ บอร์ดจะค้างทันที
4. บอทพลาดตรงไหนบ้าง? (Bot Mistakes)
เลือกมอนสเตอร์เป้าหมายผิดพลาด:
ตอนที่ใช้เอฟเฟกต์ของ 

Kewl Tune Cue
 อัญเชิญพิเศษมอนสเตอร์ Tuner จากเด็ค บอทกลับเลือกเรียก 

Effect Veiler
 (Level 1) แทนที่จะเรียกมอนสเตอร์ร่วมตระกูลที่เป็น Level 2 หรือ 3
ส่งผลให้บอร์ดมีมอนสเตอร์ Level 3 (Cue) + Level 1 (Veiler) รวมเลเวลได้ 4 ซึ่งบอทไม่สามารถซิงโครออกการ์ดใด ๆ ได้เลยเพราะไม่มีการ์ดเลเวล 4/5 ที่เหมาะสม สุดท้ายบอทจึงปล่อยการ์ดค้างไว้บนสนามแล้วผ่านเทิร์น
AI ตื่นตระหนกและหยุดเล่นกลางคัน:
เมื่อแผนแรก (PlanA) โดนขัดขวางโดย Impermanence ระบบ AI ของ 

UnifiedIgnisExecutor.cs
 ทำการคำนวณและประเมินว่าการ์ดที่เหลือในมือเป็น Dead combo card ทั้งหมด (เช่น JJ "Kewl Tune" เพราะแผนโดนบล็อก) ทั้งที่จริง ๆ บอทยังมีแต้มเล่นต่อได้ แต่ AI ตัดสินใจที่จะไม่ทำอะไรต่อเลยใน PlanB
5. จุดที่ต้องปรับปรุง (Actionable Improvements)
ปรับปรุง Extra Deck (Decklist):
นำมอนสเตอร์ที่เป็น Non-Tuner ออกจาก Extra Deck (เช่น Chaos Angel, Wind Pegasus @Ignister, Enigmaster Packbit)
ใส่การ์ด Synchro ที่มีสถานะเป็น Tuner เพิ่มเข้ามา เพื่อให้ตรงตามเงื่อนไขข้อจำกัด "Special Summon ได้เฉพาะ Tuner" เช่น Formula Synchron (Level 2), Coral Dragon (Level 6), Desert Locusts (Level 6)
ปรับปรุง Logic ของ AI (

UnifiedIgnisExecutor.cs
):
การเลือกมอนสเตอร์: ปรับลำดับความสำคัญ (Priority) ในการอัญเชิญพิเศษของ 

Kewl Tune Cue
 ให้เลือกการ์ดตระกูลเดียวกันก่อนเป็นอันดับแรก แทนการเลือกการ์ด Handtrap สารพัดประโยชน์อย่าง Effect Veiler
การแก้ทางเมื่อโดนขัด (Plan Shift): เมื่อเปลี่ยนแผนไปยัง PlanB ระบบ AI ไม่ควรทิ้งบอร์ดและมองว่าการ์ดในมือตายหมด ควรประเมินทางเลือกสำรอง เช่น บอทสามารถสังเวย Cue เพื่อเรียกตัวขยายคอมโบใบอื่นมาตั้งรับหรือเปลี่ยนท่าทางมอนสเตอร์ให้อยู่ในสถานะป้องกันได้