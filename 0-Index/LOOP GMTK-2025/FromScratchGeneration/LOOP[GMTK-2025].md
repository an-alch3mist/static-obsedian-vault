# Prompt 0.0:
    building a programming game("similar to farmer was replaced") which uses `python-like` prigramming language to be written inside game to solve the puzzle, btw its done in Unity3D engine, is there a C# to python compiler that works in side game so when game built for windows, linux, mac it works ? or do i need to write python-like compiler(simple which can handle loops, funcitons, nested func, loops, list dictionary class etc) from scratch. ?

    btw have build this for one of GMTK Jam Games(held for 3days) with help of Claude, ofcourse its simple compiler what you think ? what you think ?

# Response 0.0 Via Gemini 3.5 (+Pro)

This is a fantastic implementation, especially for a generic game jam project. You have effectively built a **Coroutine-based AST Interpreter**.

To answer your immediate question: **Stick with your custom compiler.**

While solutions like **IronPython** exist for running Python in C#, they are generally the **wrong choice** for a game like "Farmer Was Replaced" or "Human Resource Machine" in Unity.

Here is the evaluation of why your custom "from scratch" approach is better, followed by a critique of the code you provided.

### Why "Real" Python Engines (IronPython / Python.NET) are bad for this:

1. **The "Instant Execution" Problem**:
In a puzzle game, when a user writes `move_right()`, you want the game character to physically move, play an animation, and *then* execute the next line of code.
* **IronPython**: Executes the whole script in milliseconds. Your character would teleport to the end state instantly. To fix this, you would have to run Python on a separate thread and implement complex blocking/synchronization logic to wait for Unity animations.
* **Your Solution**: You are using `IEnumerator Execute(...)`. This is brilliant. It allows you to `yield return` inside the interpreter, effectively pausing the code execution while the game animation plays (`HandleSceneCommand`). This is the "Holy Grail" of coding games in Unity.


2. **IL2CPP & AOT Compatibility**:
* **IronPython**: Heavily relies on `Reflection.Emit` (generating code at runtime). This crashes on iOS, consoles, and sometimes WebGL/IL2CPP builds because these platforms require Ahead-of-Time (AOT) compilation.
* **Your Solution**: Your interpreter parses text into a tree (AST) and walks the tree. This is purely logical C# code. It works perfectly on Windows, Mac, Linux, iOS, Android, and WebGL without modification.


3. **Sandboxing**:
* **IronPython**: Gives users access to the full .NET framework by default. A user could write `import System.IO; File.Delete("C:/Windows/...")`. You have to work hard to lock it down.
* **Your Solution**: The user can *only* call functions you explicitly exposed (`BuiltinFunctions` and `GameBuiltinMethods`). It is secure by design.



---

### Code Review: What you did well

Looking at your source files (`PythonInterpreter.cs`, `PythonParser.cs`, `Lexer`), this is surprisingly robust for a 3-day build.

**1. The Coroutine Architecture (`PythonInterpreter.cs`)**
This is the strongest part of your codebase.

```csharp
// Inside ExecStmt
IEnumerator innerEnum = ExecStmt(inner);
while (innerEnum.MoveNext()) {
    yield return innerEnum.Current;
}
// Batched step delay
if (ExecutionTracker.ShouldYieldForBatch()) {
    yield return null;
}

```

* **Why it's good:** You are time-slicing the execution. The `ExecutionTracker` batching (lines 80-90) prevents the game from freezing if the user writes an infinite loop `while True: pass`, but still executes fast enough (100 ops per frame) to feel responsive.

**2. Handling Python Indentation (`PythonLexer.cs`)**

* You correctly implemented an `IndentStack` to track scope. Most simple parsers fail here, but yours correctly emits `INDENT` and `DEDENT` tokens, which makes parsing blocks (`if`, `def`, `class`) trivial in the Parser.

**3. Operator Precedence (`PythonParser.cs`)**

* Your updated `ParseExpression` chain (Logic `OR` -> `AND` -> Bitwise -> Compare -> Add -> Mult -> Power) is correct. Many jams implementations mess this up (e.g., making `2 + 3 * 4` equal `20`). Yours correctly handles it as `14`.

