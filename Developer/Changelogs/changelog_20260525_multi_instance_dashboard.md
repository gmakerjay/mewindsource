# บันทึกการเปลี่ยนแปลง (Changelog) - 25 พฤษภาคม 2026 (ระบบฝึกฝนขนานและแดชบอร์ดวัดผลการเรียนรู้)

บันทึกการอัปเดตระบบในหัวข้อโครงสร้างพื้นฐานการจำลองการแข่งขนาน (Multi-Instance Parallel Training) และการเชื่อมโยงระบบ SQLite เพื่อวิเคราะห์การพัฒนาของ AI (Learning Curve Performance) ผ่านระบบ Cockpit Dashboard

---

## 1. การเปลี่ยนแปลงและเพิ่มฟีเจอร์หลัก (Key Highlights)

### 1.1 ลูปการจำลองแข่งขันแบบ Self-Play (เทรนบอทสองตัวพร้อมกัน)
*   **[MODIFY] [run_multi_iterations.py](file:///c:/Users/admin/Documents/EDOTh/scratch/run_multi_iterations.py):**
    *   เพิ่มระบบตรวจจับเด็คคู่ต่อสู้ หากสู้กันเองระหว่างเด็คตระกูล `2026_` (Self-Play) ระบบจะทำการรันคำสั่งอัปเดตน้ำหนัก Registry และ Q-learning ให้กับบอททั้งสองเด็คพร้อมกันเมื่อจบแต่ละรอบ
    *   ปรับปรุงระบบจัดเก็บประวัติการดวล โดยทำการสแกนเก็บโฟลเดอร์ของห้องที่บอทฝั่งคู่ต่อสู้สวมบทบาทอยู่เข้าไปยังโฟลเดอร์เก็บถาวร `ArchivedMatches` ด้วย เพื่อความสะอาดและป้องกันปัญหาไฟล์ขยะกองล้นโฟลเดอร์ WindBot
    *   ส่งพารามิเตอร์เด็คหลักและเด็คคู่ซ้อมเข้าสู่ระบบตัวนำเข้าฐานข้อมูล SQL เพื่อให้จำแนกความแตกต่างการดวลได้ถูกต้อง

### 1.2 เพิ่มคอลัมน์และระบบ Migration ในฐานข้อมูล SQLite
*   **[MODIFY] [save_outcomes_to_sql.py](file:///c:/Users/admin/Documents/EDOTh/scratch/save_outcomes_to_sql.py):**
    *   เพิ่มคอลัมน์ `opponent_deck` ลงในตาราง `matches`
    *   ใส่ตรรกะตรวจเช็คความถูกต้องและระบบ Migration อัตโนมัติ (หากฐานข้อมูล SQLite `statistics.db` เดิมยังไม่มีคอลัมน์ดังกล่าว จะทำการรันคำสั่ง `ALTER TABLE` เพื่ออัปเกรดโครงสร้างโดยไม่ทำให้ประวัติการแข่งเก่าสูญหาย)
    *   เพิ่มการรองรับอาร์กิวเมนต์แบบ `--deck` และ `--opp-deck` เพื่อบันทึกคู่แข่งขันที่ดวลกันลงในคลังประวัติ ช่วยให้ผู้ใช้สามารถคิวรีดูสถิติแพ้ชนะของบอทแยกตามคู่แข่งขันได้จริง

### 1.3 ยกเครื่องหน้าวัดผล Cockpit Dashboard (Progress Page Overhaul)
*   **[MODIFY] [cockpit.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/cockpit.py):**
    *   นำเข้าไลบรารี `sqlite3`
    *   เพิ่มการโหลดและผูกเส้นทางเดินลิงก์ (Route) ของหน้าวัดผลจำลอง `/progress` กับหน้าดีไซน์ใหม่
    *   สร้างระบบ API `/api/progress_report` ที่เชื่อมต่อกับ SQLite เพื่อคิวรีหาแมตช์ประวัติของเด็คเดี่ยว จัดกลุ่มประวัติตามเวลาและแบ่งเป็นบล็อกคำนวณหา อัตราชนะ %, เทิร์นเฉลี่ย, และคะแนนเฉลี่ยการตัดสินใจ (Average Decision Score) ในบล็อกนั้นๆ
*   **[MODIFY] [progress.html](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/templates/progress.html):**
    *   เขียนทับและยกเลิกระบบ Snapshot ไฟล์ JSON แบบเดิมทั้งหมด
    *   เพิ่ม Dropdown ให้สลับเลือกกรองวิเคราะห์ตามประเภทเด็ค `2026_` และเลือกกรุ๊ปข้อมูล Matches per Round (5, 10, 20 แมตช์)
    *   เพิ่มแถบประเมินประสิทธิภาพ (Performance Comparison: Before vs After) เพื่อวัดเปอร์เซ็นต์ความฉลาดของบอทก่อนฝึกฝนเทียบกับบล็อกจำลองรอบล่าสุด
    *   ใช้ Chart.js พล็อตกราฟเส้นสองชุดอย่างพรีเมียม: กราฟอัตราชนะ (Win Rate Curve) และกราฟคะแนนความคุ้มค่าการเดินเกมเฉลี่ยคู่กับรอบเทิร์นของเกม (Avg Decision Score & Turns Curve)
    *   เพิ่มตารางแจกแจงประวัติสรุปข้อมูลผลคะแนนในรอบดวล (Rounds Table Summary)

### 1.4 การเขียนคู่มือเชิงลึกใน Docs
*   **[NEW] [Multi_Instance_Training_and_Evaluation_Structure.md](file:///c:/Users/admin/Documents/EDOTh/Docs/Multi_Instance_Training_and_Evaluation_Structure.md):** เอกสารคู่มืออธิบายรายละเอียดส่วนประกอบต่างๆ โฟลว์การเทรนของระบบ สถาปัตยกรรมลูปและผังจำลอง และวิธีคิวรีข้อมูลผ่าน SQLite

---

## 2. วิธีการยืนยันและการตรวจสอบระบบ (Verification & Testing)
1.  รันตัวคัดกรอง SQLite `python scratch/save_outcomes_to_sql.py` เพื่อตรวจสอบการแปลงโครงสร้างฐานข้อมูลและคิวรีล่าสุด
2.  รันเซิร์ฟเวอร์ Cockpit `python WindBot_Sandbox/cockpit.py` และเปิดหน้าเว็บแท็บ "วัดผล" สลับเลือกเด็คและรอบเพื่อเช็คความก้าวหน้าและการพล็อตกราฟว่าสถิติการดวลและเส้นกราฟเปลี่ยนรูปไปตามข้อมูล SQLite อย่างถูกต้อง
