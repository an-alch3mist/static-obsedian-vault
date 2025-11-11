
**Theme:** "Timeless"  
**Platform:** Unity 2020.3+ | URP | .NET 2.0 Standard  
**Goal:** Build a comprehensive, reusable door system demonstrating novel mechanics

---

## 1) Theme Interpretations

### Interpretation 1: **Time Loop / Temporal Recursion**

_"Actions persist across resets; doors remember their state through time loops"_

| Aspect              | Details                                                                                                                                                                                                                                                           |
| ------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Definition**      | Doors retain memory across temporal resets — a door opened in Loop 1 stays open in Loop 2, or requires the _same_ action sequence to unlock again                                                                                                                 |
| **Example Games**   | • **The Forgotten City** (Steam, 2021) — actions persist across loops<br>• **Outer Wilds** (Steam, 2019) — knowledge-based progression<br>• **Deathloop** (Steam, 2021) — persistent world changes<br>• **12 Minutes** (Steam, 2021) — repeating time loop puzzle |
| **Design Pros**     | • Creates emergent puzzle design (plan across loops)<br>• Encourages experimentation without permanent failure<br>• Natural fit for doors: "I unlocked this before, why is it locked again?"                                                                      |
| **Design Cons**     | • Can confuse players if not clearly telegraphed<br>• Requires robust save/load state management<br>• Risk of softlocks if critical doors reset incorrectly                                                                                                       |
| **Door System Fit** | Doors need **persistent state storage** (LOG.SaveGameData) and **event broadcasting** (OnDoorStateChanged) so other systems can react to temporal changes. Example: A door that records _who_ opened it and only opens for that "timeline version" of the player. |

---

### Interpretation 2: **Temporal Mechanics / Time Manipulation**

_"Doors exist in different time periods simultaneously or respond to time-based inputs"_

|Aspect|Details|
|---|---|
|**Definition**|Doors tied to time-of-day, age/decay states, or require time-manipulation abilities (rewind, fast-forward, freeze)|
|**Example Games**|• **Braid** (Steam, 2008) — time manipulation puzzles<br>• **Timeshift** (Steam, 2007) — time control mechanics<br>• **The Legend of Zelda: Ocarina of Time** — past/future door states<br>• **Dishonored 2** (Steam, 2016) — Crack in the Slab time-shift level|
|**Design Pros**|• Highly visual (doors visibly age/restore)<br>• Allows for "aha!" moments (door was always there, just rusted)<br>• Supports non-linear level design|
|**Design Cons**|• Requires two versions of geometry (past/future)<br>• Complex state synchronization (what if player freezes time mid-open?)<br>• Can be disorienting for players|
|**Door System Fit**|Doors need **layered state machines** (Closed_Present, Closed_Past) and **visual lerp-based transitions**. Example: A rusted door that becomes pristine when time rewinds, unlocking a new path. Moving doors (elevator/train) naturally fit here — they're "timeless" transport.|

---

### Interpretation 3: **Ageless / Eternal Existence**

_"Doors as ancient, supernatural entities with mysterious or horror elements"_

|Aspect|Details|
|---|---|
|**Definition**|Doors are sentient, haunted, or part of an eldritch environment — they sway, whisper, resist opening, or open themselves|
|**Example Games**|• **Layers of Fear** (Steam, 2016) — doors open/close on their own<br>• **Amnesia: The Dark Descent** (Steam, 2010) — oppressive door interactions<br>• **Control** (Steam, 2019) — doors as reality-warping thresholds<br>• **Silent Hill 2** — doors that lead to impossible spaces|
|**Design Pros**|• Strong atmospheric payoff (creepy swaying, sudden slams)<br>• Minimal explanation needed ("it's haunted")<br>• Supports psychological horror pacing|
|**Design Cons**|• Can feel arbitrary if overused ("why did _that_ door slam?")<br>• Requires excellent audio design to sell the horror<br>• Risk of annoying players if doors resist too much|
|**Door System Fit**|Doors need **swaying animation states**, **remote triggering** (for supernatural events), and **audio layering** (creaks, whispers). Example: A door that slowly opens while player looks away, accompanied by a low rumble — achieved via `DoorSwayBehavior` component with proximity/gaze detection.|

---

## 2) Existing Functionalities — Critique & Recommendations

### Current Scene Hierarchy (Hinged Door)

```
./turnYDoor/ (scale:1.0 | Animator, Door)
├ door (scale:1.0 | no components)
├ door origin (scale:1.0 | no components)
│ ├ handle (scale:(0.1,0.2,0.1) | MeshFilter, MeshRenderer, BoxCollider)
│ └ door block (scale:(1.0,2.0,0.1) | MeshFilter, MeshRenderer, BoxCollider)
├ hinge block (scale:(0.1,2.0,0.1) | MeshFilter, MeshRenderer, BoxCollider)
└ hinge (scale:(0.1,2.0,0.1) | MeshFilter, MeshRenderer, BoxCollider)
```

