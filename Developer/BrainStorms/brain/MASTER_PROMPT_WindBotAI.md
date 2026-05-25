# MASTER PROMPT — ProjectIgnis AI Training System
> ใช้ prompt นี้เป็น System Prompt หรือแนบต้นการสนทนาทุกครั้งที่ขอให้ AI แก้ไข/เพิ่มโค้ดในระบบนี้

---

## 1. บริบทระบบ (อ่านก่อนทำอะไรทั้งหมด)

ระบบนี้คือ **WindBot + ProjectIgnisAI** สำหรับเล่น Yu-Gi-Oh! แบบ headless parallel
มีสองฝั่งที่ทำงานร่วมกัน:

| ฝั่ง | ไฟล์หลัก | หน้าที่ |
|---|---|---|
| C# (runtime) | `BaseCustomExecutor.cs`, `DreadnoughtExecutor.cs`, `InvokeExecutor.cs`, `UnifiedIgnisExecutor.cs` | ตัดสินใจเล่น, log decisions, real-time learning |
| Python (pipeline) | `parallel_launcher.py`, `run_multi_iterations.py`, `save_outcomes_to_sql.py`, `q_learning.py`, `learning_sandbox.py`, `shared_utils.py` | เปิด process, เก็บ SQL, train Q-values |

---

## 2. กฎเหล็ก — ห้ามละเมิดไม่ว่ากรณีใด

### C# side

**G1 — Lock ก่อนเขียนไฟล์ทุกครั้ง**
ทุก file I/O ที่แตะ `cards_registry_*.json` หรือ `opponent_memory.json` ต้องอยู่ใน `lock (_staticLock)` เสมอ
ห้ามเพิ่ม `File.WriteAllText` / `File.AppendAllText` นอก lock โดยเด็ดขาด

**G2 — atomic write เท่านั้น**
การเขียน registry ใช้ `WriteFileWithRetry()` ที่มีอยู่แล้ว ห้ามเปลี่ยนเป็น `File.WriteAllText` ตรง ๆ
pattern ที่ถูกต้อง: เขียน temp → `File.Copy(backup)` → `WriteFileWithRetry(live)`

**G3 — Hard Cap priority = 8 ต้อง enforce หลังทุก write**
หลัง merge หรือ update ทุกครั้ง ต้องวน loop:
```csharp
if (card.priority > 8) card.priority = 8;
```
ห้ามลบ หรือย้ายตำแหน่ง hard cap check นี้

**G4 — `_learningApplied` flag ต้องเป็น true ก่อน SaveConfiguration()**
ห้าม call `SaveConfiguration()` โดยไม่ set `_learningApplied = true` ก่อน
(ป้องกัน double-save จาก MonitorLP thread)

**G5 — `LogDecision()` ต้องเรียกผ่าน `EvaluateCardAction()` เท่านั้น**
ห้ามเขียน callback ใน subclass executor ที่ return bool โดยตรงโดยไม่ผ่าน base `EvaluateCardAction`
pattern ที่ถูกต้องสำหรับ card-specific logic:
```csharp
// ✅ ถูก — ใช้ ExecuteWithLog wrapper
AddExecutor(ExecutorType.Activate, CARD_ID,
    () => ExecuteWithLog(CARD_ID, ExecutorType.Activate, MyCardEffect));

// ❌ ผิด — bypass logging
AddExecutor(ExecutorType.Activate, CARD_ID, MyCardEffect);
```

**G6 — `Executors.Clear()` ใน subclass ต้องลงท้ายด้วย catch-all fallbacks เสมอ**
หลัง `Executors.Clear()` และ AddExecutor การ์ดเฉพาะ ต้องมี:
```csharp
AddExecutor(ExecutorType.Activate, OnDefaultActivate);
AddExecutor(ExecutorType.Summon, OnDefaultSummon);
AddExecutor(ExecutorType.SpSummon, OnDefaultSpSummon);
AddExecutor(ExecutorType.SpellSet, OnDefaultSpellSet);
AddExecutor(ExecutorType.Repos, OnDefaultRepos);
AddExecutor(ExecutorType.MonsterSet, OnDefaultMonsterSet);
```

**G7 — ห้ามแตะ `MonitorLP()` loop interval**
`Thread.Sleep(200)` ใน MonitorLP ห้ามเปลี่ยน ลด หรือลบ
(ลดต่ำกว่านี้ทำให้ CPU spike ใน 20-instance mode)

**G8 — `ResetDuelState()` ต้องเรียก `SetupFolderLogging()` เสมอ**
ห้ามลบ `SetupFolderLogging()` ออกจาก `ResetDuelState()` — นั่นคือกลไก new-game detection

---

### Python side

**P1 — partition decisions.jsonl ด้วย `game_id` ไม่ใช่ turn number**
หากมี `game_id` field ใน decision record ให้ใช้ group by game_id
หากยังไม่มี ใช้ logic นี้เท่านั้น (อย่าเปลี่ยน):
```python
if turn < last_turn and last_turn > 0:
    # new game boundary
```
ห้ามใช้ `turn < last_turn` แบบเดิมที่ไม่มี `and last_turn > 0`

