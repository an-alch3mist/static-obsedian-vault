# LOOP Language Specification v1.0 - Complete with Modification Guide

**Role:** Act as a Principal Unity Architect and Compiler Language Engineer.  
**Project:** "Farmer Was Replaced" Clone For Learning - A Programming Puzzle Game.  
**Goal:** Generate complete, production-ready C# source code for a Coroutine-based Python Interpreter in Unity.

---

## QUICK MODIFICATION GUIDE

### 🎯 How to Use This Prompt

This prompt is organized into **clearly marked sections**. Each section has a `[MODIFY HERE]` tag showing where to make changes.

**Common Modifications:**

| **What You Want to Add** | **Go To Section** | **What To Do** |
|---------------------------|-------------------|----------------|
| New game function (like `harvest()`) | Section 4.1 - Game Builtins | Add function to `<game_builtins>` list |
| New operator (like `%`, `**`) | Section 2.1.1 - Token Types | Add token to appropriate category |
| Indentation rules | Section 2.2.2 - Indentation | Modify `<indentation_rules>` |
| Multi-line comments | Section 2.2 - Lexer | Add to comment scanning logic |
| Return-type built-ins | Section 4.1 - Game Builtins | Add with `<return_type>` specified |
| Operator precedence | Section 3.4 - Operator Precedence | Update precedence table |
| Test cases | Section 5 - Test Suite | Add new `<test_case>` block |

---

## ═══════════════════════════════════════════════════════════════
## SECTION 0: META-INSTRUCTIONS & CRITICAL RULES
## ═══════════════════════════════════════════════════════════════

<meta_instruction priority="CRITICAL">
  **Before generating any code, the AI MUST:**
  
  1. ✅ Read the ENTIRE specification carefully
  2. ✅ Review all test cases in Section 5 (Test Suite)
  3. ✅ Verify generated code will pass EVERY test case
  4. ✅ Check operator precedence matches Section 3.4
  5. ✅ Validate indentation rules from Section 2.2.2
  6. ✅ Confirm all game built-in functions are registered (Section 4.1)
  
  **All generated code MUST:**
  - Pass the entire test suite
  - Follow .NET 2.0 constraints
  - Include proper error handling with line numbers
  - Implement instruction budget system
  - Support all features listed in Section 8.1 checklist
  
  **If you cannot generate code that passes all tests:**
  - Explain which test case is ambiguous
  - Request clarification before proceeding
</meta_instruction>

---

## ═══════════════════════════════════════════════════════════════
## SECTION 1: SYSTEM ARCHITECTURE & CORE PHILOSOPHY
## ═══════════════════════════════════════════════════════════════

### 1.1 Execution Model (The "Golden Rule")

#### 1.1.1 Coroutine-Based VM

* **Single-Threaded Execution:** The Interpreter (`PythonInterpreter.cs`) runs strictly within a Unity `IEnumerator`.
* **Thread-Safety:** No background threads - ensures compatibility with Unity's API (`Transform`, `GameObject`).
* **Yield Propagation:** When user scripts call "Game Commands" (like `move()`), those commands return `YieldInstruction` objects. The Interpreter must pause and yield these to Unity.

#### 1.1.2 Global Instruction Budget System (CRITICAL)

**Purpose:** Prevent frame drops while maintaining instant execution for small loops.

**Implementation:**
* **Counter:** Maintain a global `int instructionCount` that tracks operations executed in the current frame.
* **Budget:** Set `INSTRUCTIONS_PER_FRAME = 100` (configurable).

**[MODIFY HERE - INSTRUCTION BUDGET]**
```xml
<instruction_budget>
  <default_budget>100</default_budget>
  <description>
    Number of operations allowed per frame before yielding.
    Lower = smoother frame rate but slower script execution.
    Higher = faster script execution but possible frame drops.
  </description>
  <recommended_range>
    <minimum>50</minimum>
    <maximum>500</maximum>
  </recommended_range>
</instruction_budget>
```

* **Increment Logic:** Increment `instructionCount` for:
  - Every statement execution (`x = 1`, `print(x)`)
  - Every expression evaluation (`x < 5`, `a + b`)
  - Every loop iteration check (`for` or `while` condition)
  - Every function call entry
* **Time-Slicing:** When `instructionCount >= INSTRUCTIONS_PER_FRAME`:
  - `yield return null` (pause until next frame)
  - Reset `instructionCount = 0`
  - Continue execution

**Behavior Examples:**
```python
# Example 1: 100 iterations - INSTANT (1 frame)
sum = 0
for i in range(100):
    sum += 1  # ~100 ops -> stays under budget
print(sum)

# Example 2: 1000 iterations - TIME-SLICED (~10 frames)
sum = 0
for i in range(1000):
    sum += 1  # ~1000 ops -> yields 10 times

# Example 3: Nested loops - INTELLIGENT SLICING
sum = 0
for y in range(50):      # Outer: 50 iterations
    for x in range(1000): # Inner: 1000 iterations each
        sum += 1          # Total: 50,000 ops -> yields ~500 times

# Example 4: Sleep ALWAYS pauses (independent of budget)
for i in range(50):
    print("iteration:", i)
    if i % 10 == 0:
        sleep(2.0)  # MUST yield WaitForSeconds(2.0)

# Example 5: Built-in function move() ALWAYS pauses (independent of budget)
for i in range(50):
    print("iteration:", i)
    if i % 10 == 0:
        move("right")  # MUST yield until IEnumerator completes
```

**Critical Rules:**
1. **`sleep(seconds)` Override:** Sleep ALWAYS yields `WaitForSeconds`, regardless of instruction budget.
2. **Game Commands:** Functions like `move()`, `harvest()` ALWAYS yield their `YieldInstruction`, pausing Python execution until complete.
3. **Budget Independence:** Instruction budget only controls computational time-slicing, not external waits.

#### 1.1.3 Error Context & Recovery

* **Line Tracking:** Maintain `currentLineNumber` during execution.
* **Exception Wrapping:** When ANY exception occurs:
  - Catch the original exception
  - Create `RuntimeError` with message: `"Line {currentLineNumber}: {originalMessage}"`
  - Re-throw the wrapped error
* **State Reset:** After error, call `Interpreter.Reset()` to clear:
  - All scopes (global + local)
  - Instruction counter
  - Call stack

### 1.2 Data Types & Sandboxing

#### 1.2.1 Type System

**[MODIFY HERE - DATA TYPES]**
```xml
<type_system>
  <numeric_type>
    <storage>double</storage>
    <description>
      All numeric values stored as C# double internally.
      Supports both integer and floating-point literals.
    </description>
    <automatic_conversion>
      <context>Array indexing, loop counters</context>
      <method>Cast to (int) when needed</method>
    </automatic_conversion>
  </numeric_type>
  
  <string_type>
    <storage>C# string</storage>
    <mutability>Immutable</mutability>
    <methods>
      split, join, strip, replace, upper, lower, 
      startswith, endswith, find, count
    </methods>
  </string_type>
  
  <boolean_type>
    <storage>C# bool</storage>
    <literals>True, False</literals>
  </boolean_type>
  
  <none_type>
    <storage>C# null</storage>
    <literal>None</literal>
  </none_type>
  
  <list_type>
    <storage>List&lt;object&gt;</storage>
    <description>Dynamic heterogeneous collections</description>
    <methods>
      append, remove, pop, insert, sort, reverse, 
      clear, count, index, extend
    </methods>
  </list_type>
  
  <dictionary_type>
    <storage>Dictionary&lt;object, object&gt;</storage>
    <key_requirements>Must be hashable (immutable types)</key_requirements>
    <methods>
      keys, values, items, get, pop, clear, update
    </methods>
  </dictionary_type>
  
  <class_type>
    <support>Custom user-defined classes</support>
    <features>
      - Methods with 'self' parameter
      - __init__ constructor
      - Instance variables
      - Inheritance (if implemented)
    </features>
  </class_type>
</type_system>
```