**✅ Strengths:**

- Clean separation (visual vs. functional)
- Animator on root allows easy state management

**❌ Issues:**

- `door origin` rotation pivot unclear — should be at hinge edge, not center
- Multiple colliders on decorative parts (`hinge block`, `hinge`) — only `door block` needs a collider for blocking
- No dedicated interaction collider — raycasts will hit visual mesh instead of logical bounds

**🔧 Recommended Hierarchy:**

```
./door_hinged/ (Animator, DoorHinged : MonoBehaviour)
├ visual/ (contains all MeshFilter/MeshRenderer)
│ ├ frame
│ ├ handle
│ └ panel
├ physics/
│ ├ blockingCollider (BoxCollider, solid, tag:DoorBlock)
│ └ interactionTrigger (BoxCollider, trigger, tag:DoorInteraction)
└ pivot (empty, positioned at hinge edge for rotation)
```

**Why?** Separates concerns: `visual/` for aesthetics, `physics/` for gameplay logic, `pivot` for animation. Interaction trigger is **larger** than blocking collider to show HUD before player touches door.

---

### Current Animator Controller

**✅ Strengths:**

- Simple trigger-based transitions
- No blend trees (good for clarity)

**❌ Issues:**

- No "Locked" or "Swaying" states
- Exit times hardcoded (0.00 for instant, 1.00 for full animation) — should be configurable
- Lacks `doorLocked` parameter for locked-door feedback

**🔧 Recommended Parameters:**

```csharp
// In GameStore.cs
public enum AnimParamType
{
    doorOpen,       // Trigger
    doorClose,      // Trigger
    doorLocked,     // Trigger (plays "locked jiggle" animation)
    doorSwaying,    // Bool (loops sway animation)
    autoCloseSpeed, // Float (multiplies closing speed)
}
```

**🔧 Recommended States:**

```
Entry → doorClosed (default)
doorClosed → [doorOpen trigger] → doorOpening → [exitTime=1.0] → doorOpened
doorOpened → [doorClose trigger] → doorClosing → [exitTime=1.0] → doorClosed
doorClosed → [doorLocked trigger] → doorLockedJiggle → [exitTime=1.0] → doorClosed
doorClosed/doorOpened → [doorSwaying=true] → doorSwayLoop (loops)
```

---

### Door HUD Placement

**Question:** Canvas overlay in scene or per-door GameObject?
**Recommendation:** **Main Canvas** (screen-space overlay)  
**Why?**
- Single canvas = better performance (one draw call)
- Easier to manage fade-in/fade-out from central `DoorHUDManager`
- Prefabs stay lightweight (no nested canvas)

**Implementation:**
```csharp
// DoorHUDManager updates a single TMP_Text + Image based on raycast hit
// Shows: "E to Open" | "Locked - Find Red Key" | "Door Opening..."
```

---

## 3a) Comprehensive Requirements Catalogue

### Door Types (All inherit `IDoor`)

| Name               | State Machine                                | Input Methods                             | Physics                                       | Notes                       |
| ------------------ | -------------------------------------------- | ----------------------------------------- | --------------------------------------------- | --------------------------- |
| **Hinged**         | Closed → Opening → Opened → Closing → Closed | Player interact, remote API               | Rotating collider, blocks NavMesh when closed | Standard swing door         |
| **Double**         | Uses 2 HingedDoor instances, synchronized    | Player interact (opens both)              | Both panels move                              | Church/mansion doors        |
| **Sliding**        | Closed → Opening → Opened → Closing → Closed | Proximity trigger, player interact        | Translating collider                          | Sci-fi automatic doors      |
| **Revolving**      | Continuously rotating, 4 compartments        | Player enters trigger zone                | Always moving, pushes player                  | Hotel entrance              |
| **Locked**         | Any door + `isLocked=true`                   | Requires key item or keypad code          | Same as base type                             | Extends base door           |
| **Timed**          | Auto-opens/closes on schedule                | Game time event                           | Same as base type                             | Opens at specific game time |
| **Pressure Plate** | Opens when weight applied                    | Rigidbody on plate                        | Plate animates down                           | Dungeon mechanic            |
| **Remote Switch**  | Controlled by lever/button elsewhere         | Signal bus event                          | Same as base type                             | Puzzle element              |
| **One-Way**        | Opens from one side only                     | Player interact (checks facing direction) | Same as base type                             | Directional valve           |
| **Phase Door**     | Becomes transparent/permeable when triggered | Special ability or timed event            | Disables collider when phased                 | Supernatural/sci-fi         |

---

### State Machine (Core)