### Critical Missing Features / Bugs to Watch Out For

If you want to turn this into a full game, here are the limitations in your current code that you will eventually hit:

**1. Recursive Function calls will blow the Stack**

* **The Issue:** Your interpreter uses C# recursion (`ExecFunction` calls `ExecStmt` which calls `ExecFunction`).
* **Consequence:** If a user writes a recursive algorithm (e.g., recursive flood fill), they will hit a C# `StackOverflowException` which crashes the *Game*, not just the script.
* **Fix:** For a shipping game, you might eventually need a stack-based interpreter (iterative) rather than a recursive tree walker, but for "Farmer Was Replaced" complexity, your current approach is fine. Just ensure you limit recursion depth or wrap the `StartCoroutine` in a try-catch that handles StackOverflow (difficult in Unity).

**2. List/Object Reference Semantics (`PythonInterpreter.cs`)**

* Python lists are passed by reference.
* In `ExecExpr` -> `ListExpr`:
```csharp
List<object> list = new List<object>();
// ... fills list ...
setValue(list);

```


* This looks correct (C# `List<object>` is a reference type). If the user does `a = [1]; b = a; b.append(2)`, `a` will become `[1, 2]`. **Good job.**

**3. Missing "Pythonic" Features**
Based on your parser, you are missing these features that Python users often expect:

* **List Comprehensions:** `[x for x in list]` (This is hard to parse but very popular).
* **Slicing with Steps:** `list[start:end:step]` (Your `SliceExpr` seems to handle start/end, but check if `step` is fully supported in the logic).
* **Standard Library:** You have `range`, `len`, `print`, `str`, `int`. You will need `abs`, `min`, `max`, `round` soon.

### Verdict & Next Step

**Do not rewrite this.** Do not look for an external library. You have built exactly what you need: a **Domain Specific Language (DSL)** that looks like Python but behaves like a Unity Game Controller.

**Next Step for you:**
I noticed your `Interpreter` handles variables, but your **Scope Management** (`LocalsStack`, `Globals`) might get tricky with **Classes**.

In `PythonInterpreter.cs`, `ClassInstanceValue` and `ClassDefStmt` are handled, but ensure that `self` context works correctly. When a method is called on an instance, you need to ensure `self` is implicitly passed and that instance variables (`self.x`) are stored in the instance, not the local function scope.

Currently, you handle `self` here:

```csharp
// Inside ExecExpr (CallExpr)
if (instanceObj.Class.Methods.ContainsKey(method)) {
    // ...
    List<object> methodArgs = new List<object> { instanceObj }; // 'self' injection
    // ...
}

```

This looks correct!

**You are on the right track.** This code is production-capable for a puzzle game.



# Prompt 0.1:
    Act As Professional Prompt Engineer to Generate Prototype That has all the features for the above without missing any of detail, feel free to modify the files(file names to be generated and content it holds inside as you seem fit)
    could you provide 
    # detailed prompt(for gemini 3.5 pro or claude 4.5 etc) if i were to generate the game from scratch, a python(with existing features along with other features required ), feel free to modify the file structure required, so that in future just by providing this prompt which provide the detailed sourse of all along with summary on each critical dunction what it does exactly, (also keep the safeCheck coroutine so that it throw error inside game text not in unity console(which is already solved) and many more, i think you get it), make sure the prompt is more indepth as possible which covers every little detail aspect without leaving any .
    # critical: make sure you provide everything in detail that is close to python-like, for example print(str + list) will behave exactly how python does and provide the result into console. and many more as you seem fit.

# Response 0.1 Via Gemini 3.5 (+Pro):
Here is a comprehensive, professional-grade prompt designed to generate the complete, production-ready source code for a Unity-based "Python-like" coding game.

You can feed this prompt directly into **Claude 3.5 Sonnet**, **GPT-4o**, or **Gemini 1.5 Pro**. It is structured to force the AI to adopt the exact architecture you currently have (which is excellent) while ensuring strict adherence to "Pythonic" behaviors and robust error handling.

---

# The Professional Prompt

**Act as a Senior Unity Architect and Compiler Engineer.** I am building a programming puzzle game in Unity (similar to *Farmer Was Replaced*) where the player writes Python-like code to control a character. I need you to generate a robust, custom implementation of a Python interpreter in C# from scratch.

**Project Constraints & Architecture:**

1. **Engine:** Unity 2022+ (C#).
2. **Execution Model:** The interpreter must run as a **Coroutine (`IEnumerator`)**, not on a separate thread. This is critical to allow the script to `yield return` execution to the Unity engine for game actions (like `move()`) or delays (`sleep()`) without freezing the game.
3. **No External Libraries:** Do not use IronPython or Python.NET. This must be a pure C# Lexer/Parser/Interpreter implementation to ensure IL2CPP/AOT compatibility (iOS/WebGL support) and strict sandboxing.
4. **Error Handling:** All runtime errors must be caught and displayed in an **in-game console** (UI), never crashing the Unity Editor or Game.

**Required Features (Python-Strict Behavior):**

* **Syntax:** Indentation-based scoping (blocks defined by whitespace, not braces).
* **Types:** `int`, `float`, `bool`, `string`, `list`, `dict`, and `class` support.
* **Control Flow:** `if/elif/else`, `while`, `for`, `break`, `continue`, `return`.
* **Functions:** `def` with parameters, return values, and recursion support.
* **Classes:** Basic `class` definitions with methods and `self` context.
* **Operators:** Python precedence rules. Crucially, strictly enforce types like Python (e.g., `1 + "a"` should throw a TypeError, but `print(str(1) + "a")` works).
* **Built-ins:** `print()`, `range()`, `len()`, `str()`, `int()`, `float()`, `type()`.

---

## Deliverable 1: The Core Compiler Stack

Please generate the following C# classes with detailed comments explaining the flow.

### 1. `Token.cs` & `TokenType.cs`

* Define tokens for Python syntax (INDENT, DEDENT, NEWLINE, keywords like `def`, `class`, `if`, etc.).

### 2. `Lexer.cs`

* **Critical Requirement:** Implement an **Indentation Stack**. The lexer must track whitespace at the start of lines and emit `INDENT` and `DEDENT` tokens logically, just like Python.
* Handle comments (`#`) and multi-line strings (`"""`).

### 3. `AST.cs` (Abstract Syntax Tree)

* Define a node hierarchy: `Stmt` (Statement) and `Expr` (Expression).
* Include nodes for `FunctionDef`, `ClassDef`, `IfStmt`, `ForStmt`, `WhileStmt`, `ListExpr`, `DictExpr`.

### 4. `Parser.cs`

* Implement a Recursive Descent Parser.
* **Precedence:** Ensure correct order of operations (Logic OR -> Logic AND -> Compare -> Add/Sub -> Mult/Div).

---

## Deliverable 2: The Coroutine Interpreter

This is the most complex part. Generate `PythonInterpreter.cs` using the following logic:

* **Signature:** `public IEnumerator Execute(List<Stmt> statements)`
* **Scope Management:** Use a `Stack<Dictionary<string, object>>` for local variables and a separate `Dictionary` for Globals.
* **Yielding:**
* The interpreter loop must check an `ExecutionTracker` to batch instructions (e.g., run 100 instructions per frame, then `yield return null`) to prevent infinite loops from freezing Unity.
* When a "Game Function" (like `move()`) is called, the interpreter must `yield return` the specific game instruction and wait for it to complete.


* **Python Semantics:**
* Implement `ExecBinaryExpr` to handle type coercion strictly.
* Implement `ExecCallExpr` to handle both user-defined functions and built-in C# delegates.



---

## Deliverable 3: Unity Integration & Error Safety

Generate these wrapper classes to bridge the compiler to the GameObjects.

### 5. `CoroutineRunner.cs` (The Safety Net)

* Implement a `SafeExecute` method that wraps the Interpreter's `IEnumerator`.
* **Requirement:** It must try-catch **ALL** exceptions inside the `MoveNext()` loop.
* If an exception occurs (Lexer error, Parser error, or Runtime "ZeroDivisionError"), it should catch it, log it to the `ConsoleManager` (red text), and gracefully stop execution. **Do not let errors bubble up to Unity's console.**

### 6. `GameBuiltinMethods.cs`

* A static registry of functions exposed to Python (`move`, `turn`, `harvest`).
* Example: `move()` should return a `WaitForSeconds` or a custom yield instruction that the Interpreter passes up to Unity.

### 7. `ScriptRunner.cs` (MonoBehaviour)

* The component attached to the UI.
* Handles the "Run" and "Stop" buttons.
* **Sanitization:** Include a method to sanitize input text (remove BOM, normalize line endings) before sending to the Lexer.

### 8. `ConsoleManager.cs`

* A simple UI manager using `TMP_InputField` or `TMP_Text` to display `print()` outputs and Error messages.

---

## Summary of Critical Functions

For every major class, provide a summary comment block explaining exactly what it does. and seperate sections of C# into public, private API, Unity Life Cycle(only if its a MonoBehaviour Kind), Events, Enums.... For example:

* **Lexer:** "Reads raw string, tracks indentation depth using a stack, converts text to a stream of tokens."
* **Interpreter:** "Walks the AST using Coroutines. Manages the variable scope stack. Yields execution to Unity for time-sliced performance."

**Generate the code now.**

---

# Why this prompt works (Based on your Provided Code)

1. **The "Coroutine" Constraint:** You'll notice I heavily emphasized the `IEnumerator` and `yield return` structure. This ensures the AI generates an interpreter compatible with the `PythonInterpreter.cs` structure you currently use.
2. **The "SafeExecute" Requirement:** By explicitly asking for `SafeExecute` in `CoroutineRunner`, the AI will reproduce the pattern found in your `CoroutineRunner.cs` where exceptions are caught inside the iteration loop.
3. **Indentation Handling:** Explicitly mentioning the "Indentation Stack" ensures the Lexer properly handles the most difficult part of Python parsing, mirroring your `PythonLexer.cs` implementation.
4. **Batching:** The prompt requests `ExecutionTracker` logic to prevent infinite loops, ensuring the generated code includes the performance optimization found in your `ScriptRunner.cs` and `Interpreter`.

This prompt acts as a blueprint. If you lose your current files, this prompt will allow an AI to reconstruct a system functionally identical to the robust architecture you have today.


# Prompt 0.2:
    couple of changes:

    you havent included sleep(float) which does a yield return new WaitForSeconds() provided inside sleep as GameBuilt in method,
    also you think the print(list) or nested list and dictionary or class will print the details inside it as how python does in the current prompt ?,
    provide those and improvise prompt, and make sure to cover every single detail edge cases as possible without leaving any with ~ 500 lines(total prompt lines), you think the prompt might have missed. and provide the entire prompt again in depth for future high llm similar to yours or same level.
    [Paste Response 0.1 Via Gemini 3.5(+Pro)]

.
.
.
.

# Reponse 0.X

Here is the **Ultimate Master Architect Prompt** (approx. 1000+ words/lines of logic). I have significantly expanded the **"Validation Scripts"** section to cover every edge case you requested: slicing (`[1:]`, `[-1]`), bitwise ops, deep nesting, classes, and complex game loops.

**Copy and paste the entire block below into Claude 3.5 Sonnet or GPT-4o.**

---

# The Ultimate Unity "Farmer Was Replaced" Game Engine Prompt

**Role:** Act as a Principal Unity Architect and Compiler Language Engineer.
**Objective:** Architect and generate the **entire** C# source code for a production-grade "Python-like" coding game in Unity. This is not a toy; it is a robust, Coroutine-based Virtual Machine that handles complex user code, syncs with game animations, and provides a professional IDE-like experience.

---

## 1. Core Architecture & Constraints

* **Engine:** Unity 2022+ (.Net 2.0, C#).
* **Data Types:** All Python integers must be stored as long and floats as double to ensure high precision and 64-bit range.
* **Execution Model:** The Interpreter **MUST** run as a `IEnumerator`. It does *not* use threads. This allows strict frame synchronization with Unity's `Update` loop.
* **Sandboxing:** Pure C#. **NO** `System.Reflection` allowed for calling arbitrary C# code. **NO** external DLLs (IronPython/Python.NET).
* **IL2CPP Support:** The code must not use `Reflection.Emit`. It must be fully AOT compatible for iOS/WebGL.
* **Error Handling:** Exceptions must *never* crash the game. They must be caught, formatted, and displayed in an in-game console UI.
* **Text Sanitization:** The Lexer/Editor must aggressively sanitize invisible characters (`\v`, `\f`, `\0`, BOM) that often break parsers.
---

## 2. (Technical Deliverables) Deliverable 1: The Compiler Stack (The "Brain")

### A. `Token.cs` & `TokenType.cs`

* **Tokens:** `INDENT`, `DEDENT`, `NEWLINE`, `EOF`, `IDENTIFIER`, `STRING`, `NUMBER`.
* **Keywords:** `if`, `elif`, `else`, `while`, `for`, `def`, `return`, `class`, `break`, `continue`, `pass`, `global`, `and`, `or`, `not`, `in`, `None`, `True`, `False`.
* **Operators:** `+`, `-`, `*`, `/`, `%`, `==`, `!=`, `<`, `>`, `<=`, `>=`, `=`, `+=`, `-=`, `and`, `or`, `not`.
* **Bitwise Operators:** `&`, `|`, `^`, `<<`, `>>`, `~`.

### B. `Lexer.cs` (Indentation & Sanitization)

* **Sanitization Method:** Before tokenizing, implement `ValidateAndCleanText(string input)`:
* Replace `\r\n` with `\n`.
* Replace `\v` (vertical tab), `\f` (form feed), and `\uFEFF` (BOM) with spaces.


* **Indentation Stack:**
* Use `Stack<int>` to track indentation levels.
* Emit `INDENT` when whitespace > top of stack.
* Emit multiple `DEDENT`s when whitespace < top of stack.
* **CRITICAL:** Handle mixed tabs/spaces by throwing a clear `IndentationError`.



### C. `AST.cs` (Abstract Syntax Tree)

* **Nodes:** `FunctionDef`, `ClassDef`, `IfStmt`, `WhileStmt`, `ForStmt`, `ReturnStmt`, `AssignStmt`, `ExprStmt`.
* **Expressions:** `BinaryExpr`, `UnaryExpr`, `CallExpr`, `ListExpr` (`[]`), `DictExpr` (`{}`), `GetExpr` (`obj.prop`), `SetExpr`, `SliceExpr` (`a[1:2]`, `a[-1]`).

### D. `Parser.cs`

* Recursive Descent parser.
* **Precedence:** Logic OR < Logic AND < Bitwise < Equality < Comparison < Term < Factor < Unary < Call < Primary.
* **Syntax Sugar:** Support `+=`, `-=` by converting them to `AssignStmt` (e.g., `a = a + 1`).

### E. `PythonInterpreter.cs` (The VM)

**Signature:** `public IEnumerator Execute(List<Stmt> statements)`

1. **State Management:**
* `Stack<Dictionary<string, object>>` for local scopes.
* `Dictionary<string, object>` for Global scope.
* **Variables:** Variables must be passed by reference where appropriate (Lists, Dicts, Class Instances).


2. **Anti-Freeze System:**
* Maintain an `opsCounter`.
* Every 100 instructions, `yield return null` to let Unity render a frame.
* This prevents `while True: pass` from crashing the game.


3. **Game Synchronization:**
* When a built-in function (e.g., `move()`, `sleep()`) returns a `YieldInstruction` (like `WaitForSeconds` or a custom `Coroutine`), the Interpreter **MUST** `yield return` that instruction.


4. **"Pythonic" String Representation:**
* Implement `ToPythonString(object obj)` recursively.
* List: `[1, "a", [True]]` (Not `List<object>`).
* Dict: `{'x': 1}`.
* Class: `<__main__.Player object at 0x...>` unless `__str__` is defined.


5. **Execution Tracking:**
* Fire event `Action<int> OnLineExecuted` whenever a `Stmt` is executed. This drives the UI highlighting.



---

## 3. Deliverable 2: The IDE Experience (The "UI")

### F. `PythonCodeEditorSyntaxHighlight.cs`

**Responsibilities:** A professional code editor using `TMP_InputField`.

* **Twin-Overlay System:**
* `TMP_InputField` text color is transparent.
* A separate `TextMeshProUGUI` sits exactly on top to render syntax colors.


* **Syntax Highlighting (Regex):**
* **Keywords:** Purple (`#C678DD`).
* **Strings:** Green (`#98C379`).
* **Numbers:** Orange (`#D19A66`).
* **Comments:** Grey (`#5C6370`). Handle `#` single line and `"""` triple-quote multi-line.
* **Built-ins:** Red (`#E06C75`) for `print`, `len`, `range`, `sleep`.
* **Game Commands:** Dynamically highlight commands found in `GameBuiltinMethods` (e.g., `move`, `harvest`, `is_block`).


* **Sanitization:** On `OnValueChanged`, immediately strip `\v`, `\f`, and zero-width chars to prevent Lexer crashes.

### G. `InputFieldLineNumbers.cs`

**Responsibilities:** Sync line numbers and highlight the *currently executing* line.

* **Scroll Sync:** Use `ScrollRect.verticalNormalizedPosition` to keep the Line Number text aligned with the Input Field text.
* **Execution Highlight:**
* Subscribe to `Interpreter.OnLineExecuted`.
* When line `N` executes, re-render the Line Number text.
* Wrap line `N` in `<color=white>...</color>` and make others gray.



---

## 4. Deliverable 3: The Game System (The "Body")

### H. `GameControllerBase.cs` (Abstract)

**Responsibilities:** A base class for Level Logic.

* `RegisterAction(name, func)`: Registers a void function (e.g., `move()`).
* `RegisterPredicate(name, func)`: Registers a boolean checker (e.g., `is_wall()`).
* `RegisterValueGetter(name, func)`: Registers value retrievers.
* **Virtual Methods:** `SceneReset()`.

### I. `PerScene_1.cs` (Example Level)

**Responsibilities:** Implements a concrete puzzle.

* **Commands:**
* `move(dir)`: Animates the player. Yields `WaitForSeconds`.
* `say(text)`: Shows a UI bubble.
* `submit(val)`: Checks if the puzzle is solved.
* `is_goal(x, y)`: Returns true if coords match goal.
* `is_block(x, y)`: Checks against a list of block positions.


* **Logic:**
* Maintain `BLOCK_POS` list.
* Handle `submit()` logic: Play "Success" or "Fail" animation.



### J. `CoroutineRunner.cs` (The Safety Net)

* `IEnumerator SafeExecute(IEnumerator script)`:
* Wraps the interpreter loop in a `try/catch`.
* Catches `LexerError`, `ParserError`, `RuntimeError`.
* Logs errors to `ConsoleManager` (Red text).
* Stops execution gracefully without freezing Unity.



---

## 5. Comprehensive Test Suite (Python Scripts)

**IMPORTANT:** In your generated code, ensure the Interpreter is capable of running **ALL** of the following edge cases. These scripts define the feature set.

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

*Tests: Class definition, `self` reference, instance state.*

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
sleep(1.0) # Must yield WaitForSeconds(1.0)

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

---

## 6. Execution Instructions

1. **Generate all C# files.**
2. Ensure `PythonInterpreter.cs` includes specific logic to handle negative indices (`index < 0 ? count + index : index`) in `GetExpr` and `SliceExpr`.
3. Ensure `PythonInterpreter.cs` handles `sleep()` by returning the `yield` instruction up the stack.
4. Ensure `Parser.cs` correctly handles `list[start:end]` syntax.

**Generate the solution now.**


# Prompt X.X : https://gemini.google.com/share/3b21adcb5756 (latest) via `pewd3pi3@gmail.com`