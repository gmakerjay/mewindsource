# Changelog — 25 พฤษภาคม 2026
**เวลา:** 2026-05-25 (Deep Audit + Critical Fixes)
**ผู้ตรวจสอบ/แก้ไข:** Hermes Agent (Deep Dive Code Audit)
**ขอบเขต:** UnifiedIgnisExecutor.cs (3038 → 3073 บรรทัด)

---

## 🔴 CRITICAL FIX: ระบบเรียนรู้กลับมาทำงานแล้ว

### ปัญหาที่พบ
ตรวจสอบโค้ดจริงเทียบกับ Changelog 12 รายการ + Docs + DeepDive Analysis — พบว่า **ระบบเรียนรู้ทั้งหมดเป็น Dead Code** มาตั้งแต่วันแรก

### Root Cause
**บรรทัด 749** — Guard condition กลับด้าน:
```diff
- if (Duel == null || Duel.Fields != null || ...)
+ if (Duel == null || Duel.Fields == null || ...)
```
ใช้ `!=` แทน `==` ทำให้ฟังก์ชัน `return` ทันที **ทุกครั้งที่ Duel.Fields มีค่า** (ซึ่งคือตลอดเวลาในเกม)

### ผลกระทบก่อนแก้ไข
- ❌ `ApplyRealTimeLearning()` — ไม่เคยรัน (200+ บรรทัด Dead Code)
- ❌ `SaveConfiguration()` — ไม่เคยถูกเรียกจาก Learning
- ❌ Smart Reward Learning — ไม่มีวันทำงาน
- ❌ Anti-Inflation Decay — ไม่มีวันทำงาน
- ❌ Hard Cap — ไม่มีวันทำงาน
- ❌ Bait Value Bootstrap — ไม่มีวันทำงาน
- ❌ Opponent Memory Natural Decay — ไม่มีวันทำงาน
- ❌ Priority/Q-Value/Bait/Danger ไม่เคยเปลี่ยนแปลงจากค่าเริ่มต้น

### สถานะหลังแก้ไข
- ✅ แก้ `!=` → `==` ที่บรรทัด 749
- ✅ คอมไพล์ผ่าน (0 errors, 0 warnings)

---

## 🟡 HIGH: เพิ่ม Learning Triggers หลายจุด

### 1. OnChainEnd — ตรวจจับ LP=0 ระหว่าง Chain
**ก่อน:** OnChainEnd() ทำแค่ log + base.OnChainEnd()
**หลัง:** ตรวจสอบ LP ทั้งสองฝ่าย ถ้า LP=0 → เรียก `ApplyRealTimeLearning()` ทันที
**เหตุผล:** แมตช์ส่วนใหญ่จบระหว่าง chain resolution — ถ้าไม่ตรวจจับตรงนี้ learning จะพลาด

### 2. OnNewTurn — ตรวจจับ LP=0 ระหว่าง Turn
**ก่อน:** OnNewTurn() ทำแค่ log state + reset plans
**หลัง:** ตรวจสอบ LP ทั้งสองฝ่าย ถ้า LP=0 → เรียก `ApplyRealTimeLearning()`
**เหตุผล:** Safety net สำหรับกรณีที่เกมจบระหว่าง turn transition

### 3. OnNewTurn — Periodic Save ทุก 3 เทิร์น
**ก่อน:** ไม่มี — ข้อมูลเรียนรู้จะหายถ้า process crash ก่อน Dispose
**หลัง:** เซฟ configuration ทุก 3 เทิร์น (เฉพาะเมื่อยังไม่ได้ทำ learning)
**เหตุผล:** ป้องกันข้อมูลเรียนรู้สูญหายระหว่างแมตช์ยาว

---

## 🟡 MEDIUM: Dynamic Score Threshold

### ปัญหาเดิม
`bool decision = score > 35.0;` — threshold คงที่ ไม่ปรับตามสถานการณ์

### การแก้ไข
```csharp
double threshold = 35.0;                           // Default
if (selfLP <= 2000) threshold = 15.0;              // ใกล้ตาย → เล่นเกือบทุกอย่าง
else if (selfLP <= 4000) threshold = 25.0;         // อันตราย → aggressive
else if (enemyLP <= 2000 && push_lethal) threshold = 10.0; // จะ lethal → เล่นหมด
else if (opponentThreat > 80.0) threshold = 20.0;  // บอร์ดศัตรูแน่น → ตอบโต้ไว
```