#### 1.2.2 Security Constraints (.NET 2.0 Compatibility)

* **No Reflection:** Disable `System.Reflection` access to external assemblies
* **No File I/O:** Block `System.IO` operations
* **No Threading:** Reject `System.Threading` APIs
* **IEnumerator Limitation:** CANNOT use `yield return` inside `try-catch` blocks (Unity 2020.3 .NET 2.0 constraint)
  - Solution: Store yield instructions in variables outside try-catch, then yield after the block

---

## ═══════════════════════════════════════════════════════════════
## SECTION 2: LEXER & TOKENIZATION
## ═══════════════════════════════════════════════════════════════

### 2.1 Token System (`Token.cs`, `TokenType.cs`)

#### 2.1.1 Token Types (Enum)

**[MODIFY HERE - ADD NEW OPERATORS OR KEYWORDS]**

```xml
<token_types>
  <structural>
    <tokens>INDENT, DEDENT, NEWLINE, EOF</tokens>
    <description>Control indentation-based syntax</description>
  </structural>
  
  <literals>
    <tokens>IDENTIFIER, STRING, NUMBER</tokens>
    <description>Variable names, string literals, numeric literals</description>
  </literals>
  
  <keywords>
    <control_flow>
      IF, ELIF, ELSE, WHILE, FOR, BREAK, CONTINUE, PASS
    </control_flow>
    <function_related>
      DEF, RETURN, LAMBDA
    </function_related>
    <class_related>
      CLASS
    </class_related>
    <scope_related>
      GLOBAL
    </scope_related>
    <logical>
      AND, OR, NOT, IN, IS
    </logical>
    <literals>
      TRUE, FALSE, NONE
    </literals>
    
    <!-- TO ADD A NEW KEYWORD:
         1. Add it here in appropriate category
         2. Add to Lexer's keyword map (Section 2.2)
         3. Add parsing logic in Parser (Section 2.4)
         Example: To add 'elif':
         <control_flow>... ELIF ...</control_flow>
    -->
  </keywords>
  
  <arithmetic_operators>
    <tokens>PLUS, MINUS, STAR, SLASH, PERCENT</tokens>
    <symbols>+, -, *, /, %</symbols>
    <description>Basic arithmetic operations</description>
    
    <!-- TO ADD NEW ARITHMETIC OPERATOR:
         Example: Add exponentiation (**)
         1. Add token: POWER
         2. Add symbol: **
         3. Add to operator precedence (Section 3.4)
         4. Implement in evaluateExpression() (Section 3.3)
    -->
  </arithmetic_operators>
  
  <comparison_operators>
    <tokens>
      EQUAL_EQUAL, BANG_EQUAL, LESS, GREATER, 
      LESS_EQUAL, GREATER_EQUAL
    </tokens>
    <symbols>==, !=, &lt;, &gt;, &lt;=, &gt;=</symbols>
    <description>Comparison and equality operations</description>
  </comparison_operators>
  
  <assignment_operators>
    <tokens>
      EQUAL, PLUS_EQUAL, MINUS_EQUAL, 
      STAR_EQUAL, SLASH_EQUAL
    </tokens>
    <symbols>=, +=, -=, *=, /=</symbols>
    <description>Variable assignment and compound assignment</description>
  </assignment_operators>
  
  <bitwise_operators>
    <tokens>
      AMPERSAND, PIPE, CARET, TILDE, 
      LEFT_SHIFT, RIGHT_SHIFT
    </tokens>
    <symbols>&amp;, |, ^, ~, &lt;&lt;, &gt;&gt;</symbols>
    <description>Bitwise operations on integers</description>
    
    <!-- TO ADD NEW BITWISE OPERATOR:
         Follow same pattern as arithmetic operators above
    -->
  </bitwise_operators>
  
  <delimiters>
    <tokens>
      LEFT_PAREN, RIGHT_PAREN, LEFT_BRACKET, RIGHT_BRACKET,
      DOT, COMMA, COLON
    </tokens>
    <symbols>(, ), [, ], ., ,, :</symbols>
    <description>Structural delimiters for expressions</description>
  </delimiters>
</token_types>
```

**C# Implementation:**
```csharp
public enum TokenType {
    // Structural
    INDENT, DEDENT, NEWLINE, EOF,
    
    // Literals
    IDENTIFIER, STRING, NUMBER,
    
    // Keywords
    IF, ELIF, ELSE, WHILE, FOR, DEF, RETURN, CLASS,
    BREAK, CONTINUE, PASS, GLOBAL, LAMBDA,
    AND, OR, NOT, IN, IS,
    TRUE, FALSE, NONE,
    
    // Operators (Arithmetic)
    PLUS, MINUS, STAR, SLASH, PERCENT,
    
    // Operators (Comparison)
    EQUAL_EQUAL, BANG_EQUAL, LESS, GREATER, LESS_EQUAL, GREATER_EQUAL,
    
    // Operators (Assignment)
    EQUAL, PLUS_EQUAL, MINUS_EQUAL, STAR_EQUAL, SLASH_EQUAL,
    
    // Operators (Bitwise)
    AMPERSAND, PIPE, CARET, TILDE, LEFT_SHIFT, RIGHT_SHIFT,
    
    // Delimiters
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACKET, RIGHT_BRACKET,
    DOT, COMMA, COLON
}
```

#### 2.1.2 Token Class
```csharp
public class Token {
    public TokenType Type;
    public string Lexeme;
    public object Literal;  // For NUMBER, STRING
    public int LineNumber;
    
    public Token(TokenType type, string lexeme, object literal, int line) {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        LineNumber = line;
    }
}
```

### 2.2 Lexer (`Lexer.cs`) - The Input Sanitizer

#### 2.2.1 Input Validation (CRITICAL)

```csharp
public string ValidateAndClean(string input) {
    // Step 1: Normalize line endings
    input = input.Replace("\r\n", "\n");
    input = input.Replace("\r", "\n");
    
    // Step 2: Convert tabs to 4 spaces (consistency)
    input = input.Replace("\t", "    ");
    
    // Step 3: Remove invisible characters (prevent crashes)
    input = input.Replace("\v", "");  // Vertical tab
    input = input.Replace("\f", "");  // Form feed
    input = input.Replace("\uFEFF", "");  // BOM (Byte Order Mark)
    
    // Step 4: Ensure ends with newline
    if (!input.EndsWith("\n")) {
        input += "\n";
    }
    
    return input;
}
```

#### 2.2.2 Indentation Logic (Python-Style)

**[MODIFY HERE - INDENTATION RULES]**

