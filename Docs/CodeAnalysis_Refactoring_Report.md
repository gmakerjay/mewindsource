# รายงานการวิเคราะห์และ Code Refactoring — EDOPro (Project Ignis)
> อัปเดต: 21 พฤษภาคม 2026
> ประเภท: Game Client Installation (Yu-Gi-Oh! Simulator)
> เวอร์ชัน: EDOPro (Project Ignis)

---

## สารบัญ

1. [ภาพรวมโปรเจค](#1-ภาพรวมโปรเจค)
2. [Dead Code / Broken References](#2-dead-code--broken-references)
3. [Duplicate Files](#3-duplicate-files)
4. [Superseded / Obsolete Files](#4-superseded--obsolete-files)
5. [Empty Directories](#5-empty-directories)
6. [Unused / Orphaned Files](#6-unused--orphaned-files)
7. [Configuration Issues](#7-configuration-issues)
8. [Inconsistencies & Anomalies](#8-inconsistencies--anomalies)
9. [สรุปผลการวิเคราะห์](#9-สรุปผลการวิเคราะห์)
10. [Recommendations](#10-recommendations)

---

## 1. ภาพรวมโปรเจค

โปรเจคนี้เป็น **Client Installation** ของเกม Yu-Gi-Oh! Simulator — **EDOPro (Project Ignis)** ซึ่งเป็นโปรแกรมที่ใช้เล่น Yu-Gi-Oh! แบบออนไลน์ ตัวโปรเจค **ไม่มีซอร์สโค้ด (.cs, .py ฯลฯ)** มีเพียงไฟล์คอนฟิก, ฐานข้อมูลการ์ด (.cdb), และเด็ค (.ydk) เท่านั้น

### สถิติโดยรวม

| รายการ | จำนวน |
|--------|-------:|
| ไฟล์เด็ค (.ydk) ทั้งหมด | ~300+ ไฟล์ |
| ฐานข้อมูลการ์ด (.cdb) | 13 ไฟล์ |
| ไฟล์คอนฟิก (.conf / .json) | 4 ไฟล์ |
| เอกสาร Docs | 2 ไฟล์ |
| ภาษาที่รองรับ | 6 ภาษา (De, Es, Fr, It, Pt, Th) |
| Crash Dump | 1 ไฟล์ |
| ไฟล์ที่ซ้ำซ้อน | 10 คู่ |
| Reference ที่ตายแล้ว | 1 จุด |

---

## 2. Dead Code / Broken References

### 2.1 `รันระบบควบคุม_Cockpit.bat` — Broken Reference 🔴

**ไฟล์:** `รันระบบควบคุม_Cockpit.bat`
```
@echo off
echo กำลังเปิดระบบ Cockpit ควบคุมระบบ IgnisEngine...
cd /d "%~dp0"
start http://localhost:8000
python WindBot_Sandbox/cockpit.py    <-- PATH NOT FOUND
pause
```

**ปัญหา:**
- อ้างอิงถึง `WindBot_Sandbox/cockpit.py` ซึ่ง **ไม่มีอยู่ในโปรเจคนี้**
- คำสั่ง `python` อาจใช้ไม่ได้หากไม่มี Python ติดตั้ง
- `start http://localhost:8000` เปิด localhost:8000 แต่ไม่มีเว็บเซิร์ฟเวอร์ใดรันอยู่

**สถานะ:** Dead Code — ไม่สามารถทำงานได้

---

## 3. Duplicate Files

### 3.1 ไฟล์เด็คซ้ำใน `deck/` และ `deck/2026/` 🔴

มีไฟล์เด็ค 10 คู่ที่ซ้ำกันทุกประการ (identical content) อยู่ทั้งในโฟลเดอร์หลัก `deck/` และโฟลเดอร์ย่อย `deck/2026/`:

| ไฟล์ใน `deck/` | ไฟล์ใน `deck/2026/` | สถานะ |
|---|---|---|
| `2026_AzaYummy.ydk` | `deck/2026/2026_AzaYummy.ydk` | ✅ เหมือนกัน |
| `2026_BrElfnote.ydk` | `deck/2026/2026_BrElfnote.ydk` | ✅ เหมือนกัน |
| `2026_DarkTime.ydk` | `deck/2026/2026_DarkTime.ydk` | ✅ เหมือนกัน |
| `2026_EvilTwin.ydk` | `deck/2026/2026_EvilTwin.ydk` | ✅ เหมือนกัน |
| `2026_EyeInside.ydk` | `deck/2026/2026_EyeInside.ydk` | ✅ เหมือนกัน |
| `2026_Goldlord.ydk` | `deck/2026/2026_Goldlord.ydk` | ✅ เหมือนกัน |
| `2026_Hecahand.ydk` | `deck/2026/2026_Hecahand.ydk` | ✅ เหมือนกัน |
| `2026_Invoke.ydk` | `deck/2026/2026_Invoke.ydk` | ✅ เหมือนกัน |
| `2026_Kwtune.ydk` | `deck/2026/2026_Kwtune.ydk` | ✅ เหมือนกัน |
| `2026_Labrynth.ydk` | `deck/2026/2026_Labrynth.ydk` | ✅ เหมือนกัน |

**ข้อสังเกต:** `2026_PureYummy.ydk` มีเฉพาะใน `deck/` root เท่านั้น ไม่มีใน `deck/2026/`
- `lastdeck = 2026_AzaYummy` ใน `system.conf` อ้างอิงถึงไฟล์ใน `deck/` root
- โฟลเดอร์ `2026/` เป็น alternative location ที่ไม่ถูกใช้งานจริง

### 3.2 ความซ้ำซ้อนของ Documentation 🔴

| ไฟล์ | ขนาด | สถานะ |
|---|---|---|
| `Docs/IGNIS_AgenticSkill_and_IronRules.md` | ~15 KB | ❌ **Version 1 — ล้าสมัย** |
| `Docs/IGNIS_AgenticSkill_and_IronRules_v2.md` | ~18 KB | ✅ **Version 2 — อัปเดตล่าสุด** |

**รายละเอียด:**
- v1 (IGNIS_AgenticSkill_and_IronRules.md): มีเนื้อหา 1,669 บรรทัด, Part 1-4
- v2 (IGNIS_AgenticSkill_and_IronRules_v2.md): มีเนื้อหา 1,668 บรรทัด, Part 1-4 + Appendix Known Code Issues
- **v2 เป็น superset ของ v1** — มีเนื้อหาที่ v1 มีทั้งหมด เพิ่มเติมคือ Appendix ที่บันทึก Known Code Issues อีก 5 ข้อ
- **v1 จึงไม่มีความจำเป็นอีกต่อไป** — การคงไว้ทั้งสองไฟล์ทำให้สับสนว่าจะใช้เอกสารไหนเป็นหลัก

---

## 4. Superseded / Obsolete Files

### 4.1 `Docs/IGNIS_AgenticSkill_and_IronRules.md` (v1) 🟡

ถูกแทนที่โดย v2 อย่างสมบูรณ์ (ดูข้อ 3.2)

### 4.2 `crashdumps/EDOPro-pid17308-1954703.mdmp` 🟢

เป็น crash dump ที่เกิดจาก process ID 17308 ซึ่งน่าจะเป็น crash ในอดีตที่ถูกบันทึกไว้

**สถานะ:** Obsolete — ควรลบหรือย้ายออกเพื่อลด clutter

---

## 5. Empty Directories

| Directory | สถานะ | หมายเหตุ |
|---|---|---|
| `deck/Master Duel/Loaners/` | ✅ มีไฟล์ (16 ไฟล์) |
| `deck/Master Duel/Shop/` | ✅ มีไฟล์ (1 ไฟล์) |
| `deck/Master Duel/Friend Code/` | ✅ มีไฟล์ |
| **`deck/Master Duel/Solo/`** | **🔴 ว่างเปล่า** | อาจเป็นที่เก็บเด็ค NPC ของ Solo Mode ที่ยังไม่ได้เพิ่ม |
| `deck/Anime/3 5Ds/` | 🔴 ไม่มีโฟลเดอร์นี้ | มีแค่ 1 Duel Monsters, 2 GX, Movies |
| `deck/Anime/4 Zexal/` | 🔴 ไม่มีโฟลเดอร์นี้ | 5Ds, Zexal, Arc-V ยังไม่มี |
| `deck/Anime/5 Arc-V/` | 🔴 ไม่มีโฟลเดอร์นี้ | มีแค่ชื่อ แต่ไม่มีข้อมูล |
| `deck/Anime/Movies/` | ✅ มีไฟล์ (2 ไฟล์) |
| `deck/Archetypes/#/` | 🟡 มี 4 ไฟล์ | ไฟล์น้อยมากเมื่อเทียบกับ arch ที่มี (เช่น S มี 40 ไฟล์, M มี 36 ไฟล์) |

---

## 6. Unused / Orphaned Files

### 6.1 ฐานข้อมูลการ์ด (.cdb)

มีไฟล์ .cdb จำนวน 13 ไฟล์ในโฟลเดอร์ `expansions/`:

| ไฟล์ | ประเภท | หมายเหตุ |
|---|---|---|
| `cards.cdb` | ฐานข้อมูลหลัก OCG/TCG | ✅ ถูกใช้งานแน่นอน |
| `cards-unofficial.cdb` | การ์ดนอกทางการ | ✅ อาจถูกใช้งาน (show_unofficial = 1 ใน system.conf) |
| `cards-unofficial-new.cdb` | การ์ดนอกทางชุดใหม่ | ✅ อาจถูกใช้งาน |
| `cards-rush.cdb` | Rush Duel | 🟡 ไม่ได้เปิดใช้ (ไม่มีการตั้งค่า Rush Duel) |
| `cards-skills.cdb` | Skill Cards | 🟡 ไม่ได้เปิดใช้ |
| `cards-skills-unofficial.cdb` | Skill Cards นอกทาง | 🟡 ไม่ได้เปิดใช้ |
| `cards_doomz.cdb` | การ์ด Doomz | 🟡 custom expansion |
| `cards_witchcrafter_rv01.cdb` | Witchcrafter RV01 | 🟡 custom expansion |
| `des_dogma.cdb` | Des Dogma | 🟡 custom expansion |
| `dracotail_th.cdb` | Dragontail (ไทย) | 🟡 custom expansion |
| `goat-entries.cdb` | GOAT Format | 🟡 special format |
| `lpg2.cdb` | LPG2 | 🟡 custom expansion |
| `summoned_skull.cdb` | Summoned Skull | 🟡 custom expansion |

**ข้อสังเกต:** ไฟล์ .cdb ทั้งหมดถูกโหลดโดย EDOPro ตามการตั้งค่า แต่การมี custom expansions จำนวนมากอาจทำให้ client ช้าลง

### 6.2 ภาษาที่ไม่ได้ใช้

| ภาษา | มีไฟล์ (`config/languages/*/strings.conf`) | ถูกตั้งค่าใน system.conf หรือไม่ |
|---|---|---|
| Deutsch | ✅ | ❌ (language = Thai) |
| Español | ✅ | ❌ |
| Français | ✅ | ❌ |
| Italiano | ✅ | ❌ |
| Português | ✅ | ❌ |
| Thai | ✅ | ✅ (language = Thai) |

5 ภาษาที่ไม่ได้ถูกเลือกยังคงกินพื้นที่โดยไม่จำเป็น

---

## 7. Configuration Issues

### 7.1 `system.conf` — Issues ที่พบ

| Parameter | ค่าปัจจุบัน | ปัญหา |
|---|---|---|
| `lastdeck` | `2026_AzaYummy` | 🟡 อ้างอิงไฟล์ใน `deck/` — มีไฟล์ซ้ำใน `deck/2026/` |
| `lastBot` | `47` | 🟡 ไม่มีการแมปหมายเลขบอท — ไม่รู้ว่าบอท ID 47 คืออะไร |
| `gameport` | 7911 | 🟢 OK |
| `override_ssl_certificate_path` | (ว่าง) | 🔴 มี `cacert.pem` อยู่ที่ root แต่ path ไม่ได้ตั้งค่า |

### 7.2 `configs.json` — Repositories ที่ไม่ได้ Clone

```json
{
    "url": "https://github.com/ProjectIgnis/DeltaBagooska",
    "repo_path": "./repositories/delta-bagooska"
}
```
- **ไม่มีโฟลเดอร์ `repositories/` ในโปรเจค**
- Repo อีก 2 แห่ง (LFLists, Puzzles) ก็ไม่ได้ clone เช่นกัน

### 7.3 SSL Certificate Path

มีไฟล์ `cacert.pem` ที่ root project แต่ `override_ssl_certificate_path` ใน system.conf ว่างอยู่

---

## 8. Inconsistencies & Anomalies

### 8.1 โครงสร้าง Deck ที่ไม่สมบูรณ์

- **Anime Decks:** มีแค่ Duel Monsters (2000), GX (2004) และ Movies — ขาด 5Ds, Zexal, Arc-V
- **Archetype #:** มีเพียง 4 ไฟล์ — @Ignister, [TCG] Ashened และอื่นๆ อีก 2 ไฟล์
- **Mechanics:** มีครบทุกหมวด (Fusion, Synchro, Xyz, Link ฯลฯ)
- **Starter/Structure Decks:** มีครบถ้วนตั้งแต่ปี 2002–2025

### 8.2 การตั้งชื่อ Deck ไม่เป็นระบบ

มีรูปแบบการตั้งชื่อหลายแบบผสมกัน:
- `2026_AzaYummy.ydk` (snake_case)
- `A-Yami Yugi (Virtual Deck).ydk` (มีวงเล็บและเว้นวรรค)
- `[Tactical-Try Deck] Eldlich the Conqueror.ydk` (มีวงเล็บเหลี่ยม)
- `A-Fudo Yusei(Manga-2).ydk` (ผสมกันหลายแบบ)

### 8.3 WindBot Reference

`รันระบบควบคุม_Cockpit.bat` พยายามเรียกใช้ `python WindBot_Sandbox/cockpit.py` แต่ไม่มี WindBot Sandbox อยู่ในโปรเจคนี้

หมายเหตุ: WindBot IGNIS เป็น AI bot สำหรับเล่น Yu-Gi-Oh! แบบอัตโนมัติ ซึ่งน่าจะเป็นโค้ด C# ในโปรเจคแยกต่างหาก (`UnifiedIgnisExecutor.cs`) ที่ Docs กล่าวถึง แต่ **ไม่ได้รวมอยู่ในโปรเจคนี้**

---

## 9. สรุปผลการวิเคราะห์

### 9.1 Dead Code (ต้องแก้ไข)

| # | รายการ | ความรุนแรง | คำแนะนำ |
|---|--------|:--------:|---------|
| 1 | `รันระบบควบคุม_Cockpit.bat` — path `WindBot_Sandbox/cockpit.py` ไม่มีอยู่ | 🔴 สูง | แก้ path หรือลบถ้าไม่ใช้ |
| 2 | `Docs/IGNIS_AgenticSkill_and_IronRules.md` — v1, ถูกแทนที่โดย v2 | 🟡 กลาง | ลบหรือทำ Deprecation Notice |

### 9.2 Duplicate Files

| # | รายการ | ความรุนแรง | คำแนะนำ |
|---|--------|:--------:|---------|
| 3 | 10 คู่ deck files ใน `deck/` และ `deck/2026/` | 🟡 กลาง | ลบออกจากที่ใดที่หนึ่ง (แนะนำให้เก็บใน `deck/2026/`) |
| 4 | 5 ภาษาไม่ได้ใช้ใน `config/languages/` | 🟢 ต่ำ | ลบหรือเก็บไว้เฉพาะ Thai |

### 9.3 Orphaned / Obsolete

| # | รายการ | ความรุนแรง | คำแนะนำ |
|---|--------|:--------:|---------|
| 5 | `crashdumps/EDOPro-pid17308-1954703.mdmp` | 🟢 ต่ำ | ลบ |
| 6 | `deck/Master Duel/Solo/` directory ว่าง | 🟢 ต่ำ | ลบหรือเพิ่มเนื้อหา |
| 7 | `repositories/` ยังไม่ถูก clone (ตาม configs.json) | 🟡 กลาง | รัน update จากในเกม |
| 8 | `cacert.pem` ไม่ถูกตั้งค่าใน `override_ssl_certificate_path` | 🟢 ต่ำ | ตั้งค่าหรือไม่ต้องสนใจ |

### 9.4 Deck Organization

| # | รายการ | คำแนะนำ |
|---|--------|---------|
| 9 | ไฟล์ `2026_PureYummy.ydk` ขาดใน `deck/2026/` | เพิ่มเข้าไปใน `deck/2026/` เพื่อความสมบูรณ์ |
| 10 | การตั้งชื่อไฟล์ Deck ไม่เป็นระบบ | กำหนด naming convention ให้สม่ำเสมอ |

---

## 10. Recommendations

### Priority 1 — 🔴 ต้องแก้ไขโดยด่วน

1. **แก้ไข `รันระบบควบคุม_Cockpit.bat`**:
   - ถ้า `WindBot_Sandbox/` มีอยู่ที่อื่น ให้แก้ path ให้ถูกต้อง
   - ถ้าไม่มี ให้ลบหรือ comment บรรทัดที่เรียกใช้ออก
   - หรือถ้าต้องการให้สคริปต์ทำงานได้ ให้สร้าง cockpit.py หรือระบุ path ที่ถูกต้อง

2. **ทำความสะอาด Deck Files ที่ซ้ำ**:
   - เลือกเก็บเฉพาะที่ `deck/` root หรือ `deck/2026/` เพียงที่เดียว
   - แนะนำให้เก็บที่ `deck/2026/` เพราะเป็น subdirectory ที่เป็นระเบียบกว่า
   - และลบไฟล์ออกจาก `deck/` root (ยกเว้น `2026_PureYummy.ydk` ที่ไม่มีใน `deck/2026/`)

### Priority 2 — 🟡 ควรทำ

3. **รวม Documentation**:
   - ลบ `IGNIS_AgenticSkill_and_IronRules.md` (v1)
   - หรือเพิ่ม deprecation notice ที่หัวไฟล์

4. **ลบ Crash Dump**:
   - ลบไฟล์ `crashdumps/EDOPro-pid17308-1954703.mdmp`

5. **เพิ่ม `2026_PureYummy.ydk` ใน `deck/2026/`**:
   - รวมกับเด็ค 2026 อื่นๆ เพื่อความสมบูรณ์

### Priority 3 — 🟢 เสนอเพิ่มเติม

6. **ลบภาษาที่ไม่ได้ใช้** (ถ้าต้องการลดขนาดโปรเจค)
7. **เพิ่ม Anime Decks ที่ขาด** (5Ds, Zexal, Arc-V)
8. **กำหนด Naming Convention** สำหรับไฟล์ .ydk

---

## Appendix A — จำนวนไฟล์เด็คทั้งหมดจำแนกตามโฟลเดอร์

| โฟลเดอร์ | จำนวนไฟล์ | หมายเหตุ |
|---|---|---|
| `deck/` (root) | 78 | รวม 2026_* 11 ไฟล์ + A-* 62 ไฟล์ + Tactical Try 5 ไฟล์ |
| `deck/2026/` | 10 | ขาด `2026_PureYummy.ydk` |
| `deck/Anime/` | ~40+ | แค่ Duel Monsters + GX + Movies |
| `deck/Archetypes/` | ~400+ | 27 โฟลเดอร์ย่อย A-Z + # |
| `deck/Master Duel/` | 18 | รวม Loaners + Shop + Friend Code |
| `deck/Mechanics/` | ~150+ | 14 หมวดกลไกการเล่น |
| `deck/Starter Decks/` | 25 | 2002–2024 |
| `deck/Structure Decks/` | 59 | 2005–2025 |
| `deck/todo/` | 78 | 7 หมวด — กำลังพัฒนา |
| `deck/World Championship/` | 6 | 2014–2019 |

---

## Appendix B — Known Code Issues (จาก IGNIS v2 Docs)

ปัญหาเหล่านี้ถูกบันทึกไว้ใน `IGNIS_AgenticSkill_and_IronRules_v2.md` Appendix:

| # | ปัญหา | ตำแหน่ง | ผลกระทบ |
|---|-------|---------|----------|
| 1 | Effect Veiler จำกัดแค่ Main1 | บรรทัด 915 | พลาดการ์ดที่ summon ใน Main2 |
| 2 | Hard Cap + Anti-Inflation Decay ซ้อน | บรรทัด 596–622 | Decay ไร้ผล |
| 3 | Droll & Lock Bird มี role "recovery" | cards_registry_2026_AzaYummy.json | อาจใช้ Droll ผิดจังหวะ |
| 4 | 4 เด็คไม่มี deck config | config/decks/ | Goldlord, Invoke, Kwtune, Labrynth |
| 5 | Learning Pipeline ไม่เคยทำงาน | ApplyRealTimeLearning() | Hard Cap ไม่เคยถูกเรียก |

> **หมายเหตุ:** Issues เหล่านี้เป็นข้อบกพร่องของโค้ด `UnifiedIgnisExecutor.cs` ซึ่ง **ไม่ได้อยู่ในโปรเจคนี้** แต่ถูกอ้างถึงในเอกสาร Docs เท่านั้น

---

*รายงานนี้สร้างโดย Codebuff AI — วิเคราะห์จากโครงสร้างโปรเจคจริงเท่านั้น โดยไม่มีการแก้ไขไฟล์ใดๆ*
