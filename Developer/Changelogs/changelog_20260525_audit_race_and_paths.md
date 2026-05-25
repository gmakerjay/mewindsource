# Changelog — 25 พฤษภาคม 2026 (รอบค่ำ)
**เวลา:** 2026-05-25 (Race Condition, MC Discount, and Path Portability Fixes)
**ผู้แก้ไข:** Antigravity AI Coding Assistant
**ขอบเขต:** WindBot_Sources (C#) & WindBot_Sandbox / scratch (Python)

---

## 🔴 CRITICAL FIXES (การแก้ไขระดับวิกฤต)

### 1. แก้ไข Race Condition ใน MonitorLP thread (C#)
- **ไฟล์แก้ไข:** [BaseCustomExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/Developer/WindBot_Sources/BaseCustomExecutor.cs) (บรรทัด 191–219)
- **ปัญหา:** Thread เบื้องหลัง `MonitorLP()` อ่านค่า `Duel` fields และสั่งเขียนไฟล์ log/registry ใน `ApplyRealTimeLearning()` พร้อมกันกับ Duel thread หลักโดยไม่มีการ Lock ข้อมูล ส่งผลให้ข้อมูล registry ทับกันหรือเสียหาย (Data Corruption) หรือบอทแครชกะทันหัน
- **การแก้ไข:** ครอบโค้ดส่วนเช็กสภาพบอร์ดและการสั่งอัปเดต LP/Learning ทั้งหมดใน `MonitorLP` ด้วย block `lock (_staticLock)` อย่างปลอดภัย

### 2. แก้ไขการสลับขั้ว discount ของ Monte Carlo ใน q_learning.py (Python)
- **ไฟล์แก้ไข:** 
  - [WindBot_Sandbox/q_learning.py](file:///c:/Users/admin/Documents/EDOTh/Developer/WindBot_Sandbox/q_learning.py) (บรรทัด 174–176)
  - [scratch/q_learning.py](file:///c:/Users/admin/Documents/EDOTh/Developer/scratch/q_learning.py) (บรรทัด 174–176)
- **ปัญหา:** สูตรคำนวณ MC return มีทิศทางการลดทอนมูลค่ากลับขั้ว ($G_t = \text{reward} \times \gamma^{T-1-t}$) ส่งผลให้การตัดสินใจช่วงท้ายเกม (เช่น การโจมตีธรรมดาเพื่อปิดเกม) ได้รับเครดิตเต็มที่ ในขณะที่การตัดสินใจช่วงต้นเกม (การรันคอมโบเปิดบอร์ดใน Turn 1/2) โดน discount อย่างรุนแรงจนบอทไม่เรียนรู้แนวทางรันคอมโบที่เหมาะสม
- **การแก้ไข:** ปรับปรุงสูตรให้ discount ตามระยะห่างจากต้นเกม: `steps_from_start = t` และ $G_t = \text{reward} \times \gamma^t$

### 3. แก้ไข Hardcoded paths ทั้งหมดเพื่อรองรับการทำงานข้ามเครื่อง (Portability)
- **ไฟล์แก้ไข:**
  - [scratch/save_outcomes_to_sql.py](file:///c:/Users/admin/Documents/EDOTh/Developer/scratch/save_outcomes_to_sql.py) (บรรทัด 155, 169)
  - [scratch/run_multi_iterations.py](file:///c:/Users/admin/Documents/EDOTh/Developer/scratch/run_multi_iterations.py) (บรรทัด 11)
  - [WindBot_Sandbox/find_field_locks.py](file:///c:/Users/admin/Documents/EDOTh/Developer/WindBot_Sandbox/find_field_locks.py) (บรรทัด 6)
- **ปัญหา:** มีการระบุพาธแบบ absolute เช่น `c:\Users\admin\Documents\EDOTh` ทำให้โปรแกรมไม่สามารถรันบนเครื่องอื่นหรือรันใน CI/CD pipeline ได้
- **การแก้ไข:** เปลี่ยนไปใช้การหาพาธแบบไดนามิกโดยอิงจากตำแหน่งของไฟล์รันปัจจุบัน (เช่น `os.path.dirname(...)` ของ `__file__`)

---

## 🟡 MEDIUM & LOW FIXES (การแก้ไขระดับปานกลางและต่ำ)

### 4. ลบการลงทะเบียนการ์ดซ้ำซ้อนใน DreadnoughtExecutor.cs (C#)
- **ไฟล์แก้ไข:** [DreadnoughtExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/Developer/WindBot_Sources/DreadnoughtExecutor.cs) (บรรทัด 27–28)
- **ปัญหา:** มีการเรียกใช้คำสั่ง `AddExecutor` สำหรับการ์ด `Called by the Grave` (ID 24224830) ซ้อนกันสองบรรทัดโดยไม่จำเป็น
- **การแก้ไข:** ลบคำสั่ง `AddExecutor` บรรทัดที่สองที่เป็นการซ้ำซ้อนออกไป

### 5. เพิ่มคำอธิบายการออกแบบของ subclass stubs ใน UnifiedIgnisExecutor.cs (C#)
- **ไฟล์แก้ไข:** [UnifiedIgnisExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/Developer/WindBot_Sources/UnifiedIgnisExecutor.cs) (บรรทัด 20)
- **ปัญหา:** มี stubs ว่างของมอนสเตอร์และเด็คที่ดูเหมือนไม่มีการใช้งาน
- **การแก้ไข:** เพิ่มเอกสารอธิบายการออกแบบ (Design note) ชี้แจงว่า stubs เหล่านี้ทำงานผ่านการโหลด dynamic registry แบบ config-driven อยู่แล้วจาก `BaseCustomExecutor`

---

## 📊 Verification Results (ผลการทดสอบและยืนยัน)

1. **C# Compilation:** รันคำสั่งคอมไพล์ผ่าน `compile_ai.bat` สำเร็จเรียบร้อย แสดงผล `Compilation SUCCESSFUL!` โดยไม่มี Compiler errors
2. **Git status:** ไฟล์ทั้งหมดอยู่ในสถานะพร้อมทำการ Commit