```xml
<indentation_rules>
  <unit>
    <spaces_per_level>4</spaces_per_level>
    <tabs_allowed>false</tabs_allowed>
    <description>
      Tabs are automatically converted to 4 spaces during input validation.
      Mixing tabs and spaces is not allowed.
    </description>
  </unit>
  
  <validation>
    <rule>All indentation must be multiples of 4 spaces</rule>
    <rule>Dedentation must align with a previous indentation level</rule>
    <rule>Inconsistent indentation throws LexerError</rule>
  </validation>
  
  <examples>
    <valid>
if condition:
    statement1
    if nested:
        statement2
    statement3
    </valid>
    
    <invalid reason="Not multiple of 4">
if condition:
  statement  # Only 2 spaces - ERROR
    </invalid>
    
    <invalid reason="Dedent doesn't align">
if condition:
    if nested:
        statement
      other  # 6 spaces - no matching level - ERROR
    </invalid>
  </examples>
</indentation_rules>
```

**C# Implementation:**
```csharp
Stack<int> indentStack = new Stack<int>();
// Initialize with 0
indentStack.Push(0);

void ProcessIndentation(int leadingSpaces) {
    int current = leadingSpaces;
    int previous = indentStack.Peek();
    
    if (current > previous) {
        // INDENT
        indentStack.Push(current);
        EmitToken(TokenType.INDENT);
    } else if (current < previous) {
        // DEDENT (possibly multiple)
        while (indentStack.Count > 0 && indentStack.Peek() > current) {
            indentStack.Pop();
            EmitToken(TokenType.DEDENT);
        }
        
        // Validation: Must match a level
        if (indentStack.Peek() != current) {
            throw new LexerError($"Line {lineNumber}: Indentation mismatch");
        }
    }
    // If current == previous, no indent change
}
```

#### 2.2.3 Comment Handling

**[MODIFY HERE - ADD MULTI-LINE COMMENTS]**

```xml
<comment_syntax>
  <single_line>
    <syntax>//</syntax>
    <description>Extends from // to end of line</description>
    <example>
x = 5  // This is a comment
// This entire line is a comment
    </example>
  </single_line>
  
  <multi_line>
    <syntax_start>/*</syntax_start>
    <syntax_end>*/</syntax_end>
    <description>
      Multi-line comment block. Can span multiple lines.
      Useful for documentation and temporarily disabling code blocks.
    </description>
    <nesting>Not allowed - nested /* */ will cause errors</nesting>
    <example>
/*
  This is a multi-line comment
  It can span several lines
  Useful for documentation
*/
x = 5
    </example>
    <implementation_note>
      Lexer must track whether inside multi-line comment.
      When /* encountered, skip all characters until */ found.
      Track line numbers properly for error reporting.
    </implementation_note>
  </multi_line>
  
  <!-- TO IMPLEMENT MULTI-LINE COMMENTS IN LEXER:
       1. Add boolean flag: insideMultiLineComment = false
       2. In ScanToken(), check for /* sequence
       3. When found, set flag true and skip until */ found
       4. Remember to increment lineNumber on \n inside comments
       5. Throw error if EOF reached while still in comment
  -->
</comment_syntax>
```

#### 2.2.4 Number Parsing (Support Integer & Float)

```csharp
void ScanNumber() {
    while (IsDigit(Peek())) Advance();
    
    // Check for decimal point
    if (Peek() == '.' && IsDigit(PeekNext())) {
        Advance(); // Consume '.'
        while (IsDigit(Peek())) Advance();
    }
    
    string numStr = source.Substring(start, current - start);
    double value = double.Parse(numStr);
    EmitToken(TokenType.NUMBER, value);
}
```

#### 2.2.5 String Parsing

```csharp
void ScanString() {
    while (Peek() != '"' && !IsAtEnd()) {
        if (Peek() == '\n') lineNumber++;
        Advance();
    }
    
    if (IsAtEnd()) {
        throw new LexerError($"Line {lineNumber}: Unterminated string");
    }
    
    Advance(); // Closing "
    string value = source.Substring(start + 1, current - start - 2);
    EmitToken(TokenType.STRING, value);
}
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 3: PARSER & ABSTRACT SYNTAX TREE (AST)
## ═══════════════════════════════════════════════════════════════

### 3.1 AST Node Hierarchy (`AST.cs`)

```csharp
// Base classes
public abstract class ASTNode { }
public abstract class Stmt : ASTNode { }
public abstract class Expr : ASTNode { }

// Statement types
public class ExpressionStmt : Stmt {
    public Expr Expression;
}

public class AssignmentStmt : Stmt {
    public string Target;
    public Expr Value;
    public string Operator; // "=", "+=", "-=", etc.
}

public class IfStmt : Stmt {
    public Expr Condition;
    public List<Stmt> ThenBranch;
    public List<Stmt> ElseBranch; // Can be null
}

public class WhileStmt : Stmt {
    public Expr Condition;
    public List<Stmt> Body;
}

public class ForStmt : Stmt {
    public string Variable;
    public Expr Iterable;
    public List<Stmt> Body;
}

public class FunctionDefStmt : Stmt {
    public string Name;
    public List<string> Parameters;
    public List<Stmt> Body;
}

public class ClassDefStmt : Stmt {
    public string Name;
    public List<FunctionDefStmt> Methods;
}

public class ReturnStmt : Stmt {
    public Expr Value; // Can be null
}

public class BreakStmt : Stmt { }
public class ContinueStmt : Stmt { }
public class PassStmt : Stmt { }
public class GlobalStmt : Stmt {
    public List<string> Variables;
}

// Expression types
public class BinaryExpr : Expr {
    public Expr Left;
    public TokenType Operator;
    public Expr Right;
}

public class UnaryExpr : Expr {
    public TokenType Operator;
    public Expr Operand;
}

public class LiteralExpr : Expr {
    public object Value;
}

public class VariableExpr : Expr {
    public string Name;
}

public class CallExpr : Expr {
    public Expr Callee;
    public List<Expr> Arguments;
}

public class IndexExpr : Expr {
    public Expr Object;
    public Expr Index;
}

public class SliceExpr : Expr {
    public Expr Object;
    public Expr Start; // Can be null
    public Expr Stop;  // Can be null
    public Expr Step;  // Can be null
}

public class ListExpr : Expr {
    public List<Expr> Elements;
}

public class DictExpr : Expr {
    public List<Expr> Keys;
    public List<Expr> Values;
}

public class LambdaExpr : Expr {
    public List<string> Parameters;
    public Expr Body;
}

public class ListCompExpr : Expr {
    public Expr Element;
    public string Variable;
    public Expr Iterable;
    public Expr Condition; // Can be null (for if clause)
}

public class MemberAccessExpr : Expr {
    public Expr Object;
    public string Member;
}
```

### 3.2 Parser (`Parser.cs`)

#### 3.2.1 Grammar (Recursive Descent)

```
program        → statement* EOF
statement      → simple_stmt | compound_stmt
simple_stmt    → (expr_stmt | assignment | return_stmt | break_stmt | 
                  continue_stmt | pass_stmt | global_stmt) NEWLINE
compound_stmt  → if_stmt | while_stmt | for_stmt | function_def | class_def

expr_stmt      → expression
assignment     → IDENTIFIER (EQUAL | PLUS_EQUAL | ...) expression
return_stmt    → "return" expression?
break_stmt     → "break"
continue_stmt  → "continue"
pass_stmt      → "pass"
global_stmt    → "global" IDENTIFIER ("," IDENTIFIER)*

if_stmt        → "if" expression ":" suite ("elif" expression ":" suite)* 
                 ("else" ":" suite)?
