Nice catch — thank you. I updated the Claude-4 prompt to explicitly include:

- comment support (`# single-line`) and docstrings (`"""triple-quoted"""`) in lexer + parser + AST (and storing docstrings on functions/classes),
    
- dictionary literal support (`{ "k": v, ... }`) and lookup semantics,
    
- a simple class system (class definition, `self`, `__init__`, method calls, attribute access) marked as **OPTIONAL / best-effort** if time permits.
    

Below is the full, revised prompt you can copy/paste into Claude 4. It contains the original MUST features A→E plus the new additions (comments/docstrings, dicts, classes), the files to include, deliverables, tests, and acceptance criteria. Paste it along with your project files (the ones listed previously).

---

You are an expert **Unity C#** engineer and language-embedding engineer.  
Target: **Unity 2020.3** project embedding a small **Python-like scripting interpreter**. Produce localized, minimal, and safe changes (no full interpreter rewrite). Provide concrete patches using `// add`, `// modify`, `// replace` for named files. Deliver compile-ready C# where possible and in-repo-compatible diffs. Include unit-test scripts in the embedded scripting language.

**Hard constraints**

- Unity 2020.3 environment.
    
- **Do not change** the rule: you **cannot** `return` a value inside a `try/catch` in an `IEnumerator`. If touching coroutine code, add comments and avoid `return` in try/catch blocks.
    
- Backwards compatibility: existing scripts using `[x,y]`, `get_pos()`, `move(x,y)` must continue to work.
    
- Keep changes localized to interpreter + builtins + small parser/lexer updates.
    

---

## Features to implement (MUST / High-impact)

Implement these precisely (A → E from earlier), plus the new language additions below.

### A. `v2(x,y)` + `V2Value`

- Add `V2Value` runtime type with `ToString()` => `v2(x, y)`.
    
- Register builtin `v2(x,y)` in `GameBuiltinMethods`.
    
- Preserve list literal `[1,0]` semantics and allow interoperability.
    

### B. Attribute access `.x` / `.y`

- Add AST node `AttributeAccessExpr` and parser support for `.` token (dot) if missing.
    
- Interpreter: `expr.x` / `expr.y`:
    
    - `V2Value` -> numeric component,
        
    - 2-element `ListValue` -> fallback to elements `[0]` / `[1]`.
        
- Clear error on other types.
    

### C. Binary vector ops (`+`, `-`) and `==`

- Implement `v2 + v2`, `v2 - v2`, `v2 == v2`.
    
- Allow `v2 + list` and `list + v2` via conversion.
    
- Preserve numeric semantics; **do not** allow vector+number by default.
    

### D. Auto-call zero-arg builtins

- If a resolved **builtin function** (not user function) with `Arity == 0` is used as a `NameExpr` in an expression, auto-call it and return result (e.g., `curr = get_pos` ≡ `curr = get_pos()`).
    
- Do **not** auto-call user-defined functions/closures.
    
- Implement in `PythonInterpreter` at name-resolution (`VisitName` or equivalent).
    
- Ensure `BuiltinFunctionValue` exposes `Arity` and `Invoke(Value[] args)`.
    

### E. Builtins accept `(x,y)` OR `v2` OR 2-element lists

- Update builtins (`move`, `is_block`, `get_pos`, `get_goal`, etc.) to normalize inputs:
    
    - `(x, y)` numeric arguments,
        
    - single `V2Value`,
        
    - single 2-element `ListValue`.
        
- Provide helper `NormalizeToXY(Value v)`.
    

---

## New additions (please implement; mark optional if time constrained)

### COMMENTS & DOCSTRINGS (REQUIRED)

- **Lexer**: support `#` single-line comments (ignore rest of line).
    
- **Lexer & Parser**: support triple-quoted docstrings `"""doc..."""` as string tokens that can appear immediately after `def` or `class` header.
    
- **AST**: Functions and classes should carry an optional `Docstring` attribute (string) populated from the first statement when it's a triple-quoted string.
    
- **Interpreter**: Do not execute docstrings as statements; store them on function/class objects for introspection.
    

### DICTIONARIES (BEST-EFFORT / HIGH VALUE)

- Syntax: dict literal `{ key_expr : value_expr, ... }`. Keys allowed: strings, numbers (or expressions that evaluate to strings/numbers).
    
- Runtime: `DictValue` (map from `Value`→`Value`) with `__getitem__`/indexing via `dict[key]` and assignment `dict[key] = value`.
    
- Support iteration? Optional—at least index and key lookup needed for builtins.
    

### SIMPLE CLASSES (OPTIONAL / BEST-EFFORT)

- Basic `class` definitions with `def` methods; implicit `self` parameter for instance methods.
    
- `__init__(self, ...)` initializer invoked on instance creation.
    
- Field/attribute access and assignment: `obj.field` and `obj.field = val`.
    
- Method call `obj.method(args...)` resolves binding `self`.
    
- Keep scoping simple (no inheritance required unless trivial).
    
- Document limitations (no metaclasses, no descriptors).
    

---

## Files to include (send these to Claude 4)

Minimum (required to implement & test):

- `PythonInterpreter.cs` (entire)
    
- `GameBuiltinMethods.cs` (entire)
    
- `PythonParser.cs` (entire)
    
- `PythonAST.cs` (entire)
    
- `PythonLexer.cs` (entire)
    
- `PythonToken.cs` (entire)
    