```csharp
public enum DoorState
{
    Closed,         // Door fully closed, can be opened
    Opening,        // Animation playing, cannot be interrupted
    Opened,         // Door fully open, can be closed
    Closing,        // Animation playing, can be interrupted by obstruction
    Locked,         // Door closed and locked, cannot be opened without key
    Swaying,        // Idle animation (horror effect), can transition to Opening
    Blocked,        // Something obstructing path, auto-close paused
}
```

**Transitions:**

```
Closed → Opening → Opened → Closing → Closed
   ↓                           ↑
Locked                      Blocked (temp)
   ↓                           ↓
Closed                      Closing (retry)
   ↓
Swaying → Opening
```

---

### Input/Trigger Methods

| Method                | Description                                     | Use Case                      |
| --------------------- | ----------------------------------------------- | ----------------------------- |
| **Player Raycast**    | Player looks at door, presses E                 | Standard FPS interaction      |
| **Proximity Trigger** | BoxCollider trigger on door detects player      | Automatic sliding doors       |
| **Remote API Call**   | `door.TryOpen(force: true)` from script         | Keypad, lever, cutscene       |
| **Inventory Key**     | `door.TryUnlock(keyId)` checks player inventory | Locked doors                  |
| **Quest Completion**  | Event system calls `door.Unlock()`              | Story gates                   |
| **Timed Event**       | Opens/closes at specific game time              | Day/night cycle               |
| **Signal Bus**        | Global event triggers multiple doors            | Boss defeated, power restored |

---

### Unlocking Mechanisms

```csharp
// Example: Keypad door shows UI overlay when interacted
public class DoorKeypad : MonoBehaviour, IDoorUnlockMechanism
{
    [SerializeField] string correctCode = "1234";
    [SerializeField] DoorBase targetDoor;
    
    public void ShowKeypadUI()
    {
        // DoorHUDManager.Instance.ShowKeypadPanel(this);
        // Player enters code, calls ValidateCode(string input)
    }
    
    public void ValidateCode(string input)
    {
        if (input == correctCode)
        {
            targetDoor.Unlock();
            // Play success audio
        }
        else
        {
            // Play error beep
        }
    }
}
```

---

### Auto-Close Behavior

**Approach:** Coroutine-based timer with obstruction check

|Parameter|Description|Default|
|---|---|---|
|`autoCloseEnabled`|Enable auto-close?|false|
|`autoCloseDelay`|Seconds before closing starts|3.0f|
|`obstructionCheckRadius`|Sphere radius for overlap check|0.3f|
|`obstructionLayers`|Which layers block closing?|Player, Rigidbody|
|`forceCloseAfterAttempts`|Max retry attempts before forcing close|5|

**Algorithm:**

```csharp
IEnumerator AutoCloseRoutine()
{
    yield return new WaitForSeconds(autoCloseDelay);
    
    int attempts = 0;
    while (attempts < forceCloseAfterAttempts)
    {
        if (IsObstructed() == false)
        {
            TryClose();
            yield break;
        }
        attempts++;
        yield return new WaitForSeconds(1f); // Retry every second
    }
    
    // Force close (may crush player)
    TryClose(force: true);
}
```

---

### Door Swaying Behavior (Horror)

**Trigger Conditions:**

- Player within 5–10m range
- Player _not_ looking directly at door (gaze detection)
- Random chance (10% per second)

**Implementation:**

```csharp
// Attach to door as optional component
public class DoorSwayBehavior : MonoBehaviour
{
    [SerializeField] float triggerDistance = 7f;
    [SerializeField] AudioClip creakSound;
    DoorBase door;
    
    void Update()
    {
        if (PlayerInRange() && !PlayerLookingAtDoor() && Random.value < 0.1f * Time.deltaTime)
        {
            door.StartSwaying();
            AudioSource.PlayClipAtPoint(creakSound, transform.position);
        }
    }
}
```

---

### Moving Doors (Elevator/Train)

**Challenge:** Door moves with parent platform

**Solution:** Attach door to moving parent, use **local space** for all positions

```csharp
// DoorSliding calculates target position relative to parent
Vector3 targetLocalPos = isOpen ? openLocalOffset : Vector3.zero;
transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, speed * Time.deltaTime);
```

**NavMesh:** Use `NavMeshObstacle` with `carve = true`, disable when door opens

---

### Door HUD

**Display Logic:**

|Door State|HUD Text|Icon|
|---|---|---|
|Closed (unlocked)|"E to Open"|Hand icon|
|Opened|"E to Close"|Hand icon|
|Locked|"Locked - [Key Name] Required"|Lock icon|
|Opening/Closing|"Door [Opening/Closing]..."|None|
|Swaying|"E to Approach"|Warning icon|

**Implementation:** Single screen-space TMP_Text updated by `DoorHUDManager` based on player raycast hit

---

### Scene Hierarchy (Minimal)