while_stmt     → "while" expression ":" suite
for_stmt       → "for" IDENTIFIER "in" expression ":" suite
function_def   → "def" IDENTIFIER "(" parameters? ")" ":" suite
class_def      → "class" IDENTIFIER ":" suite

suite          → NEWLINE INDENT statement+ DEDENT | simple_stmt
parameters     → IDENTIFIER ("," IDENTIFIER)*

expression     → logical_or
logical_or     → logical_and ("or" logical_and)*
logical_and    → logical_not ("and" logical_not)*
logical_not    → "not" logical_not | comparison
comparison     → bitwise_or (("==" | "!=" | "<" | ">" | "<=" | ">=" | 
                 "in" | "is") bitwise_or)*
bitwise_or     → bitwise_xor ("|" bitwise_xor)*
bitwise_xor    → bitwise_and ("^" bitwise_and)*
bitwise_and    → addition ("&" addition)*
addition       → multiplication (("+" | "-") multiplication)*
multiplication → unary (("*" | "/" | "%") unary)*
unary          → ("+" | "-" | "~" | "not") unary | primary
primary        → literal | IDENTIFIER | call | index | slice | 
                 member_access | list | dict | lambda | list_comp | 
                 "(" expression ")"

call           → primary "(" arguments? ")"
index          → primary "[" expression "]"
slice          → primary "[" expression? ":" expression? (":" expression?)? "]"
member_access  → primary "." IDENTIFIER
list           → "[" (expression ("," expression)*)? "]"
dict           → "{" (expression ":" expression ("," expression ":" expression)*)? "}"
lambda         → "lambda" parameters? ":" expression
list_comp      → "[" expression "for" IDENTIFIER "in" expression ("if" expression)? "]"

literal        → NUMBER | STRING | "True" | "False" | "None"
```

### 3.3 Expression Evaluation

**[MODIFY HERE - ADD NEW OPERATORS]**

The interpreter evaluates expressions recursively based on operator precedence (defined in Section 3.4).

```csharp
object EvaluateBinaryExpr(BinaryExpr expr) {
    object left = Evaluate(expr.Left);
    object right = Evaluate(expr.Right);
    
    switch (expr.Operator) {
        case TokenType.PLUS:
            return ToNumber(left) + ToNumber(right);
        case TokenType.MINUS:
            return ToNumber(left) - ToNumber(right);
        case TokenType.STAR:
            return ToNumber(left) * ToNumber(right);
        case TokenType.SLASH:
            double divisor = ToNumber(right);
            if (divisor == 0) throw new RuntimeError("Division by zero");
            return (int)(ToNumber(left) / divisor); // Integer division
        case TokenType.PERCENT:
            return ToNumber(left) % ToNumber(right);
        
        // Comparison
        case TokenType.EQUAL_EQUAL:
            return IsEqual(left, right);
        case TokenType.BANG_EQUAL:
            return !IsEqual(left, right);
        case TokenType.LESS:
            return ToNumber(left) < ToNumber(right);
        case TokenType.GREATER:
            return ToNumber(left) > ToNumber(right);
        case TokenType.LESS_EQUAL:
            return ToNumber(left) <= ToNumber(right);
        case TokenType.GREATER_EQUAL:
            return ToNumber(left) >= ToNumber(right);
        
        // Logical
        case TokenType.AND:
            return IsTruthy(left) && IsTruthy(right);
        case TokenType.OR:
            return IsTruthy(left) || IsTruthy(right);
        
        // Bitwise
        case TokenType.AMPERSAND:
            return (int)ToNumber(left) & (int)ToNumber(right);
        case TokenType.PIPE:
            return (int)ToNumber(left) | (int)ToNumber(right);
        case TokenType.CARET:
            return (int)ToNumber(left) ^ (int)ToNumber(right);
        case TokenType.LEFT_SHIFT:
            return (int)ToNumber(left) << (int)ToNumber(right);
        case TokenType.RIGHT_SHIFT:
            return (int)ToNumber(left) >> (int)ToNumber(right);
        
        // TO ADD NEW OPERATOR:
        // 1. Add case for new TokenType
        // 2. Implement operation logic
        // 3. Add to test suite (Section 5)
        // Example for exponentiation (**):
        // case TokenType.POWER:
        //     return Math.Pow(ToNumber(left), ToNumber(right));
    }
    
    throw new RuntimeError($"Unknown operator: {expr.Operator}");
}
```

### 3.4 Operator Precedence Table

**[MODIFY HERE - OPERATOR PRECEDENCE]**

```xml
<operator_precedence>
  <instruction>
    Operators are evaluated in this order (highest to lowest priority).
    Use parentheses to override default precedence.
    Parser must build AST respecting this precedence.
  </instruction>
  
  <level priority="1" associativity="left-to-right">
    <name>Grouping</name>
    <operators>( )</operators>
    <description>Parentheses for explicit grouping</description>
  </level>
  
  <level priority="2" associativity="left-to-right">
    <name>Member Access, Indexing, Calls</name>
    <operators>. [ ] ( )</operators>
    <description>Attribute access, subscripting, function calls</description>
  </level>
  
  <level priority="3" associativity="right-to-left">
    <name>Unary</name>
    <operators>+ - ~ not</operators>
    <description>Unary plus, minus, bitwise NOT, logical NOT</description>
  </level>
  
  <level priority="4" associativity="left-to-right">
    <name>Multiplicative</name>
    <operators>* / %</operators>
    <description>Multiplication, division, modulo</description>
    
    <!-- TO ADD EXPONENTIATION (**):
         Add here with priority 3.5 (between unary and multiplicative)
         <operators>**</operators>
         Associativity: right-to-left (2**3**4 = 2**(3**4))
    -->
  </level>
  
  <level priority="5" associativity="left-to-right">
    <name>Additive</name>
    <operators>+ -</operators>
    <description>Addition, subtraction</description>
  </level>
  
  <level priority="6" associativity="left-to-right">
    <name>Bitwise Shift</name>
    <operators>&lt;&lt; &gt;&gt;</operators>
    <description>Left shift, right shift</description>
  </level>
  
  <level priority="7" associativity="left-to-right">
    <name>Bitwise AND</name>
    <operators>&amp;</operators>
    <description>Bitwise AND</description>
  </level>
  
  <level priority="8" associativity="left-to-right">
    <name>Bitwise XOR</name>
    <operators>^</operators>
    <description>Bitwise exclusive OR</description>
  </level>
  
  <level priority="9" associativity="left-to-right">
    <name>Bitwise OR</name>
    <operators>|</operators>
    <description>Bitwise OR</description>
  </level>
  
  <level priority="10" associativity="left-to-right">
    <name>Comparison</name>
    <operators>== != &lt; &gt; &lt;= &gt;= in is</operators>
    <description>Equality, relational, membership, identity tests</description>
  </level>
  
  <level priority="11" associativity="left-to-right">
    <name>Logical AND</name>
    <operators>and</operators>
    <description>Boolean AND with short-circuit evaluation</description>
  </level>
  
  <level priority="12" associativity="left-to-right">
    <name>Logical OR</name>
    <operators>or</operators>
    <description>Boolean OR with short-circuit evaluation</description>
  </level>
  
  <level priority="13" associativity="right-to-left">
    <name>Lambda</name>
    <operators>lambda</operators>
    <description>Lambda expression (lowest precedence)</description>
  </level>
  
  <examples>
    <example>
      <expression>a and b &amp; c or 2 == 5 // 2</expression>
      <explanation>
        Evaluation order (highest to lowest precedence):
        1. 5 // 2 → 2 (multiplicative)
        2. 2 == 2 → True (comparison)
        3. b &amp; c → (bitwise AND result)
        4. a and (result from step 3) (logical AND)
        5. (result from step 4) or True (logical OR)
      </explanation>
      <result>Always True (because "or True" at end)</result>
    </example>
    
    <example>
      <expression>not a and b or c</expression>
      <explanation>
        Evaluation order:
        1. not a (unary NOT)
        2. (result from 1) and b (logical AND)
        3. (result from 2) or c (logical OR)
      </explanation>
      <equivalent>(not a and b) or c</equivalent>
    </example>
    
    <example>
      <expression>2 + 3 * 4</expression>
      <explanation>
        Evaluation order:
        1. 3 * 4 → 12 (multiplicative)
        2. 2 + 12 → 14 (additive)
      </explanation>
      <result>14 (not 20)</result>
    </example>
  </examples>
