## Overview

The system supports three types of commands that can be called from Python scripts:

1. **Actions** - Perform operations, no return value (like `move()`, `collect()`)
2. **Predicates** - Check conditions, return true/false (like `is_block()`, `can_move()`)
3. **Value Getters** - Get data immediately, return int/string/object (like `get_pos_x()`, `get_dialogue()`)

## 1. Actions (void return)

Actions perform operations and don't return values. They use coroutines for animated/timed operations.

### Registration Example:

```csharp
protected override void RegisterCommands()
{
    // Register an action that takes time to complete
    RegisterAction("move", MoveCommand);
    RegisterAction("collect", CollectCommand);
    RegisterAction("play_sound", PlaySoundCommand);
}
```

### Implementation Example:

```csharp
// Action that moves player over time
private IEnumerator MoveCommand(object[] args)
{
    if (args.Length != 1)
        throw new Exception("move() takes exactly 1 argument");

    string direction = args[0].ToString().ToLower();
    
    // Determine movement vector
    Vector3 moveVector = Vector3.zero;
    switch (direction)
    {
        case "up": moveVector = Vector3.up; break;
        case "down": moveVector = Vector3.down; break;
        case "left": moveVector = Vector3.left; break;
        case "right": moveVector = Vector3.right; break;
        default: throw new Exception($"Invalid direction: {direction}");
    }

    // Animate movement over time
    if (playerTransform != null)
    {
        Vector3 startPos = playerTransform.position;
        Vector3 endPos = startPos + moveVector;
        float elapsed = 0f;
        float duration = 1f / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            playerTransform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null; // Wait one frame
        }

        playerTransform.position = endPos; // Ensure exact final position
    }
    
    // Action completes, Python script continues to next line
}

// Action that plays sound instantly
private IEnumerator PlaySoundCommand(object[] args)
{
    if (args.Length != 1)
        throw new Exception("play_sound() takes exactly 1 argument");

    string soundName = args[0].ToString();
    
    // Play sound immediately
    AudioSource.PlayClipAtPoint(GetSoundClip(soundName), transform.position);
    
    // Even instant actions need to yield at least once
    yield return null;
}
```

### Python Usage:

```python
move("up")      # Blocks until movement animation completes
collect("gem")  # Blocks until collection animation completes
play_sound("coin")  # Plays sound, continues immediately
```

## 2. Predicates (bool return)

Predicates check conditions and return true/false. They use coroutines for complex checks that might take time.

### Registration Example:

```csharp
protected override void RegisterCommands()
{
    RegisterPredicate("is_block", IsBlockCommand);
    RegisterPredicate("can_move", CanMoveCommand);
    RegisterPredicate("has_item", HasItemCommand);
}
```

### Implementation Example:

```csharp
// Check if position is blocked
private IEnumerator IsBlockCommand(object[] args)
{
    if (args.Length != 2)
        throw new Exception("is_block() takes exactly 2 arguments");

    float x = Convert.ToSingle(args[0]);
    float y = Convert.ToSingle(args[1]);
    Vector3 checkPos = new Vector3(x, y, 0);

    // Simulate raycast or collision check (might take a frame)
    yield return null;

    // Perform the actual check
    Collider2D collider = Physics2D.OverlapPoint(checkPos);
    bool isBlocked = collider != null && collider.CompareTag("Block");

    // CRITICAL: Set the result using helper method
    SetPredicateResult("is_block", isBlocked);
}

// Check if player can move in direction
private IEnumerator CanMoveCommand(object[] args)
{
    if (args.Length != 1)
        throw new Exception("can_move() takes exactly 1 argument");

    string direction = args[0].ToString().ToLower();

    if (playerTransform == null)
    {
        SetPredicateResult("can_move", false);
        yield break;
    }

    Vector3 checkDirection = Vector3.zero;
    switch (direction)
    {
        case "up": checkDirection = Vector3.up; break;
        case "down": checkDirection = Vector3.down; break;
        case "left": checkDirection = Vector3.left; break;
        case "right": checkDirection = Vector3.right; break;
        default:
            SetPredicateResult("can_move", false);
            yield break;
    }

    // Check target position
    Vector3 targetPos = playerTransform.position + checkDirection;
    
    // Simulate complex pathfinding check
    yield return new WaitForSeconds(0.1f); // Example: pathfinding takes time
    
    // Check if target is valid
    bool canMove = !Physics2D.OverlapPoint(targetPos);// && IsInBounds(targetPos);
    
    // CRITICAL: Always set the result
    SetPredicateResult("can_move", canMove);
}

// Instant check that still uses coroutine pattern
private IEnumerator HasItemCommand(object[] args)
{
    if (args.Length != 1)
        throw new Exception("has_item() takes exactly 1 argument");

    string itemName = args[0].ToString();
    
    // Even instant checks need to yield once
    yield return null;
    
    // Check inventory (example)
    bool hasItem = inventory.Contains(itemName);
    
    SetPredicateResult("has_item", hasItem);
}
```

### Python Usage:

```python
if is_block(1, 2):          # Blocks until check completes, returns bool
    print("Position is blocked")

if can_move("up"):          # Blocks until pathfinding completes
    move("up")
else:
    print("Cannot move up")

if has_item("key"):         # Quick check, still blocks briefly
    use_item("key")
```

