# Changelog — 25 พฤษภาคม 2026 (ระบบ Logger เรียลไทม์ และแก้ไขบั๊กบอทค้างเทิร์น 11)
**เวลา:** 2026-05-25 (Real-time Logger & Cockpit Deadlock Fix)  
**ผู้ปฏิบัติการ:** Antigravity AI Agent  
**ขอบเขต:**
* [BaseCustomExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/BaseCustomExecutor.cs)
* [PureYummyExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/PureYummyExecutor.cs)
* [cockpit.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/cockpit.py)
* [System_Architecture_and_Cockpit_Guide.md](file:///c:/Users/admin/Documents/EDOTh/Docs/System_Architecture_and_Cockpit_Guide.md)

---

## 🟢 1. อัปเดตระบบ Logger และการติดตามผลแบบเรียลไทม์ (Real-time Logger Implementation)
### รายละเอียด
* แก้ไขฟังก์ชันการทำงานของ `LogToMatch`, `LogToTurn` และ `LogDecision` ใน [BaseCustomExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/BaseCustomExecutor.cs) ให้ทำการเขียนข้อมูลออกทาง Console Standard Output (`Console.WriteLine`) ทันทีแบบเรียลไทม์ พร้อม Prefix `[IgnisEngine]` เพื่อแสดงชัดเจนในหน้าต่างเทอร์มินัล
* เพิ่มการล็อกการตอบรับคำถาม Trigger เสริม (Yes/No Option) ใน [PureYummyExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/PureYummyExecutor.cs)
* ปรับปรุงหน้าจอ Progress บนแดชบอร์ด Cockpit โดยทำการกรอง (Filter) ข้อความเอาเฉพาะแถวที่มี `[IgnisEngine]` เพื่อแสดงผลการคำนวณและทิศทางคอมโบของ AI ตัวใหม่แบบเรียลไทม์ และซ่อนข้อความรายงานสถานะบอร์ดและการทำตามคำสั่งเกมของเครื่อง YGOPro/EDOPro ดั้งเดิมที่ซ้ำซ้อนและยาวเกินไป

---

## 🔴 2. แก้ไขบั๊กตัวรันบอทค้างรอบเทิร์นยาว (Turn 11 Hang Deadlock Fix)
### ปัญหาที่พบ
* บอทค้างคาในเทิร์นที่ 11 ระหว่างสั่งพิมพ์สถานะการ์ดในสุสาน (Graveyard Logging) โดยไม่มีการแจ้งเตือน Error และโปรเซสค้างคาอยู่ใน Memory ไม่ยอมปิดลง
### สาเหตุ (Root Cause)
* การเรียกเขียน Log ลงสตรีม stdout (`Console.WriteLine`) ทำงานแบบซิงโครนัส ในขณะเดียวกัน Python Web UI มีการเข้าถึงและเปิดอ่านเขียนไฟล์ `training_progress.log` จากหลายเธรดชนกัน (เธรด HTTP Request `/api/progress` และเธรดดักจับ stdout ของทั้งบอทเราและคู่ซ้อม) ส่งผลให้เกิด **Sharing Violation (`PermissionError: [WinError 32]`)** บน Windows และเธรดดักอ่าน stdout ใน Python หยุดทำงาน
* เมื่อไม่มีใครอ่านบัฟเฟอร์ stdout ของ `WindBot.exe` บัฟเฟอร์ของระบบจะเต็ม ทำให้ `WindBot.exe` ค้างคาอยู่ที่คำสั่งพิมพ์สตรีมและแขวนการทำงานไปตลอดกาล
### การแก้ไข
* นำเธรดล็อกกลาง **`progress_log_lock = threading.Lock()`** เข้ามาจัดระเบียบใน [cockpit.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/cockpit.py)
* สร้างฟังก์ชันการอ่าน/เขียนไฟล์ที่ปลอดภัย **`write_progress_log`** และ **`read_progress_log`** เพื่อครอบการเปิดไฟล์ด้วยบล็อกลองทำซ้ำ (Retry Loop 10 ครั้ง ดีเลย์ 0.05 วินาที) หากพบว่าไฟล์กำลังโดนล็อกโดยโปรเซสอื่น
* ปิดตัวรันบอท `WindBot.exe` ตัวเก่าทั้งหมดที่ค้างอยู่ในระบบ

---

## 📖 3. ปรับปรุงเอกสารคู่มือระบบ (System Architecture & Cockpit Guide Documentation)
* ปรับปรุงเอกสาร [System_Architecture_and_Cockpit_Guide.md](file:///c:/Users/admin/Documents/EDOTh/Docs/System_Architecture_and_Cockpit_Guide.md) ให้สมบูรณ์ครอบคลุม:
  * ทุกคุณสมบัติ, คลาส และฟังก์ชัน API ของ WindBot Core C#
  * ทุกช่องทาง REST HTTP API endpoints บน Python Cockpit Backend
  * คู่มือขั้นตอนละเอียด 7 ขั้นตอน (Step-by-Step Guideline) สำหรับการเขียนและลงทะเบียนลอจิกสำหรับเด็คใหม่ในอนาคต

---

## 📊 ผลการตรวจสอบและคอมไพล์ (Verification Results)
* **C# Compilation:** ✅ ผ่านสำเร็จอย่างสมบูรณ์ ไร้ข้อผิดพลาดและคำเตือน (`Compilation SUCCESSFUL!`)
* **Process Clean:** ✅ ล้างบอทค้างทั้งหมด และระบบพร้อมเปิดรันใหม่อย่างราบรื่น