**ผล:** บอทจะ aggressive ขึ้นตอน LP ต่ำ, conservative ตอน LP ปกติ, เล่นทุกอย่างตอนจะ lethal

---

## 🟡 MEDIUM: SpellSet Hand Overflow Protection

### เพิ่ม
เมื่อมือเต็ม (≥6 ใบ) → เพิ่ม score +100.0 ให้เซ็ตการ์ดเพื่อป้องกัน discard ตอน End Phase

### ปรับ Threshold
SpellSet threshold: 35.0 → 25.0 (เพราะการเซ็ตการ์ดไม่เสี่ยงเท่าการ activate)

---

## 📊 Verification Results

| รายการ | สถานะ |
|--------|:-----:|
| C# Compilation | ✅ SUCCESSFUL (0 errors, 0 warnings) |
| Iron Rules #1-#8 | ✅ ครบถ้วน ไม่ถูกแตะ |
| Card Safeguards (9 การ์ด) | ✅ ครบถ้วน |
| Battle Phase AI | ✅ ใช้ API ปลอดภัย (AI.Attack) |
| Deck Configs (11 ไฟล์) | ✅ มีครบ |
| Card Registries (10 เด็ค) | ✅ มีครบ 160-196 การ์ด/เด็ค |
| Dynamic Registrations (147 การ์ด) | ✅ ทำงาน |
| Fallback Handlers (6 type) | ✅ ทำงาน |

---

## 📋 Audit Cross-Reference

### สิ่งที่ Changelog ก่อนหน้าอ้างว่าแก้แล้ว → ตรวจสอบแล้วจริง
- ✅ Priority Hard Cap ที่ 8 → อยู่
- ✅ `(IEnumerable)` casting → อยู่
- ✅ Anti-Inflation Decay ก่อน Hard Cap → อยู่
- ✅ Dead Combo Penalty แก้บทบาท → อยู่
- ✅ Zone limit check → `return false` → อยู่
- ✅ TTT/Thrust เอาดักออก → อยู่
- ✅ `IsLethalOnBoard()` รองรับ Main2 → อยู่
- ✅ `_loggedDecisionKeys.Clear()` ใน OnNewTurn → อยู่

