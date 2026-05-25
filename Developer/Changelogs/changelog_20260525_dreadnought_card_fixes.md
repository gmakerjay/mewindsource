# Changelog - 25 May 2026 (Dreadnought Card ID & Compatibility Fixes)

## 1. ปัญหาที่พบ (Identified Issue)
ในการดวลของบอทและผู้เล่นผ่านเด็ค **`2026_Dreadnought`** ใน EDOPro มีการแจ้งเตือนเด็คผิดพลาด (**Deck has errors / Invalid cards**) เนื่องจากเด็คมีการใช้งานการ์ดที่เป็น **ID รูปภาพทางเลือก (Alternative Artwork Alias)** จำนวน 7 ใบ ซึ่งไม่มีตัวตนอยู่ในฐานข้อมูลหลักของตัวเกมหลัก EDOPro (`cards.cdb`) มีผลทำให้ตัวเกมอ่านการ์ดเป็น Unknown Card หรือบล็อกไม่ให้ใช้เด็คนี้ดวล

---

## 2. การ์ดที่ได้รับการแก้ไข (Card Fixes Summary)
ได้ทำการค้นหา ID ดั้งเดิม (Original ID) ที่ถูกลงทะเบียนอย่างถูกต้องในฐานข้อมูลหลักและอัปเดตแทนที่ ID รูปภาพทางเลือกทั้งหมด 7 ใบ ดังนี้:

| ชื่อการ์ด | ID รูปภาพทางเลือกเดิม | ID มาตรฐานที่ใช้แทนที่ | ตำแหน่ง / ประเภท |
| :--- | :---: | :---: | :--- |
| **Mask Change** | `21143941` | **`21143940`** | Quick-Play Spell |
| **Winged Kuriboh Sabatiel LV10** | `40237840` | **`40237839`** | Effect Monster (Custom Skill) |
| **Masked HERO Dark Law** | `58481573` | **`58481572`** | Fusion Monster |
| **Called by the Grave** | `24224831` | **`24224830`** | Quick-Play Spell |
| **Favorite HERO Shining Flare Wingman** | `87758526` | **`87758525`** | Fusion Monster |
| **Favorite HERO Flame Wingman** | `13243125` | **`13243124`** | Fusion Monster |
| **Destiny HERO - Plasma** | `83965311` | **`83965310`** | Effect Monster |

---

## 3. ไฟล์ที่ได้รับการแก้ไข (Modified Files)

### A. แฟ้มโครงสร้างเด็ค (YDK Decks)
ปรับเปลี่ยน ID ทั้ง 7 ใบให้เป็น ID มาตรฐานเพื่อความเข้ากันได้สูงสุด:
* [deck/2026_Dreadnought.ydk](file:///c:/Users/admin/Documents/EDOTh/deck/2026_Dreadnought.ydk) (เด็คใช้งานปกติ)
* [WindBot/Decks/2026_Dreadnought.ydk](file:///c:/Users/admin/Documents/EDOTh/WindBot/Decks/2026_Dreadnought.ydk) (เด็ครันจริงของ WindBot)
* [WindBot/Decks/AI_2026_Dreadnought.ydk](file:///c:/Users/admin/Documents/EDOTh/WindBot/Decks/AI_2026_Dreadnought.ydk) (เด็คเสริมจำลองดวล)

### B. โค้ดโปรแกรม C# Executor
* [WindBot/DreadnoughtExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/DreadnoughtExecutor.cs)
  * อัปเดตการอ้างอิง ID ของการ์ดที่แก้ไขทั้งหมดในส่วนของ `AddExecutor` เงื่อนไขลอจิกแฮนด์แทรป, สเปล และเป้าหมายการเลือกการ์ด OnSelectCard เพื่อให้แมปกับการ์ดในเด็คจริงได้อย่างแม่นยำ

### C. ไฟล์ระบบ Heuristics Registry
* [WindBot/config/cards_registry_2026_Dreadnought.json](file:///c:/Users/admin/Documents/EDOTh/WindBot/config/cards_registry_2026_Dreadnought.json)
  * ปรับแต่งข้อมูล ID การ์ดในทะเบียนน้ำหนักของบอทให้ตรงกับค่าน้ำหนักจริงหลังแก้ไข

---

## 4. การยืนยันความถูกต้อง (Verification)
1. **การคอมไพล์ (C# Compilation)**: รันสคริปต์คอมไพล์ของระบบ `compile_ai.bat` ผลลัพธ์คือ **`Compilation SUCCESSFUL!`** คอมไพล์ได้ DLL สำหรับรันจริงเรียบร้อย
2. **การโหลดระบบบอท (Dry Run Verification)**: รันทดสอบ `WindBot.exe` ด้วยเด็ค `2026_Dreadnought` ระบบสามารถโหลดไฟล์การ์ดและเริ่มต้นห้องดวล (Match Session) ได้ปกติ 100% โดยไม่มีข้อผิดพลาดการแจ้งเตือนการ์ดผิดกฎหรือ Unknown Card อีกต่อไป
