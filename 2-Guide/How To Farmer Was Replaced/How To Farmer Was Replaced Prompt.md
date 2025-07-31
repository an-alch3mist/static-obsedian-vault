
You are an expert Unity C# engineer. I will optionally supply you with two existing files (`PythonTokenStatement.cs` and `PythonStyleParser.cs`) for reference—they work but fail on nested function calls, inline expressions inside `print()`, proper `return` propagation, for-loops over lists, etc. **Ignore their limitations** and generate a **complete**, **production-ready** “Python-style” mini-language parser & executor library for **Unity 2020.3**, organized under namespace `GptDeepResearch`. All code must be drop-in ready, with no omissions or placeholder ellipses.

---

## 0. Existing Files (for reference only)
- `PythonTokenStatement.cs`
- `PythonStyleParser.cs`

These currently cannot handle:
- Nested function calls in expressions
- Arithmetic or calls inline inside `print()`
- Proper `return` type propagation
- List iteration via `for` loops

Your implementation must fix all these.

---

## 1. Goal  
Create a Unity component that reads a Python-like script (indent/colon syntax) from a **TextMeshPro InputField**, executes it when the user clicks a **Run** button, and displays errors (syntax/runtime) in a separate **TextMeshPro** output field. All script output should go to Unity’s console via `Debug.Log`.

---

## 2. Integration Points (UI)
- A MonoBehaviour with:
  - `public TMP_InputField scriptInput;`
  - `public TMP_InputField errorOutput;`
  - `public Button runButton;`
- On `runButton.onClick`, parse & execute `scriptInput.text`, then write any errors to `errorOutput.text`.

---

## 3. Namespace & File Structure  
**Root namespace:** `GptDeepResearch`

Deliver each file in its own code fence, named accurately. At minimum provide:
- `PythonToken.cs`  
- `PythonLexer.cs`  
- `PythonAST.cs`  
- `PythonParser.cs`  
- `PythonInterpreter.cs`  
- `CoroutineRunner.cs`  
- `ScriptRunner.cs` (the MonoBehaviour handling TMP UI)  
- Any additional utility classes

---

## 4. Lexing & Parsing Requirements

### 4.1 Lexer  
- Tokenize: identifiers, numbers, strings, booleans, keywords  
  (`if, elif, else, while, for, def, return, break, continue, global, and, or, not`)  
- Symbols: operators, `(`, `)`, `:`, `,`, `NEWLINE`, `INDENT`, `DEDENT`  
- Map leading spaces to `INDENT`/`DEDENT`

### 4.2 Parser  
- Build AST types:
  - **Statements:** `Assignment`, `ExprStmt`, `IfStmt`, `ElifStmt`, `ElseStmt`, `WhileStmt`, `ForStmt`, `FuncDef`, `ReturnStmt`, `BreakStmt`, `ContinueStmt`, `GlobalStmt`
  - **Expressions:** `Literal`, `VarExpr`, `Unary`, `Binary`, `Call`, `ListExpr`
- Correct block nesting via `INDENT`/`DEDENT`

---

## 5. Language Features

1. **Variables & Types**: `int`, `float`, `string`, `bool`, dynamic `list` (with `append`, `remove`, optional indexing).
2. **Lists**:
	- Dynamic, heterogenous (e.g. `[1, "a", True]`)
	- **Indexing**: `xs[0]`, `xs[-1]`
	- **Slicing**: `xs[2:]`, `xs[:3]`, `xs[1:4]`
	- Methods: `append`, `remove`, `pop`, etc.
3. **Operators**:
   - Arithmetic: `+`, `-`, `*`, `/`, `//`, `%`
   - Comparison: `==`, `!=`, `<`, `>`, `<=`, `>=`
   - Logical: `and`, `or`, `not`
   - Bitwise: `&`, `|`, `^`, `~`
   - Assignment: `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `|=`, `^=`
3. **Control Flow**:
   - `if` / `elif` / `else`
   - `while` loops
   - `for x in list:` loops
   - `break`, `continue`, `return`
4. **Functions**:
   - **Built-ins**: `print`, `len`, etc., registered in `Dictionary<string, Func<object[],object>>`
   - **User-defined**: via `def name(params):` stored separately with proper local scope
   - Support nested calls and inline expressions (e.g. `print(_A(_A(1)+_A(2)))`)

---

## 6. Error Handling

- **Syntax errors** (unexpected token, missing colon/paren, indent mismatch):
  - **Do not throw**—append descriptive messages to a public `string ErrorLog`, `Debug.LogError(ErrorLog)`, and display in `errorOutput.text`.
- **Runtime errors** (undefined name, type mismatch): same flow.

```csharp
ErrorLog = "[Syntax Error] Missing ':' at line 4";
// Debug.LogError(ErrorLog);
errorOutput.text = ErrorLog;
````

---

## 7. Unity & Coroutine Constraints

* **Unity 2020.3**: **no** `yield return` inside any `try{ … } catch{ … }` block.

* Provide helper class:

  ```csharp
  public static class CoroutineRunner
  {
      public static IEnumerator SafeExecute(
          IEnumerator routine,
          float stepDelay,
          Action<string> onError
      );
      public static IEnumerator Wait(float seconds);
  }
  ```

* In your main runner (`ScriptRunner.cs`), expose:

  ```csharp
  public float stepDelay = Time.deltaTime;
  ```

* Use only `CoroutineRunner.Wait(stepDelay)` for delays—**always** outside of `try/catch`.

---

## 8. Examples to Validate

1. **Basic arithmetic & functions**

   ```python
   a = 1
   b = 10
   print(a + b)                 # → 11
   def _A(x):
       return x * 2
   print(_A(1 + 1))             # → 2
   print(_A(_A(_A(1) + _A(2))))  # → 24
   ```

2. **Lists & for-loops**(list can also have multiple types (string, bool, int) similar to python) also 

   ```python
   xs = [1, 2, 3]
   for x in xs:
       print(x * 2)
   ```

   Unity Console:(do improve unity console such as function called while loop entered if loop entered, exited etc to make it rich)

```
11
2
24
2
4
6
```

---

## 9. Expandability & Callability

* Design your interpreter so others can:

  * **Add new built-ins** via the `builtins` dictionary.
  * **Hook into execution** at each statement (e.g. via an optional callback).
  * **Extend AST** to support new statements or expressions.

---

## 10. Deliverables

* **Every** `.cs` file required, each in its own code fence labeled with its filename.
* Fully implemented, no omissions, under namespace `GptDeepResearch`.
* Ready to paste into a Unity 2020.3 project with TextMeshPro and UI button to run.

Now generate all the `.cs` files.
make sure you provide all the .cs files.