- Runtime `Value` classes file(s) (where `Value`, `NumericValue`, `ListValue`, `BuiltinFunctionValue`, etc. live)
    
- `ScriptRunner.cs` (env seeding)
    
- Example scripts: `tests/rotate_ccw_before.py`, `tests/rotate_ccw_after.py`, `tests/attribute_and_list_fallback.py`
    

Optional but helpful:

- `ConsoleManager.cs`, `PythonCodeEditorSyntaxHighlight.cs`, `ExecutionTracker.cs`, `GlobalScriptManager.cs`
    

---

## Deliverables (what I expect)

1. Per-file patches with `// add`, `// modify`, `// replace` markers for each changed file. Keep other code untouched.
    
2. A new `V2Value` implementation (with `ToString()`), `DictValue`, and `ClassValue`/`InstanceValue` if classes implemented.
    
3. Lexer changes to drop comments and recognize triple-quoted string tokens.
    
4. Parser changes for:
    
    - dot attribute access,
        
    - tuple assignment targets (for destructuring),
        
    - dict literal parsing,
        
    - `class`/`def` block headers (if classes implemented).
        
5. AST nodes for attribute access, tuple targets, docstrings, dict literals, class defs.
    
6. Interpreter changes:
    
    - binary ops for V2,
        
    - attribute access eval,
        
    - auto-call builtins,
        
    - builtin arg normalization,
        
    - dict and class runtime support (if implemented).
        
7. Updates to `GameBuiltinMethods.cs`:
    
    - register `v2`,
        
    - normalize `move`, `is_block`, etc.,
        
    - add `step`, `movefwd` etc. (optional).
        
8. Example in-game scripts (test cases) that verify all must features + comment/docstring/dict/class examples, with expected outputs.
    
9. Short explanation (1–2 paragraphs) of tradeoffs and edge cases:
    
    - auto-call scope,
        
    - float vs int equality for V2,
        
    - mutability of V2,
        
    - docstring storage semantics,
        
    - class limitations.
        

---

## Example test scripts (must pass)

**rotate_ccw_after.py**

```py
def rotate_ccw(dir):
    """Rotate vector CCW"""
    return v2(-dir.y, dir.x)

def func():
    dir = v2(1, 0)
    curr = get_pos          # auto-called builtin (arity 0)
    while True:
        if is_goal(curr):   # accepts v2
            submit()
            break

        next = curr + dir   # vector add
        print(next)         # prints v2(x,y)
        if is_block(next):  # accepts v2
            say("there is a block ahead")
            dir = rotate_ccw(dir)
            print(dir)
        else:
            move(dir)       # move accepts v2
            curr = next

func()
```

**attribute_and_list_fallback.py**

```py
# single-line comment should be ignored
d = [2,3]
print(d.x, d.y)   # expected: 2 3
v = v2(5,6)
print(v.x, v.y)   # expected: 5 6
print(v == v2(5,6)) # true
print(v + v2(1,1))  # v2(6,7)
```

**docstring_and_comments.py**

```py
def foo():
    """This function does nothing; docstring must be stored, not executed."""
    # The next line is a comment and ignored
    return 1

print(foo.__doc__)  # expected: "This function does nothing; docstring must be stored, not executed."
```

**dict_and_class_examples.py** (if implemented)

```py
m = {"a": 1, "b": 2}
print(m["a"])      # expected: 1
m["c"] = 3
print(m["c"])      # expected: 3

class P:
    def __init__(self, x, y):
        self.x = x
        self.y = y
    def move(self, dx, dy):
        self.x = self.x + dx
        self.y = self.y + dy

p = P(1,2)
p.move(1,1)
print(p.x, p.y)    # expected: 2 3
```

---

## Acceptance criteria (must pass)

- Existing scripts using `[x,y]`, `get_pos()`, `move(x,y)` behave exactly the same.
    
- New features work per tests above.
    
- Auto-call applies **only** to builtin functions.
    
- Comments and docstrings are parsed correctly: docstrings stored on function/class objects and not executed.
    
- If classes/dicts are implemented, basic usage works as in tests; otherwise the prompt should mark them OPTIONAL and implementers must explain limitations.
    

---

## Edge cases & design decisions (include in answer)

- **Auto-call tradeoffs**: explained behavior for `f = get_pos` (function object) vs auto-call; document how to get a builtin function object if needed.
    
- **V2 equality**: choose approximate float equality (e.g., `Mathf.Approximately`), document numeric-to-int conversion for GameController calls.
    
- **V2 mutability**: state whether V2 is immutable (recommended) or mutable, and how `+=` is implemented (reassignment vs in-place).
    
- **Docstrings**: stored as `function.Docstring` or `class.Docstring`; accessible via `object.__doc__`.
    
- **Classes**: document limitations (no inheritance, limited scoping, etc.) if implemented.
    

---

## Final note to Claude 4

Please produce per-file patches with `// add`, `// modify`, `// replace`. Keep changes minimal and well-localized. If any global naming mismatches (e.g., `NumericValue` vs `NumberValue`) exist in the repo, provide a mapping block at the top of your response showing the substitutions you used.

And remember: do not introduce `return` inside a `try/catch` in any `IEnumerator` code — add a comment where relevant.

---

If you want, I can now generate the concrete per-file patches for the parts you care about first (A–E and comment/docstring support) using assumed common names for your `Value` classes (`NumericValue`, `ListValue`, `BuiltinFunctionValue`). Or paste your runtime `Value` code and I’ll produce compile-ready patches. Which do you prefer?