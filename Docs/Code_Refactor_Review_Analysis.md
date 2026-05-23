# รายงานการวิเคราะห์และตรวจสอบโค้ดเพื่อการปรับปรุงระบบ (Code Refactor, Review & Dead Code Analysis)
**อัปเดตล่าสุด:** 2026-05-23 | **สำหรับโปรเจกต์:** WindBot IGNIS Engine

---

## 📌 บทนำและภาพรวม (Introduction & Overview)

จากการวิเคราะห์โครงสร้างโค้ดทั้งหมดในโปรเจกต์ ทั้งฝั่ง **C# Core Engine (`UnifiedIgnisExecutor.cs`)** และ **Python Sandbox (`WindBot_Sandbox/`)** พบจุดที่สามารถปรับปรุง (Refactor) รวมถึงจุดที่เป็นโค้ดที่ไม่ได้ใช้งานจริง (Dead Code), ตัวแปร/ไลบรารีส่วนเกิน (Unused Imports & Variables) และพบบั๊กที่อาจทำให้ระบบหยุดทำงาน (Runtime Crash) ซึ่งได้สรุปรายละเอียดทั้งหมดลงในเอกสารฉบับนี้

---

## 1. 🚨 ปัญหาสำคัญระดับวิกฤต (Critical Issues Found)

### 🔴 1.1 บอทเด็ค 2026 ทั้ง 4 เด็คใช้งานการ์ดหลักไม่ได้เลย (Bricked Decks)
**ไฟล์ที่เกี่ยวข้อง:** `WindBot/config/cards_registry_2026_*.json` (เด็ค Goldlord, Invoke, Kwtune, Labrynth)

* **รายละเอียด:**
  ไฟล์การตั้งค่าการ์ด (Card Registries) ของเด็คทั้ง 4 ได้แก่ `2026_Goldlord`, `2026_Invoke`, `2026_Kwtune`, และ `2026_Labrynth` เป็นเพียงไฟล์ที่คัดลอก (Copy-paste) มาจากไฟล์เริ่มต้น `cards_registry.json` โดย**ไม่มีการลงทะเบียนการ์ดประจำเด็คตัวเอง**เข้าไปเลย
* **ผลกระทบ:**
  มีการ์ดหลักจำนวน **60 ใบ** ที่ถูกใช้ในเด็คจริงแต่ไม่ปรากฏใน Registry (เช่น `Eldlich the Golden Lord`, `Aleister the Invoker`, `Arianna the Labrynth Servant`, และการ์ดเวทมนตร์/กับดักเฉพาะทางอื่นๆ) 
  เมื่อบอทนำเด็คเหล่านี้ไปเล่น การ์ดเหล่านั้นจะตกลงสู่ **OnDefaultActivate / OnDefaultSummon / OnDefaultSpSummon** และถูกตัดสินด้วย **Iron Rule #4 (Fallback must be false always)** ส่งผลให้บอท**ไม่สามารถเปิดใช้งานหรืออัญเชิญการ์ดหลักเหล่านั้นได้เลยตลอดทั้งเกม (การ์ดกลายเป็นใบ้ทั้งหมด)**
* **แนวทางแก้ไข:**
  ต้องรันโปรแกรมตรวจจับและลงทะเบียนการ์ดอัตโนมัติสำหรับเด็คทั้ง 4 ดังนี้:
  ```bash
  python auto_role_detector.py --deck 2026_Goldlord
  python auto_role_detector.py --deck 2026_Invoke
  python auto_role_detector.py --deck 2026_Kwtune
  python auto_role_detector.py --deck 2026_Labrynth
  ```

---