</operator_precedence>
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 4: GAME BUILT-IN FUNCTIONS & RUNTIME
## ═══════════════════════════════════════════════════════════════

### 4.1 Game Built-in Functions (`GameBuiltinMethods.cs`)

**[MODIFY HERE - ADD NEW GAME FUNCTIONS]**

```xml
<game_builtins>
  <description>
    These are Unity-specific functions accessible from Python scripts.
    They integrate with the game world and may yield control back to Unity
    for animations, timing, or other Unity operations.
  </description>
  
  <function>
    <name>move</name>
    <parameters>direction: string</parameters>
    <return_type>void</return_type>
    <execution_time>Approximately 0.5 seconds (based on movement animation)</execution_time>
    <yields>true</yields>
    <description>
      Moves the player character one tile in the specified direction.
      Valid directions: "up", "down", "left", "right"
      Blocks script execution until movement animation completes.
    </description>
    <implementation_type>IEnumerator</implementation_type>
    <example>
move("right")  # Moves right and waits for animation
move("up")     # Then moves up
    </example>
    <error_handling>
      Throws RuntimeError if:
      - direction is not a valid string
      - direction is not one of the 4 valid directions
      - movement would go out of bounds
    </error_handling>
  </function>
  
  <function>
    <name>harvest</name>
    <parameters>x: int, y: int</parameters>
    <return_type>void</return_type>
    <execution_time>Approximately 0.2 seconds (harvest animation duration)</execution_time>
    <yields>true</yields>
    <description>
      Harvests the crop at grid position (x, y).
      Execution pauses during harvest animation (handled via coroutine
      and animation events within Unity).
      
      This is a blocking operation - code execution pauses until the
      harvest animation and all associated Unity events complete.
    </description>
    <implementation_type>IEnumerator</implementation_type>
    <usage_pattern>
      Always check crop state before harvesting:
      
      if getCropState(x, y) == "ripe":
          harvest(x, y)  # Safe to harvest
    </usage_pattern>
    <example>
for x in range(5):
    if getCropState(x, 0) == "ripe":
        harvest(x, 0)
        print("Harvested at", x, 0)
    </example>
    <error_handling>
      Throws RuntimeError if:
      - x or y are not integers
      - Position (x, y) is out of grid bounds
      - No crop exists at (x, y)
    </error_handling>
  </function>
  
  <function>
    <name>plant</name>
    <parameters>x: int, y: int, crop_type: string</parameters>
    <return_type>void</return_type>
    <execution_time>Approximately 0.3 seconds (planting animation)</execution_time>
    <yields>true</yields>
    <description>
      Plants a crop of the specified type at position (x, y).
      Valid crop types: "carrot", "wheat", "corn", "pumpkin"
    </description>
    <implementation_type>IEnumerator</implementation_type>
  </function>
  
  <function>
    <name>water</name>
    <parameters>x: int, y: int</parameters>
    <return_type>void</return_type>
    <execution_time>Approximately 0.1 seconds</execution_time>
    <yields>true</yields>
    <description>
      Waters the crop at position (x, y).
      Watered crops grow faster.
    </description>
    <implementation_type>IEnumerator</implementation_type>
  </function>
  
  <function>
    <name>getCropState</name>
    <parameters>x: int, y: int</parameters>
    <return_type>string</return_type>
    <execution_time>Instant (no yield)</execution_time>
    <yields>false</yields>
    <description>
      Returns the current state of the crop at position (x, y).
    </description>
    <possible_returns>
      "empty"    - No crop planted
      "planted"  - Seed just planted
      "growing"  - Crop is growing
      "ripe"     - Ready to harvest
      "withered" - Crop died from neglect
    </possible_returns>
    <implementation_type>Regular method (not IEnumerator)</implementation_type>
    <example>
state = getCropState(2, 3)
if state == "ripe":
    harvest(2, 3)
elif state == "empty":
    plant(2, 3, "carrot")
    </example>
  </function>
  
  <function>
    <name>isCropWatered</name>
    <parameters>x: int, y: int</parameters>
    <return_type>bool</return_type>
    <execution_time>Instant (no yield)</execution_time>
    <yields>false</yields>
    <description>
      Returns True if the crop at (x, y) has been watered recently,
      False otherwise.
    </description>
    <implementation_type>Regular method (not IEnumerator)</implementation_type>
    <example>
if not isCropWatered(5, 5):
    water(5, 5)
    </example>
  </function>
  
  <function>
    <name>getInventoryCount</name>
    <parameters>item_name: string</parameters>
    <return_type>int</return_type>
    <execution_time>Instant (no yield)</execution_time>
    <yields>false</yields>
    <description>
      Returns the quantity of the specified item in the player's inventory.
      Returns 0 if the item is not in inventory.
    </description>
    <implementation_type>Regular method (not IEnumerator)</implementation_type>
    <example>
carrots = getInventoryCount("carrot")
print("You have", carrots, "carrots")
    </example>
  </function>
  
  <function>
    <name>getGridWidth</name>
    <parameters>none</parameters>
    <return_type>int</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>
      Returns the width of the farming grid (number of columns).
    </description>
    <implementation_type>Regular method</implementation_type>
  </function>
  
  <function>
    <name>getGridHeight</name>
    <parameters>none</parameters>
    <return_type>int</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>
      Returns the height of the farming grid (number of rows).
    </description>
    <implementation_type>Regular method</implementation_type>
  </function>
  
  <!-- TEMPLATE FOR ADDING NEW GAME FUNCTION:
  
  <function>
    <name>YOUR_FUNCTION_NAME</name>
    <parameters>param1: type1, param2: type2</parameters>
    <return_type>void | string | int | bool</return_type>
    <execution_time>Describe timing (instant or ~X seconds)</execution_time>
    <yields>true | false</yields>
    <description>
      Clear description of what the function does.
      Include any important behavioral notes.
    </description>
    <implementation_type>IEnumerator | Regular method</implementation_type>
    <possible_returns>
      (If return_type is not void, list possible return values)
    </possible_returns>
    <example>
# Example usage code here
    </example>
    <error_handling>
      List all possible error conditions and what exceptions are thrown.
    </error_handling>
  </function>
  
  AFTER ADDING A FUNCTION HERE:
  1. Implement it in GameBuiltinMethods.cs
  2. Register it in RegisterGameFunctions() method
  3. Add test case in Section 5
  4. Document in user-facing guide
  -->
</game_builtins>
```

