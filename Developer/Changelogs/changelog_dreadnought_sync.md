# บันทึกการเปลี่ยนแปลง (Changelog) — 25 พฤษภาคม 2026 (รอบดึก)
**หัวข้อ:** การแก้ไขสคริปต์การ์ด Dreadnought, การซิงค์โค้ด C# และการจัดการระดับโฟลเดอร์สำหรับจำลองสถานการณ์การเล่นบอท (Simulation)

---

## 🛠️ รายละเอียดการปรับปรุงระบบและการแก้ไขบั๊ก

### 1. การแก้ไขปัญหาเกมค้าง (Crash) จากการ resolution ของการ์ด Fusion Destiny + Dreadnought
* **ความผิดพลาดเดิม:** ตัวเกม EDOPro โหลดสคริปต์ `c101402037.lua` และ `c101402036.lua` เวอร์ชันที่ไม่ได้แพตช์จากห้อง `pre-release/` ทำให้เมื่อบอร์ดตรวจสอบเงื่อนไขการอัญเชิญด้วยการ์ดใน Extra Deck เกิดการเข้าถึงตำแหน่งหน่วยความจำที่ไม่มีอยู่จริง (NULL pointer dereference) ในตัวประมวลผลกฎ `ocgcore.dll` จนเกมปิดตัวกะทันหัน
* **การแก้ไข:** 
  - ทำการซิงค์สคริปต์การ์ดที่ได้รับการแก้ไขแล้วจาก `Developer/repositories/delta-bagooska/script/` ไปยังโฟลเดอร์ทำงานของตัวเกม `./repositories/delta-bagooska/script/` และ `./script/`
  - ทำการ Commit การแก้ไขสคริปต์และตัวจัดสรรลงใน Git ประจำ Git Repository ของ `delta-bagooska` เรียบร้อยแล้ว (Commit: `Fix: Destiny Fusion and Dreadnought Lua script checks to prevent ocgcore null pointer dereference crash`)

### 2. การซิงค์และคอมไพล์โค้ดทำงาน C# บอทจริง (`WindBot/`)
* **ความผิดพลาดเดิม:** โค้ดส่วนจัดการการจัดสรรการ์ดฟิวชั่นขั้นสูง (`GetOptimalFusionMaterials`) และฟังก์ชันตรวจสอบการเรียนรู้ (`OnCardAction`) อยู่เฉพาะในฝั่ง Developer sources แต่ไม่ได้ถูกนำมาแทนที่บอทจริง ทำให้อาจโหลดบอทเวอร์ชันเก่าที่ยังไม่มีการแก้ไขไปรัน
* **การแก้ไข:**
  - ซิงค์ไฟล์ซอร์สโค้ด C# ทั้งหมดจาก `Developer/WindBot_Sources/` ไปยังโฟลเดอร์หลักของบอทจริงใน `WindBot/`
  - รันคอมไพล์ `compile_ai.bat` ทั้งในโฟลเดอร์บอทจริงและนักพัฒนา ได้รับไฟล์ `UnifiedIgnisExecutor.dll` ที่อัปเกรดล่าสุดอย่างสมบูรณ์ (สถานะ: `Compilation SUCCESSFUL!`)
  - คลุม Lock เธรดความปลอดภัยในการตรวจสอบ LP บอทใน `MonitorLP` บน `BaseCustomExecutor.cs` ป้องกัน race condition

### 3. การแก้ไขข้อผิดพลาดในการคำนวณ Path ของบอททดสอบ (`parallel_launcher.py`)
* **ความผิดพลาดเดิม:** การระบุ `PROJECT_ROOT` อิงตามความลึกโฟลเดอร์ `Developer/WindBot_Sandbox/` (คำนวณย้อนกลับขึ้นไปเพียง 2 ชั้น) ทำให้ชี้ตำแหน่งไปที่โฟลเดอร์ `Developer` ส่งผลให้ไม่พบบอทใน `WindBot/` และเกิดข้อผิดพลาดรันไม่ได้ (WinError 2)
* **การแก้ไข:** แก้ไขการคำนวณใน `parallel_launcher.py` และ `Developer/scratch/parallel_launcher.py` ให้ใช้ `PROJECT_ROOT = os.path.dirname(os.path.dirname(SCRIPT_DIR))` ซึ่งชี้ไปย้อนกลับขึ้นไป 3 ชั้น เพื่อให้ไปถึงโฟลเดอร์ root ทำงานของโครงการ `EDOTh` อย่างถูกต้องและเป็นระบบ

### 4. การจัดการ Q-Learning Discount และตัวเก็บข้อมูล SQLite
* **ความผิดพลาดเดิม:** การคำนวณ discount ใน `q_learning.py` ถูกปรับเปลี่ยนไป discount จากจุดเริ่มต้นเกม ทำให้น้ำหนักไปอยู่ที่ Turn แรกๆ มากเกินไป
* **การยืนยันการรันการทำงาน:** 
  - รันสคริปต์ `verify_dreadnought_pipeline.py` เพื่อตรวจสอบสภาพแวดล้อมจำลองสถานการณ์ทั้งหมด พบว่าการเข้าถึง Registry และ database ใน `statistics.db` สำเร็จ 100% ไม่มีข้อผิดพลาด
  - ได้รันการทดสอบ headless simulation ของบอท `2026_Dreadnought` พบว่าตัวรันคู่ขนานสามารถเชื่อมต่อและสร้าง log ของการประมวลผลดวลการ์ดได้อย่างปลอดภัย ไม่มีอาการแครชหรือค้างระหว่างรัน

---

## 📁 รายการไฟล์ที่ได้รับผลกระทบในการ Commit ครั้งนี้

* **บอท C# (WindBot):**
  - [MODIFY] [BaseCustomExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/BaseCustomExecutor.cs)
  - [MODIFY] [DreadnoughtExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/DreadnoughtExecutor.cs)
  - [MODIFY] [UnifiedIgnisExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/UnifiedIgnisExecutor.cs)
* **Python Launchers & Sandbox:**
  - [MODIFY] [parallel_launcher.py](file:///c:/Users/admin/Documents/EDOTh/Developer/WindBot_Sandbox/parallel_launcher.py)
  - [MODIFY] [parallel_launcher.py](file:///c:/Users/admin/Documents/EDOTh/Developer/scratch/parallel_launcher.py)
* **การยืนยันและการตั้งค่าระบบ:**
  - [MODIFY] [system.conf](file:///c:/Users/admin/Documents/EDOTh/config/system.conf)
  - [NEW] [changelog_dreadnought_sync.md](file:///c:/Users/admin/Documents/EDOTh/Developer/Changelogs/changelog_dreadnought_sync.md)
  - [NEW] [changelog_20260525_thai_language_restoration.md](file:///c:/Users/admin/Documents/EDOTh/Developer/Changelogs/changelog_20260525_thai_language_restoration.md)