### 🔴 1.2 บั๊กการ Unpack ในทัวร์นาเมนต์ A/B ส่งผลให้โปรแกรมแครช (Runtime Crash)
**ไฟล์ที่เกี่ยวข้อง:** [ab_tournament.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/ab_tournament.py)
* **รายละเอียด:**
  ฟังก์ชัน `parse_match_outcome` ใน [ab_tournament.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/ab_tournament.py#L45-L75) มีการคืนค่า (Return) 2 แบบที่จำนวนสมาชิกไม่เท่ากัน:
  * หากพบไฟล์สรุป: คืนค่าเป็น Tuple 4 ตัวอักษร `return outcome, bot_lp, opp_lp, turns` (บรรทัดที่ 71, 73, 75)
  * หาก**ไม่พบ**ไฟล์สรุป: คืนค่าเพียง Tuple 3 ตัวอักษร `return "Unknown", 0, 0` (บรรทัดที่ 48)
* **ผลกระทบ:**
  ในฟังก์ชัน `main` บรรทัดที่ 272 มีการเรียกรับค่าด้วยตัวแปร 4 ตัว:
  ```python
  outcome, bot_lp, opp_lp, turns = parse_match_outcome(new_log_dir)
  ```
  หากแมตช์ใดเกิดข้อผิดพลาดและไม่มีการเขียนไฟล์สรุปขึ้นมา โปรแกรมจะเกิดข้อผิดพลาด **`ValueError: not enough values to unpack (expected 4, got 3)`** และส่งผลให้ระบบทัวร์นาเมนต์ A/B หยุดทำงานทันที (Crash)
* **แนวทางแก้ไข:**
  แก้ไขบรรทัดที่ 48 ใน `ab_tournament.py` ให้ส่งกลับ 4 ค่าให้ถูกต้อง:
  ```python
  return "Unknown", 0, 0, 0
  ```

---

## 2. 🔍 การวิเคราะห์ Dead Code และไลบรารีส่วนเกิน (Unused Imports & Variables)

จากการตรวจสอบอย่างละเอียด พบไลบรารี (Imports) และตัวแปรระดับ Global ที่ถูกประกาศทิ้งไว้โดยไม่เคยถูกเรียกใช้เลยในไฟล์ต่างๆ ดังนี้:

### 🐍 2.1 ฝั่ง Python Sandbox Scripts
| ไฟล์ | รายการ Dead Code | ประเภท | ผลกระทบและข้อแนะนำ |
|---|---|---|---|
| [auto_role_detector.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/auto_role_detector.py) | `import re` (บรรทัดที่ 4) | Unused Import | ไม่จำเป็นต้องนำเข้า เนื่องจากใช้สตริงแมทช์ปกติแทนเร็กเอ็กซ์ (ควรนำออก) |
| [cockpit.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/cockpit.py) | `REGISTRY_PATH` (บรรทัดที่ 17) | Unused Global Variable | ไม่เคยใช้ ตัวแปรนี้ถูกทับด้วย `sandbox_reg` ภายใน API handlers อยู่แล้ว (ควรนำออก) |
| [ab_tournament.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/ab_tournament.py) | `import json` (บรรทัดที่ 3) | Unused Import | ไม่จำเป็นต้องนำเข้า เนื่องจากใช้ `shutil` ในการสำเนาไฟล์โดยตรง (ควรนำออก) |
| [optimize_registry.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/optimize_registry.py) | `import json` (บรรทัดที่ 2) | Unused Import | ไฟล์เรียกใช้ตัวแปรผ่าน `load_registry_list` และ `save_registry_list` ของ `shared_utils.py` อยู่แล้ว (ควรนำออก) |
| [learning_sandbox.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/learning_sandbox.py) | `import glob` (บรรทัดที่ 4) | Unused Import | ไฟล์ใช้ `os.listdir` ในการค้นหาโฟลเดอร์อยู่แล้ว ไม่เคยเรียกใช้ไลบรารี `glob` (ควรนำออก) |
| [query_deck_ids.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/query_deck_ids.py) | `import sys` (บรรทัดที่ 3) | Unused Import | ไม่มีการใช้คำสั่งระบบหรือปิดโปรแกรมผ่าน `sys` ในไฟล์นี้ (ควรนำออก) |

### 📁 2.2 สคริปต์เสริมที่ไม่ได้เชื่อมต่อกับระบบควบคุม (Standalone/Scratch Scripts)
สคริปต์เหล้านี้ไม่ได้ถูกเรียกใช้โดย Cockpit Dashboard หรือ Pipeline การเรียนรู้หลักของบอทเลย สามารถย้ายไปอยู่ในโฟลเดอร์ `scratch/` หรือนำออกได้หากไม่มีการใช้งานแบบ Manual:
1. **`query_db.py`**: ใช้คิวรี่ข้อมูลหาชื่อการ์ดเฉพาะบางใบแบบเจาะจง
2. **`query_deck_ids.py`**: ใช้ดึงรายละเอียดของการ์ดกลุ่มที่ระบุไอดีไว้ล่วงหน้าเพื่อสร้างคู่มือ

---

## 3. 🛡️ ฝั่ง C# Engine (`UnifiedIgnisExecutor.cs`)

ในฝั่ง C# นั้น ตัวแปรและฟังก์ชันทั้งหมดเขียนค่อนข้างกระชับและเกือบทั้งหมดผ่านการเรียกใช้งานโดยตรงจาก WindBot Engine หรือใช้ในการคำนวณ scoring แต่พบปัญหาเชิงลอจิกและจุดที่ยังปรับปรุงได้ดังนี้:

### ⚠️ 3.1 การสลับขั้วของระบบลดระดับความสำคัญการ์ด (Anti-Inflation Decay vs Hard Cap)
* **ตำแหน่งโค้ด:** `ApplyRealTimeLearning()` บรรทัดที่ 608–634
* **ปัญหา:**
  โค้ดรัน **Anti-Inflation Decay** ก่อน แล้วจึงตามด้วย **Hard Cap**
  * หากการ์ดมีระดับความสำคัญ (Priority) เท่ากับ 9 และไม่ถูกเล่นในแมตช์นั้น ระบบ Decay จะลดระดับความสำคัญลงเหลือ 8
  * ต่อมาใน Hard Cap จะทำการตัดค่าที่เกิน 8 ให้เหลือ 8 อีกครั้ง
  * ส่งผลให้ถ้ามีการปรับเพิ่มคะแนนจนทะลุ 8 (Inflation) ระบบจะไม่สามารถย้อนคืนสู่ระดับต่ำได้อย่างมีนัยสำคัญ เพราะ Decay จะถูกหักล้างอย่างสิ้นเชิง
* **แนวทางแก้ไข:**
  ควรย้ายขั้นตอน Hard Cap ขึ้นมาก่อนการทำ Decay เพื่อให้คะแนนที่เหลือเกินถูกกรองออกก่อน จากนั้นระบบ Decay จึงจะมีผลจริงในการปรับลดลงมาเหลือ 7 หรือต่ำกว่าได้

### ⚠️ 3.2 ข้อจำกัดความสามารถของ Effect Veiler
* **ตำแหน่งโค้ด:** `EvaluateCardAction()` บรรทัดที่ 991
* **ปัญหา:**
  จำกัดให้ใช้ได้เฉพาะ `Duel.Phase == DuelPhase.Main1` หรือ `DuelPhase.Main2` 
* **แนวทางแก้ไข:**
  สามารถเปลี่ยนไปใช้ `!Duel.IsMainPhase()` เพื่อให้โค้ดดูอ่านง่ายและสอดคล้องกับมาตรฐานฟังก์ชันอื่นๆ ของระบบ YGOSharp

---

## 4. 📊 สถิติตัวเลขของการ์ดที่ตรวจสอบ (Card Registry Integrity Stats)

| ตัวชี้วัดการตรวจสอบ | จำนวน | คำอธิบาย |
|---|---|---|
| **การ์ดทั้งหมดที่ลงทะเบียนใน `cards_registry.json`** | **160 ใบ** | ฐานข้อมูลการ์ดตั้งต้นของระบบควบคุม |
| **การ์ดที่มีการเรียกใช้งานจริงในเด็ค 2026** | **94 ใบ** | การ์ดใน Registry ที่มีการนำไปประกอบอยู่ในเด็ค 2026 เด็คใดเด็คหนึ่ง |
| **การ์ดที่ลงทะเบียนแต่ไม่ได้ใช้ในเด็ค 2026** | **66 ใบ** | การ์ดเสริมหรือการ์ดของฝ่ายตรงข้าม (เช่น `Baronne de Fleur`, `S:P Little Knight`) เพื่อคำนวณค่าความอันตราย (Danger Level) ซึ่งถือเป็นลอจิกป้องกันตัวบอท |
| **การ์ดในเด็คที่ไม่ได้ลงทะเบียน (Bricked)** | **60 ใบ** | การ์ดประจำเด็ค Goldlord, Invoke, Kwtune, Labrynth ที่ลืมอัปเดต registry ทำให้บอทใช้งานไม่ได้ |

---

## 💡 สรุปข้อเสนอแนะในการปรับปรุงระบบ (Summary of Action Items)

1. **ด่วนที่สุด (Urgent Fix):** ทำการรัน `auto_role_detector.py` สำหรับเด็ค Goldlord, Invoke, Kwtune, และ Labrynth เพื่อคืนชีพให้บอทสามารถรันคอมโบของตัวเองได้
2. **แก้ไขบั๊กใน `ab_tournament.py`:** แก้ไขการ return ของฟังก์ชัน `parse_match_outcome` บรรทัดที่ 48 ให้เป็น `return "Unknown", 0, 0, 0`
3. **Refactor ส่วนของ Python Scripts:** ทำการลบ `import` ที่ไม่ได้ใช้ออกเพื่อความสะอาดของโค้ดและลดภาระการโหลดโมดูลที่ไม่จำเป็น
4. **ปรับเปลี่ยน C# Learning Order:** ปรับลำดับการทำงานใน `ApplyRealTimeLearning()` ให้รัน Hard Cap ก่อนแล้วค่อยตามด้วย Decay เพื่อแก้ปัญหาคะแนนสะสมค้าง (Priority Lock-in at 8)