### 4.2 Standard Built-in Functions

```xml
<standard_builtins>
  <description>
    Standard Python-like built-in functions that don't interact with the game.
  </description>
  
  <function>
    <name>print</name>
    <parameters>*args</parameters>
    <description>
      Prints values to the in-game console.
      Automatically converts all arguments to strings and joins with spaces.
    </description>
    <example>
print("Score:", score, "Time:", time)
    </example>
  </function>
  
  <function>
    <name>sleep</name>
    <parameters>seconds: float</parameters>
    <description>
      Pauses script execution for the specified duration.
      ALWAYS yields WaitForSeconds, independent of instruction budget.
    </description>
    <yields>true</yields>
    <example>
print("Starting...")
sleep(2.0)
print("2 seconds later")
    </example>
  </function>
  
  <function>
    <name>range</name>
    <parameters>start, stop, step (Python-style)</parameters>
    <return_type>list</return_type>
    <description>
      Generates a list of numbers.
      range(5) → [0, 1, 2, 3, 4]
      range(2, 8) → [2, 3, 4, 5, 6, 7]
      range(0, 10, 2) → [0, 2, 4, 6, 8]
    </description>
  </function>
  
  <function>
    <name>len</name>
    <parameters>obj: list | string | dict</parameters>
    <return_type>int</return_type>
    <description>Returns the length of a collection or string</description>
  </function>
  
  <function>
    <name>str</name>
    <parameters>obj: any</parameters>
    <return_type>string</return_type>
    <description>Converts any value to its string representation</description>
  </function>
  
  <function>
    <name>int</name>
    <parameters>value: any</parameters>
    <return_type>int</return_type>
    <description>
      Converts value to integer.
      Truncates floats (int(3.7) → 3)
      Parses strings (int("42") → 42)
    </description>
  </function>
  
  <function>
    <name>float</name>
    <parameters>value: any</parameters>
    <return_type>float</return_type>
    <description>Converts value to floating-point number</description>
  </function>
  
  <function>
    <name>abs</name>
    <parameters>x: number</parameters>
    <return_type>number</return_type>
    <description>Returns absolute value</description>
  </function>
  
  <function>
    <name>min</name>
    <parameters>*args or list</parameters>
    <return_type>number</return_type>
    <description>
      Returns minimum value.
      min(1, 5, 3) → 1
      min([1, 5, 3]) → 1
    </description>
  </function>
  
  <function>
    <name>max</name>
    <parameters>*args or list</parameters>
    <return_type>number</return_type>
    <description>
      Returns maximum value.
      max(1, 5, 3) → 5
      max([1, 5, 3]) → 5
    </description>
  </function>
  
  <function>
    <name>sum</name>
    <parameters>list: list</parameters>
    <return_type>number</return_type>
    <description>Returns sum of all numbers in list</description>
  </function>
  
  <function>
    <name>sorted</name>
    <parameters>list: list, key: function (optional)</parameters>
    <return_type>list</return_type>
    <description>
      Returns a new sorted list (does not modify original).
      Supports optional key function for custom sorting.
    </description>
    <example>
nums = [3, 1, 4, 1, 5]
sorted_nums = sorted(nums)  # [1, 1, 3, 4, 5]

# Sort with key function
items = [{"val": 3}, {"val": 1}]
sorted_items = sorted(items, key=lambda x: x["val"])
    </example>
  </function>
</standard_builtins>
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 5: TEST SUITE & VALIDATION
## ═══════════════════════════════════════════════════════════════

**[MODIFY HERE - ADD NEW TEST CASES]**

```xml
<test_suite>
  <meta_instruction>
    All generated code MUST pass these test cases.
    Before generating final code:
    1. Review each test case
    2. Validate your implementation handles each scenario
    3. If any test fails, fix the issue before delivering code
  </meta_instruction>
  
  <test_category name="Operator Precedence">
    <test_case id="OP-1">
      <description>Complex boolean expression with mixed operators</description>
      <code>
a = True
b = 3
c = 5
d = False
if a and b &amp; c or 2 == 5 // 2:
    print("PASS: Complex operators work")
      </code>
      <expected_behavior>
        Evaluation order (by precedence):
        1. 5 // 2 = 2 (integer division - highest arithmetic precedence)
        2. 2 == 2 = True (comparison)
        3. 3 &amp; 5 = 1 (bitwise AND - gets 0b011 &amp; 0b101 = 0b001)
        4. True and 1 = True (logical AND, 1 is truthy)
        5. True or True = True (logical OR)
        Final result: True, so "PASS" should print
      </expected_behavior>
      <operator_precedence_check>
        // (division) > == (comparison) > &amp; (bitwise) > and > or
      </operator_precedence_check>
    </test_case>
    
    <test_case id="OP-2">
      <description>Arithmetic precedence (multiplication before addition)</description>
      <code>
result = 2 + 3 * 4
if result == 14:
    print("PASS: Arithmetic precedence correct")
else:
    print("FAIL: Expected 14, got", result)
      </code>
      <expected_result>14 (not 20)</expected_result>
    </test_case>
    
    <test_case id="OP-3">
      <description>NOT precedence higher than AND/OR</description>
      <code>
a = True
b = False
c = True
if not a and b or c:
    print("PASS: NOT precedence works")
# Should evaluate as: (not a) and b or c
# = (False and False) or True
# = False or True
# = True
      </code>
      <expected_result>Should print "PASS"</expected_result>
    </test_case>
  </test_category>
  
  <test_category name="Indentation & Control Flow">
    <test_case id="IND-1">
      <description>Nested indentation with multiple levels</description>
      <code>
x = 5
if x > 0:
    print("Positive")
    if x > 3:
        print("Greater than 3")
        if x == 5:
            print("Exactly 5")
    print("Still in first if")
print("Outside all ifs")
      </code>
      <expected_output>
        Positive
        Greater than 3
        Exactly 5
        Still in first if
        Outside all ifs
      </expected_output>
    </test_case>
    
    <test_case id="IND-2">
      <description>Dedent mismatch detection</description>
      <code>
if True:
    print("Line 1")
      print("Line 2")  # 6 spaces - not aligned!
      </code>
      <expected_behavior>
        Should throw LexerError: "Indentation mismatch"
        because dedent doesn't align with previous indent level
      </expected_behavior>
    </test_case>
  </test_category>
  
  <test_category name="Instruction Budget System">
    <test_case id="BUDGET-1">
      <description>Small loop executes instantly (under budget)</description>
      <code>
sum = 0
for i in range(50):
    sum += i
print("Sum:", sum)
      </code>
      <expected_behavior>
        ~50 operations total (under 100 instruction budget).
        Should execute in single frame without yielding.
      </expected_behavior>
    </test_case>
    
    <test_case id="BUDGET-2">
      <description>Large loop time-slices across frames</description>
      <code>
sum = 0
for i in range(500):
    sum += i
print("Sum:", sum)
      </code>
      <expected_behavior>
        ~500 operations total (5x instruction budget).
        Should yield ~5 times (once per 100 operations).
        Should produce correct sum (124750).
      </expected_behavior>
    </test_case>
    
    <test_case id="BUDGET-3">
      <description>Sleep overrides instruction budget</description>
      <code>