### สิ่งที่ Changelog อ้างว่าแก้ → แต่ไม่มีผลเพราะ Learningไม่รัน
- ⚠️ Smart Reward Learning → อยู่แต่ไม่เคยรัน (Bug #1)
- ⚠️ Bait Decay → อยู่แต่ไม่เคยรัน (Bug #1)
- ⚠️ Bait Bootstrap → อยู่แต่ไม่เคยรัน (Bug #1)
- ⚠️ Opponent Memory Natural Decay → อยู่แต่ไม่เคยรัน (Bug #1)

### สิ่งที่ DeepDive Analysis รายงานผิด
- ❌ "0 Deck Configs" → จริงๆมี 11 ไฟล์
- ❌ "4 Bricked Decks" → registries ถูกเพิ่มแล้ว (Goldlord=176, Invoke=188, Kwtune=184, Labrynth=196)
- ❌ "OnDraw override" → เป็น Nice-to-have ไม่ใช่ Critical

---

## 🎯 ผลลัพธ์ที่คาดหวังหลังแก้ไข

### สิ่งที่บอทจะทำได้เพิ่มขึ้น
1. **เรียนรู้จากทุกแมตช์** — Win → +priority ให้ starter/payoff/searcher, Loss → -priority + risk_if_negated
2. **Opponent Memory อัพเดต** — จำการ์ดศัตรูที่ disrupt บ่อยๆ เพิ่ม learned_danger
3. **Priority Balancing** — การ์ดที่ไม่ได้ใช้จะ decay, การ์ดที่ใช้ชนะจะได้ boost
4. **Bait Value Learning** — เรียนรู้ว่าการ์ดไหนใช้เป็น bait ได้ดี
5. **Q-Value Learning** — Monte Carlo returns ต่อ goal/state
6. **Dynamic Decision** — เล่น aggressive เมื่อ LP ต่ำ, conservative เมื่อนำ

### สิ่งที่ต้องดูหลังรันจริง
- Learning อาจต้องรันหลายแมตช์กว่าจะเห็นผล (ปกติ 5-10 แมตช์)
- Opponent Memory ใช้ 95% decay → ต้องเจอศัตรูซ้ำๆถึงจะสะสม danger
- Priority cap ที่ 8 ป้องกัน inflation → การ์ดจะไม่เกิน 8 แม้ชนะ 100 แมตช์

---

## 🟢 UPDATE: การป้องกันการล็อกการโจมตีและการเลือกตำแหน่งการตั้งมอนสเตอร์อย่างปลอดภัย (24 พฤษภาคม 2026 รอบค่ำ)

### 1. แก้ไขบั๊ก TY-PHON ล็อกโจมตีผิดพลาด
- **ปัญหา:** บอทมีโค้ดเช็กว่าถ้ามี TY-PHON (ID: 93039339) อยู่บนสนาม บอทจะไม่ยอมโจมตีด้วยมอนสเตอร์จากเด็คหลัก (Non-Extra Deck) ส่งผลให้เมื่อบอทเดินเกมจนตั้งบอร์ดใหญ่เสร็จ แต่ฝ่ายตรงข้ามมี TY-PHON บอทจะไม่ยอมโจมตีและปล่อยผ่านรอบไปเรื่อยๆ จนบอร์ดหาย
- **การแก้ไข:** ลบเงื่อนไขเช็กล็อก TY-PHON ในฟังก์ชัน `CanCardAttack` ออกทั้งหมด เนื่องจากตามกฎจริง TY-PHON ไม่เคยขัดขวางการโจมตี

### 2. เพิ่มการเช็กการโดนยกเลิกเอฟเฟกต์ของการ์ดล็อกการโจมตี (Negation Check)
- **ปัญหา:** การ์ดล็อกการโจมตีฟิลด์ เช่น Mystic Mine, Messenger of Peace, Gravity Bind, และ Swords of Revealing Light แม้จะถูกยกเลิกเอฟเฟกต์ไปแล้ว (เช่น โดน Forbidden Droplet ปิดเอฟเฟกต์) บอทยังคิดว่าการล็อกมีผลอยู่และปฏิเสธการโจมตี
- **การแก้ไข:** เพิ่มเงื่อนไข `!s.IsDisabled()` เข้าไปในส่วนตรวจสอบการ์ดล็อกโจมตีทั้งหมดใน `CanCardAttack` เพื่อให้บอทโจมตีได้ตามปกติหากการ์ดล็อกนั้นไร้ผลแล้ว

### 3. เพิ่มการ Override เลือกตำแหน่งตั้งรับของมอนสเตอร์พลังโจมตีต่ำ (OnSelectPosition Override)
- **ปัญหา:** บอทเรียกมอนสเตอร์ที่มีพลังโจมตีต่ำหรือ 0 ATK (เช่น Linkuriboh, Fuwalos, หรือแฮนด์แทรปต่างๆ) มาตั้งไว้ในตำแหน่งหงายหน้าโจมตี ทำให้โดนฝ่ายตรงข้ามโจมตีกลับสร้างดาเมจจนแพ้ (OTK)
- **การแก้ไข:** เพิ่มการ Override เมธอด `OnSelectPosition` ใน [UnifiedIgnisExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/UnifiedIgnisExecutor.cs) โดยมีเกณฑ์การเลือกดังนี้:
  - หากมอนสเตอร์ที่อัญเชิญมีพลังโจมตี $\le$ 500 (รวมถึงมอนสเตอร์ 0 ATK) ระบบจะเลือกตั้ง **หงายหน้าตั้งรับ (FaceUpDefence)** หรือ **คว่ำหน้าตั้งรับ (FaceDownDefence)** เสมอ (ตามเงื่อนไขที่บอร์ดอนุญาต)
  - หากเป็น Turn 1 หรืออยู่ใน Main Phase 2 และพลังโจมตีของมอนสเตอร์ $\le$ พลังป้องกัน (ATK $\le$ DEF) จะเลือกตั้งรับ
  - หากฝ่ายตรงข้ามมีมอนสเตอร์ที่มีพลังโจมตีสูงกว่ามอนสเตอร์ที่เรากำลังจะลงสนาม ระบบจะเลือกตั้งรับเพื่อเซฟ LP

### 4. การตรวจสอบและการคอมไพล์
- ดำเนินการล้างโปรเซส `WindBot.exe` ที่ค้างและล็อกไฟล์ DLL อยู่ และสั่งรัน `compile_ai.bat`
- ผลลัพธ์: **Compilation SUCCESSFUL!** บิวด์ผ่านอย่างสมบูรณ์ไร้ข้อผิดพลาด
- ผลการจำลอง LAN Audit ในเบื้องหลังยังคงดำเนินต่อไปได้ด้วยดี (ผ่านรอบ 2 เรียบร้อยและกำลังรันรอบที่ 3)