**Hinged Door:**

```
door_hinged/
├ visual/ (all meshes)
├ physics/
│ ├ blockingCollider
│ └ interactionTrigger
└ pivot (rotation point)
```

**Sliding Door:**

```
door_sliding/
├ visual/
├ physics/
│ ├ blockingCollider (moves with door)
│ └ interactionTrigger (static, larger)
└ panel (moves horizontally)
```

---

### Animation Requirements

| Clip Name        | Duration | Looping | Events                    | Notes                    |
| ---------------- | -------- | ------- | ------------------------- | ------------------------ |
| doorClosed       | 0.1s     | No      | None                      | Static pose              |
| doorOpening      | 1.0s     | No      | `OnDoorHalfOpen` (0.5s)   | Plays creak SFX          |
| doorOpened       | 0.1s     | No      | None                      | Static pose              |
| doorClosing      | 1.0s     | No      | `OnDoorHalfClosed` (0.5s) | Plays slam SFX           |
| doorLockedJiggle | 0.3s     | No      | `OnDoorJiggle` (0.1s)     | Quick shake + rattle SFX |
| doorSwayLoop     | 2.0s     | Yes     | None                      | Subtle back-and-forth    |

**Animator Parameters:**

- `doorOpen` (Trigger)
- `doorClose` (Trigger)
- `doorLocked` (Trigger)
- `doorSwaying` (Bool)
- `autoCloseSpeed` (Float, 0.5–2.0x)

---

### Audio Requirements

|Event|Sound|Duration|Loopable|Notes|
|---|---|---|---|---|
|Unlocked|Key click|0.2s|No|Instant feedback|
|Opening|Creak|1.0s|No|Plays via Animation Event at 0.5s|
|Opened|Door thud|0.3s|No|Instant when fully open|
|Closing|Creak|1.0s|No|Same as opening|
|Closed|Slam|0.5s|No|Loud impact|
|Locked Attempt|Rattle|0.3s|No|Door shakes|
|Swaying|Low creak|2.0s|Yes|Horror ambience|

**Approach:** Use **Animation Events** for synchronized audio during opening/closing. Use `AudioSource.PlayClipAtPoint()` for instant sounds (unlock, locked attempt).

---

### Accessibility & Edge Cases

|Scenario|Solution|
|---|---|
|**Player stuck in door**|Force-open door if player detected inside blocking collider for >1s|
|**Box blocking auto-close**|Retry 5 times, then force-close (box may clip through)|
|**Concurrent open/close calls**|Lock state machine during transitions, queue requests|
|**Door opened during save**|Serialize current state + animation time, resume on load|
|**NavMesh update lag**|Use `NavMeshObstacle` instead of baking, toggles instantly|

---

## 3b) Door System — Scope & Goals

### Purpose Statement

Build a **data-driven, event-based door system** for Unity that supports:

- **10+ door types** (hinged, sliding, locked, timed, etc.) via inheritance/composition
- **Robust state management** (no race conditions, handles obstructions gracefully)
- **Audio-visual feedback** (animation events, HUD overlays)
- **Save/load persistence** (door states survive scene reloads)
- **Designer-friendly** (Inspector fields, ScriptableObject configs, preview tools)

The system must be **drop-in reusable** for future projects (game jams, prototypes, full games).

---

### Non-Goals