for i in range(5):
    print("Count:", i)
    sleep(0.5)
      </code>
      <expected_behavior>
        Even though only ~10 operations (under budget),
        should yield 5 times for the sleep() calls.
        Each sleep must yield WaitForSeconds(0.5).
        Total execution time: ~2.5 seconds.
      </expected_behavior>
    </test_case>
  </test_category>
  
  <test_category name="Game Built-in Functions">
    <test_case id="GAME-1">
      <description>harvest() function blocks during animation</description>
      <code>
for x in range(5):
    state = getCropState(x, 0)
    if state == "ripe":
        print("Harvesting at", x)
        harvest(x, 0)  # Should block for ~0.2 seconds
        print("Harvested at", x)
      </code>
      <expected_behavior>
        Each harvest() call must:
        1. Yield control to Unity
        2. Wait for animation IEnumerator to complete (~0.2 sec)
        3. Resume Python execution after animation
        4. NOT count against instruction budget
      </expected_behavior>
    </test_case>
    
    <test_case id="GAME-2">
      <description>Query functions return immediately (no yield)</description>
      <code>
for x in range(10):
    for y in range(10):
        state = getCropState(x, y)  # Should NOT yield
        # Entire loop should execute in 1-2 frames
      </code>
      <expected_behavior>
        getCropState() is NOT an IEnumerator.
        Should execute instantly without yielding.
        Entire loop: ~200 operations = ~2 yields for budget only.
      </expected_behavior>
    </test_case>
  </test_category>
  
  <test_category name="List Operations">
    <test_case id="LIST-1">
      <description>Negative indexing</description>
      <code>
items = [10, 20, 30, 40, 50]
print(items[-1])   # Should print 50
print(items[-2])   # Should print 40
print(items[-5])   # Should print 10
      </code>
      <expected_output>50, 40, 10</expected_output>
    </test_case>
    
    <test_case id="LIST-2">
      <description>List slicing</description>
      <code>
nums = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
print(nums[2:5])    # [2, 3, 4]
print(nums[:3])     # [0, 1, 2]
print(nums[7:])     # [7, 8, 9]
print(nums[::2])    # [0, 2, 4, 6, 8] (with step)
      </code>
      <expected_output>As commented in code</expected_output>
    </test_case>
    
    <test_case id="LIST-3">
      <description>List comprehension</description>
      <code>
nums = [1, 2, 3, 4, 5]
doubled = [x * 2 for x in nums]
print(doubled)  # [2, 4, 6, 8, 10]

evens = [x for x in nums if x % 2 == 0]
print(evens)  # [2, 4]
      </code>
      <expected_output>[2, 4, 6, 8, 10] then [2, 4]</expected_output>
    </test_case>
    
    <test_case id="LIST-4">
      <description>List methods (append, remove, pop, sort)</description>
      <code>
items = [3, 1, 4]
items.append(2)
print(items)  # [3, 1, 4, 2]

items.sort()
print(items)  # [1, 2, 3, 4]

items.remove(3)
print(items)  # [1, 2, 4]

last = items.pop()
print(last, items)  # 4 [1, 2]
      </code>
      <expected_output>As commented in code</expected_output>
    </test_case>
  </test_category>
  
  <test_category name="String Operations">
    <test_case id="STR-1">
      <description>String methods</description>
      <code>
text = "  hello world  "
print(text.strip())      # "hello world"
print(text.upper())      # "  HELLO WORLD  "
print(text.replace("world", "python"))  # "  hello python  "

words = text.strip().split()
print(words)  # ["hello", "world"]
print(" ".join(words))  # "hello world"
      </code>
      <expected_output>As commented in code</expected_output>
    </test_case>
  </test_category>
  
  <test_category name="Functions & Lambda">
    <test_case id="FUNC-1">
      <description>Function definition and call</description>
      <code>
def add(a, b):
    return a + b

result = add(5, 3)
print("Result:", result)  # Result: 8
      </code>
      <expected_output>Result: 8</expected_output>
    </test_case>
    
    <test_case id="FUNC-2">
      <description>Lambda functions</description>
      <code>
double = lambda x: x * 2
print(double(5))  # 10

add = lambda a, b: a + b
print(add(3, 4))  # 7
      </code>
      <expected_output>10, then 7</expected_output>
    </test_case>
    
    <test_case id="FUNC-3">
      <description>Sorting with lambda key</description>
      <code>
items = [{"name": "apple", "price": 3}, 
         {"name": "banana", "price": 1},
         {"name": "cherry", "price": 2}]

sorted_items = sorted(items, key=lambda x: x["price"])
for item in sorted_items:
    print(item["name"])
# Should print: banana, cherry, apple
      </code>
      <expected_output>banana, cherry, apple</expected_output>
    </test_case>
  </test_category>
  
  <test_category name="Classes">
    <test_case id="CLASS-1">
      <description>Basic class with __init__ and methods</description>
      <code>
class Point:
    def __init__(self, x, y):
        self.x = x
        self.y = y
    
    def distance(self):
        return (self.x * self.x + self.y * self.y) ** 0.5

p = Point(3, 4)
print("Distance:", p.distance())  # 5.0
      </code>
      <expected_output>Distance: 5.0</expected_output>
    </test_case>
  </test_category>
  
  <test_category name="Dictionaries">
    <test_case id="DICT-1">
      <description>Dictionary operations</description>
      <code>
d = {"a": 1, "b": 2, "c": 3}
print(d["a"])  # 1

d["d"] = 4
print(len(d))  # 4

if "b" in d:
    print("b exists")

for key in d.keys():
    print(key, d[key])
      </code>
      <expected_output>1, 4, "b exists", then all key-value pairs</expected_output>
    </test_case>
  </test_category>
  
  <test_category name="Error Handling">
    <test_case id="ERR-1">
      <description>Index out of range error with line number</description>
      <code>
items = [1, 2, 3]
print(items[5])  # Line 2
      </code>
      <expected_error>
        RuntimeError (Line 2): IndexError - list index out of range
      </expected_error>
    </test_case>
    
    <test_case id="ERR-2">
      <description>Undefined variable error with line number</description>
      <code>
x = 5
print(unknown_var)  # Line 2
      </code>
      <expected_error>
        RuntimeError (Line 2): NameError - undefined variable 'unknown_var'
      </expected_error>
    </test_case>
    
    <test_case id="ERR-3">
      <description>Division by zero error</description>
      <code>
x = 10
y = 0
result = x / y  # Line 3
      </code>
      <expected_error>
        RuntimeError (Line 3): Division by zero
      </expected_error>
    </test_case>
  </test_category>
  
  <!-- TEMPLATE FOR ADDING NEW TEST CASE:
  
  <test_case id="CATEGORY-NUMBER">
    <description>Brief description of what is being tested</description>
    <code>
# Test code here
# Include comments explaining expected behavior
    </code>
    <expected_behavior>
      Detailed explanation of expected behavior
    </expected_behavior>
    <expected_output>Expected console output</expected_output>
    <expected_error>Expected error message (if testing errors)</expected_error>
    <validation_points>
      - Specific point to validate
      - Another specific point
    </validation_points>
  </test_case>
  
  -->
</test_suite>
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 6: FILE ORGANIZATION & CODE STRUCTURE
## ═══════════════════════════════════════════════════════════════

### 6.1 File Naming Convention

