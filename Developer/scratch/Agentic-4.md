# รายงานการวิเคราะห์และออกแบบสถาปัตยกรรมระบบ — Agentic-4

> **วันที่วิเคราะห์:** 25 พฤษภาคม 2026
> **ขอบเขต:** 12 ไฟล์ Source Code ในโฟลเดอร์ `Ignis_Train_Audit`
> **วัตถุประสงค์:** ออกแบบระบบป้องกันบัคจากการรันพร้อมกันหลายจอ, เก็บข้อมูลอัตโนมัติเมื่อ LP=0, Auto Deploy แบบ Real-time, รองรับ 20+ จอเทรนสองเด็คพร้อมกัน

---

## สารบัญ

1. [บทสรุปผู้บริหาร — Executive Summary](#1-บทสรุปผู้บริหาร--executive-summary)
2. [สถาปัตยกรรมปัจจุบัน — Current Architecture](#2-สถาปัตยกรรมปัจจุบัน--current-architecture)
3. [ปัญหาที่วิเคราะห์พบ — Identified Issues](#3-ปัญหาที่วิเคราะห์พบ--identified-issues)
4. [สถาปัตยกรรมใหม่ — Target Architecture](#4-สถาปัตยกรรมใหม่--target-architecture)
5. [Component Design Detail](#5-component-design-detail)
6. [Data Flow Diagrams](#6-data-flow-diagrams)
7. [API Design](#7-api-design)
8. [Named Pipe Protocol & JSON Schema Specification](#8-named-pipe-protocol--json-schema-specification)
9. [Implementation Priority Matrix](#9-implementation-priority-matrix)
10. [ข้อเสนอแนะเพิ่มเติม — Additional Recommendations](#10-ข้อเสนอแนะเพิ่มเติม--additional-recommendations)

---

## 1. บทสรุปผู้บริหาร — Executive Summary

จากการวิเคราะห์ Source Code ทั้ง 12 ไฟล์ พบว่าระบบปัจจุบันมี **6 จุดอ่อนหลัก** ที่ต้องแก้ไขเพื่อรองรับการรัน 20+ จอพร้อมกันและเทรนสองเด็คในเวลาเดียวกัน

### ปัญหาหลักที่พบ:
| # | ปัญหา | ความรุนแรง | ไฟล์ที่เกี่ยวข้อง |
|---|-------|-----------|-----------------|
| 1 | **Race Condition ในการเขียนไฟล์ Registry** — หลายบอทเขียน cards_registry.json พร้อมกัน | 🔴 Critical | BaseCustomExecutor.cs |
| 2 | **ไม่มี Port Reservation** — ชนพอร์ตเมื่อรันหลาย instance | 🔴 Critical | parallel_launcher.py |
| 3 | **LP Monitor ไม่ atomic** — ข้อมูลอาจสูญหายถ้าเกมรีสตาร์ทเร็วเกินไป | 🟡 High | BaseCustomExecutor.cs |
| 4 | **Auto Deploy เป็น Manual** — ต้องกดปุ่มใน cockpit เอง | 🟡 High | cockpit.py |
| 5 | **ไม่มี Dual-Deck Training** — เรียนรู้แค่ฝั่งเดียว | 🟡 Medium | q_learning.py, learning_sandbox.py |
| 6 | **ไม่มี State Recovery** — ถ้า instance crash ข้อมูลสูญหาย | 🟡 Medium | ทุกไฟล์ |

### สถาปัตยกรรมที่นำเสนอแก้ไขทุกปัญหาด้วย:
1. **IgnisOrchestrator** — ตัวกลางจัดการทุก instance
2. **FileLockManager** — ระบบล็อคไฟล์แบบ distributed
3. **Atomic Data Collector** — จับข้อมูลทันทีเมื่อ LP=0 โดยไม่รอ
4. **AutoDeployEngine** — Deploy อัตโนมัติหลัง LP=0
5. **DualDeckTrainer** — เทรนสองเด็คพร้อมกัน

---

## 2. สถาปัตยกรรมปัจจุบัน — Current Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Architecture ปัจจุบัน                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  cockpit.py (Web UI)                                             │
│     │                                                            │
│     ├── spawn_bots_on_port() → subprocess WindBot.exe × 2       │
│     │      (Bot A + Bot B on same port)                         │
│     │                                                            │
│     ├── start_training() → optimize_registry.py / combo_sim.py   │
│     │                                                            │
│     └── deploy_config() → Manual copy + compile_ai.bat           │
│                                                                  │
│  parallel_launcher.py                                            │
│     └── run_headless_parallel() → threading.Thread × instances   │
│           └── run_single_headless_match() → WindBot.exe × 2      │
│                                                                  │
│  run_multi_iterations.py                                         │
│     └── วนลูปรอบ: run_round → save_to_sql → learn → archive     │
│                                                                  │
│  BaseCustomExecutor.cs (C# in WindBot.exe)                       │
│     ├── LoadConfiguration() — โหลด registry + card_names.json    │
│     ├── MonitorLP() — Thread ตรวจ LP ทุก 200ms                  │
│     ├── ApplyRealTimeLearning() — เมื่อ LP=0                     │
│     │     └── SaveConfiguration() — เขียน registry + memory       │
│     └── LogToMatch / LogDecision — เขียนไฟล์ log                  │
│                                                                  │
│  learning_sandbox.py + q_learning.py                             │
│     └── อ่าน match logs → ปรับ priority / Q-values → เขียน registry │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### จุดอ่อนของ Architecture ปัจจุบัน:

1. **No Central Coordinator** — แต่ละ instance (WindBot.exe) เขียนไฟล์ registry ของตัวเองโดยไม่รู้จัก instance อื่น
2. **File Locking แบบง่ายเกินไป** — `_staticLock` ใน C# ล็อคแค่ใน process เดียวกัน ไม่ได้ล็อคข้าม process
3. **SaveConfiguration() เกิด race condition** — Instance A อ่าน → Instance B เขียน → Instance A เขียนทับข้อมูล Instance B
4. **LP Monitor ทำงานคนละ thread** — ถ้า game restart ทันทีหลัง LP=0 อาจเรียก ApplyRealTimeLearning ไม่ทัน
5. **Auto Deploy ต้อง manual** — ไม่มีการ deploy อัตโนมัติเมื่อ registry เปลี่ยน
6. **ไม่แยก deck A/B** — การเรียนรู้ของทั้งสองเด็คถูกบันทึกลง registry เดียวกัน

---

## 3. ปัญหาที่วิเคราะห์พบ — Identified Issues

### 3.1 🔴 Critical: File Race Condition

**ปัญหา:** `BaseCustomExecutor.cs` บรรทัด `SaveConfiguration()` (ประมาณ 445-580) ใช้ `_staticLock` ซึ่งเป็น `static readonly object` ใน CLR — แต่ล็อคนี้ใช้ได้เฉพาะภายใน Process เดียวกันเท่านั้น เมื่อรัน WindBot.exe หลายตัวพร้อมกัน แต่ละ Process มี `_staticLock` ของตัวเอง จึงไม่มีการ synchronize กัน

```csharp
protected static readonly object _staticLock = new object();
// ↑ ใช้งานได้เฉพาะภายใน process เดียวกัน!
```

**ผลกระทบ:** เมื่อ 20 instances เขียน `cards_registry.json` และ `opponent_memory.json` พร้อมกัน:
- ข้อมูลของ instance หนึ่งจะถูกทับโดยอีก instance หนึ่ง
- ไฟล์ registry อาจเสียหาย (corrupt JSON)
- Q-values ที่เรียนรู้มาหายไป

### 3.2 🟡 High: LP Monitor ไม่ Atomic

**ปัญหา:** `MonitorLP()` ทำงานทุก 200ms แต่ `ApplyRealTimeLearning()` ใช้ `_learningApplied` flag เพื่อป้องกันการเรียกซ้ำ — ถ้าเกมจบ (LP=0) แล้ว restart instance ใหม่ (ResetDuelState) ภายใน 200ms เดียวกัน ข้อมูล match ก่อนหน้าอาจไม่ถูกบันทึก

```csharp
protected void MonitorLP() {
    while (!_stopLPMonitor) {
        if (botLP == 0 || oppLP == 0) {
            ApplyRealTimeLearning();  // ใช้อาจไม่ทันถ้า ResetDuelState() ถูกเรียกก่อน
        }
        Thread.Sleep(200);
    }
}
```

### 3.3 🟡 High: Auto Deploy เป็น Manual

**ปัญหา:** `deploy_config()` ใน cockpit.py ถูกเรียกเมื่อผู้ใช้กดปุ่ม Deploy ใน Web UI เท่านั้น ต้อง manual deploy → ทำให้ Sandbox registry กับ Live registry ไม่ sync กัน

### 3.4 🟡 Medium: Dual-Deck Training ไม่มี

**ปัญหา:** 
- `q_learning.py` และ `learning_sandbox.py` รองรับแค่การปรับ priority ของ Deck เดียว (`--deck` parameter)
- `run_multi_iterations.py` รัน `run_match_learning.py --deck <deck>` ทีละฝั่ง
- `parallel_launcher.py` ใช้ deck A และ deck B แต่หลังแมตช์ ไม่มีการเทรนทั้งสองเด็คพร้อมกัน

### 3.5 🟡 Medium: ไม่มี State Recovery

**ปัญหา:** ถ้า instance ไหนเกิด crash (process die) ระหว่าง match:
- ข้อมูล match ที่ยังไม่ได้บันทึกสูญหาย
- ไม่มี checkpoint/rollback
- ไม่มีการ detect และ restart instance ที่ตาย

### 3.6 🟢 Low: Port Conflict Detection

**ปัญหา:** `parallel_launcher.py` ใช้ `start_port + i - 1` โดยไม่ตรวจสอบว่าพอร์ตว่างหรือไม่ ถ้ามีโปรแกรมอื่นใช้พอร์ตนั้นอยู่ จะเกิด socket bind error

---

## 4. สถาปัตยกรรมใหม่ — Target Architecture

```
┌──────────────────────────────────────────────────────────────────────────┐
│                     TARGET ARCHITECTURE — v2.0                           │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    IgnisOrchestrator (Python)                     │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌───────────┐ │   │
│  │  │ PortManager │ │ InstanceMgr │ │ DataCollect │ │ AutoDeploy│ │   │
│  │  │ (Reserve/   │ │ (Spawn/     │ │ (Atomic     │ │ (Deploy on│ │   │
│  │  │  Release)   │ │  Monitor/   │ │  Capture)   │ │  LP=0)    │ │   │
│  │  └─────────────┘ │  Restart)   │ └─────────────┘ └───────────┘ │   │
│  │                  └─────────────┘                                │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌───────────────────────────┐ │   │
│  │  │ FileLockMgr │ │ StateRecov  │ │ DualDeckTrainer           │ │   │
│  │  │ (Distributed│ │ (Crash      │ │ (Train Deck A + B         │ │   │
│  │  │  Lock via   │ │  Recovery)  │ │  Simultaneously)          │ │   │
│  │  │  NamedPipe) │ │             │ │                           │ │   │
│  │  └─────────────┘ └─────────────┘ └───────────────────────────┘ │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     ┌──────────┐ │
│  │ Instance 1   │  │ Instance 2   │  │ Instance 3   │ ... │ Instance │ │
│  │ Port 7911    │  │ Port 7912    │  │ Port 7913    │     │ N        │ │
│  │ Bot A + Bot B│  │ Bot A + Bot B│  │ Bot A + Bot B│     │          │ │
│  └──────────────┘  └──────────────┘  └──────────────┘     └──────────┘ │
│         │                  │                  │                │        │
│         ▼                  ▼                  ▼                ▼        │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Shared Data Layer                             │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐   │   │
│  │  │  Registry DB  │  │  Match DB    │  │  Opponent Memory DB  │   │   │
│  │  │  (SQLite WAL) │  │  (SQLite WAL)│  │  (SQLite WAL)       │   │   │
│  │  └──────────────┘  └──────────────┘  └──────────────────────┘   │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────────────┘
```

### หลักการออกแบบ (Design Principles):

1. **Centralized Orchestration** — IgnisOrchestrator ตัวเดียวจัดการทุก instance
2. **Distributed File Locking** — ใช้ Named Pipes / ไฟล์ล็อคแทน `_staticLock` ที่ใช้跨-process ไม่ได้
3. **Atomic Data Capture** — เมื่อ LP=0 → flush ทันที → unlock → deploy
4. **Dual-Deck Isolation** — แต่ละเด็คมี registry + database ของตัวเอง
5. **Recovery-First** — ทุก instance มี heartbeat + checkpoint

---

## 5. Component Design Detail

### 5.1 IgnisOrchestrator — Central Coordinator

**ไฟล์ใหม่:** `ignis_orchestrator.py`

```
┌─────────────────────────────────────────────────────────────────────┐
│                        IgnisOrchestrator                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  class IgnisOrchestrator:                                            │
│                                                                      │
│      def __init__(self, config: OrchestratorConfig):                 │
│          self.port_manager = PortManager(start_port, max_instances)  │
│          self.instance_manager = InstanceManager()                   │
│          self.data_collector = MatchDataCollector()                  │
│          self.auto_deployer = AutoDeployEngine()                    │
│          self.file_lock = FileLockManager()                         │
│          self.state_recovery = StateRecovery()                      │
│          self.dual_trainer = DualDeckTrainer()                      │
│                                                                      │
│      async def start_session(self, deck_a, deck_b, count=20):       │
│          """เริ่มเทรน 20 จอ พร้อมสองเด็ค"""                           │
│          ports = self.port_manager.reserve_ports(count * 2)         │
│          instances = []                                              │
│          for i in range(count):                                      │
│              inst = Instance(ports[i*2], ports[i*2+1], deck_a, deck_b) │
│              self.instance_manager.spawn(inst)                       │
│              instances.append(inst)                                  │
│          await self.monitor_loop(instances)                         │
│                                                                      │
│      async def monitor_loop(self, instances):                        │
│          while True:                                                 │
│              for inst in instances:                                  │
│                  if inst.is_dead():                                  │
│                      self.state_recovery.restart(inst)              │
│                  if inst.has_lp_zero_event():                        │
│                      data = self.data_collector.capture(inst)        │
│                      self.dual_trainer.ingest(data)                  │
│                      if self.auto_deployer.should_deploy(data):      │
│                          self.auto_deployer.deploy(data.deck)        │
│              await asyncio.sleep(0.05)  # 50ms loop                 │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

**Key Features:**
- REST API: `POST /start`, `GET /status`, `POST /stop`
- WebSocket: real-time streaming of match events (LP=0, deploy, etc.)
- Config file (YAML): กำหนดจำนวน instance, deck, port range, deploy policy

### 5.2 InstanceManager — Lifecycle & Port Management

**ไฟล์ใหม่:** `instance_manager.py`

```
┌─────────────────────────────────────────────────────────────────────┐
│                        InstanceManager                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Port Reservation Algorithm:                                         │
│  ┌─────────────────────────────────────────────────────────────────┐│
│  │ ● ใช้ไฟล์ล็อคกับพอร์ต (~/.ignis_ports.lock)                      ││
│  │ ● Reserve: bind socket → ตรวจสอบว่าพอร์ตว่างจริง                 ││
│  │ ● Release: ปล่อยพอร์ตเมื่อ instance ตาย                           ││
│  │ ● Conflict Resolution: ถ้าพอร์ตถูกใช้ → shift +1               ││
│  └─────────────────────────────────────────────────────────────────┘│
│                                                                      │
│  Instance Lifecycle:                                                 │
│  ┌─────────────────────────────────────────────────────────────────┐│
│  │ CREATED → SPAWNING → RUNNING → [LP=0] → CAPTURING → CONTINUE    ││
│  │                ↓                  ↓                              ││
│  │             CRASHED           FINISHED                           ││
│  │                ↓                  ↓                              ││
│  │             RECOVERY            ARCHIVE                         ││
│  └─────────────────────────────────────────────────────────────────┘│
│                                                                      │
│  Parallel Spawning Strategy:                                         │
│  ● Spawn ทีละ 2 instances (delay 1.5s ระหว่าง pair เหมือนเดิม)      │
│  ● แต่ละ pair ใช้พอร์ตต่างกัน                                          │
│  ● Monitor ด้วย Process.poll() + heartbeat timeout                   │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

**Code Snippet — Port Reservation:**

```python
class PortManager:
    def __init__(self, start_port=7911, max_ports=50):
        self.start_port = start_port
        self.max_ports = max_ports
        self._reserved = set()
        self._lock_file = os.path.expanduser("~/.ignis_port_lock")
    
    def reserve(self, count: int) -> List[int]:
        """Reserve 'count' consecutive available ports"""
        with FileLock(self._lock_file):
            available = self._scan_available_ports()
            ports = []
            for port in available:
                if port not in self._reserved and len(ports) < count:
                    if self._is_port_free(port):
                        self._reserved.add(port)
                        ports.append(port)
            return ports
    
    def _is_port_free(self, port: int) -> bool:
        """Check if port is actually free by trying to bind"""
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                s.bind(("", port))
                return True
        except OSError:
            return False
```

### 5.3 MatchDataCollector — Atomic Data Capture on LP=0

**ไฟล์ใหม่:** `match_data_collector.py`

```
┌─────────────────────────────────────────────────────────────────────┐
│                      MatchDataCollector                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ปัญหาปัจจุบัน: MonitorLP() ใช้ polling ทุก 200ms → อาจพลาด LP=0    │
│                                                                      │
│  วิธีแก้ไข C# Side (BaseCustomExecutor.cs):                          │
│  ┌─────────────────────────────────────────────────────────────────┐│
│  │ ● เพิ่ม Named Pipe Server ในแต่ละ WindBot instance              ││
│  │ ● เมื่อ LP=0 → ส่ง event ผ่าน named pipe ทันที (ไม่ต้องรอ poll)  ││
│  │ ● ส่งข้อมูล match (decisions, outcome, LP, turns) ใน event เดียว││
│  │ ● ใช้ Fire-and-Forget + Retry Queue ถ้า orchestrator ไม่ว่าง    ││
│  └─────────────────────────────────────────────────────────────────┘│
│                                                                      │
│  Data Capture Sequence:                                              │
│  1. LP=0 ถูก detect ใน C# thread                                    │
│  2. C# serialize match data → JSON → ส่งผ่าน Named Pipe             │
│  3. Orchestrator รับ event → ACK ทันที                               │
│  4. Orchestrator เขียนลง Match DB (SQLite WAL)                      │
│  5. C# receive ACK → clear state → รอรอบถัดไป                       │
│                                                                      │
│  Fallback (กรณี Named Pipe ล้มเหลว):                                │
│  ● C# เขียนไฟล์ .matchdata (atomic rename เหมือนใน shared_utils)     │
│  ● Orchestrator ตรวจสอบไฟล์ใหม่ทุก 100ms                            │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

**C# Side — LP=0 Event Emitter (add to BaseCustomExecutor.cs):**

```csharp
// เพิ่ม Named Pipe Client สำหรับส่ง LP=0 event
protected void OnLPZero(string outcome, int botLP, int oppLP, int turns)
{
    var matchData = new
    {
        event_type = "lp_zero",
        deck = _resolvedDeckName,
        outcome,
        bot_lp = botLP,
        opp_lp = oppLP,
        turns,
        timestamp = DateTime.UtcNow.ToString("o"),
        decisions = CollectDecisionsForLastMatch() // serialize ตัดสินใจทั้งหมด
    };
    
    string json = new JavaScriptSerializer().Serialize(matchData);
    
    // ส่งผ่าน Named Pipe (ไม่ต้องรอ response)
    try
    {
        using (var pipe = new NamedPipeClientStream(".", "IgnisLPZeroPipe", PipeDirection.Out))
        {
            pipe.Connect(100); // 100ms timeout
            using (var writer = new StreamWriter(pipe))
            {
                writer.Write(json);
                writer.Flush();
            }
        }
    }
    catch
    {
        // Fallback: เขียนไฟล์ .matchdata ด้วย atomic rename
        string tempFile = Path.Combine(_matchLogDir, $"lpzero_{DateTime.Now.Ticks}.tmp");
        string finalFile = Path.Combine(_matchLogDir, $"lpzero_{DateTime.Now.Ticks}.matchdata");
        File.WriteAllText(tempFile, json);
        File.Move(tempFile, finalFile); // Atomic rename
    }
}
```

### 5.4 AutoDeployEngine — Real-Time Deployment

**ไฟล์ใหม่:** `auto_deploy_engine.py`

```
┌─────────────────────────────────────────────────────────────────────┐
│                       AutoDeployEngine                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Trigger Conditions:                                                 │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ เมื่อ LP=0 และตรงเงื่อนไขใดเงื่อนไขหนึ่ง:                         │ │
│  │                                                                  │ │
│  │ 1. ทุกๆ N ครั้งที่ LP=0 (configurable, default=10)              │ │
│  │    → deploy ครั้งที่ 10, 20, 30, ...                             │ │
│  │                                                                  │ │
│  │ 2. เมื่อ win rate ในช่วง 50 match ล่าสุด > threshold            │ │
│  │    → deploy ทันที (พบทางใหม่ที่เวิร์ค)                            │ │
│  │                                                                  │ │
│  │ 3. เมื่อ Q-value ของ goal ใด goal หนึ่งเปลี่ยนแปลง > threshold  │ │
│  │    → deploy เพื่ออัปเดต live bot                                 │ │
│  │                                                                  │ │
│  │ 4. Manual override via API                                      │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  Deployment Pipeline:                                                │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ 1. Lock Registry (ป้องกันการเขียนระหว่าง deploy)                  │ │
│  │ 2. Copy Sandbox → Live (backup อัตโนมัติ)                       │ │
│  │ 3. รัน compile_ai.bat                                           │ │
│  │ 4. Restart instances ที่ใช้ live registry                        │ │
│  │ 5. Unlock Registry                                              │ │
│  │ 6. Log deployment event                                         │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  Deployment Strategy (สำหรับ 20+ instances):                         │
│  ● Rolling Update: ทีละ 2 instances (ป้องกัน downtime ทั้งหมด)       │
│  ● Blue-Green: รันกลุ่มใหม่ก่อน → สลับ traffic                      │
│  ● Fallback: ถ้า compile fail → rollback ไป version ก่อนหน้า       │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

**Code Snippet — Smart Deploy Decision:**

```python
class AutoDeployEngine:
    def __init__(self, deploy_interval=10, win_rate_threshold=0.60):
        self.deploy_interval = deploy_interval
        self.win_rate_threshold = win_rate_threshold
        self._deploy_counter = defaultdict(int)  # per deck
    
    def should_deploy(self, match_data: MatchData) -> bool:
        deck = match_data.deck_name
        self._deploy_counter[deck] += 1
        
        # Condition 1: Interval-based deploy
        if self._deploy_counter[deck] >= self.deploy_interval:
            self._deploy_counter[deck] = 0
            return True
        
        # Condition 2: Win rate spike (discovered a new strategy)
        recent_matches = self._get_recent_matches(deck, 50)
        if len(recent_matches) >= 20:
            win_rate = sum(1 for m in recent_matches if m.outcome in ("Win", "WeakWin")) / len(recent_matches)
            if win_rate >= self.win_rate_threshold:
                logger.info(f"[AutoDeploy] Win rate spike detected: {win_rate:.1%} for {deck}")
                return True
        
        return False
```

### 5.5 DualDeckTrainer — Two-Deck Simultaneous Training

**ไฟล์ใหม่:** `dual_deck_trainer.py`

```
┌─────────────────────────────────────────────────────────────────────┐
│                       DualDeckTrainer                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ปัญหาปัจจุบัน: q_learning.py และ learning_sandbox.py รองรับแค่     │
│  การเทรนทีละเด็ค (--deck parameter)                                 │
│                                                                      │
│  วิธีแก้ไข:                                                          │
│  ● สร้าง TrainingPipeline ที่แยก Deck A และ Deck B อย่างชัดเจน       │
│  ● แต่ละเด็คมี registry, Q-table, database เป็นของตัวเอง               │
│  ● Match crossover (Deck A ชนะ) → ปรับ priority Deck A ↑, Deck B ↓ │
│  ● ใช้ shared_utils.py เดิม แต่เพิ่ม path isolation                   │
│                                                                      │
│  Training Pipeline:                                                  │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                                                                  │
│  │  [LP=0 Event]                                                    │
│  │       │                                                          │
│  │       ▼                                                          │
│  │  ┌──────────────────────┐                                        │
│  │  │ 1. Classify Match    │ ← Deck A ชนะ? Deck B ชนะ? เสมอ?       │
│  │  └──────────────────────┘                                        │
│  │       │                                                          │
│  │       ▼                                                          │
│  │  ┌────────────────────────────────┐                              │
│  │  │ 2. Update Deck A Registry      │ ← ปรับ priority ของ Deck A  │
│  │  │    + Q-table + Opponent Memory │                              │
│  │  └────────────────────────────────┘                              │
│  │       │                                                          │
│  │       ▼                                                          │
│  │  ┌────────────────────────────────┐                              │
│  │  │ 3. Update Deck B Registry      │ ← ปรับ priority ของ Deck B  │
│  │  │    + Q-table + Opponent Memory │                              │
│  │  └────────────────────────────────┘                              │
│  │       │                                                          │
│  │       ▼                                                          │
│  │  ┌────────────────────────────────────┐                          │
│  │  │ 4. Cross-Learning:                 │                          │
│  │  │    ● Deck A learns about Deck B    │                          │
│  │  │      (opponent memory)             │                          │
│  │  │    ● Deck B learns about Deck A    │                          │
│  │  │      (opponent memory)             │                          │
│  │  └────────────────────────────────────┘                          │
│  │       │                                                          │
│  │       ▼                                                          │
│  │  ┌──────────────────────┐                                        │
│  │  │ 5. Evaluate:         │ ← AutoDeploy?                         │
│  │  │    should_deploy()   │                                        │
│  │  └──────────────────────┘                                        │
│  │                                                                  │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  Registry Isolation:                                                 │
│  ● cards_registry_DeckA.json + cards_registry_DeckB.json             │
│  ● q_table_DeckA.json + q_table_DeckB.json                           │
│  ● opponent_memory_A.json + opponent_memory_B.json                   │
│  ● statistics_A.db + statistics_B.db                                 │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

**Code Snippet — Dual-Deck Training Ingestion:**

```python
class DualDeckTrainer:
    def __init__(self, deck_a: str, deck_b: str):
        self.deck_a = deck_a
        self.deck_b = deck_b
        self.pipeline_a = TrainingPipeline(deck_a)
        self.pipeline_b = TrainingPipeline(deck_b)
    
    def ingest(self, match: MatchData):
        """Ingest a match result into both decks' training pipelines"""
        
        # Step 1: Classify who won
        if match.bot_name == "IgnisBot_A":
            winner_deck = self.deck_a
            loser_deck = self.deck_b
        else:
            winner_deck = self.deck_b
            loser_deck = self.deck_a
        
        # Step 2 & 3: Update both registries in parallel
        with ThreadPoolExecutor(max_workers=2) as executor:
            f_a = executor.submit(self.pipeline_a.update, match, winner_deck, loser_deck)
            f_b = executor.submit(self.pipeline_b.update, match, loser_deck, winner_deck)
            f_a.result()
            f_b.result()
        
        # Step 4: Cross-learning (opponent memory)
        self._cross_learn(match)
        
        # Step 5: Auto-deploy evaluation
        if self.auto_deploy.should_deploy(match):
            self.auto_deploy.deploy(winner_deck)
    
    def _cross_learn(self, match: MatchData):
        """Both decks remember each other's cards and patterns"""
        for card_id, card_info in match.opponent_cards_played.items():
            # Deck A learns about Deck B's cards
            self.pipeline_a.update_opponent_memory(card_id, card_info)
            # Deck B learns about Deck A's cards  
            self.pipeline_b.update_opponent_memory(card_id, card_info)
```

### 5.6 FileLockManager — Race Condition Prevention

**ไฟล์ใหม่:** `file_lock_manager.py`

```
┌─────────────────────────────────────────────────────────────────────┐
│                      FileLockManager                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ปัญหาปัจจุบัน: _staticLock ใน C# ไม่สามารถ跨-process ได้            │
│                                                                      │
│  Solution: Hybrid Locking Strategy                                   │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                                                                  │
│  │  Level 1: File-based Advisory Lock                               │
│  │  ● ใช้ .lock ไฟล์สำหรับแต่ละ resource                              │
│  │  ● ใช้ os.open() with O_CREAT | O_EXCL (atomic)                  │
│  │  ● Timeout: 5 seconds แล้ว retry                                 │
│  │                                                                  │
│  │  Level 2: Named Pipe (Windows)                                   │
│  │  ● C# side สร้าง NamedPipeServerStream                           │
│  │  ● Python orchestrator เป็น client                               │
│  │  ● ใช้สำหรับส่ง event LP=0 + รับ ACK                             │
│  │                                                                  │
│  │  Level 3: SQLite WAL Mode                                        │
│  │  ● เปลี่ยน Registry จาก JSON → SQLite (WAL mode)                │
│  │  ● SQLite รองรับ concurrent read/write ได้ดีกว่า JSON            │
│  │  ● ใช้ INSERT OR REPLACE สำหรับอัปเดต                            │
│  │                                                                  │
│  │  Level 4: Redis (Optional สำหรับ 50+ instances)                 │
│  │  ● ใช้ Redis hash สำหรับ real-time registry sync                 │
│  │  ● Pub/Sub สำหรับ LP=0 events                                    │
│  │                                                                  │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  Implementation Priority:  Level 1 → Level 3 (SQLite) → Level 4      │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

**Code Snippet — File-based Advisory Lock (Python):**

```python
class FileLock:
    """Cross-process advisory file lock using atomic file creation"""
    
    def __init__(self, lock_path: str, timeout: float = 5.0):
        self.lock_path = lock_path
        self.timeout = timeout
        self._fd = None
    
    def __enter__(self):
        start = time.time()
        while True:
            try:
                # O_CREAT | O_EXCL = atomic create (fails if exists)
                fd = os.open(self.lock_path, os.O_CREAT | os.O_EXCL | os.O_RDWR)
                self._fd = fd
                # Write PID for debugging
                os.write(fd, str(os.getpid()).encode())
                return self
            except FileExistsError:
                if time.time() - start > self.timeout:
                    # Stale lock check
                    if self._is_stale():
                        os.remove(self.lock_path)
                        continue
                    raise TimeoutError(f"Could not acquire lock: {self.lock_path}")
                time.sleep(0.05)
    
    def __exit__(self, *args):
        if self._fd:
            os.close(self._fd)
            os.remove(self.lock_path)
    
    def _is_stale(self) -> bool:
        """Check if the process holding the lock is still alive"""
        try:
            with open(self.lock_path, 'r') as f:
                pid = int(f.read().strip())
            # Check if process exists (Windows)
            import ctypes
            kernel32 = ctypes.windll.kernel32
            handle = kernel32.OpenProcess(0x400, False, pid)
            if handle:
                kernel32.CloseHandle(handle)
                return False  # Process still alive
            return True  # Process dead → stale lock
        except:
            return True
```

### 5.7 StateRecovery — Recovery After Crash

**ไฟล์ใหม่:** `state_recovery.py`

```
┌─────────────────────────────────────────────────────────────────────┐
│                        StateRecovery                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Heartbeat System:                                                   │
│  ● แต่ละ instance ส่ง heartbeat ทุก 1 วินาที                         │
│  ● Orchestrator ตรวจจับถ้าไม่มี heartbeat เกิน 5 วินาที → ถือว่าตาย   │
│  ● Restart instance ด้วยพอร์ตเดิม                                    │
│                                                                      │
│  Checkpoint System:                                                  │
│  ● ทุก turn จบ → C# เขียน checkpoint ไฟล์                            │
│  ● checkpoint: turn number + state summary                           │
│  ● ถ้า instance ตาย → restart → โหลด checkpoint ล่าสุด                │
│  ● ถ้า checkpoint เก่ากว่า 30 วินาที → เริ่ม match ใหม่               │
│                                                                      │
│  Crash Recovery Flow:                                                │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                                                                  │
│  │  1. Detect: No heartbeat > 5s                                   │
│  │  2. Kill: force kill process if zombie                          │
│  │  3. Collect: อ่าน checkpoint + partial match data                │
│  │  4. Restart: spawn instance ใหม่ด้วย config เดิม                   │
│  │  5. Resume: ถ้ามี checkpoint → เล่นต่อ, ถ้าไม่มี → match ใหม่     │
│  │  6. Report: log crash + recovery time                           │
│  │                                                                  │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 6. Data Flow Diagrams

### 6.1 LP=0 → Capture → Deploy Flow

```
┌──────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────┐
│ C# Bot A │    │ Orchestrator │    │ DualDeck     │    │ Auto     │
│(Instance)│    │ (Python)     │    │ Trainer      │    │ Deployer │
└────┬─────┘    └──────┬───────┘    └──────┬───────┘    └────┬─────┘
     │                  │                   │                │
     │ LP=0 detected    │                   │                │
     │═════════════════►│                   │                │
     │                  │                   │                │
     │ [Named Pipe]     │                   │                │
     │ {"event":"lp_zero",                  │                │
     │  "deck":"DeckA",                     │                │
     │  "outcome":"Win",                    │                │
     │  "bot_lp":0,"opp_lp":0,             │                │
     │  "turns":5,                          │                │
     │  "decisions":[...]}                  │                │
     │                  │                   │                │
     │     ACK ◄════════╛                   │                │
     │                  │                   │                │
     │ [C# clears state,                   │                │
     │  ready for next match]              │                │
     │                  │                   │                │
     │                  │  ingest(data)     │                │
     │                  │══════════════════►│                │
     │                  │                   │                │
     │                  │                   │ Update Deck A  │
     │                  │                   │ registry + Q   │
     │                  │                   │ Update Deck B  │
     │                  │                   │ registry + Q   │
     │                  │                   │ Cross-learn    │
     │                  │                   │                │
     │                  │                   │ should_deploy? │
     │                  │                   │════════════════►│
     │                  │                   │                │
     │                  │                   │    True        │
     │                  │                   │◄════════════════╛
     │                  │                   │                │
     │                  │  deploy(DeckA)    │                │
     │                  │◄══════════════════╛                │
     │                  │                                   │
     │                  │  1. Lock registry                  │
     │                  │  2. Sandbox → Live                 │
     │                  │  3. backup + compile               │
     │                  │  4. Rolling restart                │
     │                  │  5. Unlock registry                │
     │                  │                                   │
     │  restart ◄══════╛                                   │
     │  (new registry)                                      │
     │                  │                                   │
     ▼                  ▼                   ▼               ▼
```

### 6.2 Multi-Instance Startup Sequence

```
Time ──────────────────────────────────────────────────────────────►

Orchestrator:
  │   Init PortManager
  │   Reserve 20 ports
  │
  ├── Spawn Instance 1 (Port 7911-7912)
  │   │
  │   ├── Spawn Instance 2 (Port 7913-7914)  [delay 1.5s]
  │   │
  │   ├── Spawn Instance 3 (Port 7915-7916)  [delay 1.5s]
  │   │
  │   └── ... (until 20 instances)
  │
  │   Start monitor_loop()  [50ms interval]
  │
  │   ┌──────────────────────────────────────────────┐
  │   │  Monitor Loop:                                │
  │   │  for each instance:                           │
  │   │    check heartbeat                           │
  │   │    check LP=0 event                          │
  │   │    check file lock queue                     │
  │   │    sleep(50ms)                               │
  │   └──────────────────────────────────────────────┘

Instance 1:
  │
  ├── Bot A (WindBot.exe) — Deck A
  │   │  Load cards_registry_DeckA.json
  │   │  Connect to port 7911
  │   │  Start MonitorLP thread
  │   │  Start Heartbeat thread [every 1s]
  │   │
  │   └── Bot B (WindBot.exe) — Deck B
  │       │  Load cards_registry_DeckB.json
  │       │  Connect to port 7912
  │       │  Start MonitorLP thread
  │       │  Start Heartbeat thread [every 1s]
  │
  │   Match begins...
  │   [LP=0 detected] ──► Send event to Orchestrator
  │   [Game restart]    ──► ResetDuelState()
  │   [Next match]      ──► loop
```

### 6.3 Dual-Deck Training Loop

```
┌─────────────────────────────────────────────────────────────────────┐
│                  DUAL-DECK TRAINING LOOP                            │
│                                                                      │
│  Instance 1:                   Instance 2:          ... Instance N: │
│  ┌───────────────────┐        ┌──────────────────┐    ┌───────────┐ │
│  │ Deck A vs Deck B  │        │ Deck A vs Deck B │    │ Deck A vs │ │
│  │ (Port 7911-7912)  │        │ (Port 7913-7914) │    │ Deck B    │ │
│  └────────┬──────────┘        └────────┬─────────┘    └─────┬─────┘ │
│           │                            │                    │       │
│           ▼                            ▼                    ▼       │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    Match Result Pool                         │   │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐          │   │
│  │  │ Win A   │ │ Win B   │ │ Win A   │ │ Draw    │  ...     │   │
│  │  │ LP:0,0  │ │ LP:0,0  │ │ LP:0,0  │ │ LP:0,0  │          │   │
│  │  └─────────┘ └─────────┘ └─────────┘ └─────────┘          │   │
│  └──────────────────────────────────────────────────────────────┘   │
│           │                            │                    │       │
│           ▼                            ▼                    ▼       │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │              Training Pipeline (ทุก 10 events หรือทุก 1s)    │   │
│  │                                                                  │
│  │  1. Batch: collect last N events from pool                      │
│  │  2. Win Rate Analysis:                                          │
│  │     Deck A: 65% win rate (last 100 matches)                     │
│  │     Deck B: 35% win rate (last 100 matches)                    │
│  │  3. Update Deck A: priority adjustments (learning_sandbox)       │
│  │     + Q-learning update                                          │
│  │  4. Update Deck B: priority adjustments (learning_sandbox)       │
│  │     + Q-learning update                                          │
│  │  5. AutoDeploy Check                                            │
│  │     → ถ้า Deck A win rate > 60% → deploy A                      │
│  │     → ถ้า Deck B win rate < 20% → deploy B (need improvement)  │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 7. API Design

### 7.1 IgnisOrchestrator REST API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/session/start` | เริ่ม training session ใหม่ |
| `POST` | `/api/v1/session/stop` | หยุด session |
| `GET`  | `/api/v1/session/status` | ดูสถานะ session ปัจจุบัน |
| `GET`  | `/api/v1/instances` | รายการ instances ทั้งหมด + สถานะ |
| `GET`  | `/api/v1/instances/{id}` | ดู instance เดียว |
| `POST` | `/api/v1/instances/{id}/restart` | Restart instance |
| `GET`  | `/api/v1/stats` | สถิติรวม (win rate, matches, ฯลฯ) |
| `GET`  | `/api/v1/stats/{deck}` | สถิติแยกตามเด็ค |
| `POST` | `/api/v1/deploy` | Force deploy |
| `GET`  | `/api/v1/deploy/history` | ประวัติการ deploy |
| `GET`  | `/api/v1/ports` | ดูพอร์ตที่ถูกใช้ |
| `GET`  | `/api/v1/health` | Health check |

### 7.2 WebSocket Events

| Event | Direction | Description |
|-------|-----------|-------------|
| `lp_zero` | Instance → Orchestrator | LP=0 event with match data |
| `ack` | Orchestrator → Instance | Confirm data received |
| `heartbeat` | Instance → Orchestrator | Keep-alive (every 1s) |
| `deploy_start` | Orchestrator → All | กำลัง deploy |
| `deploy_complete` | Orchestrator → All | Deploy เสร็จ |
| `restart` | Orchestrator → Instance | สั่ง restart |

### 7.3 Configuration File (YAML)

```yaml
# ignis_config.yaml

session:
  deck_a: "2026_Dreadnought"
  deck_b: "2026_PureYummy"
  instance_count: 20
  start_port: 7911
  
training:
  learning_rate: 0.1      # alpha (q_learning)
  discount_factor: 0.9    # gamma (q_learning)
  batch_size: 50           # matches per training batch
  cross_learn: true        # both decks learn from each other
  
auto_deploy:
  enabled: true
  interval: 10             # deploy every N LP=0 events
  win_rate_threshold: 0.60 # deploy if win rate > 60%
  rolling_restart: true    # restart instances one by one
  
recovery:
  heartbeat_timeout: 5     # seconds
  max_restart_attempts: 3
  checkpoint_enabled: true
  
logging:
  level: "INFO"
  match_logs: "./logs/matches/"
  deploy_logs: "./logs/deploys/"
  crash_logs: "./logs/crashes/"
```

---

## 8. Named Pipe Protocol & JSON Schema Specification

### 8.1 Protocol Overview

Named Pipe Protocol ใช้สำหรับการสื่อสารแบบ Real-time ระหว่าง **C# WindBot Instance (Server)** และ **Python IgnisOrchestrator (Client)**

#### 8.1.1 Topology

```
┌─────────────────────────────────────────────────────────────────────┐
│                    NAMED PIPE TOPOLOGY                              │
│                                                                      │
│  Orchestra (Python)                    Instances (C#)               │
│  ┌──────────────────────┐            ┌──────────────────────────┐  │
│  │                      │            │  Instance 1              │  │
│  │  NamedPipeClient     │◄──────────►│  ├── NamedPipeServer     │  │
│  │  (reads all pipes)   │            │  │  Pipe: Ignis_7911     │  │
│  │                      │            │  │  PID: 1234            │  │
│  │  ┌────────────────┐  │            │  └──────────────────────────┘  │
│  │  │ Connection Pool│  │            ┌──────────────────────────┐  │
│  │  │  Dict:         │  │            │  Instance 2              │  │
│  │  │  pid → pipe    │  │◄──────────►│  ├── NamedPipeServer     │  │
│  │  └────────────────┘  │            │  │  Pipe: Ignis_7913     │  │
│  │                      │            │  │  PID: 5678            │  │
│  │  ┌────────────────┐  │            │  └──────────────────────────┘  │
│  │  │  Async Reader   │  │            ┌──────────────────────────┐  │
│  │  │  (asyncio)     │  │            │  Instance N              │  │
│  │  └────────────────┘  │◄──────────►│  ├── NamedPipeServer     │  │
│  │                      │            │  │  Pipe: Ignis_791N     │  │
│  └──────────────────────┘            │  │  PID: 9012            │  │
│                                       │  └──────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

#### 8.1.2 Pipe Naming Convention

```
ชื่อ Pipe: Ignis_{InstancePort}_{BotName}

ตัวอย่าง:
  Ignis_7911_BotA   ← Deck A on port 7911
  Ignis_7912_BotB   ← Deck B on port 7912
  Ignis_7913_BotA   ← Deck A on port 7913
  Ignis_7914_BotB   ← Deck B on port 7914

รูปแบบ: Ignis_{port}_{bot_name}
```

**เหตุผลที่ใช้ port number:** ทำให้ Orchestrator สามารถ map pipe ไปยัง instance ได้ทันทีโดยไม่ต้องส่ง metadata เพิ่มเติม

#### 8.1.3 Message Framing Protocol

ใช้ **Length-Prefixed JSON** เพื่อป้องกัน partial read ใน Named Pipe:

```
┌─────────────────────────────────────────────────────────────────────┐
│                    MESSAGE FRAME FORMAT                             │
│                                                                      │
│  ┌─────────┬────────────────────────────────────────────────────┐   │
│  │ 4 bytes │                 N bytes                            │   │
│  │(UInt32) │                JSON UTF-8                         │   │
│  ├─────────┼────────────────────────────────────────────────────┤   │
│  │ Length  │              Message Payload                      │   │
│  │   = N   │  {"event_type":"lp_zero","deck":"...", ...}       │   │
│  └─────────┴────────────────────────────────────────────────────┘   │
│                                                                      │
│  Maximum message size: 1 MB (1048576 bytes)                         │
│  Timeout: 5 seconds read/write                                      │
│  Encoding: UTF-8 (no BOM)                                           │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

**C# Writer Implementation:**

```csharp
public static class NamedPipeProtocol
{
    private const int MAX_MESSAGE_SIZE = 1_048_576; // 1 MB
    private static readonly Encoding Encoding = new UTF8Encoding(false); // no BOM

    public static void WriteMessage(PipeStream pipe, string json)
    {
        byte[] payload = Encoding.GetBytes(json);
        if (payload.Length > MAX_MESSAGE_SIZE)
            throw new InvalidOperationException($"Message too large: {payload.Length} bytes");

        byte[] header = BitConverter.GetBytes((uint)payload.Length);
        // Little-endian (Windows native)
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(header);

        pipe.Write(header, 0, 4);
        pipe.Write(payload, 0, payload.Length);
        pipe.Flush();
    }

    public static string ReadMessage(PipeStream pipe)
    {
        // Read 4-byte header
        byte[] header = new byte[4];
        int bytesRead = 0;
        while (bytesRead < 4)
        {
            int n = pipe.Read(header, bytesRead, 4 - bytesRead);
            if (n == 0) throw new EndOfStreamException("Pipe closed");
            bytesRead += n;
        }

        if (!BitConverter.IsLittleEndian)
            Array.Reverse(header);

        uint length = BitConverter.ToUInt32(header, 0);
        if (length > MAX_MESSAGE_SIZE)
            throw new InvalidDataException($"Message length {length} exceeds maximum");

        // Read payload
        byte[] payload = new byte[length];
        bytesRead = 0;
        while (bytesRead < length)
        {
            int n = pipe.Read(payload, bytesRead, (int)length - bytesRead);
            if (n == 0) throw new EndOfStreamException("Pipe closed");
            bytesRead += n;
        }

        return Encoding.GetString(payload);
    }
}
```

**Python Reader Implementation:**

```python
import struct
import asyncio

class NamedPipeProtocol:
    HEADER_FORMAT = '<I'  # Little-endian UInt32
    HEADER_SIZE = 4
    MAX_MESSAGE_SIZE = 1_048_576
    ENCODING = 'utf-8'
    
    @staticmethod
    async def read_message(reader: asyncio.StreamReader) -> dict:
        """Read one length-prefixed JSON message from the pipe."""
        header = await reader.readexactly(4)
        length = struct.unpack('<I', header)[0]
        
        if length > NamedPipeProtocol.MAX_MESSAGE_SIZE:
            raise ValueError(f"Message too large: {length} bytes")
        
        payload = await reader.readexactly(length)
        json_str = payload.decode(ENCODING)
        return json.loads(json_str)
    
    @staticmethod
    def write_message(writer: asyncio.StreamWriter, data: dict):
        """Write one length-prefixed JSON message to the pipe."""
        json_str = json.dumps(data, ensure_ascii=False)
        payload = json_str.encode(ENCODING)
        
        if len(payload) > NamedPipeProtocol.MAX_MESSAGE_SIZE:
            raise ValueError(f"Message too large: {len(payload)} bytes")
        
        header = struct.pack('<I', len(payload))
        writer.write(header + payload)
```

### 8.2 Message Types & JSON Schema

#### 8.2.1 Message Type Registry

| Message Type | Direction | Priority | Schema Version | Description |
|-------------|-----------|----------|----------------|-------------|
| `lp_zero` | C# → Python | HIGH | 1.0 | LP=0 detected, match ended |
| `ack` | Python → C# | HIGH | 1.0 | Acknowledge receipt of event |
| `heartbeat` | C# → Python | LOW | 1.0 | Keep-alive signal |
| `heartbeat_ack` | Python → C# | LOW | 1.0 | Heartbeat acknowledgment |
| `checkpoint` | C# → Python | MEDIUM | 1.0 | Turn checkpoint for crash recovery |
| `deploy_cmd` | Python → C# | HIGH | 1.0 | Command to restart with new registry |
| `error` | Bidirectional | HIGH | 1.0 | Error notification |
| `shutdown` | Python → C# | HIGH | 1.0 | Graceful shutdown command |

#### 8.2.2 JSON Schema: `lp_zero` (LP=0 Event)

นี่คือ Schema หลักที่สำคัญที่สุด — ใช้เมื่อบอทตรวจพบว่า LP ของฝ่ายใดฝ่ายหนึ่งเป็น 0

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "https://ignis.orchestrator/schemas/lp_zero.v1.json",
  "title": "LPZeroEvent",
  "description": "Event emitted when either bot's LP reaches 0 in a duel match. This is the primary data capture event that triggers training and auto-deploy.",
  "type": "object",
  "required": [
    "event_type",
    "event_version",
    "event_id",
    "timestamp",
    "instance_id",
    "deck",
    "bot_name",
    "outcome",
    "bot_lp",
    "opp_lp",
    "turns",
    "match_duration_ms",
    "decisions"
  ],
  "properties": {
    "event_type": {
      "type": "string",
      "enum": ["lp_zero"],
      "description": "Must be 'lp_zero' to identify this event type."
    },
    "event_version": {
      "type": "string",
      "pattern": "^\\d+\\.\\d+$",
      "example": "1.0",
      "description": "Schema version for forward compatibility."
    },
    "event_id": {
      "type": "string",
      "pattern": "^[a-f0-9]{8}-[a-f0-9]{4}-4[a-f0-9]{3}-[89ab][a-f0-9]{3}-[a-f0-9]{12}$",
      "description": "UUID v4 unique identifier for deduplication. Prevents the same match from being processed twice."
    },
    "timestamp": {
      "type": "string",
      "format": "date-time",
      "pattern": "^\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d+Z$",
      "description": "ISO 8601 UTC timestamp of when LP=0 was detected."
    },
    "instance_id": {
      "type": "integer",
      "minimum": 1,
      "maximum": 100,
      "description": "Orchestrator-assigned instance number (1-20 for typical 20-instance setup)."
    },
    "deck": {
      "type": "string",
      "minLength": 1,
      "maxLength": 64,
      "pattern": "^2026_[A-Za-z]+$",
      "example": "2026_Dreadnought",
      "description": "Name of the deck that this bot was using."
    },
    "bot_name": {
      "type": "string",
      "enum": ["IgnisBot_A", "IgnisBot_B"],
      "description": "Which bot in the instance detected LP=0. 'IgnisBot_A' = Host/Player bot, 'IgnisBot_B' = Opponent/Client bot."
    },
    "outcome": {
      "type": "string",
      "enum": ["Win", "WeakWin", "Draw", "WeakLoss", "Loss"],
      "description": "Match result from the perspective of the deck specified in the 'deck' field."
    },
    "bot_lp": {
      "type": "integer",
      "minimum": 0,
      "maximum": 80000,
      "description": "Life Points of the bot (the deck that emitted this event). 0 means this bot lost."
    },
    "opp_lp": {
      "type": "integer",
      "minimum": 0,
      "maximum": 80000,
      "description": "Life Points of the opponent. 0 means this bot won."
    },
    "lp_differential": {
      "type": "integer",
      "description": "bot_lp - opp_lp. Positive means bot had more LP. Used for WeakWin/WeakLoss determination."
    },
    "turns": {
      "type": "integer",
      "minimum": 0,
      "maximum": 255,
      "description": "Number of turns played in the match. 0 means the match ended before any turn completed (FTK/OTK at turn start)."
    },
    "match_duration_ms": {
      "type": "integer",
      "minimum": 0,
      "maximum": 1800000,
      "description": "Real-time duration of the match in milliseconds. Useful for performance monitoring."
    },
    "start_timestamp": {
      "type": "string",
      "format": "date-time",
      "description": "ISO 8601 UTC timestamp of when the match started."
    },
    "decisions": {
      "type": "array",
      "minItems": 0,
      "maxItems": 1000,
      "description": "Array of all decisions made during the match, in chronological order.",
      "items": {
        "$ref": "#/definitions/Decision"
      }
    },
    "disrupted_cards": {
      "type": "array",
      "description": "Card IDs that were disrupted (negated) by the opponent during the match.",
      "items": {
        "type": "integer",
        "minimum": 0
      }
    },
    "choke_points_triggered": {
      "type": "array",
      "description": "Deck-specific choke point card IDs that were disrupted by the opponent.",
      "items": {
        "type": "integer",
        "minimum": 0
      }
    },
    "opening_hand": {
      "type": "array",
      "maxItems": 5,
      "description": "Card IDs of the bot's opening hand (5 cards).",
      "items": {
        "type": "integer",
        "minimum": 0
      }
    },
    "deck_played_ids": {
      "type": "array",
      "description": "All card IDs that were played or activated by the bot during this match.",
      "items": { "$ref": "#/definitions/PlayedCard" }
    }
  },
  "definitions": {
    "Decision": {
      "type": "object",
      "required": ["turn", "card_id", "card_name", "action", "goal", "score", "decision"],
      "properties": {
        "turn": {
          "type": "integer",
          "minimum": 0,
          "description": "Turn number when this decision was evaluated."
        },
        "card_id": {
          "type": "integer",
          "minimum": 0,
          "description": "OCG card ID of the card being evaluated."
        },
        "card_name": {
          "type": "string",
          "description": "Human-readable card name."
        },
        "action": {
          "type": "string",
          "enum": ["Activate", "Summon", "SpSummon", "Set", "SpellSet", "Repos", "MonsterSet"],
          "description": "Type of action being evaluated."
        },
        "goal": {
          "type": "string",
          "enum": ["establish_interruptions", "push_lethal", "survive", "break_board", "draw", "search"],
          "description": "Current strategic goal when this decision was made."
        },
        "score": {
          "type": "number",
          "minimum": -1000,
          "maximum": 1000,
          "description": "Calculated score for this action. Higher = more likely to be chosen."
        },
        "decision": {
          "type": "boolean",
          "description": "True if the bot chose to perform this action, False if it declined."
        },
        "plan": {
          "type": "string",
          "default": "PlanA",
          "description": "Combo plan being executed (e.g. 'PlanA', 'PlanB')."
        },
        "lp_self": {
          "type": "integer",
          "description": "Bot's LP at the time of this decision."
        },
        "lp_opp": {
          "type": "integer",
          "description": "Opponent's LP at the time of this decision."
        },
        "opponent_threat": {
          "type": "number",
          "description": "Calculated threat level of opponent's board at this decision point."
        }
      }
    },
    "PlayedCard": {
      "type": "object",
      "required": ["card_id"],
      "properties": {
        "card_id": {
          "type": "integer",
          "minimum": 0
        },
        "count": {
          "type": "integer",
          "minimum": 1,
          "maximum": 3,
          "description": "How many times this card was played in the match."
        }
      }
    }
  },
  "examples": [
    {
      "event_type": "lp_zero",
      "event_version": "1.0",
      "event_id": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
      "timestamp": "2026-05-25T14:30:00.123Z",
      "instance_id": 1,
      "deck": "2026_Dreadnought",
      "bot_name": "IgnisBot_A",
      "outcome": "Win",
      "bot_lp": 8000,
      "opp_lp": 0,
      "lp_differential": 8000,
      "turns": 4,
      "match_duration_ms": 45000,
      "start_timestamp": "2026-05-25T14:29:15.000Z",
      "decisions": [
        {
          "turn": 1,
          "card_id": 73628505,
          "card_name": "Terraforming",
          "action": "Activate",
          "goal": "establish_interruptions",
          "score": 85.0,
          "decision": true,
          "plan": "PlanA",
          "lp_self": 8000,
          "lp_opp": 8000,
          "opponent_threat": 5.0
        },
        {
          "turn": 1,
          "card_id": 101402062,
          "card_name": "Dark City Field",
          "action": "Activate",
          "goal": "establish_interruptions",
          "score": 90.0,
          "decision": true,
          "plan": "PlanA",
          "lp_self": 8000,
          "lp_opp": 8000,
          "opponent_threat": 5.0
        }
      ],
      "disrupted_cards": [],
      "choke_points_triggered": [],
      "opening_hand": [73628505, 101402062, 101402023, 24094653, 50720316],
      "deck_played_ids": [
        {"card_id": 73628505, "count": 1},
        {"card_id": 101402062, "count": 1},
        {"card_id": 101402023, "count": 2},
        {"card_id": 24094653, "count": 1}
      ]
    }
  ]
}
```

#### 8.2.3 JSON Schema: `ack` (Acknowledgement)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "https://ignis.orchestrator/schemas/ack.v1.json",
  "title": "Acknowledgement",
  "description": "Sent by Orchestrator to confirm receipt of an event. Critical for the C# side to know it can safely clear state and reset for next match.",
  "type": "object",
  "required": ["event_type", "ack_for_event_id", "status", "timestamp"],
  "properties": {
    "event_type": {
      "type": "string",
      "enum": ["ack"]
    },
    "ack_for_event_id": {
      "type": "string",
      "description": "The event_id of the event being acknowledged (copied from the original lp_zero event)."
    },
    "status": {
      "type": "string",
      "enum": ["received", "stored", "duplicate"],
      "description": "'received' = data captured in memory, 'stored' = written to DB, 'duplicate' = already processed."
    },
    "timestamp": {
      "type": "string",
      "format": "date-time",
      "description": "ISO 8601 UTC timestamp of acknowledgment."
    },
    "orchestrator_pid": {
      "type": "integer",
      "description": "PID of orchestrator process for debugging."
    }
  }
}
```

#### 8.2.4 JSON Schema: `heartbeat` & `heartbeat_ack`

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "https://ignis.orchestrator/schemas/heartbeat.v1.json",
  "title": "Heartbeat",
  "description": "Periodic keep-alive signal from C# instance to Python orchestrator. Orchestrator uses this to detect crashed instances.",
  "type": "object",
  "required": ["event_type", "instance_id", "pid", "sequence", "timestamp"],
  "properties": {
    "event_type": {
      "type": "string",
      "enum": ["heartbeat", "heartbeat_ack"]
    },
    "instance_id": {
      "type": "integer",
      "description": "Instance number assigned by orchestrator."
    },
    "pid": {
      "type": "integer",
      "description": "OS Process ID of this instance."
    },
    "sequence": {
      "type": "integer",
      "minimum": 0,
      "description": "Monotonically increasing heartbeat sequence number. Used to detect gaps (missed heartbeats)."
    },
    "timestamp": {
      "type": "string",
      "format": "date-time",
      "description": "ISO 8601 UTC timestamp."
    },
    "status": {
      "type": "object",
      "properties": {
        "current_match_turns": {
          "type": "integer",
          "description": "Current turn number of the ongoing match (0 if idle)."
        },
        "matches_completed": {
          "type": "integer",
          "description": "Total matches completed by this instance since spawn."
        },
        "memory_mb": {
          "type": "number",
          "description": "Approximate memory usage of this process in MB."
        }
      }
    }
  }
}
```

#### 8.2.5 JSON Schema: `checkpoint` (Crash Recovery)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "https://ignis.orchestrator/schemas/checkpoint.v1.json",
  "title": "TurnCheckpoint",
  "description": "Sent every turn completion. Used for crash recovery — allows resuming from last known good state.",
  "type": "object",
  "required": ["event_type", "instance_id", "turn", "decisions_count", "timestamp"],
  "properties": {
    "event_type": {
      "type": "string",
      "enum": ["checkpoint"]
    },
    "instance_id": {
      "type": "integer"
    },
    "turn": {
      "type": "integer",
      "minimum": 0,
      "description": "The turn number that just completed."
    },
    "decisions_count": {
      "type": "integer",
      "minimum": 0,
      "description": "Number of decisions logged up to this checkpoint."
    },
    "bot_lp": {
      "type": "integer",
      "description": "Bot's LP at this checkpoint."
    },
    "opp_lp": {
      "type": "integer",
      "description": "Opponent's LP at this checkpoint."
    },
    "timestamp": {
      "type": "string",
      "format": "date-time"
    }
  }
}
```

#### 8.2.6 JSON Schema: `deploy_cmd` (Deploy Command)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "https://ignis.orchestrator/schemas/deploy_cmd.v1.json",
  "title": "DeployCommand",
  "description": "Sent by Orchestrator to instruct an instance to restart with a new registry after deployment.",
  "type": "object",
  "required": ["event_type", "deck", "deploy_id", "new_registry_path", "timestamp"],
  "properties": {
    "event_type": {
      "type": "string",
      "enum": ["deploy_cmd"]
    },
    "deck": {
      "type": "string",
      "description": "Deck name that was deployed."
    },
    "deploy_id": {
      "type": "string",
      "pattern": "^deploy_[a-f0-9]{8}$",
      "description": "Unique deploy ID for tracking."
    },
    "new_registry_path": {
      "type": "string",
      "description": "Absolute path to the newly deployed registry file."
    },
    "compile_success": {
      "type": "boolean",
      "description": "Whether the AI compilation was successful."
    },
    "restart_delay_ms": {
      "type": "integer",
      "default": 2000,
      "description": "Delay before restarting to allow other instances to finish writing."
    },
    "timestamp": {
      "type": "string",
      "format": "date-time"
    }
  }
}
```

#### 8.2.7 JSON Schema: `error` (Error Notification)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "https://ignis.orchestrator/schemas/error.v1.json",
  "title": "ErrorEvent",
  "description": "Sent by either side when an error condition is detected.",
  "type": "object",
  "required": ["event_type", "severity", "code", "message", "timestamp"],
  "properties": {
    "event_type": {
      "type": "string",
      "enum": ["error"]
    },
    "severity": {
      "type": "string",
      "enum": ["warning", "error", "fatal"],
      "description": "'warning' = non-critical, 'error' = operation failed, 'fatal' = instance will terminate."
    },
    "code": {
      "type": "string",
      "enum": [
        "PIPE_TIMEOUT",
        "PIPE_DISCONNECTED",
        "WRITE_FAILED",
        "READ_FAILED",
        "SCHEMA_VALIDATION_FAILED",
        "DUPLICATE_EVENT",
        "MEMORY_LOW",
        "MATCH_ABORTED",
        "UNKNOWN"
      ],
      "description": "Machine-readable error code."
    },
    "message": {
      "type": "string",
      "maxLength": 512,
      "description": "Human-readable error description."
    },
    "details": {
      "type": "object",
      "description": "Optional additional error context."
    },
    "timestamp": {
      "type": "string",
      "format": "date-time"
    }
  }
}
```

### 8.3 C# Implementation: NamedPipeServer (in WindBot.exe)

```csharp
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

public class NamedPipeServer : IDisposable
{
    private readonly string _pipeName;
    private readonly int _instanceId;
    private CancellationTokenSource _cts;
    private Task _listenTask;

    // Server pipe stream (accepted connection from Orchestrator)
    private PipeStream _serverPipe;
    private readonly object _pipeLock = new object();

    // Callback fired when Orchestrator sends us a message
    public event Action<string, object> OnMessage;

    public NamedPipeServer(int instanceId, int port, string botName)
    {
        _instanceId = instanceId;
        _pipeName = $"Ignis_{port}_{botName}";
        _cts = new CancellationTokenSource();
    }

    public void Start()
    {
        _listenTask = Task.Run(() => ListenLoop(_cts.Token));
    }

    public void Stop()
    {
        _cts.Cancel();
        _listenTask?.Wait(1000);
    }

    private async Task ListenLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using (var server = new NamedPipeServerStream(
                    _pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous))
                {
                    Log($"[NamedPipe] Waiting for orchestrator connection on {_pipeName}...");
                    await server.WaitForConnectionAsync(token);
                    Log($"[NamedPipe] Orchestrator connected on {_pipeName}");

                    // Store reference to the accepted pipe for sending events
                    lock (_pipeLock) { _serverPipe = server; }

                    try
                    {
                        while (!token.IsCancellationRequested && server.IsConnected)
                        {
                            string json = NamedPipeProtocol.ReadMessage(server);
                            var message = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);

                            if (message != null && message.ContainsKey("event_type"))
                            {
                                string eventType = message["event_type"].ToString();
                                ProcessOrchestratorCommand(eventType, message, server);
                            }
                        }
                    }
                    catch (IOException)
                    {
                        // Orchestrator disconnected (normal during restart)
                        Log($"[NamedPipe] Orchestrator disconnected from {_pipeName}");
                    }
                    finally
                    {
                        lock (_pipeLock) { _serverPipe = null; }
                    }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Log($"[NamedPipe] Listen error: {ex.Message}");
                // Wait before reconnect attempt
                await Task.Delay(1000, token);
            }
        }
    }

    public async Task SendEventAsync(object eventData)
    {
        // Write directly through the existing server pipe connection
        // (Orchestrator connected to us — we write back on the same stream)
        PipeStream pipe;
        lock (_pipeLock) { pipe = _serverPipe; }

        if (pipe != null && pipe.IsConnected)
        {
            try
            {
                string json = new JavaScriptSerializer().Serialize(eventData);
                NamedPipeProtocol.WriteMessage(pipe, json);
                Log($"[NamedPipe] Sent event to orchestrator via {_pipeName}");
            }
            catch (Exception ex)
            {
                Log($"[NamedPipe] Failed to send event: {ex.Message}");
                lock (_pipeLock) { _serverPipe = null; }
                throw; // Fallback handled by caller
            }
        }
        else
        {
            throw new InvalidOperationException("Named pipe not connected");
        }
    }

    private void ProcessOrchestratorCommand(string eventType, Dictionary<string, object> message, PipeStream pipe)
    {
        switch (eventType)
        {
            case "ack":
                // Orchestrator confirmed receipt of our LP=0 event
                // Safe to clear state now
                OnMessage?.Invoke("ack_received", message);
                break;

            case "heartbeat_ack":
                // Orchestrator received our heartbeat
                OnMessage?.Invoke("heartbeat_ack", message);
                break;

            case "deploy_cmd":
                // Orchestrator wants us to restart with new registry
                OnMessage?.Invoke("deploy", message);
                break;

            case "shutdown":
                // Graceful shutdown
                OnMessage?.Invoke("shutdown", message);
                break;
        }
    }

    private void Log(string msg) => Console.WriteLine($"{msg}");
    
    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
}
```

### 8.4 Python Implementation: NamedPipeClient (in IgnisOrchestrator)

```python
import json
import asyncio
import struct
import os
import logging
from typing import Dict, Optional, Callable, Awaitable
from pathlib import Path

logger = logging.getLogger("IgnisOrchestrator.NamedPipeClient")


class InstancePipeConnection:
    """Manages a single Named Pipe connection to one C# WindBot instance."""
    
    def __init__(self, instance_id: int, pipe_name: str, on_event: Callable):
        self.instance_id = instance_id
        self.pipe_name = pipe_name
        self.on_event = on_event
        self.reader: Optional[asyncio.StreamReader] = None
        self.writer: Optional[asyncio.StreamWriter] = None
        self._connected = False
        self._reconnect_task: Optional[asyncio.Task] = None
    
    async def connect(self):
        """Connect to the C# NamedPipeServer."""
        # On Windows, Named Pipe path is: \\.\pipe\Ignis_7911_BotA
        pipe_path = f"\\\\.\\pipe\\{self.pipe_name}"
        try:
            self.reader, self.writer = await asyncio.open_connection(pipe_path)
            self._connected = True
            logger.info(f"[Instance {self.instance_id}] Connected to {pipe_path}")
            return True
        except Exception as e:
            logger.warning(f"[Instance {self.instance_id}] Connection failed: {e}")
            self._connected = False
            return False
    
    async def read_loop(self):
        """Continuously read messages from the pipe."""
        while self._connected:
            try:
                message = await NamedPipeProtocol.read_message(self.reader)
                await self.on_event(self.instance_id, message)
            except (asyncio.IncompleteReadError, ConnectionError):
                logger.warning(f"[Instance {self.instance_id}] Pipe disconnected")
                self._connected = False
                break
            except Exception as e:
                logger.error(f"[Instance {self.instance_id}] Read error: {e}")
                await asyncio.sleep(0.1)
    
    async def send(self, data: dict):
        """Send a message to the C# instance."""
        if not self._connected:
            await self.connect()
        try:
            NamedPipeProtocol.write_message(self.writer, data)
            await self.writer.drain()
            return True
        except Exception as e:
            logger.error(f"[Instance {self.instance_id}] Send failed: {e}")
            self._connected = False
            return False
    
    async def close(self):
        self._connected = False
        if self.writer:
            self.writer.close()
            await self.writer.wait_closed()


class NamedPipeOrchestratorClient:
    """Manages all Named Pipe connections to all C# instances."""
    
    def __init__(self, max_dedup_events: int = 10000):
        self._connections: Dict[str, InstancePipeConnection] = {}
        self._pending_acks: Dict[str, asyncio.Event] = {}
        self._processed_events: set = set()
        self._max_dedup_events = max_dedup_events
        # Ordered deque for O(1) oldest removal when capping
        self._processed_event_queue: list = []
    
    def register_instance(self, instance_id: int, port: int, bot_name: str):
        """Register a C# instance and start listening to its pipe."""
        pipe_name = f"Ignis_{port}_{bot_name}"
        conn = InstancePipeConnection(instance_id, pipe_name, self._on_event)
        self._connections[pipe_name] = conn
        return conn
    
    async def send_lp_zero_ack(self, instance_id: int, event_id: str):
        """Send ACK back to C# instance confirming LP=0 data received."""
        ack_msg = {
            "event_type": "ack",
            "ack_for_event_id": event_id,
            "status": "received",
            "timestamp": datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%S.%fZ"),
            "orchestrator_pid": os.getpid()
        }
        # Find which connection has this instance_id
        for conn in self._connections.values():
            if conn.instance_id == instance_id:
                await conn.send(ack_msg)
                break
    
    async def broadcast_shutdown(self):
        """Send shutdown command to all instances."""
        shutdown_msg = {
            "event_type": "shutdown",
            "timestamp": datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%S.%fZ")
        }
        tasks = [conn.send(shutdown_msg) for conn in self._connections.values()]
        await asyncio.gather(*tasks, return_exceptions=True)
    
    async def _on_event(self, instance_id: int, message: dict):
        """Route incoming events to the appropriate handler."""
        event_type = message.get("event_type")
        
        if event_type == "lp_zero":
            # Validate against JSON Schema before processing
            if not self._validate_lp_zero(message):
                logger.error(f"[Instance {instance_id}] Invalid lp_zero schema")
                return
            
            # Deduplicate by event_id
            event_id = message.get("event_id")
            if event_id in self._processed_events:
                logger.warning(f"[Instance {instance_id}] Duplicate event {event_id}")
                # Send duplicate ACK so C# can proceed
                await self.send_lp_zero_ack(instance_id, event_id)
                return
            
            self._processed_events.add(event_id)
            self._processed_event_queue.append(event_id)
            # Cap dedup set to prevent unbounded memory growth
            if len(self._processed_event_queue) > self._max_dedup_events:
                oldest = self._processed_event_queue.pop(0)
                self._processed_events.discard(oldest)
            
            # Forward to orchestrator's data collector
            await self.on_lp_zero_callback(instance_id, message)
            
            # Send ACK
            await self.send_lp_zero_ack(instance_id, event_id)
        
        elif event_type == "heartbeat":
            # Update instance heartbeat registry
            self._update_heartbeat(instance_id, message)
            
            # Send minimal ACK — lookup by instance_id, not by port
            ack = {
                "event_type": "heartbeat_ack",
                "instance_id": instance_id,
                "timestamp": datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%S.%fZ")
            }
            for conn in self._connections.values():
                if conn.instance_id == instance_id:
                    await conn.send(ack)
                    break
        
        elif event_type == "checkpoint":
            # Store checkpoint for crash recovery
            self._store_checkpoint(instance_id, message)
    
    def _validate_lp_zero(self, message: dict) -> bool:
        """Quick validation of required fields."""
        required = ["event_id", "deck", "outcome", "bot_lp", "opp_lp", "turns"]
        for field in required:
            if field not in message:
                logger.error(f"Missing required field: {field}")
                return False
        if message.get("outcome") not in ("Win", "WeakWin", "Draw", "WeakLoss", "Loss"):
            logger.error(f"Invalid outcome: {message.get('outcome')}")
            return False
        return True
```

### 8.5 Error Handling & Retry Logic

#### 8.5.1 C# Side Error Handling

```
┌─────────────────────────────────────────────────────────────────────┐
│              C# LP=0 SEND — RETRY STRATEGY                         │
│                                                                      │
│  OnLPZeroDetected()                                                  │
│       │                                                              │
│       ▼                                                              │
│  ┌──────────────────────┐                                            │
│  │ Try NamedPipe Send   │──── Timeout 100ms ────► ┌──────────────┐ │
│  │ (with 100ms timeout)│                          │ Fallback:     │ │
│  └──────────┬───────────┘                          │ Write .match- │ │
│             │ Success                               │ data file     │ │
│             ▼                                      │ (atomic       │ │
│  ┌──────────────────────┐                          │  rename)      │ │
│  │ Wait for ACK         │                          └──────┬───────┘ │
│  │ (max 500ms timeout)  │                                 │         │
│  └──────────┬───────────┘                                ▼         │
│             │ Timeout                           ┌──────────────┐   │
│             ▼                                   │ Log fallback │   │
│  ┌──────────────────────┐                       │ + continue   │   │
│  │ Mark as sent, clear  │                       │ without ACK  │   │
│  │ state for next match │                       └──────────────┘   │
│  └──────────────────────┘                                           │
│                                                                      │
│  Security: ถ้า Named Pipe ล้มเหลว 3 ครั้งติด → ถือว่า Orchestrator  │
│  offline → เขียน fallback ทุกรอบ                                     │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

**C# Retry Logic Code:**

```csharp
private int _namedPipeFailCount = 0;
private const int MAX_PIPE_FAILS_BEFORE_FALLBACK_ONLY = 5;

protected async Task<bool> TrySendLPZeroEventAsync(string outcome, int botLP, int oppLP, int turns)
{
    var eventData = BuildLPZeroEventData(outcome, botLP, oppLP, turns);
    
    // If too many pipe failures, go straight to file fallback
    if (_namedPipeFailCount >= MAX_PIPE_FAILS_BEFORE_FALLBACK_ONLY)
    {
        WriteFallbackMatchDataFile(eventData);
        return false;
    }
    
    // Try Named Pipe
    try
    {
        await _namedPipeServer.SendEventAsync(eventData);
        
        // Wait for ACK (max 500ms)
        bool ackReceived = await WaitForAckAsync(eventData.event_id, 500);
        if (ackReceived)
        {
            _namedPipeFailCount = Math.Max(0, _namedPipeFailCount - 2); // Recover gradually
            LogToMatch("LP=0 event ACK received from orchestrator");
            return true;
        }
        
        // No ACK within timeout
        _namedPipeFailCount++;
        WriteFallbackMatchDataFile(eventData);
        return false;
    }
    catch
    {
        _namedPipeFailCount++;
        WriteFallbackMatchDataFile(eventData);
        return false;
    }
}

private void WriteFallbackMatchDataFile(object eventData)
{
    string json = new JavaScriptSerializer().Serialize(eventData);
    string tempFile = Path.Combine(_matchLogDir, $"lpzero_{DateTime.Now.Ticks}.tmp");
    string finalFile = Path.Combine(_matchLogDir, $"lpzero_{DateTime.Now.Ticks}.matchdata");
    File.WriteAllText(tempFile, json);
    File.Move(tempFile, finalFile); // Atomic rename
    LogToMatch("LP=0 data written to fallback file: " + finalFile);
}
```

#### 8.5.2 Python Side Fallback Scanner

```python
class FallbackFileScanner:
    """Scans match log directories for .matchdata fallback files."""
    
    def __init__(self, watch_dirs: List[Path], poll_interval_ms: int = 100):
        self.watch_dirs = watch_dirs
        self.poll_interval = poll_interval_ms / 1000.0
        self._processed = set()
    
    async def scan_loop(self):
        """Continuously scan for new .matchdata files."""
        while True:
            for watch_dir in self.watch_dirs:
                if not watch_dir.exists():
                    continue
                for matchdata_file in watch_dir.glob("*.matchdata"):
                    if matchdata_file.name in self._processed:
                        continue
                    
                    try:
                        content = matchdata_file.read_text(encoding="utf-8")
                        data = json.loads(content)
                        
                        # Process as LP=0 event
                        await self.on_fallback_event(data)
                        
                        # Mark as processed
                        self._processed.add(matchdata_file.name)
                        
                        # Optionally delete after processing
                        matchdata_file.unlink()
                        
                    except Exception as e:
                        logger.error(f"Failed to process fallback file {matchdata_file}: {e}")
            
            await asyncio.sleep(self.poll_interval)
```

### 8.6 Connection Lifecycle Sequence Diagrams

#### 8.6.1 Normal LP=0 Flow (Named Pipe Success)

```
C# Instance                    Named Pipe                    Orchestrator
    │                              │                              │
    │ LP=0 Detected                │                              │
    │─────────────────────────────►│                              │
    │                              │  Send: lp_zero event         │
    │                              │─────────────────────────────►│
    │                              │                              │
    │                              │                              │  Validate JSON Schema
    │                              │                              │  Deduplicate by event_id
    │                              │                              │  Forward to DataCollector
    │                              │                              │
    │                              │  Send: ack                   │
    │                              │◄─────────────────────────────│
    │                              │                              │
    │ ACK Received                 │                              │
    │◄─────────────────────────────│                              │
    │                              │                              │
    │ Clear State                  │                              │
    │ ResetDuelState()             │                              │
    │ Wait for next match          │                              │
    │                              │                              │
    │ Total elapsed: ~50-200ms     │                              │
    │                              │                              │
```

#### 8.6.2 LP=0 Flow with Fallback (Named Pipe Failed)

```
C# Instance                    Named Pipe                    Orchestrator
    │                              │                              │
    │ LP=0 Detected                │                              │
    │─────────────────────────────►│                              │
    │                              │  Timeout (100ms)             │
    │                              │     OR                        │
    │                              │  Pipe not available          │
    │                              │                              │
    │ Fallback:                    │                              │
    │ Write .matchdata file        │                              │
    │ (atomic rename)              │                              │
    │                              │                              │
    │ Clear State                  │                              │
    │ ResetDuelState()             │                              │
    │                              │                              │
    │                              │         Fallback Scanner     │
    │                              │         polls every 100ms    │
    │                              │◄─────────────────────────────│
    │                              │    Found: *.matchdata        │
    │                              │─────────────────────────────►│
    │                              │                              │
    │                              │                              │  Read + Process
    │                              │                              │  Delete .matchdata
    │                              │                              │
    │ Total elapsed: ~100-500ms   │                              │
    │ (C# doesn't wait for scan)   │                              │
```

#### 8.6.3 Heartbeat Flow

```
C# Instance                    Named Pipe                    Orchestrator
    │                              │                              │
    │ Every 1 second:              │                              │
    │                              │                              │
    │ Send: heartbeat              │                              │
    │─────────────────────────────►│                              │
    │                              │─────────────────────────────►│
    │                              │                              │
    │                              │                              │  Update heartbeat registry
    │                              │                              │  instance_id → last_seen
    │                              │                              │
    │ Send: heartbeat_ack          │                              │
    │◄─────────────────────────────│                              │
    │◄─────────────────────────────│                              │
    │                              │                              │
    │                              │                              │
    │ Orchestrator Detects Crash:  │                              │
    │ No heartbeat > 5 seconds     │                              │
    │ → Instance marked as DEAD    │                              │
    │ → StateRecovery.restart()    │                              │
```

### 8.7 Security & Reliability Considerations

| Concern | Solution | Implementation |
|---------|----------|---------------|
| **Unauthorized pipe access** | Named Pipes on Windows are secured by default to the same user session | ไม่ต้องแก้ — WindBot และ Orchestrator รันด้วย user เดียวกัน |
| **Pipe name collision** | Use port number + bot name as suffix | `Ignis_{port}_{botName}` ไม่ซ้ำกันแน่ |
| **Deadlock on full pipe buffer** | Length-prefixed framing prevents indefinite reads | 4-byte header + max 1 MB payload |
| **C# crash during pipe write** | Try-catch + fallback file | `WriteFallbackMatchDataFile()` ทุกครั้ง |
| **Orchestrator restart** | C# detects disconnection → reconnect | C# server auto-reconnects when pipe available |
| **Message order guarantee** | TCP-like ordered delivery within single pipe connection | Named Pipe guarantees ordering |
| **Data integrity** | JSON Schema validation + UUID deduplication | Orchestrator drops duplicates |
| **Memory leak in C#** | Dispose pipe objects after each send | `using` block + `IDisposable` |

### 8.8 Performance Benchmarks (Expected)

| Metric | Named Pipe | Fallback File | Notes |
|--------|------------|---------------|-------|
| **Latency (P50)** | 5-15 ms | 100-200 ms | File I/O is slower |
| **Latency (P99)** | 50-100 ms | 500-1000 ms | Under high load |
| **Throughput** | 10,000 msg/s | 100 msg/s | Single pipe |
| **Max concurrent pipes** | 50+ (OS limit ~256) | N/A | Windows pipe limit |
| **CPU overhead** | < 0.5% | < 0.1% | Named Pipe slightly more CPU |
| **Memory per pipe** | ~50 KB | ~0 KB | File-based has no memory cost |
| **Data loss risk** | Very Low (0.01%) | Low (0.1%) | If C# crashes before file write |

**Recommendation:** ใช้ Named Pipe เป็น Primary, Fallback File เป็น Secondary เสมอ

---

## 9. Implementation Priority Matrix

| Phase | Component | Dependencies | Effort | Impact | Risk |
|-------|-----------|-------------|--------|--------|------|
| **P0** | FileLockManager | — | 2 days | 🔴 Critical — ป้องกัน data loss | Low |
| **P0** | Atomic MatchDataCollector | C# Named Pipe | 3 days | 🔴 Critical — LP=0 capture | Medium |
| **P1** | IgnisOrchestrator | FileLockManager | 4 days | 🟡 High — Central control | Medium |
| **P1** | InstanceManager + PortManager | Orchestrator | 2 days | 🟡 High — 20 instances | Low |
| **P2** | DualDeckTrainer | Orchestrator | 3 days | 🟡 High — รองรับสองเด็ค | Medium |
| **P2** | AutoDeployEngine | Trainer | 2 days | 🟡 High — Real-time deploy | Low |
| **P3** | StateRecovery | InstanceManager | 3 days | 🟢 Medium — Crash recovery | High |
| **P3** | Registry → SQLite Migration | FileLockManager | 5 days | 🟢 Medium — Better concurrency | High |

### Phase Plan:

**Phase 0 (Week 1): Foundation**
- FileLockManager — cross-process file locking
- Atomic MatchDataCollector — C# Named Pipe event emitter
- ReadFileWithRetry / WriteFileWithRetry improvement in C#
- Testing single instance + single deck

**Phase 1 (Week 2): Multi-Instance**
- IgnisOrchestrator — central coordinator
- InstanceManager + PortManager
- 10 instances testing
- Web UI update (cockpit.py v2)

**Phase 2 (Week 3): Dual-Deck + Auto Deploy**
- DualDeckTrainer — เทรนสองเด็คพร้อมกัน
- AutoDeployEngine — deploy อัตโนมัติ
- 20 instances testing with dual deck
- Performance tuning

**Phase 3 (Week 4): Production Hardening**
- StateRecovery — crash recovery
- SQLite migration (optional)
- Monitoring dashboard
- Documentation

---

## 10. ข้อเสนอแนะเพิ่มเติม — Additional Recommendations

### 10.1 สิ่งที่ควรแก้ไขทันที (Quick Wins)

1. **เปลี่ยน `_staticLock` ใน C# เป็น File-based Lock (Level 1)** — ใช้เวลาครึ่งวัน ป้องกัน data loss ทันที
2. **เพิ่ม Retry + Stale Lock Detection ใน ReadFileWithRetry** — แก้ไขโค้ดที่มีอยู่แล้ว
3. **เพิ่ม `--deck-a` และ `--deck-b` ใน parallel_launcher.py** — รองรับสองเด็คทันที
4. **แยก registry file ตาม deck** (`cards_registry_DeckA.json` + `cards_registry_DeckB.json`) — shared_utils.py รองรับอยู่แล้ว

### 10.2 ข้อควรระวัง (Caveats)

1. **CPU Utilization:** 20 instances = 40 WindBot.exe processes (2 ต่อ instance) อาจใช้ CPU 100% ถ้าไม่ limit
   - แนะนำ: ใช้ `--cpu-affinity` หรือ set process priority
2. **RAM Usage:** แต่ละ WindBot.exe ใช้ ~50-100MB → 20 instances = 2-4GB RAM
3. **Network Port Range:** ต้องแน่ใจว่า firewall ไม่บล็อกพอร์ต 7911-7950
4. **Named Pipe Security:** Windows Named Pipe ต้องมี permission ที่ถูกต้อง

### 10.3 การตรวจสอบความถูกต้องของข้อมูล (Data Validation)

| Check | Description | Frequency |
|-------|-------------|-----------|
| CRC32 checksum บน registry | ป้องกัน corrupt JSON | ทุกครั้งที่เขียน |
| Schema validation (JSON Schema) | ตรวจสอบ structure ถูกต้อง | ทุกครั้งที่อ่าน |
| Duplicate match detection | ป้องกัน data ถูกบันทึกซ้ำ | ทุก LP=0 event |
| Anomaly detection | ตรวจจับ priority spike (>8 หรือ <1) | ทุก training batch |

### 10.4 การ Monitor และ Alerting

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Monitoring Dashboard                             │
│                                                                      │
│  ตัวอย่างเมตริกที่ควร追踪:                                             │
│                                                                      │
│  ● Active instances / Total instances                               │
│  ● Matches per minute (ต่อ instance + รวม)                          │
│  ● Win rate (Deck A, Deck B) — rolling 100 matches                 │
│  ● Average match duration (turns + real time)                       │
│  ● LP=0 events per minute                                          │
│  ● Deploy count (today)                                             │
│  ● Crash count (today) — sorted by instance                         │
│  ● File lock wait time (average, 95th percentile)                   │
│  ● Registry size (# cards + Q-value count)                          │
│  ● Top 10 opponent dangers learned                                  │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### 10.5 การทดสอบระบบ (Testing Strategy)

1. **Unit Tests:**
   - FileLockManager: concurrent lock acquisition, stale lock detection
   - PortManager: reservation, release, conflict handling
   - DualDeckTrainer: correct deck classification

2. **Integration Tests:**
   - 2 instances + 2 decks → verify no data loss
   - Simulate LP=0 + restart → verify data capture
   - Simulate instance crash → verify recovery

3. **Stress Tests:**
   - 20 instances → verify CPU/memory stability
   - 1000 matches → verify database integrity
   - File lock under high contention → verify no deadlock

---

> **เอกสารนี้เป็นส่วนหนึ่งของรายงาน Agentic-4 — วิเคราะห์และออกแบบสถาปัตยกรรมระบบ**
> 
> **ข้อเสนอแนะ:** แนะนำให้เริ่ม Phase 0 ทันที (FileLockManager + Atomic Data Collector) 
> เนื่องจากเป็น Critical Path ที่ป้องกัน data loss และต้องมีก่อนขยายเป็น 20 instances
