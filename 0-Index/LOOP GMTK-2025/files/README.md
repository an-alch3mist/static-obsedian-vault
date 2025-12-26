# Python-Like Interpreter for Unity

A complete, production-ready Python interpreter implementation for Unity that runs entirely in coroutines for seamless integration with Unity's API. Designed for programming puzzle games like "Farmer Was Replaced".

## Features

### Core Language Features
- ✅ **Variables & Types**: Numbers (double), Strings, Booleans, None, Lists, Dictionaries
- ✅ **Operators**: Arithmetic (+, -, *, /, %, **), Comparison (==, !=, <, >, <=, >=)
- ✅ **Bitwise**: &, |, ^, ~, <<, >>
- ✅ **Control Flow**: if/elif/else, while, for...in, break, continue, pass
- ✅ **Functions**: def with parameters, return, recursion (up to 1000 depth)
- ✅ **Classes**: class definitions, __init__, methods, self reference
- ✅ **Advanced**: List comprehensions, Lambda expressions, Global declarations
- ✅ **String Methods**: split(), strip(), replace(), join(), lower(), upper()
- ✅ **List Methods**: append(), remove(), pop(), sort(key=, reverse=)
- ✅ **Dict Methods**: get(), keys(), values()

### Unity Integration
- ✅ **Coroutine-Based**: Runs in IEnumerator with time-slicing (no frame drops)
- ✅ **Yielding Support**: Game commands return YieldInstructions (WaitForSeconds, etc.)
- ✅ **Thread-Safe**: Single-threaded, no background threads
- ✅ **Error Handling**: Line-accurate error reporting with context
- ✅ **.NET 2.0 Compatible**: Works with Unity 2020.3+ legacy scripting

### Game Features
- ✅ **Movement**: move(direction), get_pos(), is_passable()
- ✅ **Actions**: harvest(), sleep(seconds), say(text)
- ✅ **Queries**: get_grid_size(), is_goal(), can_move()
- ✅ **Built-ins**: print(), range(), len(), min(), max(), abs(), str(), int()

## File Structure

```
PythonInterpreter/
├── TokenType.cs          # Token type enumeration
├── Token.cs              # Token class with line numbers
├── Lexer.cs              # Tokenizer with indentation handling
├── AST.cs                # Abstract Syntax Tree node definitions
├── Parser.cs             # Recursive descent parser
├── PythonInterpreter.cs  # Main execution engine (~2000 lines)
├── CoroutineRunner.cs    # Safe execution wrapper
├── GameBuiltinMethods.cs # Game-specific functions
├── ConsoleManager.cs     # UI console for output
└── DemoScripts.cs        # Test scripts & examples
```

## Setup Instructions

### 1. Import Files
1. Create a folder: `Assets/Scripts/PythonInterpreter/`
2. Copy all `.cs` files into this folder
3. Ensure namespace is set to `PythonInterpreter` in all files

### 2. Create Scene Hierarchy
```
Scene
├── ScriptRunner (GameObject)
│   ├── CoroutineRunner (Component)
│   ├── GameBuiltinMethods (Component)
│   └── ConsoleManager (Component)
├── Canvas
│   └── ConsolePanel
│       ├── ScrollView (ScrollRect)
│       │   └── Viewport
│       │       └── ConsoleText (TextMeshProUGUI)
│       └── Scrollbar
└── Player (GameObject with Transform)
```

### 3. Configure Components

**CoroutineRunner:**
- Assign `ConsoleManager` reference

**GameBuiltinMethods:**
- Assign `CoroutineRunner` reference
- Assign `PlayerTransform` reference
- Set `GridWidth` and `GridHeight`

**ConsoleManager:**
- Assign `ConsoleText` (TextMeshProUGUI) reference
- Assign `ScrollRect` reference
- Set `MaxLines` (default: 1000)
- Enable `AutoScroll`

### 4. Create Script Input UI
Add an `InputField` or `TMP_InputField` for code input and a "Run" button:

```csharp
public class ScriptInputUI : MonoBehaviour
{
    public TMP_InputField codeInput;
    public CoroutineRunner runner;

    public void OnRunButtonClick()
    {
        runner.RunScript(codeInput.text);
    }

    public void OnStopButtonClick()
    {
        runner.Stop();
    }
}
```