**P2 — atomic write ทุก registry save ใน Python**
```python
# ✅ ถูก
with tempfile.NamedTemporaryFile("w", dir=dir, delete=False, suffix=".json") as tmp:
    json.dump(data, tmp)
os.replace(tmp.name, live_path)

# ❌ ผิด
with open(live_path, "w") as f:
    json.dump(data, f)
```

**P3 — merge ด้วย max/avg ไม่ใช่ overwrite**
เมื่อ merge registry จากหลาย port ให้:
- `priority` → `max(a, b)` (เลือกค่าสูงสุด)
- `q_values[goal]` → `(a + b) / 2` (ค่าเฉลี่ย)
- `risk_if_negated`, `bait_value`, `followup_value` → `max(a, b)`
ห้ามทำ last-write-wins

**P4 — ห้าม `--wipe` โดยไม่มี explicit flag**
`parse_and_save(..., wipe=True)` ต้องมาจาก `--wipe` CLI argument เท่านั้น
ห้ามเรียก wipe จาก code path ปกติ

**P5 — `save_registry_list()` ใน shared_utils enforce hard cap ก่อน write**
ห้ามลบ block นี้ออก:
```python
for card in data:
    if "priority" in card and card["priority"] > 8:
        card["priority"] = 8
```

**P6 — ทุก path constant ต้องมาจาก `shared_utils.py`**
ห้ามเขียน hardcode path ซ้ำในไฟล์ใหม่
ให้ import จาก shared_utils: `SCRIPT_DIR`, `LIVE_LOGS_DIR`, `LIVE_CONFIG_DIR`, `DECKS_DIR`

---

## 3. สิ่งที่เพิ่มได้ (และต้องทำอย่างไร)

### เพิ่ม game_id ใน LogDecision (C#)
```csharp
// ใน SetupFolderLogging() — เพิ่มบรรทัดเดียว
_currentGameId = Guid.NewGuid().ToString().Substring(0, 8);

// ใน LogDecision() — เพิ่ม field เดียวใน JSON string
// หา line ที่ build json แล้ว inject "game_id" ก่อน "turn"
```
ไม่ต้องเปลี่ยน signature method ใด ๆ

### เพิ่ม per-port temp registry (C#)
```csharp
// ใน SetupFolderLogging() เพิ่ม:
string tempDir = Path.Combine(baseDir, "config", "temp");
Directory.CreateDirectory(tempDir);
_tempRegistryPath = Path.Combine(tempDir,
    $"registry_{_resolvedDeckName}_port{_currentPort}.json");

// ใน SaveConfiguration() เปลี่ยน WriteFileWithRetry target:
// เขียนลง _tempRegistryPath แทน registryPath โดยตรง
// (merger daemon จะ merge ทีหลัง)
```

### เพิ่ม ExecuteWithLog wrapper (C#)
```csharp
// เพิ่มใน BaseCustomExecutor — ไม่กระทบ method เดิม
protected bool ExecuteWithLog(int cardId, ExecutorType type, Func<bool> condition)
{
    UpdateGoal();
    bool result = condition();
    LogDecision(cardId, type.ToString(), _currentGoal,
                result ? 999.0 : 0.0, result, _currentPlan);
    return result;
}
```

### เพิ่ม merge_registries.py (Python — ไฟล์ใหม่)
ต้อง import จาก `shared_utils` และใช้ `save_registry_list()` เท่านั้น
ห้ามเขียน file I/O ตรง ๆ ในไฟล์นี้

---

## 4. สิ่งที่ต้องตรวจก่อน submit โค้ดใหม่ทุกครั้ง

```
Checklist (ตอบทุกข้อก่อน):
□ มี lock (_staticLock) ครอบทุก file write ใหม่หรือไม่?
□ priority hard cap (≤8) ถูก enforce หลัง logic ใหม่หรือไม่?
□ callback ใหม่ใน executor ผ่าน ExecuteWithLog หรือ OnCardAction หรือไม่?
□ Python write ใช้ tempfile + os.replace หรือไม่?
□ merge logic ใช้ max/avg ไม่ใช่ overwrite หรือไม่?
□ ไม่มี hardcode path ใหม่นอก shared_utils หรือไม่?
□ _learningApplied = true ถูก set ก่อน SaveConfiguration() หรือไม่?
```

---

## 5. รูปแบบ output ที่ต้องการ

เมื่อแก้ไขโค้ด ให้แสดงเฉพาะ:
1. **ชื่อไฟล์ + บรรทัดที่แก้** (เช่น `BaseCustomExecutor.cs line 502`)
2. **โค้ดเดิม** (ก่อน)
3. **โค้ดใหม่** (หลัง)
4. **เหตุผลสั้น ๆ** ว่าไม่ละเมิดกฎใด

ห้าม rewrite ทั้งไฟล์ถ้าไม่จำเป็น