All C# files must follow these conventions:
* **Classes:** `ClassName.cs` (PascalCase)
* **Regions:** Use `#region` to organize code sections:
  - `#region Public API` - Public methods/properties
  - `#region Private API` - Private/protected methods
  - `#region Unity Lifecycle` - Start, Update, OnDestroy, etc.
  - `#region Enums` - Enum definitions
  - `#region Events` - Event declarations
  - `#region Nested Classes` - Inner class definitions

### 6.2 Required Files (Complete List)

1. **Token.cs** - Token class and TokenType enum
2. **Lexer.cs** - Tokenizer with indentation handling
3. **AST.cs** - All AST node classes (Stmt, Expr, and subclasses)
4. **Parser.cs** - Recursive descent parser
5. **PythonInterpreter.cs** - Main execution engine with instruction budget
6. **Scope.cs** - Variable scope management
7. **BuiltinFunction.cs** - Wrapper for built-in functions
8. **ClassInstance.cs** - Runtime class instance representation
9. **GameBuiltinMethods.cs** - Unity-specific game commands
10. **CoroutineRunner.cs** - Safe coroutine execution wrapper
11. **ConsoleManager.cs** - UI console for print() output
12. **DemoScripts.cs** - All test scripts as string constants
13. **Exceptions.cs** - Custom exception classes (LexerError, ParserError, RuntimeError)

### 6.3 Code Quality Requirements

* **Comments:** Every public method must have XML documentation
* **Error Messages:** All errors must include context (line number, variable name)
* **Validation:** All user input (indices, keys, arguments) must be validated
* **Null Checks:** All object access must check for null
* **.NET 2.0 Compliance:** No `yield return` inside try-catch in IEnumerators

---

## ═══════════════════════════════════════════════════════════════
## SECTION 7: ADVANCED FEATURES & EDGE CASES
## ═══════════════════════════════════════════════════════════════

### 7.1 Edge Cases for Indexing

```python
# Empty list slicing
empty = []
print("Empty slice:", empty[0:10])  # Should be []

# Out of bounds slicing (should not error)
nums = [1, 2, 3]
print("OOB slice:", nums[10:20])  # Should be []

# Negative indices beyond list size
print(nums[-100])  # Should throw IndexError with line number
```

### 7.2 Recursion Depth

```python
# Test deep recursion (should handle up to 1000 calls)
def deep_recurse(n):
    if n == 0:
        return "done"
    return deep_recurse(n - 1)

print(deep_recurse(100))  # Should succeed
# print(deep_recurse(2000))  # May hit stack limit - should error gracefully
```

### 7.3 Type Coercion in Comparisons

```python
# Number to string comparison
if str(42) == "42":
    print("Type coercion works")

# List equality (should compare contents, not references)
list1 = [1, 2, 3]
list2 = [1, 2, 3]
if list1 == list2:
    print("Deep equality works")
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 8: IMPLEMENTATION CHECKLIST & GENERATION COMMAND
## ═══════════════════════════════════════════════════════════════

### 8.1 AI Generation Checklist

When generating code, ensure you:

- ✅ Create **separate** `.cs` files for each class
- ✅ Use `#region` tags to organize code
- ✅ Add XML documentation (`/// <summary>`) for all public methods
- ✅ Implement **all** test cases from Section 5
- ✅ Handle **all** exceptions with line numbers
- ✅ Implement instruction budget with `INSTRUCTIONS_PER_FRAME = 100`
- ✅ Support **decimal and integer** numbers (store as `double`)
- ✅ Implement `sleep()` to **always** yield, independent of budget
- ✅ Support complex boolean logic: `(a and b) or (c and not d)`
- ✅ Handle negative list indices: `items[-1]`
- ✅ Support list slicing: `items[1:4]`, `items[:3]`, `items[3:]`
- ✅ Implement list comprehensions: `[x*2 for x in nums if x > 0]`
- ✅ Implement lambda functions: `lambda x, y: x + y`
- ✅ Support sorting with key: `list.sort(key=lambda x: x.val)`
- ✅ Implement all string methods: `split`, `join`, `strip`, `replace`
- ✅ Implement all list methods: `append`, `remove`, `pop`, `sort`
- ✅ Support classes with `__init__` and `self`
- ✅ Support dictionaries with any hashable key
- ✅ Handle nested data structures (lists in dicts, dicts in lists)
- ✅ Implement recursion (up to 1000 depth)
- ✅ Validate all array access (throw IndexError/KeyError)
- ✅ Reset interpreter state on error (`Interpreter.Reset()`)
- ✅ Comply with .NET 2.0 restrictions (no yield in try-catch)

### 8.2 Error Reporting Template

Every error must follow this format:
```
[ERROR TYPE] (Line X): [Detailed message]
```

Examples:
```
LexerError (Line 5): Indentation mismatch - expected 4 spaces, found 2
ParserError (Line 12): Expected ')' after function arguments
RuntimeError (Line 23): IndexError - list index out of range
RuntimeError (Line 45): NameError - undefined variable 'player_pos'
```

### 8.3 Performance Targets

* **Small loops** (<100 iterations): Execute in 1 frame (instant)
* **Medium loops** (100-1000 iterations): Time-slice across 1-10 frames
* **Large loops** (>1000 iterations): Time-slice proportionally
* **Sleep calls**: Always yield for exact duration, independent of ops
* **Game commands**: Always yield and pause Python until Unity action completes

---

## ═══════════════════════════════════════════════════════════════
## SECTION 9: FINAL GENERATION COMMAND
## ═══════════════════════════════════════════════════════════════

**GENERATE NOW:**

Create **all** C# files as separate artifacts with:

1. ✅ Proper `#region` organization
2. ✅ XML documentation on public methods
3. ✅ Complete error handling with line numbers
4. ✅ Full instruction budget implementation
5. ✅ All test cases from Section 5 validated
6. ✅ .NET 2.0 compliance (no yield in try-catch)
7. ✅ Unity 2020.3+ compatibility

**Target Unity Version:** 2020.3+  
**Target .NET:** 2.0 Standard  
**Estimated Lines of Code:** 3000-5000 total across all files

**Before generating:**
1. Review the entire specification
2. Check all test cases in Section 5
3. Verify operator precedence in Section 3.4
4. Confirm all game built-ins in Section 4.1

---

## ═══════════════════════════════════════════════════════════════
## APPENDIX: QUICK REFERENCE GUIDE
## ═══════════════════════════════════════════════════════════════

### Modification Quick Reference

**Want to add a new operator?**
→ Go to Section 2.1.1 (Token Types) + Section 3.4 (Precedence)

**Want to add a game function?**
→ Go to Section 4.1 (Game Builtins)

**Want to change indentation rules?**
→ Go to Section 2.2.2 (Indentation Rules)

**Want to add multi-line comments?**
→ Go to Section 2.2.3 (Comment Handling)

**Want to add a test case?**
→ Go to Section 5 (Test Suite)

**Want to modify instruction budget?**
→ Go to Section 1.1.2 (Instruction Budget)

**Want to add a new data type?**
→ Go to Section 1.2.1 (Type System)

---

## END OF SPECIFICATION

**This specification is complete and ready for code generation.**

All sections are marked with `[MODIFY HERE]` tags for easy location of modification points.

Begin generation now.
**Generate the code now (covering all edge cases, and .Net 2.0 limitation that i mentioned at the begining) all in seperate files as required**