## Usage Examples

### Basic Script
```python
# Variables and math
x = 10
y = 20
print("Sum:", x + y)

# Loops
for i in range(5):
    print("Count:", i)
```

### Classes and Objects
```python
class Robot:
    def __init__(self, name):
        self.name = name
        self.energy = 100
    
    def move(self, distance):
        self.energy -= distance * 2
        print(self.name, "moved", distance, "units")

bot = Robot("Explorer")
bot.move(5)
print("Energy:", bot.energy)
```

### Game Integration
```python
# Get current position
pos = get_pos()
print("Position:", pos)

# Move and wait
say("Moving right!")
move("right")
sleep(0.5)

# Pathfinding logic
while not is_goal(get_pos_x(), get_pos_y()):
    if can_move("right"):
        move("right")
    else:
        move("up")
    sleep(0.3)

say("Goal reached!")
```

### List Comprehensions
```python
# Generate odd squares
nums = [1, 2, 3, 4, 5]
squares = [x * x for x in nums if x % 2 == 1]
print(squares)  # [1, 9, 25]
```

### Lambda Functions
```python
# Sort objects by property
class Node:
    def __init__(self, val):
        self.val = val

nodes = [Node(10), Node(2), Node(5)]
nodes.sort(key=lambda n: n.val)
print([n.val for n in nodes])  # [2, 5, 10]
```

## API Reference

### Built-in Functions

#### I/O
- `print(*args)` - Print to console
- `say(text)` - Show speech bubble

#### Math
- `abs(x)` - Absolute value
- `min(*args)` or `min(list)` - Minimum value
- `max(*args)` or `max(list)` - Maximum value
- `int(x)` - Convert to integer
- `float(x)` - Convert to float
- `str(x)` - Convert to string

#### Collections
- `len(obj)` - Length of list/string/dict
- `range(stop)` or `range(start, stop)` or `range(start, stop, step)`

### Game Functions

#### Movement
- `move(direction)` - Move player ("up", "down", "left", "right")
- `harvest()` - Harvest at current position

#### Position
- `get_pos()` - Returns [x, y]
- `get_pos_x()` - Returns x coordinate
- `get_pos_y()` - Returns y coordinate

#### Grid
- `get_grid_size()` - Returns [width, height]
- `get_grid_width()` - Returns width
- `get_grid_height()` - Returns height

#### Queries
- `is_passable(x, y)` - Check if tile is walkable
- `is_block(x, y)` - Check if tile is blocked
- `is_goal(x, y)` - Check if position is goal
- `can_move(direction)` - Check if can move in direction

#### Timing
- `sleep(seconds)` - Pause execution (yields)

#### Submission
- `submit(answer)` - Submit puzzle answer

### String Methods
- `str.split(delimiter)` - Split string
- `str.strip()` - Remove whitespace
- `str.replace(old, new)` - Replace substring
- `str.join(list)` - Join list elements
- `str.lower()` - Convert to lowercase
- `str.upper()` - Convert to uppercase
- `str.startswith(prefix)` - Check prefix
- `str.endswith(suffix)` - Check suffix

### List Methods
- `list.append(x)` - Add element
- `list.remove(x)` - Remove first occurrence (throws ValueError if not found)
- `list.pop(index=-1)` - Remove and return element (throws IndexError if out of bounds)
- `list.sort(key=None, reverse=False)` - Sort in place

### Dictionary Methods
- `dict.get(key, default=None)` - Get value with default
- `dict.keys()` - Get list of keys
- `dict.values()` - Get list of values

## Advanced Features

### Slicing
```python
items = [0, 1, 2, 3, 4, 5]
print(items[1:4])   # [1, 2, 3]
print(items[:3])    # [0, 1, 2]
print(items[3:])    # [3, 4, 5]
print(items[-1])    # 5 (negative indexing)
```

### Nested Data Structures
```python
# Dictionary in list
data = [
    {"name": "Alice", "scores": [95, 87, 92]},
    {"name": "Bob", "scores": [88, 91, 85]}
]

print(data[0]["name"])         # Alice
print(data[0]["scores"][1])    # 87
```

### Recursion Example
```python
def fibonacci(n):
    if n <= 1:
        return n
    return fibonacci(n-1) + fibonacci(n-2)

print(fibonacci(10))  # 55
```