- **Destruction physics** (doors don't break/explode)
- **Multiplayer sync** (single-player only)
- **Advanced IK** (character hands don't grip handles)
- **Procedural generation** (doors are manually placed)

---

### Target Platforms

- **PC** (Windows/Mac/Linux) — keyboard + mouse
- **WebGL** — optional, if performance allows
- **Unity 2020.3+ LTS** — URP rendering
- **.NET 2.0 Standard** — no C# 8+ features, no async/await inside coroutines

---

### Accessibility & UX

- **Keyboard/Mouse**: E to interact, Esc to close keypad UI
- **Audio fallbacks**: Subtitles for door sounds (optional)
- **Colorblind mode**: HUD icons use shapes, not just colors
- **Raycast distance**: Adjustable (default 3m) for different player scales

---

## 4) Implementation Blueprint

### File Structure (Minimal)

```
Scripts/
├ DoorSystem.cs          // IDoor, DoorState, DoorBase (abstract), events
├ DoorHinged.cs          // MonoBehaviour: hinged door implementation
├ DoorSliding.cs         // MonoBehaviour: sliding door implementation
├ DoorDouble.cs          // MonoBehaviour: controls 2 DoorHinged instances
├ DoorHUDManager.cs      // MonoBehaviour: manages screen-space HUD text/icon
├ DoorConfigSO.cs        // ScriptableObject: door settings (speeds, sounds)
└ DoorSwayBehavior.cs    // MonoBehaviour (optional): horror swaying logic
```

**Why this structure?**

- `DoorSystem.cs` contains all shared logic (interface, enum, base class, events) → **single file, non-MonoBehaviour**
- Each door _type_ is a separate MonoBehaviour (hinged, sliding, double) → **inherits from `DoorBase`**
- `DoorHUDManager` is singleton MonoBehaviour → **one instance in scene**
- `DoorConfigSO` is data-only → **no logic, just Inspector fields**

---

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│  IDoor (interface)                                      │
│  - DoorState State { get; }                             │
│  - bool TryOpen() / TryClose() / TryUnlock()            │
│  - event Action<DoorState> OnStateChanged               │
└─────────────────────────────────────────────────────────┘
                        ▲
                        │ inherits
┌─────────────────────────────────────────────────────────┐
│  DoorBase : MonoBehaviour (abstract)                    │
│  - Animator animator                                    │
│  - DoorConfigSO config                                  │
│  - Coroutine autoCloseRoutine                           │
│  - IEnumerator OpenRoutine() / CloseRoutine()           │
│  - bool IsObstructed()                                  │
└─────────────────────────────────────────────────────────┘
           ▲                    ▲                   ▲
           │                    │                   │
  ┌────────┴────────┐  ┌────────┴────────┐  ┌───────┴────────┐
  │ DoorHinged      │  │ DoorSliding     │  │ DoorDouble     │
  │ (rotate pivot)  │  │ (translate pos) │  │ (sync 2 doors) │
  └─────────────────┘  └─────────────────┘  └────────────────┘
```

**Responsibilities:**
- **IDoor**: Public contract (what all doors must do)
- **DoorBase**: Shared logic (state machine, audio, save/load)
- **DoorHinged/Sliding/etc**: Specific animation/movement logic
---
### Public API (.NET 2.0)

#### **DoorSystem.cs** (Non-MonoBehaviour, Single File)---

#### **DoorHinged.cs** (MonoBehaviour)---

#### **DoorSliding.cs** (MonoBehaviour)---

#### **DoorDouble.cs** (MonoBehaviour)---

#### **DoorConfigSO.cs** (ScriptableObject)---

#### **DoorHUDManager.cs** (MonoBehaviour Singleton)---

#### **DoorSwayBehavior.cs** (Optional MonoBehaviour Component)---

#### **DoorInteractionController.cs** (MonoBehaviour)---

### Updated **GameStore.cs** (Add AnimParamType.doorSwaying)---

## 5) Robustness, Concurrency & Edge Cases

### Race Conditions from Simultaneous Open/Close Calls

**Problem:** Multiple scripts calling `TryOpen()` or `TryClose()` simultaneously can cause undefined behavior.

**Solution:** State-based locking pattern in `DoorBase`:

```csharp
// DoorBase.cs - Already implemented
public bool TryOpen(bool force = false)
{
    // Check if already opening/opened
    if (currentState == DoorState.Opened || currentState == DoorState.Opening)
        return false; // Reject request
    
    // Stop conflicting close routine
    if (closeRoutine != null)
    {
        StopCoroutine(closeRoutine);
        closeRoutine = null;
    }
    
    openRoutine = StartCoroutine(OpenRoutine());
    return true;
}
```

**Key Points:**

- State checks prevent duplicate coroutines
- Conflicting routines are stopped before starting new ones
- `force` parameter allows override for special cases (e.g., cutscenes)
---

### Auto-Close While Obstruction Exists

**Problem:** Player or box stuck in doorway prevents auto-close → door hangs open forever.

**Solution:** Retry mechanism with force-close fallback (already implemented):

```csharp
// DoorBase.cs - AutoCloseRoutine()
private IEnumerator AutoCloseRoutine()
{
    yield return new WaitForSeconds(autoCloseDelay);
    
    int attempts = 0;
    while (attempts < 100)
    {
        if (!IsObstructed())
        {
            TryClose();
            yield break; // Success
        }
        attempts++;
        yield return new WaitForSeconds(1f); // Retry every second
    }
    
    // Force close (may crush player)
    TryClose(force: true);
}
```
---

### Player Clipping and Stuck Objects

**Problem:** Player gets stuck inside door collider during opening animation.

**Solutions:**
#### Option 2: Force-Open if Player Inside Collider for >1s

```csharp
// DoorBase.cs - Add to Update()
private float playerStuckTimer = 0f;

void Update()
{
    if (currentState == DoorState.Closed || currentState == DoorState.Closing)
    {
        if (IsPlayerInsideCollider())
        {
            playerStuckTimer += Time.deltaTime;
            if (playerStuckTimer > 1f)
            {
                Debug.Log(C.method(this, "orange", "Player stuck, force-opening door"));
                TryOpen(force: true);
                playerStuckTimer = 0f;
            }
        }
        else
        {
            playerStuckTimer = 0f;
        }
    }
}

bool IsPlayerInsideCollider()
{
    // Check if player's center is inside door bounds
    Transform player = Camera.main != null ? Camera.main.transform : null;
    if (player == null) return false;
    
    return blockingCollider.bounds.Contains(player.position);
}
```

**Recommended:** Use **Option 2** for better feel (door reacts to player presence).

---

## Animator Setup (Mecanim)

### Recommended Animator Controller

**Parameters Table:**

|Parameter|Type|Default|Purpose|
|---|---|---|---|
|`doorOpen`|Trigger|false|Transition Closed → Opening|
|`doorClose`|Trigger|false|Transition Opened → Closing|
|`doorLocked`|Trigger|false|Play jiggle animation|
|`doorSwaying`|Bool|false|Loop sway animation (horror)|
|`autoCloseSpeed`|Float|1.0|Multiplier for closing speed (optional)|

---
### State Machine Graph
```
Entry → doorClosed (default)
States:
├─ doorClosed (motion: doorClosedAnim, speed: 1.0x)
│  ├─ [doorOpen trigger] → doorOpening (exitTime: 0.00, duration: 0.01s)
│  └─ [doorLocked trigger] → doorLockedJiggle (exitTime: 0.00, duration: 0.01s)
│
├─ doorOpening (motion: doorOpeningAnim, speed: 1.0x)
│  └─ [exitTime: 1.00] → doorOpened (auto transition, duration: 0.01s)
│
├─ doorOpened (motion: doorOpenedAnim, speed: 1.0x)
│  └─ [doorClose trigger] → doorClosing (exitTime: 0.00, duration: 0.01s)
│
├─ doorClosing (motion: doorClosingAnim, speed: autoCloseSpeed)
│  └─ [exitTime: 1.00] → doorClosed (auto transition, duration: 0.01s)
│
├─ doorLockedJiggle (motion: doorLockedJiggleAnim, speed: 1.0x)
│  └─ [exitTime: 1.00] → doorClosed (auto transition, duration: 0.01s)
│
└─ doorSwayLoop (motion: doorSwayLoopAnim, speed: 1.0x, loop: true)
   ├─ [doorSwaying == false] → doorClosed (exitTime: 0.00, duration: 0.5s)
   └─ [doorOpen trigger] → doorOpening (exitTime: 0.00, duration: 0.2s)
```

---
### Animation Clips (Keyframe Guide)
#### doorOpeningAnim (1.0s duration)

|Time|Rotation (Y-axis)|Event|
|---|---|---|
|0.0s|0° (closed)|—|
|0.5s|45°|`OnDoorHalfOpen()` → Play creak SFX|
|1.0s|90° (open)|—|

#### doorClosingAnim (1.0s duration)

|Time|Rotation (Y-axis)|Event|
|---|---|---|
|0.0s|90° (open)|—|
|0.5s|45°|`OnDoorHalfClosed()` → Play creak SFX|
|1.0s|0° (closed)|`OnDoorClosed()` → Play slam SFX|

#### doorLockedJiggleAnim (0.3s duration)

|Time|Rotation (Y-axis)|Event|
|---|---|---|
|0.0s|0°|—|
|0.1s|3°|`OnDoorJiggle()` → Play rattle SFX|
|0.15s|-3°|—|
|0.25s|2°|—|
|0.3s|0°|—|

#### doorSwayLoopAnim (2.0s duration, looping)

|Time|Rotation (Y-axis)|Notes|
|---|---|---|
|0.0s|0°|—|
|0.5s|2°|Subtle sway right|
|1.0s|0°|Return to center|
|1.5s|-2°|Subtle sway left|
|2.0s|0°|Loop back to start|

---
### Animation Events Setup

**Add via Animation Window → Select Clip → Right-click Timeline → Add Event**
```csharp
// Add to DoorBase.cs (called by Animation Events)

// Called at 0.5s of doorOpeningAnim
public void OnDoorHalfOpen()
{
    PlayOpenSound(); // Plays creak
}

// Called at 0.5s of doorClosingAnim
public void OnDoorHalfClosed()
{
    PlayCloseSound(); // Plays creak
}

// Called at 1.0s of doorClosingAnim
public void OnDoorClosed()
{
    if (config != null && config.closeSound != null)
        AudioSource.PlayClipAtPoint(config.closeSound, transform.position, config.volume);
}

// Called at 0.1s of doorLockedJiggleAnim
public void OnDoorJiggle()
{
    PlayLockedFeedback();
}
```

**Why Animation Events?**  
✅ Perfect sync between animation and audio  
✅ No script polling required  
✅ Designer-friendly (visible in Animation timeline)

---
## Audio System Integration

### Recommended Audio Structure

```
Resources/
└─ audio/
   ├─ door/
   │  ├─ open_creak.wav (1.0s, stereo, 44.1kHz)
   │  ├─ close_creak.wav (1.0s)
   │  ├─ slam.wav (0.5s, high impact)
   │  ├─ unlock_click.wav (0.2s, mechanical)
   │  ├─ locked_rattle.wav (0.3s, metallic)
   │  └─ sway_creak_loop.wav (2.0s, looping)
```

### DoorConfigSO Setup (Example)

Create via: `Assets > Create > Door System > Door Config`

**Inspector Fields:**

- `openSound`: `open_creak.wav`
- `closeSound`: `close_creak.wav`
- `unlockSound`: `unlock_click.wav`
- `lockedSound`: `locked_rattle.wav`
- `swaySound`: `sway_creak_loop.wav`
- `volume`: `0.7`

---

### Audio Mixer Integration (Optional)

Create snapshots for dynamic audio:

```csharp
// DoorBase.cs - Add mixer support
[SerializeField] private AudioMixerGroup doorAudioGroup;

protected void PlayOpenSound()
{
    AudioSource source = AudioSource.PlayClipAtPoint(config.openSound, transform.position, config.volume);
    source.outputAudioMixerGroup = doorAudioGroup;
}
```

**Mixer Hierarchy:**

```
Master
└─ SFX
   └─ Doors (group)
      ├─ Lowpass filter (horror effect)
      └─ Reverb (spatial feel)
```

---

## Testing Harness

### Test Scene Setup

1. **Create Test Scene:** `Scenes/DoorSystemTest.unity`
    
2. **Add Components:**
    
    ```
    Hierarchy:
    ├─ Main Camera (DoorInteractionController)
    ├─ Canvas (DoorHUDManager)
    │  └─ HUD Panel (TMP_Text + Image)
    ├─ Doors/
    │  ├─ door_hinged_test (DoorHinged + BoxCollider)
    │  ├─ door_sliding_test (DoorSliding)
    │  ├─ door_double_test (DoorDouble + 2x DoorHinged children)
    │  └─ door_locked_test (DoorHinged, isLocked=true)
    └─ Obstacles/
       ├─ Cube (Rigidbody, for obstruction tests)
       └─ Player (CharacterController, for stuck tests)
    ```
    
3. **Debug UI Panel:**
    
    ```csharp
    // Add to test scene
    public class DoorDebugUI : MonoBehaviour
    {
        [SerializeField] private DoorBase[] testDoors;
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 300));
            foreach (DoorBase door in testDoors)
            {
                GUILayout.Label($"{door.name}: {door.State}");
                if (GUILayout.Button("Force Open")) door.TryOpen(force: true);
                if (GUILayout.Button("Force Close")) door.TryClose(force: true);
                if (GUILayout.Button("Lock")) door.Lock();
                if (GUILayout.Button("Unlock")) door.TryUnlock();
                GUILayout.Space(10);
            }
            GUILayout.EndArea();
        }
    }
    ```
    

---

### Automated Playmode Tests (Optional)

```csharp
// Tests/DoorSystemTests.cs (Unity Test Framework)
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class DoorSystemTests
{
    [UnityTest]
    public IEnumerator DoorOpensAndCloses()
    {
        // Setup
        GameObject doorObj = new GameObject("TestDoor");
        DoorHinged door = doorObj.AddComponent<DoorHinged>();
        
        // Open
        bool opened = door.TryOpen();
        Assert.IsTrue(opened);
        yield return new WaitUntil(() => door.State == DoorState.Opened);
        
        // Close
        bool closed = door.TryClose();
        Assert.IsTrue(closed);
        yield return new WaitUntil(() => door.State == DoorState.Closed);
        
        // Cleanup
        Object.Destroy(doorObj);
    }
    
    [UnityTest]
    public IEnumerator LockedDoorStaysLocked()
    {
        GameObject doorObj = new GameObject("TestDoor");
        DoorHinged door = doorObj.AddComponent<DoorHinged>();
        door.Lock();
        
        bool opened = door.TryOpen();
        Assert.IsFalse(opened);
        Assert.AreEqual(DoorState.Locked, door.State);
        
        Object.Destroy(doorObj);
        yield return null;
    }
}
```

---

## 6) Extension Paths (Modular Future Features)

### 1. **Keypad/Puzzle Doors**

**Implementation:**

```csharp
// DoorKeypad.cs (attach alongside DoorBase)
public class DoorKeypad : MonoBehaviour
{
    [SerializeField] private DoorBase targetDoor;
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private Canvas keypadCanvas; // Overlay UI
    
