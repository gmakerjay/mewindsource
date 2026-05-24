# คู่มือปรับปรุง Bot ฉบับเร่งด่วน — WindBot IGNIS

> อัปเดต: 2026-05-24 | ภาษา: ไทย  
> สรุปขั้นตอนที่ต้องทำเพื่อทำให้ Bot เล่นเก่งขึ้น

---

## 🚨 ภารกิจด่วนที่สุด (ทำวันนี้)

### 1. กู้ชีพ 4 เด็คที่เล่นไม่ได้

เด็ค Goldlord, Invoke, Kwtune, Labrynth **ไม่สามารถเล่นการ์ดหลักของตัวเองได้** เพราะ Registry ว่างเปล่า

**วิธีแก้:** รันคำสั่งต่อไปนี้:
```bash
python auto_role_detector.py --deck 2026_Goldlord --overwrite
python auto_role_detector.py --deck 2026_Invoke --overwrite
python auto_role_detector.py --deck 2026_Kwtune --overwrite
python auto_role_detector.py --deck 2026_Labrynth --overwrite
```

### 2. แก้ Destructor ที่ทำให้ Learning ไม่เวิร์ก

Destructor ใน C# (`~UnifiedIgnisExecutor()`) ทำงานตอน Garbage Collection ซึ่งไม่แน่นอน  
→ ทำให้ `ApplyRealTimeLearning()` ไม่ได้ถูกเรียก หรือเรียกตอน `Duel.Fields` ถูก Dispose แล้ว

**วิธีแก้:** เปลี่ยนเป็น `IDisposable` pattern + เรียกใน `OnChainEnd()` หรือ `OnNewTurn()`

### 3. แก้ Anti-Inflation Decay vs Hard Cap

ปัจจุบัน Hard Cap (priority > 8 → 8) ทำงานก่อน Anti-Inflation Decay (priority ≥ 8 → -1)  
→ Decay ไม่มีวันทำงาน

**วิธีแก้:** สลับลำดับ — เอา Decay ไว้ก่อน แล้วค่อย Hard Cap

---

## ⚡ อัพเกรดที่เห็นผลทันที (1-2 วัน)

### 4. สร้าง Battle Phase AI

Bot **ไม่มี** AI สำหรับ Battle Phase → ตีไม่เป็น, ไม่รู้ว่าเมื่อไหร่ควรตี

**สิ่งที่ต้องสร้างใน C#:**

```csharp
// ฟังก์ชันใหม่ที่ต้องเพิ่ม
public override bool OnBattlePhase()
{
    // ตรวจ lethal
    if (IsLethalOnBoard()) return true;
    
    // ตรวจ opponent backrow (battle traps)
    if (HasOpponentBattleTrap()) return false;
    
    // ตรวจ opponent hand (Honest, Kuriphoton)
    if (HasOpponentHandTrap()) return false;
    
    return true; // safe to attack
}

public override ClientCard OnSelectAttackTarget(List<ClientCard> targets)
{
    // Logic: attack weakest link → attack directly → break key monster
    // Priority: 1. Direct attack (ถ้าได้) 2. Weakest monster 3. Choke point
}
```

### 5. เพิ่ม Resource Tracking

Bot ควรรู้ card advantage / tempo / deck count:

- `Bot.Hand.Count - Opponent.Hand.Count` → card advantage
- `Bot.LifePoints - Opponent.LifePoints` → life advantage
- `Deck.Count` → รู้ว่าเหลือกี่ใบ (สำคัญตอนใช้ Pot of Desires)

### 6. เพิ่ม Hand Trap Probability Model

Bot ควรเดาว่าฝ่ายตรงข้ามมี hand trap อะไรบ้าง:

- ถ้า opponent จั่ว 5 ใบ + ไม่ activate → high hand trap probability
- ถ้า opponent ใช้ Maxx "C" → มี hand traps อื่นอีก
- ถ้า opponent ผ่านเทิร์นโดยไม่ทำอะไร → low resources / brick

---

## 🧠 ระบบที่ทำให้ Bot เล่นเทียบเท่าคน (1-2 สัปดาห์)

### 7. Lookahead Search

Bot ปัจจุบันตัดสินใจแบบ greedy (score ปัจจุบันเท่านั้น)  
Bot ระดับสูงใช้ search tree (BFS / Minimax):

```python
# สิ่งที่ Bot ควรทำ:
def evaluate_board_state(board):
    score = 0
    score += card_advantage * 10
    score += monster_presence * 5
    score -= opponent_threat * 8
    return score

def search(hand, board, depth=2):
    if depth == 0:
        return evaluate_board_state(board)
    best_score = -inf
    for card in hand:
        new_board = simulate_play(card, board)
        score = search(hand - card, new_board, depth - 1)
        best_score = max(best_score, score)
    return best_score
```

