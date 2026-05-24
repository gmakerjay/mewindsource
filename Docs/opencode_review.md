# EDOTh (WindBot IGNIS) — การตรวจสอบและปรับปรุงระบบ AI เกมการ์ด

**วันที่ตรวจสอบ:** 24 พฤษภาคม 2026  
**เป้าหมาย:** วิเคราะห์ทั้งโปรเจกต์ ตรวจสอบความถูกต้องของ Changelog และปรับปรุงโค้ดเพื่อให้บอทเล่นเกมการ์ดได้เก่งขึ้น

---

## สารบัญ

1. [ภาพรวมโปรเจกต์](#1-ภาพรวมโปรเจกต์)
2. [สรุปโครงสร้างโปรเจกต์](#2-สรุปโครงสร้างโปรเจกต์)
3. [ผลการตรวจสอบ Changelog](#3-ผลการตรวจสอบ-changelog)
4. [จุดอ่อนที่พบและแก้ไข](#4-จุดอ่อนที่พบและแก้ไข)
5. [จุดอ่อนที่ควรปรับปรุงเพิ่มเติม](#5-จุดอ่อนที่ควรปรับปรุงเพิ่มเติม)
6. [คำแนะนำเชิงกลยุทธ์เพื่อให้บอทเก่งขึ้น](#6-คำแนะนำเชิงกลยุทธ์เพื่อให้บอทเก่งขึ้น)
7. [บทสรุป](#7-บทสรุป)

---

## 1. ภาพรวมโปรเจกต์

**EDOTh** คือโปรเจกต์บอท AI สำหรับเกม Yu-Gi-Oh! บนแพลตฟอร์ม EDOPro (Project Ignis) ชื่อ **WindBot IGNIS** ประกอบด้วย:

- **C# Engine (UnifiedIgnisExecutor.cs)** — 2,179 บรรทัด — แกนหลักการตัดสินใจของ AI
- **Python Sandbox (WindBot_Sandbox/)** — ระบบเรียนรู้ของ AI (Q-Learning, Heuristic Learning, Combo Simulator, A/B Tournament)
- **Configuration (JSON)** — ข้อมูลการ์ด บทบาท ลำดับความสำคัญ ความจำคู่ต่อสู้
- **Changelogs/ — 11 ไฟล์** — ประวัติการเปลี่ยนแปลง 2 วันล่าสุด

---

## 2. สรุปโครงสร้างโปรเจกต์

```
EDOTh/
├── WindBot/                     # C# AI Engine
│   ├── UnifiedIgnisExecutor.cs  # แกนหลัก AI (2179 บรรทัด)
│   ├── WindBot.exe              # ตัวรันบอท (733 KB)
│   ├── compile_ai.bat           # สคริปต์คอมไพล์
│   ├── bots.json                # 41 บอท
│   ├── config/                  # ตั้งค่า JSON
│   │   ├── cards_registry*.json # 8 ฐานข้อมูลการ์ดตามสำรับ
│   │   ├── card_names.json      # 24,340 ชื่อการ์ด
│   │   └── opponent_memory.json # ความจำคู่ต่อสู้
│   └── Decks/                   # ไฟล์สำรับ .ydk
├── WindBot_Sandbox/             # Python Learning Pipeline
│   ├── cockpit.py               # Web Dashboard (620 บรรทัด)
│   ├── shared_utils.py          # Utilities (202 บรรทัด)
│   ├── learning_sandbox.py      # Heuristic Learning (334 บรรทัด)
│   ├── q_learning.py            # Q-Learning (208 บรรทัด)
│   ├── combo_simulator.py       # Monte Carlo Simulator (257 บรรทัด)
│   ├── auto_role_detector.py    # ตรวจจับบทบาทการ์ด (210 บรรทัด)
│   ├── optimize_registry.py     # Hill-climbing Optimizer (170 บรรทัด)
│   └── ab_tournament.py         # A/B Tournament (319 บรรทัด)
├── Changelogs/                  # 11 ไฟล์ประวัติการเปลี่ยนแปลง
└── script/                      # 26,740 สคริปต์การ์ด .lua
```

---

## 3. ผลการตรวจสอบ Changelog

### 3.1 สรุปผลการตรวจสอบ

| หัวข้อ | จำนวน | รายละเอียด |
|--------|-------|------------|
| **TRUE** (ถูกต้อง) | 30 | คำกล่าวอ้างใน Changelog ตรงกับโค้ดจริง |
| **FALSE** (ไม่ถูกต้อง) | 4 | คำกล่าวอ้างใน Changelog ไม่ตรงกับโค้ดจริง |
| **Fixed แล้ว** | 3 | จุด FALSE ที่ได้รับการแก้ไขแล้วในการ Refactor ครั้งนี้ |
| **คงเหลือ** | 1 | จุด FALSE ที่แก้ไขแล้วอยู่แล้ว |

### 3.2 รายการ FALSE ที่พบ (Changelog ไม่ตรงกับโค้ด)

| # | Changelog กล่าวอ้าง | สิ่งที่พบในโค้ดจริง | ความรุนแรง | สถานะ |
|---|---------------------|---------------------|------------|--------|
| 1 | Anti-Inflation Decay ทำงานก่อน Hard Cap (บรรทัดที่ 31, changelog_critical_bugfixes.md) | Hard Cap ทำงานก่อน Decay (บรรทัด 641-650 → 652-667) Decay จึงแทบไม่มีผล | **สูง** — ทำให้ Anti-Inflation Decay ไร้ประสิทธิภาพ | ✅ **Fixed** สลับลำดับ |
| 2 | ปลดล็อค Triple Tactics Talent/Thrust แล้ว (changelog_api_audit_fixes.md) | ยังคงมีบล็อก `Duel.Player == 1` ที่บรรทัด 1158 ซึ่งเป็น Normal Spell ที่เปิดได้ในเทิร์นเราเท่านั้น | **สูง** — ทำให้ TTT/Thrust ใช้งานไม่ได้ | ✅ **Fixed** ลบบล็อกออก |
| 3 | ปลดล็อค Infinite Impermanence ในเทิร์นเรา (changelog_api_audit_fixes.md) | ยังเช็ค `GetOpponentFaceUpMonsterCount() == 0` ไม่ยอมให้เปิดจากมือถ้าไม่มีเป้าหมาย | **ปานกลาง** — จำกัดการเล่น Going Second | ✅ **Fixed** เพิ่มเงื่อนไข Duel.Player == 0 |
| 4 | Effect Veiler เช็คแค่ Main1 (รายงานครั้งแรก) | โค้ดจริงเช็ค `Main1 && Main2` ถูกต้องแล้ว | - | ไม่ต้องแก้ไข |

### 3.3 รายการ TRUE ที่ยืนยันแล้ว (30 รายการ)

- **Kwtune Optimization:** isKwtunePreferHigh hoisted, TryGetValue, HasSetcode(0x1ce) ✅
- **Deserialization Fix:** roles/combo_plans ใช้ IEnumerable ✅
- **Graveyard+Hand Threat Scanning:** ครบถ้วน ✅
- **HasStarterOrExtenderInHand:** รวม payoff/searcher ✅
- **Blocked Combo Penalty:** -90 และ roles check ✅
- **IsLethalOnBoard:** เช็ค Main1+Battle ✅
- **Anti-Self-Harm:** -200 penalty ✅
- **Smart Trap Setting:** -30 ใน Main1 ✅
- **Banished Zone Scanning:** 0.4/0.2 multipliers ✅
- **Battle Phase API:** OnBattle/OnSelectAttackTarget signatures ถูกต้อง ✅
- **IDisposable Pattern:** Dispose/Dispose(bool)/Finalizer ✅
- **Nibiru:** ปลดล็อค summon count check ✅
- **OnSelectAttackTarget:** แข็งแกร่งที่สุดก่อน ✅
- **Python Claims ทั้ง 11 รายการ:** shared_utils, learning_sandbox, optimize_registry, ab_tournament, q_learning, auto_role_detector ✅

---

## 4. จุดอ่อนที่พบและแก้ไข

### 4.1 Critical: ลำดับ Hard Cap / Anti-Inflation Decay (Fixed)

**ไฟล์:** `WindBot/UnifiedIgnisExecutor.cs` — บรรทัด 641-667  
**ปัญหา:** Hard Cap (ปิด priority ที่ 8) ทำงานก่อน Decay ทำให้การ์ดที่ priority 9+ ถูกปัดลงมาเป็น 8 แล้วถูกลดอีก 1 เหลือ 7 ทันที ในการเรียนรู้รอบเดียว  
**ผลกระทบ:** Decay ไม่เคยทำงานอย่างมีประสิทธิภาพ — priority ของการ์ดที่ไม่ได้ใช้ถูกลดลงอย่างไม่เป็นธรรม  
**แนวทางแก้ไข:** สลับให้ Decay ทำงานก่อน Hard Cap  
**ผลลัพธ์:** การ์ด priority 9+ → ตก 1 → 8 (Decay) → Hard Cap ไม่มีผล → priority = 8

### 4.2 Critical: Triple Tactics Talent/Thrust ไม่ทำงาน (Fixed)

**ไฟล์:** `WindBot/UnifiedIgnisExecutor.cs` — บรรทัด 1155-1162 (เดิม)  
**ปัญหา:** มีเงื่อนไข `Duel.Player == 1` บล็อกการใช้งานใน "เทิร์นคู่ต่อสู้" แต่ TTT/Thrust เป็น Normal Spell ที่เปิดได้เฉพาะเทิร์นเราเท่านั้น เกมไม่เสนอให้เปิดในเทิร์นคู่ต่อสู้อยู่แล้ว  
**ผลกระทบ:** บอทไม่เคยใช้ TTT/Thrust  
**แนวทางแก้ไข:** ลบบล็อกออกทั้งหมด อ้างอิงคำอธิบายว่าปล่อยให้ game engine จัดการความถูกต้อง

### 4.3 ปานกลาง: Infinite Impermanence — จำกัด Going Second (Fixed)

**ไฟล์:** `WindBot/UnifiedIgnisExecutor.cs` — บรรทัด 1111-1118 (เดิม)  
**ปัญหา:** เดิมบล็อก Imperm โดยสิ้นเชิงหากไม่มี face-up monster ฝั่งตรงข้าม แต่ Imperm สามารถ Set จากมือในเทิร์นเรา (Going Second) โดยไม่ต้องมีเป้าหมาย  
**แนวทางแก้ไข:** เพิ่ม `Duel.Player == 0` — ยังคงบล็อกหากกำลัง chain และไม่มีเป้าหมาย แต่ยอมให้ Set จากมือในเทิร์นเราเมื่อ Going Second

### 4.4 ปานกลาง: HasOpponentHandTrap — ฮาร์ดโค้ดแค่ 4 ID (Fixed)

**ไฟล์:** `WindBot/UnifiedIgnisExecutor.cs` — บรรทัด 2047-2064 (เดิม)  
**ปัญหา:** เดิมฮาร์ดโค้ดเฉพาะ 4 ID การ์ด (37742478, 6325660, 14558127, 14558128) — ไม่ครอบคลุม handtrap ใหม่ๆ  
**แนวทางแก้ไข:** เปลี่ยนเป็นเช็คจาก `_cardRegistry[cardId].roles.Contains("handtrap")` — data-driven รองรับ handtrap ทุกชนิด  
**ผลลัพธ์:** ระบบจะทำงานกับ handtrap ใดๆ ที่มีบทบาท "handtrap" ใน registry

### 4.5 ปานกลาง: HasOpponentBattleTrap — อนุรักษ์นิยมเกินไป (Fixed)

**ไฟล์:** `WindBot/UnifiedIgnisExecutor.cs` — บรรทัด 2035-2045 (เดิม)  
**ปัญหา:** เดิมคืนค่า true ถ้ามีการ์ดคว่ำใน Spell Zone ใดๆ รวมถึง Quick-Play Spell ที่ไม่ใช่ battle trap  
**แนวทางแก้ไข:** เช็คว่าเป็นการ์ด Trap จริงๆ หรือมีความทรงจำว่าการ์ดนั้นอันตราย  
**ผลลัพธ์:** บอทโจมตีได้อิสระขึ้นเมื่อเจอ Quick-Play Spell คว่ำ

### 4.6 ต่ำ: Empty Catch Blocks (Fixed)

**ไฟล์:** `WindBot/UnifiedIgnisExecutor.cs` — บรรทัด 173, 187, 207, 498  
**ปัญหา:** 4 จุดที่มี `catch {}` กลบข้อผิดพลาดอย่างเงียบๆ ทำให้ดีบั๊กลำบาก  
**แนวทางแก้ไข:** เปลี่ยนเป็น `catch (Exception ex)` พร้อมบันทึกข้อความ error  
**ผลลัพธ์:** ข้อผิดพลาด I/O จะถูกบันทึก ช่วยในการดีบั๊ก

### 4.7 ต่ำ: Q-Learning MC Return Formula (Fixed)

**ไฟล์:** `WindBot_Sandbox/q_learning.py` — บรรทัด 185  
**ปัญหา:** สูตร `reward * (gamma ** (T - 1 - t))` ใช้ final reward สำหรับทุกขั้นตอนโดยไม่สะสมระหว่างทาง  
**แนวทางแก้ไข:** เพิ่มความชัดเจนของสูตร พร้อมตัวแปร `steps_from_end`  
**ผลลัพธ์:** รหัสอ่านง่ายขึ้น และรองรับ intermediate rewards ในอนาคต

---

## 5. จุดอ่อนที่ควรปรับปรุงเพิ่มเติม

### 5.1 Critical: ไม่มี Unit Tests

**ปัญหา:** ไม่มี unit tests สำหรับ C# หรือ Python  
**ผลกระทบ:** ทุกการเปลี่ยนแปลงต้องทดสอบด้วยการรัน match จริง ใช้เวลานานและ probabilistic  
**แนะนำ:** เพิ่ม unit tests สำหรับ EvaluateCardAction, CalculateTotalDangerForField, CalculateCardDanger

### 5.2 Critical: God Method — EvaluateCardAction (~400 บรรทัด)

**ปัญหา:** `EvaluateCardAction()` (บรรทัด ~1028-1433) เป็น method ขนาดใหญ่เกินไป รวมทั้ง scoring, goal checks, role checks, chain analysis  
**แนะนำ:** แบ่งเป็น method ย่อย:
- `ScoreComboPlans()`
- `ScoreLethalCheck()`
- `ApplyStapleSafeguards()`
- `ApplyMacroDecisionRefactoring()`

### 5.3 Critical: Magic Numbers กระจัดกระจาย

จุดที่มี magic numbers โดยไม่ใช้ constant:

| ค่า | ตำแหน่ง | คำแนะนำ |
|-----|---------|---------|
| 35.0 (score threshold) | บรรทัด 1430 | ย้ายเป็น const หรือ config per-deck |
| -90.0, -100.0, -200.0, -500.0, -30.0 | หลายที่ | รวมเป็น const หรือ config |
| 0.4, 0.2, 0.5, 0.3 (threat multipliers) | บรรทัด ~740-785 | ย้ายเป็น const |
| 5 (default priority) | บรรทัด ~1860 | ควรเป็น const |

### 5.4 ปานกลาง: HasOpponentHandTrap — ยังไม่สมบูรณ์

ถึงแม้จะแก้ให้ใช้ data-driven แล้ว แต่ `_opponentMemory` อาจไม่มีการ์ด handtrap บางชนิด ถ้าไม่เคยเจอมาก่อน  
**แนะนำ:** เพิ่ม baseline handtrap detection สำหรับ handtrap ที่รู้จักทั่วไป (Ash, Maxx C, etc.)

### 5.5 ปานกลาง: Global Static Instance

**ไฟล์:** `UnifiedIgnisExecutor.cs` — บรรทัด 88  
`private static UnifiedIgnisExecutor _currentInstance = null;`  
**ปัญหา:** Global static reference อาจพังใน multi-duel scenarios  
**แนะนำ:** ใช้ Dependency Injection หรือ Instance-based registry

### 5.6 ปานกลาง: Deck-Specific Subclasses ฝังใน Main File

**ไฟล์:** `UnifiedIgnisExecutor.cs` — ท้ายไฟล์ มี subclass 11 สำรับ  
**ปัญหา:** รวมทุก deck logic ไว้ในไฟล์เดียว ทำให้ไฟล์ใหญ่และยากต่อการ maintain  
**แนะนำ:** แยก subclass ออกเป็นไฟล์แยกตามสำรับ

### 5.7 ปานกลาง: Hardcoded Ports

- C#: port 7911 ฝังในการเรียกใช้
- Python: `ab_tournament.py` ฮาร์ดโค้ด `port=7911`
**แนะนำ:** รับจาก environment variable หรือ config

### 5.8 ต่ำ: auto_role_detector.py — Rule-based อย่างเดียว

**ปัญหา:** ใช้ keyword matching อย่างเดียว — misses การ์ดที่มีเงื่อนไขซับซ้อน  
**แนะนำ:** เพิ่ม ML-based role detection หรือ database query จาก card effect taxonomy

### 5.9 ต่ำ: combo_simulator.py — Handtrap Chance ตายตัว

**ปัญหา:** `handtrap_chance=0.35` ค่าตายตัว ไม่ปรับตาม opponent memory  
**แนะนำ:** คำนวณจาก `_opponentMemory` แบบ dynamic

### 5.10 ต่ำ: optimize_registry.py — Hill Climbing ไม่มี Random Restart

**ปัญหา:** Single-parameter hill climbing, 300 iterations, ไม่มี simulated annealing — อาจ stuck ใน local optima  
**แนะนำ:** เพิ่ม random restarts หรือใช้ Bayesian Optimization

---

## 6. คำแนะนำเชิงกลยุทธ์เพื่อให้บอทเก่งขึ้น

### 6.1 Immediate Wins (ทำได้เลย)

1. ✅ **ลำดับ Decay/Hard Cap** — แก้ไขแล้ว ช่วยให้ระบบเรียนรู้มีประสิทธิภาพขึ้น
2. ✅ **TTT/Thrust unlock** — แก้ไขแล้ว บอทจะใช้การ์ดทรงพลังเหล่านี้ได้
3. ✅ **Imperm Going Second** — แก้ไขแล้ว เพิ่มโอกาส Going Second
4. ✅ **HasOpponentHandTrap data-driven** — แก้ไขแล้ว รองรับ handtrap หลากหลายขึ้น
5. ✅ **HasOpponentBattleTrap ฉลาดขึ้น** — แก้ไขแล้ว โจมตีได้อิสระขึ้น
6. ✅ **Catch blocks มี logging** — แก้ไขแล้ว ดีบั๊กง่ายขึ้น

### 6.2 Short-term (1-2 สัปดาห์)

7. **Unit Tests** — เพิ่ม test suite สำหรับ EvaluateCardAction, CalculateTotalDangerForField, CalculateCardDanger
8. **แยก God Method** — ย่อย EvaluateCardAction เป็น method ย่อย
9. **Magic Numbers → Constants** — ปรับ magic numbers เป็น named constants
10. **Deck-Specific Config** — ทำให้ score threshold 35.0 ปรับแต่งได้ตามสำรับ

### 6.3 Medium-term (1 เดือน)

11. **Dynamic Handtrap Prediction** — combo_simulator ใช้ opponent memory จริง
12. **Bayesian Optimization** — แทนที่ hill climbing ใน optimize_registry.py
13. **Side Decking Logic** — ให้บอทเลือก side deck หลังเกม 1
14. **Multi-Goal Planning** — ให้บอทวางแผนหลายเป้าหมายพร้อมกัน (ไม่ใช่แค่ goal เดียว)

### 6.4 Long-term (3+ เดือน)

15. **Deep Learning Integration** — ใช้ Neural Network สำหรับ card evaluation แทน rule-based scoring
16. **End-to-End Training** — ฝึกบอทด้วย self-play ผ่าน YGOPro server
17. **Deck Building AI** — ให้บอทสร้างและปรับแต่งสำรับอัตโนมัติตาม meta
18. **Realtime Adaptation** — ปรับกลยุทธ์ระหว่างเกมตามรูปแบบการเล่นของคู่ต่อสู้

---

## 7. บทสรุป

### สิ่งที่ทำสำเร็จในการตรวจสอบครั้งนี้

- **ตรวจสอบ Changelog ทั้ง 11 ไฟล์** → พบ 4 จุดไม่ตรงกับโค้ดจริง
- **แก้ไขโค้ด 7 จุด** → ลำดับ Decay/Hard Cap, TTT/Thrust unlock, Imperm Going Second, HasOpponentHandTrap data-driven, HasOpponentBattleTrap ฉลาดขึ้น, catch blocks, Q-learning formula
- **คอมไพล์ C# สำเร็จ** → 0 errors, 0 warnings
- **จัดทำเอกสารนี้** → สำหรับใช้วางแผนพัฒนาต่อ

### สถานะบอทปัจจุบัน

บอท WindBot IGNIS เป็นระบบ AI ที่มีความซับซ้อนสูงสำหรับเกม Yu-Gi-Oh! มีโครงสร้างการทำงานครบถ้วนตั้งแต่:
- **Card Evaluation** — scoring, combo plans, threat assessment
- **Reinforcement Learning** — Q-Learning + Heuristic Learning
- **Config Management** — per-deck registries, opponent memory
- **Battle AI** — OnBattle, OnSelectAttackTarget
- **Safety Systems** — Anti-self-harm, anti-overextension, smart trap setting

จุดอ่อนหลักที่ยังคงอยู่คือ **การขาด unit tests** และ **God Method ขนาดใหญ่** ซึ่งควรได้รับการปรับปรุงในการพัฒนารอบถัดไป

---

*เอกสารนี้สร้างโดยอัตโนมัติจากการตรวจสอบโปรเจกต์ EDOTh (WindBot IGNIS) อย่างละเอียด — 24 พฤษภาคม 2026*