    public void OnPlayerInteract()
    {
        keypadCanvas.gameObject.SetActive(true);
        // Show numpad UI, call ValidateCode(input) on submit
    }
    
    public void ValidateCode(string input)
    {
        if (input == correctCode)
        {
            targetDoor.TryUnlock();
            keypadCanvas.gameObject.SetActive(false);
        }
        else
        {
            // Play error beep, shake UI
        }
    }
}
```

---

### 3. **Portal Doors** (Seamless Transitions)

**Example:** Door leads to different scene/location

```csharp
// DoorPortal.cs (attach alongside DoorBase)
public class DoorPortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private Transform spawnPoint;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Load scene asynchronously
            SceneManager.LoadSceneAsync(targetSceneName);
            // Teleport player to spawn point on load
        }
    }
}
```

---

### 4. **Destructible Doors**

**Implementation:** Add health component:

```csharp
// DoorHealth.cs
public class DoorHealth : MonoBehaviour
{
    [SerializeField] private DoorBase door;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    
    void Start() { currentHealth = maxHealth; }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            DestroyDoor();
        }
    }
    
    void DestroyDoor()
    {
        // Spawn debris particles
        // Disable door collider
        // Play destruction sound
        door.TryOpen(force: true); // Force open
    }
}
```

---

### 5. **One-Way Doors**

**Implementation:** Check player facing direction:

```csharp
// DoorOneWay.cs (override TryOpen in DoorHinged)
public class DoorOneWay : DoorHinged
{
    [SerializeField] private bool allowFromFront = true;
    
