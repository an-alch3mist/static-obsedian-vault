This is the **Ultimate Master Specification**. It incorporates **List Comprehensions**, **Custom Sorting Keys (Python style)**, **String Manipulation**, and the **Complete Extended Test Suite** (merging the Algorithm tests with the Feature tests).

Save this content as `PythonGameEnginePrompt.md`. You can feed this file directly to Claude 3.5 Sonnet, GPT-4o, or Gemini 1.5 Pro to generate the entire engine.
# inline prompt: Now Generate all required files for Unity3D 2020.3+ (.NET 2.0 limitations apply you cannot `yield return value/null inside try-catch clause of an IEnumerator`) after analysing the [pasted prompt] and  use `UTIL.cs` if you ever required.


# The Ultimate Unity "Python-Like" Game Engine Specification

**Role:** Act as a Principal Unity Architect and Compiler Language Engineer.
**Project:** "Farmer Was Replaced" Clone - A Programming Puzzle Game.
**Goal:** Generate the complete, production-ready C# source code for a Coroutine-based Python Interpreter in Unity.

---

## 1. System Architecture & Core Philosophy

### 1.1 Execution Model (The "Golden Rule")
* **Coroutine-Based VM:** The Interpreter (`PythonInterpreter.cs`) must run strictly within a Unity `IEnumerator`.
* **Single Thread:** No background threads. This ensures thread-safety with Unity's API (`Transform`, `GameObject`).
* **Time-Slicing:** The interpreter must process a fixed budget of operations (e.g., 250 ops) per frame. If the budget is exceeded, it must `yield return null` to prevent the game from freezing (e.g., during infinite loops).
* **Yield Propagation:** If a user script calls a "Game Command" (like `move()`), that command returns a `YieldInstruction` (e.g., `WaitForSeconds`). The Interpreter must pause and yield this instruction to Unity.
* **Error Context:** When an exception occurs, the Interpreter **MUST** catch it, append the **Current Line Number**, and re-throw a custom `RuntimeError`.

### 1.2 Data Types & Sandboxing
* **Numbers:** Store all numbers internally as `double` (or `long` where appropriate). Handle conversions strictly when calling Unity APIs.
* **Strings:** Fully supported (immutable).
* **Lists/Dicts:** Use `List<object>` and `Dictionary<object, object>` to mimic Python's dynamic typing.
* **Classes:** Support custom class definitions with methods and `self`.
* **Strict Sandboxing:** Pure C#(.Net 2.0 - meaning -> no yield return value/null inside `try-catch` clause which is inside `IEnumerator`). No `System.Reflection` to external assemblies. No `System.IO`.

---

## 2. Technical Specifications (The Compiler Stack)

### 2.1 `Token.cs` & `TokenType.cs`
* **Tokens:** `INDENT`, `DEDENT`, `NEWLINE`, `EOF`, `IDENTIFIER`, `STRING`, `NUMBER`.
* **Keywords:** `if`, `elif`, `else`, `while`, `for`, `def`, `return`, `class`, `break`, `continue`, `pass`, `global`, `and`, `or`, `not`, `in`, `True`, `False`, `None`, `lambda`.
* **Operators:** `+`, `-`, `*`, `/`, `%`, `==`, `!=`, `<`, `>`, `<=`, `>=`, `=`, `+=`, `-=`, `[`, `]`, `.`, `,`, `:`, `(`, `)`.
* **Bitwise:** `&`, `|`, `^`, `~`, `<<`, `>>`.

### 2.2 `Lexer.cs` (The Gatekeeper)
* **Sanitization (Critical):**
    * Method `ValidateAndClean(string input)`:
        * Replace `\r\n` with `\n`.
        * Replace `\t` (tabs) with 4 spaces (enforce consistency).
        * Remove `\v`, `\f`, `\uFEFF` (BOM) to prevent crashes.
* **Indentation Logic:**
    * Maintain a `Stack<int> indentStack`. Initialize with 0.
    * Calculate leading spaces of a line.
    * **Logic:**
        * If `current > stack.Peek()`: Push `current`, emit `INDENT`.
        * If `current < stack.Peek()`: Pop from stack and emit `DEDENT` until `stack.Peek() == current`. If no match found, throw `IndentationError`.
