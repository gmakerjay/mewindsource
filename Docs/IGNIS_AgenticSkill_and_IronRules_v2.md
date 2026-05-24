# WindBot IGNIS — Agentic Skill & Iron Rules v2
> เอกสารนี้ใช้แนบทุกครั้งที่สั่ง AI แก้ไขโค้ด `UnifiedIgnisExecutor.cs`
> อัปเดต: 2026-05-24 | **v2.1 — ตรงกับโค้ดจริง (2,408 บรรทัด)**

---
Folder BrainStroms คือที่เก็บ Analysis ทุกครั้งที่ผมสั่งอะไรให้สกัดออกมาเป็นไฟล์ md+ลงชื่อแล้วประทับเวลาด้วย หากผมสั่งให้ Refactor ,review นั่น = ว่าคุณต้้องวิเคราะ Code Tracer อย่างละเอียดทุกซอกทุกมุมเพิ่อหาแนวทางพัฒนาบอทให้เก่งขึ้น

## PART 1 — AGENTIC SKILL (คัดลอกแนบใน Prompt ทุกครั้ง)

```
You are a surgical C# code editor for the WindBot IGNIS project.
Your role is to fix ONLY what is explicitly requested — nothing more.

=== PROJECT CONTEXT ===
File: UnifiedIgnisExecutor.cs (2,408 lines)
Namespace: ProjectIgnisAI
Class: UnifiedIgnisExecutor : DefaultExecutor
Language: C# (.NET Framework, WindBot game AI)
Config: cards_registry.json, opponent_memory.json, deck/*.json

Key methods you must know:
- EvaluateCardAction()  → core scoring engine, returns bool (line 1050)
- OnCardAction()        → routes registered card actions (line 1471)
- OnDefaultActivate()   → fallback for unregistered card activations (line 1493)
- OnDefaultSummon()     → fallback for unregistered normal summons (line 1545)
- OnDefaultSpSummon()   → fallback for unregistered special summons (line 1602)
- OnChaining()          → tracks opponent disruptions (line 2217)
- ApplyRealTimeLearning() → post-match weight adjustment (line 550)
- GetNextPlan()         → combo plan branching PlanA→B→C→A (line 2210)
- UpdateGoal()          → sets current goal (line 800)

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
11. Safeguard against Card ID Mismatches: Always verify official Card IDs from yugioh database (e.g. Nibiru is 27204311, Gamma is 38814750) before checking them in safeguards.
12. Safeguard against strict equality checks on spell/trap positions: Never use raw value comparisons (like `== 1` or `== 4`) for checking if a Spell/Trap is face-up; always use native `card.IsFaceup()`.
13. Safeguard against ID-based filtering in card selection: When filtering out/excluding the currently active card from selection lists, always use reference equality (`c == Card`) rather than ID matching (`c.Id == Card.Id`), which would incorrectly exclude all duplicates of the same card.

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

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — บล็อก `if (type == ExecutorType.Activate)` — บรรทัด 1085–1092

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
| Droll & Lock Bird | 94145021 | `Duel.Player == 0` → false | 1097 |
| Effect Veiler | 97268402 | `Duel.Player == 0` หรือ `(Phase != Main1 && Phase != Main2)` → false | 1104 |
| Infinite Impermanence | 10045474 | เทิร์นตัวเอง → false / Chain 1 แต่ไม่มีเป้า → false | 1134 |
| Mulcharmy Fuwalos | 42141493 | `Duel.Player == 0` → false | 1144 |
| Nibiru, the Primal Being | 27204311 | `Duel.Player == 0` → false | 1152 |
| PSY-Framegear Gamma | 38814750 | `ourMonCount > 0` (ควบคุมมอนสเตอร์อยู่) → false | 1162 |

**⚠️ Known Issue — Effect Veiler:** (แก้ไขแล้วใน v2.1) ให้ใช้ได้ทั้ง Main1 และ Main2 โดยตรวจเช็ค `(Duel.Phase != DuelPhase.Main1 && Duel.Phase != DuelPhase.Main2)`

---

### ⚔️ กฎที่ 2 — ห้าม Chain ขัดขวางการ์ดตัวเอง

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — ต้นบล็อก Activate — บรรทัด 1073–1081

```csharp
// IRON RULE #2 — DO NOT REMOVE OR MODIFY
if (lastChainCard != null && lastChainCard.Controller == 0)
{
    if (meta.roles.Contains("interruption") || meta.roles.Contains("handtrap") || meta.roles.Contains("disruption"))
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

**ตำแหน่งในโค้ด:** `EvaluateCardAction()` — specific card safeguards — บรรทัด 1113–1141

```csharp
// IRON RULE #3a — Called by the Grave — DO NOT REMOVE (line 1113)
if (card.Id == 24224830)
{
    if (GetOpponentGraveMonsterCount() == 0)
    {
        LogToTurn("Block Called by the Grave: No monsters in opponent's GY to target.");
        return false;
    }
}

// IRON RULE #3b — Bystial Druiswurm & Magnamhut — DO NOT REMOVE (line 1123)
if (card.Id == 6637331 || card.Id == 33854624)
{
    if (GetOpponentGraveLightDarkCount() + GetBotGraveLightDarkCount() == 0)
    {
        LogToTurn(string.Format("Block Bystial {0}: No LIGHT/DARK monsters in either GY to banish.", GetCardName(card.Id)));
        return false;
    }
}

// IRON RULE #3c — Infinite Impermanence (Chain 1) — DO NOT REMOVE (line 1133)
if (card.Id == 10045474)
{
    if (lastChainCard == null && Duel.Player == 0 && GetOpponentFaceUpMonsterCount() == 0)
    {
        LogToTurn("Block Infinite Impermanence activation from hand: No face-up monsters on opponent's field to target.");
        return false;
    }
}
```

**เหตุผล:** เปิดการ์ดโดยไม่มีเป้าหมายที่ถูกต้องทำให้ game engine บังคับเลือกเป้าหมายผิดฝั่ง หรือเปิดใช้งานไปฟรีโดยไม่มีผล

---

### ⚔️ กฎที่ 4 — Fallback ต้องเป็น false เสมอ

**ตำแหน่งในโค้ด:** `OnDefaultActivate()` (1505), `OnDefaultSummon()` (1591), `OnDefaultSpSummon()` (1624)

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

**ตำแหน่งในโค้ด:** `ApplyRealTimeLearning()` — หลัง loop ปรับค่า `_ourCardsPlayed` — บรรทัด 667–676

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

**⚠️ Known Issue — Anti-Inflation Decay ซ้อนทับ:** (แก้ไขแล้วใน v2.1) ได้สลับลำดับเอา Decay ไว้ก่อน แล้วค่อยรัน Hard Cap (บรรทัด 667-676) เรียบร้อยแล้ว ทำให้ Decay และ Hard Cap ทำงานร่วมกันได้อย่างสมบูรณ์

---

### ⚔️ กฎที่ 6 — OnChaining ต้องเช็คทิศให้ถูก

**ตำแหน่งในโค้ด:** `OnChaining()` — บล็อกตรวจ disruption — บรรทัด 2232–2237

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

**ตำแหน่งในโค้ด:** `GetNextPlan()` — บรรทัด 2210

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

- [ ] กฎที่ 1 ยังอยู่: `Duel.Player == 0 && meta.roles.Contains("handtrap")` → false (บรรทัด 1085)
- [ ] กฎที่ 2 ยังอยู่: `lastChainCard.Controller == 0` + role interruption/handtrap/disruption → false (บรรทัด 1073)
- [ ] กฎที่ 3 ยังอยู่: Called by the Grave / Bystial / Imperm target check (บรรทัด 1113–1141)
- [ ] กฎที่ 4 ยังอยู่: fallback ทั้ง 3 ฟังก์ชันเป็น `false` (บรรทัด 1505, 1591, 1624)
- [ ] กฎที่ 5 ยังอยู่: Hard Cap loop `priority > 8 → 8` (บรรทัด 667–676)
- [ ] กฎที่ 6 ยังอยู่: `Controller == 0` และ `player == 1` ใน OnChaining (บรรทัด 2232–2237)
- [ ] กฎที่ 7 ยังอยู่: `GetNextPlan()` บรรทัดสุดท้าย return `"PlanA"` (บรรทัด 2214)
- [ ] บัค Card ID Mismatches: ตรวจสอบและแก้ไข ID ของ Nibiru (27204311) และ Gamma (38814750) ให้ถูกต้องเสมอ
- [ ] บัค Position Check: ไม่ใช้การเปรียบเทียบค่าดิบสำหรับเช็คการ์ดเวทมนตร์/กับดักหงายการ์ด ให้ใช้ `card.IsFaceup()` เสมอ
- [ ] บัค Reference Equality ใน OnSelectCard: การกรองการ์ดใบที่เปิดใช้งานออกจากรายการเลือก ต้องเปรียบเทียบด้วย instance reference (`c == Card`) เสมอ ห้ามใช้ ID (`c.Id == Card.Id`) ซึ่งจะกรองการ์ดชื่อซ้ำใบอื่นออกไปด้วย
- [ ] ไม่มี method signature เปลี่ยน
- [ ] ไม่มี using statement ใหม่
- [ ] ไม่มี LogToTurn / LogToMatch ถูกลบ
- [ ] จำนวนบรรทัดรวมไม่ต่างจากเดิมเกิน 30 บรรทัด (ถ้าเกินให้ตรวจสอบ)
- [ ] **⚠️ ตรวจสอบ Anti-Inflation Decay: ถ้าแก้ไขให้สลับลำดับกับ Hard Cap หรือปรับ condition**

---

## APPENDIX: Known Code Issues (ไม่ใช่ Iron Rules — แค่บันทึก)

ปัญหาเหล่านี้ถูกพบระหว่าง Audit v1→v2 แต่ **ไม่ใช่ Iron Rules** — สามารถแก้ไขได้โดยไม่ต้องขออนุญาตเจ้าของโปรเจกต์:

| # | ปัญหา | ตำแหน่ง | ผลกระทบ | สถานะ |
|---|-------|---------|----------|-------|
| 1 | Effect Veiler จำกัดแค่ Main1 | บรรทัด 1106 | พลาดการ์ดที่ summon ใน Main2 | แก้ไขแล้วใน v2.1 (ขยายเป็น Main1 & Main2) |
| 2 | Hard Cap + Anti-Inflation Decay ซ้อน | บรรทัด 660–676 | Decay ไร้ผล | แก้ไขแล้วใน v2.1 (สลับให้ Decay ทำงานก่อน) |
| 3 | Droll & Lock Bird มี role "recovery" | cards_registry_2026_AzaYummy.json | อาจใช้ Droll ผิดจังหวะ | ยังไม่แก้ไข |
| 4 | 4 เด็คไม่มี deck config | config/decks/ | Goldlord, Invoke, Kwtune, Labrynth | ยังไม่แก้ไข |
| 5 | Learning Pipeline ไม่เคยทำงาน* | ApplyRealTimeLearning() | Hard Cap ไม่เคยถูกเรียก ดูหมายเหตุด้านล่าง | ยังไม่แก้ไข |

> ***หมายเหตุข้อ 5:** Learning Pipeline ต้องการ outcome = Win/Loss ถึงจะปรับค่า ถ้า match timeout (ทุก match ที่ผ่านมา) → ไม่มีการเรียนรู้เกิดขึ้น ไม่เกี่ยวกับ Iron Rules โดยตรง แต่เป็น precondition ที่ทำให้ Iron Rules #5 ไม่มีผล*
