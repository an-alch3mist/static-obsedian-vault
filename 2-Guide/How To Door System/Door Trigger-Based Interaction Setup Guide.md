# Door Trigger-Based Interaction Setup Guide

## 🎮 UX Philosophy

```
E = Open/Close (all door movement)
L = Lock/Unlock (toggles lock state)
```

**Common Lock Behavior:**

- `L` key works from ANY side (inside or outside)
- Side detection is automatic via triggers

**Separate Locks Behavior:**

- `L` key locks/unlocks the lock on YOUR side
- Player inside = affects inside lock
- Player outside = affects outside lock

---

## 🏗️ Scene Setup (Per Door)

### Required Hierarchy:

```
./DoorHinged_01/
├─ trigger/ (empty GameObject)
│  ├─ door outside trigger (BoxCollider, isTrigger=true)
│  └─ door inside trigger  (BoxCollider, isTrigger=true)
├─ door/ (visuals)
│  ├─ frame
│  ├─ handleOutside
│  └─ handleInside
├─ doorFrame/ (visuals)
└─ Components:
   ├─ Animator
   ├─ DoorHinged (or DoorBase subclass)
   └─ DoorAnimEventForwarder
```

### Trigger Configuration:

#### Step 1: Create Trigger Parent

```
1. Add empty GameObject as child of door root
2. Name it "trigger"
3. Position at door center
```

#### Step 2: Create Outside Trigger

```
1. Add empty GameObject as child of "trigger"
2. Name: "door outside trigger" (MUST contain "outside")
3. Add BoxCollider component:
   - isTrigger: ✅ TRUE
   - Size: (2, 2.5, 1.5) - adjust to fit doorway
   - Center: (0, 1.25, -1) - place in front of door
4. Layer: Default (or "DoorTrigger" if using layer filtering)
```

#### Step 3: Create Inside Trigger

```
1. Duplicate outside trigger
2. Name: "door inside trigger" (MUST contain "inside")
3. BoxCollider:
   - isTrigger: ✅ TRUE
   - Size: (2, 2.5, 1.5)
   - Center: (0, 1.25, +1) - place behind door
```

**Visual Reference:**

```
        [Outside Trigger]
               ↓
    ┌──────────────────┐
    │                  │
    │   🚪 Door Panel  │ ← Door at (0, 0, 0)
    │                  │
    └──────────────────┘
               ↑
        [Inside Trigger]
```

---

## 🎯 Player Setup

### Add Component to Player:

```
1. Select your Player GameObject (or Player Camera)
2. Add Component → DoorInteraction
3. Configure:
   ├─ Interact Key: E
   ├─ Lock Key: L
   ├─ Prompt Text: [Drag UI TextMeshPro element]
   └─ Player Inventory: [Drag PlayerInventory component]
```

### Player Requirements:

```
✅ Must have Rigidbody (for trigger detection)
✅ Must have Collider (CapsuleCollider for FPS)
✅ Rigidbody settings:
   - Use Gravity: ✅
   - Is Kinematic: ☐ (usually false for FPS)
```

---

## 🔧 Trigger Sizing Guidelines

### Standard Door (2m x 2.5m):

```csharp
// Outside Trigger
Size: (2, 2.5, 1.5)    // Width, Height, Depth
Center: (0, 1.25, -1)  // Forward from door

// Inside Trigger
Size: (2, 2.5, 1.5)
Center: (0, 1.25, +1)  // Behind door
```

### Wide Double Doors:

```csharp
// Increase width
Size: (3.5, 2.5, 1.5)
```

### Narrow Hallway Door:

```csharp
// Shorter depth to avoid adjacent rooms
Size: (2, 2.5, 0.8)
```

### Rule of Thumb:

- **Width**: Door width + 0.5m padding
- **Height**: Ceiling height or 2.5m
- **Depth**: 1.5m (player's interaction range)

---

## 🎨 Visual Debug (Gizmos)

To visualize triggers in Scene view:

```csharp
// Add this to a script attached to "trigger" GameObject
private void OnDrawGizmos()
{
    foreach (Transform child in transform)
    {
        BoxCollider col = child.GetComponent<BoxCollider>();
        if (col != null && col.isTrigger)
        {
            // Color code by name
            Gizmos.color = child.name.Contains("outside") 
                ? new Color(0, 1, 0, 0.3f)   // Green for outside
                : new Color(1, 0, 0, 0.3f);  // Red for inside
            
            Gizmos.matrix = child.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
        }
    }
}
```

---

## ✅ Testing Checklist

### Single Door Test:

- [ ] Walk up to door → UI shows "[E] Open Door"
- [ ] Press E → Door opens
- [ ] Press E again → Door closes
- [ ] Press L → Door locks
- [ ] Press E → UI shows "[E] Unlock & Open" (if has key)
- [ ] Press L → Door unlocks

### Multiple Doors Test:

- [ ] Stand between two doors → Only closest door is active
- [ ] Walk from door A to door B → Current door switches
- [ ] Exit trigger → UI prompt disappears

### Common Lock Test:

- [ ] Set door.usesCommonLock = true
- [ ] Set door.initInsideLockState = UnlockedJam
- [ ] Walk to outside → Press L → Locks
- [ ] Walk to inside → Press L → Unlocks (same lock)

### Side Detection Test:

- [ ] Door with separate locks
- [ ] Stand outside → Press L → Locks outside
- [ ] Stand inside → Press L → Locks inside
- [ ] Both locks work independently

---

## 🐛 Common Issues

### "Trigger not detected"

✅ **Fix**: Add Rigidbody to player (required for OnTriggerEnter)

### "Wrong side detected"

✅ **Fix**: Ensure trigger names contain "outside" or "inside"

### "Multiple doors activate at once"

✅ **Fix**: System automatically picks closest door - working as intended

### "Player gets stuck in trigger"

✅ **Fix**: Increase trigger depth or adjust position

### "Lock key doesn't work from one side"

✅ **Fix**: Check if door.usesCommonLock matches your setup

- Common lock = Set initInsideLockState to UnlockedJam
- Separate locks = Both locks should be functional

---

## 🎯 Advanced: Layer-Based Filtering

If you want triggers to ONLY affect the player:

### Create Custom Layer:

```
1. Edit → Project Settings → Tags and Layers
2. Add new layer: "DoorTrigger"
3. Set both trigger colliders to layer "DoorTrigger"
```

### Player Collision Matrix:

```
Edit → Project Settings → Physics → Layer Collision Matrix
✅ Player ↔ DoorTrigger (should collide)
☐ DoorTrigger ↔ Everything else (optional)
```

This prevents enemies/objects from triggering door prompts.

---

## 📦 Prefab Setup

To make a door prefab:

```
1. Set up ONE door completely with triggers
2. Test all interactions work
3. Drag door root to Project window → Creates prefab
4. Now you can:
   - Drag prefab into scene (instant working door)
   - Override per-instance:
     * door.key = "specific_key"
     * door.initDoorState = Opened/Closed
     * Trigger sizes for narrow/wide doors
```

**That's it!** This system works with unlimited doors automatically. 🚪✨