* **Tracking:** Each Token must store its `LineNumber` for error reporting.

### 2.3 `AST.cs` (The Structure)
* **Hierarchy:** `AstNode` -> `Stmt` / `Expr`.
* **Statements:**
    * `FunctionDef`, `ClassDef`, `IfStmt`, `WhileStmt`, `ForStmt`, `ReturnStmt`.
    * `AssignStmt` (target `Expr`, value `Expr`), `ExprStmt` (standalone calls), `GlobalStmt`.
* **Expressions:**
    * `BinaryExpr`, `UnaryExpr`, `CallExpr`, `ListExpr`, `DictExpr`.
    * `GetExpr` (property access), `SetExpr` (property set), `SliceExpr` (index/range).
    * `LiteralExpr`, `VariableExpr`, `SelfExpr`.
    * **ListCompExpr:** `[expr for var in iterable if condition]` (Python List Comprehension).
    * **LambdaExpr:** `lambda args: expr` (Anonymous functions).

### 2.4 `Parser.cs` (The Logic)
* **Recursive Descent:** Implement standard parsing functions (`ParseStatement`, `ParseExpression`).
* **Precedence:** `Logic OR` < `Logic AND` < `Bitwise` < `Equality` < `Comparison` < `Term` < `Factor` < `Unary` < `Call` < `Primary`.
* **Parsing Features:**
    * **List Comprehensions:** Detect `[` followed by an expression, then `for`. Parse the loop and optional `if` guard. Desugar this into a loop logic or handle explicitly in AST.
    * **Lambdas:** Parse `lambda` keyword, arguments, `:`, and single expression body.

---

## 3. The Runtime Engine (`PythonInterpreter.cs`)

### 3.1 State Management
* **Scopes:** `Stack<Scope>`. A `Scope` contains `Dictionary<string, object> variables`.
* **Globals:** Accessible from the bottom of the stack.
* **Classes:** Support `ClassInstance` objects which hold their own scope.
* **Error Context:** When an exception occurs, the Interpreter **MUST** catch it, append the **Current Line Number**, and re-throw a custom `RuntimeError`.

### 3.2 String Operations (Method Implementation)
Implement these methods when called on String objects:
* `.split(delimiter)`: Returns `List<object>`. Handles empty string or specific delimiters (e.g., `" "`, `"\n"`).
* `.strip()`: Removes whitespace.
* `.replace(old, new)`: Returns new string.
* `.join(iterable)`: Joins a list of strings using the caller string as separator (e.g., `", ".join(list)`).

### 3.3 List & Dictionary API (Critical Implementation)
You must implement these methods on the underlying C# `List<object>`:
* `append(x)`: Adds `x`.
* `remove(x)`: Finds first occurrence of `x` and removes it. **Throw ValueError** if not found.
* `pop(i)`: Removes item at index `i` (default -1). **Throw IndexError** if out of bounds.
* `len()`: Returns count.
* **`sort(key=None, reverse=False)`**:
    * If `key` is provided (as a Function or Lambda), map elements to values before sorting.
    * Example: `list.sort(key=lambda x: x.val)`.
    * Must handle stable sort if possible, or standard C# `.Sort()`.
* `min()`, `max()`: Returns min/max value.

### 3.4 Algorithms & Recursion
* **Recursion:** Support deep recursion (up to 1000 stack depth) for flood fill/DFS algorithms.
* **Pathfinding APIs:** The user will write A* in "Python". Ensure the interpreter handles strict comparisons (`<`, `>`) and boolean logic (`not in visited`) correctly.

---

## 4. Unity Integration & Safety