### 8. Chain Optimization

Bot ควรรู้ว่า chain การ์ดไหนก่อน-หลัง:

- **Card Interruption (Ash/Veiler)**: รอให้ opponent ใช้ resource ก่อน
- **Card with protection (Called by)**: ใช้หลังเพื่อ protect combo
- **Bait card (low value)**: ใช้ก่อน เพื่อดึง hand trap

---

## 📊 ระบบที่ต้องแก้ (Infrastructure)

### 9. Continuous Learning Loop

ปัจจุบัน: match → (ผู้ใช้ต้องรัน learning เอง) → deploy  
อนาคต: match → auto learn → auto deploy → repeat

สร้าง orchestrator script:

```python
def continuous_learning_loop(deck, iterations=100):
    for i in range(iterations):
        # Step 1: เล่น match
        run_match(deck)
        
        # Step 2: เรียนรู้จาก match log
        subprocess.run(["python", "learning_sandbox.py", "--deck", deck])
        subprocess.run(["python", "q_learning.py", "--deck", deck])
        
        # Step 3: Deploy ไป LIVE
        deploy_registry(deck)
        
        print(f"Iteration {i+1}/{iterations} complete")
```

### 10. Registry Versioning

ทุกครั้งที่ deploy → save snapshot + timestamp:

```
config/registry_history/
  cards_registry_2026_AzaYummy_20260524_120000.json
  cards_registry_2026_AzaYummy_20260524_130000.json
  ...
```

---

## 🔍 เครื่องมือที่ช่วย Debug

### 11. Dashboard Analytics

ปัจจุบัน `/analytics` มี dashboard อยู่แล้ว → ควรเพิ่ม:

- **Win rate over time**: กราฟ win/loss/draw ตาม match
- **Priority distribution**: ดูว่า priority inflation เกิดขึ้นไหม
- **Goal distribution**: ดูว่า goal เปลี่ยนบ่อยแค่ไหน
- **Combo success rate**: ดูว่าแผน A/B/C สำเร็จกี่%

### 12. Per-Match Visualizer

สร้างเครื่องมือ visualize decision log:

```
Turn 1:
  Draw Phase: 5 cards in hand
  Main Phase:
    ✓ Activate Chicken Game (score: 55.3, goal: establish_interruptions)
    ✗ Activate Pot of Desires (score: 32.1, goal: establish_interruptions)
    ✓ Summon Aleister (score: 48.7, goal: establish_interruptions)
  Chain: Opponent responded with Ash Blossom!
    → Plan A blocked, switching to Plan B
```

---

## 📋 Priority Matrix

| Task | Impact | Effort | ทำก่อน? |
|------|:------:|:------:|:-------:|
| Fix 4 empty registries | 🔴 สูงมาก | ⚡ 1 ชม. | ✅ **อันดับ 1** |
| Fix destructor | 🔴 สูง | 🕐 2 ชม. | ✅ **อันดับ 2** |
| Fix Anti-Inflation Decay | 🟡 กลาง | ⚡ 30 นาที | ✅ **อันดับ 3** |
| Add Battle Phase AI | 🟢 สูง | 🕐 3 ชม. | ✅ **อันดับ 4** |
| Add Resource Tracking | 🟡 กลาง | 🕐 3 ชม. | ➡️ อันดับ 5 |
| Add Hand Trap Model | 🟢 สูง | 🕐 5 ชม. | ➡️ อันดับ 6 |
| Add Lookahead Search | 🔴 สูงมาก | 📅 10 ชม. | ➡️ อันดับ 7 |
| Continuous Learning Loop | 🟢 สูง | 📅 8 ชม. | ➡️ อันดับ 8 |
| Chain Optimization | 🟡 กลาง | 🕐 2 ชม. | ➡️ อันดับ 9 |
| Registry Versioning | 🟢 สูง | 🕐 2 ชม. | ➡️ อันดับ 10 |

> **Legend:** ⚡ = <1 ชม. | 🕐 = 1-5 ชม. | 📅 = 5+ ชม.

---

## 🎯 สรุปเป้าหมาย

| ระยะ | เป้าหมาย | Win Rate ที่คาด |
|------|----------|:---------------:|
| **วันนี้** | กู้ชีพ 4 เด็ค + แก้ learning pipeline | 0% → 20% (4 decks กลับมาเล่นได้) |
| **1 สัปดาห์** | Battle Phase + Resource Tracking + Hand Trap Model | 20% → 40% |
| **2 สัปดาห์** | Lookahead Search + Chain Optimization | 40% → 60% |
| **1 เดือน** | Continuous Learning + MCTS | 60% → 75% |
| **3 เดือน** | Full combo planning + Opponent adaptation | 75% → 85% |

---

*จัดทำโดย Codebuff AI — แนวทางปรับปรุง WindBot IGNIS*