## 3. Value Getters (immediate return)

Value getters return data immediately without coroutines. Use for simple property access.

### Registration Example:

```csharp
protected override void RegisterCommands()
{
    RegisterValueGetter("get_pos_x", GetPosXCommand);
    RegisterValueGetter("get_pos_y", GetPosYCommand);
    RegisterValueGetter("get_health", GetHealthCommand);
    RegisterValueGetter("get_dialogue", GetDialogueCommand);
    RegisterValueGetter("get_item_count", GetItemCountCommand);
}
```

### Implementation Example:

```csharp
// Get player X position
private object GetPosXCommand(object[] args)
{
    if (args.Length != 0)
        throw new Exception("get_pos_x() takes no arguments");
    
    return playerTransform != null ? (int)playerTransform.position.x : 0;
}

// Get player Y position
private object GetPosYCommand(object[] args)
{
    if (args.Length != 0)
        throw new Exception("get_pos_y() takes no arguments");
    
    return playerTransform != null ? (int)playerTransform.position.y : 0;
}

// Get current health
private object GetHealthCommand(object[] args)
{
    if (args.Length != 0)
        throw new Exception("get_health() takes no arguments");
    
    return playerHealth.currentHealth; // Returns int
}

// Get dialogue from NPC
private object GetDialogueCommand(object[] args)
{
    if (args.Length != 1)
        throw new Exception("get_dialogue() takes exactly 1 argument");
    
    string npcName = args[0].ToString();
    
    // Look up dialogue in dialogue system
    if (dialogueDatabase.ContainsKey(npcName))
    {
        return dialogueDatabase[npcName]; // Returns string
    }
    
    return "..."; // Default dialogue
}

// Get count of specific item
private object GetItemCountCommand(object[] args)
{
    if (args.Length != 1)
        throw new Exception("get_item_count() takes exactly 1 argument");
    
    string itemName = args[0].ToString();
    return inventory.Count(item => item.name == itemName); // Returns int
}
```

### Python Usage:

```python
x = get_pos_x()           # Returns immediately: int
y = get_pos_y()           # Returns immediately: int
health = get_health()     # Returns immediately: int

dialogue = get_dialogue("shopkeeper")  # Returns immediately: string
print(dialogue)

coin_count = get_item_count("coin")     # Returns immediately: int
print("You have", coin_count, "coins")

# Can use in expressions
if get_health() < 50:
    use_item("potion")

# Can use in calculations
total_pos = get_pos_x() + get_pos_y()
```

## Complete Scene Example

Here's a complete scene controller showing all three types:

```csharp
public class PlatformerSceneController : GameControllerBase
{
    [Header("Game Objects")]
    public Transform playerTransform;
    public AudioSource audioSource;
    
    private List<string> inventory = new List<string>();
    private int playerHealth = 100;
    private Dictionary<string, string> npcDialogues = new Dictionary<string, string>
    {
        {"guard", "Halt! Who goes there?"},
        {"merchant", "Welcome to my shop!"},
        {"wizard", "I sense great power in you..."}
    };

    protected override void RegisterCommands()
    {
        // Actions - take time, no return value
        RegisterAction("move", MoveCommand);
        RegisterAction("jump", JumpCommand);
        RegisterAction("attack", AttackCommand);
        
        // Predicates - may take time, return bool
        RegisterPredicate("can_move", CanMoveCommand);
        RegisterPredicate("is_enemy_nearby", IsEnemyNearbyCommand);
        RegisterPredicate("has_key", HasKeyCommand);
        
        // Value Getters - immediate, return data
        RegisterValueGetter("get_x", GetXCommand);
        RegisterValueGetter("get_y", GetYCommand);
        RegisterValueGetter("get_health", GetHealthCommand);
        RegisterValueGetter("talk_to", TalkToCommand);
    }

    // Action implementations...
    private IEnumerator MoveCommand(object[] args) { /* ... */ }
    private IEnumerator JumpCommand(object[] args) { /* ... */ }
    private IEnumerator AttackCommand(object[] args) { /* ... */ }
    
    // Predicate implementations...
    private IEnumerator CanMoveCommand(object[] args) { /* ... */ }
    private IEnumerator IsEnemyNearbyCommand(object[] args) { /* ... */ }
    private IEnumerator HasKeyCommand(object[] args) { /* ... */ }
    
    // Value getter implementations...
    private object GetXCommand(object[] args) { return (int)playerTransform.position.x; }
    private object GetYCommand(object[] args) { return (int)playerTransform.position.y; }
    private object GetHealthCommand(object[] args) { return playerHealth; }
    private object TalkToCommand(object[] args) 
    { 
        string npc = args[0].ToString();
        return npcDialogues.ContainsKey(npc) ? npcDialogues[npc] : "...";
    }
}
```

## Key Points

1. **Actions**: Use `IEnumerator`, must `yield return` at least once, no return value
2. **Predicates**: Use `IEnumerator`, must call `SetPredicateResult(commandName, bool)`, return bool
3. **Value Getters**: Return `object` directly, no coroutines, immediate execution
4. **Naming**: Use clear, descriptive names that match Python conventions
5. **Arguments**: Always validate argument count and types
6. **Syntax Highlighting**: Commands automatically appear in code editor highlighting