### 4.1 `CoroutineRunner.cs` (The Wrapper)
* `IEnumerator SafeExecute(IEnumerator script)`:
    * **Try/Catch Block:** Wrap `script.MoveNext()` in try/catch.
    * **Error Handling:**
        * On `LexerError`, `ParserError`, `RuntimeError`:
        * Extract the **Line Number** and **Error Message**.
        * Call `ConsoleManager.LogError($"Line {line}: {msg}")`.
        * **Reset State:** Call `Interpreter.Reset()` to clear variables/stack.
        * `yield break`.

### 4.2 `GameBuiltinMethods.cs`
* Register these functions:
    * `move(dir)`: Returns `WaitForSeconds`.
    * `harvest()`: Returns `WaitForSeconds`.
    * `get_pos()`: Returns `[x, y]`.
    * `get_grid_size()`: Returns `[width, height]`.
    * `is_passable(x, y)`: Returns `bool`.
    * `sleep(seconds)`: Returns `WaitForSeconds`.
    * `say(text)`: Displays UI text bubble.

### 4.3 `ConsoleManager.cs`
* UI for `print()` output.
* Support Rich Text (Red for errors, White for normal).
* Auto-scroll to bottom.

---

## 5. The Mega Test Suite (Validation Scripts)

**IMPORTANT:** Create a file `DemoScripts.cs` that contains these exact scripts strings. The Interpreter MUST run all of them successfully.

### Script 1: The Kitchen Sink (Lists, Slicing, Negative Indices)
*Tests: Negative indexing, slicing logic, append/remove, nested access.*
```python
# List Initialization
items = [0, 1, 2, 3, 4, 5]
print("Original:", items)

# Slicing
print("Slice [1:4]:", items[1:4])   # Expect: [1, 2, 3]
print("Slice [:3]:", items[:3])     # Expect: [0, 1, 2]
print("Slice [3:]:", items[3:])     # Expect: [3, 4, 5]

# Negative Indexing
print("Last item:", items[-1])      # Expect: 5
print("Second last:", items[-2])    # Expect: 4

# Modification
items.append(6)
items.pop() # Removes 6
items[0] = 99
print("Modified:", items)           # Expect: [99, 1, 2, 3, 4, 5]

```

### Script 2: The Deep Diver (Nested Loops, Bitwise, Recursion)

*Tests: 3-level nesting, bitwise math, recursive functions.*

```python
# Nested Loops (3 Levels)
count = 0
for i in range(3):
    for j in range(2):
        while count < 5:
            count += 1
            print("Loop depth:", i, j, count)

# Bitwise Operations
a = 60            # 0011 1100
b = 13            # 0000 1101
c = a & b         # 0000 1100 (12)
d = a | b         # 0011 1101 (61)
e = a ^ b         # 0011 0001 (49)
print("Bitwise AND:", c)

# Recursion
def fib(n):
    if n <= 1: return n
    return fib(n-1) + fib(n-2)

print("Fibonacci(6):", fib(6)) # Expect: 8

```

### Script 3: Object Oriented (Classes, Methods, Self)

*Tests: Class definition, self reference, instance state.*

```python
class Robot:
    def __init__(self, name):
        self.name = name
        self.battery = 100
    
    def work(self, cost):
        self.battery -= cost
        print(self.name + " working. Battery: " + str(self.battery))

bot = Robot("FarmerBot")
bot.work(10) # Expect: FarmerBot working. Battery: 90
bot.work(20) # Expect: FarmerBot working. Battery: 70

```

### Script 4: Data Structures (Dicts & Nesting)

*Tests: Dictionaries, Mixed Nesting (List in Dict, Dict in List).*

```python
# Dictionary
data = {"x": 10, "y": 20}
print(data["x"]) 

# Complex Nesting
complex_obj = [
    {"id": 1, "tags": ["a", "b"]}, 
    {"id": 2, "tags": ["c"]}
]
# Accessing deeply nested element
print(complex_obj[0]["tags"][1]) # Expect: "b"

# Modifying deep element
complex_obj[1]["tags"].append("d")
print(complex_obj[1]) # Expect: {'id': 2, 'tags': ['c', 'd']}

```

### Script 5: Game Interaction (Yielding, Sleep, Predicates)

*Tests: Unity integration, coroutine yielding, boolean logic.*