    public override bool TryOpen(bool force = false)
    {
        if (!force && !CanOpenFromCurrentSide())
        {
            Debug.Log(C.method(this, "yellow", "Cannot open from this side"));
            return false;
        }
        return base.TryOpen(force);
    }
    
    bool CanOpenFromCurrentSide()
    {
        Transform player = Camera.main.transform;
        Vector3 toPlayer = (player.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, toPlayer);
        
        return allowFromFront ? (dot < 0) : (dot > 0);
    }
}
```

---

### 6. **Pressure Plate Doors**

```csharp
// DoorPressurePlate.cs
public class DoorPressurePlate : MonoBehaviour
{
    [SerializeField] private DoorBase targetDoor;
    [SerializeField] private Animator plateAnimator;
    private int objectsOnPlate = 0;
    
    void OnTriggerEnter(Collider other)
    {
        objectsOnPlate++;
        if (objectsOnPlate == 1)
        {
            plateAnimator.SetTrigger("pressDown");
            targetDoor.TryOpen();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        objectsOnPlate--;
        if (objectsOnPlate == 0)
        {
            plateAnimator.SetTrigger("pressUp");
            targetDoor.TryClose();
        }
    }
}
```

---

## Summary — File List & Responsibilities

|File|Type|Purpose|
|---|---|---|
|**DoorSystem.cs**|Non-MonoBehaviour|Interface, enum, base class, events|
|**DoorHinged.cs**|MonoBehaviour|Rotating door (hinge pivot)|
|**DoorSliding.cs**|MonoBehaviour|Translating door (elevator/sci-fi)|
|**DoorDouble.cs**|MonoBehaviour|Synchronized double doors|
|**DoorConfigSO.cs**|ScriptableObject|Data-driven config (audio, speeds)|
|**DoorHUDManager.cs**|MonoBehaviour|Screen-space interaction prompts|
|**DoorInteractionController.cs**|MonoBehaviour|Player raycast & input handling|
|**DoorSwayBehavior.cs**|MonoBehaviour (optional)|Horror swaying effect|
|**GameStore.cs**|MonoBehaviour|Global enums, save/load, InputActions|

**Total Files:** 9 (8 scripts + 1 ScriptableObject template)

---

## Inspector Workflow (Designer-Friendly)

1. **Create Door Prefab:**
    
    - Add empty GameObject → `door_hinged`
    - Add `DoorHinged` component
    - Assign `rotationPivot` child (at hinge edge)
    - Assign `Animator` (with doorOpenCloseAnimController)
    - Create `DoorConfigSO` asset → assign to `config` field
2. **Configure in Inspector:**
    
    - `openAngle`: 90 (degrees)
    - `autoCloseEnabled`: true/false
    - `isLocked`: false
    - `requiredKeyId`: "redKey" (if locked)
3. **Preview in Editor:**
    
    - Select door → Inspector → click "Preview Open" button (add via custom editor script)
    - Gizmos show interaction range, obstruction checks

---

## Critical Reminders

✅ **Never yield inside try-catch** (.NET 2.0 limitation)  
✅ **Always use `C.method()` for logging** (color-tagged Debug.Log)  
✅ **Use `animator.TrySetTrigger()`** instead of raw `SetTrigger()`  
✅ **Save door states via `LOG.SaveGameData()`** (persistent across sessions)  
✅ **Use local space for moving doors** (elevator/train compatibility)  
✅ **Handle obstructions gracefully** (retry + force-close after 5 attempts)

---

**End of Door System Documentation**  
Ready for immediate use in Unity 3D Game Jam 2025 🎮