# Yu-Gi-Oh! Attack-Blocking Cards Lua Script Analysis
This document contains the detailed Lua script analysis of all **274** cards in the database that implement field-wide or self attack blocking/restriction effects.

---

## [1] "Infernoble Arms - Hauteclere" (ID: 64867422)
**Lua File:** `script\official\c64867422.lua`

**Description:**
> ในขณะที่การ์ดใบนี้ติดตั้งให้กับมอนสเตอร์: คุณสามารถเลือกเป้าหมายมอนสเตอร์หงายหน้าที่คุณควบคุม 1 ตัว; เทิร์นนี้ คุณไม่สามารถประกาศโจมตีได้ ยกเว้นกับมอนสเตอร์นั้น และมอนสเตอร์นั้นจะได้รับความสามารถในการโจมตีครั้งที่สองระหว่าง Battle Phase แต่ละครั้งในเทิร์นนี้ จากนั้นทำลายการ์ดใบนี้ หากการ์ดใบนี้ถูกส่งลงสุสานเพราะมอนสเตอร์ที่ติดตั้งถูกส่งลงสุสาน: คุณสามารถเลือกเป้าหมายมอนสเตอร์หงายหน้าบนฟิลด์ 1 ตัว; ทำลายมัน คุณสามารถใช้เอฟเฟกต์ของ 'Infernoble Arms - Hauteclere' ได้เพียง 1 เอฟเฟกต์ต่อเทิร์น และใช้ได้เพียงครั้งเดียวในเทิร์นนั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Other monsters cannot attack
		local e1=Effect.CreateEffect(c)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetLabelObject(tc)
		e1:SetTarget(s.atktg)
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
		--Can make a second attack
		local e2=Effect.CreateEffect(c)
		e2:SetDescription(3201)
```

---

## [2] 'Artifacts Unleashed' (ID: 56611470)
**Lua File:** `script\official\c56611470.lua`

**Description:**
> เลือกเป้าหมายมอนสเตอร์ 'Artifact' 2 ตัวที่คุณควบคุม; ทันทีหลังจากเอฟเฟกต์นี้แก้ไข ให้อัญเชิญเอ็กซีสมอนสเตอร์ 1 ตัวโดยใช้มอนสเตอร์ 2 ตัวนั้นเท่านั้น และตลอดช่วงที่เหลือของเทิร์นนี้หลังจากที่การ์ดใบนี้แก้ไข มอนสเตอร์ที่คุณควบคุมไม่สามารถโจมตีได้ ยกเว้นมอนสเตอร์ 'Artifact' หากการ์ดใบนี้ที่คุณครอบครองถูกทำลายโดยฝ่ายตรงข้าม: คุณสามารถเปิดเผยมอนสเตอร์ธาตุแสง เลเวล 5 1 ตัวจากมือของคุณ; จั่วการ์ด 1 ใบ

* ข้อความข้างต้นไม่เป็นทางการและอธิบายการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.activate(e,tp,eg,ep,ev,re,r,rp)
	if e:IsHasType(EFFECT_TYPE_ACTIVATE) then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetTarget(s.attg)
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
	end
	local g=Duel.GetTargetCards(e):Match(Card.IsFaceup,nil)
	if #g<2 then return end
```

---

## [3] 'Beacon of White' (ID: 50371210)
**Lua File:** `script\official\c50371210.lua`

**Description:**
> หากคุณไม่มี "Beacon of White" ใบอื่นควบคุม และคุณมีมอนสเตอร์ "Blue-Eyes" 3 ตัวขึ้นไปในสุสานของคุณ: เลือกเป้าหมาย 1 ตัวในนั้น; อัญเชิญแบบพิเศษออกมา แต่เอฟเฟกต์ของมันถูกยกเลิก และสวมใสการ์ดนี้ให้กับมันด้วย เมื่อการ์ดนี้ออกจากฟิลด์ ให้นำมอนสเตอร์ที่สวมใสออกนอกเกม มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ และหากคุณมีมอนสเตอร์ "Blue-Eyes" จำนวนเท่าใดก็ได้ในสุสานของคุณ มอนสเตอร์ที่สวมใสสามารถโจมตีได้สูงสุดตามจำนวนนั้นในแต่ละ Battle Phase

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_CONTINUOUS)
	e3:SetCode(EVENT_LEAVE_FIELD)
	e3:SetOperation(s.rmop)
	c:RegisterEffect(e3)
	--cannot attack
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_FIELD)
	e4:SetCode(EFFECT_CANNOT_ATTACK)
	e4:SetRange(LOCATION_SZONE)
	e4:SetTargetRange(LOCATION_MZONE,0)
	e4:SetTarget(s.ftarget)
	c:RegisterEffect(e4)
end
s.listed_series={SET_BLUE_EYES}
s.listed_names={id}
function s.spfilter(c,e,tp)
	return c:IsSetCard(SET_BLUE_EYES) and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end
function s.gvfilter(c)
	return c:IsSetCard(SET_BLUE_EYES)
end
function s.cfilter(c)
	return c:IsFaceup() and c:IsCode(id)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	if chkc then return chkc:IsLocation(LOCATION_GRAVE) and chkc:IsControler(tp) and s.spfilter(chkc,e,tp) end
	if chk==0 then return Duel.GetLocationCount(tp,LOCATION_MZONE)>0
		and Duel.IsExistingTarget(s.spfilter,tp,LOCATION_GRAVE,0,1,nil,e,tp)
		and Duel.IsExistingMatchingCard(s.gvfilter,tp,LOCATION_GRAVE,0,3,nil)
		and not Duel.IsExistingMatchingCard(s.cfilter,tp,LOCATION_ONFIELD,0,1,e:GetHandler()) end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
	local g=Duel.SelectTarget(tp,s.spfilter,tp,LOCATION_GRAVE,0,1,1,nil,e,tp)
	Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,g,1,0,0)
	Duel.SetOperationInfo(0,CATEGORY_EQUIP,e:GetHandler(),1,0,0)
end
function s.eqlimit(e,c)
	return e:GetOwner()==c
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local tc=Duel.GetFirstTarget()
	if c:IsRelateToEffect(e) and tc:IsRelateToEffect(e) then
		if Duel.SpecialSummonStep(tc,0,tp,tp,false,false,POS_FACEUP)==0 then return end
		Duel.Equip(tp,c,tc)
		--Add Equip limit
		local e1=Effect.CreateEffect(tc)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_EQUIP_LIMIT)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		e1:SetValue(s.eqlimit)
		c:RegisterEffect(e1)
		--Disable
		local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_SINGLE)
		e2:SetCode(EFFECT_DISABLE)
		e2:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e2)
		local e3=e2:Clone()
		e3:SetCode(EFFECT_DISABLE_EFFECT)
		tc:RegisterEffect(e3)
```

---

## [4] 'Blockman' (ID: 48115277)
**Lua File:** `script\official\c48115277.lua`

**Description:**
> คุณสามารถสังเวยการ์ดใบนี้; อัญเชิญแบบพิเศษในตำแหน่งป้องกัน 'Block Tokens' จำนวนเท่ากับจำนวนเทิร์นของคุณที่การ์ดใบนี้ถูกหงายหน้าบนฟิลด์ของคุณ (ประเภทหิน/โลก/เลเวล 4/ATK 1000/DEF 1500) โทเคนเหล่านี้ไม่สามารถประกาศโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
Duel.SpecialSummonStep(token,0,tp,tp,false,false,POS_FACEUP_DEFENSE)
			local e1=Effect.CreateEffect(e:GetHandler())
			e1:SetType(EFFECT_TYPE_SINGLE)
			e1:SetCode(EFFECT_CANNOT_ATTACK)
			e1:SetReset(RESET_EVENT|RESETS_STANDARD)
			token:RegisterEffect(e1,true)
		end
		Duel.SpecialSummonComplete()
	end
```

---

## [5] 'Blue-Eyes Alternative White Dragon' (ID: 38517737)
**Lua File:** `script\official\c38517737.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษก่อน (จากมือ) โดยเปิดเผย "Blue-Eyes White Dragon" ในมือของคุณ คุณสามารถอัญเชิญแบบพิเศษ "Blue-Eyes Alternative White Dragon" ด้วยวิธีนี้เทิร์นละครั้งเท่านั้น ชื่อการ์ดใบนี้กลายเป็น "Blue-Eyes White Dragon" ขณะอยู่บนฟิลด์หรือในสุสาน เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่ฝ่ายตรงข้ามควบคุม 1 ตัว; ทำลายมัน การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นที่เปิดใช้เอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [6] 'Blue-Eyes Toon Dragon' (ID: 53183600)
**Lua File:** `script\official\c53183600.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ทได้ ต้องอัญเชิญแบบพิเศษ (จากมือของคุณ) ก่อนโดยการสังเวยมอนสเตอร์ 2 ตัว ขณะที่คุณควบคุม "Toon World" ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญแบบพิเศษ คุณต้องจ่าย LP 500 เพื่อประกาศโจมตีด้วยมอนสเตอร์นี้ ถ้า "Toon World" บนฟิลด์ถูกทำลาย ทำลายการ์ดใบนี้ สามารถโจมตีคู่ต่อสู้ของคุณโดยตรง เว้นแต่พวกเขาจะควบคุมมอนสเตอร์ Toon ในกรณีนั้น การ์ดใบนี้ต้องเลือกมอนสเตอร์ Toon เป็นเป้าหมายในการโจมตี

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_DIRECT_ATTACK)
```lua
e4:SetCode(EFFECT_CANNOT_SELECT_BATTLE_TARGET)
	e4:SetCondition(s.atcon)
	e4:SetValue(s.atlimit)
	c:RegisterEffect(e4)
	local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_SINGLE)
	e5:SetCode(EFFECT_CANNOT_DIRECT_ATTACK)
	e5:SetCondition(s.atcon)
	c:RegisterEffect(e5)
	--Cannot attack
	local e6=Effect.CreateEffect(c)
	e6:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_CONTINUOUS)
	e6:SetCode(EVENT_SPSUMMON_SUCCESS)
	e6:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e6:SetOperation(s.atklimit)
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [7] 'Crackdown' (ID: 36975314)
**Lua File:** `script\official\c36975314.lua`

**Description:**
> เปิดใช้งานโดยเลือกมอนสเตอร์หงายหน้า 1 ตัวที่ฝ่ายตรงข้ามควบคุม ยึดการควบคุมมอนสเตอร์นั้น ในขณะที่คุณควบคุมมอนสเตอร์นั้น มันไม่สามารถโจมตีหรือเปิดใช้งานเอฟเฟกต์ได้ เมื่อมอนสเตอร์นั้นออกจากฟิลด์ ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e3:SetTarget(aux.PersistentTargetFilter)
	e3:SetValue(s.tg)
	c:RegisterEffect(e3)
	--cannot attack
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_FIELD)
	e4:SetCode(EFFECT_CANNOT_ATTACK)
	e4:SetRange(LOCATION_SZONE)
	e4:SetTargetRange(LOCATION_MZONE,0)
	e4:SetTarget(aux.PersistentTargetFilter)
	c:RegisterEffect(e4)
	--cannot activate
	local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_FIELD)
	e5:SetCode(EFFECT_CANNOT_TRIGGER)
	e5:SetRange(LOCATION_SZONE)
```

---

## [8] 'Crusadia Spatha' (ID: 39528955)
**Lua File:** `script\official\c39528955.lua`

**Description:**
> มอนสเตอร์เอฟเฟกต์ 2 ตัว รวมถึงมอนสเตอร์ "Crusadia" 1 ตัว
ได้รับ ATK เท่ากับ ATK เดิมของมอนสเตอร์ใดก็ตามที่การ์ดใบนี้ชี้ไป มอนสเตอร์ที่การ์ดใบนี้ชี้ไปไม่สามารถโจมตีได้ เทิร์นละครั้ง หากมอนสเตอร์เอฟเฟกต์ถูกอัญเชิญแบบพิเศษมายังโซนที่การ์ดใบนี้ชี้ไป (ยกเว้นในช่วง Damage Step): คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวใน Main Monster Zone ของผู้เล่นคนใดก็ได้ ยกเว้นการ์ดใบนี้; ย้ายมันไปยัง Main Monster Zone อีกโซนบนฟิลด์ของผู้เล่นที่ควบคุมมัน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetRange(LOCATION_MZONE)
	e1:SetValue(s.atkval)
	c:RegisterEffect(e1)
	--Prevent attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.atklimit)
	c:RegisterEffect(e2)
	--Move
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,0))
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_O)
```

---

## [9] 'Cryomancer of the Ice Barrier' (ID: 23950192)
**Lua File:** `script\official\c23950192.lua`

**Description:**
> ขณะที่คุณควบคุมมอนสเตอร์ 'Ice Barrier' ตัวอื่น มอนสเตอร์เลเวล 4 หรือสูงกว่าไม่สามารถประกาศการโจมตี

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.tg)
	e2:SetCondition(s.con)
	c:RegisterEffect(e2)
```
Associated helper functions:
```lua
function s.tg(e,c)
	return c:GetLevel()>=4
end
```
```lua
function s.con(e)
	return Duel.IsExistingMatchingCard(aux.FaceupFilter(Card.IsSetCard,SET_ICE_BARRIER),e:GetHandler():GetControler(),LOCATION_MZONE,0,1,e:GetHandler())
end
```

---

## [10] 'Cubic Mandala' (ID: 8837932)
**Lua File:** `script\official\c8837932.lua`

**Description:**
> หากคุณควบคุมมอนสเตอร์ "Cubic": เปิดใช้งานการ์ดใบนี้โดยเลือกเป้าหมายมอนสเตอร์ตามจำนวนที่อยู่ในสุสานของคู่ต่อสู้เพราะถูกทำลายและส่งไปที่นั่นในเทิร์นนี้; อัญเชิญแบบพิเศษพวกมันไปยังฟิลด์ของคู่ต่อสู้ แต่แต่ละตัวมี ATK 0 และมี Cubic Counter 1 ตัววางอยู่ (มอนสเตอร์ที่มี Cubic Counter ไม่สามารถโจมตีได้ และยกเลิกเอฟเฟกต์ของพวกมัน) ในขณะที่คู่ต่อสู้ควบคุมมอนสเตอร์ใดๆ ที่ถูกอัญเชิญโดยเอฟเฟกต์นี้ ให้ยกเลิกเอฟเฟกต์มอนสเตอร์ที่คู่ต่อสู้เปิดใช้งาน เมื่อมอนสเตอร์สุดท้ายเหล่านี้ออกจากฟิลด์ ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_SINGLE)
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		e2:SetCondition(function(e) return e:GetHandler():GetCounter(COUNTER_CUBIC)>0 end)
		e2:SetReset(RESET_EVENT|RESETS_STANDARD)
		oc:RegisterEffect(e2)
```

---

## [11] 'Enishi, Shien's Chancellor' (ID: 38280762)
**Lua File:** `script\official\c38280762.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือ) โดยการนำมอนสเตอร์ 'Six Samurai' 2 ตัวจากสุสานของคุณออกนอกเกม เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่หงายหน้าอยู่ 1 ตัว; ทำลายเป้าหมายนั้น การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นที่เปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e:GetHandler():RegisterEffect(e1)
```

---

## [12] 'Heroic Challenger - Knuckle Sword' (ID: 71549257)
**Lua File:** `script\official\c71549257.lua`

**Description:**
> หากคุณควบคุมมอนสเตอร์ "Heroic" ยกเว้นมอนสเตอร์ระดับ 1: คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้จากมือของคุณ หากการ์ดใบนี้ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษ: คุณสามารถเลือกเป้าหมายมอนสเตอร์ประเภท Warrior อีก 1 ตัวที่คุณควบคุมซึ่งมีระดับ; ระดับของมอนสเตอร์นั้นหรือการ์ดใบนี้จะกลายเป็นระดับของอีกตัวหนึ่ง และคุณไม่สามารถประกาศโจมตีได้ในช่วงที่เหลือของเทิร์นนี้ ยกเว้นด้วยมอนสเตอร์ Xyz คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Heroic Challenger - Knuckle Sword" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
oc:RegisterEffect(e1)
	end
	--Cannot declare attacks, except with Xyz Monsters
	local ge1=Effect.CreateEffect(c)
	ge1:SetType(EFFECT_TYPE_FIELD)
	ge1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	ge1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	ge1:SetTargetRange(LOCATION_MZONE,0)
	ge1:SetTarget(function(e,c) return not c:IsType(TYPE_XYZ) end)
	ge1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(ge1,tp)
```

---

## [13] 'Malefic Blue-Eyes White Dragon' (ID: 9433350)
**Lua File:** `script\official\c9433350.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือ) ก่อนโดยการนำ 'Blue-Eyes White Dragon' 1 ตัวจากเด็คของคุณออกนอกเกม มอนสเตอร์ 'Malefic' สามารถมีได้บนฟิลด์เพียง 1 ตัวเท่านั้น มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถประกาศโจมตีได้ หากไม่มีเวทมนตร์สนามที่หงายหน้าอยู่บนฟิลด์ ให้ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e7:SetCondition(s.descon)
	c:RegisterEffect(e7)
	--cannot announce
	local e8=Effect.CreateEffect(c)
	e8:SetType(EFFECT_TYPE_FIELD)
	e8:SetRange(LOCATION_MZONE)
	e8:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e8:SetTargetRange(LOCATION_MZONE,0)
	e8:SetTarget(s.antarget)
	c:RegisterEffect(e8)
end
s.listed_names={CARD_BLUEEYES_W_DRAGON}
function s.descon(e)
	return not Duel.IsExistingMatchingCard(Card.IsFaceup,0,LOCATION_FZONE,LOCATION_FZONE,1,nil)
end
```

---

## [14] 'Photon Delta Wing' (ID: 47051709)
**Lua File:** `script\official\c47051709.lua`

**Description:**
> หากการ์ดใบนี้ถูกอัญเชิญแบบปกติ: คุณสามารถอัญเชิญ "Photon Delta Wing" 1 ใบจากมือหรือเด็คในรูปแบบตั้งป้องกันแบบพิเศษ และคุณไม่สามารถอัญเชิญแบบพิเศษได้ตลอดเทิร์นที่เหลือ ยกเว้นมอนสเตอร์แสง ฝ่ายตรงข้ามไม่สามารถประกาศโจมตีได้ในขณะที่คุณควบคุม "Photon Delta Wing" ใบอื่น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetProperty(EFFECT_FLAG_PLAYER_TARGET)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(0,1)
	e2:SetCondition(function(e) return Duel.IsExistingMatchingCard(aux.FaceupFilter(Card.IsCode,id),e:GetHandlerPlayer(),LOCATION_ONFIELD,0,1,e:GetHandler()) end)
	c:RegisterEffect(e2)
```

---

## [15] 'Shien's Advisor' (ID: 98126725)
**Lua File:** `script\official\c98126725.lua`

**Description:**
> หากการ์ดใบนี้ถูกอัญเชิญแบบปกติในขณะที่คุณควบคุมมอนสเตอร์ 'Six Samurai': ประกาศประเภทมอนสเตอร์ 1 ประเภท; ขณะที่การ์ดใบนี้หงายหน้าอยู่บนฟิลด์ มอนสเตอร์ประเภทที่ประกาศไว้ไม่สามารถประกาศโจมตีหรือถูกอัญเชิญแบบพิเศษ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetRange(LOCATION_MZONE)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetTarget(s.tglimit)
	e1:SetLabel(e:GetLabel())
	e1:SetReset(RESET_EVENT|RESETS_STANDARD)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.tglimit(e,c)
	return c:IsRace(e:GetLabel())
end
```

---

## [16] 'Tainted of the Tistina' (ID: 50281477)
**Lua File:** `script\official\c50281477.lua`

**Description:**
> คุณสามารถอัญเชิญแบบสังเวยการ์ดใบนี้หงายหน้าโดยการสังเวยมอนสเตอร์ที่คว่ำหน้าอยู่ที่ฝ่ายตรงข้ามควบคุม 1 ตัว เลเวลของการ์ดใบนี้กลายเป็น 10 หากถูกอัญเชิญแบบปกติ/เซ็ต หากการ์ดใบนี้ถูกส่งไปยังสุสาน ยกเว้นจากฟิลด์: คุณสามารถเลือกเป้าหมายมอนสเตอร์ "Tistina" ที่คุณควบคุม 1 ตัว; เทิร์นนี้ มอนสเตอร์นั้นสามารถโจมตีครั้งที่สองระหว่างแต่ละ Battle Phase ได้ นอกจากนี้คุณสามารถโจมตีด้วยมอนสเตอร์เพียง 1 ตัวเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
tc:RegisterEffect(e1)
	end
	--Can only attack with 1 monster this turn
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetCondition(function(e) return e:GetLabel()~=0 end)
	e1:SetTarget(function(e,c) return c:GetFieldID()~=e:GetLabel() end)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	local e2=Effect.CreateEffect(c)
```

---

## [17] 'Tri-Brigade Stand-Off' (ID: 25908748)
**Lua File:** `script\official\c25908748.lua`

**Description:**
> คุณไม่สามารถอัญเชิญแบบพิเศษมอนสเตอร์จากเอ็กซ์ตร้าเด็คได้ ยกเว้นมอนสเตอร์สัตว์ร้าย สัตว์ร้ายนักรบ หรือสัตว์ปีก คุณสามารถส่งมอนสเตอร์ 1 ใบจากมือหรือฟิลด์ของคุณลงสุสาน; เพิ่มมอนสเตอร์ "Tri-Brigade" 1 ใบจากเด็คของคุณขึ้นมือที่มีประเภทเดิมแตกต่างจากมอนสเตอร์ที่ส่งลงสุสานนั้น คุณสามารถใช้เอฟเฟกต์นี้ของ "Tri-Brigade Stand-Off" ได้เทิร์นละครั้งเท่านั้น หากการ์ดใบนี้ในโซนเวทมนตร์และกับดักของเจ้าของถูกทำลายด้วยเอฟเฟกต์การ์ดของฝ่ายตรงข้าม: คุณสามารถเปิดใช้เอฟเฟกต์นี้; ฝ่ายตรงข้ามไม่สามารถประกาศการโจมตีในเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.limop(e,tp,eg,ep,ev,re,r,rp)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_CLIENT_HINT)
	e1:SetDescription(aux.Stringid(id,2))
	e1:SetTargetRange(0,1)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
```

---

## [18] 'ZS - Ouroboros Sage' (ID: 32281491)
**Lua File:** `script\official\c32281491.lua`

**Description:**
> เมื่อการ์ดใบนี้ถูกอัญเชิญแบบปกติ: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์ 'Number' ที่ไม่ใช่ LIGHT 1 ตัวจากสุสานของคุณ, แต่ยกเลิกเอฟเฟกต์ของมัน, และถ้าคุณทำเช่นนั้น, สวมใส่ทั้งการ์ดใบนี้และมอนสเตอร์ 'Utopia' 1 ตัวที่คุณควบคุมให้กับมัน, แต่ละใบเป็น Equip Spell ที่ให้มันได้รับ ATK 1700, นอกจากนี้คุณสามารถประกาศโจมตีได้เพียงครั้งเดียวตลอดช่วงที่เหลือของเทิร์นนี้ เมื่อมอนสเตอร์ที่สวมใส่การ์ดใบนี้ด้วยเอฟเฟกต์ของการ์ดใบนี้ประกาศโจมตีบนมอนสเตอร์ของคู่ต่อสู้: คุณสามารถเพิ่ม ATK ของมอนสเตอร์ที่โจมตีเป็นสองเท่า, แต่ทำลายมันในช่วง End Phase

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(1,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.equipop(c,uc,e,tp,tc)
	local g=Group.FromCards(c,uc)
	for ec in g:Iter() do
		if not aux.EquipAndLimitRegister(ec,e,tp,tc) then return end
		--Increase ATK
		local e1=Effect.CreateEffect(ec)
		e1:SetType(EFFECT_TYPE_EQUIP)
		e1:SetCode(EFFECT_UPDATE_ATTACK)
		e1:SetValue(1700)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		ec:RegisterEffect(e1)
```

---

## [19] Abyss Actor - Twinkle Little Star (ID: 7279373)
**Lua File:** `script\official\c7279373.lua`

**Description:**
> [ เอฟเฟกต์เพนดูลั่ม ]
คุณไม่สามารถอัญเชิญเพนดูลั่มมอนสเตอร์ได้ ยกเว้นมอนสเตอร์ "Abyss Actor" เอฟเฟกต์นี้ไม่สามารถถูกยกเลิกได้ เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ "Abyss Actor" 1 ตัวที่คุณควบคุม; ในเทิร์นนี้ มอนสเตอร์นั้นสามารถโจมตีมอนสเตอร์ได้สูงสุด 3 ครั้งในแต่ละ Battle Phase และมอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ในเทิร์นที่เหลือ (แม้การ์ดใบนี้จะออกจากฟิลด์)
----------------------------------------
[ เอฟเฟกต์มอนสเตอร์ ]
ไม่สามารถถูกทำลายในการต่อสู้ระหว่างเทิร์นของคุณ การ์ดใบนี้สามารถโจมตีมอนสเตอร์ได้สูงสุด 3 ครั้งในแต่ละ Battle Phase

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
tc:RegisterEffect(e1)
	end
	--Other monsters you control cannot attack for the rest of this turn
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(function(e,c) return fid==0 or c:GetRealFieldID()~=fid end)
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
	aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,2))
```

---

## [20] Abyss-squall (ID: 34707034)
**Lua File:** `script\official\c34707034.lua`

**Description:**
> เลือกเป้าหมายมอนสเตอร์ "Mermail" 3 ตัวในสุสานของคุณ; อัญเชิญแบบพิเศษเป้าหมายเหล่านั้นในท่า Defense หงายหน้า เอฟเฟกต์ของพวกมันถูกยกเลิก และพวกมันไม่สามารถประกาศโจมตีได้ ทำลายพวกมันในช่วง End Phase

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1)
```

---

## [21] Air Cracking Storm (ID: 98864751)
**Lua File:** `script\official\c98864751.lua`

**Description:**
> สวมใส่ให้กับมอนสเตอร์ประเภทเครื่องจักรเท่านั้น เมื่อมอนสเตอร์ที่โจมตีนั้นทำลายมอนสเตอร์ของคู่ต่อสู้ด้วยการต่อสู้: คุณสามารถเปิดใช้งานเอฟเฟกต์นี้; มันสามารถโจมตีครั้งที่สองในระหว่าง Battle Phase นี้ มอนสเตอร์อื่นของคุณไม่สามารถโจมตีในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(c:GetEquipTarget():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.drop(e,tp,eg,ep,ev,re,r,rp)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_EXTRA_ATTACK)
	e1:SetValue(1)
	e1:SetReset(RESET_EVENT|RESETS_STANDARD|RESET_PHASE|PHASE_BATTLE)
	e:GetHandler():GetEquipTarget():RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [22] Alien Psychic (ID: 58012107)
**Lua File:** `script\official\c58012107.lua`

**Description:**
> การ์ดใบนี้เปลี่ยนเป็นตำแหน่งป้องกันเมื่อถูกอัญเชิญแบบปกติหรืออัญเชิญแบบฟลิป มอนสเตอร์ที่มี A-Counter ไม่สามารถประกาศโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetRange(LOCATION_MZONE)
	e3:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e3:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e3:SetTarget(s.atktg)
	c:RegisterEffect(e3)
```
Associated helper functions:
```lua
function s.atktg(e,c)
	return c:GetCounter(COUNTER_A)>0
end
```

---

## [23] All-Eyes Phantom Dragon (ID: 70335319)
**Lua File:** `script\official\c70335319.lua`

**Description:**
> [ เอฟเฟกต์พินดูลั่ม ]
เทิร์นละครั้ง, เมื่อสิ้นสุด Damage Step, หากมอนสเตอร์พินดูลั่มมังกรของคุณโจมตีมอนสเตอร์ของฝ่ายตรงข้าม: คุณสามารถเปิดใช้งานเอฟเฟกต์นี้; มอนสเตอร์นั้นสามารถโจมตีอีกครั้งติดต่อกันได้ คุณไม่สามารถประกาศโจมตีในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้ ยกเว้นกับมอนสเตอร์นั้น
----------------------------------------
[ เอฟเฟกต์มอนสเตอร์ ]
ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือหรือเอ็กซ์ตร้าเด็คที่หงายหน้า) โดยการสังเวยมอนสเตอร์ที่คุณควบคุมทั้งหมด (อย่างน้อย 2 ตัว) รวมถึงมอนสเตอร์พินดูลั่มมังกร 1 ตัว คุณสามารถอัญเชิญแบบพิเศษ "All-Eyes Phantom Dragon" ด้วยวิธีนี้เทิร์นละครั้งเท่านั้น เทิร์นละครั้ง, ในระหว่างการคำนวณความเสียหาย หากการ์ดใบนี้ต่อสู้กับมอนสเตอร์ของฝ่ายตรงข้าม: เพิ่ม ATK ปัจจุบันของการ์ดใบนี้เป็นสองเท่าจนกระทั่งจบเทิร์นนี้ เทิร์นละครั้ง, เมื่อฝ่ายตรงข้ามเปิดใช้งานการ์ดเวทมนตร์/กับดักหรือเอฟเฟกต์ (ควิกเอฟเฟกต์): คุณสามารถส่งเวทมนตร์/กับดักที่คุณควบคุม 1 ใบลงสุสาน; ยกเลิกการเปิดใช้งาน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(at:GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.catg(e,tp,eg,ep,ev,re,r,rp,chk)
	local at=Duel.GetAttacker()
	if chk==0 then return at and at:CanChainAttack() end
end
function s.caop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local at=Duel.GetAttacker()
	if at and at:IsRelateToBattle() and at:IsControler(tp) then Duel.ChainAttack() end
end
function s.atkcon(e,tp,eg,ep,ev,re,r,rp)
	return e:GetHandler():GetBattleTarget()~=nil
end
function s.atkop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	if c:IsRelateToEffect(e) and c:IsFaceup() then
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_SET_ATTACK_FINAL)
		e1:SetValue(c:GetAttack()*2)
		e1:SetReset(RESETS_STANDARD_DISABLE_PHASE_END)
		c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [24] Altergeist Kidolga (ID: 76685519)
**Lua File:** `script\official\c76685519.lua`

**Description:**
> มอนสเตอร์ "Altergeist" 2 ตัว
เมื่อมอนสเตอร์ "Altergeist" ตัวอื่นที่คุณควบคุมสร้างความเสียหายต่อสู้ให้กับฝ่ายตรงข้าม: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ใบในสุสานของพวกเขา; อัญเชิญแบบพิเศษมันไปยังโซนที่การ์ดใบนี้ชี้ไป แต่ในแต่ละเทิร์น มันไม่สามารถโจมตีได้ เว้นแต่การ์ดใบนี้ได้ประกาศการโจมตีในเทิร์นนั้นแล้ว หากการ์ดใบนี้ถูกทำลายในการต่อสู้: คุณสามารถเลือกเป้าหมายการ์ด "Altergeist" 1 ใบในสุสานของคุณ; นำมันขึ้นมือ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetDescription(aux.Stringid(id,2))
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetCondition(function(e) return not e:GetHandler():HasFlagEffect(id) end)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1)
```

---

## [25] Amazoness Call (ID: 57312333)
**Lua File:** `script\official\c57312333.lua`

**Description:**
> นำการ์ด "Amazoness" 1 ใบจากเด็คของคุณ ยกเว้น "Amazoness Call" แล้วเพิ่มขึ้นมือหรือส่งลงสุสาน ในช่วง Main Phase ของคุณ: คุณสามารถนำการ์ดนี้ออกจากเกมจากสุสานของคุณ จากนั้นเลือกเป้าหมายมอนสเตอร์ "Amazoness" 1 ตัวที่คุณควบคุม; เทิร์นนี้ มอนสเตอร์นั้นสามารถโจมตีมอนสเตอร์ทั้งหมดที่ฝ่ายตรงข้ามควบคุมได้ครั้งละ 1 ตัว และมอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ คุณสามารถเปิดใช้งาน "Amazoness Call" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetValue(1)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
	end
	local e2=Effect.CreateEffect(e:GetHandler())
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.ftarget)
	e2:SetLabel(tc:GetFieldID())
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
```

---

## [26] Amazoness Pet Liger King (ID: 59353647)
**Lua File:** `script\official\c59353647.lua`

**Description:**
> มอนสเตอร์ "Amazoness" เลเวล 5 หรือสูงกว่า 1 ตัว + มอนสเตอร์ "Amazoness" 1 ตัว
มอนสเตอร์ของฝ่ายตรงข้ามไม่สามารถโจมตีมอนสเตอร์ใด ๆ ได้ยกเว้นการ์ดใบนี้ คุณสามารถเลือกเป้าหมายการ์ด "Amazoness" 1 ใบที่คุณควบคุม และมอนสเตอร์นักรบ "Amazoness" 1 ตัวในสุสานของคุณ ทำลายการ์ดนั้นบนฟิลด์ และถ้าทำเช่นนั้น อัญเชิญแบบพิเศษมอนสเตอร์อีกตัวนั้นจากสุสาน การ์ดใบนี้ไม่สามารถโจมตีได้ในเทิร์นที่เปิดใช้เอฟเฟกต์นี้ คุณสามารถใช้เอฟเฟกต์นี้ของ "Amazoness Pet Liger King" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	c:RegisterEffect(e1,true)
end
function s.desfilter(c,tp)
	return c:IsFaceup() and c:IsSetCard(SET_AMAZONESS) and Duel.GetMZoneCount(tp,c)>0
end
function s.spfilter(c,e,tp)
```

---

## [27] Amazoness War Chief (ID: 50486289)
**Lua File:** `script\official\c50486289.lua`

**Description:**
> ถ้าคุณไม่มีมอนสเตอร์ควบคุม หรือมีแค่มอนสเตอร์ 'Amazoness' เท่านั้น: คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้จากมือของคุณ ถ้าการ์ดใบนี้ถูกอัญเชิญแบบปกติหรือแบบพิเศษ: คุณสามารถเซ็ตการ์ดเวทมนตร์/กับดัก 'Amazoness' 1 ใบหรือ 'Polymerization' 1 ใบจากเด็คของคุณโดยตรงด้วย นอกจากนี้คุณสามารถโจมตีด้วยมอนสเตอร์ 'Amazoness' เท่านั้นในเทิร์นที่เหลือ คุณสามารถใช้แต่ละเอฟเฟกต์ของ 'Amazoness War Chief' ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if tc then Duel.SSet(tp,tc) end
	local c=e:GetHandler()
	--Cannot attack, except with "Amazoness" monsters
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(function(e,c) return not c:IsSetCard(SET_AMAZONESS) end)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
```

---

## [28] Andro Sphinx (ID: 15013468)
**Lua File:** `script\official\c15013468.lua`

**Description:**
> คุณสามารถจ่าย 500 ไลฟ์พอยท์เพื่ออัญเชิญแบบพิเศษการ์ดใบนี้เมื่อ "Pyramid of Light" อยู่บนฟิลด์ การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญแบบปกติหรือแบบพิเศษ การ์ดใบนี้ไม่สามารถอัญเชิญแบบพิเศษจากสุสานได้ หากการ์ดใบนี้ทำลายมอนสเตอร์ในตำแหน่งป้องกันอันเป็นผลมาจากการต่อสู้ ให้สร้างความเสียหายแก่ไลฟ์พอยท์ของฝ่ายตรงข้ามเท่ากับครึ่งหนึ่งของ ATK ของมอนสเตอร์ที่ถูกทำลาย

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [29] Anesthrokket Dragon (ID: 53266486)
**Lua File:** `script\official\c53266486.lua`

**Description:**
> เมื่อเอฟเฟกต์ของมอนสเตอร์ลิงก์ถูกเปิดใช้งานที่เลือกเป้าการ์ดที่เปิดอยู่บนฟิลด์นี้ (ควิกเอฟเฟกต์): คุณสามารถทำลายการ์ดใบนี้ จากนั้นทำให้มอนสเตอร์ที่เปิดอยู่บนฟิลด์ 1 ตัวไม่สามารถโจมตีได้ และเอฟเฟกต์ของมันถูกยกเลิก ในช่วงเอนด์เฟส ถ้าการ์ดใบนี้อยู่ในสุสานเพราะถูกทำลายบนฟิลด์โดยการต่อสู้หรือเอฟเฟกต์การ์ดและถูกส่งไปที่นั่นเทิร์นนี้: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์ 'Rokket' 1 ตัวจากเด็คของคุณ ยกเว้น 'Anesthrokket Dragon' คุณสามารถใช้แต่ละเอฟเฟกต์ของ 'Anesthrokket Dragon' ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e3:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
			e3:SetCode(EFFECT_DISABLE_TRAPMONSTER)
			e3:SetReset(RESETS_STANDARD_PHASE_END)
			tc:RegisterEffect(e3)
		end
		local e4=e1:Clone()
		e4:SetCode(EFFECT_CANNOT_ATTACK)
		tc:RegisterEffect(e4)
	end
end
function s.regop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	if c:IsReason(REASON_BATTLE|REASON_EFFECT) and c:IsReason(REASON_DESTROY) and c:IsPreviousLocation(LOCATION_ONFIELD) then
		local e1=Effect.CreateEffect(c)
```

---

## [30] Anteatereatingant (ID: 13250922)
**Lua File:** `script\official\c13250922.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องถูกอัญเชิญแบบพิเศษ (จากมือของคุณ) โดยการส่งเวทมนตร์/กับดัก 2 ใบที่คุณควบคุมไปยังสุสาน เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายเวทมนตร์/กับดัก 1 ใบที่คู่ต่อสู้ของคุณควบคุม; ทำลายมัน การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นที่เปิดใช้งานเอฟเฟกต์นี้

* ข้อความข้างต้นไม่เป็นทางการและอธิบายถึงฟังก์ชันการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [31] Aquamirror Illusion (ID: 64437633)
**Lua File:** `script\official\c64437633.lua`

**Description:**
> อัญเชิญแบบพิเศษมอนสเตอร์พิธีกรรม "Gishki" 1 ตัวจากมือของคุณ มันไม่สามารถโจมตีได้ และจะถูกส่งกลับไปที่มือในช่วง End Phase

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot attack
		local e1=Effect.CreateEffect(c)
		e1:SetDescription(aux.Stringid(id,1))
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		sc:RegisterEffect(e1,true)
		--Return it to hand during end phase
		local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
		e2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
```

---

## [32] Arc Rebellion Xyz Dragon (ID: 64276752)
**Lua File:** `script\official\c64276752.lua`

**Description:**
> มอนสเตอร์เลเวล 5 จำนวน 3 ตัว การ์ดที่อัญเชิญ Xyz นี้ไม่สามารถถูกทำลายด้วยเอฟเฟกต์การ์ดได้ คุณสามารถถอด Material 1 ตัวจากการ์ดใบนี้; การ์ดใบนี้ได้รับ ATK เท่ากับ ATK เดิมรวมของมอนสเตอร์อื่นทั้งหมดบนฟิลด์ จากนั้น หากการ์ดใบนี้มีมอนสเตอร์ Xyz มืดเป็น Material ให้ยกเลิกเอฟเฟกต์ของมอนสเตอร์หงายหน้าอื่นทั้งหมดบนฟิลด์ หลังจากที่แก้ไขเอฟเฟกต์นี้แล้ว คุณไม่สามารถประกาศโจมตีด้วยมอนสเตอร์อื่นได้ในเทิร์นที่เหลือนี้ คุณสามารถใช้เอฟเฟกต์นี้ของ "Arc Rebellion Xyz Dragon" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.atkop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local e0=Effect.CreateEffect(e:GetHandler())
	e0:SetType(EFFECT_TYPE_FIELD)
	e0:SetCode(EFFECT_CANNOT_ATTACK)
	e0:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e0:SetTargetRange(LOCATION_MZONE,0)
	e0:SetTarget(s.ftarget)
	e0:SetLabel(c:GetFieldID())
	e0:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e0,tp)
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,1),nil)
```

---

## [33] Archfiend Commander (ID: 68371799)
**Lua File:** `script\official\c68371799.lua`

**Description:**
> หากคุณควบคุมการ์ด 'Archfiend' อยู่ คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้ (จากมือของคุณ) ได้ แต่การ์ดใบนี้ไม่สามารถโจมตีได้ในเทิร์นนี้ คุณสามารถอัญเชิญแบบพิเศษ 'Archfiend Commander' ด้วยวิธีนี้เทิร์นละครั้งเท่านั้น เมื่ออัญเชิญแบบพิเศษด้วยวิธีนี้: เลือกเป้าหมายการ์ด 'Archfiend' 1 ใบที่คุณควบคุม; ทำลายเป้าหมายนั้น เมื่อการ์ดใบนี้ถูกอัญเชิญแบบสังเวย: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 'Archfiend' 1 ตัว ที่มีเลเวล 6 ในสุสานของคุณ; อัญเชิญแบบพิเศษเป้าหมายนั้นในตำแหน่งป้องกันหงายหน้า

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESET_EVENT|(RESETS_STANDARD&~RESET_TOFIELD)|RESET_PHASE|PHASE_END)
	c:RegisterEffect(e1)
```

---

## [34] Archfiend's Advent (ID: 53008933)
**Lua File:** `script\official\c53008933.lua`

**Description:**
> ในขณะที่คุณควบคุม 'Shining Sarcophagus' คุณสามารถอัญเชิญแบบปกติการ์ดใบนี้โดยไม่ต้องสังเวย มอนสเตอร์อื่นที่คุณควบคุมจะได้รับ ATK 500 เพิ่มขึ้น เฉพาะในเทิร์นของคุณเท่านั้น หากการ์ดใบนี้ถูกอัญเชิญแบบปกติหรือแบบพิเศษ: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; นำการควบคุมของมันมาไว้ในมือของคุณจนจบเอนด์เฟส แต่ มอนสเตอร์นั้นไม่สามารถโจมตีได้เว้นแต่คุณจะควบคุม 'Shining Sarcophagus' ในตอนที่เอฟเฟกต์นี้ถูกเปิดใช้งาน คุณสามารถใช้เอฟเฟกต์นี้ของ 'Archfiend's Advent' ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD|RESET_CONTROL)
		tc:RegisterEffect(e1)
```

---

## [35] Armor Exe (ID: 7180418)
**Lua File:** `script\official\c7180418.lua`

**Description:**
> การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นเดียวกับที่อัญเชิญแบบปกติ, อัญเชิญแบบพลิก หรืออัญเชิญแบบพิเศษ ในแต่ละ Standby Phase ของคุณและคู่ต่อสู้, เอาตัวนับเวทมนตร์ 1 ตัวบนฟิลด์ของคุณออก หากคุณไม่ทำ, จงทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [36] Array of Revealing Light (ID: 69296555)
**Lua File:** `script\official\c69296555.lua`

**Description:**
> ประกาศประเภทมอนสเตอร์ 1 ประเภท มอนสเตอร์ประเภทที่ประกาศไม่สามารถประกาศโจมตีในเทิร์นที่มันถูกอัญเชิญแบบปกติ อัญเชิญแบบพลิก หรืออัญเชิญแบบพิเศษ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	e1:SetTarget(s.target)
	c:RegisterEffect(e1)
	--race
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_FZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.atktg)
	c:RegisterEffect(e2)
	e1:SetLabelObject(e2)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return true end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_RACE)
```

---

## [37] Attraffic Control (ID: 46083380)
**Lua File:** `script\official\c46083380.lua`

**Description:**
> หากคู่ต่อสู้ของคุณควบคุมมอนสเตอร์ 3 ตัวหรือมากกว่า พวกเขาไม่สามารถประกาศการโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetProperty(EFFECT_FLAG_PLAYER_TARGET)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(0,1)
	e2:SetCondition(s.atcon)
	c:RegisterEffect(e2)
end
function s.atcon(e)
	return Duel.GetFieldGroupCount(e:GetHandlerPlayer(),0,LOCATION_MZONE)>=3
```

---

## [38] Beelzeus of the Diabolic Dragons (ID: 8763963)
**Lua File:** `script\official\c8763963.lua`

**Description:**
> จูนเนอร์ธาตุความมืด 1 ตัว + มอนสเตอร์ที่ไม่ใช่จูนเนอร์ 2+ ตัว
ไม่สามารถถูกทำลายด้วยการต่อสู้หรือเอฟเฟกต์การ์ด มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; ความเสียหายจากการต่อสู้ที่ฝ่ายตรงข้ามได้รับจากการโจมตีที่เกี่ยวข้องกับการ์ดใบนี้ในเทิร์นนี้จะลดลงครึ่งหนึ่ง และเปลี่ยน ATK ของมอนสเตอร์นั้นเป็น 0 และหากคุณทำเช่นนั้น คุณจะได้รับ LP เท่ากับ ATK เดิมของมัน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetRange(LOCATION_MZONE)
	e3:SetCode(EFFECT_CANNOT_ATTACK)
	e3:SetTargetRange(LOCATION_MZONE,0)
	e3:SetTarget(s.antarget)
	c:RegisterEffect(e3)
```
Associated helper functions:
```lua
function s.antarget(e,c)
	return c~=e:GetHandler()
end
```

---

## [39] Beetrooper Formation (ID: 64213017)
**Lua File:** `script\official\c64213017.lua`

**Description:**
> คุณสามารถเลือกเป้าหมายมอนสเตอร์ "Beetrooper" 1 ตัวในสุสานของคุณ; อัญเชิญแบบพิเศษมัน แต่ไม่สามารถโจมตีได้ในเทิร์นนี้ และคุณเสีย LP เท่ากับ ATK เดิมของมัน หากมอนสเตอร์แมลงที่หงายหน้าอยู่บนฟิลด์ของคุณถูกทำลายด้วยการต่อสู้หรือเอฟเฟกต์การ์ด: คุณสามารถอัญเชิญแบบพิเศษ "Beetrooper Token" 1 ตัว (แมลง/โลก/เลเวล 3/ATK 1000/DEF 1000) คุณสามารถใช้เอฟเฟกต์แต่ละอย่างของ "Beetrooper Formation" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```

---

## [40] Big-Tusked Mammoth (ID: 59380081)
**Lua File:** `script\official\c59380081.lua`

**Description:**
> มอนสเตอร์ที่ฝ่ายตรงข้ามควบคุมไม่สามารถโจมตีในเทิร์นที่พวกมันถูกอัญเชิญ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetRange(LOCATION_MZONE)
	e1:SetTargetRange(0,LOCATION_MZONE)
	e1:SetTarget(s.target)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.target(e,c)
	return c:IsStatus(STATUS_SUMMON_TURN+STATUS_FLIP_SUMMON_TURN+STATUS_SPSUMMON_TURN)
end
```

---

## [41] Black Luster Soldier - Envoy of the Beginning (ID: 72989439)
**Lua File:** `script\official\c72989439.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้
ต้องอัญเชิญแบบพิเศษ (จากมือ) ก่อนโดยการนำมอนสเตอร์แสง 1 ตัวและมอนสเตอร์มืด 1 ตัวจากสุสานของคุณออกนอกเกม
เทิร์นละครั้ง คุณสามารถเปิดใช้งานเอฟเฟกต์ 1 อย่างจากต่อไปนี้
● เลือกเป้าหมายมอนสเตอร์ 1 ตัวบนฟิลด์; นำออกนอกเกม การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นที่เปิดใช้งานเอฟเฟกต์นี้
● หากการ์ดที่กำลังโจมตีนี้ทำลายมอนสเตอร์ของฝ่ายตรงข้ามด้วยการต่อสู้: มันสามารถโจมตีครั้งที่สองต่อเนื่องกันได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	c:RegisterEffect(e1,true)
	c:RegisterFlagEffect(id,RESETS_STANDARD_PHASE_END,0,1)
end
function s.rmtg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	if chkc then return chkc:IsLocation(LOCATION_MZONE) and chkc:IsAbleToRemove() end
```

---

## [42] Blackwing - Sirocco the Dawn (ID: 75498415)
**Lua File:** `script\official\c75498415.lua`

**Description:**
> หากฝ่ายตรงข้ามควบคุมมอนสเตอร์อยู่และคุณไม่ควบคุมมอนสเตอร์ คุณสามารถอัญเชิญแบบปกติ/เซ็ตการ์ดใบนี้โดยไม่ต้องสังเวย เทิร์นละครั้ง ในช่วง Main Phase 1 ของคุณ: คุณสามารถเลือกเป้าหมายมอนสเตอร์ "Blackwing" 1 ตัวที่คุณควบคุม; จนกระทั่งสิ้นสุดเทิร์นนี้ มอนสเตอร์นั้นจะได้รับ ATK เท่ากับ ATK รวมของมอนสเตอร์ "Blackwing" ทั้งหมดที่อยู่บนฟิลด์ในปัจจุบัน ยกเว้นตัวมันเอง มอนสเตอร์อื่นนอกจากมอนสเตอร์ที่ถูกเลือกเป้าหมายไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local g=Duel.SelectTarget(tp,s.filter,tp,LOCATION_MZONE,0,1,1,nil)
	local e2=Effect.CreateEffect(e:GetHandler())
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetProperty(EFFECT_FLAG_OATH)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.ftarget)
	e2:SetLabel(g:GetFirst():GetFieldID())
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
```

---

## [43] Blaze Accelerator (ID: 69537999)
**Lua File:** `script\official\c69537999.lua`

**Description:**
> คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; ส่งมอนสเตอร์ประเภทไฟที่มี ATK 500 หรือน้อยกว่า 1 ตัวจากมือของคุณลงสุสาน และถ้าทำเช่นนั้น ทำลายเป้าหมายนั้น มอนสเตอร์ของคุณไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if chk==0 then return Duel.GetActivityCount(tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_OATH)
	e1:SetTargetRange(1,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,1),nil)
end
function s.disfilter(c)
```

---

## [44] Brave-Eyes Pendulum Dragon (ID: 88305705)
**Lua File:** `script\official\c88305705.lua`

**Description:**
> "Pendulum Dragon" 1 ตัว + มอนสเตอร์ประเภทนักรบ 1 ตัว
เมื่อการ์ดใบนี้ถูกอัญเชิญแบบฟิวชัน: คุณสามารถเปลี่ยน ATK ของมอนสเตอร์ที่หงายหน้าทั้งหมดที่ฝ่ายตรงข้ามควบคุมเป็น 0 และสำหรับที่เหลือของเทิร์นนี้ มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ ยกเลิกเอฟเฟกต์ที่ถูกเปิดใช้งานของมอนสเตอร์ที่มี ATK 0 ในตอนท้ายของแดเมจสเต็ป เมื่อการ์ดใบนี้โจมตีมอนสเตอร์ของฝ่ายตรงข้าม แต่มอนสเตอร์ของฝ่ายตรงข้ามไม่ได้ถูกทำลายจากการต่อสู้: คุณสามารถนำมอนสเตอร์ของฝ่ายตรงข้ามนั้นออกนอกเกม

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetValue(0)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1)
	end
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.ftarget)
	e2:SetLabel(c:GetFieldID())
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.discon(e,tp,eg,ep,ev,re,r,rp)
```

---

## [45] Broomy (ID: 57985393)
**Lua File:** `script\official\c57985393.lua`

**Description:**
> ไม่สามารถใช้เป็นวัตถุดิบซิงโครได้ ยกเว้นสำหรับการอัญเชิญซิงโครของมอนสเตอร์เลเวล 8 หรือต่ำกว่า คุณสามารถเปิดเผยการ์ดใบนี้และมอนสเตอร์ 1 ตัวในมือของคุณ; อัญเชิญแบบพิเศษ 1 ใน 2 ใบนั้น และหากทำเช่นนั้น ให้นำอีกใบออกนอกเกม และสำหรับเทิร์นที่เหลือ คุณไม่สามารถอัญเชิญแบบพิเศษจากเอ็กซ์ตร้าเด็คได้ ยกเว้นมอนสเตอร์ซิงโคร และสามารถประกาศโจมตีด้วยมอนสเตอร์ซิงโครเท่านั้น คุณสามารถใช้เอฟเฟกต์นี้ของ "Broomy" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot declare an attack, except with Synchro Monsters
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(function(e,c) return not c:IsType(TYPE_SYNCHRO) end)
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
```

---

## [46] Brotherhood of the Fire Fist - Spirit (ID: 1662004)
**Lua File:** `script\official\c1662004.lua`

**Description:**
> ไม่สามารถใช้เป็นวัตถุดิบซิงโครได้ ยกเว้นสำหรับการอัญเชิญแบบซิงโครของมอนสเตอร์ประเภทสัตว์ร้ายสงคราม เมื่อการ์ดใบนี้ถูกอัญเชิญแบบปกติ: คุณสามารถเลือกเป้ามอนสเตอร์ไฟ เลเวล 3 ที่มี DEF 200 หรือต่ำกว่าในสุสานของคุณ 1 ตัว; อัญเชิญเป้านั้นแบบพิเศษในโหมดป้องกัน และหากทำสำเร็จ มอนสเตอร์ที่คุณควบคุมไม่สามารถโจมตีได้ในเทิร์นที่เหลือนี้ ยกเว้นมอนสเตอร์ประเภทสัตว์ร้ายสงคราม คุณสามารถใช้เอฟเฟกต์นี้ของ "Brotherhood of the Fire Fist - Spirit" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local c=e:GetHandler()
		--Non-Beast-Warrior monsters you control cannot attack
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetTarget(function(e,c) return not c:IsRace(RACE_BEASTWARRIOR) end)
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
		aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,1))
	end
```

---

## [47] Bujingi Swallow (ID: 86868952)
**Lua File:** `script\official\c86868952.lua`

**Description:**
> ในช่วงเมนเฟส 1 ของคุณ: คุณสามารถส่งการ์ดใบนี้จากมือของคุณไปที่สุสาน จากนั้นเลือกเป้าหมายมอนสเตอร์ "Bujin" ที่คุณควบคุม 1 ตัว; มันสามารถโจมตีครั้งที่สองในแต่ละเฟสแบทเทิลในเทิร์นนี้ มอนสเตอร์อื่นไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(g:GetFirst():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc:IsRelateToEffect(e) then
		--Can make a second attack this turn
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3201)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_EXTRA_ATTACK)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		e1:SetValue(1)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [48] Bunch of Beast Bodies (ID: 19974890)
**Lua File:** `script\official\c19974890.lua`

**Description:**
> เปลี่ยนมอนสเตอร์ทั้งหมดที่ไม่ได้ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษในเทิร์นนี้ให้อยู่ในตำแหน่งป้องกัน สำหรับที่เหลือของเทิร์นนี้ ผู้เล่นที่ควบคุมมอนสเตอร์ในตำแหน่งป้องกันไม่สามารถประกาศโจมตีด้วยมอนสเตอร์ที่ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษในเทิร์นนี้ได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(c,nil,tp,1,1,aux.Stringid(id,1),nil)
end
function s.atktg(e,c)
	local tp=c:GetControler()
```

---

## [49] Burst Stream of Destruction (ID: 17655904)
**Lua File:** `script\official\c17655904.lua`

**Description:**
> หากคุณควบคุม "Blue-Eyes White Dragon": ทำลายมอนสเตอร์ทั้งหมดที่คู่ต่อสู้ของคุณควบคุม "Blue-Eyes White Dragon" ที่คุณควบคุมไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานการ์ดนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if chk==0 then return Duel.GetCustomActivityCount(id,tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTarget(aux.TargetBoolFunction(Card.IsCode,CARD_BLUEEYES_W_DRAGON))
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
```

---

## [50] Cattle Call (ID: 50619462)
**Lua File:** `script\official\c50619462.lua`

**Description:**
> ส่งมอนสเตอร์หงายหน้า 1 ตัวที่คุณควบคุมลงสุสาน ซึ่งประเภทดั้งเดิมเป็นสัตว์ร้าย, สัตว์ร้ายนักรบ, หรือสัตว์ปีก; อัญเชิญแบบพิเศษมอนสเตอร์ 1 ตัวจากเอ็กโตราเด็คของคุณที่มีประเภทดั้งเดิมเดียวกัน แต่มันไม่สามารถโจมตีได้ เอฟเฟกต์ของมันถูกยกเลิก และมันถูกทำลายในช่วงเอนด์เฟส คุณสามารถเปิดใช้งาน "Cattle Call" ได้เทิร์นละ 1 ใบเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot attack
		local e4=Effect.CreateEffect(c)
		e4:SetDescription(3206)
		e4:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e4:SetType(EFFECT_TYPE_SINGLE)
		e4:SetCode(EFFECT_CANNOT_ATTACK)
		e4:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
		e4:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e4,true)
	end
	Duel.SpecialSummonComplete()
end
function s.descon(e,tp,eg,ep,ev,re,r,rp)
	local tc=e:GetLabelObject()
```

---

## [51] Chain Material (ID: 39980304)
**Lua File:** `script\official\c39980304.lua`

**Description:**
> เมื่อใดก็ตามที่คุณฟิวชันอัญเชิญมอนสเตอร์ในเทิร์นนี้ คุณสามารถนำการ์ดมอนสเตอร์ที่เป็นวัตถุดิบฟิวชันที่ระบุบนการ์ดมอนสเตอร์ฟิวชันจากฟิลด์ของคุณ เด็ค มือ หรือสุสาน ออกจากเกม และใช้มันเป็นวัตถุดิบฟิวชันได้ คุณไม่สามารถโจมตีได้ในเทิร์นที่เปิดใช้การ์ดนี้ หากคุณใช้เอฟเฟกต์นี้สำหรับการฟิวชันอัญเชิญ มอนสเตอร์ฟิวชันที่ถูกอัญเชิญจะถูกทำลายในเอนด์เฟส

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if chk==0 then return Duel.GetActivityCount(tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return e:IsHasType(EFFECT_TYPE_ACTIVATE) end
end
```

---

## [52] Chaos Dragon Levianeer (ID: 55878038)
**Lua File:** `script\official\c55878038.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือคุณ) ก่อนโดยการนำมอนสเตอร์ LIGHT และ/หรือ DARK 3 ตัวจากสุสานของคุณออกนอกเกม เมื่ออัญเชิญด้วยวิธีนี้: คุณสามารถเปิดใช้เอฟเฟกต์นี้; ใช้เอฟเฟกต์ต่อไปนี้ตามแอตทริบิวต์ของมอนสเตอร์ที่ถูกนำออกนอกเกมสำหรับการอัญเชิญแบบพิเศษ นอกจากนี้ การ์ดใบนี้ไม่สามารถโจมตีได้ในเทิร์นที่เหลือ
● LIGHT เท่านั้น: อัญเชิญแบบพิเศษมอนสเตอร์ 1 ตัวจากสุสานของคุณในท่าการป้องกัน
● DARK เท่านั้น: สับการ์ด 1 ใบแบบสุ่มจากมือคู่ต่อสู้ของคุณกลับเข้าเด็ค
● ทั้ง LIGHT และ DARK: ทำลายการ์ดบนฟิลด์ได้สูงสุด 2 ใบ
คุณสามารถใช้เอฟเฟกต์ของ "Chaos Dragon Levianeer" นี้ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```
### Effect 3 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [53] Chaos Sorcerer (ID: 9596126)
**Lua File:** `script\official\c9596126.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือ) ก่อนโดยการนำมอนสเตอร์แสง 1 ตัวและมอนสเตอร์มืด 1 ตัวจากสุสานของคุณออกนอกเกม เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่หงายหน้าบนฟิลด์ 1 ตัว; นำเป้าหมายนั้นออกนอกเกม การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	c:RegisterEffect(e1)
```

---

## [54] Clear World (ID: 33900648)
**Lua File:** `script\official\c33900648.lua`

**Description:**
> เทิร์นละครั้ง ใน End Phase ของคุณ จ่าย LP 500 หรือทำลายการ์ดนี้ ผู้เล่นแต่ละคนจะได้รับเอฟเฟกต์ ขึ้นอยู่กับแอททริบิวต์ของมอนสเตอร์ที่พวกเขาควบคุม
● LIGHT: เล่นโดยเปิดมือให้เห็นตลอดเวลา
● DARK: หากคุณควบคุมมอนสเตอร์ 2 ตัวขึ้นไป คุณไม่สามารถประกาศโจมตีได้
● EARTH: ใน Standby Phase ของคุณ ทำลายมอนสเตอร์ในตำแหน่งป้องกันแบบหงายหน้า 1 ตัวที่คุณควบคุม
● WATER: ใน End Phase ของคุณ ทิ้งการ์ด 1 ใบ
● FIRE: ใน End Phase ของคุณ รับความเสียหาย 1000
● WIND: คุณต้องจ่าย LP 500 เพื่อเปิดใช้งานการ์ดเวทมนตร์

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetProperty(EFFECT_FLAG_PLAYER_TARGET)
	e3:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e3:SetRange(LOCATION_FZONE)
	e3:SetTargetRange(1,0)
	e3:SetCondition(s.darkconyou)
	c:RegisterEffect(e3)
```
Associated helper functions:
```lua
function s.darkconyou(e)
	local affected_player=e:GetHandlerPlayer()
	return s.PlayerIsAffectedByClearWorld(affected_player,ATTRIBUTE_DARK) and Duel.GetFieldGroupCount(affected_player,LOCATION_MZONE,0)>=2
end
```

---

## [55] Cloudcastle (ID: 9348522)
**Lua File:** `script\official\c9348522.lua`

**Description:**
> Tuner 1 ใบ + มอนสเตอร์ที่ไม่ใช่ Tuner 1+ ใบ
เมื่อการ์ดใบนี้ถูกอัญเชิญแบบ Synchro: คุณสามารถเลือกเป้าหมายมอนสเตอร์ระดับ 9 1 ใบในสมุสานของคุณ; อัญเชิญแบบพิเศษเป้าหมายนั้น มอนสเตอร์ระดับ 8 หรือต่ำกว่าไม่สามารถโจมตีในเทิร์นที่พวกมันถูกอัญเชิญแบบปกติหรือแบบพิเศษ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.limtg)
	c:RegisterEffect(e2)
```
Associated helper functions:
```lua
function s.limtg(e,c)
	return c:IsLevelBelow(8) and c:GetFlagEffect(id)~=0
end
```

---

## [56] Concentrating Current (ID: 20501450)
**Lua File:** `script\official\c20501450.lua`

**Description:**
> เลือกเป้าหมายมอนสเตอร์หงายหน้า 1 ตัวที่คุณควบคุม; มันได้รับ ATK เท่ากับ DEF ปัจจุบันของมัน จนจบเทิร์นนี้ มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานการ์ดนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetProperty(EFFECT_FLAG_OATH)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetTarget(s.ftarget)
		e1:SetLabel(g:GetFirst():GetFieldID())
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
		aux.RegisterClientHint(e:GetHandler(),nil,tp,0,1,aux.Stringid(id,1),nil)
	end
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.activate(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc:IsRelateToEffect(e) and tc:IsFaceup() then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_UPDATE_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		e1:SetValue(tc:GetDefense())
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [57] Consecrated Light (ID: 2980764)
**Lua File:** `script\official\c2980764.lua`

**Description:**
> ผู้เล่นทั้งสองฝ่ายไม่สามารถอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษมอนสเตอร์มืด หรือประกาศโจมตีด้วยมอนสเตอร์มืด การ์ดนี้ไม่สามารถถูกทำลายโดยการต่อสู้กับมอนสเตอร์มืดได้ และคุณไม่ได้รับความเสียหายจากการต่อสู้จากการต่อสู้ครั้งนั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e2:SetType(EFFECT_TYPE_SINGLE)
	e2:SetCode(EFFECT_INDESTRUCTABLE_BATTLE)
	e2:SetValue(s.tglimit)
	c:RegisterEffect(e2)
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e3:SetRange(LOCATION_MZONE)
	e3:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e3:SetTarget(s.tglimit)
	c:RegisterEffect(e3)
	--disable spsummon
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_FIELD)
	e4:SetRange(LOCATION_MZONE)
```

---

## [58] Coordius the Triphasic Dealmon (ID: 70219023)
**Lua File:** `script\official\c70219023.lua`

**Description:**
> มอนสเตอร์ซิงโคร 1 ตัว + มอนสเตอร์เอ็กซ์ซีส 1 ตัว + มอนสเตอร์ลิงก์ 1 ตัว
ครั้งเดียวในขณะที่การ์ดที่ถูกอัญเชิญฟิวชันนี้หงายหน้าอยู่บนฟิลด์: คุณสามารถจ่าย LP เป็นจำนวนทวีคูณของ 2000 และเลือกเอฟเฟกต์ 1 อย่างต่อทุก ๆ 2000 LP ที่จ่าย (คุณสามารถใช้แต่ละเอฟเฟกต์ได้เพียงครั้งเดียว และคุณแก้ไขตามลำดับที่ระบุ ข้ามเอฟเฟกต์ที่ไม่ได้เลือก);
● เพิ่มเวทมนตร์/กับดัก 1 ใบจากสุสานของคุณขึ้นมือ
● ทำลายการ์ด 3 ใบที่คู่ต่อสู้ควบคุม
● เทิร์นนี้ การ์ดใบนี้ได้รับ ATK เท่ากับครึ่งหนึ่งของผลต่างระหว่าง LP ของคุณและ LP ของคู่ต่อสู้ และมอนสเตอร์อื่นของคุณไม่สามารถโจมตีได้
คุณสามารถใช้เอฟเฟกต์นี้ของ "Coordius the Triphasic Dealmon" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetValue(math.floor(diff/2))
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		c:RegisterEffect(e1)
		local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_FIELD)
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		e2:SetTargetRange(LOCATION_MZONE,0)
		e2:SetTarget(s.ftarget)
		e2:SetLabel(c:GetFieldID())
		e2:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e2,tp)
		aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,3),nil)
	end
end
function s.ftarget(e,c)
```

---

## [59] Corridor of Agony (ID: 26257572)
**Lua File:** `script\official\c26257572.lua`

**Description:**
> มอนสเตอร์ที่ถูกอัญเชิญแบบพิเศษจากเด็คหลักไม่สามารถเปิดใช้งานเอฟเฟกต์ของพวกมันได้ เอฟเฟกต์ของพวกมันถูกยกเลิก และพวกมันไม่สามารถประกาศโจมตีได้ ตราบใดที่พวกมันยังคงหงายหน้าอยู่บนฟิลด์

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e2:SetTarget(s.target)
	c:RegisterEffect(e2)
	local e3=e2:Clone()
	e3:SetCode(EFFECT_DISABLE)
	c:RegisterEffect(e3)
	local e4=e2:Clone()
	e4:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	c:RegisterEffect(e4)
end
function s.target(e,c)
	return c:IsSummonLocation(LOCATION_DECK)
```

---

## [60] Crimson Knight Vampire Bram (ID: 38250531)
**Lua File:** `script\official\c38250531.lua`

**Description:**
> มอนสเตอร์จอมปิศาจเลเวล 5 จำนวน 2 ตัว
คุณสามารถถอดแมททีเรียล 1 ตัวจากการ์ดใบนี้ จากนั้นเลือกเป้าหมายมอนสเตอร์ 1 ตัวในสุสานของฝ่ายตรงข้าม; อัญเชิญแบบพิเศษเป้าหมายนั้นมายังฟิลด์ของคุณ แต่สำหรับเทิร์นที่เหลือ มอนสเตอร์นั้นเท่านั้นที่สามารถโจมตีได้ คุณสามารถใช้เอฟเฟกต์ของ "Crimson Knight Vampire Bram" ได้เพียงครั้งเดียวเท่านั้นในแต่ละเทิร์น เทิร์นละครั้ง ในสแตนด์บายเฟสของเทิร์นถัดไปหลังจากการ์ดใบนี้ที่คุณควบคุมถูกทำลายด้วยการ์ดของฝ่ายตรงข้ามและถูกส่งไปยังสุสานของคุณ: อัญเชิญแบบพิเศษการ์ดใบนี้ในท่าป้องกัน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if tc:IsRelateToEffect(e) and Duel.SpecialSummon(tc,0,tp,tp,false,false,POS_FACEUP)>0 then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetTarget(s.ftarget)
		e1:SetLabel(tc:GetFieldID())
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
	end
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.spreg(e,tp,eg,ep,ev,re,r,rp)
```

---

## [61] Crusadia Equimax (ID: 45002991)
**Lua File:** `script\official\c45002991.lua`

**Description:**
> มอนสเตอร์เอฟเฟกต์ 2 ตัวขึ้นไป รวมถึงมอนสเตอร์ลิงก์
ได้รับ ATK เท่ากับ ATK เดิมรวมของมอนสเตอร์ทั้งหมดที่การ์ดนี้ชี้ไป มอนสเตอร์ที่การ์ดนี้ชี้ไปไม่สามารถโจมตีได้ เทิร์นละครั้ง (ควิกเอฟเฟกต์): คุณสามารถสังเวยมอนสเตอร์ "Crusadia" หรือ "World Legacy" 1 ตัวที่การ์ดนี้ชี้ไป จากนั้นเลือกเป้าหมายการ์ดที่หงายหน้าของฝ่ายตรงข้าม 1 ใบ ยกเลิกเอฟเฟกต์ของการ์ดนั้นจนจบเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_UPDATE_ATTACK)
	e1:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
	e1:SetRange(LOCATION_MZONE)
	e1:SetValue(s.atkval)
	c:RegisterEffect(e1)
	--Cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.atklimit)
	c:RegisterEffect(e2)
	--Negate effect
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,0))
	e3:SetCategory(CATEGORY_DISABLE)
	e3:SetType(EFFECT_TYPE_QUICK_O)
	e3:SetCode(EVENT_FREE_CHAIN)
	e3:SetProperty(EFFECT_FLAG_CARD_TARGET)
	e3:SetCountLimit(1)
	e3:SetRange(LOCATION_MZONE)
	e3:SetCost(s.discost)
	e3:SetTarget(s.distg)
	e3:SetOperation(s.disop)
	c:RegisterEffect(e3)
end
s.listed_series={SET_WORLD_LEGACY,SET_CRUSADIA}
function s.lcheck(g,lc,sumtype,tp)
	return g:IsExists(Card.IsType,1,nil,TYPE_LINK,lc,sumtype,tp)
end
function s.atkval(e,c)
	local g=e:GetHandler():GetLinkedGroup():Filter(Card.IsFaceup,nil)
	return g:GetSum(Card.GetBaseAttack)
end
function s.atklimit(e,c)
	return e:GetHandler():GetLinkedGroup():IsContains(c)
end
function s.disfilter(c,e)
	return c:IsNegatable() and c:IsCanBeEffectTarget(e)
end
function s.cfilter(c,tg,lg)
	return c:IsSetCard({SET_WORLD_LEGACY,SET_CRUSADIA}) and lg:IsContains(c)
end
function s.discost(e,tp,eg,ep,ev,re,r,rp,chk)
	local tg=Duel.GetMatchingGroup(s.disfilter,tp,0,LOCATION_ONFIELD,nil,e)
	local lg=e:GetHandler():GetLinkedGroup()
	if chk==0 then return Duel.CheckReleaseGroupCost(tp,s.cfilter,1,false,aux.ReleaseCheckTarget,nil,tg,lg) end
	local g=Duel.SelectReleaseGroupCost(tp,s.cfilter,1,1,false,aux.ReleaseCheckTarget,nil,tg,lg)
	Duel.Release(g,REASON_COST)
end
function s.distg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	if chkc then return chkc:IsControler(1-tp) and chkc:IsOnField() and chkc:IsNegatable() end
	if chk==0 then return Duel.IsExistingTarget(Card.IsNegatable,tp,0,LOCATION_ONFIELD,1,nil) end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_NEGATE)
	local g=Duel.SelectTarget(tp,Card.IsNegatable,tp,0,LOCATION_ONFIELD,1,1,nil)
	Duel.SetOperationInfo(0,CATEGORY_DISABLE,g,1,0,0)
end
function s.disop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local tc=Duel.GetFirstTarget()
	if ((tc:IsFaceup() and not tc:IsDisabled()) or tc:IsType(TYPE_TRAPMONSTER)) and tc:IsRelateToEffect(e) then
		Duel.NegateRelatedChain(tc,RESET_TURN_SET)
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetCode(EFFECT_DISABLE)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.atkval(e,c)
	local g=e:GetHandler():GetLinkedGroup():Filter(Card.IsFaceup,nil)
	return g:GetSum(Card.GetBaseAttack)
end
```

---

## [62] Crusadia Magius (ID: 72228247)
**Lua File:** `script\official\c72228247.lua`

**Description:**
> มอนสเตอร์ "Crusadia" 1 ตัว ยกเว้น "Crusadia Magius"
การ์ดใบนี้ได้รับ ATK เท่ากับ ATK เดิมของมอนสเตอร์ที่การ์ดใบนี้ชี้ไป มอนสเตอร์ที่การ์ดใบนี้ชี้ไปไม่สามารถโจมตีได้ หากมอนสเตอร์เอฟเฟกต์ถูกอัญเชิญแบบพิเศษไปยังโซนที่การ์ดใบนี้ชี้ไป (ยกเว้นในช่วงสเตจความเสียหาย): คุณสามารถนำมอนสเตอร์ "Crusadia" 1 ตัวจากเด็คขึ้นมือ คุณสามารถใช้เอฟเฟกต์ของ "Crusadia Magius" ได้เพียงเทิร์นละครั้งเท่านั้น.

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetRange(LOCATION_MZONE)
	e1:SetValue(s.atkval)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.atklimit)
	c:RegisterEffect(e2)
	--Search
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,0))
	e3:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH)
```

---

## [63] Crusadia Maximus (ID: 81524756)
**Lua File:** `script\official\c81524756.lua`

**Description:**
> คุณสามารถอัญเชิญแบบพิเศษการ์ดนี้ (จากมือของคุณ) ในตำแหน่งป้องกันไปยังโซนที่มอนสเตอร์ลิงก์ที่คุณควบคุมชี้ไป คุณสามารถอัญเชิญแบบพิเศษ "Crusadia Maximus" ด้วยวิธีนี้เทิร์นละครั้งเท่านั้น คุณสามารถเลือกเป้าหมายมอนสเตอร์ลิงก์ "Crusadia" 1 ตัวที่คุณควบคุม; เทิร์นนี้ หากมันต่อสู้กับมอนสเตอร์ของคู่ต่อสู้ ความเสียหายต่อสู้ใดๆ ที่มันสร้างให้คู่ต่อสู้จะเพิ่มเป็นสองเท่า และมอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ คุณสามารถใช้เอฟเฟกต์นี้ของ "Crusadia Maximus" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
		--Also other monsters you control cannot attack
		local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_FIELD)
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		e2:SetTargetRange(LOCATION_MZONE,0)
		e2:SetTarget(function(e,c) return fid~=c:GetFieldID() end)
		e2:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e2,tp)
	end
```

---

## [64] Crusadia Regulex (ID: 9617996)
**Lua File:** `script\official\c9617996.lua`

**Description:**
> มอนสเตอร์เอฟเฟกต์ 2 ตัว รวมถึงมอนสเตอร์ "Crusadia" 1 ตัว
ได้รับ ATK เท่ากับค่า ATK เดิมรวมกันของมอนสเตอร์ทั้งหมดที่การ์ดใบนี้ชี้ไป มอนสเตอร์ที่การ์ดใบนี้ชี้ไปไม่สามารถโจมตีได้ หากมอนสเตอร์เอฟเฟกต์ถูกอัญเชิญแบบพิเศษมายังโซนที่การ์ดใบนี้ชี้ไป (ยกเว้นในช่วง Damage Step): คุณสามารถนำเวทมนตร์/กับดัก "Crusadia" 1 ใบจากเด็คขึ้นมือ คุณสามารถใช้เอฟเฟกต์นี้ของ "Crusadia Regulex" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetRange(LOCATION_MZONE)
	e1:SetValue(s.atkval)
	c:RegisterEffect(e1)
	--Monsters this card points to cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.atklimit)
	c:RegisterEffect(e2)
	--Search 1 "Crusadia" Spell/Trap
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,0))
```

---

## [65] Crusadia Revival (ID: 69039982)
**Lua File:** `script\official\c69039982.lua`

**Description:**
> มอนสเตอร์ลิงก์ "Crusadia" ทั้งหมดบนฟิลด์ได้รับ ATK 500 ตัว หนึ่งครั้งต่อเทิร์น: คุณสามารถเลือกเป้าหมายมอนสเตอร์ลิงก์ "Crusadia" 1 ตัวที่คุณควบคุม; เทิร์นนี้ (แม้ว่าการ์ดใบนี้จะออกจากฟิลด์) มันสามารถโจมตีมอนสเตอร์ทั้งหมดที่ฝ่ายตรงข้ามควบคุมได้ 1 ครั้งต่อตัว และมอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetValue(1)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
	end
	local e2=Effect.CreateEffect(e:GetHandler())
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.ftarget)
	e2:SetLabel(tc:GetFieldID())
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
```

---

## [66] Cubic Causality (ID: 38606913)
**Lua File:** `script\official\c38606913.lua`

**Description:**
> กระจาย Cubic Counter จำนวนหนึ่งไปบนมอนสเตอร์ที่หงายหน้าอยู่ที่ฝ่ายตรงข้ามควบคุม สูงสุดเท่ากับจำนวนมอนสเตอร์ "Cubic" ที่คุณควบคุม (มอนสเตอร์ที่มี Cubic Counter ไม่สามารถโจมตี และยกเลิกเอฟเฟกต์ของพวกมันด้วย) คุณสามารถนำการ์ดใบนี้จากสุสานของคุณออกนอกเกม จากนั้นเลือกเป้าหมายมอนสเตอร์ "Cubic" 1 ตัวที่คุณควบคุม; เทิร์นนี้ ทุกครั้งที่มันทำลายมอนสเตอร์ในการต่อสู้ที่มี Cubic Counter ให้สร้างความเสียหายให้กับฝ่ายตรงข้ามเท่ากับ ATK ดั้งเดิมของมอนสเตอร์ที่ถูกทำลายนั้น คุณสามารถใช้เอฟเฟกต์แต่ละอย่างของ "Cubic Causality" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
			e1:SetType(EFFECT_TYPE_SINGLE)
			e1:SetCode(EFFECT_CANNOT_ATTACK)
			e1:SetCondition(s.condition)
			e1:SetReset(RESET_EVENT|RESETS_STANDARD)
			ac:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.condition(e)
	return e:GetHandler():GetCounter(0x1038)>0
end
```

---

## [67] Cursed Eldland (ID: 31434645)
**Lua File:** `script\official\c31434645.lua`

**Description:**
> คุณไม่สามารถประกาศโจมตีได้ ยกเว้นด้วยมอนสเตอร์ประเภทซอมบี้ คุณสามารถใช้แต่ละเอฟเฟกต์ต่อไปนี้ของ "Cursed Eldland" ได้เทิร์นละครั้งเท่านั้น
● คุณสามารถจ่าย LP 800 คะแนน; นำมอนสเตอร์ "Eldlich" 1 ตัว หรือเวทมนตร์/กับดัก "Golden Land" 1 ใบจากเด็คของคุณขึ้นมือ 
● หากการ์ดใบนี้ถูกส่งจากโซนเวทมนตร์และกับดักลงสุสาน: คุณสามารถส่งมอนสเตอร์ "Eldlich" 1 ตัว หรือเวทมนตร์/กับดัก "Golden Land" 1 ใบจากเด็คของคุณลงสุสาน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.atktg)
	c:RegisterEffect(e2)
	--search
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,0))
	e3:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH)
	e3:SetType(EFFECT_TYPE_IGNITION)
```

---

## [68] Cyberload Fusion (ID: 55704856)
**Lua File:** `script\official\c55704856.lua`

**Description:**
> Fusion Summon มอนสเตอร์ Fusion 1 ตัวจากเอ็กซ์ตร้าเด็คของคุณ โดยการสับ Fusion Materials ที่ระบุไว้บนมันเข้าไปในเด็ค รวมถึงมอนสเตอร์ "Cyber Dragon" ที่มันระบุเป็นวัตถุดิบ จากมอนสเตอร์บนฟิลด์ของคุณ และ/หรือการ์ดที่ถูกนำออกนอกเกมที่หงายหน้าอยู่ของคุณ แต่มอนสเตอร์ที่คุณควบคุมไม่สามารถโจมตีไปจนสิ้นสุดเทิร์นนี้ ยกเว้นมอนสเตอร์ที่ถูก Fusion Summon นั้น คุณสามารถเปิดใช้งาน "Cyberload Fusion" ได้เทิร์นละ 1 ใบเท่านั้น\r\n\r\n* ข้อความข้างต้นไม่เป็นทางการและอธิบายฟังก์ชันของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.stage2(e,tc,tp,sg,chk)
	if chk==1 then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetLabel(tc:GetFieldID())
		e1:SetTarget(s.atktg)
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
	end
end
function s.atktg(e,c)
	return e:GetLabel()~=c:GetFieldID()
```

---

## [69] Cyberse Clock Dragon (ID: 42717221)
**Lua File:** `script\official\c42717221.lua`

**Description:**
> "Clock Wyvern" + มอนสเตอร์ลิงก์ 1 ตัวขึ้นไป
เมื่อการ์ดใบนี้ถูกอัญเชิญแบบฟิวชัน คุณสามารถ: ส่งการ์ดจากด้านบนของเด็คของคุณลงสุสานเท่ากับค่า Link Rating รวมของวัตถุดิบที่ใช้ในการอัญเชิญแบบฟิวชันของการ์ดนี้  และจนกว่าจะสิ้นสุดเทิร์นถัดไป มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ และการ์ดใบนี้ได้รับ ATK เพิ่ม 1000 สำหรับการ์ดแต่ละใบที่ถูกส่งไปยังสุสานด้วยเอฟเฟกต์นี้ ในขณะที่คุณควบคุมมอนสเตอร์ลิงก์ มอนสเตอร์ของฝ่ายตรงข้ามไม่สามารถเลือกมอนสเตอร์อื่นที่คุณควบคุมเป็นเป้าหมายในการโจมตี และฝ่ายตรงข้ามไม่สามารถเลือกมอนสเตอร์อื่นที่คุณควบคุมเป็นเป้าหมายด้วยเอฟเฟกต์การ์ด ถ้าการ์ดที่ถูกอัญเชิญแบบฟิวชันนี้ที่คุณควบคุมถูกส่งไปยังสุสานของคุณด้วยเอฟเฟกต์การ์ดของฝ่ายตรงข้าม: คุณสามารถนำเวทมนตร์ 1 ใบจากเด็คขึ้นมือ

* ข้อความข้างต้นไม่เป็นทางการและอธิบายการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetValue(atk)
		e1:SetReset(RESETS_STANDARD_DISABLE_PHASE_END,2)
		c:RegisterEffect(e1)
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.ftarget)
	e2:SetLabel(c:GetFieldID())
	e2:SetReset(RESET_PHASE|PHASE_END,2)
	Duel.RegisterEffect(e2,tp)
	end
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.tgcon(e)
```

---

## [70] D/D Dog (ID: 32349062)
**Lua File:** `script\official\c32349062.lua`

**Description:**
> [เอฟเฟกต์เพนดูลั่ม]
คุณสามารถเลือกเป้าหมายมอนสเตอร์ฟิวชัน ซิงโคร หรือเอ็กซีส์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; ยกเลิกเอฟเฟกต์ของการ์ดนั้นจนกระทั่งจบเทิร์นนี้ จากนั้นทำลายการ์ดใบนี้ คุณสามารถใช้เอฟเฟกต์นี้ของ "D/D Dog" ได้เทิร์นละครั้งเท่านั้น
----------------------------------------
[เอฟเฟกต์มอนสเตอร์]
เทิร์นละครั้ง ถ้าฝ่ายตรงข้ามอัญเชิญแบบพิเศษมอนสเตอร์ฟิวชัน ซิงโคร หรือเอ็กซีส์ (ยกเว้นในช่วง Damage Step): คุณสามารถเลือกเป้าหมายมอนสเตอร์เหล่านั้น 1 ตัว; ในเทิร์นนี้ มอนสเตอร์ที่หงายหน้าอยู่นั้นไม่สามารถโจมตี และเอฟเฟกต์ของมันจะถูกยกเลิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
tc:RegisterEffect(e0)
		local e1=e0:Clone()
		e1:SetCode(EFFECT_DISABLE_EFFECT)
		tc:RegisterEffect(e1)
		--Cannot attack this turn
		local e2=e0:Clone()
		e2:SetDescription(3206)
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		e2:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		tc:RegisterEffect(e2)
	end
```

---

## [71] D/D Evil (ID: 55415564)
**Lua File:** `script\official\c55415564.lua`

**Description:**
> [ Pendulum Effect ]
ครั้งเดียว, ในขณะที่การ์ดนี้อยู่ใน Pendulum Zone ของคุณ, เมื่อฝ่ายตรงข้ามอัญเชิญแบบเพนดูลั่มมอนสเตอร์ (ยกเว้นในช่วง Damage Step): คุณสามารถเปิดใช้งานเอฟเฟกต์นี้; เทิร์นนี้, มอนสเตอร์ที่ถูกอัญเชิญแบบเพนดูลั่มนั้นไม่สามารถโจมตี, และเอฟเฟกต์ของมันถูกยกเลิก
----------------------------------------
[ Monster Effect ]
ไม่สามารถโจมตีได้เว้นแต่คุณจะมีมอนสเตอร์ "D/D" ตัวอื่นควบคุม ในช่วง Main Phase ของฝ่ายตรงข้าม (Quick Effect): คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่ถูกอัญเชิญแบบเพนดูลั่มหงายหน้า 1 ตัวที่ฝ่ายตรงข้ามควบคุม; ยกเลิกเอฟเฟกต์ของมอนสเตอร์หงายหน้านั้นจนจบเทิร์นนี้ คุณสามารถใช้เอฟเฟกต์นี้ของ "D/D Evil" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_SINGLE)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetCondition(s.atkcon)
	c:RegisterEffect(e2)
```
Associated helper functions:
```lua
function s.atkcon(e)
	return not Duel.IsExistingMatchingCard(aux.FaceupFilter(Card.IsSetCard,SET_DD),e:GetHandlerPlayer(),LOCATION_MZONE,0,1,e:GetHandler())
end
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
e0:SetType(EFFECT_TYPE_SINGLE)
		e0:SetCode(EFFECT_DISABLE)
		e0:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e0)
		local e1=e0:Clone()
		e1:SetDescription(3206)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		tc:RegisterEffect(e1)
		local e2=e0:Clone()
		e2:SetCode(EFFECT_DISABLE_EFFECT)
		tc:RegisterEffect(e2)
	end
end
	--If no other "D/D" on your side
function s.atkcon(e)
```

---

## [72] Dark Summoning Beast (ID: 87917187)
**Lua File:** `script\official\c87917187.lua`

**Description:**
> คุณสามารถสังเวยการ์ดใบนี้; อัญเชิญ "Uria, Lord of Searing Flames", "Hamon, Lord of Striking Thunder" หรือ "Raviel, Lord of Phantasms" 1 ใบจากมือหรือเด็คของคุณแบบพิเศษ โดยไม่สนใจเงื่อนไขการอัญเชิญ และมอนสเตอร์ที่คุณควบคุมไม่สามารถโจมตีได้ในเทิร์นที่เหลือนี้ คุณสามารถใช้เอฟเฟกต์นี้ของ "Dark Summoning Beast" ได้เทิร์นละครั้งเท่านั้น คุณสามารถนำการ์ดใบนี้ออกจากเกมจากสุสานของคุณ; เพิ่ม "Uria, Lord of Searing Flames", "Hamon, Lord of Striking Thunder" หรือ "Raviel, Lord of Phantasms" 1 ใบจากเด็คของคุณขึ้นมือ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.spop(e,tp,eg,ep,ev,re,r,rp)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	if Duel.GetLocationCount(tp,LOCATION_MZONE)<=0 then return end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
```

---

## [73] Darkworld Shackles (ID: 83584898)
**Lua File:** `script\official\c83584898.lua`

**Description:**
> มอนสเตอร์ที่สวมใส่ไม่สามารถโจมตีได้ และ ATK และ DEF ของมันกลายเป็น 100 ในแต่ละ Standby Phase ของคุณ: ทำให้ผู้ควบคุมมอนสเตอร์ที่สวมใส่ได้รับความเสียหาย 500

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e3:SetType(EFFECT_TYPE_EQUIP)
	e3:SetCode(EFFECT_SET_DEFENSE)
	e3:SetValue(100)
	c:RegisterEffect(e3)
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_EQUIP)
	e4:SetCode(EFFECT_CANNOT_ATTACK)
	c:RegisterEffect(e4)
	--damage
	local e6=Effect.CreateEffect(c)
	e6:SetDescription(aux.Stringid(id,0))
	e6:SetCategory(CATEGORY_DAMAGE)
	e6:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_F)
	e6:SetCode(EVENT_PHASE|PHASE_STANDBY)
	e6:SetRange(LOCATION_SZONE)
```

---

## [74] Defender of the Ice Barrier (ID: 82498947)
**Lua File:** `script\official\c82498947.lua`

**Description:**
> ในขณะที่คุณควบคุมมอนสเตอร์ "Ice Barrier" ตัวอื่น มอนสเตอร์ที่ฝ่ายตรงข้ามควบคุมไม่สามารถประกาศโจมตีได้หาก ATK ของพวกมันมากกว่าหรือเท่ากับ DEF ของการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetRange(LOCATION_MZONE)
	e1:SetTargetRange(0,LOCATION_MZONE)
	e1:SetTarget(s.tg)
	e1:SetCondition(s.con)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.tg(e,c)
	return c:GetAttack()>=e:GetHandler():GetDefense()
end
```
```lua
function s.con(e)
	return Duel.IsExistingMatchingCard(aux.FaceupFilter(Card.IsSetCard,SET_ICE_BARRIER),e:GetHandler():GetControler(),LOCATION_MZONE,0,1,e:GetHandler())
end
```

---

## [75] Deskbot 009 (ID: 25494711)
**Lua File:** `script\official\c25494711.lua`

**Description:**
> เทิร์นละครั้ง ในช่วง Main Phase 1 ของคุณ: คุณสามารถให้การ์ดใบนี้ได้รับ ATK เท่ากับ ATK รวมของมอนสเตอร์ "Deskbot" ทั้งหมดที่คุณควบคุมอยู่ในปัจจุบัน ยกเว้น "Deskbot 009" จนกระทั่งสิ้นสุดเทิร์นของฝ่ายตรงข้าม มีเพียงการ์ดใบนี้เท่านั้นที่สามารถโจมตีได้ในเทิร์นที่เปิดใช้งานเอฟเฟกต์นี้ ถ้าการ์ดใบนี้ต่อสู้ การ์ดและเอฟเฟกต์ของฝ่ายตรงข้ามไม่สามารถเปิดใช้งานได้จนกว่าจะสิ้นสุด Damage Step ถ้าการ์ดใบนี้จะถูกทำลายในการต่อสู้หรือด้วยเอฟเฟกต์การ์ด คุณสามารถทำลายการ์ด "Deskbot" 1 ใบที่คุณควบคุมแทนได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(e:GetHandler():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.atkop(e,tp,eg,ep,ev,re,r,rp)
	local tc=e:GetHandler()
	if tc:IsFaceup() and tc:IsRelateToEffect(e) then
		local g=Duel.GetMatchingGroup(s.atkfilter,tp,LOCATION_MZONE,0,nil)
		local atk=g:GetSum(Card.GetAttack)
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_UPDATE_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END|RESET_OPPO_TURN)
		e1:SetValue(atk)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [76] Desperado Barrel Dragon (ID: 76728962)
**Lua File:** `script\official\c76728962.lua`

**Description:**
> หากมอนสเตอร์เครื่องจักร DARK ที่หงายหน้าซึ่งคุณควบคุมถูกทำลายด้วยการต่อสู้หรือเอฟเฟกต์การ์ด: คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้จากมือของคุณ ครั้งต่อเทิร์น ในช่วงแบตเทิลเฟส (ควิกเอฟเฟกต์) คุณสามารถ: โยนเหรียญ 3 ครั้ง และทำลายมอนสเตอร์ที่หงายหน้าบนฟิลด์สูงสุดตามจำนวนหัว จากนั้นหากผลลัพธ์เป็นหัว 3 ครั้ง จั่วการ์ด 1 ใบ การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นที่เปิดใช้งานเอฟเฟกต์นี้ หากการ์ดใบนี้ถูกส่งลงสุสาน: คุณสามารถนำมอนสเตอร์ที่มีเลเวล 7 หรือต่ำกว่าที่มีเอฟเฟกต์การโยนเหรียญ 1 ใบ จากเด็คของคุณขึ้นมือ

* ข้อความข้างต้นไม่เป็นทางการและอธิบายการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [77] Destiny HERO - Doom Lord (ID: 41613948)
**Lua File:** `script\official\c41613948.lua`

**Description:**
> เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; นำเป้าหมายนั้นออกนอกเกม คุณไม่สามารถประกาศโจมตีในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้ คุณต้องควบคุมการ์ดใบนี้ในตำแหน่งโจมตีหงายหน้าอยู่เพื่อเปิดใช้งานและแก้ไขเอฟเฟกต์นี้ มอนสเตอร์ที่ถูกนำออกนอกเกมจะกลับสู่ฟิลด์ของฝ่ายตรงข้ามใน Standby Phase ที่ 2 ของคุณหลังจากเปิดใช้งาน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if chk==0 then return Duel.GetActivityCount(tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_OATH)
	e1:SetTargetRange(1,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
```

---

## [78] Diffusion Wave-Motion (ID: 87880531)
**Lua File:** `script\official\c87880531.lua`

**Description:**
> (การ์ดใบนี้ไม่ถูกนับเป็นการ์ด "Fusion")
หากฝ่ายตรงข้ามควบคุมมอนสเตอร์: จ่าย LP 1000 แต้ม จากนั้นเลือกเป้าหมายมอนสเตอร์เวทมนตร์เลเวล 7 ขึ้นไปที่คุณควบคุม 1 ตัว; เทิร์นนี้ มันต้องโจมตีมอนสเตอร์ทั้งหมดที่ฝ่ายตรงข้ามควบคุม อย่างละครั้ง และมอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ เอฟเฟกต์ของมอนสเตอร์ที่ถูกทำลายด้วยการโจมตีเหล่านี้ไม่สามารถเปิดใช้งานและถูกยกเลิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local tc=Duel.GetFirstTarget()
	if tc:IsFaceup() and tc:IsControler(tp) and tc:IsRelateToEffect(e) then
		local ae=Effect.CreateEffect(e:GetHandler())
		ae:SetType(EFFECT_TYPE_FIELD)
		ae:SetCode(EFFECT_CANNOT_ATTACK)
		ae:SetTargetRange(LOCATION_MZONE,0)
		ae:SetTarget(s.ftarget)
		ae:SetLabel(tc:GetFieldID())
		ae:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(ae,tp)
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_MUST_ATTACK)
```

---

## [79] Dinomorphia Alert (ID: 52020510)
**Lua File:** `script\official\c52020510.lua`

**Description:**
> จ่าย LP ครึ่งหนึ่งของตัวเอง; อัญเชิญแบบพิเศษ "Dinomorphia" มอนสเตอร์จากสุสานของคุณได้มากสุด 2 ตัว ซึ่งรวมเลเวลต้องเท่ากับ 8 หรือน้อยกว่า แต่ในเทิร์นนี้ คุณไม่สามารถประกาศโจมตีด้วยมอนสเตอร์เหล่านั้นได้ อีกทั้งคุณไม่สามารถอัญเชิญแบบพิเศษในช่วงที่เหลือของเทิร์นนี้ได้ ยกเว้นมอนสเตอร์ "Dinomorphia" เมื่อฝ่ายตรงข้ามเปิดใช้งานการ์ดหรือเอฟเฟกต์ ในขณะที่ LP ของคุณคือ 2000 หรือน้อยกว่า: คุณสามารถนำการ์ดใบนี้จากสุสานของคุณออกนอกเกมได้; ในเทิร์นนี้ คุณไม่ได้รับความเสียหายจากเอฟเฟกต์จากการ์ดของฝ่ายตรงข้าม คุณสามารถเปิดใช้งาน "Dinomorphia Alert" ได้เทิร์นละ 1 ใบเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
for tc in aux.Next(g) do
			--Cannot declare attacks
			local e1=Effect.CreateEffect(c)
			e1:SetType(EFFECT_TYPE_SINGLE)
			e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
			e1:SetReset(RESETS_STANDARD_PHASE_END)
			tc:RegisterEffect(e1,true)
		end
	end
	--Cannot Special Summon, except "Dinoruffia"
	local e1=Effect.CreateEffect(c)
	e1:SetDescription(aux.Stringid(id,2))
	e1:SetType(EFFECT_TYPE_FIELD)
```

---

## [80] Dinowrestler King T Wrextle (ID: 77967790)
**Lua File:** `script\official\c77967790.lua`

**Description:**
> มอนสเตอร์ "Dinowrestler" 2 ตัวขึ้นไป
หากการ์ดใบนี้ทำการต่อสู้ ฝ่ายตรงข้ามไม่สามารถเปิดใช้งานการ์ดเวทมนตร์/กับดักได้จนกระทั่งจบ Damage Step มอนสเตอร์ของฝ่ายตรงข้ามไม่สามารถเลือกเป้าหมายมอนสเตอร์เพื่อโจมตีได้ ยกเว้นการ์ดใบนี้ เมื่อเริ่มต้น Battle Phase ของฝ่ายตรงข้าม: คุณสามารถเลือกเป้าหมายมอนสเตอร์ในตำแหน่งโจมตี 1 ตัวที่ฝ่ายตรงข้ามควบคุม; ใน Battle Phase นี้ ฝ่ายตรงข้ามไม่สามารถประกาศการโจมตีด้วยมอนสเตอร์ตัวอื่นได้จนกว่าพวกเขาจะได้ประกาศการโจมตีด้วยมอนสเตอร์ที่ถูกเลือกเป้าหมาย และทำลายมันเมื่อจบ Battle Phase หากมันไม่ได้ประกาศการโจมตี

* ข้อความข้างต้นเป็นข้อมูลไม่เป็นทางการและอธิบายฟังก์ชันการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if tc:IsRelateToEffect(e) then
		tc:RegisterFlagEffect(id,RESETS_STANDARD_PHASE_END,0,1)
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
		e1:SetTargetRange(0,LOCATION_MZONE)
		e1:SetCondition(s.atkcon2)
		e1:SetTarget(s.atktg2)
		e1:SetLabelObject(tc)
		e1:SetReset(RESET_PHASE|PHASE_BATTLE)
		Duel.RegisterEffect(e1,tp)
		local e2=Effect.CreateEffect(c)
```

---

## [81] Dystopia the Despondent (ID: 52085072)
**Lua File:** `script\official\c52085072.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือหรือสุสานของคุณ) โดยการส่งมอนสเตอร์เลเวล 1 หงายหน้าที่คุณควบคุม 4 ตัวลงสุสาน และไม่สามารถอัญเชิญแบบพิเศษด้วยวิธีอื่นได้ มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ ในช่วง Battle Step ของผู้เล่นคนใดก็ได้ ครั้งละ 1 ครั้งต่อการต่อสู้ที่เกี่ยวข้องกับการ์ดใบนี้: คุณสามารถนำมอนสเตอร์เลเวล 1 จากสุสานของคุณออกนอกเกม 1 ตัว; จนกระทั่งสิ้นสุด Damage Step การ์ดใบนี้ไม่ได้รับผลกระทบจากเอฟเฟกต์ของการ์ดอื่น อีกทั้งไม่สามารถถูกทำลายจากการต่อสู้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetRange(LOCATION_MZONE)
	e3:SetCode(EFFECT_CANNOT_ATTACK)
	e3:SetTargetRange(LOCATION_MZONE,0)
	e3:SetTarget(s.antarget)
	c:RegisterEffect(e3)
```
Associated helper functions:
```lua
function s.antarget(e,c)
	return c~=e:GetHandler()
end
```

---

## [82] Earthbound Immortal Chacu Challhua (ID: 69931927)
**Lua File:** `script\official\c69931927.lua`

**Description:**
> บนฟิลด์จะมีมอนสเตอร์ "Earthbound Immortal" ได้เพียง 1 ตัวเท่านั้น หากไม่มีเวทมนตร์ฟิลด์ที่หงายหน้าอยู่บนฟิลด์ ให้ทำลายการ์ดใบนี้ มอนสเตอร์ของฝ่ายตรงข้ามไม่สามารถเลือกเป้าการ์ดใบนี้เพื่อโจมตีได้ การ์ดใบนี้สามารถโจมตีฝ่ายตรงข้ามได้โดยตรง เทิร์นละครั้ง: คุณสามารถสร้างความเสียหายให้ฝ่ายตรงข้ามเท่ากับครึ่งหนึ่งของ DEF ของการ์ดใบนี้บนฟิลด์ การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นที่เปิดใช้งานเอฟเฟกต์นี้ ขณะที่การ์ดใบนี้อยู่ในตำแหน่งป้องกัน ฝ่ายตรงข้ามไม่สามารถดำเนิน Battle Phase ของพวกเขาได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [83] Ekibyo Drakmord (ID: 69954399)
**Lua File:** `script\official\c69954399.lua`

**Description:**
> มอนสเตอร์ที่สวมใส่ไม่สามารถโจมตีได้ ทำลายมอนสเตอร์ที่สวมใส่เมื่อสิ้นสุดเทิร์นที่ 2 ของผู้ควบคุมหลังจากเปิดใช้งานการ์ดใบนี้ ในเวลานั้น การ์ดใบนี้จะกลับขึ้นมือของเจ้าของ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_EQUIP)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	c:RegisterEffect(e2)
```

---

## [84] Elemental Absorber (ID: 94253609)
**Lua File:** `script\official\c94253609.lua`

**Description:**
> นำการ์ดมอนสเตอร์ 1 ใบในมือของคุณออกจากเกมเพื่อเปิดใช้งานการ์ดใบนี้ มอนสเตอร์ของฝ่ายตรงข้ามที่มีธาตุเดียวกันกับมอนสเตอร์ที่ถูกนำออกโดยเอฟเฟกต์นี้ไม่สามารถประกาศโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetOperation(s.operation)
	c:RegisterEffect(e1)
	--Opponent's monsters with the same Attribute cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(0,LOCATION_MZONE)
	e2:SetTarget(s.atktarget)
	c:RegisterEffect(e2)
	e1:SetLabelObject(e2)
end
function s.cfilter(c)
	return c:IsMonster() and c:IsAbleToRemove()
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
```

---

## [85] Eternal Bond (ID: 45283341)
**Lua File:** `script\official\c45283341.lua`

**Description:**
> เลือกเป้าหมายมอนสเตอร์ "Photon" ในสุสานของคุณกี่ใบก็ได้; อัญเชิญแบบพิเศษพวกมัน แต่เอฟเฟกต์ของพวกมันถูกยกเลิก ในช่วงเมนเฟสของคุณ: คุณสามารถนำการ์ดใบนี้จากสุสานของคุณออกนอกเกม จากนั้น เลือกเป้าหมายมอนสเตอร์ "Photon" 1 ตัวที่คู่ต่อสู้ของคุณควบคุม; ควบคุมมัน และหากทำเช่นนั้น ค่า ATK ของมันสำหรับเทิร์นที่เหลือจะกลายเป็นค่า ATK เดิมรวมกันของมอนสเตอร์ "Photon" ทั้งหมดที่คุณควบคุมอยู่ในขณะนี้ และคุณไม่สามารถประกาศโจมตีในเทิร์นนี้ได้ ยกเว้นด้วยมอนสเตอร์นั้น คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Eternal Bond" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local tc=Duel.GetFirstTarget()
	if tc:IsRelateToEffect(e) then
		Duel.GetControl(tc,tp)
		--cannot attack
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetLabelObject(tc)
		e1:SetTarget(s.atktg)
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
		--change ATK
```

---

## [86] Euler's Circuit (ID: 9547962)
**Lua File:** `script\official\c9547962.lua`

**Description:**
> มอนสเตอร์ของฝ่ายตรงข้ามไม่สามารถโจมตีได้หากคุณควบคุมมอนสเตอร์ 'Tindangle' 3 ตัวขึ้นไป เทิร์นละครั้ง ในช่วงสแตนด์บายเฟสของคุณ: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 'Tindangle' ที่คุณควบคุม 1 ตัว; ส่งการควบคุมให้ฝ่ายตรงข้าม คุณสามารถนำการ์ดนี้จากสุสานของคุณออกนอกเกมและทิ้งการ์ด 'Tindangle' 1 ใบ; นำ 'Euler's Circuit' 1 ใบจากเด็คของคุณขึ้นมือ คุณสามารถใช้เอฟเฟกต์นี้ของ 'Euler's Circuit' ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_FZONE)
	e2:SetTargetRange(0,LOCATION_MZONE)
	e2:SetCondition(s.atkcon)
	c:RegisterEffect(e2)
	--give control
	local e3=Effect.CreateEffect(c)
	e3:SetCategory(CATEGORY_CONTROL)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_O)
```

---

## [87] Evil HERO Malicious Bane (ID: 86165817)
**Lua File:** `script\official\c86165817.lua`

**Description:**
> 1 มอนสเตอร์ "Evil HERO" + มอนสเตอร์ที่มีเลเวล 5 ขึ้นไป 1 ตัว
ต้องถูกอัญเชิญแบบพิเศษด้วย "Dark Fusion" ไม่สามารถถูกทำลายจากการต่อสู้หรือเอฟเฟกต์การ์ดได้ ใน Main Phase ของคุณ: คุณสามารถทำลายมอนสเตอร์ทั้งหมดที่คู่ต่อสู้ควบคุมที่มี ATK น้อยกว่าหรือเท่ากับการ์ดใบนี้ และการ์ดใบนี้จะได้รับ ATK 200 ต่อมอนสเตอร์แต่ละตัวที่ถูกทำลายด้วยวิธีนี้ และคุณไม่สามารถประกาศโจมตีในเทิร์นที่เหลือได้ ยกเว้นด้วยมอนสเตอร์ "HERO" คุณสามารถใช้เอฟเฟกต์นี้ของ "Evil HERO Malicious Bane" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(function(e,c) return not c:IsSetCard(SET_HERO) end)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,1))
	if not c:IsRelateToEffect(e) or c:IsFacedown() then return end
	local g=Duel.GetMatchingGroup(aux.FaceupFilter(Card.IsAttackBelow,c:GetAttack()),tp,0,LOCATION_MZONE,nil)
	if #g>0 and Duel.Destroy(g,REASON_EFFECT)>0 then
		--Gains 200 ATK for each destroyed monster
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_UPDATE_ATTACK)
		e1:SetValue(#(Duel.GetOperatedGroup())*200)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD_DISABLE)
		c:RegisterEffect(e1)
```

---

## [88] Evolution Burst (ID: 52875873)
**Lua File:** `script\official\c52875873.lua`

**Description:**
> หากคุณควบคุม "Cyber Dragon": เลือกเป้าหมายการ์ด 1 ใบที่ฝ่ายตรงข้ามควบคุม; ทำลายเป้าหมายนั้น "Cyber Dragon" ไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานการ์ดนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.cost(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return Duel.GetFlagEffect(tp,id)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTarget(aux.TargetBoolFunction(Card.IsCode,CARD_CYBER_DRAGON))
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
```

---

## [89] Fairy Archer Ingunar (ID: 44451698)
**Lua File:** `script\official\c44451698.lua`

**Description:**
> ไม่สามารถโจมตีในเทิร์นที่การ์ดใบนี้ถูกอัญเชิญแบบพิเศษ ถ้าการ์ดใบนี้ถูกอัญเชิญแบบพิเศษโดยเอฟเฟกต์ของมอนสเตอร์พืช: คุณสามารถเลือกเป้าหมายมอนสเตอร์พืชเลเวล 6 ขึ้นไปในสุสานของคุณ 1 ตัว; อัญเชิญแบบพิเศษมันในท่าโจมตีป้องกัน และคุณไม่สามารถอัญเชิญแบบพิเศษได้จนกว่าจะสิ้นสุดเทิร์นนี้ ยกเว้นมอนสเตอร์พืช คุณสามารถใช้เอฟเฟกต์นี้ของ "Fairy Archer Ingunar" ได้เพียงครั้งเดียวต่อเทิร์น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [90] Fake Hero (ID: 78387742)
**Lua File:** `script\official\c78387742.lua`

**Description:**
> อัญเชิญแบบพิเศษมอนสเตอร์ 'Elemental HERO' 1 ตัวจากมือ แต่ไม่สามารถโจมตีได้ และให้คืนกลับขึ้นมือในช่วง End Phase

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot attack
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1,true)
		--Return it to hand during end phase
		local e2=Effect.CreateEffect(e:GetHandler())
		e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
		e2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
```

---

## [91] Feather Shot (ID: 19394153)
**Lua File:** `script\official\c19394153.lua`

**Description:**
> เลือกเป้าหมาย "Elemental HERO Avian" หงายหน้า 1 ตัวที่คุณควบคุม; ในเทิร์นนี้ เป้าหมายนั้นสามารถโจมตีได้จำนวนครั้งเท่ากับจำนวนมอนสเตอร์ที่คุณควบคุมเมื่อการ์ดใบนี้แก้ไขผล แต่ไม่สามารถโจมตีโดยตรง และมอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ในเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(g:GetFirst():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc:IsRelateToEffect(e) then
		local ct=Duel.GetFieldGroupCount(tp,LOCATION_MZONE,0)
		if ct>1 then
			--Attack up to the number of monsters you control
			local e1=Effect.CreateEffect(e:GetHandler())
			e1:SetType(EFFECT_TYPE_SINGLE)
			e1:SetCode(EFFECT_EXTRA_ATTACK)
			e1:SetValue(ct-1)
			e1:SetReset(RESETS_STANDARD_PHASE_END)
			tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```
### Effect 2 (EFFECT_CANNOT_DIRECT_ATTACK)
```lua
--Cannot attack directly
			local e2=Effect.CreateEffect(e:GetHandler())
			e2:SetDescription(3207)
			e2:SetProperty(EFFECT_FLAG_CLIENT_HINT)
			e2:SetType(EFFECT_TYPE_SINGLE)
			e2:SetCode(EFFECT_CANNOT_DIRECT_ATTACK)
			e2:SetReset(RESETS_STANDARD_PHASE_END)
			tc:RegisterEffect(e2)
		end
	end
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
```

---

## [92] Fiend's Sanctuary (ID: 24874630)
**Lua File:** `script\official\c24874630.lua`

**Description:**
> อัญเชิญแบบพิเศษ "Metal Fiend Token" (ปีศาจ/มืด/เลเวล 1/ATK 0/DEF 0) 1 ตัว โทเคนนี้ไม่สามารถโจมตีได้ ฝ่ายตรงข้ามรับความเสียหายจากการต่อสู้ทั้งหมดที่คุณจะได้รับจากการต่อสู้ที่เกี่ยวข้องกับโทเคนนี้ เทิร์นละครั้ง ระหว่างสแตนด์บายเฟสของคุณ จ่าย 1000 LP หรือทำลายโทเคนนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local token=Duel.CreateToken(tp,id+1)
	Duel.SpecialSummon(token,0,tp,tp,false,false,POS_FACEUP)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e1:SetReset(RESET_EVENT|RESETS_STANDARD)
	token:RegisterEffect(e1,true)
	local e2=Effect.CreateEffect(e:GetHandler())
	e2:SetType(EFFECT_TYPE_SINGLE)
	e2:SetCode(EFFECT_REFLECT_BATTLE_DAMAGE)
```

---

## [93] Fiendish Chain (ID: 50078509)
**Lua File:** `script\official\c50078509.lua`

**Description:**
> เปิดใช้งานการ์ดใบนี้โดยเลือกเป้าหมายมอนสเตอร์เอฟเฟกต์ 1 ตัวบนฟิลด์; ยกเลิกเอฟเฟกต์ของมอนสเตอร์ที่หงายหน้านั้นในขณะที่มันอยู่บนฟิลด์ และมอนสเตอร์ที่หงายหน้านั้นไม่สามารถโจมตีได้ เมื่อมันถูกทำลาย จงทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e3:SetRange(LOCATION_SZONE)
	e3:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e3:SetTarget(aux.PersistentTargetFilter)
	c:RegisterEffect(e3)
	--cannot attack
	local e4=e3:Clone()
	e4:SetCode(EFFECT_CANNOT_ATTACK)
	c:RegisterEffect(e4)
	--Destroy
	local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_CONTINUOUS+EFFECT_TYPE_FIELD)
	e5:SetRange(LOCATION_SZONE)
	e5:SetCode(EVENT_LEAVE_FIELD)
	e5:SetCondition(s.descon)
	e5:SetOperation(s.desop)
	c:RegisterEffect(e5)
end
```

---

## [94] Fiendsmith's Sanct (ID: 35552985)
**Lua File:** `script\official\c35552985.lua`

**Description:**
> หากคุณไม่มีมอนสเตอร์หงายหน้าควบคุมอยู่ หรือมอนสเตอร์หงายหน้าที่คุณควบคุมมีเพียงมอนสเตอร์ LIGHT Fiend: อัญเชิญ "Fiendsmith Token" (Fiend/LIGHT/Level 1/ATK 0/DEF 0) 1 ใบแบบพิเศษ และคุณไม่สามารถประกาศโจมตีได้จนกว่าจะสิ้นสุดเทิร์นนี้ ยกเว้นด้วยมอนสเตอร์ Fiend หากมอนสเตอร์ "Fiendsmith" ที่หงายหน้าซึ่งคุณควบคุมถูกทำลายด้วยเอฟเฟกต์การ์ดของคู่ต่อสู้ ขณะที่การ์ดใบนี้อยู่ในสุสานของคุณ: คุณสามารถเซ็ตการ์ดใบนี้ คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Fiendsmith's Sanct" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot declare attacks for the rest of this turn, except with Fiend monsters
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(function(_,c) return not c:IsRace(RACE_FIEND) end)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,2))
end
function s.setconfilter(c,tp)
```

---

## [95] Fire Prison (ID: 269510)
**Lua File:** `script\official\c269510.lua`

**Description:**
> มอนสเตอร์ประเภทมังกรทั้งหมดบนฟิลด์ได้รับ DEF 300 แต้ม ถ้ามีมอนสเตอร์ลิงก์ใดๆ บนฟิลด์ ผู้เล่นทั้งสองฝ่ายไม่สามารถอัญเชิญลิงก์มอนสเตอร์ที่มีเรตลิงก์ต่ำกว่าเรตลิงก์สูงสุดบนฟิลด์ได้ มอนสเตอร์ไม่สามารถโจมตีได้ ยกเว้นมอนสเตอร์ลิงก์ ใช้เอฟเฟกต์ต่อไปนี้ในขณะที่มีมอนสเตอร์ลิงก์ไซเบอร์ส 2 ตัวขึ้นไปบนฟิลด์
●ยกเลิกเอฟเฟกต์ที่ถูกเปิดใช้งานของมอนสเตอร์ไซเบอร์ส
●มอนสเตอร์ไซเบอร์สไม่สามารถโจมตี ไม่สามารถถูกเลือกเป็นเป้าหมายสำหรับการโจมตี และไม่สามารถถูกเลือกเป็นเป้าหมายด้วยเอฟเฟกต์การ์ด

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e3:SetRange(LOCATION_FZONE)
	e3:SetTargetRange(1,1)
	e3:SetTarget(s.splimit)
	c:RegisterEffect(e3)
	--cannot attack
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_FIELD)
	e4:SetCode(EFFECT_CANNOT_ATTACK)
	e4:SetRange(LOCATION_FZONE)
	e4:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e4:SetTarget(s.atktg)
	c:RegisterEffect(e4)
	--disable
	local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
	e5:SetCode(EVENT_CHAIN_SOLVING)
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
e5:SetRange(LOCATION_FZONE)
	e5:SetCondition(s.discon)
	e5:SetOperation(s.disop)
	c:RegisterEffect(e5)
	--cannot attack
	local e6=Effect.CreateEffect(c)
	e6:SetType(EFFECT_TYPE_FIELD)
	e6:SetCode(EFFECT_CANNOT_ATTACK)
	e6:SetRange(LOCATION_FZONE)
	e6:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e6:SetCondition(s.limcon)
	e6:SetTarget(s.atlimit)
	c:RegisterEffect(e6)
	--cannot be battle target
	local e7=Effect.CreateEffect(c)
	e7:SetType(EFFECT_TYPE_FIELD)
```

---

## [96] Flying Pegasus Railroad Stampede (ID: 88875132)
**Lua File:** `script\official\c88875132.lua`

**Description:**
> หากการ์ดใบนี้ถูกอัญเชิญแบบปกติหรือพิเศษ: คุณสามารถเลือกเป้าหมายมอนสเตอร์ EARTH ประเภทจักรกลในสุสานของคุณ 1 ตัว ยกเว้น "Flying Pegasus Railroad Stampede"; อัญเชิญแบบพิเศษมันในตำแหน่งป้องกัน แต่ยกเลิกเอฟเฟกต์ของมัน คุณสามารถเลือกเป้าหมายมอนสเตอร์หงายหน้าอื่น 1 ตัวที่คุณควบคุม; เลเวลของมอนสเตอร์นั้นหรือการ์ดใบนี้กลายเป็นเลเวลของอีกตัว คุณไม่สามารถประกาศโจมตีในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้ ยกเว้นกับมอนสเตอร์ Xyz คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Flying Pegasus Railroad Stampede" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	local e3=Effect.CreateEffect(e:GetHandler())
	e3:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_CLIENT_HINT)
	e3:SetDescription(aux.Stringid(id,3))
	e3:SetReset(RESET_PHASE|PHASE_END)
	e3:SetTargetRange(1,0)
	Duel.RegisterEffect(e3,tp)
end
function s.atktg(e,c)
	return not c:IsType(TYPE_XYZ)
end
function s.lvfilter(c,lv)
	return c:IsFaceup() and c:IsLevelAbove(1) and not c:IsLevel(lv)
end
function s.lvtg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	local c=e:GetHandler()
	local lv=c:GetLevel()
	if chkc then return chkc:IsLocation(LOCATION_MZONE) and s.lvfilter(chkc,lv) end
	if chk==0 then return Duel.IsExistingTarget(s.lvfilter,tp,LOCATION_MZONE,0,1,c,lv) end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TARGET)
	Duel.SelectTarget(tp,s.lvfilter,tp,LOCATION_MZONE,0,1,1,c,lv)
end
function s.lvop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local tc=Duel.GetFirstTarget()
	if c:IsFaceup() and c:IsRelateToEffect(e) and tc and tc:IsFaceup() and tc:IsRelateToEffect(e) and not tc:IsLevel(c:GetLevel()) then
		local g=Group.FromCards(c,tc)
		Duel.Hint(HINT_SELECTMSG,tp,aux.Stringid(id,2)) --Select the monster with the level you want
		local sg=g:Select(tp,1,1,nil)
		local oc=(g-sg):GetFirst()
		local e1=Effect.CreateEffect(c)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CHANGE_LEVEL)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		e1:SetValue(sg:GetFirst():GetLevel())
		oc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.atktg(e,c)
	return not c:IsType(TYPE_XYZ)
end
```

---

## [97] Geargiauger (ID: 47687766)
**Lua File:** `script\official\c47687766.lua`

**Description:**
> เมื่อการ์ดใบนี้ถูกอัญเชิญแบบปกติ: คุณสามารถเพิ่มมอนสเตอร์ประเภทเครื่องจักรธาตุดินเลเวล 4 1 ตัวจากเด็คของคุณขึ้นมือ ยกเว้น "Geargiauger" และคุณไม่สามารถประกาศโจมตีหรืออัญเชิญแบบพิเศษมอนสเตอร์ ยกเว้นประเภทเครื่องจักร ในเทิร์นที่เหลือ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if #g>0 then
		Duel.SendtoHand(g,nil,REASON_EFFECT)
		Duel.ConfirmCards(1-tp,g)
	end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET)
	e1:SetTargetRange(1,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	local e2=Effect.CreateEffect(e:GetHandler())
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetProperty(EFFECT_FLAG_PLAYER_TARGET)
```

---

## [98] Ghostrick Ghoul (ID: 85463083)
**Lua File:** `script\official\c85463083.lua`

**Description:**
> ไม่สามารถ Normal Summon ได้ เว้นแต่คุณจะควบคุมมอนสเตอร์ "Ghostrick" เทิร์นละครั้ง: คุณสามารถเปลี่ยนการ์ดใบนี้เป็นตำแหน่งป้องกันคว่ำ เทิร์นละครั้ง ใน Main Phase 1 ของคุณ: คุณสามารถเลือกเป้าหมายมอนสเตอร์ "Ghostrick" 1 ตัวที่คุณควบคุม; ATK ของมันจะเท่ากับ ATK เดิมรวมของมอนสเตอร์ "Ghostrick" ทั้งหมดที่อยู่บนฟิลด์ในปัจจุบัน จนกระทั่งสิ้นสุดเทิร์นถัดไปของฝ่ายตรงข้าม แต่หากทำเช่นนั้น มีเพียงมอนสเตอร์นั้นเท่านั้นที่สามารถโจมตีในเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(g:GetFirst():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.atkop(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc:IsFaceup() and tc:IsRelateToEffect(e) then
		local atk=0
		local g=Duel.GetMatchingGroup(s.filter,tp,LOCATION_MZONE,LOCATION_MZONE,nil)
		local bc=g:GetFirst()
		for bc in aux.Next(g) do
			local catk=bc:GetBaseAttack()
			if catk<0 then catk=0 end
			atk=atk+catk
		end
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_SET_ATTACK_FINAL)
		e1:SetValue(atk)
		e1:SetReset(RESETS_STANDARD_PHASE_END,2)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [99] Ghostrick Museum (ID: 7617062)
**Lua File:** `script\official\c7617062.lua`

**Description:**
> มอนสเตอร์ที่คุณควบคุม ยกเว้นมอนสเตอร์ 'Ghostrick' ไม่สามารถโจมตีได้ มอนสเตอร์ไม่สามารถโจมตีมอนสเตอร์ในตำแหน่งป้องกันคว่ำหน้าได้ แต่สามารถโจมตีโดยตรงได้หากมอนสเตอร์ทั้งหมดที่คู่ต่อสู้ควบคุมอยู่ในตำแหน่งป้องกันคว่ำหน้า เมื่อสิ้นสุดแดเมจสเต็ป หากมอนสเตอร์สร้างความเสียหายจากการต่อสู้ให้กับผู้เล่นในระหว่างการต่อสู้นี้: เปลี่ยนมอนสเตอร์นั้นให้อยู่ในตำแหน่งป้องกันคว่ำหน้า

* ข้อความข้างต้นเป็นข้อความที่ไม่เป็นทางการและอธิบายการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_FZONE)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.ftarget)
	c:RegisterEffect(e2)
	--atklimit
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_SELECT_BATTLE_TARGET)
	e3:SetRange(LOCATION_FZONE)
```

---

## [100] Ghostrick Night (ID: 85827713)
**Lua File:** `script\official\c85827713.lua`

**Description:**
> ในขณะที่มอนสเตอร์ "Ghostrick" อยู่บนฟิลด์ คู่ต่อสู้ของคุณไม่สามารถฟลิปซัมมอนได้ หากการ์ดใบนี้ที่ครอบครองถูกทำลายโดยคู่ต่อสู้ของคุณและส่งไปยังสุสานของคุณ: คู่ต่อสู้ของคุณไม่สามารถประกาศการโจมตีได้สำหรับส่วนที่เหลือของเทิร์นนี้

* ข้อความข้างต้นไม่เป็นทางการและอธิบายการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.limop(e,tp,eg,ep,ev,re,r,rp)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetRange(LOCATION_SZONE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_CLIENT_HINT)
	e1:SetDescription(aux.Stringid(id,1))
	e1:SetTargetRange(0,1)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
```

---

## [101] Ghostrick or Treat (ID: 27170599)
**Lua File:** `script\official\c27170599.lua`

**Description:**
> หากคุณควบคุม Field Spell "Ghostrick" หรือ Link Monster "Ghostrick": คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่หงายหน้าที่คู่ต่อสู้ของคุณควบคุม 1 ตัว; คู่ต่อสู้ของคุณสามารถจ่าย 2000 LP เพื่อทำให้เอฟเฟกต์ของการ์ดใบนี้เป็น "ตั้งการ์ดใบนี้แทนที่จะส่งไปยังสุสานหลังจากการเปิดใช้งาน" มิฉะนั้น สำหรับเทิร์นที่เหลือ มอนสเตอร์ที่หงายหน้านั้นไม่สามารถโจมตีได้ เอฟเฟกต์ของมันจะถูกยกเลิก และมันจะเปลี่ยนเป็นตำแหน่งป้องกันคว่ำหน้าในช่วง End Phase คุณสามารถเปิดใช้งาน "Ghostrick or Treat" ได้เทิร์นละ 1 ใบเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
tc:RegisterEffect(e2)
		--Cannot attack
		local e3=Effect.CreateEffect(c)
		e3:SetDescription(3206)
		e3:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e3:SetType(EFFECT_TYPE_SINGLE)
		e3:SetCode(EFFECT_CANNOT_ATTACK)
		e3:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e3)
		if not tc:IsImmuneToEffect(e) then
			--Change Position during the End Phase
			local fid=c:GetFieldID()
			tc:RegisterFlagEffect(id,RESETS_STANDARD_PHASE_END,0,1,fid)
			local e4=Effect.CreateEffect(c)
```

---

## [102] Gimmick Puppet Gigantes Doll (ID: 7593748)
**Lua File:** `script\official\c7593748.lua`

**Description:**
> มอนสเตอร์ 'Gimmick Puppet' เลเวล 4 จำนวน 2 ตัว
คุณสามารถถอดซ้อนทับ 2 ตัวจากการ์ดใบนี้ จากนั้นเลือกเป้าหมายมอนสเตอร์ที่คู่ต่อสู้ควบคุมได้สูงสุด 2 ตัว; ควบคุมมอนสเตอร์เหล่านั้นจนจบเอนด์เฟส และเทิร์นนี้คุณจะไม่สามารถอัญเชิญแบบพิเศษมอนสเตอร์ได้ ยกเว้นมอนสเตอร์ 'Gimmick Puppet' หรือประกาศโจมตีได้ ยกเว้นด้วยมอนสเตอร์ Xyz คุณสามารถสังเวยการ์ดใบนี้; มอนสเตอร์ทั้งหมดที่คุณควบคุมอยู่ในตอนนี้จะกลายเป็นเลเวล 8 จนจบเทิร์นนี้ คุณสามารถใช้แต่ละเอฟเฟกต์ของ 'Gimmick Puppet Gigantes Doll' ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
ge1:SetTarget(s.splimit)
	ge1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(ge1,tp)
	--cannot attack
	local ge2=Effect.CreateEffect(c)
	ge2:SetType(EFFECT_TYPE_FIELD)
	ge2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	ge2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	ge2:SetTargetRange(LOCATION_MZONE,0)
	ge2:SetTarget(s.atktg)
	ge2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(ge2,tp)
	--client hint
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,2),nil)
```

---

## [103] Gishki Grimness (ID: 38356857)
**Lua File:** `script\official\c38356857.lua`

**Description:**
> หากคุณอัญเชิญพิธีกรรมมอนสเตอร์พิธีกรรมธาตุน้ำ 1 ตัวพอดีด้วยเอฟเฟกต์การ์ดที่ต้องใช้มอนสเตอร์ การ์ดนี้สามารถใช้เป็นสังเวยทั้งหมดได้ หากการ์ดนี้ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษ: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์ "Gishki" 1 ตัวจากเด็ค ยกเว้น "Gishki Grimness" และคุณสามารถประกาศโจมตีด้วยมอนสเตอร์พิธีกรรมเท่านั้นจนกระทั่งจบเทิร์นนี้ คุณสามารถใช้เอฟเฟกต์นี้ของ "Gishki Grimness" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local c=e:GetHandler()
	--Cannot declare attacks, except with Ritual Monsters
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(function(e,c) return not c:IsRitualMonster() end)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,1),nil)
```

---

## [104] Gizmek Inaba, the Hopping Hare of Hakuto (ID: 50901852)
**Lua File:** `script\official\c50901852.lua`

**Description:**
> เมื่อการ์ดใบนี้ถูกอัญเชิญแบบปกติ: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์เครื่องจักร 1 ตัวที่มี ATK เท่ากับ DEF ของตัวเองจากมือของคุณในตำแหน่งป้องกัน คุณสามารถนำการ์ดใบนี้จากสุสานของคุณออกนอกเกม จากนั้นเลือกเป้าหมายมอนสเตอร์เครื่องจักร 1 ตัวที่คุณควบคุมที่มี ATK เท่ากับ DEF ของตัวเอง; เทิร์นนี้ คุณไม่สามารถประกาศโจมตีได้ ยกเว้นด้วยมอนสเตอร์นั้น และ ATK/DEF ของมันจะกลายเป็น ATK เดิมรวมของมอนสเตอร์เครื่องจักรทั้งหมดที่คุณควบคุมอยู่ในปัจจุบันที่มี ATK เท่ากับ DEF ของตัวเอง คุณสามารถใช้เอฟเฟกต์นี้ของ 'Gizmek Inaba, the Hopping Hare of Hakuto' ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(aux.Stringid(id,1))
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(e:GetLabel())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	local tc=Duel.GetFirstTarget()
	if tc:IsRelateToEffect(e) then
		local atk=0
		local g=Duel.GetMatchingGroup(s.ffilter,tp,LOCATION_MZONE,0,nil)
		for tc in aux.Next(g) do
			atk=atk+tc:GetBaseAttack()
		end
		--Set ATK/DEF
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_SET_ATTACK_FINAL)
		e1:SetValue(atk)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [105] Gizmek Okami, the Dreaded Deluge Dragon (ID: 43218406)
**Lua File:** `script\official\c43218406.lua`

**Description:**
> คุณสามารถจ่าย LP 1500 แต้ม; ทำลายมอนสเตอร์ทั้งหมดบนฟิลด์ที่ถูกอัญเชิญแบบพิเศษจากเอ็กซ์ตร้าเด็ค และคุณสามารถโจมตีด้วยมอนสเตอร์เพียง 1 ตัวในเทิร์นนี้ คุณสามารถใช้แต่ละเอฟเฟกต์ต่อไปนี้ของ "Gizmek Okami, the Dreaded Deluge Dragon" ได้เทิร์นละครั้งเท่านั้น หากมอนสเตอร์ที่ถูกอัญเชิญแบบพิเศษจากเอ็กซ์ตร้าเด็ค 2 ตัวขึ้นไปอยู่บนฟิลด์: คุณสามารถอัญเชิญการ์ดใบนี้จากมือแบบพิเศษ หากการ์ดใบนี้ที่คุณครอบครองถูกส่งไปยังสุสานของคุณโดยฝ่ายตรงข้าม: คุณสามารถนำมอนสเตอร์ 1 ตัวจากสุสานของฝ่ายตรงข้ามออกนอกเกม จากนั้นคุณได้รับ LP เท่ากับ ATK ของมัน

* ข้อความข้างต้นไม่เป็นทางการและอธิบายการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(aux.Stringid(id,3))
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetCondition(s.atkcon)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	local e2=Effect.CreateEffect(e:GetHandler())
	e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [106] Gladiator Beast United (ID: 66290900)
**Lua File:** `script\official\c66290900.lua`

**Description:**
> ในช่วง Battle Phase: สับกลับเข้าเด็ค จากมือ ฟิลด์ หรือสุสานของคุณ วัตถุฟิวชันที่ระบุไว้บนมอนสเตอร์ฟิวชัน 'Gladiator Beast' จากนั้นอัญเชิญแบบพิเศษมอนสเตอร์ฟิวชันนั้นจากเอ็กซ์ตร้าเด็คของคุณ โดยไม่สนใจเงื่อนไขการอัญเชิญ คุณสามารถเปิดใช้งาน "Gladiator Beast United" ได้เทิร์นละ 1 ใบเท่านั้น คุณไม่สามารถประกาศโจมตีในเทิร์นที่คุณเปิดใช้งานการ์ดนี้ได้ ยกเว้นกับมอนสเตอร์ 'Gladiator Beast'

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if chk==0 then return Duel.GetCustomActivityCount(id,tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,1),nil)
end
```

---

## [107] Goblin Biker Grand Breakout (ID: 29111045)
**Lua File:** `script\official\c29111045.lua`

**Description:**
> สังเวยมอนสเตอร์ 1 ตัว; อัญเชิญแบบพิเศษมอนสเตอร์ "Goblin" 1 ตัวจากเด็คของคุณ แต่ไม่สามารถโจมตีในเทิร์นนี้ เมื่อมอนสเตอร์ประกาศโจมตี: คุณสามารถนำการ์ดใบนี้ออกจากเกมจากสุสานของคุณและถอดวัตถุดิบจำนวนเท่าใดก็ได้จากมอนสเตอร์ Xyz "Goblin" ที่คุณควบคุม; มอนสเตอร์ทั้งหมดที่ฝ่ายตรงข้ามควบคุมในปัจจุบันสูญเสีย ATK 1000 แต้มต่อวัตถุดิบ 1 ชิ้นที่ถูกถอด จนจบเทิร์นนี้ คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Goblin Biker Grand Breakout" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1,true)
	end
end
function s.atkcostfilter(c)
	return c:IsSetCard(SET_GOBLIN) and c:IsType(TYPE_XYZ) and c:IsFaceup() and c:GetOverlayCount()>0
end
function s.atkcost(e,tp,eg,ep,ev,re,r,rp,chk)
	e:SetLabel(-1)
	local c=e:GetHandler()
	local xyzg=Duel.GetMatchingGroup(s.atkcostfilter,tp,LOCATION_MZONE,0,nil)
	if chk==0 then return c:IsAbleToRemoveAsCost() and #xyzg>0 and Duel.CheckRemoveOverlayCard(tp,0,0,1,REASON_COST,xyzg) end
	Duel.Remove(c,POS_FACEUP,REASON_COST)
	local maxct=xyzg:GetSum(Card.GetOverlayCount)
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_REMOVEXYZ)
	local ct=Duel.RemoveOverlayCard(tp,0,0,1,maxct,REASON_COST,xyzg)
	e:SetLabel(ct)
end
function s.atktg(e,tp,eg,ep,ev,re,r,rp,chk)
	local atkg=Duel.GetMatchingGroup(Card.IsFaceup,tp,0,LOCATION_MZONE,nil)
	if chk==0 then
		local cost_chk=e:GetLabel()==-1
		e:SetLabel(0)
		return cost_chk and #atkg>0
	end
	Duel.SetOperationInfo(0,CATEGORY_ATKCHANGE,atkg,#atkg,tp,e:GetLabel()*-1000)
end
function s.atkop(e,tp,eg,ep,ev,re,r,rp,chk)
	local g=Duel.GetMatchingGroup(Card.IsFaceup,tp,0,LOCATION_MZONE,nil)
	if #g==0 then return end
	local c=e:GetHandler()
	local atk=e:GetLabel()*-1000
	for tc in g:Iter() do
		--It loses 1000 ATK for each material detached
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetCode(EFFECT_UPDATE_ATTACK)
		e1:SetValue(atk)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```

---

## [108] Gora Turtle (ID: 80233946)
**Lua File:** `script\official\c80233946.lua`

**Description:**
> ตราบใดที่การ์ดใบนี้ยังคงหงายหน้าอยู่บนฟิลด์ มอนสเตอร์ที่มี ATK เท่ากับ 1900 หรือมากกว่าไม่สามารถประกาศโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetRange(LOCATION_MZONE)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetTarget(s.atktarget)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.atktarget(e,c)
	return c:IsAttackAbove(1900)
end
```

---

## [109] Gouki Finishing Move (ID: 35870016)
**Lua File:** `script\official\c35870016.lua`

**Description:**
> เลือกเป้าหมาย "Gouki" Link มอนสเตอร์ 1 ตัวที่คุณควบคุม; มันได้รับ ATK เท่ากับ Link Rating ของมัน x 1000 จนกระทั่งสิ้นสุดเทิร์นนี้ และหากมันโจมตีมอนสเตอร์ในตำแหน่งป้องกันในเทิร์นนี้ ให้สร้างความเสียหายต่อสู้แบบทะลุทะลวงแก่ฝ่ายตรงข้าม หลังจากที่การ์ดนี้แก้ไข จนสิ้นสุดเทิร์นนี้ คุณจะไม่สามารถประกาศโจมตีได้ ยกเว้นด้วย "Gouki" มอนสเตอร์ คุณสามารถเปิดใช้งาน "Gouki Finishing Move" ได้เทิร์นละ 1 ใบเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.activate(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	--Non-"Gouki" monsters cannot attack
	local ge1=Effect.CreateEffect(c)
	ge1:SetType(EFFECT_TYPE_FIELD)
	ge1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	ge1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	ge1:SetTargetRange(LOCATION_MZONE,0)
	ge1:SetTarget(function(e,c) return not c:IsSetCard(SET_GOUKI) end)
	ge1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(ge1,tp)
	--Client hint
```

---

## [110] Grapple Blocker (ID: 32907538)
**Lua File:** `script\official\c32907538.lua`

**Description:**
> เมื่อการ์ดใบนี้ถูกอัญเชิญแบบปกติ คุณสามารถเลือกมอนสเตอร์ที่คู่ต่อสู้ควบคุม 1 ตัว มอนสเตอร์ที่ถูกเลือกไม่สามารถโจมตีหรือถูกสังเวยได้ ในขณะที่การ์ดใบนี้ยังคงหงายอยู่บนฟิลด์ ในแต่ละ End Phase ของคุณ จ่าย 500 Life Points หรือทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		e1:SetCondition(s.rcon)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.rcon(e)
	return e:GetOwner():IsHasCardTarget(e:GetHandler())
end
```

---

## [111] Grave of the Super Ancient Organism (ID: 83266092)
**Lua File:** `script\official\c83266092.lua`

**Description:**
> มอนสเตอร์ที่ถูกอัญเชิญแบบพิเศษที่มีเลเวล 6 ขึ้นไปบนฟิลด์ไม่สามารถประกาศโจมตีได้ และผู้เล่นไม่สามารถเปิดใช้งานเอฟเฟกต์ของพวกมันได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
c:RegisterEffect(e1)
	--Level 6 or higher Special Summoned monsters on the field cannot declare attacks
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.target)
	c:RegisterEffect(e2)
	--Players cannot activate the effects of Level 6 or higher Special Summoned monsters
	local e3=Effect.CreateEffect(c)
```

---

## [112] Gravity Bind (ID: 85742772)
**Lua File:** `script\official\c85742772.lua`

**Description:**
> มอนสเตอร์เลเวล 4 ขึ้นไปไม่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.atktarget)
	c:RegisterEffect(e2)
end
function s.atktarget(e,c)
	return c:GetLevel()>=4
```

---

## [113] Great Sand Sea - Gold Golgonda (ID: 60884672)
**Lua File:** `script\official\c60884672.lua`

**Description:**
> มอนสเตอร์เอ็กซ์ซีส "Springans" ทั้งหมดบนฟิลด์ได้รับ ATK 1000 แต้ม คุณสามารถใช้แต่ละเอฟเฟกต์ต่อไปนี้ของ "Great Sand Sea - Gold Golgonda" ได้เทิร์นละครั้งเท่านั้น หากคุณไม่มีมอนสเตอร์เอ็กซ์ซีส "Springans" ควบคุม: คุณสามารถทิ้งการ์ด "Springans" 1 ใบ; อัญเชิญแบบพิเศษมอนสเตอร์เอ็กซ์ซีส "Springans" 1 ตัวจากเอ็กซ์ตร้าเด็คของคุณ หากมอนสเตอร์เอ็กซ์ซีสหงายหน้าที่คุณควบคุมออกจากฟิลด์ด้วยเอฟเฟกต์การ์ด (ยกเว้นในช่วง Damage Step): คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่คู่ต่อสู้ของคุณควบคุม; มันไม่สามารถโจมตีได้ในเทิร์นที่เหลือนี้ (แม้การ์ดนี้จะออกจากฟิลด์)

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetDescription(3206)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```

---

## [114] Grisaille Prison (ID: 22888900)
**Lua File:** `script\official\c22888900.lua`

**Description:**
> ถ้าคุณควบคุมมอนสเตอร์ที่หงายหน้าอยู่ที่ถูกอัญเชิญแบบสังเวย, อัญเชิญแบบพิธีกรรม หรืออัญเชิญแบบฟิวชัน: จนกระทั่งสิ้นสุดเทิร์นถัดไปของฝ่ายตรงข้าม ผู้เล่นทั้งสองฝ่ายไม่สามารถอัญเชิญซิงโครหรืออัญเชิญเอ็กซีดได้ มอนสเตอร์ซิงโครและเอ็กซีดไม่สามารถโจมตี และเอฟเฟกต์ของพวกมันจะถูกยกเลิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetReset(RESET_PHASE|PHASE_END|RESET_OPPO_TURN,2)
	else
		e1:SetReset(RESET_PHASE|PHASE_END|RESET_OPPO_TURN)
	end
	Duel.RegisterEffect(e1,tp)
	local e2=e1:Clone()
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	Duel.RegisterEffect(e2,tp)
	--
	local e3=Effect.CreateEffect(e:GetHandler())
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_SPECIAL_SUMMON)
	e3:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_CLIENT_HINT)
	e3:SetDescription(aux.Stringid(id,1))
```

---

## [115] Heavy Armored Train Ironwolf (ID: 49121795)
**Lua File:** `script\official\c49121795.lua`

**Description:**
> มอนสเตอร์ Level 4 ประเภท Machine 2 ตัว
เทิร์นละครั้ง: คุณสามารถถอด Xyz Material 1 ตัวจากการ์ดใบนี้ จากนั้นเลือกเป้าหมายมอนสเตอร์ประเภท Machine ที่คุณควบคุม 1 ตัว; เทิร์นนี้ มันสามารถโจมตีผู้เล่นโดยตรงได้โดยตรง และมอนสเตอร์อื่นไม่สามารถโจมตีได้ หากการ์ดใบนี้ที่ครอบครองของถูกทำลายโดยการ์ดของฝ่ายตรงข้าม (โดยการต่อสู้หรือเอฟเฟกต์การ์ด) และถูกส่งลง Graveyard ของคุณ: คุณสามารถเพิ่มมอนสเตอร์ประเภท Machine Level 4 1 ตัวจากเด็คของคุณขึ้นมือ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.daop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local tc=Duel.GetFirstTarget()
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(tc:GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	if tc:IsRelateToEffect(e) then
		local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_SINGLE)
```

---

## [116] Heliosphere Dragon (ID: 51043053)
**Lua File:** `script\official\c51043053.lua`

**Description:**
> ในขณะที่ฝ่ายตรงข้ามมีการ์ดในมือ 4 ใบหรือน้อยกว่าและการ์ดใบนี้เป็นมอนสเตอร์เดียวที่คุณควบคุม ฝ่ายตรงข้ามไม่สามารถประกาศโจมตีได้ เทิร์นละครั้ง หากคุณควบคุมมอนสเตอร์ประเภทมังกรเลเวล 8: คุณสามารถทำให้การ์ดใบนี้มีเลเวล 8 จนกว่าจะสิ้นสุดเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetRange(LOCATION_MZONE)
	e1:SetTargetRange(0,LOCATION_MZONE)
	e1:SetCondition(s.atcon)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.atcon(e)
	return Duel.GetFieldGroupCount(e:GetHandlerPlayer(),LOCATION_MZONE,0)==1
		and Duel.GetFieldGroupCount(e:GetHandlerPlayer(),0,LOCATION_HAND)<5
end
```

---

## [117] Hieratic Dragon of Tefnuit (ID: 77901552)
**Lua File:** `script\official\c77901552.lua`

**Description:**
> หากมีเพียงมอนสเตอร์ของฝ่ายตรงข้ามที่ควบคุมอยู่ คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้ (จากมือ) ได้ การ์ดใบนี้ไม่สามารถโจมตีได้ในเทิร์นที่มันถูกอัญเชิญแบบพิเศษด้วยวิธีนี้ เมื่อการ์ดใบนี้ถูกสังเวย: อัญเชิญแบบพิเศษมอนสเตอร์ปกติประเภทมังกร 1 ตัวจากมือ เด็ค หรือสุสานของคุณ และทำให้ ATK/DEF ของมันเป็น 0

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESET_EVENT|(RESETS_STANDARD_PHASE_END&~RESET_TOFIELD))
	c:RegisterEffect(e1)
```

---

## [118] Hot Red Dragon Archfiend (ID: 39765958)
**Lua File:** `script\official\c39765958.lua`

**Description:**
> จูนเนอร์ 1 ตัว + มอนสเตอร์ที่ไม่ใช่จูนเนอร์ 1+ ตัว
เทิร์นละครั้ง ในช่วง Main Phase 1 ของคุณ: คุณสามารถทำลายมอนสเตอร์ตัวอื่นในตำแหน่งโจมตีที่หงายหน้าอยู่บนฟิลด์ทั้งหมด มอนสเตอร์อื่นไม่สามารถโจมตีในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.descost(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return true end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(e:GetHandler():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	local e2=Effect.CreateEffect(e:GetHandler())
```

---

## [119] Hyper Psychic Riser (ID: 99115354)
**Lua File:** `script\official\c99115354.lua`

**Description:**
> จูนเนอร์ 1 ตัว + มอนสเตอร์ที่ไม่ใช่จูนเนอร์ 1 ตัวขึ้นไป
มอนสเตอร์ที่มี ATK น้อยกว่าการ์ดนี้ไม่สามารถโจมตีได้ และผู้เล่นทั้งสองฝ่ายไม่สามารถเปิดใช้งานเอฟเฟกต์ของมอนสเตอร์ที่หงายหน้าอยู่บนฟิลด์ที่มี ATK มากกว่าการ์ดนี้ หากการ์ดใบนี้ที่คุณครอบครองถูกทำลายด้วยการ์ดของคู่ต่อสู้และถูกส่งไปยังสุสานของคุณ: คุณสามารถเลือกเป้าหมายจูนเนอร์ 1 ตัวและมอนสเตอร์ที่ไม่ใช่จูนเนอร์ 1 ตัวในสุสานของคุณ ที่มีประเภทและคุณลักษณะเดียวกัน; นำพวกมันขึ้นมือ คุณสามารถใช้เอฟเฟกต์นี้ของ 'Hyper Psychic Riser' ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetRange(LOCATION_MZONE)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTarget(s.atktg)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.atktg(e,c)
	return c:GetAttack()<e:GetHandler():GetAttack()
end
```

---

## [120] Inferno Fire Blast (ID: 52684508)
**Lua File:** `script\official\c52684508.lua`

**Description:**
> เลือกเป้าหมาย "Red-Eyes Black Dragon" 1 ใบในโซนมอนสเตอร์ของคุณ; สร้างความเสียหายให้ฝ่ายตรงข้ามเท่ากับ ATK ดั้งเดิมของ "Red-Eyes Black Dragon" นั้น "Red-Eyes Black Dragon" ไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_OATH)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTarget(aux.TargetBoolFunction(Card.IsCode,CARD_REDEYES_B_DRAGON))
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
```

---

## [121] Insect Barrier (ID: 23615409)
**Lua File:** `script\official\c23615409.lua`

**Description:**
> มอนสเตอร์ประเภทแมลงที่ฝ่ายตรงข้ามควบคุมไม่สามารถประกาศโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(0,LOCATION_MZONE)
	e2:SetTarget(s.atktarget)
	c:RegisterEffect(e2)
end
function s.atktarget(e,c)
	return c:IsRace(RACE_INSECT)
```

---

## [122] Instant Contact (ID: 16169772)
**Lua File:** `script\official\c16169772.lua`

**Description:**
> จ่าย LP 1000 แต้ม; อัญเชิญแบบพิเศษมอนสเตอร์ "Elemental HERO" หรือ "Neo-Spacian" ที่มีเลเวล 7 หรือต่ำกว่า 1 ตัวจากเอ็กซ์ตร้าเด็คของคุณ โดยไม่สนใจเงื่อนไขการอัญเชิญ แต่หาก "Elemental HERO Neos" ไม่อยู่บนฟิลด์ของคุณหรือในสุสานของคุณ ให้ใช้เอฟเฟกต์นี้กับมัน:
● มันไม่สามารถโจมตีได้, เอฟเฟกต์ของมันถูกยกเลิก, และมันจะกลับไปที่เอ็กซ์ตร้าเด็คในช่วง End Phase
คุณสามารถเปิดใช้งาน "Instant Contact" ได้เทิร์นละ 1 ใบเท่านั้น

* ข้อความข้างต้นไม่เป็นทางการและอธิบายฟังก์ชันการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local c=e:GetHandler()
		--Cannot attack
		local e1=Effect.CreateEffect(c)
		e1:SetDescription(aux.Stringid(id,1))
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1,true)
		--Effects are negated
		local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_SINGLE)
		e2:SetCode(EFFECT_DISABLE)
```

---

## [123] Invoked Caliga (ID: 13529466)
**Lua File:** `script\official\c13529466.lua`

**Description:**
> "Aleister the Invoker" + มอนสเตอร์มืด 1 ตัว
ผู้เล่นแต่ละคนสามารถพยายามเปิดใช้งานเอฟเฟกต์มอนสเตอร์ได้เทิร์นละ 1 ครั้งเท่านั้น ผู้เล่นแต่ละคนสามารถโจมตีด้วยมอนสเตอร์เพียง 1 ตัวในแต่ละแบทเทิลเฟส

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e7=Effect.CreateEffect(c)
	e7:SetType(EFFECT_TYPE_FIELD)
	e7:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e7:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e7:SetRange(LOCATION_MZONE)
	e7:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e7:SetCondition(s.atkcon)
	e7:SetTarget(s.atktg)
	c:RegisterEffect(e7)
```
Associated helper functions:
```lua
function s.atkcon(e)
	return e:GetHandler():GetFlagEffect(id+2)~=0
end
```
```lua
function s.atktg(e,c)
	return c:GetFieldID()~=e:GetLabel()
end
```

---

## [124] Inzektor Hopper (ID: 52601736)
**Lua File:** `script\official\c52601736.lua`

**Description:**
> เทิร์นละครั้ง: คุณสามารถให้มอนสเตอร์ 'Inzektor' 1 ตัวจากมือหรือสุสานของคุณเป็นการ์ดสวมใส่ให้การ์ดใบนี้ได้ ขณะที่การ์ดใบนี้ถูกสวมใส่ให้มอนสเตอร์ ระดับของมอนสเตอร์นั้นจะเพิ่มขึ้น 4 ขณะที่การ์ดใบนี้ถูกสวมใส่ให้มอนสเตอร์: คุณสามารถส่งการ์ดสวมใส่ใบนี้ลงสุสาน; มอนสเตอร์ที่ถูกสวมใส่สามารถโจมตีตรงได้ในเทิร์นนี้ มอนสเตอร์อื่นไม่สามารถโจมตีได้ในเทิร์นที่คุณใช้เอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(function(e,c) return e:GetLabel()~=c:GetFieldID() end)
	e1:SetLabel(tc:GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.daop(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc:IsFaceup() and tc:IsRelateToEffect(e) then
		--Can attack directly
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_DIRECT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```

---

## [125] Kumongous, the Sticky String Kaiju (ID: 29726552)
**Lua File:** `script\official\c29726552.lua`

**Description:**
> คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้ (จากมือของคุณ) ลงบนฟิลด์ของฝ่ายตรงข้ามในตำแหน่งโจมตี โดยการสังเวยมอนสเตอร์ 1 ตัวที่พวกเขาควบคุมอยู่ หากฝ่ายตรงข้ามของคุณควบคุมมอนสเตอร์ "Kaiju" อยู่ คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้ (จากมือของคุณ) ในตำแหน่งโจมตี คุณสามารถควบคุมมอนสเตอร์ "Kaiju" ได้เพียง 1 ตัวเท่านั้น เมื่อฝ่ายตรงข้ามอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษมอนสเตอร์ (ยกเว้นในระหว่าง Damage Step): คุณสามารถลบ Kaiju Counter 2 ตัวจากที่ใดก็ได้บนฟิลด์; จนกว่าจะสิ้นสุดเทิร์นถัดไป มอนสเตอร์นั้นไม่สามารถโจมตีได้ และเอฟเฟกต์ของมันจะถูกยกเลิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetDescription(3206)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END,2)
		tc:RegisterEffect(e1)
```

---

## [126] Labyrinth Wall Shadow (ID: 34771947)
**Lua File:** `script\official\c34771947.lua`

**Description:**
> มอนสเตอร์ไม่สามารถโจมตีในเทิร์นที่อัญเชิญ ยกเว้นมอนสเตอร์ที่มีเลเวลเดิม 5 หรือสูงกว่า เทิร์นละครั้ง ในช่วง Main Phase ของคุณ: คุณสามารถวาง 'Sanga of the Thunder', 'Kazejin', หรือ 'Suijin' 1 ใบที่ถูกนำออกนอกเกม หรืออยู่ในมือหรือเด็คของคุณ ไว้ในโซนเวทมนตร์ & กับดักของคุณในสภาพหงายหน้าอย่างต่อเนื่องเป็นเวทมนตร์ต่อเนื่อง เมื่อเริ่มต้น Battle Phase ของคู่ต่อสู้: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่คู่ต่อสู้ควบคุมที่มี ATK น้อยกว่า 1600; ทำลายมัน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--Monsters cannot attack the turn they are Summoned
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_FZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.limtg)
	c:RegisterEffect(e2)
	--Place 1 Sanga, Kazejin, or Suijin face-up in your S&T Zone as a Continuous Spell
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,0))
	e3:SetType(EFFECT_TYPE_IGNITION)
	e3:SetRange(LOCATION_FZONE)
	e3:SetCountLimit(1)
	e3:SetTarget(s.placetg)
	e3:SetOperation(s.placeop)
	c:RegisterEffect(e3)
	--Destroy 1 opponent's monster with less than 1600 ATK
	local e4=Effect.CreateEffect(c)
	e4:SetDescription(aux.Stringid(id,1))
	e4:SetCategory(CATEGORY_DESTROY)
	e4:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_O)
	e4:SetProperty(EFFECT_FLAG_CARD_TARGET)
	e4:SetCode(EVENT_PHASE|PHASE_BATTLE_START)
	e4:SetRange(LOCATION_FZONE)
	e4:SetCountLimit(1)
	e4:SetCondition(function(_,tp) return Duel.IsTurnPlayer(1-tp) end)
	e4:SetTarget(s.destg)
	e4:SetOperation(s.desop)
	c:RegisterEffect(e4)
end
s.listed_names=CARDS_SANGA_KAZEJIN_SUIJIN
function s.limtg(e,c)
	return c:IsStatus(STATUS_SUMMON_TURN|STATUS_FLIP_SUMMON_TURN|STATUS_SPSUMMON_TURN) and (c:GetOriginalLevel()<5 or not c:HasLevel())
end
function s.plfilter(c)
	return c:IsCode(CARDS_SANGA_KAZEJIN_SUIJIN) and not c:IsForbidden() and (c:IsFaceup() or not c:IsLocation(LOCATION_REMOVED))
end
function s.placetg(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return Duel.GetLocationCount(tp,LOCATION_SZONE)>0
		and Duel.IsExistingMatchingCard(s.plfilter,tp,LOCATION_REMOVED|LOCATION_HAND|LOCATION_DECK,0,1,nil) end
end
function s.placeop(e,tp,eg,ep,ev,re,r,rp)
	if Duel.GetLocationCount(tp,LOCATION_SZONE)<=0 then return end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TOFIELD)
	local tc=Duel.SelectMatchingCard(tp,s.plfilter,tp,LOCATION_REMOVED|LOCATION_HAND|LOCATION_DECK,0,1,1,nil):GetFirst()
	if tc and Duel.MoveToField(tc,tp,tp,LOCATION_SZONE,POS_FACEUP,true) then
		--Treated as a Continuous Spell
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetCode(EFFECT_CHANGE_TYPE)
		e1:SetValue(TYPE_SPELL+TYPE_CONTINUOUS)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1)
```

---

## [127] Laval Judgment Lord (ID: 14047624)
**Lua File:** `script\official\c14047624.lua`

**Description:**
> เทิร์นละครั้ง: คุณสามารถนำมอนสเตอร์ "Laval" 1 ตัวจากสุสานของคุณออกนอกเกม; ทำให้ฝ่ายตรงข้ามได้รับความเสียหาย 1000 "Laval Judgment Lord" ไม่สามารถประกาศโจมตีในเทิร์นที่เปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
Duel.Remove(g,POS_FACEUP,REASON_COST)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTarget(aux.TargetBoolFunction(Card.IsCode,id))
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.damtg(e,tp,eg,ep,ev,re,r,rp,chk)
```

---

## [128] Legacy of the Duelist (ID: 88851326)
**Lua File:** `script\official\c88851326.lua`

**Description:**
> เมื่อมอนสเตอร์ของคุณประกาศโจมตี: คุณสามารถเลือกเป้าหมายการ์ดเวทมนตร์/กับดัก 1 ใบที่ฝ่ายตรงข้ามควบคุม; ยกเลิกการโจมตี และถ้าทำเช่นนั้น ให้ทำลายการ์ดใบนั้น ผู้เล่นแต่ละคนสามารถเซ็ตการ์ดเวทมนตร์/กับดักจากมือได้เทิร์นละ 1 ใบเท่านั้น มอนสเตอร์ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญแบบพิเศษจากเอ็กซ์ตร้าเด็ค ใน Draw Phase ของคุณ ก่อนจั่ว: คุณสามารถสละการจั่วปกติของคุณในเทิร์นนี้ และถ้าทำเช่นนั้น ให้เพิ่มมอนสเตอร์ 1 ตัวจากสุสานของคุณขึ้นมือ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_FIELD)
	e5:SetCode(EFFECT_CANNOT_ATTACK)
	e5:SetRange(LOCATION_SZONE)
	e5:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e5:SetTarget(s.attg)
	c:RegisterEffect(e5)
```
Associated helper functions:
```lua
function s.attg(e,c)
	return c:IsStatus(STATUS_SPSUMMON_TURN) and c:IsSummonLocation(LOCATION_EXTRA)
end
```

---

## [129] Lightray Sorcerer (ID: 91349449)
**Lua File:** `script\official\c91349449.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติหรือวางคว่ำได้ ต้องอัญเชิญแบบพิเศษ (จากมือ) ในขณะที่มอนสเตอร์แสงของคุณถูกนำออกนอกเกม 3 ตัวขึ้นไป และไม่สามารถอัญเชิญแบบพิเศษด้วยวิธีอื่นได้ เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์แสงของคุณที่ถูกนำออกนอกเกม 1 ตัว และมอนสเตอร์ที่หงายหน้าอยู่บนฟิลด์ 1 ตัว; สับเป้าหมายแรกลงเด็ค และถ้าทำเช่นนั้น นำเป้าหมายที่สองออกนอกเกม การ์ดใบนี้ไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้เอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1,true)
end
function s.filter1(c)
	return c:IsFaceup() and c:IsAttribute(ATTRIBUTE_LIGHT) and c:IsAbleToDeck()
end
function s.filter2(c)
	return c:IsFaceup() and c:IsAbleToRemove()
end
```

---

## [130] Link Bumper (ID: 67231737)
**Lua File:** `script\official\c67231737.lua`

**Description:**
> มอนสเตอร์ไซเบอร์ส 2 ตัว
เทิร์นละครั้ง ในตอนท้ายของแดเมจสเต็ป หากมอนสเตอร์ที่การ์ดใบนี้ชี้ไปโจมตีมอนสเตอร์ลิงค์ของฝ่ายตรงข้าม: คุณสามารถเปิดใช้งานเอฟเฟกต์นี้; มอนสเตอร์ที่โจมตีได้รับการโจมตีเพิ่มเติม 1 ครั้งบนมอนสเตอร์ลิงค์ของฝ่ายตรงข้ามในระหว่างแบทเทิลเฟสนี้ สำหรับมอนสเตอร์ลิงค์แต่ละตัวที่คุณควบคุมในปัจจุบัน ยกเว้นการ์ดใบนี้ มอนสเตอร์ของคุณ (ยกเว้นมอนสเตอร์นั้น) ไม่สามารถโจมตีในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local a=Duel.GetAttacker()
	local c=e:GetHandler()
	if chk==0 then return s[tp]==0 or a:GetFlagEffect(id)~=0 end
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(a:GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.ftarget(e,c)
```

---

## [131] Linkbelt Wall Dragon (ID: 63092423)
**Lua File:** `script\official\c63092423.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องถูกอัญเชิญแบบพิเศษด้วยเอฟเฟกต์ของมันเอง เมื่อคุณอัญเชิญลิงก์: คุณสามารถอัญเชิญการ์ดใบนี้จากมือของคุณแบบพิเศษ และหากคุณทำเช่นนั้น วางเคาน์เตอร์ 2 ตัวบนมัน หากมอนสเตอร์ถูกอัญเชิญลิงก์: นำเคาน์เตอร์ 2 ตัวนี้ออกจากมัน (หรือทั้งหมด หากน้อยกว่า 2) เทิร์นละครั้ง ในแต่ละสแตนด์บายเฟส: วางเคาน์เตอร์เหล่านี้ 1 ตัวบนการ์ดใบนี้ ไม่มีมอนสเตอร์ใดสามารถถูกอัญเชิญลิงก์ได้ เว้นแต่การ์ดใบนี้จะมีเคาน์เตอร์เหล่านี้อย่างน้อยเท่ากับลิงก์เรตของมอนสเตอร์นั้น มอนสเตอร์ไม่สามารถโจมตีได้ ยกเว้นมอนสเตอร์ลิงก์

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e3:SetRange(LOCATION_MZONE)
	e3:SetTargetRange(1,1)
	e3:SetTarget(s.splimit)
	c:RegisterEffect(e3)
	--cannot attack
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_FIELD)
	e4:SetCode(EFFECT_CANNOT_ATTACK)
	e4:SetRange(LOCATION_MZONE)
	e4:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e4:SetTarget(s.atktg)
	c:RegisterEffect(e4)
	--counter
	local e5=Effect.CreateEffect(c)
	e5:SetDescription(aux.Stringid(id,1))
	e5:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_F)
```

---

## [132] Loge's Flame (ID: 18478530)
**Lua File:** `script\official\c18478530.lua`

**Description:**
> ในขณะที่คุณควบมอนสเตอร์ "Valkyrie" อยู่ มอนสเตอร์ที่ฝ่ายตรงข้ามควบคุมที่มี ATK 2000 หรือน้อยกว่าไม่สามารถโจมตีได้ หากการ์ดใบนี้ที่คุณครอบครองถูกทำลายด้วยเอฟเฟกต์การ์ดของฝ่ายตรงข้าม: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์ "Valkyrie" เลเวล 5 ขึ้นไป 1 ตัว จากมือหรือเด็คของคุณ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--Monsters your opponent controls with 2000 or less ATK cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(0,LOCATION_MZONE)
	e2:SetCondition(s.atkcon)
	e2:SetTarget(aux.TargetBoolFunction(Card.IsAttackBelow,2000))
	c:RegisterEffect(e2)
	--Special Summon 1 Level 5 or higher "Valkyrie" monster from your hand or Deck
```

---

## [133] Lubellion the Searing Dragon (ID: 70534340)
**Lua File:** `script\official\c70534340.lua`

**Description:**
> มอนสเตอร์ธาตุมืด 1 ตัว + "Fallen of Albaz"
ถ้าการ์ดใบนี้ถูกอัญเชิญแบบฟิวชัน: คุณสามารถทิ้งการ์ด 1 ใบ; อัญเชิญแบบฟิวชันมอนสเตอร์ฟิวชันเลเวล 8 หรือต่ำกว่า 1 ตัวจากเอ็กซ์ตร้าเด็คของคุณ ยกเว้น "Lubellion the Searing Dragon" โดยการสับวัตถุดิบฟิวชันที่กล่าวถึงบนนั้นกลับเข้าไปในเด็ค จากมอนสเตอร์ของคุณบนฟิลด์ สุสาน และ/หรือการ์ดที่ถูกยกเว้นที่หงายหน้า สำหรับเทิร์นที่เหลือ การ์ดใบนี้ไม่สามารถโจมตี และคุณไม่สามารถอัญเชิญแบบพิเศษมอนสเตอร์จากเอ็กซ์ตร้าเด็คได้ ยกเว้นมอนสเตอร์ฟิวชัน คุณสามารถใช้เอฟเฟกต์นี้ของ "Lubellion the Searing Dragon" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		c:RegisterEffect(e1)
```

---

## [134] Lunalight Tiger (ID: 83190280)
**Lua File:** `script\official\c83190280.lua`

**Description:**
> [ เอฟเฟกต์เพนดูลั่ม ]
เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ "Lunalight" ในสุสานของคุณ 1 ตัว; อัญเชิญแบบพิเศษมัน แต่ไม่สามารถโจมตีได้ เอฟเฟกต์ของมันถูกยกเลิก และถูกทำลายในช่วงเอนด์เฟส
----------------------------------------
[ เอฟเฟกต์มอนสเตอร์ ]
หากการ์ดนี้บนฟิลด์ถูกทำลายด้วยการต่อสู้หรือเอฟเฟกต์การ์ด: คุณสามารถเลือกเป้าหมายมอนสเตอร์ "Lunalight" ในสุสานของคุณ 1 ตัว; อัญเชิญแบบพิเศษมัน คุณสามารถใช้เอฟเฟกต์ของ "Lunalight Tiger" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e2:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e2)
		--Cannot attack
		local e3=Effect.CreateEffect(c)
		e3:SetDescription(3206)
		e3:SetType(EFFECT_TYPE_SINGLE)
		e3:SetCode(EFFECT_CANNOT_ATTACK)
		e3:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_CLIENT_HINT)
		e3:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e3)
		tc:RegisterFlagEffect(id,RESET_EVENT|RESETS_STANDARD,0,1,fid)
		--Destroy it during end phase
		local e4=Effect.CreateEffect(c)
```

---

## [135] Madolche Chickolates (ID: 26570480)
**Lua File:** `script\official\c26570480.lua`

**Description:**
> เมื่อการ์ดใบนี้ในความครอบครองของคุณถูกทำลายโดยการ์ดของคู่ต่อสู้ (ไม่ว่าจะโดยการต่อสู้หรือโดยเอฟเฟกต์การ์ด) และถูกส่งลงสุสานของคุณ: สับการ์ดใบนี้กลับเข้าเด็ค เทิร์นละครั้ง เมื่อตำแหน่งการต่อสู้ของมอนสเตอร์ "Madolche" ที่คุณควบคุมถูกเปลี่ยน (และตอนนี้หงายหน้า) ขณะที่การ์ดใบนี้หงายหน้าอยู่บนฟิลด์ (ยกเว้นในช่วง Damage Step): คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวบนฟิลด์; เปลี่ยนมันเป็นตำแหน่งป้องกันแบบหงายหน้า และถ้าคุณทำเช่นนั้น เว้นแต่มันจะเป็นมอนสเตอร์ "Madolche" มันไม่สามารถโจมตีได้ และเอฟเฟกต์ของมันจะถูกยกเลิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
			e1:SetDescription(3206)
			e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
			e1:SetType(EFFECT_TYPE_SINGLE)
			e1:SetCode(EFFECT_CANNOT_ATTACK)
			e1:SetReset(RESET_EVENT|RESETS_STANDARD)
			tc:RegisterEffect(e1)
```

---

## [136] Magic Hole Golem (ID: 82458280)
**Lua File:** `script\official\c82458280.lua`

**Description:**
> เทิร์นละครั้ง คุณสามารถเลือกมอนสเตอร์ที่หงายหน้า 1 ตัวที่คุณควบคุม ATK ของมันลดลงครึ่งหนึ่งจนถึง End Phase และมันสามารถโจมตีฝ่ายตรงข้ามโดยตรงในเทิร์นนี้ ในระหว่างเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้ มีเพียงมอนสเตอร์ที่ถูกเลือกเท่านั้นที่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(g:GetFirst():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc:IsFaceup() and tc:IsRelateToEffect(e) then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_SET_ATTACK_FINAL)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		e1:SetValue(tc:GetAttack()/2)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [137] Magicians Unite (ID: 36045450)
**Lua File:** `script\official\c36045450.lua`

**Description:**
> หากคุณควบคุม Spellcaster มอนสเตอร์ในตำแหน่งโจมตี 2 ตัวขึ้นไป: เลือกเป้าหมายหนึ่งในนั้น; ATK ของมันกลายเป็น 3000 จนกระทั่งสิ้นสุดเทิร์นนี้ และสำหรับเทิร์นที่เหลือหลังจากที่การ์ดนี้แก้ไข Spellcaster มอนสเตอร์อื่นๆ ที่คุณควบคุมไม่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(g:GetFirst():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc:IsFaceup() and tc:IsRelateToEffect(e) then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_SET_ATTACK_FINAL)
		e1:SetValue(3000)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID() and c:IsRace(RACE_SPELLCASTER)
end
```

---

## [138] Magicore Warrior of the Relics (ID: 66078354)
**Lua File:** `script\official\c66078354.lua`

**Description:**
> ไม่สามารถโจมตีได้เว้นแต่คุณจะควบคุม 'Adventurer Token' คุณสามารถใช้แต่ละเอฟเฟกต์ต่อไปนี้ของ "Magicore Warrior of the Relics" ได้เทิร์นละครั้งเท่านั้น หากคุณควบคุม 'Adventurer Token': คุณสามารถอัญเชิญแบบพิเศษการ์ดนี้จากมือคุณ เมื่อสิ้นสุด Battle Phase หากมอนสเตอร์ของคุณที่กล่าวถึง 'Adventurer Token' ได้ต่อสู้: คุณสามารถเซ็ทกับดัก 1 ใบที่กล่าวถึง 'Adventurer Token' โดยตรงจากเด็คของคุณ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetCondition(aux.NOT(s.bravecon))
	c:RegisterEffect(e1)
```

---

## [139] Malefic Cyber End Dragon (ID: 1710476)
**Lua File:** `script\official\c1710476.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องถูกอัญเชิญแบบพิเศษ (จากมือของคุณ) โดยการนำ "Cyber End Dragon" 1 ใบจาก Extra Deck ของคุณออกนอกเกม จะมีมอนสเตอร์ "Malefic" บนฟิลด์ได้เพียง 1 ตัวเท่านั้น มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถประกาศโจมตีได้ หากไม่มีเวทมนตร์ฟิลด์หงายหน้าบนฟิลด์ ให้ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e7:SetCondition(s.descon)
	c:RegisterEffect(e7)
	--cannot announce
	local e8=Effect.CreateEffect(c)
	e8:SetType(EFFECT_TYPE_FIELD)
	e8:SetRange(LOCATION_MZONE)
	e8:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e8:SetTargetRange(LOCATION_MZONE,0)
	e8:SetTarget(s.antarget)
	c:RegisterEffect(e8)
	--spson
	local e9=Effect.CreateEffect(c)
	e9:SetType(EFFECT_TYPE_SINGLE)
	e9:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_UNCOPYABLE)
	e9:SetCode(EFFECT_SPSUMMON_CONDITION)
```

---

## [140] Malefic Paradigm Dragon (ID: 16958382)
**Lua File:** `script\official\c16958382.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือของคุณ) โดยการนำมอนสเตอร์ "Malefic" 1 ตัวจาก Extra Deck ของคุณออกนอกเกม ในขณะที่ "Malefic Paradigm Dragon" ไม่ได้อยู่บนสนาม หาก "Malefic World" ไม่ได้อยู่บนสนาม ทำลายการ์ดใบนี้ เทิร์นละครั้ง: คุณสามารถส่งการ์ด "Malefic" 1 ใบจากเด็คของคุณลงสุสาน; คืนมอนสเตอร์ซิงโครเลเวล 8 ที่ถูกนำออกนอกเกมของคุณ 1 ตัวไปยัง Extra Deck จากนั้นคุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์นั้นจาก Extra Deck และคุณสามารถโจมตีด้วยมอนสเตอร์ "Malefic" เท่านั้นสำหรับเทิร์นที่เหลือ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.retop(e,tp,eg,ep,ev,re,r,rp)
	--Can only attack with "Malefic" monsters
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	local e2=Effect.CreateEffect(e:GetHandler())
```

---

## [141] Malefic Rainbow Dragon (ID: 598988)
**Lua File:** `script\official\c598988.lua`

**Description:**
> การ์ดใบนี้ไม่สามารถอัญเชิญแบบปกติหรือเซ็ตได้ การ์ดใบนี้ไม่สามารถอัญเชิญแบบพิเศษได้ ยกเว้นโดยการนำ "Rainbow Dragon" 1 ใบจากมือหรือเด็คของคุณออกนอกเกม สามารถมีมอนสเตอร์ "Malefic" ที่หงายหน้าอยู่บนฟิลด์ได้เพียง 1 ตัวเท่านั้น มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถประกาศโจมตีได้ หากไม่มีเวทมนตร์ฟิลด์ที่หงายหน้าอยู่บนฟิลด์, ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e7:SetCondition(s.descon)
	c:RegisterEffect(e7)
	--cannot announce
	local e8=Effect.CreateEffect(c)
	e8:SetType(EFFECT_TYPE_FIELD)
	e8:SetRange(LOCATION_MZONE)
	e8:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e8:SetTargetRange(LOCATION_MZONE,0)
	e8:SetTarget(s.antarget)
	c:RegisterEffect(e8)
	--spson
	local e9=Effect.CreateEffect(c)
	e9:SetType(EFFECT_TYPE_SINGLE)
	e9:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_UNCOPYABLE)
	e9:SetCode(EFFECT_SPSUMMON_CONDITION)
```

---

## [142] Malefic Red-Eyes Black Dragon (ID: 55343236)
**Lua File:** `script\official\c55343236.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษก่อน (จากมือของคุณ) โดยนำ "Red-Eyes Black Dragon" 1 ใบจากเด็คของคุณออกนอกเกม บนฟิลด์จะมีมอนสเตอร์ "Malefic" ได้เพียง 1 ตัวเท่านั้น มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถประกาศโจมตีได้ หากไม่มีเวทมนตร์ฟิลด์หงายหน้าบนฟิลด์, ทำลายการ์ดนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e7:SetCondition(s.descon)
	c:RegisterEffect(e7)
	--cannot announce
	local e8=Effect.CreateEffect(c)
	e8:SetType(EFFECT_TYPE_FIELD)
	e8:SetRange(LOCATION_MZONE)
	e8:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e8:SetTargetRange(LOCATION_MZONE,0)
	e8:SetTarget(s.antarget)
	c:RegisterEffect(e8)
end
s.listed_names={CARD_REDEYES_B_DRAGON}
function s.descon(e)
	return not Duel.IsExistingMatchingCard(Card.IsFaceup,0,LOCATION_FZONE,LOCATION_FZONE,1,nil)
end
```

---

## [143] Malefic Stardust Dragon (ID: 36521459)
**Lua File:** `script\official\c36521459.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติหรือเซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือ) โดยการนำ "Stardust Dragon" 1 ตัวจากเอ็กซ์ตร้าเด็คของคุณออกนอกเกม บนฟิลด์จะมีมอนสเตอร์ "Malefic" ได้เพียง 1 ตัวเท่านั้น การ์ดสนามที่หงายหน้าไม่สามารถถูกทำลายด้วยเอฟเฟกต์การ์ดได้ มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถประกาศโจมตีได้ หากไม่มีการ์ดสนามที่หงายหน้าอยู่บนฟิลด์ ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e7:SetCondition(s.descon)
	c:RegisterEffect(e7)
	--cannot announce
	local e8=Effect.CreateEffect(c)
	e8:SetType(EFFECT_TYPE_FIELD)
	e8:SetRange(LOCATION_MZONE)
	e8:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e8:SetTargetRange(LOCATION_MZONE,0)
	e8:SetTarget(s.antarget)
	c:RegisterEffect(e8)
	--indes
	local e9=Effect.CreateEffect(c)
	e9:SetType(EFFECT_TYPE_FIELD)
	e9:SetCode(EFFECT_INDESTRUCTABLE_EFFECT)
	e9:SetRange(LOCATION_MZONE)
```

---

## [144] Manga Ryu-Ran (ID: 38369349)
**Lua File:** `script\official\c38369349.lua`

**Description:**
> (การ์ดใบนี้จะถูกปฏิบัติเสมือนเป็นการ์ด "Toon" เสมอ)
ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือของคุณ) ก่อนโดยการสังเวยมอนสเตอร์ 2 ตัว ในขณะที่คุณควบคุม "Toon World" ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญแบบพิเศษ คุณต้องจ่าย 500 LP เพื่อประกาศโจมตีด้วยมอนสเตอร์นี้ หาก "Toon World" บนฟิลด์ถูกทำลาย ให้ทำลายการ์ดนี้ สามารถโจมตีฝ่ายตรงข้ามโดยตรงได้ เว้นแต่พวกเขาจะควบคุมมอนสเตอร์ Toon ซึ่งในกรณีนี้การ์ดใบนี้ต้องเลือกเป้าหมายมอนสเตอร์ Toon สำหรับการโจมตีของมัน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_DIRECT_ATTACK)
```lua
e4:SetCode(EFFECT_CANNOT_SELECT_BATTLE_TARGET)
	e4:SetCondition(s.atcon)
	e4:SetValue(s.atlimit)
	c:RegisterEffect(e4)
	local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_SINGLE)
	e5:SetCode(EFFECT_CANNOT_DIRECT_ATTACK)
	e5:SetCondition(s.atcon)
	c:RegisterEffect(e5)
	--Cannot attack
	local e6=Effect.CreateEffect(c)
	e6:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_CONTINUOUS)
	e6:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e6:SetCode(EVENT_SPSUMMON_SUCCESS)
	e6:SetOperation(s.atklimit)
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [145] Mask of the Accursed (ID: 56948373)
**Lua File:** `script\official\c56948373.lua`

**Description:**
> มอนสเตอร์ที่สวมใส่ไม่สามารถโจมตีได้ เทิร์นละครั้ง ใน Standby Phase ของคุณ: สร้างความเสียหาย 500 แก่ผู้ควบคุมมอนสเตอร์ที่สวมใส่

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_EQUIP)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	c:RegisterEffect(e1)
```

---

## [146] Masked HERO Divine Wind (ID: 22093873)
**Lua File:** `script\official\c22093873.lua`

**Description:**
> ต้องอัญเชิญแบบพิเศษด้วย "Mask Change" และไม่สามารถอัญเชิญแบบพิเศษด้วยวิธีอื่นได้ ไม่สามารถถูกทำลายด้วยการต่อสู้ คู่ต่อสู้ของคุณสามารถโจมตีด้วยมอนสเตอร์เพียง 1 ตัวในแต่ละ Battle Phase เมื่อการ์ดใบนี้ทำลายมอนสเตอร์ของคู่ต่อสู้ด้วยการต่อสู้และส่งมันลงสุสาน: คุณสามารถจั่วการ์ด 1 ใบ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e2:SetCode(EFFECT_INDESTRUCTABLE_BATTLE)
	e2:SetValue(1)
	c:RegisterEffect(e2)
	--cannot attack
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e3:SetRange(LOCATION_MZONE)
	e3:SetTargetRange(0,LOCATION_MZONE)
	e3:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e3:SetCondition(s.atkcon)
	e3:SetTarget(s.atktg)
	c:RegisterEffect(e3)
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [147] Mathmech Addition (ID: 80965043)
**Lua File:** `script\official\c80965043.lua`

**Description:**
> คุณสามารถเลือกเป้าหมายมอนสเตอร์หงายหน้า 1 ตัวบนฟิลด์ คุณไม่สามารถอัญเชิญมอนสเตอร์แบบพิเศษจากเอ็กซ์ตร้าเด็คได้ตลอดเทิร์นที่เหลือนี้ ยกเว้นมอนสเตอร์ Cyberse และอัญเชิญแบบพิเศษการ์ดใบนี้จากมือของคุณ (แต่มันไม่สามารถโจมตีในเทิร์นนี้) และหากคุณทำเช่นนั้น มอนสเตอร์ที่ถูกเลือกเป้าหมายจะได้รับ ATK 1000 จนกระทั่งสิ้นสุดเทิร์นนี้ คุณสามารถใช้เอฟเฟกต์นี้ของ "Mathmech Addition" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetValue(1000)
		tc:RegisterEffect(e1)
		--Cannot attack this turn
		local e2=Effect.CreateEffect(c)
		e2:SetDescription(3206)
		e2:SetType(EFFECT_TYPE_SINGLE)
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		e2:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		e2:SetReset(RESETS_STANDARD_PHASE_END)
		c:RegisterEffect(e2)
	end
end
function s.splimit(e,c,sump,sumtype,sumpos,targetp,se)
	return c:IsLocation(LOCATION_EXTRA) and not c:IsRace(RACE_CYBERSE)
end
```

---

## [148] Mathmech Circular (ID: 36521307)
**Lua File:** `script\official\c36521307.lua`

**Description:**
> คุณสามารถส่งมอนสเตอร์ "Mathmech" 1 ตัว ยกเว้น "Mathmech Circular" จากเด็คของคุณลงสุสาน; อัญเชิญแบบพิเศษการ์ดใบนี้จากมือของคุณ อีกทั้งคุณสามารถโจมตีด้วยมอนสเตอร์ได้เพียง 1 ตัวในช่วงที่เหลือของเทิร์นนี้ หากมอนสเตอร์ "Mathmech" ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษมายังฟิลด์ของคุณในขณะที่คุณควบคุมมอนสเตอร์นี้ (ยกเว้นในช่วง Damage Step): คุณสามารถเพิ่มเวทมนตร์/กับดัก "Mathmech" 1 ใบจากเด็คของคุณขึ้นมือ คุณสามารถใช้เอฟเฟกต์ละ 1 ครั้งของ "Mathmech Circular" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
	--Can only attack with 1 monster this turn
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetCondition(function(e) return e:GetLabel()~=0 end)
	e1:SetTarget(function(e,c) return c:GetFieldID()~=e:GetLabel() end)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	local e2=Effect.CreateEffect(c)
```

---

## [149] Mathmech Subtraction (ID: 16360142)
**Lua File:** `script\official\c16360142.lua`

**Description:**
> คุณสามารถเลือกเป้าหมายมอนสเตอร์หงาย 1 ตัวบนฟิลด์; คุณไม่สามารถอัญเชิญแบบพิเศษมอนสเตอร์จากเอ็กซ์ตร้าเด็คไปจนจบเทิร์นนี้ ยกเว้นมอนสเตอร์ไซเบอร์ส และอัญเชิญแบบพิเศษการ์ดใบนี้จากมือของคุณ (แต่ไม่สามารถโจมตีในเทิร์นนี้) และหากคุณทำเช่นนั้น มอนสเตอร์ที่ถูกเลือกเป้าหมายจะสูญเสีย ATK 1000 ไปจนจบเทิร์นนี้ คุณสามารถใช้เอฟเฟกต์ของ "Mathmech Subtraction" ได้เทิร์นละครั้ง

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetValue(-1000)
		tc:RegisterEffect(e1)
		--Cannot attack this turn
		local e2=Effect.CreateEffect(c)
		e2:SetDescription(3206)
		e2:SetType(EFFECT_TYPE_SINGLE)
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		e2:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		e2:SetReset(RESETS_STANDARD_PHASE_END)
		c:RegisterEffect(e2)
	end
end
function s.splimit(e,c,sump,sumtype,sumpos,targetp,se)
	return c:IsLocation(LOCATION_EXTRA) and not c:IsRace(RACE_CYBERSE)
end
```

---

## [150] Meklord Emperor Skiel (ID: 31930787)
**Lua File:** `script\official\c31930787.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษด้วยเอฟเฟกต์ของตัวเองเท่านั้น เมื่อมอนสเตอร์หงายหน้าที่คุณควบคุมถูกทำลายโดยเอฟเฟกต์การ์ดและส่งลงสุสาน (ยกเว้นในช่วง Damage Step): คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้จากมือของคุณ มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถประกาศโจมตีได้ เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ซิงโครที่ฝ่ายตรงข้ามควบคุม 1 ตัว; สวมใส่เป้าหมายนั้นให้กับการ์ดใบนี้ การ์ดใบนี้ได้รับ ATK เท่ากับ ATK รวมของมอนสเตอร์ที่สวมใส่มันด้วยเอฟเฟกต์นี้ คุณสามารถส่งมอนสเตอร์ 1 ตัวที่คุณควบคุมซึ่งสวมใส่ให้กับการ์ดใบนี้ลงสุสาน; การ์ดใบนี้สามารถโจมตีฝ่ายตรงข้ามโดยตรงในเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetRange(LOCATION_MZONE)
	e3:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e3:SetTargetRange(LOCATION_MZONE,0)
	e3:SetTarget(s.antarget)
	c:RegisterEffect(e3)
```
Associated helper functions:
```lua
function s.antarget(e,c)
	return c~=e:GetHandler()
end
```

---

## [151] Meklord Emperor Wisel (ID: 68140974)
**Lua File:** `script\official\c68140974.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษด้วยเอฟเฟกต์ของตัวเองเท่านั้น เมื่อมอนสเตอร์ที่หงายหน้าที่คุณควบคุมถูกทำลายด้วยเอฟเฟกต์การ์ดและส่งลงสุสาน (ยกเว้นระหว่าง Damage Step): คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้จากมือของคุณ มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถประกาศโจมตีได้ เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ซิงโครที่ฝ่ายตรงข้ามควบคุม 1 ตัว; สวมใส่เป้าหมายนั้นให้กับการ์ดใบนี้ ได้รับ ATK เท่ากับ ATK รวมของมอนสเตอร์ที่สวมใส่ให้กับการ์ดใบนี้ด้วยเอฟเฟกต์นี้ เทิร์นละครั้ง เมื่อฝ่ายตรงข้ามเปิดใช้งานการ์ดเวทมนตร์ (เอฟเฟกต์ด่วน): คุณสามารถยกเลิกการเปิดใช้งาน และถ้าคุณทำเช่นนั้น ให้ทำลายมัน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetRange(LOCATION_MZONE)
	e3:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e3:SetTargetRange(LOCATION_MZONE,0)
	e3:SetTarget(s.antarget)
	c:RegisterEffect(e3)
```
Associated helper functions:
```lua
function s.antarget(e,c)
	return c~=e:GetHandler()
end
```

---

## [152] Meklord Nucleus Infinity Core (ID: 77710579)
**Lua File:** `script\official\c77710579.lua`

**Description:**
> ครั้งแรกในแต่ละเทิร์นที่การ์ดใบนี้จะถูกทำลายจากการต่อสู้ มันจะไม่ถูกทำลาย คุณสามารถใช้แต่ละเอฟเฟกต์ต่อไปนี้ของ 'Meklord Nucleus Infinity Core' ได้เทิร์นละครั้งเท่านั้น หากการ์ดใบนี้ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษ: คุณสามารถนำเวทมนตร์/กับดัก 'Meklord' 1 ใบจากเด็คของคุณขึ้นมือ หากการ์ดใบนี้ถูกทำลายด้วยเอฟเฟกต์การ์ด: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์ 'Meklord Emperor' 1 ตัวจากมือหรือเด็คของคุณที่มีแอตทริบิวต์แตกต่างจากมอนสเตอร์ที่คุณควบคุม โดยไม่สนเงื่อนไขการอัญเชิญของมัน และสำหรับเทิร์นที่เหลือนี้ คุณสามารถประกาศโจมตีด้วยมอนสเตอร์ได้เพียง 1 ตัวเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetDescription(aux.Stringid(id,2))
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetCondition(s.atkcon)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	local e2=Effect.CreateEffect(e:GetHandler())
	e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [153] Melffy of the Forest (ID: 30439101)
**Lua File:** `script\official\c30439101.lua`

**Description:**
> มอนสเตอร์เลเวล 2 จำนวน 2 ตัว
คุณสามารถถอดวัสดุ 1 ตัวจากการ์ดใบนี้; นำการ์ด "Melffy" 1 ใบจากเด็คของคุณขึ้นมือ หากมอนสเตอร์ "Melffy" หงายหน้าอีกตัวที่คุณควบคุมกลับคืนสู่มือ (ยกเว้นในช่วง Damage Step): คุณสามารถเลือกเป้าหมายมอนสเตอร์หงายหน้า 1 ตัวที่คู่ต่อสู้ควบคุม; มันไม่สามารถโจมตี และยกเลิกเอฟเฟกต์ของมันด้วย คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Melffy of the Forest" ได้เทิร์นละครั้งเท่านั้น

* ข้อความข้างต้นไม่เป็นทางการและอธิบายฟังก์ชันการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot attack
		local e3=Effect.CreateEffect(e:GetHandler())
		e3:SetDescription(3206)
		e3:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e3:SetType(EFFECT_TYPE_SINGLE)
		e3:SetCode(EFFECT_CANNOT_ATTACK)
		e3:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e3)
	end
end
```

---

## [154] Mermail Abyssgaios (ID: 74371660)
**Lua File:** `script\official\c74371660.lua`

**Description:**
> มอนสเตอร์ WATER เลเวล 7 จำนวน 2 ตัว
ในขณะที่การ์ดหงายหน้าใบนี้มี Xyz Material มอนสเตอร์เลเวล 5 หรือสูงกว่าไม่สามารถโจมตีได้ เทิร์นละครั้ง ในเทิร์นของผู้เล่นคนใดก็ได้: คุณสามารถถอด Xyz Material 1 ตัวจากการ์ดใบนี้ ยกเลิกเอฟเฟกต์ของมอนสเตอร์หงายหน้าที่ฝ่ายตรงข้ามควบคุมทั้งหมดที่มี ATK น้อยกว่าการ์ดใบนี้ จนกระทั่งสิ้นสุดเทิร์น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetRange(LOCATION_MZONE)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetTarget(aux.TargetBoolFunction(Card.IsLevelAbove,5))
	e1:SetCondition(s.dscon)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.dscon(e)
	return e:GetHandler():GetOverlayCount()~=0
end
```

---

## [155] Messenger of Peace (ID: 44656491)
**Lua File:** `script\official\c44656491.lua`

**Description:**
> มอนสเตอร์ที่มี ATK 1500 ขึ้นไปไม่สามารถประกาศโจมตีได้ เทิร์นละครั้ง ในสแตนด์บายเฟสของคุณ จ่าย LP 100 แต้มหรือทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.atktarget)
	c:RegisterEffect(e2)
	--maintain
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [156] Millennium-Eyes Restrict (ID: 41578483)
**Lua File:** `script\official\c41578483.lua`

**Description:**
> 'Relinquished' + มอนสเตอร์เอฟเฟกต์ 1 ตัว
เทิร์นละครั้ง เมื่อฝ่ายตรงข้ามเปิดใช้งานเอฟเฟกต์มอนสเตอร์ (ควิกเอฟเฟกต์): คุณสามารถเลือกเป้าหมายมอนสเตอร์เอฟเฟกต์ 1 ตัวที่ฝ่ายตรงข้ามควบคุมหรือในสุสานของพวกเขา; สวมการ์ดเป้าหมายนั้นให้กับการ์ดใบนี้ที่คุณควบคุม การ์ดใบนี้ได้รับ ATK/DEF เท่ากับมอนสเตอร์ที่สวมอยู่นั้น มอนสเตอร์ที่มีชื่อเดิมของมอนสเตอร์ที่สวมอยู่นั้นไม่สามารถโจมตีได้ และเอฟเฟกต์ของพวกมันบนฟิลด์และเอฟเฟกต์ที่เปิดใช้งานจะถูกยกเลิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_FIELD)
	e4:SetCode(EFFECT_CANNOT_ATTACK)
	e4:SetRange(LOCATION_MZONE)
	e4:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e4:SetTarget(s.distg)
	c:RegisterEffect(e4)
```
Associated helper functions:
```lua
function s.distg(e,c)
	local eqg=e:GetHandler():GetEquipGroup():Match(s.eqgfilter,nil)
	return eqg:IsExists(Card.IsOriginalCodeRule,1,nil,c:GetOriginalCodeRule())
end
```

---

## [157] Mimighoul Dungeon (ID: 86809440)
**Lua File:** `script\official\c86809440.lua`

**Description:**
> มอนสเตอร์ "Mimighoul" ที่คุณควบคุมที่ไม่ได้ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษในเทิร์นนี้ ได้รับ ATK เท่ากับ DEF เดิมของพวกมัน ผู้เล่นคนใดก็ตามที่ควบคุมมอนสเตอร์คว่ำหน้าไม่สามารถอัญเชิญแบบปกติมอนสเตอร์ หรือประกาศโจมตีด้วยมอนสเตอร์ที่ถูกอัญเชิญแบบพิเศษในเทิร์นนี้ ในช่วงเมนเฟสของคุณ คุณสามารถเพิ่มมอนสเตอร์ "Mimighoul" 1 ใบจากเด็คหรือสุสานของคุณขึ้นมือ คุณสามารถใช้เอฟเฟกต์นี้ของ "Mimighoul Dungeon" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e3:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e3:SetRange(LOCATION_FZONE)
	e3:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e3:SetTarget(function(e,c) return c:IsStatus(STATUS_SPSUMMON_TURN) and Duel.IsExistingMatchingCard(Card.IsFacedown,c:GetControler(),LOCATION_MZONE,0,1,nil) end)
	c:RegisterEffect(e3)
```

---

## [158] Mind Protector (ID: 85060248)
**Lua File:** `script\official\c85060248.lua`

**Description:**
> ผู้ควบคุมการ์ดใบนี้ต้องจ่าย 500 Life Points ในแต่ละ Standby Phase ของตนเอง หากทำไม่ได้ ให้ทำลายการ์ดใบนี้ มอนสเตอร์ที่มี ATK 2000 หรือน้อยกว่าไม่สามารถประกาศโจมตีได้ ยกเว้นมอนสเตอร์ประเภทไซคิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.atktarget)
	c:RegisterEffect(e2)
```
Associated helper functions:
```lua
function s.atktarget(e,c)
	return not c:IsRace(RACE_PSYCHIC) and c:IsAttackBelow(2000)
end
```

---

## [159] Misfortune (ID: 1036974)
**Lua File:** `script\official\c1036974.lua`

**Description:**
> เลือกมอนสเตอร์ที่หงายหน้าอยู่บนฟิลด์ที่ฝ่ายตรงข้ามควบคุม 1 ตัว สร้างความเสียหายให้ฝ่ายตรงข้ามเท่ากับครึ่งหนึ่งของ ATK เดิมของมอนสเตอร์นั้น มอนสเตอร์ของคุณไม่สามารถโจมตีได้ในเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if chk==0 then return Duel.GetActivityCount(tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.cfilter(c)
	return c:GetBaseAttack()>0 and c:IsFaceup()
end
```

---

## [160] Morphtronic Bind (ID: 85101228)
**Lua File:** `script\official\c85101228.lua`

**Description:**
> ในขณะที่คุณควบคุมมอนสเตอร์ "Morphtronic" ที่หงายหน้า มอนสเตอร์เลเวล 4 ขึ้นไปทั้งหมดที่คู่ต่อสู้ควบคุมไม่สามารถประกาศโจมตีหรือเปลี่ยนตำแหน่งการต่อสู้ได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(0,LOCATION_MZONE)
	e2:SetTarget(s.tg)
	e2:SetCondition(s.con)
	c:RegisterEffect(e2)
	--pos limit
	local e3=e2:Clone()
	e3:SetCode(EFFECT_CANNOT_CHANGE_POSITION)
	c:RegisterEffect(e3)
end
s.listed_series={SET_MORPHTRONIC}
```

---

## [161] Morphtronic Magnen Bar (ID: 45593005)
**Lua File:** `script\official\c45593005.lua`

**Description:**
> ●ขณะอยู่ในตำแหน่งโจมตี: เทิร์นละครั้ง หากคุณควบคุมมอนสเตอร์ในตำแหน่งโจมตีที่หงายหน้าอยู่อีก 2 ตัวพอดี และไม่มีมอนสเตอร์เพิ่มเติม การ์ดใบนี้จะได้รับ ATK รวมของมอนสเตอร์อีกสองตัวที่คุณควบคุมจนถึง End Phase มอนสเตอร์อื่นไม่สามารถโจมตีในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้
●ขณะอยู่ในตำแหน่งป้องกัน: มอนสเตอร์ที่คุณควบคุมไม่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetCondition(s.cond)
	c:RegisterEffect(e2)
```
Associated helper functions:
```lua
function s.cond(e)
	return e:GetHandler():IsDefensePos()
end
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(c:GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.filter(c,e)
	return c:IsFaceup() and c:IsRelateToEffect(e)
end
function s.opa(e,tp,eg,ep,ev,re,r,rp)
	local g=Duel.GetChainInfo(0,CHAININFO_TARGET_CARDS)
	local sg=g:Filter(s.filter,nil,e)
	if #sg==0 then return end
	local atk=sg:GetSum(Card.GetAttack)
	local c=e:GetHandler()
	if not c:IsRelateToEffect(e) or c:IsFacedown() then return end
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_UPDATE_ATTACK)
	e1:SetValue(atk)
	e1:SetReset(RESETS_STANDARD_DISABLE_PHASE_END)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [162] Mosaic Manticore (ID: 8483333)
**Lua File:** `script\official\c8483333.lua`

**Description:**
> ในช่วง Standby Phase ของเทิร์นถัดไปของคุณ หลังจากที่คุณอัญเชิญแบบสังเวยการ์ดใบนี้แบบหงายหน้าอยู่บนฟิลด์: อัญเชิญแบบพิเศษ จากสุสาน มอนสเตอร์จำนวนมากที่สุดเท่าที่เป็นไปได้ที่ถูกใช้สำหรับการอัญเชิญแบบสังเวยของมัน พวกมันไม่สามารถประกาศโจมตี และเอฟเฟกต์ของพวกมันถูกยกเลิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
			e1:SetType(EFFECT_TYPE_SINGLE)
			e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
			e1:SetReset(RESET_EVENT|RESETS_STANDARD)
			tc:RegisterEffect(e1)
```

---

## [163] Mystic Mine (ID: 76375976)
**Lua File:** `script\official\c76375976.lua`

**Description:**
> หากคู่ต่อสู้ควบคุมมอนสเตอร์มากกว่าคุณ คู่ต่อสู้ไม่สามารถเปิดใช้งานเอฟเฟกต์มอนสเตอร์หรือประกาศโจมตีได้ หากคุณควบคุมมอนสเตอร์มากกว่าคู่ต่อสู้ คุณไม่สามารถเปิดใช้งานเอฟเฟกต์มอนสเตอร์หรือประกาศโจมตีได้ เทิร์นละครั้ง ระหว่าง End Phase หากผู้เล่นทั้งสองควบคุมมอนสเตอร์จำนวนเท่ากัน: ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_FIELD)
	e4:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e4:SetProperty(EFFECT_FLAG_PLAYER_TARGET)
	e4:SetRange(LOCATION_FZONE)
	e4:SetTargetRange(1,0)
	e4:SetCondition(s.conself)
	c:RegisterEffect(e4)
```
Associated helper functions:
```lua
function s.conself(e)
	local tp=e:GetHandlerPlayer()
	return Duel.GetFieldGroupCount(tp,LOCATION_MZONE,0)>Duel.GetFieldGroupCount(tp,0,LOCATION_MZONE)
end
```

---

## [164] Naturia White Oak (ID: 24644634)
**Lua File:** `script\official\c24644634.lua`

**Description:**
> เมื่อคู่ต่อสู้ของคุณเปิดใช้งานการ์ดหรือเอฟเฟกต์ที่เลือกเป้าหมายการ์ดใบนี้ (ควิกเอฟเฟกต์): คุณสามารถส่งการ์ดใบนี้จากฟิลด์ลงสุสาน; อัญเชิญแบบพิเศษมอนสเตอร์ "Naturia" ระดับ 4 หรือต่ำกว่า 2 ตัว จากเด็คของคุณ แต่พวกมันไม่สามารถประกาศโจมตี และทำลายพวกมันในช่วงเอนด์เฟสของคุณ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1)
```

---

## [165] Nightmare Wheel (ID: 54704216)
**Lua File:** `script\official\c54704216.lua`

**Description:**
> เปิดใช้งานการ์ดใบนี้โดยเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; มันไม่สามารถโจมตีหรือเปลี่ยนตำแหน่งการต่อสู้ได้ เมื่อมันออกจากฟิลด์ ทำลายการ์ดใบนี้ เทิร์นละครั้ง ในช่วงสแตนด์บายเฟสของคุณ: สร้างความเสียหาย 500 แต้มให้กับฝ่ายตรงข้าม มอนสเตอร์นั้นต้องอยู่บนฟิลด์เพื่อเปิดใช้งานและแก้ไขเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetRange(LOCATION_SZONE)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetTarget(aux.PersistentTargetFilter)
	c:RegisterEffect(e1)
```

---

## [166] Nightmare's Steelcage (ID: 58775978)
**Lua File:** `script\official\c58775978.lua`

**Description:**
> การ์ดใบนี้คงอยู่บนฟิลด์เป็นเวลา 2 เทิร์นของฝ่ายตรงข้าม ในขณะที่การ์ดใบนี้หงายหน้าอยู่บนฟิลด์ ไม่มีมอนสเตอร์ใดสามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetCategory(CATEGORY_POSITION)
	e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	e1:SetTarget(s.target)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	c:RegisterEffect(e2)
	--remain field
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_SINGLE)
	e3:SetCode(EFFECT_REMAIN_FIELD)
	c:RegisterEffect(e3)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return e:IsHasType(EFFECT_TYPE_ACTIVATE) end
	local c=e:GetHandler()
	c:SetTurnCounter(0)
	--destroy
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e1:SetCode(EVENT_PHASE+PHASE_END)
	e1:SetCountLimit(1)
	e1:SetRange(LOCATION_SZONE)
	e1:SetCondition(s.descon)
	e1:SetOperation(s.desop)
	e1:SetReset(RESETS_STANDARD_PHASE_END|RESET_OPPO_TURN,2)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return e:IsHasType(EFFECT_TYPE_ACTIVATE) end
	local c=e:GetHandler()
	c:SetTurnCounter(0)
	--destroy
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e1:SetCode(EVENT_PHASE+PHASE_END)
	e1:SetCountLimit(1)
	e1:SetRange(LOCATION_SZONE)
	e1:SetCondition(s.descon)
	e1:SetOperation(s.desop)
	e1:SetReset(RESETS_STANDARD_PHASE_END|RESET_OPPO_TURN,2)
	c:RegisterEffect(e1)
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_SINGLE)
	e3:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_UNCOPYABLE+EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_SET_AVAILABLE)
	e3:SetCode(1082946)
	e3:SetLabelObject(e1)
	e3:SetOwnerPlayer(tp)
	e3:SetOperation(s.reset)
	e3:SetReset(RESETS_STANDARD_PHASE_END|RESET_OPPO_TURN,2)
	c:RegisterEffect(e3)
end
```
```lua
function s.descon(e,tp,eg,ep,ev,re,r,rp)
	return Duel.IsTurnPlayer(1-tp)
end
```

---

## [167] Noble Knight Brothers (ID: 57690191)
**Lua File:** `script\official\c57690191.lua`

**Description:**
> การ์ดใบนี้สามารถโจมตีได้ก็ต่อเมื่อคุณควบคุมมอนสเตอร์ "Noble Knight" จำนวน 3 ตัวพอดี (และไม่มีมอนสเตอร์อื่น) เมื่อการ์ดใบนี้ถูกอัญเชิญแบบปกติ: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์ "Noble Knight" จากมือของคุณได้มากสุด 2 ตัว และคุณไม่สามารถอัญเชิญแบบพิเศษมอนสเตอร์ได้ในเทิร์นที่เหลือ ยกเว้นมอนสเตอร์ "Noble Knight" เทิร์นละครั้ง: คุณสามารถเลือกการ์ด "Noble Knight" และ/หรือ "Noble Arms" 3 ใบในสุสานของคุณเป็นเป้าหมาย; สับการ์ดทั้ง 3 ใบกลับเข้าเด็ค จากนั้นจั่ว 1 ใบ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetCondition(s.atcon)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.atcon(e)
	local g=Duel.GetFieldGroup(e:GetHandlerPlayer(),LOCATION_MZONE,0)
	return #g~=3 or g:IsExists(s.atkfilter,1,nil)
end
```

---

## [168] Number 26: Spaceway Octobypass (ID: 39622156)
**Lua File:** `script\official\c39622156.lua`

**Description:**
> มอนสเตอร์เลเวล 3 จำนวน 2 ตัว
เมื่อเริ่มต้น Battle Phase: คุณสามารถถอดโอเวอร์เลย์ยูนิต 1 ตัวจากการ์ดใบนี้ ใน Battle Phase นี้ ผู้เล่นสามารถโจมตีด้วยมอนสเตอร์ได้เพียง 1 ตัวเท่านั้น และการโจมตีของมันจะกลายเป็นการโจมตีโดยตรง คุณสามารถใช้เอฟเฟกต์นี้ของ "Number 26: Spaceway Octobypass" ได้เทิร์นละครั้งเท่านั้น เมื่อสิ้นสุด Damage Step หากมอนสเตอร์สร้างความเสียหายในการต่อสู้จากการโจมตีโดยตรง: ส่งการควบคุมของมอนสเตอร์ที่โจมตีให้กับคู่ต่อสู้ของผู้เล่นเทิร์นนั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local c=e:GetHandler()
	local p=Duel.GetTurnPlayer()
	--cannot attack
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetCondition(s.atkcon)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_BATTLE)
	Duel.RegisterEffect(e1,p)
	--check
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [169] Number 30: Acid Golem of Destruction (ID: 81330115)
**Lua File:** `script\official\c81330115.lua`

**Description:**
> มอนสเตอร์ระดับ 3 จำนวน 2 ตัว
ในช่วงสแตนด์บายเฟสของคุณ: ถอดวัตถุ Xyz 1 ชิ้นจากการ์ดใบนี้หรือรับความเสียหาย 2,000 แต้ม คุณไม่สามารถอัญเชิญแบบพิเศษมอนสเตอร์ใดๆ ได้ ในขณะที่การ์ดใบนี้ไม่มีวัตถุ Xyz มันไม่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_SINGLE)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetCondition(s.atcon)
	c:RegisterEffect(e2)
```
Associated helper functions:
```lua
function s.atcon(e)
	return e:GetHandler():GetOverlayCount()==0
end
```

---

## [170] Number 47: Nightmare Shark (ID: 31320433)
**Lua File:** `script\official\c31320433.lua`

**Description:**
> มอนสเตอร์เลเวล 3 จำนวน 2 ตัว
เมื่อการ์ดใบนี้ถูกอัญเชิญแบบพิเศษ: คุณสามารถแนบมอนสเตอร์ธาตุน้ำเลเวล 3 1 ตัวจากมือหรือจากฟิลด์ของคุณในการ์ดนี้เป็นวัตถุเอ็กซีส์ เทิร์นละครั้ง: คุณสามารถถอดวัตถุเอ็กซีส์ 1 ตัวจากการ์ดนี้ จากนั้นเลือกเป้าหมายมอนสเตอร์ธาตุน้ำ 1 ตัวที่คุณควบคุม; เทิร์นนี้ มอนสเตอร์นั้นสามารถโจมตีฝั่งตรงข้ามโดยตรง และไม่มีมอนสเตอร์อื่นที่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_DIRECT_ATTACK)
```lua
end
end
function s.dacon(e,tp,eg,ep,ev,re,r,rp)
	return Duel.IsAbleToEnterBP()
end
function s.filter(c)
	return c:IsFaceup() and c:IsAttribute(ATTRIBUTE_WATER) and not c:IsHasEffect(EFFECT_CANNOT_DIRECT_ATTACK)
end
function s.datg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	if chkc then return chkc:IsLocation(LOCATION_MZONE) and chkc:IsControler(tp) and s.filter(chkc) end
	if chk==0 then return Duel.IsExistingTarget(s.filter,tp,LOCATION_MZONE,0,1,nil) end
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
	end
	--Other monsters cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.ftarget)
	e2:SetLabel(e:GetLabel())
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [171] Number 67: Pair-a-Dice Smasher (ID: 35772782)
**Lua File:** `script\official\c35772782.lua`

**Description:**
> มอนสเตอร์เลเวล 5 จำนวน 2 ตัวขึ้นไป
เทิร์นละครั้ง, ใน Main Phase 1 ของคุณ: คุณสามารถถอดวัตถุ 2 ชิ้นจากการ์ดนี้; ผู้เล่นแต่ละคนทอยลูกเต๋า 6 หน้าสองครั้ง ผู้เล่นที่มีผลรวมสูงกว่าไม่สามารถเปิดใช้งานเอฟเฟกต์มอนสเตอร์หรือประกาศโจมตี จนกระทั่งสิ้นสุดเทิร์นถัดไป เทิร์นละครั้ง หากผู้เล่นคนใดคนหนึ่งทอยลูกเต๋า 6 หน้า (หรือลูกเต๋า) ขณะที่การ์ดนี้มีวัตถุ คุณสามารถถือว่าผลลัพธ์หนึ่งผลเป็น 7

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetValue(s.aclimit)
	e1:SetReset(RESET_PHASE|PHASE_END,2)
	Duel.RegisterEffect(e1,p)
	--cannot attack
	local e2=Effect.CreateEffect(e:GetHandler())
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetProperty(EFFECT_FLAG_PLAYER_TARGET)
	e2:SetTargetRange(1,0)
	e2:SetReset(RESET_PHASE|PHASE_END,2)
	Duel.RegisterEffect(e2,p)
	--client hint
	local e3=Effect.CreateEffect(e:GetHandler())
	e3:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_CLIENT_HINT)
```

---

## [172] Number 82: Heartlandraco (ID: 31437713)
**Lua File:** `script\official\c31437713.lua`

**Description:**
> มอนสเตอร์เลเวล 4 จำนวน 2 ตัว
ในขณะที่คุณควบคุมเวทมนตร์ที่หงายหน้าอยู่ ฝั่งตรงข้ามไม่สามารถเลือกการ์ดใบนี้เป็นเป้าหมายการโจมตีได้ เทิร์นละครั้ง: คุณสามารถถอดวัตถุ 1 ตัวจากการ์ดใบนี้; เทิร์นนี้ การ์ดใบนี้สามารถโจมตีฝั่งตรงข้ามโดยตรง แต่ไม่มีมอนสเตอร์อื่นที่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
c:RegisterEffect(e1)
	end
	aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,1))
	--But other monsters cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(function(e,c) return c:GetFieldID()~=fid end)
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
end
```

---

## [173] Number 97: Draglubion (ID: 28400508)
**Lua File:** `script\official\c28400508.lua`

**Description:**
> มอนสเตอร์เลเวล 8 จำนวน 2 ตัว
ฝ่ายตรงข้ามไม่สามารถเลือกการ์ดใบนี้เป็นเป้าหมายด้วยเอฟเฟกต์การ์ดได้ คุณสามารถถอดวัตถุดิบ 1 ตัวจากการ์ดใบนี้; หยิบมอนสเตอร์ "Number" ประเภทมังกรที่มีชื่อแตกต่างกัน 2 ตัวจากเอ็กซ์ตร้าเด็คและ/หรือสุสานของคุณ ยกเว้น "Number 97: Draglubion", อัญเชิญแบบพิเศษ 1 ตัวในพวกมัน และติดอีกตัวเข้ากับมันเป็นวัตถุดิบ นอกจากนี้ ในเทิร์นนี้ที่เหลือ คุณไม่สามารถอัญเชิญแบบพิเศษมอนสเตอร์อื่น หรือประกาศโจมตีได้ ยกเว้นกับมอนสเตอร์ที่ถูกอัญเชิญแบบพิเศษนั้น คุณสามารถใช้เอฟเฟกต์นี้ของ "Number 97: Draglubion" ได้เทิร์นละครั้ง

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local oc=sg-tg
			local tc=tg:GetFirst()
			Duel.Overlay(tc,oc)
			--Limit attacks for the rest of the turn
			local e1=Effect.CreateEffect(c)
			e1:SetType(EFFECT_TYPE_FIELD)
			e1:SetCode(EFFECT_CANNOT_ATTACK)
			e1:SetTargetRange(LOCATION_MZONE,0)
			e1:SetLabelObject(tc)
			e1:SetTarget(s.atktg)
			e1:SetReset(RESET_PHASE|PHASE_END)
			Duel.RegisterEffect(e1,tp)
		end
	end
	--Cannot Special Summon other monsters
	local e2=Effect.CreateEffect(c)
```

---

## [174] Obelisk the Tormentor (ID: 10000000)
**Lua File:** `script\official\c10000000.lua`

**Description:**
> ต้องสังเวย 3 ตัวเพื่ออัญเชิญแบบปกติ (ไม่สามารถเซ็ตแบบปกติได้) การอัญเชิญแบบปกติของการ์ดนี้ไม่สามารถถูกยกเลิกได้ เมื่ออัญเชิญแบบปกติ จะไม่สามารถเปิดใช้งานการ์ดและเอฟเฟกต์ได้ ผู้เล่นทั้งสองฝ่ายไม่สามารถเลือกการ์ดนี้เป็นเป้าหมายด้วยเอฟเฟกต์การ์ดได้ เทิร์นละครั้ง ในช่วง End Phase หากการ์ดใบนี้ถูกอัญเชิญแบบพิเศษ: ส่งมันลงสุสาน คุณสามารถสังเวยมอนสเตอร์ 2 ตัว; ทำลายมอนสเตอร์ทั้งหมดที่ฝ่ายตรงข้ามควบคุม การ์ดใบนี้ไม่สามารถประกาศโจมตีในเทิร์นที่เปิดใช้เอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [175] Odd-Eyes Meteorburst Dragon (ID: 80696379)
**Lua File:** `script\official\c80696379.lua`

**Description:**
> จูนเนอร์ 1 ตัว + มอนสเตอร์ที่ไม่ใช่จูนเนอร์ 1+ ตัว
เมื่อการ์ดนี้ถูกอัญเชิญแบบพิเศษ: คุณสามารถเลือกเป้าหมายการ์ด 1 ใบในโซนเพนดูลั่มของคุณ; อัญเชิญแบบพิเศษมัน, และการ์ดนี้ไม่สามารถโจมตีสำหรับเทิร์นที่เหลือ คุณสามารถใช้เอฟเฟกต์นี้ของ "Odd-Eyes Meteorburst Dragon" ได้เทิร์นละครั้งเท่านั้น มอนสเตอร์ที่ฝ่ายตรงข้ามครอบครองไม่สามารถเปิดใช้เอฟเฟกต์ของพวกเขาในช่วง Battle Phase

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		c:RegisterEffect(e1)
```

---

## [176] Opera the Melodious Diva (ID: 43268675)
**Lua File:** `script\official\c43268675.lua`

**Description:**
> ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญแบบปกติหรือหงายหน้าขึ้น หากการ์ดใบนี้ถูกส่งไปที่สุสานในฐานะวัตถุฟิวชันสำหรับการอัญเชิญแบบฟิวชัน: คุณสามารถเปิดใช้เอฟเฟกต์นี้; ตลอดเทิร์นที่เหลือนี้ มอนสเตอร์ 'Melodious' ที่คุณควบคุมจะไม่สามารถถูกทำลายในการต่อสู้หรือด้วยเอฟเฟกต์การ์ด

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [177] Paladin of White Dragon (ID: 73398797)
**Lua File:** `script\official\c73398797.lua`

**Description:**
> คุณสามารถอัญเชิญพิธีกรรม์การ์ดใบนี้ด้วย "White Dragon Ritual" ในตอนเริ่มของ Damage Step หากการ์ดใบนี้โจมตีมอนสเตอร์ในตำแหน่งป้องกันแบบตั้งหน้ากลาง: จงทำลายมอนสเตอร์ที่ตั้งหน้ากลางนั้น คุณสามารถสังเวยการ์ดใบนี้; อัญเชิญแบบพิเศษ "Blue-Eyes White Dragon" 1 ตัวจากมือหรือเด็คของคุณ แต่ "Blue-Eyes White Dragon" ไม่สามารถโจมตีได้ในเทิร์นนี้ที่เหลือ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
Duel.SpecialSummon(g,0,tp,tp,false,false,POS_FACEUP)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTarget(aux.TargetBoolFunction(Card.IsCode,CARD_BLUEEYES_W_DRAGON))
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
```

---

## [178] Parasite Paranoid (ID: 14457896)
**Lua File:** `script\official\c14457896.lua`

**Description:**
> (เอฟเฟกต์ความเร็วสูง): คุณสามารถเลือกเป้าหมายมอนสเตอร์หงายหน้าบนฟิลด์ 1 ตัว; สวมใส่การ์ดใบนี้จากมือของคุณให้กับเป้าหมายนั้น มอนสเตอร์ที่สวมใส่จะกลายเป็นมอนสเตอร์ประเภทแมลง ไม่สามารถโจมตีมอนสเตอร์ประเภทแมลงได้ และเอฟเฟกต์ของมันที่เปิดใช้งานโดยการเลือกเป้าหมายมอนสเตอร์ประเภทแมลงจะถูกยกเลิก คุณสามารถใช้เอฟเฟกต์นี้ของ "Parasite Paranoid" ได้เทิร์นละครั้งเท่านั้น หากการ์ดสวมใส่นี้ถูกส่งไปที่สุสาน: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์ประเภทแมลงเลเวล 7 หรือสูงกว่า 1 ตัวจากมือของคุณ โดยไม่สนใจเงื่อนไขการอัญเชิญ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e5=Effect.CreateEffect(c)
			e5:SetType(EFFECT_TYPE_EQUIP)
			e5:SetCode(EFFECT_CANNOT_ATTACK)
			e5:SetReset(RESET_EVENT|RESETS_STANDARD)
			c:RegisterEffect(e5)
```

---

## [179] Performapal Changeraffe (ID: 69228245)
**Lua File:** `script\official\c69228245.lua`

**Description:**
> [เอฟเฟกต์เพนดูลั่ม]
เมื่อมอนสเตอร์ 1 ตัวที่คุณควบคุม (และไม่มีการ์ดอื่น) ถูกทำลายในการต่อสู้พอดี: คุณสามารถทำลายการ์ดใบนี้ และถ้าทำ อัญเชิญแบบพิเศษมอนสเตอร์ที่ถูกทำลายในการต่อสู้ในตำแหน่งโจมตี และถ้าทำ มันไม่สามารถถูกทำลายในการต่อสู้ในเทิร์นนี้ (แม้การ์ดใบนี้ออกจากฟิลด์)
----------------------------------------
[เอฟเฟกต์มอนสเตอร์]
เมื่อการ์ดใบนี้ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษ: คุณสามารถเลือกมอนสเตอร์หงายหน้า 1 ตัวที่คู่ต่อสู้ควบคุม; ในขณะที่การ์ดใบนี้หงายหน้าอยู่บนฟิลด์ มอนสเตอร์หงายหน้าใบนั้นไม่สามารถโจมตี และยกเลิกเอฟเฟกต์ของมอนสเตอร์หงายหน้าใบนั้นในขณะที่มันอยู่บนฟิลด์

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_DISABLE)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		e1:SetCondition(s.rcon)
		tc:RegisterEffect(e1,true)
		local e2=e1:Clone()
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		tc:RegisterEffect(e2,true)
	end
end
function s.rcon(e)
	return e:GetOwner():IsHasCardTarget(e:GetHandler())
```

---

## [180] Performapal Five-Rainbow Magician (ID: 19619755)
**Lua File:** `script\official\c19619755.lua`

**Description:**
> [ เอฟเฟกต์เพนดูลัม ]
คุณไม่สามารถอัญเชิญแบบเพนดูลัมได้ ยกเว้นจากเอ็กซ์ตร้าเด็ค เอฟเฟกต์นี้ไม่ถูกยกเลิก ผู้เล่นแต่ละคนใช้ 1 ในเอฟเฟกต์เหล่านี้ตามจำนวนการ์ดที่ตั้งไว้ใน Spell & Trap Zone ของพวกเขา
●0: มอนสเตอร์ทั้งหมดที่พวกเขาควบคุมไม่สามารถโจมตีหรือเปิดใช้งานเอฟเฟกต์ได้
●4 ขึ้นไป: ATK ของมอนสเตอร์ทั้งหมดที่พวกเขาควบคุมจะกลายเป็นสองเท่าของ ATK เดิม
----------------------------------------
[ เอฟเฟกต์มอนสเตอร์ ]
หากผู้เล่นคนใดตั้งเวทมนตร์/กับดักบนฟิลด์ของคุณในขณะที่การ์ดนี้อยู่ในสุสานของคุณ (ยกเว้นระหว่าง Damage Step): คุณสามารถวางการ์ดนี้ในโซนเพนดูลัมของคุณ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_ATTACK)
	e3:SetRange(LOCATION_PZONE)
	e3:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e3:SetTarget(s.atktg)
	c:RegisterEffect(e3)
```
Associated helper functions:
```lua
function s.atktg(e,c)
	local tp=c:GetControler()
	return Duel.GetMatchingGroupCount(s.countfilter,tp,LOCATION_SZONE,0,nil)==0
end
```

---

## [181] Performapal Partnaga (ID: 69211541)
**Lua File:** `script\official\c69211541.lua`

**Description:**
> [เอฟเฟกต์เพนดูลั่ม]
เทิร์นละครั้ง: คุณสามารถเลือกมอนสเตอร์หงายหน้า 1 ตัวที่คุณควบคุม; มันได้รับ ATK 300 แต้มสำหรับการ์ด "Performapal" แต่ละใบที่คุณควบคุมอยู่ในปัจจุบัน จนจบเทิร์นนี้
----------------------------------------
[เอฟเฟกต์มอนสเตอร์]
ถ้าการ์ดใบนี้ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษ: คุณสามารถเลือกมอนสเตอร์ 1 ตัวที่คุณควบคุม; มันได้รับ ATK 300 แต้มสำหรับมอนสเตอร์ "Performapal" แต่ละตัวที่คุณควบคุมอยู่ในปัจจุบัน มอนสเตอร์ Level 5 หรือต่ำกว่าไม่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_FIELD)
	e5:SetCode(EFFECT_CANNOT_ATTACK)
	e5:SetRange(LOCATION_MZONE)
	e5:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e5:SetTarget(aux.TargetBoolFunction(Card.IsLevelBelow,5))
	c:RegisterEffect(e5)
```

---

## [182] Phantasmal Lord Ultimitl Bishbaalkin (ID: 90884403)
**Lua File:** `script\official\c90884403.lua`

**Description:**
> (เลเวลเดิมของการ์ดใบนี้จะถือว่าเป็น 12 เสมอ)
ไม่สามารถอัญเชิญแบบซิงโครได้ ต้องอัญเชิญแบบพิเศษ (จากเอ็กซ์ตร้าเด็คของคุณ) โดยส่งมอนสเตอร์เลเวล 8 หรือสูงกว่าที่คุณควบคุม 2 ตัวที่มีเลเวลเท่ากันไปสุสาน (1 ตัวเป็นทูเนอร์ 1 ตัวไม่ใช่ทูเนอร์) และไม่สามารถอัญเชิญแบบพิเศษด้วยวิธีอื่น ไม่สามารถถูกทำลายโดยเอฟเฟกต์การ์ด การ์ดใบนี้ได้รับ ATK 1000 สำหรับมอนสเตอร์แต่ละตัวบนฟิลด์ เทิร์นละครั้ง ในช่วง Main Phase ของผู้เล่นคนใดก็ตาม: คุณสามารถอัญเชิญแบบพิเศษ "Utchatzimime Token" (ประเภทปีศาจ/ธาตุมืด/เลเวล 1/ATK 0/DEF 0) จำนวนเท่ากันในตำแหน่งป้องกันบนฟิลด์ของผู้เล่นแต่ละคน เพื่ออัญเชิญให้มากที่สุดเท่าที่จะทำได้ และการ์ดใบนี้ไม่สามารถโจมตีในช่วงที่เหลือของเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		c:RegisterEffect(e1)
```

---

## [183] Phantom Knights' Fog Blade (ID: 25542642)
**Lua File:** `script\official\c25542642.lua`

**Description:**
> เปิดใช้งานการ์ดใบนี้โดยเลือกมอนสเตอร์ Effect 1 ตัวบนฟิลด์; ยกเลิกเอฟเฟกต์ของมอนสเตอร์ที่หงายหน้านั้น มอนสเตอร์ที่หงายหน้านั้นไม่สามารถโจมตีได้ และมอนสเตอร์ไม่สามารถเลือกมอนสเตอร์ที่หงายหน้านั้นเป็นเป้าหมายสำหรับการโจมตี เมื่อมันออกจากฟิลด์ ให้ทำลายการ์ดใบนี้ คุณสามารถนำการ์ดใบนี้จากสุสานของคุณออกนอกเกม จากนั้นเลือกมอนสเตอร์ "The Phantom Knights" 1 ตัวในสุสานของคุณ; อัญเชิญแบบพิเศษมัน แต่ให้นำมันออกนอกเกมเมื่อออกจากฟิลด์ คุณสามารถใช้เอฟเฟกต์นี้ของ "Phantom Knights' Fog Blade" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_ATTACK)
	e3:SetRange(LOCATION_SZONE)
	e3:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e3:SetTarget(aux.PersistentTargetFilter)
	c:RegisterEffect(e3)
```

---

## [184] Phantom Skyblaster (ID: 12958919)
**Lua File:** `script\official\c12958919.lua`

**Description:**
> เมื่อการ์ดนี้ถูกอัญเชิญแบบปกติหรือฟลิป: คุณสามารถอัญเชิญแบบพิเศษ "Skyblaster Token" จำนวนเท่าใดก็ได้ (ประเภทปีศาจ/มืด/เลเวล 4/ATK 500/DEF 500) สูงสุดเท่ากับจำนวนมอนสเตอร์ที่คุณควบคุม เทิร์นละครั้ง ระหว่าง Standby Phase ของคุณ: คุณสามารถสร้างความเสียหาย 300 แต้มให้คู่ต่อสู้สำหรับมอนสเตอร์ "Skyblaster" แต่ละตัวที่คุณควบคุม มอนสเตอร์ "Skyblaster" ที่คุณควบคุมไม่สามารถประกาศโจมตีได้ในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.damcost(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return Duel.GetFlagEffect(tp,id)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTarget(aux.TargetBoolFunction(Card.IsSetCard,SET_SKYBLASTER))
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.damtg(e,tp,eg,ep,ev,re,r,rp,chk)
```

---

## [185] Photon Sanctuary (ID: 17418744)
**Lua File:** `script\official\c17418744.lua`

**Description:**
> อัญเชิญแบบพิเศษ "Photon Token" 2 ใบ (ธันเดอร์/ไฟ/เลเวล 4/ATK 2000/DEF 0) ในรูปแบบตั้งป้องกัน โทเคนเหล่านี้ไม่สามารถโจมตีหรือถูกใช้เป็นวัตถุดิบสำหรับซิงโคร คุณไม่สามารถอัญเชิญมอนสเตอร์อื่นในเทิร์นที่คุณเปิดใช้งานการ์ดนี้ ยกเว้นมอนสเตอร์ไฟ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
Duel.SpecialSummonStep(token,0,tp,tp,false,false,POS_FACEUP_DEFENSE)
			local e1=Effect.CreateEffect(e:GetHandler())
			e1:SetType(EFFECT_TYPE_SINGLE)
			e1:SetCode(EFFECT_CANNOT_ATTACK)
			e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
			e1:SetReset(RESET_EVENT|RESETS_STANDARD)
			token:RegisterEffect(e1,true)
			local e2=Effect.CreateEffect(e:GetHandler())
			e2:SetType(EFFECT_TYPE_SINGLE)
			e2:SetCode(EFFECT_CANNOT_BE_SYNCHRO_MATERIAL)
```

---

## [186] Photon Thrasher (ID: 65367484)
**Lua File:** `script\official\c65367484.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือของคุณ) ก่อน ในขณะที่คุณไม่มีมอนสเตอร์ควบคุม ไม่สามารถโจมตีได้หากคุณควบคุมมอนสเตอร์ตัวอื่น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetRange(LOCATION_HAND)
	e1:SetCondition(s.spcon)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_SINGLE)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetCondition(s.atcon)
	c:RegisterEffect(e2)
end
function s.spcon(e,c)
	if c==nil then return true end
	return Duel.GetFieldGroupCount(c:GetControler(),LOCATION_MZONE,0,nil)==0
		and Duel.GetLocationCount(c:GetControler(),LOCATION_MZONE)>0
end
function s.atcon(e)
```

---

## [187] Photon Vanisher (ID: 43147039)
**Lua File:** `script\official\c43147039.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือของคุณ) ก่อนในขณะที่คุณควบคุมมอนสเตอร์ "Photon" หรือ "Galaxy" ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญแบบพิเศษ คุณสามารถอัญเชิญแบบพิเศษ "Photon Vanisher" ได้เทิร์นละครั้งเท่านั้น หากการ์ดใบนี้ถูกอัญเชิญแบบพิเศษ: คุณสามารถเพิ่ม "Galaxy-Eyes Photon Dragon" 1 ใบจากเด็คของคุณขึ้นมือ มอนสเตอร์เอ็กซีสที่ถูกอัญเชิญโดยใช้การ์ดใบนี้บนฟิลด์เป็นวัตถุจะได้รับเอฟเฟกต์นี้
● นำมอนสเตอร์ใดๆ ที่ถูกทำลายด้วยการต่อสู้กับการ์ดใบนี้ออกนอกเกม

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [188] Pile Armed Dragon (ID: 19153590)
**Lua File:** `script\official\c19153590.lua`

**Description:**
> คุณสามารถส่งมอนสเตอร์มังกรลมอื่น 1 ตัว หรือมอนสเตอร์มังกรเลเวล 7 หรือสูงกว่าอื่น 1 ตัวจากมือของคุณลงสุสาน; อัญเชิญแบบพิเศษการ์ดใบนี้จากมือของคุณ คุณสามารถส่งมอนสเตอร์ "Armed Dragon" 1 ตัวจากมือหรือเด็คของคุณลงสุสาน ยกเว้น "Pile Armed Dragon" จากนั้นเลือกเป้าหมายมอนสเตอร์ที่หงายหน้าอยู่ 1 ตัวที่คุณควบคุม; มอนสเตอร์ที่ถูกเลือกเป้าหมายนั้นจะได้รับ ATK เท่ากับเลเวลของมอนสเตอร์ที่ถูกส่งลงสุสาน x 300 จนกระทั่งสิ้นสุดเทิร์นนี้ และคุณสามารถโจมตีด้วยมอนสเตอร์ได้เพียง 1 ตัวในเทิร์นนี้ คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Pile Armed Dragon" ได้เทิร์นละ 1 ครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetDescription(aux.Stringid(id,2))
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_CLIENT_HINT)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetCondition(s.limitcon)
	e2:SetTarget(s.limittg)
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [189] Planckton (ID: 10282757)
**Lua File:** `script\official\c10282757.lua`

**Description:**
> ใช้เอฟเฟกต์เหล่านี้จนถึงสิ้นสุดเทิร์นนี้
● มอนสเตอร์เอ็กซีส์แรงค์ 3 หรือต่ำกว่า ได้รับ ATK และ DEF 500
● มอนสเตอร์เอ็กซีส์แรงค์ 4 หรือสูงกว่า ไม่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
Duel.RegisterEffect(e1,tp)
	local e2=e1:Clone()
	e2:SetCode(EFFECT_UPDATE_DEFENSE)
	Duel.RegisterEffect(e2,tp)
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_ATTACK)
	e3:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e3:SetReset(RESET_PHASE|PHASE_END)
	e3:SetTarget(s.filter2)
	Duel.RegisterEffect(e3,tp)
end
function s.filter1(e,c)
	return c:IsType(TYPE_XYZ) and c:IsRankBelow(3)
end
function s.filter2(e,c)
```

---

## [190] Prevention Star (ID: 94303232)
**Lua File:** `script\official\c94303232.lua`

**Description:**
> สวมใส่ให้กับมอนสเตอร์ที่คุณควบคุมเท่านั้น หากมันถูกเปลี่ยนจากตำแหน่งโจมตีหงายหน้าเป็นตำแหน่งป้องกันหงายหน้าในเทิร์นนี้ เลือกมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม มอนสเตอร์นั้นไม่สามารถโจมตีหรือเปลี่ยนตำแหน่งการต่อสู้ได้ เมื่อมอนสเตอร์ที่สวมใส่ถูกทำลายและการ์ดใบนี้ถูกส่งลงสุสาน ให้นำมอนสเตอร์ที่เลือกนั้นออกจากเกม

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetCode(EFFECT_CANNOT_CHANGE_POSITION)
			e1:SetReset(RESET_EVENT|RESETS_STANDARD)
			e1:SetCondition(s.rcon)
			dc:RegisterEffect(e1,true)
			local e2=e1:Clone()
			e2:SetCode(EFFECT_CANNOT_ATTACK)
			dc:RegisterEffect(e2,true)
		end
	end
end
function s.rcon(e)
	return e:GetOwner():IsHasCardTarget(e:GetHandler())
end
function s.rmcon(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
```

---

## [191] Proof of Powerlessness (ID: 11373345)
**Lua File:** `script\official\c11373345.lua`

**Description:**
> เปิดใช้งานเมื่อคุณควบคุมมอนสเตอร์ระดับ 7 หรือสูงกว่าหงายหน้าเท่านั้น. ทำลายมอนสเตอร์ระดับ 5 หรือต่ำกว่าหงายหน้าทั้งหมดที่ฝ่ายตรงข้ามควบคุม. มอนสเตอร์ที่คุณควบคุมไม่สามารถโจมตีในเทิร์นนี้.

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if chk==0 then return Duel.GetActivityCount(tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_OATH)
	e1:SetTargetRange(1,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
```

---

## [192] Queen Dragun Djinn (ID: 90726340)
**Lua File:** `script\official\c90726340.lua`

**Description:**
> มอนสเตอร์เลเวล 4 จำนวน 2 ตัว
มอนสเตอร์ประเภทมังกรที่คุณควบคุมไม่สามารถถูกทำลายโดยการต่อสู้ ยกเว้น "Queen Dragun Djinn" เทิร์นละครั้ง: คุณสามารถถอดวัตถุ Xyz 1 ชิ้นจากการ์ดใบนี้เพื่อเลือกเป้าหมายมอนสเตอร์ประเภทมังกรเลเวล 5 หรือสูงกว่า 1 ตัวในสุสานของคุณ; อัญเชิญแบบพิเศษเป้าหมายนั้น มันไม่สามารถโจมตีในเทิร์นนี้ และเอฟเฟกต์ของมันถูกยกเลิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot attack this turn
		local e3=Effect.CreateEffect(e:GetHandler())
		e3:SetDescription(3206)
		e3:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e3:SetType(EFFECT_TYPE_SINGLE)
		e3:SetCode(EFFECT_CANNOT_ATTACK)
		e3:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e3,true)
	end
	Duel.SpecialSummonComplete()
end
```

---

## [193] Quick Launch (ID: 31443476)
**Lua File:** `script\official\c31443476.lua`

**Description:**
> อัญเชิญแบบพิเศษมอนสเตอร์ "Rokket" 1 ตัวจากเด็คของคุณ แต่ไม่สามารถโจมตีได้ และทำลายมันในช่วงเอนด์เฟส

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot attack
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1,true)
		tc:RegisterFlagEffect(id,RESET_EVENT|RESETS_STANDARD,0,1)
		--Destroy it during end phase
		local e2=Effect.CreateEffect(e:GetHandler())
		e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [194] Rainbow Kuriboh (ID: 2830693)
**Lua File:** `script\official\c2830693.lua`

**Description:**
> คุณสามารถใช้แต่ละเอฟเฟกต์ของ 'Rainbow Kuriboh' ได้เทิร์นละครั้งเท่านั้น
●เมื่อมอนสเตอร์ของคู่ต่อสู้ประกาศโจมตี: คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่โจมตีนั้น; ติดตั้งการ์ดใบนี้จากมือของคุณให้กับมอนสเตอร์นั้น มันไม่สามารถโจมตีได้
●เมื่อมอนสเตอร์ของคู่ต่อสู้ประกาศโจมตีโดยตรง ขณะที่การ์ดใบนี้อยู่ในสุสานของคุณ: คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้ แต่ให้นำมันออกนอกเกมเมื่อมันออกจากฟิลด์

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		e1:SetValue(s.eqlimit)
		c:RegisterEffect(e1)
		local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_EQUIP)
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		e2:SetReset(RESET_EVENT|RESETS_STANDARD)
		c:RegisterEffect(e2)
	end
end
function s.eqlimit(e,c)
	return e:GetOwner()==c
end
function s.spcon(e,tp,eg,ep,ev,re,r,rp)
	return Duel.GetAttacker():IsControler(1-tp) and Duel.GetAttackTarget()==nil
end
```

---

## [195] Rapid Warrior (ID: 255998)
**Lua File:** `script\official\c255998.lua`

**Description:**
> ในช่วง Main Phase 1 ของคุณ คุณสามารถเปิดใช้งานเอฟเฟกต์ของการ์ดนี้ได้ ถ้าทำเช่นนั้น มันสามารถโจมตีฝ่ายตรงข้ามโดยตรงในเทิร์นนี้ มอนสเตอร์อื่นไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(e:GetHandler():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,1),nil)
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	if c:IsFaceup() and c:IsRelateToEffect(e) then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_DIRECT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [196] Raviel, Lord of Phantasms (ID: 69890967)
**Lua File:** `script\official\c69890967.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือของคุณ) โดยการสังเวยมอนสเตอร์ปีศาจ 3 ตัว ทุกครั้งที่ฝ่ายตรงข้ามอัญเชิญแบบปกติมอนสเตอร์: อัญเชิญแบบพิเศษ "Phantasm Token" 1 โทเค็น (ปีศาจ/มืด/ระดับ 1/ATK 1000/DEF 1000) แต่โทเค็นนั้นไม่สามารถประกาศโจมตีได้ เทิร์นละครั้ง: คุณสามารถสังเวยมอนสเตอร์ 1 ตัว; การ์ดใบนี้ได้รับ ATK เท่ากับ ATK เดิมของมอนสเตอร์ที่ถูกสังเวย จนกระทั่งจบเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		token:RegisterEffect(e1,true)
	end
end
function s.atkfilter(c)
	return c:GetTextAttack()>0
end
function s.atcost(e,tp,eg,ep,ev,re,r,rp,chk)
	local c=e:GetHandler()
	if chk==0 then return Duel.CheckReleaseGroupCost(tp,s.atkfilter,1,false,nil,c) end
	local g=Duel.SelectReleaseGroupCost(tp,s.atkfilter,1,1,false,nil,c)
	local atk=g:GetFirst():GetTextAttack()
	if atk<0 then atk=0 end
	e:SetLabel(atk)
	Duel.Release(g,REASON_COST)
end
function s.atop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	if c:IsFaceup() and c:IsRelateToEffect(e) then
		--Increase ATK
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_UPDATE_ATTACK)
		e1:SetValue(e:GetLabel())
		e1:SetReset(RESETS_STANDARD_DISABLE_PHASE_END)
		c:RegisterEffect(e1)
```

---

## [197] Ready Fusion (ID: 63854005)
**Lua File:** `script\official\c63854005.lua`

**Description:**
> จ่าย LP 1000; อัญเชิญแบบพิเศษมอนสเตอร์ฟิวชันที่ไม่ใช่เอฟเฟกต์เลเวล 6 หรือต่ำกว่า 1 ตัวจาก Extra Deck ของคุณ แต่ไม่สามารถโจมตีได้ และทำลายมันในช่วง End Phase (ถือว่าเป็นการอัญเชิญฟิวชัน) คุณสามารถเปิดใช้งาน "Ready Fusion" ได้เทิร์นละ 1 ครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot attack
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1,true)
		tc:RegisterFlagEffect(id,RESET_EVENT|RESETS_STANDARD,0,1)
		tc:CompleteProcedure()
		--Destroy it during end phase
```

---

## [198] Rebellion (ID: 87567063)
**Lua File:** `script\official\c87567063.lua`

**Description:**
> ในระหว่าง Battle Phase ของผู้เล่นคนใดก็ตาม: เลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; ยึดการควบคุมมันจนกระทั่งจบ Battle Phase รวมถึงมอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้ในเทิร์นที่เหลือของเทิร์นนี้ คุณสามารถเปิดใช้งาน "Rebellion" ได้ 1 ใบต่อเทิร์นเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local tc=Duel.GetFirstTarget()
	if tc:IsRelateToEffect(e) then
		Duel.GetControl(tc,tp,PHASE_BATTLE,1)
	end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(tc:GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
```

---

## [199] Red Screen (ID: 18634367)
**Lua File:** `script\official\c18634367.lua`

**Description:**
> มอนสเตอร์ของฝ่ายตรงข้ามไม่สามารถประกาศโจมตีได้ ในแต่ละเอนด์เฟสของคุณ คุณต้องจ่าย 1000 LP (ไม่ใช่ทางเลือก) หรือไม่เช่นนั้นการ์ดนี้จะถูกทำลาย คุณสามารถเลือกเป้าหมายมอนสเตอร์จูนเนอร์เลเวล 1 1 ตัวในสุสานของคุณ; ทำลายการ์ดนี้ และหากทำเช่นนั้น อัญเชิญแบบพิเศษเป้าหมายนั้น "Red Dragon Archfiend" ต้องอยู่บนฟิลด์เพื่อที่จะเปิดใช้งานและแก้ไขเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetTarget(s.target)
	e1:SetOperation(s.spop)
	c:RegisterEffect(e1)
	--Your opponent's monsters cannot declare attacks
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(0,LOCATION_MZONE)
	c:RegisterEffect(e2)
	--Maintenance cost
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
	e3:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_UNCOPYABLE)
```

---

## [200] Ripple Bird (ID: 56410769)
**Lua File:** `script\official\c56410769.lua`

**Description:**
> มอนสเตอร์ระดับ 1 2 ตัว
คุณสามารถถอดวัตถุ 1 ตัวจากการ์ดใบนี้ จากนั้นเลือกเป้าหมายมอนสเตอร์ 1 ตัวบนฟิลด์; เปลี่ยนท่าโจมตีของมัน ในขณะที่มอนสเตอร์ทั้งหมดที่คุณควบคุมอยู่ในท่าโจมตี, พวกมันจะได้รับ ATK 500 ในขณะที่มอนสเตอร์ทั้งหมดที่คุณควบคุมอยู่ในท่าโจมตีป้องกัน, ฝ่ายตรงข้ามไม่สามารถประกาศโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
c:RegisterEffect(e2)
	--While all monsters you control are in Defense Position, your opponent cannot declare an attack
	local e3=e2:Clone(c)
	e3:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e3:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e3:SetTargetRange(0,LOCATION_MZONE)
	e3:SetCondition(s.poscon(POS_DEFENSE))
	c:RegisterEffect(e3)
end
function s.postg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	if chkc then return chkc:IsLocation(LOCATION_MZONE) and chkc:IsCanChangePosition() end
```

---

## [201] Ritual Foregone (ID: 65450690)
**Lua File:** `script\official\c65450690.lua`

**Description:**
> จ่าย 1000 LP; อัญเชิญแบบพิเศษมอนสเตอร์ Ritual 1 ตัวจากมือของคุณ แต่มันไม่สามารถโจมตีได้ และทำลายมันในช่วง End Phase คุณสามารถเปิดใช้งาน "Ritual Foregone" ได้เทิร์นละใบเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1,true)
		tc:RegisterFlagEffect(id,RESET_EVENT|RESETS_STANDARD,0,1)
		--Destroy it during end phase
		local e2=Effect.CreateEffect(e:GetHandler())
		e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [202] Rookie Fur Hire (ID: 48214588)
**Lua File:** `script\official\c48214588.lua`

**Description:**
> สังเวยมอนสเตอร์ 1 ตัว; อัญเชิญแบบพิเศษมอนสเตอร์ "Fur Hire" 1 ตัวจากมือหรือเด็คของคุณ ที่มีเลเวลสูงกว่า 1 หรือต่ำกว่า 1 เมื่อเทียบกับเลเวลของมอนสเตอร์ที่ถูกสังเวยบนฟิลด์ คุณไม่สามารถประกาศโจมตีในเทิร์นที่คุณเปิดใช้งานการ์ดนี้ ยกเว้นด้วยมอนสเตอร์ "Fur Hire" คุณสามารถเปิดใช้งาน "Rookie Fur Hire" ได้เพียงครั้งเดียวต่อเทิร์น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e:SetLabel(100)
	if chk==0 then return Duel.GetCustomActivityCount(id,tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,1),nil)
end
```

---

## [203] Ryzeal Plugin (ID: 60394026)
**Lua File:** `script\official\c60394026.lua`

**Description:**
> เลือกเป้าหมายมอนสเตอร์ Xyz หรือมอนสเตอร์ "Ryzeal" 1 ตัวของคุณที่ถูกนำออกนอกเกมหรืออยู่ในสุสาน อัญเชิญแบบพิเศษ จากนั้นคุณสามารถติดการ์ด "Ryzeal" 1 ใบจากเด็คของคุณเข้ากับมอนสเตอร์ Xyz Rank 4 1 ตัวที่คุณควบคุม นอกจากนี้ คุณไม่สามารถประกาศโจมตีในเทิร์นที่เหลือได้ ยกเว้นด้วยมอนสเตอร์ Xyz Rank 4 คุณสามารถเปิดใช้งาน "Ryzeal Plugin" ได้เทิร์นละ 1 ใบเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetDescription(aux.Stringid(id,2))
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(function(e,c) return not (c:IsType(TYPE_XYZ) and c:IsRank(4)) end)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
```

---

## [204] Satellarknight Altair (ID: 2273734)
**Lua File:** `script\official\c2273734.lua`

**Description:**
> หากการ์ดใบนี้ถูกอัญเชิญ: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 'tellarknight' 1 ตัวในสุสานของคุณ ยกเว้น 'Satellarknight Altair'; อัญเชิญแบบพิเศษมอนสเตอร์นั้นในตำแหน่งป้องกัน และมอนสเตอร์ที่คุณควบคุมไม่สามารถโจมตีได้สำหรับช่วงที่เหลือของเทิร์นนี้ ยกเว้นมอนสเตอร์ 'tellarknight' คุณสามารถใช้เอฟเฟกต์นี้ของ 'Satellarknight Altair' ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Monsters you control cannot attack for the rest of this turn, except "tellarknight" monsters
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(function(e,c) return not c:IsSetCard(SET_TELLARKNIGHT) end)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
```

---

## [205] Secret Sanctuary of the Spellcasters (ID: 25407643)
**Lua File:** `script\official\c25407643.lua`

**Description:**
> เมื่อมอนสเตอร์ (ยกเว้นมอนสเตอร์ประเภท Spellcaster) ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษมายังฟิลด์ของฝ่ายตรงข้าม ในขณะที่คุณควบคุมการ์ดเวทมนตร์ที่หงายหน้าอยู่อีกใบ และฝ่ายตรงข้ามไม่มีการ์ดเวทมนตร์ที่หงายหน้าอยู่: มอนสเตอร์นั้นไม่สามารถโจมตีหรือใช้เอฟเฟกต์ของมันในเทิร์นนี้ (สิ่งนี้ใช้ได้แม้การ์ดใบนี้ออกจากฟิลด์) หากคุณไม่ควบคมมอนสเตอร์ประเภท Spellcaster ใด ๆ ให้ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
			e1:SetDescription(3206)
			e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
			e1:SetType(EFFECT_TYPE_SINGLE)
			e1:SetCode(EFFECT_CANNOT_ATTACK)
			e1:SetReset(RESETS_STANDARD_PHASE_END)
			tc:RegisterEffect(e1)
```

---

## [206] Shadow Spell (ID: 29267084)
**Lua File:** `script\official\c29267084.lua`

**Description:**
> เปิดใช้งานการ์ดนี้โดยเลือกเป้าหมายมอนสเตอร์ที่หงายหน้าควบคุมโดยฝ่ายตรงข้าม 1 ตัว; มันเสีย ATK 700 แต้ม และไม่สามารถโจมตีหรือเปลี่ยนตำแหน่งการต่อสู้ได้ เมื่อมันออกจากฟิลด์ ให้ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetRange(LOCATION_SZONE)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetTarget(aux.PersistentTargetFilter)
	c:RegisterEffect(e1)
	local e2=e1:Clone()
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	c:RegisterEffect(e2)
	local e3=e1:Clone()
	e3:SetCode(EFFECT_UPDATE_ATTACK)
	e3:SetValue(-700)
	c:RegisterEffect(e3)
	--Destroy
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_CONTINUOUS+EFFECT_TYPE_FIELD)
	e4:SetRange(LOCATION_SZONE)
	e4:SetCode(EVENT_LEAVE_FIELD)
```

---

## [207] Shadow Vampire (ID: 14212201)
**Lua File:** `script\official\c14212201.lua`

**Description:**
> ไม่สามารถใช้เป็นวัตถุดิบสำหรับซัมมอนเอ็กซีสได้ ยกเว้นสำหรับซัมมอนเอ็กซีสของมอนสเตอร์ DARK เมื่อการ์ดใบนี้ถูกอัญเชิญแบบปกติ: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์ "Vampire" DARK 1 ตัวจากมือหรือเด็คของคุณ ยกเว้น "Shadow Vampire" แต่เทิร์นนี้มอนสเตอร์ที่คุณควบคุมไม่สามารถโจมตีได้ ยกเว้นมอนสเตอร์ที่ถูกอัญเชิญแบบพิเศษนั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local tc=g:GetFirst()
	if tc and Duel.SpecialSummon(tc,0,tp,tp,false,false,POS_FACEUP)>0 then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetTarget(s.ftarget)
		e1:SetLabel(tc:GetFieldID())
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
		aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,1),nil)
	end
end
function s.ftarget(e,c)
```

---

## [208] Shooting Star Dragon (ID: 24696097)
**Lua File:** `script\official\c24696097.lua`

**Description:**
> มอนสเตอร์ซิงโครจูนเนอร์ 1 ตัว + "Stardust Dragon"
เที่ยวละครั้ง: คุณสามารถขุดการ์ดด้านบนสุดของเด็คของคุณ 5 ใบ สับกลับเข้าไป และจำนวนการโจมตีสูงสุดต่อแบทเทิลเฟสของการ์ดใบนี้ในเทิร์นนี้เท่ากับจำนวนมอนสเตอร์จูนเนอร์ที่ถูกขุด เที่ยวละครั้ง เมื่อการ์ดหรือเอฟเฟกต์ถูกเปิดใช้งานที่จะทำลายการ์ดบนฟิลด์ (ควิกเอฟเฟกต์): คุณสามารถยกเลิกเอฟเฟกต์ และหากทำได้ ให้ทำลายมัน เที่ยวละครั้ง เมื่อมอนสเตอร์ของคู่ต่อสู้ประกาศการโจมตี: คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่โจมตี; นำการ์ดใบนี้ออกนอกเกม และหากทำได้ ให้ยกเลิกการโจมตีนั้น ในช่วงเอนด์เฟสถัดไป: อัญเชิญแบบพิเศษการ์ดใบนี้ที่ถูกนำออกนอกเกมด้วยเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_EXTRA_ATTACK)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		e1:SetValue(ct-1)
		c:RegisterEffect(e1)
	elseif ct==0 then
		local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_SINGLE)
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		e2:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e2:SetReset(RESETS_STANDARD_PHASE_END)
		c:RegisterEffect(e2)
	end
end
function s.discon(e,tp,eg,ep,ev,re,r,rp)
	if e:GetHandler():IsStatus(STATUS_BATTLE_DESTROYED) or not Duel.IsChainNegatable(ev) then return false end
	if re:IsHasCategory(CATEGORY_NEGATE)
		and Duel.GetChainInfo(ev-1,CHAININFO_TRIGGERING_EFFECT):IsHasType(EFFECT_TYPE_ACTIVATE) then return false end
	local ex,tg,tc=Duel.GetOperationInfo(ev,CATEGORY_DESTROY)
	return ex and tg~=nil and tc+tg:FilterCount(Card.IsOnField,nil)-#tg>0
end
function s.distg(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return true end
	Duel.SetOperationInfo(0,CATEGORY_DISABLE,eg,1,0,0)
	if re:GetHandler():IsRelateToEffect(re) and re:GetHandler():IsDestructable() then
		Duel.SetOperationInfo(0,CATEGORY_DESTROY,eg,1,0,0)
	end
end
function s.disop(e,tp,eg,ep,ev,re,r,rp)
	if Duel.NegateEffect(ev) and re:GetHandler():IsRelateToEffect(re) then
		Duel.Destroy(eg,REASON_EFFECT)
	end
end
function s.dacon(e,tp,eg,ep,ev,re,r,rp)
	return Duel.GetAttacker():IsControler(1-tp)
end
function s.datg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	local c=e:GetHandler()
	if chkc then return chkc==Duel.GetAttacker() end
	if chk==0 then return c:IsAbleToRemove() and Duel.GetAttacker():IsCanBeEffectTarget(e)
		and not c:IsStatus(STATUS_CHAINING) end
	Duel.SetTargetCard(Duel.GetAttacker())
	Duel.SetOperationInfo(0,CATEGORY_REMOVE,c,1,0,0)
end
function s.daop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	if c:IsRelateToEffect(e) and Duel.Remove(c,POS_FACEUP,REASON_EFFECT)~=0 then
		Duel.NegateAttack()
		c:RegisterFlagEffect(id,RESETS_STANDARD_PHASE_END,0,0)
	end
end
function s.sumtg(e,tp,eg,ep,ev,re,r,rp,chk)
	local c=e:GetHandler()
	if chk==0 then return Duel.GetLocationCount(tp,LOCATION_MZONE)>0 and c:HasFlagEffect(id)
		and c:IsCanBeSpecialSummoned(e,0,tp,false,false) end
	Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,c,1,0,0)
end
function s.sumop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	if not c:IsRelateToEffect(e) then return end
	Duel.SpecialSummon(c,0,tp,tp,false,false,POS_FACEUP)
end
function s.valcheck(e,c)
	local g=c:GetMaterial()
	if g:IsExists(Card.IsType,2,nil,TYPE_TUNER) then
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_UNCOPYABLE)
		e1:SetCode(EFFECT_MULTIPLE_TUNERS)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD&~(RESET_TOFIELD)|RESET_PHASE|PHASE_END)
		c:RegisterEffect(e1)
```

---

## [209] Sky Scourge Enrise (ID: 11458071)
**Lua File:** `script\official\c11458071.lua`

**Description:**
> การ์ดใบนี้ไม่สามารถอัญเชิญแบบปกติหรือเซ็ต. การ์ดใบนี้ไม่สามารถอัญเชิญแบบพิเศษได้ยกเว้นโดยการนำมอนสเตอร์ประเภทนางฟ้า LIGHT 3 ตัวและมอนสเตอร์ประเภทปีศาจ DARK 1 ตัวในสุสานของคุณออกจากเกม. เทิร์นละครั้ง, คุณสามารถนำมอนสเตอร์หงายหน้า 1 ตัวบนฟิลด์ออกจากเกม. ถ้าคุณเปิดใช้งานเอฟเฟกต์นี้, การ์ดใบนี้ไม่สามารถโจมตีได้ในเทิร์นนี้.

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1,true)
end
function s.tgfilter(c)
	return c:IsFaceup() and c:IsAbleToRemove()
end
function s.rmtg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
```

---

## [210] Sky Striker Ace - Kaina (ID: 12421694)
**Lua File:** `script\official\c12421694.lua`

**Description:**
> มอนสเตอร์ 'Sky Striker Ace' 1 ตัว ที่ไม่ใช่ธาตุดิน ถ้าการ์ดใบนี้ถูกอัญเชิญแบบพิเศษ: คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่หงายหน้าอยู่ 1 ตัวที่คู่ต่อสู้ควบคุม; มอนสเตอร์นั้นไม่สามารถโจมตีได้จนกว่าจะสิ้นสุดเทิร์นของคู่ต่อสู้ ทุกครั้งที่คุณเปิดใช้งานการ์ดเวทมนตร์ 'Sky Striker' หรือเอฟเฟกต์ของมัน ให้ได้รับ LP 100 แต้มทันทีหลังจากที่การ์ดหรือเอฟเฟกต์นั้นเรโซลฟ์ คุณสามารถอัญเชิญแบบพิเศษ 'Sky Striker Ace - Kaina' ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END,ct)
		tc:RegisterEffect(e1)
```

---

## [211] Smile Universe (ID: 35259350)
**Lua File:** `script\official\c35259350.lua`

**Description:**
> อัญเชิญแบบพิเศษมอนสเตอร์ Pendulum ที่หงายหน้าอยู่จากเอ็กซ์ตร้าเด็คของคุณให้มากที่สุดเท่าที่จะทำได้ แต่เอฟเฟกต์ของพวกมันจะถูกยกเลิก (ถ้ามี) จากนั้นฝ่ายตรงข้ามได้รับ LP เท่ากับ ATK ดั้งเดิมรวมของมอนสเตอร์ที่ถูกอัญเชิญแบบพิเศษเหล่านั้น คุณไม่สามารถอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษมอนสเตอร์อื่น หรือโจมตี ในเทิร์นที่คุณเปิดใช้การ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e2:SetTargetRange(1,0)
	e2:SetTarget(s.sumlimit)
	Duel.RegisterEffect(e2,tp)
	local e3=Effect.CreateEffect(e:GetHandler())
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_ATTACK)
	e3:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e3:SetTargetRange(LOCATION_MZONE,0)
	e3:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e3,tp)
	local e4=Effect.CreateEffect(e:GetHandler())
```

---

## [212] Snowdust Dragon (ID: 67675300)
**Lua File:** `script\official\c67675300.lua`

**Description:**
> คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้ (จากมือ) โดยการเอาเคาน์เตอร์น้ำแข็ง 4 ตัวออกจากที่ใดก็ได้บนฟิลด์ มอนสเตอร์อื่นที่มีเคาน์เตอร์น้ำแข็งไม่สามารถโจมตีหรือเปลี่ยนตำแหน่งการต่อสู้ได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetRange(LOCATION_HAND)
	e1:SetCondition(s.spcon)
	e1:SetOperation(s.spop)
	c:RegisterEffect(e1)
	--atk,pos limit
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_MZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.target)
	c:RegisterEffect(e2)
	local e3=e2:Clone()
	e3:SetCode(EFFECT_CANNOT_CHANGE_POSITION)
	c:RegisterEffect(e3)
end
s.counter_list={0x1015}
function s.spcon(e,c)
```

---

## [213] Sonic Boom (ID: 93211810)
**Lua File:** `script\official\c93211810.lua`

**Description:**
> ในช่วงเทิร์นของคุณ: เลือกเป้าหมายมอนสเตอร์ "Mecha Phantom Beast" 1 ตัวบนฟิลด์; เทิร์นนี้, ATK ของมันกลายเป็นสองเท่าของ ATK เดิม, มันไม่ได้รับผลกระทบจากเอฟเฟกต์เวทมนตร์/กับดักอื่น, และหากมันโจมตีมอนสเตอร์ในท่าป้องกัน, สร้างความเสียหายจากการต่อสู้แบบทะลุทะลวงให้ฝ่ายตรงข้าม หากเอฟเฟกต์นี้ถูกใช้กับมอนสเตอร์นั้น, ทำลายมอนสเตอร์ประเภทเครื่องจักรทั้งหมดที่คุณควบคุมในช่วง End Phase ของเทิร์นนี้ มอนสเตอร์อื่นไม่สามารถโจมตีในเทิร์นที่คุณเปิดใช้งานการ์ดนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(g:GetFirst():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.activate(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local tc=Duel.GetFirstTarget()
	if tc:IsFaceup() and tc:IsRelateToEffect(e) then
		--ATK becomes doubled its original ATK
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_SET_ATTACK_FINAL)
		e1:SetValue(tc:GetBaseAttack()*2)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [214] Soul of Fire (ID: 95026693)
**Lua File:** `script\official\c95026693.lua`

**Description:**
> ฝ่ายตรงข้ามจั่วการ์ด 1 ใบ เลือกมอนสเตอร์ประเภทไฟ 1 ตัวจากเด็คของคุณและนำออกนอกเกม สร้างความเสียหายให้ฝ่ายตรงข้ามเท่ากับครึ่งหนึ่งของ ATK ของมอนสเตอร์ที่ถูกนำออกนอกเกม หากคุณเปิดใช้งานการ์ดใบนี้ คุณไม่สามารถประกาศโจมตีในเทิร์นนี้ได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if chk==0 then return Duel.GetActivityCount(tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_OATH)
	e1:SetTargetRange(1,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.filter(c)
	return c:IsRace(RACE_PYRO) and c:IsAbleToRemove()
end
```

---

## [215] Spellbinding Circle (ID: 18807108)
**Lua File:** `script\official\c18807108.lua`

**Description:**
> เปิดใช้งานการ์ดใบนี้โดยเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; มันไม่สามารถโจมตีหรือเปลี่ยนตำแหน่งการต่อสู้ได้ เมื่อมอนสเตอร์นั้นถูกทำลาย ให้ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetRange(LOCATION_SZONE)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetTarget(aux.PersistentTargetFilter)
	c:RegisterEffect(e1)
	local e2=e1:Clone()
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	c:RegisterEffect(e2)
	--Destroy
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_CONTINUOUS+EFFECT_TYPE_FIELD)
	e3:SetRange(LOCATION_SZONE)
	e3:SetCode(EVENT_LEAVE_FIELD)
	e3:SetCondition(s.descon)
	e3:SetOperation(s.desop)
	c:RegisterEffect(e3)
end
```

---

## [216] Sphinx Teleia (ID: 51402177)
**Lua File:** `script\official\c51402177.lua`

**Description:**
> คุณสามารถจ่าย Life Points 500 แต้มเพื่ออัญเชิญแบบพิเศษการ์ดใบนี้เมื่อ "Pyramid of Light" อยู่บนฟิลด์ การ์ดใบนี้ไม่สามารถโจมตีได้ในเทิร์นที่มันถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษ การ์ดใบนี้ไม่สามารถถูกอัญเชิญแบบพิเศษจากสุสานได้ ถ้าการ์ดใบนี้ทำลายมอนสเตอร์ในตำแหน่งป้องกันด้วยผลของการต่อสู้ ให้สร้างความเสียหายให้กับ Life Points ของฝ่ายตรงข้ามเท่ากับครึ่งหนึ่งของ DEF ของมอนสเตอร์ที่ถูกทำลาย

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [217] Spirit with Eyes of Blue (ID: 42097666)
**Lua File:** `script\official\c42097666.lua`

**Description:**
> มอนสเตอร์ประเภทมังกรหรือนักเวทมนตร์เลเวล 4 หรือต่ำกว่า 1 ตัว
คุณไม่สามารถอัญเชิญแบบพิเศษได้ ยกเว้นมอนสเตอร์ประเภทมังกร คุณสามารถใช้แต่ละเอฟเฟกต์ต่อไปนี้ของ "Spirit with Eyes of Blue" ได้เทิร์นละครั้งเท่านั้น หากการ์ดใบนี้ถูกอัญเชิญลิงก์: คุณสามารถนำ "Mausoleum of White" 1 ใบจากเด็คของคุณ และเพิ่มมันขึ้นมือหรือส่งมันลงสุสานก็ได้ คุณสามารถสังเวยการ์ดใบนี้; อัญเชิญแบบพิเศษมอนสเตอร์ "Blue-Eyes" 1 ตัวจากมือหรือสุสานของคุณ แต่หากคุณอัญเชิญแบบพิเศษเอฟเฟกต์มอนสเตอร์จากสุสานของคุณด้วยเอฟเฟกต์นี้ มอนสเตอร์นั้นไม่สามารถโจมตีได้และเอฟเฟกต์ของมันถูกยกเลิก

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		sc:RegisterEffect(e1)
```

---

## [218] Successor Soul (ID: 69145169)
**Lua File:** `script\official\c69145169.lua`

**Description:**
> สังเวยมอนสเตอร์เอฟเฟกต์ 1 ตัว จากนั้นเลือกมอนสเตอร์เอฟเฟกต์ 1 ตัวที่คู่ต่อสู้ควบคุม; ส่งมันลงสุสาน จากนั้นอัญเชิญแบบพิเศษมอนสเตอร์ปกติ Level 7 ขึ้นไป 1 ตัวจากมือหรือเด็คของคุณ คุณสามารถเปิดใช้ "Successor Soul" ได้เทิร์นละ 1 ใบเท่านั้น คุณสามารถโจมตีด้วยมอนสเตอร์ได้เพียง 1 ตัวในเทิร์นที่คุณเปิดใช้การ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Prevents other attacks
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_OATH)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetCondition(s.atkcon)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_BATTLE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,1),nil)
end
```

---

## [219] Super Strident Blaze (ID: 81193865)
**Lua File:** `script\official\c81193865.lua`

**Description:**
> สวมใส่ให้กับมอนสเตอร์ฟิวชันประเภทเครื่องจักรเท่านั้น ฝ่ายตรงข้ามไม่สามารถเปิดใช้งานการ์ดหรือเอฟเฟกต์ในช่วงแบตเทิลเฟสของคุณ เมื่อสิ้นสุดแดเมจสเต็ป หากมอนสเตอร์ที่สวมใส่โจมตีมอนสเตอร์ของฝ่ายตรงข้าม: คุณสามารถนำมอนสเตอร์ 'Cyber Dragon' 1 ตัวจากสุสานของคุณออกนอกเกม; มอนสเตอร์ที่สวมใส่สามารถโจมตีมอนสเตอร์ของฝ่ายตรงข้ามอีกครั้งติดต่อกัน มอนสเตอร์อื่นของคุณไม่สามารถโจมตีในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(c:GetEquipTarget():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_REMOVE)
	local g=Duel.SelectMatchingCard(tp,s.cafilter,tp,LOCATION_GRAVE,0,1,1,nil)
	Duel.Remove(g,POS_FACEUP,REASON_COST)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.catg(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return e:GetHandler():GetEquipTarget():CanChainAttack(0,true) end
end
function s.caop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local ec=c:GetEquipTarget()
	if not ec:IsRelateToBattle() then return end
	Duel.ChainAttack()
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_DIRECT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e1:SetReset(RESET_EVENT|RESETS_STANDARD|RESET_PHASE|PHASE_BATTLE|PHASE_DAMAGE_CAL)
	ec:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```
### Effect 2 (EFFECT_CANNOT_DIRECT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_DIRECT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e1:SetReset(RESET_EVENT|RESETS_STANDARD|RESET_PHASE|PHASE_BATTLE|PHASE_DAMAGE_CAL)
	ec:RegisterEffect(e1)
```

---

## [220] Superdreadnought Rail Cannon Juggernaut Liebe (ID: 26096328)
**Lua File:** `script\official\c26096328.lua`

**Description:**
> มอนสเตอร์เลเวล 11 จำนวน 3 ตัว
เทิร์นละครั้ง คุณสามารถอัญเชิญเอ็กซ์ซี "Superdreadnought Rail Cannon Juggernaut Liebe" โดยใช้มอนสเตอร์เอ็กซ์ซีเครื่องจักรแรงค์ 10 ที่คุณควบคุม 1 ตัวเป็นวัตถุดิบได้ด้วย (ย้ายวัตถุดิบของมันไปที่การ์ดใบนี้) เทิร์นละครั้ง: คุณสามารถถอดวัตถุดิบ 1 ตัวจากการ์ดใบนี้; การ์ดใบนี้ได้รับ ATK/DEF 2000 และในเทิร์นที่เหลือ คุณสามารถประกาศโจมตีด้วยการ์ดใบนี้เท่านั้น ในแต่ละเฟสแบทเทิล การ์ดใบนี้สามารถโจมตีมอนสเตอร์ได้มากถึงจำนวนวัตถุดิบเอ็กซ์ซีที่มันมี +1 ครั้ง

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
c:RegisterEffect(e1)
		local e2=e1:Clone()
		e2:SetCode(EFFECT_UPDATE_DEFENSE)
		c:RegisterEffect(e2)
	end
	local e0=Effect.CreateEffect(e:GetHandler())
	e0:SetType(EFFECT_TYPE_FIELD)
	e0:SetCode(EFFECT_CANNOT_ATTACK)
	e0:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e0:SetTargetRange(LOCATION_MZONE,0)
	e0:SetTarget(s.ftarget)
	e0:SetLabel(c:GetFieldID())
	e0:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e0,tp)
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,2),nil)
end
```

---

## [221] Swords of Burning Light (ID: 93087299)
**Lua File:** `script\official\c93087299.lua`

**Description:**
> ขณะที่คุณไม่มีมอนสเตอร์ควบคุม, มอนสเตอร์ที่ฝ่ายตรงข้ามควบคุมไม่สามารถประกาศโจมตีได้ หากคุณมีมอนสเตอร์ควบคุม, หรือหากฝ่ายตรงข้ามมีการ์ดในมือ 5 ใบขึ้นไป, ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(0,LOCATION_MZONE)
	e2:SetCondition(s.atcon)
	c:RegisterEffect(e2)
	--
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_SINGLE)
	e3:SetCode(EFFECT_SELF_DESTROY)
	e3:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
```

---

## [222] Swords of Revealing Light (ID: 72302403)
**Lua File:** `script\official\c72302403.lua`

**Description:**
> หลังจากเปิดใช้การ์ดใบนี้ มันจะยังคงอยู่บนฟิลด์ แต่คุณต้องทำลายมันในระหว่าง End Phase ของเทิร์นที่ 3 ของฝ่ายตรงข้าม เมื่อการ์ดใบนี้ถูกเปิดใช้: หากฝ่ายตรงข้ามควบคุมมอนสเตอร์คว่ำหน้า ให้หงายมอนสเตอร์ทั้งหมดที่ฝ่ายตรงข้ามควบคุมขึ้น ในขณะที่การ์ดใบนี้หงายหน้าอยู่บนฟิลด์ มอนสเตอร์ของฝ่ายตรงข้ามไม่สามารถประกาศโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetCategory(CATEGORY_POSITION)
	e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	e1:SetTarget(s.target)
	e1:SetOperation(s.activate)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(0,LOCATION_MZONE)
	c:RegisterEffect(e2)
	--remain field
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_SINGLE)
	e3:SetCode(EFFECT_REMAIN_FIELD)
	c:RegisterEffect(e3)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return e:IsHasType(EFFECT_TYPE_ACTIVATE) end
	local c=e:GetHandler()
	c:SetTurnCounter(0)
	local sg=Duel.GetMatchingGroup(Card.IsFacedown,tp,0,LOCATION_MZONE,nil)
	Duel.SetOperationInfo(0,CATEGORY_POSITION,sg,#sg,0,0)
	--destroy
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e1:SetCode(EVENT_PHASE+PHASE_END)
	e1:SetCountLimit(1)
	e1:SetRange(LOCATION_SZONE)
	e1:SetCondition(s.descon)
	e1:SetOperation(s.desop)
	e1:SetReset(RESETS_STANDARD_PHASE_END|RESET_OPPO_TURN,3)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return e:IsHasType(EFFECT_TYPE_ACTIVATE) end
	local c=e:GetHandler()
	c:SetTurnCounter(0)
	local sg=Duel.GetMatchingGroup(Card.IsFacedown,tp,0,LOCATION_MZONE,nil)
	Duel.SetOperationInfo(0,CATEGORY_POSITION,sg,#sg,0,0)
	--destroy
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e1:SetCode(EVENT_PHASE+PHASE_END)
	e1:SetCountLimit(1)
	e1:SetRange(LOCATION_SZONE)
	e1:SetCondition(s.descon)
	e1:SetOperation(s.desop)
	e1:SetReset(RESETS_STANDARD_PHASE_END|RESET_OPPO_TURN,3)
	c:RegisterEffect(e1)
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_SINGLE)
	e3:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_UNCOPYABLE+EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_SET_AVAILABLE)
	e3:SetCode(1082946)
	e3:SetLabelObject(e1)
	e3:SetOwnerPlayer(tp)
	e3:SetOperation(s.reset)
	e3:SetReset(RESETS_STANDARD_PHASE_END|RESET_OPPO_TURN,3)
	c:RegisterEffect(e3)
end
```
```lua
function s.descon(e,tp,eg,ep,ev,re,r,rp)
	return Duel.IsTurnPlayer(1-tp)
end
```

---

## [223] Synchro Zone (ID: 60306277)
**Lua File:** `script\official\c60306277.lua`

**Description:**
> ผู้เล่นทั้งสองฝ่ายไม่สามารถประกาศโจมตีได้ ยกเว้นด้วยมอนสเตอร์ซิงโคร คุณสามารถใช้เอฟเฟกต์แต่ละอย่างต่อไปนี้ของ "Synchro Zone" ได้เทิร์นละครั้งเท่านั้น หากมอนสเตอร์ซิงโครที่ไม่ใช่จูนเนอร์ถูกส่งไปยังสุสานของคุณ (ยกเว้นในช่วง Damage Step): คุณสามารถเลือกเป้าหมายมอนสเตอร์เหล่านั้น 1 ตัว อัญเชิญแบบพิเศษมัน และหากทำเช่นนั้น มันจะถือว่าเป็นจูนเนอร์ ในช่วง Main Phase ของฝ่ายตรงข้าม: คุณสามารถส่งการ์ดหงายหน้านี้ลงสุสาน ทันทีหลังจากเอฟเฟกต์นี้แก้ไข อัญเชิญซิงโครโดยใช้มอนสเตอร์ที่คุณควบคุมเป็นวัตถุดิบ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--Non-Syncho Monsters cannot declare attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(function(_,c) return not c:IsType(TYPE_SYNCHRO) end)
	c:RegisterEffect(e2)
	--Special Summon 1 non-Tuner monster that was sent to the GY
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,0))
	e3:SetCategory(CATEGORY_SPECIAL_SUMMON)
	e3:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
	e3:SetProperty(EFFECT_FLAG_CARD_TARGET+EFFECT_FLAG_DELAY)
	e3:SetCode(EVENT_CUSTOM+id)
	e3:SetRange(LOCATION_SZONE)
	e3:SetCountLimit(1,id)
	e3:SetTarget(s.sptg)
	e3:SetOperation(s.spop)
	c:RegisterEffect(e3)
	local g=Group.CreateGroup()
	g:KeepAlive()
	e3:SetLabelObject(g)
	--Register cards sent to the GY
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
	e4:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e4:SetCode(EVENT_TO_GRAVE)
	e4:SetRange(LOCATION_SZONE)
	e4:SetLabelObject(e3)
	e4:SetOperation(s.regop)
	c:RegisterEffect(e4)
	--Synchro Summon during the opponent's Main Phase
	local e5=Effect.CreateEffect(c)
	e5:SetDescription(aux.Stringid(id,1))
	e5:SetCategory(CATEGORY_SPECIAL_SUMMON)
	e5:SetType(EFFECT_TYPE_QUICK_O)
	e5:SetCode(EVENT_FREE_CHAIN)
	e5:SetRange(LOCATION_SZONE)
	e5:SetHintTiming(0,TIMING_MAIN_END)
	e3:SetCountLimit(1,{id,1})
	e5:SetCondition(function(_,tp) return Duel.IsMainPhase() and Duel.IsTurnPlayer(1-tp) end)
	e5:SetCost(s.synchcost)
	e5:SetTarget(s.synchtg)
	e5:SetOperation(s.synchop)
	c:RegisterEffect(e5)
end
function s.cfilter(c,e,tp)
	return c:IsMonster() and c:IsType(TYPE_SYNCHRO) and not c:IsType(TYPE_TUNER)
		and c:IsControler(tp) and c:IsCanBeEffectTarget(e) and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end
function s.regop(e,tp,eg,ep,ev,re,r,rp)
	local tg=eg:Filter(s.cfilter,nil,e,tp)
	if #tg>0 then
		for tc in tg:Iter() do
			tc:RegisterFlagEffect(id,RESET_CHAIN,0,1)
		end
		local g=e:GetLabelObject():GetLabelObject()
		if Duel.GetCurrentChain()==0 then g:Clear() end
		g:Merge(tg)
		g:Remove(function(c) return c:GetFlagEffect(id)==0 end,nil)
		e:GetLabelObject():SetLabelObject(g)
		Duel.RaiseSingleEvent(e:GetHandler(),EVENT_CUSTOM+id,e,0,tp,tp,0)
	end
end
function s.sptg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	local g=e:GetLabelObject():Filter(s.cfilter,nil,e,tp)
	if chkc then return g:IsContains(chkc) and s.cfilter(chkc,e,tp) end
	if chk==0 then return Duel.GetLocationCount(tp,LOCATION_MZONE)>0 and #g>0 and Duel.GetCurrentPhase()~=PHASE_DAMAGE end
	local tc=nil
	if #g==1 then
		tc=g:GetFirst()
	else
		Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
		tc=g:Select(tp,1,1,nil):GetFirst()
	end
	Duel.SetTargetCard(tc)
	Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,tc,1,tp,0)
end
function s.spop(e,tp,eg,ep,ev,re,r,rp)
	if Duel.GetLocationCount(tp,LOCATION_MZONE)<=0 then return end
	local tc=Duel.GetFirstTarget()
	if tc:IsRelateToEffect(e) and Duel.SpecialSummonStep(tc,0,tp,tp,false,false,POS_FACEUP) then
		--Treated as a Tuner
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_IGNORE_IMMUNE)
		e1:SetCode(EFFECT_ADD_TYPE)
		e1:SetValue(TYPE_TUNER)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1)
```

---

## [224] Tellarknight Altairan (ID: 42822433)
**Lua File:** `script\official\c42822433.lua`

**Description:**
> (การ์ดใบนี้จะถูกมองว่าเป็นไพ่ "Constellar" เสมอ)
หากการ์ดใบนี้ถูกอัญเชิญ: คุณสามารถเลือกเป้าหมายการ์ดบนฟิลด์ได้สูงสุดตามจำนวนมอนสเตอร์ Xyz ที่มีธาตุแสงและความมืดที่คุณควบคุม; ทำลายพวกมัน หากมอนสเตอร์ "tellarknight" หรือ "Constellar" ถูกอัญเชิญแบบพิเศษมายังฟิลด์ของคุณ ยกเว้น "Tellarknight Altairan" (ยกเว้นในช่วง Damage Step): คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้จากสุสานของคุณ และคุณไม่สามารถประกาศโจมตีในเทิร์นที่เหลือได้ ยกเว้นด้วยมอนสเตอร์ Xyz คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Tellarknight Altairan" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(function(e,c) return not c:IsType(TYPE_XYZ) end)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
```

---

## [225] Teva (ID: 16469012)
**Lua File:** `script\official\c16469012.lua`

**Description:**
> เมื่อการ์ดใบนี้ถูกอัญเชิญแบบสังเวยสำเร็จ ฝ่ายตรงข้ามไม่สามารถประกาศโจมตีในเทิร์นถัดไปของเขา/เธอ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
return e:GetHandler():IsTributeSummoned()
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTargetRange(0,LOCATION_MZONE)
	e1:SetReset(RESET_PHASE|PHASE_END,2)
	Duel.RegisterEffect(e1,tp)
```

---

## [226] The Dark Door (ID: 30606547)
**Lua File:** `script\official\c30606547.lua`

**Description:**
> มอนสเตอร์เพียง 1 ตัวเท่านั้นที่สามารถโจมตีได้ในแต่ละ Battle Phase

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetCondition(s.atkcon)
	e2:SetTarget(s.atktg)
	c:RegisterEffect(e2)
	--check
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [227] The Ice-Bound God (ID: 22748199)
**Lua File:** `script\official\c22748199.lua`

**Description:**
> หากมีมอนสเตอร์ธาตุน้ำ 2 ตัวขึ้นไปบนฟิลด์: เลือกเป้าหมายมอนสเตอร์หงายหน้า 1 ตัวที่ฝ่ายตรงข้ามควบคุม; มอนสเตอร์หงายหน้านั้นไม่สามารถโจมตี และเอฟเฟกต์ของมันถูกยกเลิก หากมอนสเตอร์ธาตุน้ำเลเวล 5 ขึ้นไปถูกอัญเชิญแบบปกติหรือแบบพิเศษมายังฟิลด์ของคุณ ขณะที่การ์ดนี้อยู่ในสุสานของคุณ (ยกเว้นในช่วงแดเมจสเต็ป): คุณสามารถเซ็ตการ์ดนี้ แต่จะถูกนำออกนอกเกมเมื่อออกจากฟิลด์ คุณสามารถใช้เอฟเฟกต์นี้ของ "The Ice-Bound God" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
tc:RegisterEffect(e2)
		--Cannot attack
		local e3=Effect.CreateEffect(c)
		e3:SetDescription(3206)
		e3:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e3:SetType(EFFECT_TYPE_SINGLE)
		e3:SetCode(EFFECT_CANNOT_ATTACK)
		e3:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e3)
	end
end
function s.cfilter(c,tp)
	return c:IsLevelAbove(5) and c:IsAttribute(ATTRIBUTE_WATER) and c:IsControler(tp)
end
function s.setcon(e,tp,eg,ep,ev,re,r,rp)
	return eg:IsExists(s.cfilter,1,nil,tp)
end
```

---

## [228] The League of Uniform Nomenclature Strikes (ID: 29265962)
**Lua File:** `script\official\c29265962.lua`

**Description:**
> เลือกเป้าหมายมอนสเตอร์หงายหน้า 1 ตัวที่คุณควบคุม; สวมมอนสเตอร์ 2 ตัวที่มีชื่อเดิมเดียวกับเป้าหมายนั้นจากมือ เด็ค และ/หรือสุสานของคุณ ให้กับเป้าหมายนั้นในฐานะเวทมนตร์สวมใส่ และถ้าทำเช่นนั้น มันไม่สามารถโจมตีหรือถูกทำลายในการต่อสู้ขณะที่สวมใส่ด้วยการ์ด 2 ใบนั้น คุณไม่สามารถอัญเชิญแบบพิเศษในเทิร์นที่เหลือหลังจากที่การ์ดนี้เรโซลฟ์ได้ ยกเว้นมอนสเตอร์ที่มีประเภทเดิมเดียวกับมอนสเตอร์ที่ถูกเลือกเป้าหมาย คุณสามารถเปิดใช้งาน "The League of Uniform Nomenclature Strikes" ได้เทิร์นละ 1 ใบเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
			e2:SetDescription(aux.Stringid(id,1))
			e2:SetType(EFFECT_TYPE_SINGLE)
			e2:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
			e2:SetCode(EFFECT_CANNOT_ATTACK)
			e2:SetCondition(function(e) return e:GetHandler():GetEquipGroup():IsExists(function(ec) return ec:GetFlagEffectLabel(id)==fid end,2,nil) end)
			e2:SetReset(RESET_EVENT|RESETS_STANDARD)
			tc:RegisterEffect(e2)
```

---

## [229] The Legendary Fisherman III (ID: 44968687)
**Lua File:** `script\official\c44968687.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือ) โดยการสังเวย "The Legendary Fisherman" 1 ตัว เมื่อการ์ดใบนี้ถูกอัญเชิญแบบพิเศษ: คุณสามารถนำมอนสเตอร์ทั้งหมดที่ฝ่ายตรงข้ามควบคุมออกนอกเกม และการ์ดใบนี้ไม่สามารถโจมตีในเทิร์นนี้ ไม่สามารถถูกทำลายโดยการต่อสู้หรือเอฟเฟกต์การ์ด และไม่ได้รับผลกระทบจากเอฟเฟกต์เวทมนตร์/กับดัก ครั้งเดียวต่อเทิร์น: คุณสามารถส่งการ์ดที่ถูกนำออกนอกเกมของฝ่ายตรงข้ามกลับลงสุสานมากที่สุดเท่าที่เป็นไปได้ และหากทำเช่นนั้น ความเสียหายจากการต่อสู้หรือเอฟเฟกต์ครั้งแรกที่ฝ่ายตรงข้ามได้รับในเทิร์นนี้จะเพิ่มเป็นสองเท่า

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_CLIENT_HINT)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		e:GetHandler():RegisterEffect(e1)
```

---

## [230] The Regulation of Tribe (ID: 296499)
**Lua File:** `script\official\c296499.lua`

**Description:**
> เปิดใช้งานการ์ดใบนี้โดยประกาศประเภทมอนสเตอร์ 1 ชนิด มอนสเตอร์ที่มีประเภทที่ประกาศไว้ไม่สามารถประกาศโจมตีได้ เทิร์นละครั้ง ใน Standby Phase ของคุณ สังเวยมอนสเตอร์ 1 ตัวหรือทำลายการ์ดใบนี้

*ข้อความข้างต้นไม่เป็นทางการและอธิบายการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetCode(EVENT_FREE_CHAIN)
	e1:SetTarget(s.target)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.atktarget)
	c:RegisterEffect(e2)
	e2:SetLabelObject(e1)
	--maintain
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [231] The True Sun God (ID: 11587414)
**Lua File:** `script\official\c11587414.lua`

**Description:**
> เมื่อการ์ดใบนี้ถูกเปิดใช้งาน: เพิ่ม "The Winged Dragon of Ra" 1 ใบ หรือการ์ด 1 ใบที่กล่าวถึงการ์ดนั้นจากเด็คของคุณขึ้นมือ ยกเว้น "The True Sun God" มอนสเตอร์ ยกเว้น "The Winged Dragon of Ra" ไม่สามารถโจมตีในเทิร์นที่พวกมันถูกอัญเชิญแบบพิเศษ เทิร์นละครั้ง ใน Main Phase ของคุณ: คุณสามารถส่งการ์ดใบนี้จากฟิลด์ หรือ "The Winged Dragon of Ra - Immortal Phoenix" 1 ใบจากเด็คของคุณ ลงสุสาน จากนั้นส่ง "The Winged Dragon of Ra" 1 ใบจาก Monster Zone ของคุณลงสุสาน คุณสามารถเปิดใช้งาน "The True Sun God" ได้เพียงเทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(function(_,c) return not c:IsCode(CARD_RA) and c:IsStatus(STATUS_SPSUMMON_TURN) end)
	c:RegisterEffect(e2)
```

---

## [232] The Unhappy Girl (ID: 27618634)
**Lua File:** `script\official\c27618634.lua`

**Description:**
> ขณะที่การ์ดนี้อยู่ในตำแหน่งโจมตีหงายหน้าบนฟิลด์ การ์ดนี้จะไม่ถูกทำลายอันเป็นผลมาจากการต่อสู้ (ใช้คำนวณดาเมจตามปกติ) มอนสเตอร์ที่ต่อสู้กับการ์ดนี้ไม่สามารถเปลี่ยนตำแหน่งการต่อสู้ได้ ยกเว้นด้วยเอฟเฟกต์การ์ด หรือโจมตีในขณะที่การ์ดนี้อยู่ในตำแหน่งโจมตีหงายหน้าบนฟิลด์

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
c:RegisterEffect(e2)
	local g=Group.CreateGroup()
	g:KeepAlive()
	e2:SetLabelObject(g)
	--atk limit
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_ATTACK)
	e3:SetRange(LOCATION_MZONE)
	e3:SetTargetRange(0,LOCATION_MZONE)
	e3:SetCondition(s.atlcon)
	e3:SetTarget(s.atltg)
	e3:SetLabelObject(g)
	c:RegisterEffect(e3)
	local e4=e3:Clone()
	e4:SetCode(EFFECT_CANNOT_CHANGE_POSITION)
	e4:SetLabelObject(g)
	c:RegisterEffect(e4)
	--
```

---

## [233] The Winged Dragon of Ra - Sphere Mode (ID: 10000080)
**Lua File:** `script\official\c10000080.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบพิเศษได้ ต้องสังเวย 3 ตัวจากทั้งสองฝั่งฟิลด์เพื่ออัญเชิญแบบปกติขึ้นฟิลด์ฝั่งนั้น (ไม่สามารถเซ็ตแบบปกติได้) จากนั้นเปลี่ยนการควบคุมให้เป็นเจ้าของการ์ดใบนี้ในช่วง End Phase ของเทิร์นถัดไป ไม่สามารถโจมตีได้ ฝ่ายตรงข้ามไม่สามารถเลือกการ์ดใบนี้เป็นเป้าหมายด้วยเอฟเฟกต์การ์ดได้ และมอนสเตอร์ที่ฝ่ายตรงข้ามควบคุมไม่สามารถเลือกการ์ดใบนี้เป็นเป้าหมายโจมตีได้ คุณสามารถสังเวยการ์ดใบนี้; อัญเชิญแบบพิเศษ "The Winged Dragon of Ra" 1 ใบจากมือหรือเด็ค โดยไม่สนเงื่อนไขการอัญเชิญ และถ้าทำเช่นนั้น ATK/DEF ของมันจะเป็น 4000 * ข้อความข้างต้นไม่เป็นทางการและอธิบายการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e5:SetOperation(s.retreg)
	c:RegisterEffect(e5)
	--attack limit
	local e6=Effect.CreateEffect(c)
	e6:SetType(EFFECT_TYPE_SINGLE)
	e6:SetCode(EFFECT_CANNOT_ATTACK)
	c:RegisterEffect(e6)
	--cannot be target
	local e7=Effect.CreateEffect(c)
	e7:SetType(EFFECT_TYPE_SINGLE)
	e7:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
	e7:SetCode(EFFECT_CANNOT_BE_BATTLE_TARGET)
	e7:SetRange(LOCATION_MZONE)
	e7:SetValue(aux.imval2)
	c:RegisterEffect(e7)
```

---

## [234] Thousand-Eyes Restrict (ID: 63519819)
**Lua File:** `script\official\c63519819.lua`

**Description:**
> "Relinquished" + "Thousand-Eyes Idol"
มอนสเตอร์อื่นบนฟิลด์ไม่สามารถเปลี่ยนตำแหน่งการต่อสู้หรือโจมตี 1 เทิร์นละครั้ง: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; ติดตั้งเป้าหมายนั้นกับการ์ดใบนี้ (สูงสุด 1 ใบ) ATK/DEF ของการ์ดใบนี้กลายเป็นค่าเท่ากับมอนสเตอร์ที่ติดตั้งนั้น หากการ์ดใบนี้จะถูกทำลายด้วยการต่อสู้ ให้ทำลายมอนสเตอร์ที่ติดตั้งนั้นแทน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetRange(LOCATION_MZONE)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.antarget)
	c:RegisterEffect(e2)
```
Associated helper functions:
```lua
function s.antarget(e,c)
	return c~=e:GetHandler()
end
```

---

## [235] Threatening Roar (ID: 36361633)
**Lua File:** `script\official\c36361633.lua`

**Description:**
> คู่ต่อสู้ของคุณไม่สามารถประกาศโจมตีในเทิร์นนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
function s.activate(e,tp,eg,ep,ev,re,r,rp)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET)
	e1:SetReset(RESET_PHASE|PHASE_END)
	e1:SetTargetRange(0,1)
	Duel.RegisterEffect(e1,tp)
```

---

## [236] Thunder Unicorn (ID: 77506119)
**Lua File:** `script\official\c77506119.lua`

**Description:**
> จูนเนอร์ประเภทสัตว์ร้าย 1 ตัว + มอนสเตอร์ที่ไม่ใช่จูนเนอร์ 1 ตัวขึ้นไป
เทิร์นละครั้ง ในเมนเฟสของคุณ คุณสามารถเลือกมอนสเตอร์ที่หงายหน้าอยู่บนฟิลด์ที่ฝ่ายตรงข้ามควบคุม 1 ตัว มอนสเตอร์นั้นสูญเสีย ATK 500 ต่อมอนสเตอร์ 1 ตัวที่คุณควบคุม จนถึงเอนด์เฟส ในเทิร์นที่เปิดใช้งานเอฟเฟกต์นี้ มอนสเตอร์อื่นไม่สามารถโจมตีได้ ยกเว้นการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
	end
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.atlimit)
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
end
function s.atlimit(e,c)
	return c~=e:GetOwner()
```

---

## [237] Thunderforce Attack (ID: 42469671)
**Lua File:** `script\official\c42469671.lua`

**Description:**
> การเปิดใช้งานและเอฟเฟกต์ของการ์ดใบนี้ไม่สามารถถูกยกเลิกได้
หากคุณควบคุมมอนสเตอร์ที่มีชื่อเดิมว่า "Slifer the Sky Dragon": ทำลายมอนสเตอร์หงายหน้าที่คู่ต่อสู้ควบคุมให้มากที่สุดเท่าที่เป็นไปได้, จากนั้น หากคุณเปิดใช้งานการ์ดใบนี้ในช่วงเมนเฟสของคุณ, คุณสามารถใช้เอฟเฟกต์ต่อไปนี้
● จั่วการ์ดเท่ากับจำนวนมอนสเตอร์ที่ถูกทำลายด้วยเอฟเฟกต์นี้และถูกส่งไปยังสุสานของคู่ต่อสู้, และคุณสามารถโจมตีด้วยมอนสเตอร์เพียง 1 ตัวในเทิร์นนี้
คุณสามารถเปิดใช้งาน "Thunderforce Attack" ได้เทิร์นละ 1 ใบเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
Duel.BreakEffect()
		Duel.Draw(tp,ct,REASON_EFFECT)
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
		e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetCondition(s.atkcon)
		e1:SetTarget(s.atktg)
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
		local e2=Effect.CreateEffect(e:GetHandler())
		e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [238] Toon Ancient Gear Golem (ID: 7171149)
**Lua File:** `script\official\c7171149.lua`

**Description:**
> ไม่สามารถโจมตีในเทิร์นที่อัญเชิญ ในขณะที่คุณควบคุม 'Toon World' และคู่ต่อสู้ของคุณไม่มีมอนสเตอร์ Toon, การ์ดใบนี้สามารถโจมตีคู่ต่อสู้ของคุณโดยตรง หากการ์ดใบนี้โจมตีมอนสเตอร์ในตำแหน่งป้องกัน, สร้างความเสียหายทะลุทะลวงให้คู่ต่อสู้ของคุณ หากการ์ดใบนี้โจมตี, คู่ต่อสู้ของคุณไม่สามารถเปิดใช้งานการ์ดเวทมนตร์/กับดักใดๆ จนกว่าจะสิ้นสุด Damage Step

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
c:RegisterEffect(e6)
end
s.listed_names={15259703}
function s.atklimit(e,tp,eg,ep,ev,re,r,rp)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
end
function s.cfilter1(c)
	return c:IsFaceup() and c:IsCode(15259703)
end
function s.cfilter2(c)
	return c:IsFaceup() and c:IsType(TYPE_TOON)
end
function s.dircon(e)
	local tp=e:GetHandlerPlayer()
```

---

## [239] Toon Black Luster Soldier (ID: 28711704)
**Lua File:** `script\official\c28711704.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือ) ก่อนโดยสังเวยมอนสเตอร์ Toon จากมือหรือสนามของคุณที่มีเลเวลรวมกัน 8 หรือมากกว่า ขณะที่คุณควบคุม "Toon World" และคู่ต่อสู้ของคุณไม่มีมอนสเตอร์ Toon การ์ดใบนี้สามารถโจมตีโดยตรงได้ เทิร์นละครั้ง หากคุณควบคุม "Toon World": คุณสามารถเลือกเป้าหมายการ์ด 1 ใบบนสนาม; นำมันออกนอกเกม การ์ดใบนี้ไม่สามารถโจมตีในเทิร์นที่เปิดใช้งานเอฟเฟกต์นี้

* ข้อความข้างต้นไม่เป็นทางการและอธิบายฟังก์ชันการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetDescription(3206)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_OATH+EFFECT_FLAG_CLIENT_HINT)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	c:RegisterEffect(e1,true)
end
function s.rmtg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	if chkc then return chkc:IsOnField() and chkc:IsAbleToRemove() end
```

---

## [240] Toon Cannon Soldier (ID: 79875176)
**Lua File:** `script\official\c79875176.lua`

**Description:**
> ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญ หาก "Toon World" บนฟิลด์ถูกทำลาย ให้ทำลายการ์ดใบนี้ ขณะที่คุณควบคุม "Toon World" และฝ่ายตรงข้ามไม่มีการ์ด Toon โทนควบคุม การ์ดใบนี้สามารถโจมตีตรงได้ คุณสามารถสังเวยมอนสเตอร์ 1 ตัว; สร้างความเสียหาย 500 แต้มให้ฝ่ายตรงข้าม

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [241] Toon Cyber Dragon (ID: 83629030)
**Lua File:** `script\official\c83629030.lua`

**Description:**
> หากคู่ต่อสู้ของคุณควบคุมมอนสเตอร์และคุณไม่ควบคุมมอนสเตอร์ใด ๆ คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้ (จากมือคุณ) ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญ ในขณะที่คุณควบคุม "Toon World" และคู่ต่อสู้ของคุณไม่ควบคุมมอนสเตอร์ Toon การ์ดใบนี้สามารถโจมตีคู่ต่อสู้ของคุณโดยตรงได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [242] Toon Dark Magician Girl (ID: 90960358)
**Lua File:** `script\official\c90960358.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/ถูกวางได้ ต้องอัญเชิญแบบพิเศษ (จากมือของคุณ) ก่อนโดยการสังเวยมอนสเตอร์ 1 ตัว ขณะที่คุณควบคุม "Toon World" ถ้า "Toon World" บนฟิลด์ถูกทำลาย ทำลายการ์ดใบนี้ สามารถโจมตีคู่ต่อสู้ของคุณโดยตรง เว้นแต่พวกเขาจะควบคุมมอนสเตอร์การ์ตูน ซึ่งในกรณีนี้การ์ดใบนี้ต้องเลือกมอนสเตอร์การ์ตูนเป็นเป้าหมายการโจมตี ได้รับ ATK 300 สำหรับ "Dark Magician" หรือ "Magician of Black Chaos" ทุกใบในสุสานของผู้เล่นคนใด

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_DIRECT_ATTACK)
```lua
e5:SetCode(EFFECT_CANNOT_SELECT_BATTLE_TARGET)
	e5:SetCondition(s.atcon)
	e5:SetValue(s.atlimit)
	c:RegisterEffect(e5)
	local e6=Effect.CreateEffect(c)
	e6:SetType(EFFECT_TYPE_SINGLE)
	e6:SetCode(EFFECT_CANNOT_DIRECT_ATTACK)
	e6:SetCondition(s.atcon)
	c:RegisterEffect(e6)
	--cannot attack
	local e7=Effect.CreateEffect(c)
	e7:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_CONTINUOUS)
	e7:SetCode(EVENT_SUMMON_SUCCESS)
	e7:SetOperation(s.atklimit)
	c:RegisterEffect(e7)
	--atkup
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [243] Toon Gemini Elf (ID: 42386471)
**Lua File:** `script\official\c42386471.lua`

**Description:**
> ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญ
หาก "Toon World" บนฟิลด์ถูกทำลาย, ทำลายการ์ดใบนี้
ขณะที่คุณควบคุม "Toon World" และคู่ต่อสู้ของคุณไม่สามารถควบคุมมอนสเตอร์ตูน, การ์ดใบนี้สามารถโจมตีคู่ต่อสู้ของคุณโดยตรง
หากการ์ดใบนี้สร้างความเสียหายจากการต่อสู้ให้คู่ต่อสู้ของคุณ: คุณสามารถทิ้งการ์ด 1 ใบจากมือของคู่ต่อสู้แบบสุ่ม

* ข้อความข้างต้นไม่เป็นทางการและอธิบายการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [244] Toon Goblin Attack Force (ID: 15270885)
**Lua File:** `script\official\c15270885.lua`

**Description:**
> ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญ หาก 'Toon World' บนฟิลด์ถูกทำลาย ให้ทำลายการ์ดใบนี้ ในขณะที่คุณควบคุม 'Toon World' และฝ่ายตรงข้ามไม่มีมอนสเตอร์ Toon ควบคุมอยู่ การ์ดใบนี้สามารถโจมตีฝ่ายตรงข้ามโดยตรงได้ หากการ์ดใบนี้โจมตี มันจะถูกเปลี่ยนเป็นโหมดป้องกันเมื่อจบเฟสต่อสู้ และตำแหน่งการต่อสู้ของมันไม่สามารถเปลี่ยนได้จนกระทั่งเอนด์เฟสของเทิร์นถัดไปของคุณ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [245] Toon Masked Sorcerer (ID: 16392422)
**Lua File:** `script\official\c16392422.lua`

**Description:**
> ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญ หาก "Toon World" บนฟิลด์ถูกทำลาย จงทำลายการ์ดใบนี้ ในขณะที่คุณควบคุม "Toon World" และฝ่ายตรงข้ามไม่มีมอนสเตอร์ Toon ใด ๆ การ์ดใบนี้สามารถโจมตีฝ่ายตรงข้ามโดยตรงได้ หากการ์ดใบนี้สร้างความเสียหายจากการต่อสู้ให้ฝ่ายตรงข้าม: จั่วการ์ด 1 ใบ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [246] Toon Mermaid (ID: 65458948)
**Lua File:** `script\official\c65458948.lua`

**Description:**
> ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือของคุณ) ก่อน ในขณะที่คุณควบคุม "Toon World" ไม่สามารถโจมตีในเทิร์นที่มันถูกอัญเชิญแบบพิเศษ คุณต้องจ่าย 500 LP เพื่อประกาศโจมตีด้วยมอนสเตอร์นี้ หาก "Toon World" บนฟิลด์ถูกทำลาย ให้ทำลายการ์ดนี้ สามารถโจมตีคู่ต่อสู้ของคุณโดยตรง เว้นแต่พวกเขาจะควบคุมมอนสเตอร์ Toon ในกรณีนี้ การ์ดนี้ต้องเลือกเป้าหมายมอนสเตอร์ Toon สำหรับการโจมตีของมัน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_DIRECT_ATTACK)
```lua
e5:SetCode(EFFECT_CANNOT_SELECT_BATTLE_TARGET)
	e5:SetCondition(s.atcon)
	e5:SetValue(s.atlimit)
	c:RegisterEffect(e5)
	local e6=Effect.CreateEffect(c)
	e6:SetType(EFFECT_TYPE_SINGLE)
	e6:SetCode(EFFECT_CANNOT_DIRECT_ATTACK)
	e6:SetCondition(s.atcon)
	c:RegisterEffect(e6)
	--cannot attack
	local e7=Effect.CreateEffect(c)
	e7:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_CONTINUOUS)
	e7:SetCode(EVENT_SPSUMMON_SUCCESS)
	e7:SetOperation(s.atklimit)
	c:RegisterEffect(e7)
	--attack cost
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [247] Toon Summoned Skull (ID: 91842653)
**Lua File:** `script\official\c91842653.lua`

**Description:**
> (การ์ดใบนี้ถือว่าเป็นการ์ด "Archfiend" เสมอ)
ไม่สามารถอัญเชิญแบบปกติ/เซ็ตได้ ต้องอัญเชิญแบบพิเศษ (จากมือ) ก่อนโดยการสังเวยมอนสเตอร์ 1 ตัว ในขณะที่คุณควบคุม "Toon World" ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญแบบพิเศษ คุณต้องจ่าย LP 500 แต้มเพื่อประกาศโจมตีด้วยมอนสเตอร์นี้ หาก "Toon World" บนฟิลด์ถูกทำลาย ให้ทำลายการ์ดใบนี้ สามารถโจมตีฝ่ายตรงข้ามโดยตรงได้ ยกเว้นว่าฝ่ายตรงข้ามจะควบคุมมอนสเตอร์ทูน (Toon) ซึ่งในกรณีนี้การ์ดใบนี้ต้องเลือกเป้าหมายมอนสเตอร์ทูนสำหรับการโจมตี

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_DIRECT_ATTACK)
```lua
e4:SetCode(EFFECT_CANNOT_SELECT_BATTLE_TARGET)
	e4:SetCondition(s.atcon)
	e4:SetValue(s.atlimit)
	c:RegisterEffect(e4)
	local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_SINGLE)
	e5:SetCode(EFFECT_CANNOT_DIRECT_ATTACK)
	e5:SetCondition(s.atcon)
	c:RegisterEffect(e5)
	--Cannot attack
	local e6=Effect.CreateEffect(c)
	e6:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_CONTINUOUS)
	e6:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
	e6:SetCode(EVENT_SPSUMMON_SUCCESS)
	e6:SetOperation(s.atklimit)
```
### Effect 2 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetReset(RESETS_STANDARD_PHASE_END)
	e:GetHandler():RegisterEffect(e1)
```

---

## [248] Topologic Bomber Dragon (ID: 5821478)
**Lua File:** `script\official\c5821478.lua`

**Description:**
> มอนสเตอร์เอฟเฟกต์ 2+ ตัว
หากมอนสเตอร์ตัวอื่นถูกอัญเชิญแบบพิเศษไปยังโซนที่มอนสเตอร์ลิงก์ชี้ไป ในขณะที่มอนสเตอร์นี้อยู่บนฟิลด์: ทำลายมอนสเตอร์ทั้งหมดใน Main Monster Zones นอกจากนี้มอนสเตอร์อื่นของคุณไม่สามารถโจมตีได้ในเทิร์นที่เหลือ หลังการคำนวณความเสียหาย หากการ์ดใบนี้โจมตีมอนสเตอร์ของฝ่ายตรงข้าม: สร้างความเสียหายแก่ฝ่ายตรงข้ามเท่ากับ ATK เดิมของมอนสเตอร์นั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
Duel.Destroy(g,REASON_EFFECT)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(e:GetHandler():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,2),nil)
end
function s.ftarget(e,c)
```

---

## [249] Toy Parade (ID: 92607427)
**Lua File:** `script\official\c92607427.lua`

**Description:**
> (การ์ดใบนี้ถือว่าเป็นการ์ด "Frightfur" เสมอ)
เลือกเป้าหมายมอนสเตอร์ธาตุมืด 1 ตัวที่คุณควบคุมที่ถูกอัญเชิญแบบพิเศษจาก Extra Deck; สำหรับเทิร์นที่เหลือนี้ คุณไม่สามารถประกาศโจมตีได้ยกเว้นกับมอนสเตอร์นั้น นอกจากนี้ ทุกครั้งที่มอนสเตอร์นั้นทำลายมอนสเตอร์ด้วยการต่อสู้และส่งมันลงสุสาน มันสามารถโจมตีอีกครั้งต่อเนื่องได้ หากคุณควบคุมมอนสเตอร์เทพปีก: คุณสามารถนำการ์ดใบนี้จากสุสานของคุณออกนอกเกม; เพิ่มมอนสเตอร์ธาตุมืดเลเวล 4 หรือต่ำกว่า 1 ตัวจากสุสานของคุณขึ้นมือ คุณสามารถใช้เอฟเฟกต์ของ "Toy Parade" ได้ 1 เอฟเฟกต์ต่อเทิร์น และใช้ได้เพียงครั้งเดียวในเทิร์นนั้น

* ข้อความข้างต้นเป็นข้อความไม่เป็นทางการและอธิบายฟังก์ชันการทำงานของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
end
	--You cannot declare attacks except with that monster
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(function(e,c) return fid==0 or c:GetRealFieldID()~=fid end)
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
	aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,3))
end
```

---

## [250] Train Connection (ID: 60879050)
**Lua File:** `script\official\c60879050.lua`

**Description:**
> ต้องสวมใส่ให้กับมอนสเตอร์เครื่องจักรดิน โดยการนำมอนสเตอร์เครื่องจักรเลเวล 10 หรือสูงกว่า 2 ตัวจากสุสานของคุณออกนอกเกม ATK ของมอนสเตอร์ที่สวมใส่จะกลายเป็นสองเท่าของ ATK เดิม และหากมันโจมตีมอนสเตอร์ในตำแหน่งป้องกัน จะสร้างความเสียหายเจาะทะลุให้กับคู่ต่อสู้ของคุณ มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e2:SetType(EFFECT_TYPE_EQUIP)
	e2:SetCode(EFFECT_PIERCE)
	c:RegisterEffect(e2)
	--cannot attack
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_ATTACK)
	e3:SetRange(LOCATION_SZONE)
	e3:SetTargetRange(LOCATION_MZONE,0)
	e3:SetTarget(s.ftarget)
	c:RegisterEffect(e3)
end
function s.eqlimit(e,c)
	return e:GetHandler():GetEquipTarget()==c and c:IsRace(RACE_MACHINE) and c:IsAttribute(ATTRIBUTE_EARTH)
end
function s.filter(c)
```

---

## [251] Tri-Blaze Accelerator (ID: 21420702)
**Lua File:** `script\official\c21420702.lua`

**Description:**
> เปิดใช้งานการ์ดใบนี้โดยส่ง "Blaze Accelerator" ที่หงายหน้าซึ่งคุณควบคุม 1 ใบลงสุสาน ในช่วงเมนเฟสของคุณ: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; ส่งมอนสเตอร์ประเภทไฟ 1 ตัวจากมือคุณลงสุสาน และหากคุณทำเช่นนั้น ให้ทำลายเป้าหมายนั้น และหากคุณทำเช่นนั้น ให้สร้างความเสียหาย 500 แต้มแก่ฝ่ายตรงข้าม มอนสเตอร์ของคุณไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
if chk==0 then return Duel.GetActivityCount(tp,ACTIVITY_ATTACK)==0 end
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_OATH)
	e1:SetTargetRange(1,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.disfilter(c)
	return c:IsRace(RACE_PYRO)
end
function s.destg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
```

---

## [252] True King of All Calamities (ID: 88581108)
**Lua File:** `script\official\c88581108.lua`

**Description:**
> มอนสเตอร์เลเวล 9 2 ตัวขึ้นไป
เทิร์นละครั้ง (ควิกเอฟเฟกต์): คุณสามารถถอดวัตถุดิบ 1 ตัวจากการ์ดใบนี้และประกาศแอตทริบิวต์ 1 อย่าง; เทิร์นนี้ มอนสเตอร์ที่หงายหน้าทั้งหมดบนฟิลด์กลายเป็นแอตทริบิวต์นั้น และมอนสเตอร์ทั้งหมดที่ฝ่ายตรงข้ามครอบครองที่มีแอตทริบิวต์นั้นไม่สามารถเปิดใช้งานเอฟเฟกต์หรือโจมตีได้ มอนสเตอร์ที่มอนสเตอร์ "True Draco" และ "True King" ในมือของคุณจะทำลายด้วยเอฟเฟกต์ของพวกมัน สามารถเลือกได้จากฟิลด์ของฝ่ายตรงข้าม

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
	--All monsters with that Attribute cannot attack
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD)
	e3:SetCode(EFFECT_CANNOT_ATTACK)
	e3:SetTargetRange(0,LOCATION_MZONE)
	e3:SetLabel(attr)
	e3:SetTarget(s.atktarget)
	e3:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e3,tp)
end
function s.aclimit(e,re,tp)
	local c=re:GetHandler()
	return re:IsMonsterEffect() and c:IsAttribute(e:GetLabel())
end
```

---

## [253] Tuning Gum (ID: 82744076)
**Lua File:** `script\official\c82744076.lua`

**Description:**
> คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่หงายหน้าอยู่ 1 ตัวที่คุณควบคุม; มอนสเตอร์ที่หงายหน้าอยู่นั้นจะถูกมองว่าเป็นจูนเนอร์ในเทิร์นนี้ คุณสามารถใช้เอฟเฟกต์นี้ของ "Tuning Gum" ได้เทิร์นละครั้งเท่านั้น คุณสามารถโจมตีด้วยมอนสเตอร์ซิงโครเท่านั้นในเทิร์นที่คุณเปิดใช้งานเอฟเฟกต์นี้ ในเทิร์นของผู้เล่นคนใดก็ได้ เมื่อการ์ดหรือเอฟเฟกต์ถูกเปิดใช้งานที่เลือกเป้าหมายมอนสเตอร์ซิงโคร 1 ตัวที่คุณควบคุมโดยเฉพาะ (และไม่มีการ์ดอื่น): คุณสามารถนำการ์ดใบนี้จากสุสานของคุณออกนอกเกม; ยกเลิกการเปิดใช้งาน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTarget(s.atklimit)
	e1:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.atklimit(e,c)
	return not c:IsType(TYPE_SYNCHRO)
end
function s.filter(c)
	return c:IsFaceup() and not c:IsType(TYPE_TUNER)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	if chkc then return chkc:IsLocation(LOCATION_MZONE) and chkc:IsControler(tp) and s.filter(chkc) end
	if chk==0 then return Duel.IsExistingTarget(s.filter,tp,LOCATION_MZONE,0,1,nil) end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_FACEUP)
	Duel.SelectTarget(tp,s.filter,tp,LOCATION_MZONE,0,1,1,nil)
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local tc=Duel.GetFirstTarget()
	if tc:IsRelateToEffect(e) and tc:IsFaceup() then
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetCode(EFFECT_ADD_TYPE)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		e1:SetValue(TYPE_TUNER)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.atklimit(e,c)
	return not c:IsType(TYPE_SYNCHRO)
end
```

---

## [254] Tyrant Red Dragon Archfiend (ID: 16172067)
**Lua File:** `script\official\c16172067.lua`

**Description:**
> จูนเนอร์ 2 ตัว + มอนสเตอร์ที่ไม่ใช่จูนเนอร์ 1 ตัวขึ้นไป
ต้องถูกอัญเชิญซิงโครแบบเท่านั้น และไม่สามารถอัญเชิญแบบพิเศษด้วยวิธีอื่นได้ คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Tyrant Red Dragon Archfiend" ได้เทิร์นละครั้งเท่านั้น
●ในช่วงเมนเฟสที่ 1 ของคุณ: คุณสามารถทำลายการ์ดอื่น ๆ ทั้งหมดบนฟิลด์ และสำหรับเทิร์นที่เหลือนี้ มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถโจมตีได้
●ในช่วง Battle Phase ของผู้เล่นคนใดก็ตาม เมื่อการ์ดเวทมนตร์/กับดักถูกเปิดใช้งาน: คุณสามารถยกเลิกการเปิดใช้งาน และหากทำเช่นนั้น ให้ทำลายการ์ดนั้น และหากทำเช่นนั้น การ์ดใบนี้จะได้รับ ATK 500 แต้ม

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(c:GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,2),nil)
end
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
function s.discon(e,tp,eg,ep,ev,re,r,rp)
	return not e:GetHandler():IsStatus(STATUS_BATTLE_DESTROYED) and re:IsHasType(EFFECT_TYPE_ACTIVATE)
		and Duel.IsChainNegatable(ev) and Duel.IsBattlePhase()
end
function s.distg(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return true end
	Duel.SetOperationInfo(0,CATEGORY_NEGATE,eg,1,0,0)
	if re:GetHandler():IsDestructable() and re:GetHandler():IsRelateToEffect(re) then
		Duel.SetOperationInfo(0,CATEGORY_DESTROY,eg,1,0,0)
	end
end
function s.disop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	if Duel.NegateActivation(ev) and re:GetHandler():IsRelateToEffect(re)
		and Duel.Destroy(eg,REASON_EFFECT) and c:IsRelateToEffect(e) and c:IsFaceup() then
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_UPDATE_ATTACK)
		e1:SetProperty(EFFECT_FLAG_COPY_INHERIT)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD_DISABLE)
		e1:SetValue(500)
		c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [255] Ultimate Tyranno (ID: 15894048)
**Lua File:** `script\official\c15894048.lua`

**Description:**
> การ์ดใบนี้สามารถโจมตีมอนสเตอร์ทั้งหมดที่ฝ่ายตรงข้ามควบคุมได้ ครั้งละ 1 ตัว ในระหว่างเฟสแบทเทิลของคุณ หากคุณควบคุม "Ultimate Tyranno" ที่สามารถโจมตีได้ มอนสเตอร์อื่นที่ไม่ใช่ "Ultimate Tyranno" จะไม่สามารถโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetRange(LOCATION_MZONE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.catg)
	e1:SetCondition(s.cacon)
	c:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.catg(e,c)
	return not c:IsCode(id)
end
```
```lua
function s.cacon(e)
	return Duel.IsBattlePhase() and Duel.IsExistingMatchingCard(s.cfilter,e:GetHandlerPlayer(),LOCATION_MZONE,0,1,nil)
end
```

---

## [256] Uni-Zombie (ID: 49959355)
**Lua File:** `script\official\c49959355.lua`

**Description:**
> คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่เปิดเผย 1 ตัวบนฟิลด์; ทิ้งการ์ด 1 ใบ และถ้าคุณทำเช่นนั้น เพิ่มเลเวลของเป้าหมายนั้น 1 เลเวล คุณสามารถเลือกเป้าหมายมอนสเตอร์ที่เปิดเผย 1 ตัวบนฟิลด์; มอนสเตอร์ที่คุณควบคุมไม่สามารถโจมตีได้ในเทิร์นที่เหลือ ยกเว้นมอนสเตอร์ประเภทซอมบี้ และส่งมอนสเตอร์ประเภทซอมบี้ 1 ตัวจากเด็คของคุณลงสุสาน และถ้าคุณทำเช่นนั้น เพิ่มเลเวลของเป้าหมายนั้น 1 เลเวล คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Uni-Zombie" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetValue(1)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1)
	end
	local e2=Effect.CreateEffect(e:GetHandler())
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetTargetRange(LOCATION_MZONE,0)
	e2:SetTarget(s.atktg)
	e2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e2,tp)
	local e3=Effect.CreateEffect(e:GetHandler())
	e3:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_CLIENT_HINT)
	e3:SetDescription(aux.Stringid(id,2))
```

---

## [257] Union Attack (ID: 60399954)
**Lua File:** `script\official\c60399954.lua`

**Description:**
> เลือกเป้าหมายมอนสเตอร์ที่หงายหน้าอยู่ 1 ตัวที่คุณควบคุม เทิร์นนี้ มอนสเตอร์นั้นไม่สามารถสร้างความเสียหายจากการต่อสู้ให้แก่ฝ่ายตรงข้ามได้ และมอนสเตอร์ในตำแหน่งโจมตีที่หงายหน้าตัวอื่นไม่สามารถโจมตีได้ นอกจากนี้ ในช่วงสตาร์ทสเต็ปของแต่ละแบตเทิลเฟสในเทิร์นนี้ ให้มอนสเตอร์นั้นได้รับ ATK เท่ากับ ATK รวมของมอนสเตอร์อื่นทั้งหมดในตำแหน่งโจมตีที่คุณควบคุม จนกว่าจะสิ้นสุดแบตเทิลเฟสนั้น

* ข้อความข้างต้นไม่เป็นทางการและอธิบายฟังก์ชันของการ์ดใน OCG

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetProperty(EFFECT_FLAG_OATH+EFFECT_FLAG_IGNORE_IMMUNE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.ftarget)
	e1:SetLabel(g:GetFirst():GetFieldID())
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc:IsFaceup() and tc:IsRelateToEffect(e) then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
		e1:SetCode(EVENT_PHASE|PHASE_BATTLE_START)
		e1:SetRange(LOCATION_MZONE)
		e1:SetCountLimit(1)
		e1:SetOperation(s.atkop)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [258] Urgent Schedule (ID: 25274141)
**Lua File:** `script\official\c25274141.lua`

**Description:**
> หากฝ่ายตรงข้ามควบคุมมอนสเตอร์มากกว่าคุณ: อัญเชิญแบบพิเศษมอนสเตอร์ EARTH Machine เลเวล 4 หรือต่ำกว่า 1 ตัว และมอนสเตอร์ EARTH Machine เลเวล 5 หรือสูงกว่า 1 ตัว จากเด็คของคุณในตำแหน่งป้องกัน แต่ยกเลิกเอฟเฟกต์ของพวกมัน คุณไม่สามารถประกาศโจมตีในเทิร์นที่คุณเปิดใช้งานการ์ดนี้ได้ ยกเว้นกับมอนสเตอร์ Machine หากการ์ดที่เซ็ตนี้ถูกส่งจากฟิลด์ลงสุสาน: คุณสามารถนำมอนสเตอร์ Machine เลเวล 10 1 ตัวจากเด็คขึ้นมือ คุณสามารถใช้แต่ละเอฟเฟกต์ของ "Urgent Schedule" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE+EFFECT_FLAG_OATH)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(s.atktg)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
	aux.RegisterClientHint(e:GetHandler(),nil,tp,1,0,aux.Stringid(id,2),nil)
end
function s.atktg(e,c)
	return not c:IsRace(RACE_MACHINE)
end
function s.spfilter(c,e,tp)
	return c:IsRace(RACE_MACHINE) and c:IsAttribute(ATTRIBUTE_EARTH) and c:IsCanBeSpecialSummoned(e,0,tp,false,false,POS_FACEUP_DEFENSE)
end
function s.rescon(sg,e,tp,mg)
	return aux.ChkfMMZ(2)(sg,e,tp,mg) and sg:FilterCount(s.spfilter,nil,e,tp)==2
		and sg:FilterCount(Card.IsLevelAbove,nil,5)==1 and sg:FilterCount(Card.IsLevelBelow,nil,4)==1
end
function s.sptg(e,tp,eg,ep,ev,re,r,rp,chk)
	local g=Duel.GetMatchingGroup(s.spfilter,tp,LOCATION_DECK,0,nil,e,tp)
	if chk==0 then return aux.SelectUnselectGroup(g,e,tp,2,2,s.rescon,chk) and Duel.GetLocationCount(tp,LOCATION_MZONE)>1
		and not Duel.IsPlayerAffectedByEffect(tp,CARD_BLUEEYES_SPIRIT) 
	end
	Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,2,tp,LOCATION_DECK)
end
function s.spop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	if Duel.IsPlayerAffectedByEffect(tp,CARD_BLUEEYES_SPIRIT) or Duel.GetLocationCount(tp,LOCATION_MZONE)<2 then return end
	local g=Duel.GetMatchingGroup(s.spfilter,tp,LOCATION_DECK,0,nil,e,tp)
	local sg=aux.SelectUnselectGroup(g,e,tp,2,2,s.rescon,1,tp,HINTMSG_SPSUMMON)
	if #sg>0 then
		for tc in aux.Next(sg) do
			if Duel.SpecialSummonStep(tc,0,tp,tp,false,false,POS_FACEUP_DEFENSE)~=0 then
				local e1=Effect.CreateEffect(c)
				e1:SetType(EFFECT_TYPE_SINGLE)
				e1:SetCode(EFFECT_DISABLE)
				e1:SetReset(RESET_EVENT|RESETS_STANDARD)
				tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.atktg(e,c)
	return not c:IsRace(RACE_MACHINE)
end
```

---

## [259] Ursarctic Slider (ID: 53865474)
**Lua File:** `script\official\c53865474.lua`

**Description:**
> เลือกเป้าหมายมอนสเตอร์ 'Ursarctic' 1 ตัวของคุณที่ถูกนำออกนอกเกมหรืออยู่ในสุสานของคุณ; อัญเชิญแบบพิเศษมัน แต่ไม่สามารถโจมตีได้ และทำลายมันในช่วง End Phase สำหรับเทิร์นที่เหลือหลังจากที่การ์ดใบนี้รีโซลฟ์ คุณไม่สามารถอัญเชิญแบบพิเศษได้ ยกเว้นมอนสเตอร์ที่มีเลเวล คุณสามารถเปิดใช้งาน 'Ursarctic Slider' ได้ 1 ครั้งต่อเทิร์นเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1,true)
		tc:RegisterFlagEffect(id,RESET_EVENT|RESETS_STANDARD,0,1)
		local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
		e2:SetCode(EVENT_PHASE+PHASE_END)
```

---

## [260] Vaalmonica, the Agathokakological Voice (ID: 39210885)
**Lua File:** `script\official\c39210885.lua`

**Description:**
> เมื่อการ์ดใบนี้ถูกเปิดใช้งาน: คุณสามารถนำมอนสเตอร์ "Vaalmonica" 1 ตัวจากเด็คของคุณขึ้นมือ หากการ์ดในโซนเพนดูลัมของคุณมี Resonance Counter ตัวที่ 3 วางอยู่: คุณสามารถเลือกเป้าหมายมอนสเตอร์ 1 ตัวที่ฝ่ายตรงข้ามควบคุม; ยึดการควบคุมมันจนกระทั่งสิ้นสุดเฟสเอนด์ แต่ไม่สามารถประกาศโจมตีได้ คุณสามารถใช้เอฟเฟกต์นี้ของ "Vaalmonica, the Agathokakological Voice" ได้เทิร์นละครั้งเท่านั้น คุณสามารถเปิดใช้งาน "Vaalmonica, the Agathokakological Voice" ได้เทิร์นละ 1 ใบเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetCondition(function(e) return e:GetHandler():IsControler(tp) end)
		e1:SetReset(RESET_EVENT|(RESETS_STANDARD&~RESET_TURN_SET)|RESET_PHASE|PHASE_END)
		tc:RegisterEffect(e1)
```

---

## [261] Vain Betrayer (ID: 94933468)
**Lua File:** `script\official\c94933468.lua`

**Description:**
> เมื่อมอนสเตอร์เอ็กซีสของฝ่ายตรงข้ามประกาศโจมตี: เปิดใช้งานการ์ดนี้โดยเลือกมอนสเตอร์ที่โจมตีนั้น มอนสเตอร์นั้นไม่สามารถโจมตีได้ และเอฟเฟกต์ของมันจะถูกยกเลิก ขณะที่มอนสเตอร์นั้นอยู่บนฟิลด์ ในแต่ละเอนด์เฟสของฝ่ายตรงข้าม: ส่งการ์ด 3 ใบจากด้านบนของเด็คของฝ่ายตรงข้ามลงสุสาน เมื่อมอนสเตอร์นั้นออกจากฟิลด์ ให้ทำลายการ์ดนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e3:SetRange(LOCATION_SZONE)
	e3:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e3:SetTarget(aux.PersistentTargetFilter)
	c:RegisterEffect(e3)
	local e4=e3:Clone()
	e4:SetCode(EFFECT_CANNOT_ATTACK)
	c:RegisterEffect(e4)
	--destroy
	local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_CONTINUOUS+EFFECT_TYPE_FIELD)
	e5:SetRange(LOCATION_SZONE)
	e5:SetCode(EVENT_LEAVE_FIELD)
	e5:SetCondition(s.descon)
	e5:SetOperation(s.desop)
	c:RegisterEffect(e5)
	--discard deck
```

---

## [262] Vampire Scarlet Scourge (ID: 79523365)
**Lua File:** `script\official\c79523365.lua`

**Description:**
> หากการ์ดใบนี้ถูกอัญเชิญแบบปกติหรืออัญเชิญแบบพิเศษ: คุณสามารถจ่าย LP 1,000 แต้ม จากนั้นเลือกเป้าหมายมอนสเตอร์ "Vampire" 1 ตัวในสุสานของคุณ ยกเว้น "Vampire Scarlet Scourge"; อัญเชิญแบบพิเศษมัน แต่ไม่สามารถโจมตีในเทิร์นนี้ได้ คุณสามารถใช้เอฟเฟกต์นี้ของ "Vampire Scarlet Scourge" ได้เทิร์นละครั้งเท่านั้น เมื่อจบแบตเทิลเฟส หากการ์ดใบนี้ทำลายมอนสเตอร์ใดๆ ด้วยการต่อสู้: คุณสามารถอัญเชิญแบบพิเศษพวกมันจากสุสานมายังฟิลด์ของคุณ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetDescription(3206)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e1)
```

---

## [263] Vengeful Bog Spirit (ID: 95220856)
**Lua File:** `script\official\c95220856.lua`

**Description:**
> มอนสเตอร์ไม่สามารถโจมตีในเทิร์นที่ถูกอัญเชิญ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--attack res
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetTarget(s.target)
	c:RegisterEffect(e2)
end
function s.target(e,c)
	return c:IsStatus(STATUS_SUMMON_TURN+STATUS_FLIP_SUMMON_TURN+STATUS_SPSUMMON_TURN)
```

---

## [264] Viper's Grudge (ID: 1683982)
**Lua File:** `script\official\c1683982.lua`

**Description:**
> มอนสเตอร์ที่ไม่ใช่สัตว์เลื้อยคลานที่คุณควบคุมไม่สามารถโจมตี และคุณไม่สามารถเปิดใช้งานเอฟเฟกต์ของพวกมัน คุณสามารถใช้แต่ละเอฟเฟกต์ต่อไปนี้ของ "Viper's Grudge" ได้เทิร์นละครั้งเท่านั้น หากมอนสเตอร์สัตว์เลื้อยคลานหงายหน้าที่คุณควบคุมถูกทำลายโดยการต่อสู้หรือถูกส่งลงสุสาน: คุณสามารถอัญเชิญแบบพิเศษมอนสเตอร์สัตว์เลื้อยคลานเลเวล 4 หรือต่ำกว่า 1 ตัวจากเด็คของคุณ หากการ์ดใบนี้ถูกทำลายใน Spell & Trap Zone: คุณสามารถคืนมอนสเตอร์สัตว์เลื้อยคลานที่ถูกนำออกนอกเกมทั้งหมดของคุณไปยังสุสาน

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetRange(LOCATION_SZONE)
	e1:SetTargetRange(LOCATION_MZONE,0)
	e1:SetTarget(function(_,c) return not c:IsRace(RACE_REPTILE) end)
	c:RegisterEffect(e1)
	local e2=e1:Clone()
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	c:RegisterEffect(e2)
	--Special Summon Reptile from Deck
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,0))
	e3:SetCategory(CATEGORY_SPECIAL_SUMMON)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_O)
```

---

## [265] Wall of Revealing Light (ID: 17078030)
**Lua File:** `script\official\c17078030.lua`

**Description:**
> เปิดใช้งานโดยการจ่าย Life Points เป็นจำนวนเท่าใดก็ได้ที่เป็นพหุคูณของ 1000 มอนสเตอร์ที่ฝ่ายตรงข้ามควบคุมไม่สามารถโจมตีได้หาก ATK ของพวกมันน้อยกว่าหรือเท่ากับจำนวนที่คุณจ่าย

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetType(EFFECT_TYPE_ACTIVATE)
	e1:SetCode(EVENT_FREE_CHAIN)
	e1:SetCost(s.cost)
	c:RegisterEffect(e1)
	--cannot attack
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_SZONE)
	e2:SetTargetRange(0,LOCATION_MZONE)
	e2:SetTarget(s.atktarget)
	c:RegisterEffect(e2)
	e1:SetLabelObject(e2)
end
function s.cost(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then
```

---

## [266] Wicked Rebirth (ID: 23440062)
**Lua File:** `script\official\c23440062.lua`

**Description:**
> เปิดใช้งานการ์ดใบนี้โดยจ่าย 800 LP จากนั้นเลือกเป้าหมายมอนสเตอร์ซิงโคร 1 ตัวในสุสานของคุณ; อัญเชิญมันแบบพิเศษในตำแหน่งโจมตี เอฟเฟกต์ของมันถูกยกเลิก และไม่สามารถประกาศโจมตีในเทิร์นนี้ เมื่อการ์ดใบนี้ออกจากฟิลด์ ทำลายมอนสเตอร์นั้น เมื่อมอนสเตอร์นั้นถูกทำลาย ทำลายการ์ดใบนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e2=Effect.CreateEffect(c)
		e2:SetType(EFFECT_TYPE_SINGLE)
		e2:SetProperty(EFFECT_FLAG_SINGLE_RANGE+EFFECT_FLAG_OWNER_RELATE)
		e2:SetRange(LOCATION_ONFIELD)
		e2:SetCode(EFFECT_CANNOT_ATTACK)
		e2:SetReset(RESETS_STANDARD_PHASE_END)
		tc:RegisterEffect(e2,true)
	end
	Duel.SpecialSummonComplete()
end
function s.desop(e,tp,eg,ep,ev,re,r,rp)
	local tc=e:GetHandler():GetFirstCardTarget()
	if tc and tc:IsLocation(LOCATION_MZONE) then
		Duel.Destroy(tc,REASON_EFFECT)
```

---

## [267] Wild Fire (ID: 68815401)
**Lua File:** `script\official\c68815401.lua`

**Description:**
> จ่าย LP 500 แต้ม; ทำลายการ์ด "Blaze Accelerator" ทั้งหมดที่คุณควบคุม และถ้าคุณทำเช่นนั้น ให้ทำลายมอนสเตอร์บนฟิลด์ให้มากที่สุดเท่าที่จะทำได้ จากนั้น อัญเชิญแบบพิเศษ "Wild Fire Token" 1 ตัว (Pyro/FIRE/Level 3/ATK 1000/DEF 1000) ในตำแหน่งโจมตี มอนสเตอร์ของคุณไม่สามารถโจมตีได้ในเทิร์นที่คุณเปิดใช้งานการ์ดนี้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
Duel.PayLPCost(tp,500)
	local e1=Effect.CreateEffect(e:GetHandler())
	e1:SetType(EFFECT_TYPE_FIELD)
	e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_OATH)
	e1:SetTargetRange(1,0)
	e1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(e1,tp)
end
function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
```

---

## [268] Wonder Clover (ID: 38568567)
**Lua File:** `script\official\c38568567.lua`

**Description:**
> เลือกมอนสเตอร์ที่หงายหน้าอยู่ที่คุณควบคุม 1 ตัว และส่งมอนสเตอร์ประเภทพืช เลเวล 4 1 ตัวจากมือของคุณลงสุสาน ในเทิร์นนี้ มอนสเตอร์ที่ถูกเลือกสามารถโจมตีได้สองครั้ง แต่มอนสเตอร์อื่นที่คุณควบคุมไม่สามารถประกาศโจมตีได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_FIELD)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetProperty(EFFECT_FLAG_OATH)
		e1:SetTargetRange(LOCATION_MZONE,0)
		e1:SetTarget(s.ftarget)
		e1:SetLabel(g:GetFirst():GetFieldID())
		e1:SetReset(RESET_PHASE|PHASE_END)
		Duel.RegisterEffect(e1,tp)
		e:SetLabel(0)
	end
end
function s.operation(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc:IsRelateToEffect(e) then
		local e1=Effect.CreateEffect(e:GetHandler())
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_EXTRA_ATTACK)
		e1:SetReset(RESETS_STANDARD_PHASE_END)
		e1:SetValue(1)
		tc:RegisterEffect(e1)
```
Associated helper functions:
```lua
function s.ftarget(e,c)
	return e:GetLabel()~=c:GetFieldID()
end
```

---

## [269] World Dino Wrestling (ID: 90173539)
**Lua File:** `script\official\c90173539.lua`

**Description:**
> ในขณะที่คุณควบคุมมอนสเตอร์ "Dinowrestler" ผู้เล่นแต่ละคนสามารถโจมตีด้วยมอนสเตอร์เพียง 1 ตัวในแต่ละ Battle Phase หากมอนสเตอร์ "Dinowrestler" ของคุณโจมตีมอนสเตอร์ของคู่ต่อสู้ มันจะได้รับ ATK 200 หน่วยเฉพาะในระหว่างการคำนวณความเสียหายเท่านั้น หากคู่ต่อสู้ของคุณควบคุมมอนสเตอร์มากกว่าคุณ: คุณสามารถนำการ์ดใบนี้จากสุสานออกนอกเกม; อัญเชิญแบบพิเศษมอนสเตอร์ "Dinowrestler" 1 ตัวจากเด็คของคุณ คุณสามารถใช้เอฟเฟกต์นี้ของ "World Dino Wrestling" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetCode(EVENT_FREE_CHAIN)
	c:RegisterEffect(e1)
	--attack limit
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_FIELD)
	e2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	e2:SetCode(EFFECT_CANNOT_ATTACK)
	e2:SetRange(LOCATION_FZONE)
	e2:SetTargetRange(LOCATION_MZONE,LOCATION_MZONE)
	e2:SetCondition(s.atkcon1)
	e2:SetTarget(s.atktg1)
	c:RegisterEffect(e2)
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
```

---

## [270] World Legacy Monstrosity (ID: 14604710)
**Lua File:** `script\official\c14604710.lua`

**Description:**
> เปิดใช้งาน 1 ในเอฟเฟกต์เหล่านี้
● อัญเชิญแบบพิเศษมอนสเตอร์เลเวล 9 1 ตัวจากมือของคุณ
● เลือกเป้าหมายมอนสเตอร์เลเวล 9 1 ตัวที่คุณควบคุม; อัญเชิญแบบพิเศษจากเด็คของคุณ มอนสเตอร์เลเวล 9 2 ตัวที่มีประเภทดั้งเดิมและธาตุดั้งเดิมแตกต่างจากมอนสเตอร์เป้าหมายที่หงายหน้านั้น และมีชื่อแตกต่างกัน แต่ละตัว แต่ทั้ง 2 ตัวนี้ไม่สามารถโจมตีได้ และทำลายพวกมันในช่วง End Phase
คุณสามารถเปิดใช้งาน "World Legacy Monstrosity" ได้เทิร์นละครั้งเท่านั้น

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
--Cannot attack
			local e1=Effect.CreateEffect(c)
			e1:SetDescription(3206)
			e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
			e1:SetType(EFFECT_TYPE_SINGLE)
			e1:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
			e1:SetReset(RESET_EVENT|RESETS_STANDARD)
			sc:RegisterEffect(e1,true)
			sc:RegisterFlagEffect(id,RESET_EVENT|RESETS_STANDARD,0,1,fid)
		end
		Duel.SpecialSummonComplete()
		sg:KeepAlive()
		--Destroy them during end phase
		local e2=Effect.CreateEffect(c)
```

---

## [271] Worm Rakuyeh (ID: 17649753)
**Lua File:** `script\official\c17649753.lua`

**Description:**
> การ์ดนี้สามารถประกาศโจมตีได้เทิร์นที่การ์ดนี้ถูกพลิกหงายหน้าขึ้นเท่านั้น หากการ์ดนี้โจมตี การ์ดนี้จะถูกเปลี่ยนเป็นตำแหน่งการ์ดป้องกันแบบคว่ำเมื่อสิ้นสุด Battle Phase

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
e1:SetCode(EVENT_FLIP)
	e1:SetOperation(s.flipop)
	c:RegisterEffect(e1)
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_SINGLE)
	e2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	e2:SetCondition(s.atkcon)
	c:RegisterEffect(e2)
	--to defense
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
	e3:SetCode(EVENT_PHASE|PHASE_BATTLE)
	e3:SetRange(LOCATION_MZONE)
	e3:SetCountLimit(1)
	e3:SetCondition(s.poscon)
```

---

## [272] Xyz Armor Torpedo (ID: 94151981)
**Lua File:** `script\official\c94151981.lua`

**Description:**
> มอนสเตอร์เลเวล 3 จำนวน 2 ตัว
ไม่สามารถโจมตีได้หากไม่มีวัตถุดิบ คุณสามารถถอดวัตถุดิบ 2 ชิ้นจากการ์ดใบนี้; จั่วการ์ด 1 ใบ คุณสามารถใช้เอฟเฟกต์นี้ของ "Xyz Armor Torpedo" ได้เทิร์นละครั้งเท่านั้น ใช้เอฟเฟกต์เหล่านี้ในขณะที่การ์ดใบนี้ถูกสวมใส่ให้กับมอนสเตอร์
● หากมอนสเตอร์ที่สวมใส่ต่อสู้ จนกระทั่งสิ้นสุด Damage Step ฝ่ายตรงข้ามไม่สามารถเปิดใช้งานการ์ดหรือเอฟเฟกต์ได้ และยกเลิกเอฟเฟกต์ของมอนสเตอร์ที่หงายหน้าทั้งหมดที่พวกเขาควบคุม
● หากมอนสเตอร์ที่สวมใส่เป็นมอนสเตอร์ Xyz ฝ่ายตรงข้ามไม่สามารถเลือกมันเป็นเป้าหมายด้วยเอฟเฟกต์การ์ดได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetCode(EFFECT_CANNOT_ATTACK)
	e1:SetCondition(function(e) return e:GetHandler():GetOverlayCount()==0 end)
	c:RegisterEffect(e1)
```

---

## [273] Xyz Remora (ID: 43138260)
**Lua File:** `script\official\c43138260.lua`

**Description:**
> คุณสามารถอัญเชิญแบบพิเศษการ์ดใบนี้ (จากมือของคุณ) โดยการถอดวัตถุ 2 ตัวจากมอนสเตอร์ที่คุณควบคุม เมื่อถูกอัญเชิญด้วยวิธีนี้: คุณสามารถเลือกเป้าหมายมอนสเตอร์ประเภทปลาเลเวล 4 จำนวน 2 ตัวในสุสานของคุณ อัญเชิญแบบพิเศษเป้าหมายเหล่านั้นในตำแหน่งป้องกัน เอฟเฟกต์ของพวกมันจะถูกยกเลิก พวกมันไม่สามารถโจมตีหรือเปลี่ยนตำแหน่งต่อสู้ได้ และไม่สามารถใช้เป็นวัตถุสำหรับการอัญเชิญแบบเอ็กซีสได้ ยกเว้นสำหรับการอัญเชิญแบบเอ็กซีสของมอนสเตอร์ธาตุน้ำ

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
local e1=Effect.CreateEffect(c)
		e1:SetDescription(3206)
		e1:SetProperty(EFFECT_FLAG_CLIENT_HINT)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_CANNOT_ATTACK)
		e1:SetReset(RESET_EVENT|RESETS_STANDARD)
		tc:RegisterEffect(e1)
```

---

## [274] ZS - Utopic Sage (ID: 31123642)
**Lua File:** `script\official\c31123642.lua`

**Description:**
> มอนสเตอร์ 2 ตัว เลเวล 4
คุณสามารถถอดวัตถุดิบ 2 ตัวจากการ์ดใบนี้; อัญเชิญแบบพิเศษมอนสเตอร์ "ZW-" หรือ "ZS-" 1 ตัวจากเด็คของคุณ และสำหรับเทิร์นที่เหลือนี้ คุณสามารถโจมตีด้วยมอนสเตอร์ "Number" เท่านั้น และคุณไม่สามารถอัญเชิญแบบพิเศษจากเอ็กซ์ตร้าเด็คได้ ยกเว้นมอนสเตอร์ Xyz คุณสามารถใช้เอฟเฟกต์นี้ของ "ZS - Utopic Sage" ได้เทิร์นละครั้งเท่านั้น หากมอนสเตอร์ Xyz "Utopia" หรือ "Utopic" ที่คุณควบคุมซึ่งแอตทริบิวต์เดิมเป็น LIGHT จะถูกทำลายจากการต่อสู้หรือเอฟเฟกต์การ์ด ยกเว้น "ZS - Utopic Sage" คุณสามารถนำการ์ดใบนี้จากฟิลด์หรือสุสานของคุณออกนอกเกมแทนได้

**Lua Implementation details:**
### Effect 1 (EFFECT_CANNOT_ATTACK)
```lua
ge1:SetTarget(s.splimit)
	ge1:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(ge1,tp)
	--cannot attack
	local ge2=Effect.CreateEffect(c)
	ge2:SetType(EFFECT_TYPE_FIELD)
	ge2:SetCode(EFFECT_CANNOT_ATTACK_ANNOUNCE)
	ge2:SetProperty(EFFECT_FLAG_IGNORE_IMMUNE)
	ge2:SetTargetRange(LOCATION_MZONE,0)
	ge2:SetTarget(s.atktg)
	ge2:SetReset(RESET_PHASE|PHASE_END)
	Duel.RegisterEffect(ge2,tp)
	--client hint
	aux.RegisterClientHint(c,nil,tp,1,0,aux.Stringid(id,2),nil)
```

---
