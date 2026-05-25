# Changelog — 25 พฤษภาคม 2026 (ระบบแปลภาษาไทยเด็ค Dreadnought & ป้องกันการเขียนทับ)
**เวลา:** 2026-05-25 (Dreadnought Deck Translations & Overwrite Resilience)  
**ผู้ปฏิบัติการ:** Antigravity AI Agent  
**ขอบเขต:**
* [apply_translations.py](file:///c:/Users/admin/Documents/EDOTh/apply_translations.py)
* [verify_translations.py](file:///c:/Users/admin/Documents/EDOTh/verify_translations.py)
* [inspect_repo_cards.py](file:///c:/Users/admin/Documents/EDOTh/WindBot_Sandbox/scratch/inspect_repo_cards.py)

---

## 🟢 1. เพิ่มคำแปลและการ์ดที่ตกหล่น (Denier & Dreadmaster Translation Added)
* **รายละเอียด**:
  * เพิ่มคำแปลภาษาไทยแบบสมบูรณ์ให้แก่การ์ดระดับคีย์ของเด็ค Dreadnought:
    * `16605586` (**Destiny HERO - Denier**)
    * `40591390` (**Destiny HERO - Dreadmaster**)
  * คำแปลคงชื่อเป็นภาษาอังกฤษ และแปลเอฟเฟกต์เป็นภาษาไทยโดยไม่ใช้คำต้องห้าม (`"คาถา"` หรือ `"กัปดัก"`) 
  * อัปเดตและเขียนคำแปลลงสู่ไฟล์ฐานข้อมูลเดลต้าหลัก `cards.delta.cdb` เรียบร้อยแล้ว

---

## 🛠️ 2. ป้องกันการเขียนทับโดยระบบ EDOPro Auto-updater
* **รายละเอียด**:
  * ตัวเกม EDOPro มักจะสั่ง Git Checkout/Pull ทับไฟล์ `.cdb` ใน `repositories/delta-bagooska/` เมื่อเริ่มเกมทำให้คำแปลหาย
  * แก้ไขตรรกะใน [apply_translations.py](file:///c:/Users/admin/Documents/EDOTh/apply_translations.py) ให้**เขียนแปลการ์ดทั้งหมด 119 ใบแบบไดนามิกใหม่ทุกครั้ง**เมื่อกดรันสคริปต์ โดยไม่สนใจว่าฐานข้อมูลเดิมใน submodule จะโดนดึงภาษาอังกฤษมาทับหรือไม่

---

## 📂 3. จุด Deploy การ์ดแปลภาษาไทย (Deploy Points)
เพื่อให้ EDOPro และ WindBot ค้นหาคำแปลภาษาไทยเจอเสมอ สคริปต์ได้ Deploy ไฟล์ฐานข้อมูลแปลไทยไปยังตำแหน่งต่างๆ ดังนี้:

1. **`config/languages/Thai/` (ตำแหน่งหลัก)**
   * *ความสำคัญ*: EDOPro จะสแกนค้นหาไฟล์คำแปล `.cdb` ที่ตำแหน่งนี้เป็นหลัก (สแกนแบบไม่ลึกลงโฟลเดอร์ย่อย) เพื่อแสดงผลชื่อ/คำอธิบายการ์ดภาษาไทยในหน้าดูเอลและห้องจัดเด็ค
2. **`config/languages/Thai/repositories/delta-bagooska/` (ตำแหน่งเสริมนิวเมอร์ิคัล)**
   * *ความสำคัญ*: ป้องกันในกรณีที่ระบบการโหลดแบบ Subfolder ของเวอร์ชันอนาคตอ้างอิงตำแหน่งสัมพัทธ์
3. **`WindBot/` (ตำแหน่งจำลอง AI)**
   * *ความสำคัญ*: เพื่อให้ C# Engine ของ WindBot โหลดฐานข้อมูลการ์ดแปลไทยไปวิเคราะห์และรันการ์ดคอมโบของ AI ได้ถูกต้อง

---

## 📖 4. ขั้นตอนและจุด Deploy เมื่อมีการโหลดการ์ดชุดใหม่ในอนาคต (Guide for Deploying New Cards)
หากต้องการเพิ่มคำแปลและการ์ดชุดใหม่ๆ เข้าสู่ระบบ สามารถดำเนินการตามขั้นตอนเหล่านี้:

1. **นำฐานข้อมูลการ์ดใหม่เข้า**:
   * วางไฟล์ `.cdb` ชุดการ์ดใหม่ของคุณไว้ในไดเรกทอรี `repositories/delta-bagooska/`
2. **ลงทะเบียนชื่อฐานข้อมูลใหม่**:
   * เปิด [apply_translations.py](file:///c:/Users/admin/Documents/EDOTh/apply_translations.py) และ [verify_translations.py](file:///c:/Users/admin/Documents/EDOTh/verify_translations.py) แล้วเพิ่มชื่อไฟล์ `.cdb` นั้นเข้าไปในอาเรย์ `cdb_names` (เช่นเพิ่ม `"prerelease-lpg2.cdb"`)
3. **ใส่คำแปลภาษาไทย**:
   * นำคำแปลของการ์ดใบใหม่ (ชื่อภาษาอังกฤษดั้งเดิม และคำอธิบายภาษาไทย) ไปเพิ่มในตัวแปร `CUSTOM_TRANSLATIONS` ในสคริปต์ [apply_translations.py](file:///c:/Users/admin/Documents/EDOTh/apply_translations.py)
4. **ดำเนินการติดตั้งและตรวจสอบ**:
   * เปิดเทอร์มินัลแล้วสั่งรันสคริปต์:
     ```bash
     python apply_translations.py
     python verify_translations.py
     ```
   * ระบบจะทำการแปล ติดตั้งลงจุด Deploy ทั้ง 3 ตำแหน่ง พร้อมทำความสะอาดไฟล์ขยะให้คุณโดยอัตโนมัติ

---

## 📊 ผลการตรวจสอบล่าสุด (Verification Status)
* **การแปลการ์ดทั้งหมด**: ✅ ผ่านสมบูรณ์แบบ (`Untranslated: 0 | Translated: 119`)
* **ผลการรันตรวจสอบ**: ✅ `Expected IDs: 118 | Verified IDs: 118 | Issues Found: 0` (การ์ดเด็ค Dreadnought แปลไทยครบถ้วนสมบูรณ์)