### A* Pathfinding Template
See `DemoScripts.cs` → `Script7_Pathfinding` for a complete A* implementation.

## Error Handling

The interpreter provides detailed error messages with line numbers:

```
Error at line 5: Undefined variable: x
Error at line 12: List index out of range
Error at line 18: Division by zero
Error at line 23: IndentationError: inconsistent indentation
```

Errors automatically:
1. Stop script execution
2. Display in console (red text)
3. Reset interpreter state
4. Preserve console history

## Performance Notes

### Time-Slicing
- Executes 250 operations per frame by default
- Prevents freezing during infinite loops
- Automatically yields to Unity every 250 ops

### Recursion Depth
- Supports deep recursion (tested to 1000 levels)
- Suitable for DFS, flood fill, recursive algorithms

### Memory
- All data stored as `List<object>` and `Dictionary<object, object>`
- Efficient for game-scale data (tested with 10,000+ elements)

## Testing

Run demo scripts from `DemoScripts.cs`:

```csharp
// In your test script
public CoroutineRunner runner;

public void TestScript(int index)
{
    string script = DemoScripts.GetScript(index);
    runner.RunScript(script);
}
```

Available test scripts:
1. Kitchen Sink (Lists, Slicing, Indexing)
2. Deep Diver (Nested Loops, Bitwise, Recursion)
3. Object Oriented (Classes, Methods)
4. Data Structures (Dicts, Nesting)
5. Game Interaction (Yielding, Commands)
6. Advanced Features (Comprehensions, Lambdas)
7. Pathfinding (A* Algorithm)

## Limitations

### .NET 2.0 Constraints
- Cannot `yield return` inside `try-catch` in IEnumerator
- Solution: Use flag-based error handling (already implemented)

### Not Supported
- Python modules/imports (import, from)
- Decorators (@decorator)
- Generators (yield)
- Multiple inheritance
- Operator overloading (custom classes)
- Exception handling (try/except)
- With statements (with/as)

### Differences from Python
- All numbers stored as `double` internally
- Dictionary keys use C# equality (not Python hash)
- String immutability enforced
- No GIL or threading

## Troubleshooting

### "Undefined variable" Error
- Check for typos in variable names
- Ensure variable is defined before use
- Use `global` for variables defined in outer scope

### "IndentationError"
- Use consistent indentation (4 spaces recommended)
- Don't mix tabs and spaces
- Ensure colons (`:`) after `if`, `def`, `class`, etc.

### Script Doesn't Yield
- Ensure game functions return `YieldInstruction`
- Check that `move()`, `sleep()` are properly called
- Verify `CoroutineRunner` is set up correctly

### Console Not Showing Output
- Check `ConsoleManager` references in Inspector
- Verify `ConsoleText` is assigned
- Enable `AutoScroll` in ConsoleManager

### Game Functions Not Working
- Ensure `GameBuiltinMethods.Start()` has been called
- Check `CoroutineRunner` reference is assigned
- Verify `PlayerTransform` is set

## Extension Guide

### Adding Custom Built-in Functions

In `CoroutineRunner.cs` → `RegisterBuiltins()`:

```csharp
interpreter.RegisterBuiltin("my_function", (args) =>
{
    // Your implementation
    // args is List<object>
    // Return object or YieldInstruction
    
    if (args.Count < 1)
        throw new RuntimeError(0, "my_function() requires 1 argument");
    
    double value = interpreter.ConvertToDouble(args[0]);
    // Do something with value
    
    return result; // or return new WaitForSeconds(1f);
});
```

### Adding Custom Methods to Objects

Edit `StringMethod`, `ListMethod`, or `DictMethod` classes in `PythonInterpreter.cs`.

### Custom Classes in C#

Register C# objects as Python classes:

```csharp
// Create a wrapper
interpreter.SetVariable("MyObject", myObjectInstance);

// Access in Python:
# my_script.py
value = MyObject.SomeProperty
MyObject.SomeMethod(10, 20)
```

## Credits

Developed for Unity 2020.3+ with .NET 2.0 compatibility.
Designed for "Farmer Was Replaced" style programming puzzle games.

## License

This interpreter is provided as-is for game development purposes.
