```cs
// Your original SimpleDoorInteraction.cs
if (INPUT.K.InstantDown(KeyCode.O)) door.TryOpen();
if (INPUT.K.InstantDown(KeyCode.C)) door.TryClose();
if (INPUT.K.InstantDown(KeyCode.L)) door.TryLock(LockSide.Inside);
```
**Problems:**
1. ❌ Not scalable (works for 1 door, breaks with multiple doors)
2. ❌ No spatial awareness (can't tell which door you're looking at)
3. ❌ Breaks immersion (pressing "O" for open is not intuitive)
4. ❌ No player position context (can't determine inside/outside)
5. ❌ Requires 6+ keybinds just for doors

---
## ✅ UX Philosophy in New System

### **One Key to Rule Them All ("E")**

Door Closed + Unlocked → E = Open
Door Closed + Locked   → E = Auto-unlock + Open (if has key)
Door Opened            → E = Close


**Why this is better:**
- ✅ Player only needs to remember ONE key
- ✅ System does the "smart thing" based on context
- ✅ Mirrors games like **Resident Evil**, **Amnesia**, **Outlast**

### **Separate Lock Key ("F")**
```
Hold F = Lock door (deliberate action)
```

**Why hold instead of tap:**
- Prevents accidental locks
- Gives player time to change their mind
- Feels more deliberate/immersive

---

## 🎮 Player Experience Comparison

### Your Original System:
```
Player sees door → Presses O → Nothing happens (locked)
Player presses U → Nothing happens (wrong side)
Player presses 0 → Door unlocks (outside)
Player presses O again → Door opens
```
**Result**: Confusing, trial-and-error

### New System:
```
Player looks at door → UI shows "[E] Unlock & Open"
Player presses E → Auto-unlocks → Opens
```
**Result**: Intuitive, one action

---

## 🔧 Setup for Multiple Doors

### Scene Setup:
```
Scene Hierarchy:
├─ Player (with DoorInteraction.cs on Camera)
├─ Door_01 (DoorHinged, layer: "Interactable")
├─ Door_02 (DoorHinged, layer: "Interactable")
├─ Door_03 (DoorHinged, layer: "Interactable")
└─ ... (unlimited doors)
```

### Inspector Setup:
```
DoorInteraction (on Player Camera):
├─ interactionRange: 2.0m
├─ doorLayers: Interactable ✅
├─ interactKey: E
├─ lockKey: F
└─ promptText: [Drag UI TextMeshPro]
```

## The New DoorInteraction.cs
```cs
using UnityEngine;
using System.Collections.Generic;
using SPACE_UTIL;

/// <summary>
/// Trigger-based door interaction system for first-person.
/// 
/// UX Philosophy:
/// - E = Open/Close (one key for all door movement)
/// - L = Lock/Unlock (toggles lock state)
/// - Works with ANY door in scene automatically
/// - Uses trigger zones to detect which side player is on
/// 
/// Setup Per Door:
/// 1. Door has two child triggers: "door outside trigger" and "door inside trigger"
/// 2. Triggers are BoxColliders with isTrigger = true
/// 3. Both triggers on layer "DoorTrigger" (or your choice)
/// 4. This component goes on Player GameObject
/// 
/// Hierarchy per door:
/// ./DoorHinged/
/// ├ trigger/
/// │ ├ door outside trigger (BoxCollider, isTrigger=true, layer: DoorTrigger)
/// │ └ door inside trigger  (BoxCollider, isTrigger=true, layer: DoorTrigger)
/// └ ... (visuals, animator, DoorBase component)
/// </summary>
public class DoorInteraction : MonoBehaviour
{
    // ====================================================================
    // CONFIGURATION
    // ====================================================================
    
    [Header("INTERACTION KEYS")]
    [Tooltip("Key to open/close doors")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Tooltip("Key to lock/unlock doors")]
    [SerializeField] private KeyCode lockKey = KeyCode.L;

    [Header("UI FEEDBACK")]
    [Tooltip("UI text for contextual prompts (optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI promptText;

    [Header("PLAYER INVENTORY (Optional)")]
    [Tooltip("Reference to player inventory for key checking")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("DEBUG")]
    [SerializeField] private bool showDebugLogs = true;

    // ====================================================================
    // RUNTIME STATE
    // ====================================================================
    
    // Tracks which doors the player is currently inside trigger zones of
    private HashSet<DoorBase> doorsPlayerIsNear = new HashSet<DoorBase>();
    
    // Tracks which side of each door the player is on
    private Dictionary<DoorBase, LockSide> doorSides = new Dictionary<DoorBase, LockSide>();
    
    // The door player interacted with most recently (or is closest to)
    private DoorBase currentDoor;

    // ====================================================================
    // UNITY LIFECYCLE
    // ====================================================================
    
    private void Update()
    {
        // Determine which door is "active" (closest if multiple nearby)
        UpdateCurrentDoor();

        // Update UI prompt
        UpdatePrompt();

        // Handle input
        if (currentDoor != null)
        {
            // E = Open/Close
            if (Input.GetKeyDown(interactKey))
            {
                HandleInteract();
            }

            // L = Lock/Unlock
            if (Input.GetKeyDown(lockKey))
            {
                HandleLockToggle();
            }
        }
    }

    // ====================================================================
    // TRIGGER DETECTION (Automatic via OnTriggerEnter/Exit)
    // ====================================================================
    
    /// <summary>
    /// Called when player enters a door trigger zone.
    /// Automatically detects which side based on trigger name.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check if this is a door trigger
        DoorBase door = other.GetComponentInParent<DoorBase>();
        if (door == null) return;

        // Determine which side based on trigger name
        LockSide side = DetermineSideFromTrigger(other);

        // Add door to nearby set
        doorsPlayerIsNear.Add(door);
        doorSides[door] = side;

        if (showDebugLogs)
            Debug.Log($"[DoorInteraction] Entered {door.name} trigger ({side} side)".colorTag("cyan"));
    }

    /// <summary>
    /// Called when player exits a door trigger zone.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        DoorBase door = other.GetComponentInParent<DoorBase>();
        if (door == null) return;

        // Remove door from nearby set
        doorsPlayerIsNear.Remove(door);
        doorSides.Remove(door);

        // Clear currentDoor if we left its trigger
        if (currentDoor == door)
            currentDoor = null;

        if (showDebugLogs)
            Debug.Log($"[DoorInteraction] Exited {door.name} trigger".colorTag("grey"));
    }

    /// <summary>
    /// Determines which side of door based on trigger object name.
    /// Expected names: "door outside trigger" or "door inside trigger"
    /// </summary>
    private LockSide DetermineSideFromTrigger(Collider trigger)
    {
        string triggerName = trigger.name.ToLower();

        if (triggerName.Contains("outside"))
            return LockSide.Outside;
        else if (triggerName.Contains("inside"))
            return LockSide.Inside;
        else
        {
            Debug.LogWarning($"[DoorInteraction] Trigger name '{trigger.name}' doesn't contain 'inside' or 'outside'. Defaulting to Outside.".colorTag("yellow"));
            return LockSide.Outside;
        }
    }

    /// <summary>
    /// Updates which door is currently active (closest to player).
    /// If only one door nearby, that's the active door.
    /// If multiple doors, choose closest.
    /// </summary>
    private void UpdateCurrentDoor()
    {
        if (doorsPlayerIsNear.Count == 0)
        {
            currentDoor = null;
            return;
        }

        // If only one door nearby, use it
        if (doorsPlayerIsNear.Count == 1)
        {
            foreach (var door in doorsPlayerIsNear)
            {
                currentDoor = door;
                return;
            }
        }

        // Multiple doors - find closest
        float closestDist = float.MaxValue;
        DoorBase closestDoor = null;

        foreach (var door in doorsPlayerIsNear)
        {
            float dist = Vector3.Distance(transform.position, door.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestDoor = door;
            }
        }

        currentDoor = closestDoor;
    }

    // ====================================================================
    // INTERACTION LOGIC
    // ====================================================================
    
    /// <summary>
    /// Handles E key - Open or Close door based on current state.
    /// Auto-unlocks if player has key.
    /// </summary>
    private void HandleInteract()
    {
        if (currentDoor == null) return;

        // === CASE 1: Door is CLOSED ===
        if (currentDoor.currDoorState == DoorState.Closed)
        {
            // Try to open
            DoorActionResult result = currentDoor.TryOpen();

            if (result == DoorActionResult.Locked)
            {
                // Door is locked - try to auto-unlock
                string playerKey = GetPlayerKeyForDoor(currentDoor);

                if (playerKey != "")
                {
                    // Player has key - unlock then open
                    LockSide playerSide = GetPlayerSideOfDoor(currentDoor);
                    DoorActionResult unlockResult = currentDoor.TryUnlock(playerSide, playerKey);

                    if (unlockResult == DoorActionResult.Success)
                    {
                        ShowFeedback($"Unlocked with {playerKey}");
                        
                        // Wait for unlock animation, then open
                        StartCoroutine(OpenAfterDelay(0.3f));
                    }
                    else if (unlockResult == DoorActionResult.WrongKeyToUnlock)
                    {
                        ShowFeedback($"Wrong key! Need: {currentDoor.key}");
                    }
                }
                else
                {
                    // No key
                    ShowFeedback($"Locked. Need: {currentDoor.key}");
                }
            }
            else if (result == DoorActionResult.Success)
            {
                ShowFeedback("Door opened");
            }
            else if (result == DoorActionResult.Blocked)
            {
                ShowFeedback("Door won't budge...");
            }
            else if (result == DoorActionResult.AnimationInProgress)
            {
                ShowFeedback("Wait for door to finish...");
            }
        }
        
        // === CASE 2: Door is OPENED ===
        else if (currentDoor.currDoorState == DoorState.Opened)
        {
            DoorActionResult result = currentDoor.TryClose();

            if (result == DoorActionResult.Success)
            {
                ShowFeedback("Door closed");
            }
            else if (result == DoorActionResult.ObstructionDetected)
            {
                ShowFeedback("Something is blocking the door");
            }
            else if (result == DoorActionResult.Blocked)
            {
                ShowFeedback("Door won't budge...");
            }
        }
        
        // === CASE 3: Door is ANIMATING ===
        else if (currentDoor.isAnimatingDoorPanel)
        {
            ShowFeedback("Wait for door to finish moving...");
        }
    }

    /// <summary>
    /// Handles L key - Toggles lock state (lock if unlocked, unlock if locked).
    /// For common locks: side doesn't matter.
    /// For separate locks: uses player's current side.
    /// </summary>
    private void HandleLockToggle()
    {
        if (currentDoor == null) return;

        // Only lock/unlock closed doors
        if (currentDoor.currDoorState != DoorState.Closed)
        {
            ShowFeedback("Close the door first");
            return;
        }

        // Get player's side (or Any for common locks)
        LockSide playerSide = GetPlayerSideOfDoor(currentDoor);

        // Check current lock state on player's side
        bool isCurrentlyLocked = IsLockLockedOnSide(currentDoor, playerSide);

        if (isCurrentlyLocked)
        {
            // === UNLOCK ===
            string playerKey = GetPlayerKeyForDoor(currentDoor);

            DoorActionResult result = currentDoor.TryUnlock(playerSide, playerKey);

            if (result == DoorActionResult.Success)
            {
                ShowFeedback($"Unlocked {GetSideName(playerSide)} lock");
            }
            else if (result == DoorActionResult.WrongKeyToUnlock)
            {
                ShowFeedback($"Wrong key! Need: {currentDoor.key}");
            }
            else if (result == DoorActionResult.AlreadyInState)
            {
                ShowFeedback("Already unlocked");
            }
            else if (result == DoorActionResult.AnimationInProgress)
            {
                ShowFeedback("Lock is moving...");
            }
        }
        else
        {
            // === LOCK ===
            DoorActionResult result = currentDoor.TryLock(playerSide);

            if (result == DoorActionResult.Success)
            {
                ShowFeedback($"Locked {GetSideName(playerSide)} lock");
            }
            else if (result == DoorActionResult.UnlockedJam)
            {
                ShowFeedback("Lock is jammed");
            }
            else if (result == DoorActionResult.AlreadyInState)
            {
                ShowFeedback("Already locked");
            }
            else if (result == DoorActionResult.AnimationInProgress)
            {
                ShowFeedback("Lock is moving...");
            }
        }
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================
    
    /// <summary>
    /// Gets which side of the door the player is on.
    /// Uses stored dictionary from trigger detection.
    /// </summary>
    private LockSide GetPlayerSideOfDoor(DoorBase door)
    {
        // For common locks, side doesn't matter
        if (door.usesCommonLock)
            return LockSide.Any;

        // Return stored side from trigger detection
        if (doorSides.TryGetValue(door, out LockSide side))
            return side;

        // Fallback: shouldn't happen if triggers are set up correctly
        Debug.LogWarning("[DoorInteraction] No side info for door - defaulting to Outside".colorTag("yellow"));
        return LockSide.Outside;
    }

    /// <summary>
    /// Checks if the lock on player's side is locked.
    /// For common locks, checks outside lock (since inside is jammed).
    /// </summary>
    private bool IsLockLockedOnSide(DoorBase door, LockSide side)
    {
        if (door.usesCommonLock)
        {
            // Common lock uses outside lock state
            return door.currOutsideLockState == DoorLockState.Locked;
        }

        if (side == LockSide.Inside)
            return door.currInsideLockState == DoorLockState.Locked;
        else
            return door.currOutsideLockState == DoorLockState.Locked;
    }

    /// <summary>
    /// Gets the key from player inventory that matches this door.
    /// Returns empty string if player doesn't have the key.
    /// </summary>
    private string GetPlayerKeyForDoor(DoorBase door)
    {
        // If door doesn't need a key
        if (string.IsNullOrEmpty(door.key))
            return "any";

        // Check inventory
        if (playerInventory != null)
        {
            if (playerInventory.HasKey(door.key))
                return door.key;

            // Check for master key
            if (playerInventory.HasKey("master_key"))
                return "master_key";
        }

        return ""; // Player doesn't have key
    }

    /// <summary>
    /// Returns friendly name for lock side.
    /// </summary>
    private string GetSideName(LockSide side)
    {
        if (currentDoor != null && currentDoor.usesCommonLock)
            return "common";
        
        return side == LockSide.Inside ? "inside" : "outside";
    }

    /// <summary>
    /// Waits, then tries to open door (for unlock → open flow).
    /// </summary>
    private System.Collections.IEnumerator OpenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentDoor != null)
            currentDoor.TryOpen();
    }

    // ====================================================================
    // UI FEEDBACK
    // ====================================================================
    
    /// <summary>
    /// Updates contextual prompt based on door state.
    /// </summary>
    private void UpdatePrompt()
    {
        if (promptText == null) return;

        if (currentDoor == null)
        {
            promptText.text = "";
            return;
        }

        string prompt = "";
        LockSide playerSide = GetPlayerSideOfDoor(currentDoor);
        bool isLocked = IsLockLockedOnSide(currentDoor, playerSide);

        // Build prompt based on door state
        if (currentDoor.currDoorState == DoorState.Closed)
        {
            if (isLocked)
            {
                string playerKey = GetPlayerKeyForDoor(currentDoor);
                if (playerKey != "")
                    prompt = $"[{interactKey}] Unlock & Open";
                else
                    prompt = $"[Locked] Need: {currentDoor.key}";
            }
            else
            {
                prompt = $"[{interactKey}] Open Door";
            }

            // Add lock option
            if (currentDoor.canBeLocked)
            {
                string lockAction = isLocked ? "Unlock" : "Lock";
                prompt += $"\n[{lockKey}] {lockAction}";
            }
        }
        else if (currentDoor.currDoorState == DoorState.Opened)
        {
            prompt = $"[{interactKey}] Close Door";
        }
        else if (currentDoor.isAnimatingDoorPanel)
        {
            prompt = "Door is moving...";
        }

        promptText.text = prompt;
    }

    /// <summary>
    /// Shows temporary feedback message.
    /// </summary>
    private void ShowFeedback(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[DoorInteraction] {message}".colorTag("lime"));
        
        // TODO: Hook up to your UI feedback system
        // Examples: subtitle text, audio cue, screen message
    }
}

// ====================================================================
// PLACEHOLDER INVENTORY SYSTEM
// ====================================================================

/// <summary>
/// Placeholder - replace with your actual inventory!
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private string[] keysOwned = { "office_key", "master_key" };

    public bool HasKey(string keyName)
    {
        foreach (string key in keysOwned)
        {
            if (key.ToLower() == keyName.ToLower())
                return true;
        }
        return false;
    }

    public void AddKey(string keyName)
    {
        Debug.Log($"[Inventory] Added key: {keyName}".colorTag("lime"));
    }
}
```




