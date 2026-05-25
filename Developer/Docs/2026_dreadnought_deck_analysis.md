# รายงานการวิจัยและวิเคราะห์เด็ค [2026_Dreadnought.ydk](file:///c:/Users/admin/Documents/EDOTh/deck/2026_Dreadnought.ydk) อย่างละเอียด

รายงานฉบับนี้วิเคราะห์โครงสร้างของเด็คการ์ด Custom/Prerelease ในธีม **Destiny HERO (D-HERO)** และเปรียบเทียบการทำงานของโค้ดสคริปต์ภาษา **Lua** ของแต่ละการ์ด รวมถึงจำแนกบทบาทการ์ดสำคัญ (Ace Cards, Handtraps, Protect Targets) และการวิเคราะห์คอมโบหลักสำหรับการนำไปใช้เล่นจริง

---

## 1. ข้อมูลการ์ด Custom/Prerelease และการเปรียบเทียบไฟล์ Lua

เด็คนี้ประกอบด้วยการ์ดที่ถูกออกแบบมาเป็นพิเศษเพื่อให้เกิดการ Synergy ร่วมกับการ์ด D-HERO ดั้งเดิมของซีรีส์ โดยมีรายละเอียดโค้ดสคริปต์ดังนี้:

### 1.1 Destiny HERO - Doom Liege (เดวิลโหลดกอย - 101402022)
* **ประเภทการ์ด**: มอนสเตอร์เอฟเฟกต์ (Level 6 / DARK / Warrior)
* **หน้าที่หลัก**: การ์ดเริ่มต้นคอมโบ (Combo Starter) และทำลายจังหวะของศัตรู
* **การวิเคราะห์โค้ด Lua ([c101402022.lua](file:///c:/Users/admin/Documents/EDOTh/repositories/delta-bagooska/script/pre-release/c101402022.lua))**:
  * **เอฟเฟกต์ที่ 1 (Trigger Effect - เมื่ออัญเชิญสำเร็จ)**:
    ```lua
    e1a:SetCategory(CATEGORY_REMOVE)
    e1a:SetCode(EVENT_SUMMON_SUCCESS)
    -- ใช้ฟังก์ชัน RemoveUntil เพื่อกวาดล้างมอนสเตอร์ศัตรูออกชั่วคราว
    aux.RemoveUntil(tc,nil,REASON_EFFECT,PHASE_STANDBY,id,e,tp,aux.DefaultFieldReturnOp,return_condition,nil,reset_count)
    ```
    ทำให้เมื่อถูกอัญเชิญแบบปกติหรือพิเศษ จะสามารถเนรเทศมอนสเตอร์คู่ต่อสู้ 1 ตัวออกนอกเกมชั่วคราวจนถึง Standby Phase ถัดไป
  * **เอฟเฟกต์ที่ 2 (Ignition Effect - สุสานส่งการ์ด + ค้นหาการ์ดฟิลด์)**:
    ```lua
    -- ส่ง D-HERO จากเด็คลงสุสานเพื่อเป็นคอสต์ (Cost)
    Duel.SendtoGrave(g,REASON_COST)
    -- ค้นหา Clock Tower Prison หรือ Clock Tower Prison City - Dark City ขึ้นมือ
    e2:SetCategory(CATEGORY_SEARCH+CATEGORY_TOHAND)
    ```
    เอฟเฟกต์นี้มีประสิทธิภาพสูงมาก เพราะช่วยเตรียมวัตถุดิบ (เช่น Denier หรือ Dreadnought Servant) ลงสุสานพร้อมเปิดใช้การ์ดฟิลด์การโจมตีได้ทันที

### 1.2 Clock Tower Prison City - Dark City (101402062)
* **ประเภทการ์ด**: เวทมนตร์ฟิลด์ (Field Spell)
* **หน้าที่หลัก**: ค้นหาการ์ดในเด็ค และอัญเชิญมอนสเตอร์คีย์แบบพิเศษเมื่อถูกทำลาย
* **การวิเคราะห์โค้ด Lua ([c101402062.lua](file:///c:/Users/admin/Documents/EDOTh/repositories/delta-bagooska/script/pre-release/c101402062.lua))**:
  * **เอฟเฟกต์ที่ 1 (Ignition Effect - ค้นหาการ์ด)**:
    ```lua
    -- สามารถค้นหา D-HERO หรือการ์ดที่ระบุชื่อ D-HERO ได้
    e2:SetCondition(function(e) return e:GetHandler():HasFlagEffect(id) end)
    ```
    สามารถเสิร์ชการ์ดใดก็ได้ที่มีความเกี่ยวข้องกับ D-HERO ในเทิร์นที่การ์ดใบนี้เปิดใช้งาน
  * **เอฟเฟกต์ที่ 2 (Trigger Effect - อัญเชิญพิเศษจากเด็คเมื่อถูกทำลาย)**:
    ```lua
    -- เมื่อถูกทำลาย จะอัญเชิญมอนสเตอร์ D-HERO จากเด็ค
    e3:SetCode(EVENT_DESTROYED)
    -- สำคัญที่สุด: การอัญเชิญนี้จะถูกจัดประเภทเสมือนถูกอัญเชิญด้วย "Clock Tower Prison"
    -- ทำให้สามารถสั่งการคอมโบทำลายบอร์ดของ Dreadmaster ได้ทันที!
    ```

### 1.3 Destiny HERO - Dreadnought Servant (101402023)
* **ประเภทการ์ด**: มอนสเตอร์เอฟเฟกต์ (Level 4 / DARK / Warrior)
* **หน้าที่หลัก**: ตัวเร่งคอมโบและขัดขวางการตั้งบอร์ดของคู่ต่อสู้จากสุสาน
* **การวิเคราะห์โค้ด Lua ([c101402023.lua](file:///c:/Users/admin/Documents/EDOTh/repositories/delta-bagooska/script/pre-release/c101402023.lua))**:
  * **เอฟเฟกต์ที่ 1 (Ignition Effect - อัญเชิญพิเศษจากมือ + ทำลายเพื่อเสิร์ช Polymerization)**:
    ```lua
    -- อัญเชิญตัวเองจากมือหากเราควบคุม D-HERO หรือการ์ดฟิลด์
    e1:SetRange(LOCATION_HAND)
    -- สามารถเลือกทำลายการ์ดที่เราควบคุม (เช่นการทำลาย Dark City ของเราเอง)
    -- จากนั้นค้นหา "Polymerization" จากเด็คขึ้นมือ
    ```
  * **เอฟเฟกต์ที่ 2 (Trigger Effect จากสุสาน - ขัดขวางบอร์ด)**:
    ```lua
    -- เมื่อมีการอัญเชิญพิเศษ D-HERO เลเวล 8 สำเร็จ (เช่น Dreadmaster หรือ Death Dogma)
    e2:SetCode(EVENT_SPSUMMON_SUCCESS)
    -- เนรเทศตัวเองออกจากสุสานเพื่อนำการ์ดคู่ต่อสู้กลับไปวางไว้บนสุดของเด็ค (Spin to top deck)
    e2:SetCost(Cost.SelfBanish)
    ```

### 1.4 Destiny HERO - Dreadnought (101402037)
* **ประเภทการ์ด**: มอนสเตอร์ฟิวชันเอฟเฟกต์ (Level 8 / DARK / Warrior)
* **หน้าที่หลัก**: ตัวทำความเสียหายขนาดใหญ่ (Beater) และจั่วทรัพยากร (+2 ในมือ)
* **การวิเคราะห์โค้ด Lua ([c101402037.lua](file:///c:/Users/admin/Documents/EDOTh/repositories/delta-bagooska/script/pre-release/c101402037.lua))**:
  * **การอัญเชิญทางเลือก (Alternative Fusion Procedure)**:
    ```lua
    -- นอกจากการฟิวชันปกติแล้ว สามารถส่ง Destiny HERO - Dreadmaster ที่เราควบคุมลงสุสานเพื่ออัญเชิญพิเศษการ์ดใบนี้จากเอ็กซ์ตร้าเด็คได้โดยตรง
    e0a:SetCode(EFFECT_SPSUMMON_PROC)
    ```
  * **เอฟเฟกต์การเสิร์ชการ์ด 2 ใบ**:
    ```lua
    -- เมื่ออัญเชิญสำเร็จ จะสามารถเลือกการ์ด D-HERO หรือการ์ดระบุชื่อ 2 ใบขึ้นมือทันที
    e1:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH)
    ```
  * **ค่าพลังโจมตีมหาศาล**:
    ```lua
    -- พลังโจมตีจะเท่ากับผลรวมของพลังโจมตีตั้งต้นของ D-HERO ใบอื่นๆ ทั้งหมดบนสนามและในสุสาน
    e2:SetValue(function(e,c) return Duel.GetMatchingGroup(aux.FaceupFilter(Card.IsSetCard,SET_DESTINY_HERO),c:GetControler(),LOCATION_MZONE|LOCATION_GRAVE,0,c):GetSum(Card.GetBaseAttack) end)
    ```

### 1.5 Destiny HERO - Death Dogma (101402021)
* **ประเภทการ์ด**: มอนสเตอร์เอฟเฟกต์ (Level 10 / DARK / Warrior)
* **หน้าที่หลัก**: ตัวปิดเกมสร้างความเสียหาย (Burn Damage) และการฟิวชันในเทิร์นคู่ต่อสู้ (Quick Fusion)
* **การวิเคราะห์โค้ด Lua ([c101402021.lua](file:///c:/Users/admin/Documents/EDOTh/repositories/delta-bagooska/script/pre-release/c101402021.lua))**:
  * **การอัญเชิญพิเศษ**:
    ```lua
    -- อัญเชิญตัวเองจากมือหรือสุสานโดยการเนรเทศมอนสเตอร์ DARK/Warrior 3 ตัวในสุสาน
    e0:SetRange(LOCATION_HAND|LOCATION_GRAVE)
    ```
  * **การสร้างความเสียหายต่อเนื่อง**: Inflict 2000 damage แก่คู่ต่อสู้ในช่วง Standby Phase ถัดไปทันที
  * **การทำ Quick Fusion แทรกแซงเทิร์นคู่ต่อสู้**:
    ```lua
    -- เมื่อคู่ต่อสู้เปิดใช้งานเอฟเฟกต์การ์ด สามารถทำฟิวชันอัญเชิญมอนสเตอร์ DARK/Warrior โดยนำวัตถุดิบจากมือ สนาม หรือสุสานสับกลับเข้าเด็ค
    e2:SetType(EFFECT_TYPE_QUICK_O)
    e2:SetCode(EVENT_CHAINING)
    ```

### 1.6 D - Burst (100456010)
* **ประเภทการ์ด**: เวทมนตร์ปกติ (Normal Spell)
* **การทำงานหลัก**: ทำลายการ์ดเวทมนตร์เราเพื่อจั่ว + อัญเชิญพิเศษการ์ดที่ถูกนำออกนอกเกมหรือในสุสาน/มือ
* **GY Effect**: ช่วยให้มอนสเตอร์ที่มีการ์ดสวมใส่ หรือ `Destiny HERO - Dogma` สามารถประกาศโจมตีได้เป็นครั้งที่สองติดต่อกัน
* **การวิเคราะห์โค้ด Lua ([c100456010.lua](file:///c:/Users/admin/Documents/EDOTh/repositories/delta-bagooska/script/pre-release/c100456010.lua))**

### 1.7 การ์ดสนับสนุน Masked HERO (Main Deck)
* **Masked HERO Dusk Crow (10808715)**: อัญเชิญพิเศษตัวเองจากมือโดยเนรเทศ HERO จากสุสาน เมื่ออัญเชิญสำเร็จสามารถค้นหาการ์ด Masked HERO ใบอื่นขึ้นมือ (วิเคราะห์โค้ดใน [c10808715.lua](file:///c:/Users/admin/Documents/EDOTh/repositories/delta-bagooska/script/official/c10808715.lua))
* **Masked HERO Furnace (58288218)**: ทิ้งตัวมันเพื่อค้นหา Mask Change หรือ Polymerization และสามารถชุบตัวเองจากสุสาน/มือได้เมื่อมีการอัญเชิญพิเศษ Fusion มอนสเตอร์ที่ไม่ใช่ธาตุไฟ (วิเคราะห์โค้ดใน [c58288218.lua](file:///c:/Users/admin/Documents/EDOTh/repositories/delta-bagooska/script/official/c58288218.lua))
* **Masked HERO Fountain (66206748)**: อัญเชิญพิเศษ HERO จากมือ และหากตัวมันถูกส่งลงสุสานโดยเอฟเฟกต์หรือคอสต์ จะสามารถเซ็ต Mask Change จากเด็คหรือสุสานได้โดยตรง (วิเคราะห์โค้ดใน [c66206748.lua](file:///c:/Users/admin/Documents/EDOTh/repositories/delta-bagooska/script/official/c66206748.lua))

---

## 2. การจำแนกบทบาทและหน้าที่สำคัญของการ์ดในเด็ค (Deck Roles)

### 2.1 Ace Cards (การ์ดบอสหลักและตัวทำเกมปิดสนาม)
1. **Destiny HERO - Plasma**: บอสควบคุมสนาม ปิดเอฟเฟกต์มอนสเตอร์ฝั่งตรงข้ามทั้งหมด และดูดมอนสเตอร์มาเพิ่มพลังโจมตี
2. **Destiny HERO - Destroyer Phoenix Enforcer (DPE)**: บอสขัดขวางอเนกประสงค์ ทำลายการ์ดบนฟิลด์แบบ Quick Effect และเกิดใหม่ได้เรื่อย ๆ จากสุสาน
3. **Destiny HERO - Dreadnought**: มอนสเตอร์ฟิวชันเลเวล 8 มีดาเมจมหาศาลจากการสะสมพลังโจมตี D-HERO ในสุสาน/สนาม และบวกทรัพยากรบนมือ (+2 การ์ด)
4. **Destiny HERO - Death Dogma**: ทำดาเมจเผาพลังชีวิต 2000 LP และทำฟิวชันในเทิร์นตรงข้ามเพื่อรีไซเคิลทรัพยากร
5. **Contrast HERO Chaos**: บอสขัดขวางเอฟเฟกต์การ์ดหงายหน้าแบบ Quick Effect โดยใช้ Masked HERO 2 ใบฟิวชัน

### 2.2 Handtraps (การ์ดขัดขวางจากบนมือ)
1. **Mulcharmy Fuwalos**: การ์ดจั่วขัดขวางการกางบอร์ดของศัตรูในกรณีที่เราได้เริ่มเล่นทีหลัง (ใส่ 3 ใบ)
2. **Ash Blossom & Joyous Spring**: การ์ดหยุดยั้งการค้นหา/ส่งลงสุสาน/อัญเชิญพิเศษจากเด็ค (ใส่ 2 ใบ)
3. **Infinite Impermanence**: กับดักปิดเอฟเฟกต์มอนสเตอร์คู่แข่งจากมือ (ใส่ 3 ใบ)
4. **Dominus Spark** (ใน Side Deck): การ์ดทิ้งเนรเทศมอนสเตอร์ศัตรู (มีผลล็อคการใช้งานมอนสเตอร์ธาตุดิน/น้ำ/ลม/ไฟ ของเราหากใช้จากมือ)
5. **Droll & Lock Bird** (ใน Side Deck): สั่งปิดกั้นการเสิร์ชและการจั่วการ์ดทั้งหมดของทั้งสองฝ่ายหลังจากการค้นหาครั้งแรก

### 2.3 Protect Targets (การ์ดคีย์สำคัญที่ต้องคอยปกป้อง)
1. **Destiny HERO - Plasma และ D - Force**: Plasma มีพลังชีวิตต่ำและไม่มีเกราะป้องกันตัวเอง ต้องมี `D - Force` คอยเคลือบพลังอมตะและชี้เป้า รวมถึงระวังไม่ให้ `D - Force` โดนขัดขวางหรือทำลาย
2. **Destiny HERO - Destroyer Phoenix Enforcer (ในสุสาน)**: ระวังไม่ให้โดนนำออกนอกเกม (Banish) โดยการ์ดขัดขวางสุสาน เช่น `Called by the Grave` หรือ `D.D. Crow`
3. **ตัวเริ่มต้นคอมโบหลักบนสนาม (Vision HERO Vyon / Destiny HERO - Doom Liege)**: หากการประกาศใช้เอฟเฟกต์เริ่มต้นโดนหยุดยั้ง บอร์ดจะไม่สามารถก้าวต่อไปได้ ต้องปกป้องด้วยการล่อเป้าหรือใช้ `Called by the Grave` ขัดขวางกลับ

---

## 3. การวิเคราะห์สายคอมโบหลัก (Dreadnought Engine Combo Lines)

ด้วยเอฟเฟกต์แบบส่งเสริมกันอย่างลงตัว คอมโบของเด็คนี้สามารถรันได้อย่างมีประสิทธิภาพสูง โดยขอยกตัวอย่างสายคอมโบหลักดังนี้:

### [Combo Line 1] บอร์ดเริ่มต้นและเรียกเสิร์ชสูงสุด (1 Card Starter: Doom Liege)
* **มือเริ่มต้น**: `Destiny HERO - Doom Liege` 1 ใบ
* **ขั้นตอน**:
  1. **อัญเชิญแบบปกติ** `Destiny HERO - Doom Liege` (หรือหากมีการ์ดเปิดทางอื่นๆ)
  2. เปิดใช้งานเอฟเฟกต์ของ `Doom Liege`: ส่ง `Destiny HERO - Denier` จากเด็คลงสุสาน เพื่อเสิร์ชเวทมนตร์ฟิลด์ `Clock Tower Prison City - Dark City` ขึ้นมือ
  3. **เปิดใช้งานฟิลด์** `Clock Tower Prison City - Dark City` จากนั้นใช้เอฟเฟกต์ของฟิลด์เสิร์ช `Destiny HERO - Dreadnought Servant` ขึ้นมือ
  4. เนื่องจากเราควบคุมการ์ดฟิลด์อยู่ ให้ทำการ **Special Summon** `Destiny HERO - Dreadnought Servant` จากมือลงสู่สนาม
  5. เปิดใช้งานเอฟเฟกต์ของ `Dreadnought Servant`: เลือกทำลายฟิลด์ `Clock Tower Prison City - Dark City` เพื่อค้นหาการ์ด **`Polymerization`** ขึ้นมือ
  6. เอฟเฟกต์ของฟิลด์ทำงานเมื่อถูกทำลาย: ทำการ **Special Summon** `Destiny HERO - Dreadmaster` จากในเด็คลงสู่สนาม (ถือว่าเป็นการอัญเชิญผ่าน Clock Tower)
  7. เอฟเฟกต์ของ `Dreadmaster` ทำงาน:
     * ทำลายมอนสเตอร์อื่นๆ ทั้งหมดที่ไม่ใช่ D-HERO (ซึ่งสนามเรามีแต่ D-HERO)
     * **Special Summon** `Destiny HERO - Denier` จากสุสานกลับขึ้นมาบนสนาม
  8. เอฟเฟกต์ของ `Denier` ในสนามทำงาน: นำทรัพยากรหรือมอนสเตอร์ D-HERO ที่ถูกเนรเทศหรือในสุสานสับคืนเด็คเพื่อจัดเตรียมหน้าเด็คใหม่
  9. เอฟเฟกต์ของ `Dreadnought Servant` ในสุสานทำงาน (เนื่องจากมีการอัญเชิญมอนสเตอร์เลเวล 8 ประสบความสำเร็จ): เนรเทศตัวเองออกจากสุสานเพื่อ **Spin การ์ดคู่ต่อสู้กลับขึ้นบนสุดของเด็ค** 1 ใบ
  10. ทำการ **สังเวย** `Destiny HERO - Dreadmaster` บนสนาม เพื่ออัญเชิญพิเศษ **`Destiny HERO - Dreadnought`** จาก Extra Deck
  11. เอฟเฟกต์ของ `Dreadnought` ทำงานเมื่ออัญเชิญสำเร็จ: ค้นหาการ์ด D-HERO หรือการ์ดระบุชื่อเพิ่มขึ้นมือทันที **2 ใบ** (แนะนำให้เสิร์ช `Destiny HERO - Death Dogma` และการ์ดป้องกันอื่นๆ)
  12. ตอนนี้บนสนามเราจะมี `Destiny HERO - Dreadnought` (มีพลังโจมตีเพิ่มขึ้นมหาศาลจากผลรวมการ์ด D-HERO ในสุสานและสนาม) ร่วมกับ `Destiny HERO - Denier` และบนมือเราจะมี `Polymerization` รวมถึงการ์ดที่เพิ่งเสิร์ชมาอีก 2 ใบ พร้อมที่จะทำคอมโบต่อได้ทันที!

### [Combo Line 2] การทำ Quick Fusion และปิดเกมด้วย Death Dogma
* **ขั้นตอน**:
  1. ในช่วงท้ายเทิร์นหรือเมื่อทรัพยากรในสุสานมีมากพอ ให้ทำอัญเชิญพิเศษ `Destiny HERO - Death Dogma` จากมือหรือสุสานโดยการเนรเทศมอนสเตอร์ DARK/Warrior 3 ตัวออกจากสุสาน
  2. เมื่อเริ่มเทิร์นถัดไป (Standby Phase) เอฟเฟกต์ของ `Death Dogma` จะสร้างความเสียหายแก่คู่แข่งทันที 2000 LP
  3. ในเทิร์นของคู่ต่อสู้ เมื่อคู่ต่อสู้เริ่มเปิดใช้งานเอฟเฟกต์การ์ด:
     * สั่งการ Quick Effect ของ `Death Dogma`: ทำการฟิวชันมอนสเตอร์ฟิวชัน D-HERO เช่น **`Destiny HERO - Destroyer Phoenix Enforcer (DPE)`** หรือ **`Destiny HERO - Dominance`** แทรกเข้ามาในเชน โดยการนำวัตถุดิบในสุสานหรือสนามสับกลับเข้าเด็ค
     * วิธีนี้ช่วยให้เราสามารถเรียกบอสการ์ดบอร์ดขัดขวางศัตรูได้อย่างอิสระพร้อมทั้งยังได้รีไซเคิลทรัพยากร D-HERO กลับเข้าสู่เด็คไปพร้อมๆ กัน