```python
say("Starting Mission...")
sleep(1.0) # Must yield return new WaitForSeconds(1.0)

x = get_pos_x()
y = get_pos_y()

while not is_goal(x, y):
    if is_block(x + 1, y):
        say("Obstacle ahead!")
        move("up")
        sleep(0.5)
    elif can_move("right"):
        move("right")
    else:
        say("Stuck!")
        break
    
    # Update coords
    x = get_pos_x()
    y = get_pos_y()

submit("password123")

```

### Script 6: Advanced Features (List Comp, Lambdas, String Ops)

*Tests: Pythonic syntax features and string manipulation.*

```python
# List Comprehension
nums = [1, 2, 3, 4, 5]
squares = [x * x for x in nums if x % 2 == 1]
print("Odd Squares:", squares) # Expect: [1, 9, 25]

# String Operations
text = "apple,banana,cherry"
fruits = text.split(",")
print("Splitted:", fruits) # Expect: ['apple', 'banana', 'cherry']

joined = " | ".join(fruits)
print("Joined:", joined) # Expect: "apple | banana | cherry"

# Lambda Sorting
class Node:
    def __init__(self, val):
        self.val = val

nodes = [Node(10), Node(2), Node(5)]
# Sort by value using Lambda
nodes.sort(key=lambda n: n.val)

print("Sorted Nodes:", [n.val for n in nodes]) # Expect: [2, 5, 10]

```

### Script 7: Pathfinding (A* Simulation)

*Tests: Deep algorithms, sets, priority queue logic (simulated with lists).*

```python
grid = [[0, 0, 0], [0, 1, 0], [0, 0, 0]]
start = [0, 0]
end = [2, 2]

def get_neighbors(pos):
    res = []
    x = pos[0]
    y = pos[1]
    if x > 0: res.append([x-1, y])
    if x < 2: res.append([x+1, y])
    if y > 0: res.append([x, y-1])
    if y < 2: res.append([x, y+1])
    return res

def heuristic(a, b):
    return abs(a[0] - b[0]) + abs(a[1] - b[1])

open_list = [start]
came_from = {}
g_score = {str(start): 0}
f_score = {str(start): heuristic(start, end)}

while len(open_list) > 0:
    # Simulating Priority Queue pop
    current = open_list[0]
    best_f = f_score.get(str(current), 999)
    
    for node in open_list:
        f = f_score.get(str(node), 999)
        if f < best_f:
            current = node
            best_f = f
            
    if current == end:
        print("Path Found!")
        break
        
    open_list.remove(current)
    
    for neighbor in get_neighbors(current):
        if grid[neighbor[1]][neighbor[0]] == 1:
            continue # Wall
            
        tentative_g = g_score.get(str(current), 999) + 1
        if tentative_g < g_score.get(str(neighbor), 999):
            came_from[str(neighbor)] = current
            g_score[str(neighbor)] = tentative_g
            f_score[str(neighbor)] = tentative_g + heuristic(neighbor, end)
            if neighbor not in open_list:
                open_list.append(neighbor)

```

---

## 6. Execution Instructions for the AI

1. **Generate ALL C# Files(as seperate `*.cs` make sure there is seperation and summary into region for public API, private API, unity life cycle(only if its monoBehaviour inherited), ENUMS, EVENTS and many more):** `Token.cs`, `Lexer.cs`, `AST.cs`, `Parser.cs`, `PythonInterpreter.cs`, `CoroutineRunner.cs`, `GameBuiltinMethods.cs`, `ConsoleManager.cs`, `DemoScripts.cs`.
2. **Strict Error Reporting:** In `PythonInterpreter.cs`, ensure every `Stmt` execution updates a `currentLine` variable. When `RuntimeError` is thrown, include this line number.
3. **Variable Reset:** Ensure `ScriptRunner.Stop()` or error handling calls `Interpreter.Reset()` to wipe `Globals` and `Scopes`.

**Generate the code now (covering all edge cases, and .Net 2.0 limitation that i mentioned at the begining) all in seperate files as required**
