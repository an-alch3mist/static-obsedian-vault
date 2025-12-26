# LOOP Language Prompt - Anatomy & Modification Guide

## 📚 Table of Contents

1. [Understanding Prompt Anatomy](#understanding-prompt-anatomy)
2. [The Three-Layer Structure](#the-three-layer-structure)
3. [Modification Patterns by Use Case](#modification-patterns-by-use-case)
4. [XML vs Prose: When to Use Each](#xml-vs-prose-when-to-use-each)
5. [Test-Driven Prompting](#test-driven-prompting)
6. [Advanced Techniques](#advanced-techniques)

---

## 📖 Understanding Prompt Anatomy

### What is a "System Specification Prompt"?

This type of prompt is a **technical blueprint** that combines:

1. **Requirements Documentation** - What the system must do
2. **API Specification** - How components interact
3. **Test Suite** - Validation criteria
4. **Implementation Guide** - How to build it

Think of it as:
- 📋 **The "What"** - Language features, operators, data types
- 🔧 **The "How"** - Implementation details, algorithms
- ✅ **The "Verify"** - Test cases that validate correctness
- 📝 **The "Extend"** - Modification points for future changes

---

## 🏗️ The Three-Layer Structure

### Layer 1: Meta-Instructions (The Control Tower)

```xml
<meta_instruction priority="CRITICAL">
  Before generating any code, the AI MUST:
  1. Read ENTIRE specification
  2. Review ALL test cases
  3. Verify code will pass tests
</meta_instruction>
```

**Purpose:** Directs the AI's approach before it starts generating code

**When to modify:**
- You want the AI to prioritize certain aspects
- You need to enforce specific workflows
- You want to add quality gates

**Example modification:**
```xml
<meta_instruction priority="CRITICAL">
  Before generating any code, the AI MUST:
  1. Read ENTIRE specification
  2. Review ALL test cases
  3. Verify code will pass tests
  4. ✨ CHECK for security vulnerabilities ✨ [YOUR ADDITION]
  5. ✨ VALIDATE against OWASP top 10 ✨ [YOUR ADDITION]
</meta_instruction>
```

---

### Layer 2: Specification Sections (The Blueprint)

The main body is organized into **numbered sections**:

```
Section 1: System Architecture
Section 2: Lexer & Tokenization
Section 3: Parser & AST
Section 4: Game Built-ins & Runtime
Section 5: Test Suite
Section 6: File Organization
Section 7: Advanced Features
Section 8: Implementation Checklist
```

Each section has a specific purpose:

#### **Section 1: System Architecture**
- **What it defines:** Core execution model, data types, constraints
- **Modify when:** You want to change fundamental behavior (instruction budget, type system)

#### **Section 2: Lexer & Tokenization**
- **What it defines:** Token types, operators, keywords, comments, indentation
- **Modify when:** Adding new syntax (operators, keywords, comment styles)

#### **Section 3: Parser & AST**
- **What it defines:** Grammar rules, AST nodes, expression evaluation
- **Modify when:** Adding new language constructs (loops, conditionals, expressions)

#### **Section 4: Game Built-ins**
- **What it defines:** Functions the game provides to scripts
- **Modify when:** Adding new game functions or query methods

#### **Section 5: Test Suite**
- **What it defines:** Validation tests that must pass
- **Modify when:** Adding new features (always add corresponding tests!)

#### **Sections 6-8: Meta-information**
- **What they define:** File structure, quality requirements, checklists
- **Modify when:** Changing code organization or quality standards

---

### Layer 3: Modification Markers (The Change Points)

Every section that's meant to be modified has a **clear marker**:

```xml
**[MODIFY HERE - DESCRIPTIVE NAME]**
```

This is followed by XML that **shows the structure** you should follow.

**Example from the prompt:**

```xml
**[MODIFY HERE - ADD NEW OPERATORS OR KEYWORDS]**

<token_types>
  <arithmetic_operators>
    <tokens>PLUS, MINUS, STAR, SLASH, PERCENT</tokens>
    <symbols>+, -, *, /, %</symbols>
    
    <!-- TO ADD NEW ARITHMETIC OPERATOR:
         Example: Add exponentiation (**)
         1. Add token: POWER
         2. Add symbol: **
         3. Add to operator precedence (Section 3.4)
         4. Implement in evaluateExpression() (Section 3.3)
    -->
  </arithmetic_operators>
</token_types>
```

Notice:
1. ✅ Clear marker: `[MODIFY HERE - ...]`
2. ✅ XML structure showing current state
3. ✅ Inline comments explaining **how to add** new items
4. ✅ References to related sections

---

## 🔧 Modification Patterns by Use Case

### Use Case 1: Adding `harvest()` Function with Animation

**Question:** "Where do I add a game function that takes 0.2 seconds to execute?"

**Answer:** Section 4.1 - Game Built-ins

**Step-by-step:**

1. **Locate the section:**
   ```
   Search for: "[MODIFY HERE - ADD NEW GAME FUNCTIONS]"
   Or: "Section 4.1"
   ```

2. **Follow the template:**
   ```xml
   <function>
     <n>harvest</n>
     <parameters>x: int, y: int</parameters>
     <return_type>void</return_type>
     <execution_time>Approximately 0.2 seconds (harvest animation duration)</execution_time>
     <yields>true</yields>  ← IMPORTANT: Tells AI this blocks execution
     <description>
       Harvests the crop at grid position (x, y).
       Execution pauses during harvest animation.
     </description>
     <implementation_type>IEnumerator</implementation_type>  ← Unity coroutine
     <usage_pattern>
       Always check crop state before harvesting:
       if getCropState(x, y) == "ripe":
           harvest(x, y)
     </usage_pattern>
   </function>
   ```

3. **Add corresponding test case in Section 5:**
   ```xml
   <test_case id="GAME-HARVEST">
     <description>harvest() function blocks during animation</description>
     <code>
   for x in range(5):
       if getCropState(x, 0) == "ripe":
           harvest(x, 0)  # Should block for ~0.2 seconds
     </code>
     <expected_behavior>
       Each harvest() must yield to Unity for animation
     </expected_behavior>
   </test_case>
   ```

**Why this structure?**
- The XML provides **type information** (parameters, return type)
- The `<yields>` tag tells the AI this is a **blocking operation**
- The `<implementation_type>` specifies **Unity coroutine** pattern
- The test case **validates** the blocking behavior

---

### Use Case 2: Adding Indentation Rules

**Question:** "How do I enforce 2-space indentation instead of 4-space?"

**Answer:** Section 2.2.2 - Indentation Rules

**Step-by-step:**

1. **Find the modification point:**
   ```
   Search for: "[MODIFY HERE - INDENTATION RULES]"
   ```

2. **Update the XML:**
   ```xml
   <indentation_rules>
     <unit>
       <spaces_per_level>2</spaces_per_level>  ← CHANGE FROM 4 TO 2
       <tabs_allowed>false</tabs_allowed>
     </unit>
     
     <validation>
       <rule>All indentation must be multiples of 2 spaces</rule>  ← UPDATE
       <rule>Dedentation must align with a previous indentation level</rule>
     </validation>
   </indentation_rules>
   ```

3. **Update the C# implementation note:**
   ```csharp
   // In Lexer validation
   input = input.Replace("\t", "  ");  // Convert tabs to 2 spaces (not 4)
   ```

**Why XML here?**
- **Structured data** is easier for AI to parse than prose
- Clear **field names** (`spaces_per_level`) reduce ambiguity
- **Validation rules** are explicitly listed

---

### Use Case 3: Adding a New Operator (e.g., `%` modulo)

**Question:** "How do I add the modulo operator `%`?"

**Answer:** Multiple sections! This shows the **cascading nature** of features.

**Step-by-step:**

1. **Section 2.1.1 - Add Token Type:**
   ```xml
   <arithmetic_operators>
     <tokens>PLUS, MINUS, STAR, SLASH, PERCENT</tokens>  ← ADD PERCENT
     <symbols>+, -, *, /, %</symbols>  ← ADD %
   </arithmetic_operators>
   ```

2. **Section 3.3 - Add Evaluation Logic:**
   ```csharp
   case TokenType.PERCENT:
       return ToNumber(left) % ToNumber(right);
   ```

3. **Section 3.4 - Add to Precedence Table:**
   ```xml
   <level priority="4" associativity="left-to-right">
     <n>Multiplicative</n>
     <operators>* / %</operators>  ← ADD % HERE
   </level>
   ```

4. **Section 5 - Add Test Case:**
   ```xml
   <test_case id="OP-MODULO">
     <description>Modulo operator</description>
     <code>
   print(7 % 3)  # Should print 1
   print(10 % 5)  # Should print 0
     </code>
   </test_case>
   ```

**Key insight:** Adding a feature requires **coordinated changes** across multiple sections!

---

### Use Case 4: Adding Multi-line Comments

**Question:** "How do I add `/* ... */` multi-line comments?"

**Answer:** Section 2.2.3 - Comment Handling

**Step-by-step:**

1. **Find the section:**
   ```
   Search for: "[MODIFY HERE - ADD MULTI-LINE COMMENTS]"
   ```

2. **Add to XML specification:**
   ```xml
   <comment_syntax>
     <multi_line>
       <syntax_start>/*</syntax_start>
       <syntax_end>*/</syntax_end>
       <description>Multi-line comment block</description>
       <nesting>Not allowed</nesting>
       <implementation_note>
         Lexer must track whether inside multi-line comment.
         When /* encountered, skip all characters until */ found.
       </implementation_note>
     </multi_line>
   </comment_syntax>
   ```

3. **Add implementation guidance (inline comments):**
   ```xml
   <!-- TO IMPLEMENT MULTI-LINE COMMENTS IN LEXER:
        1. Add boolean flag: insideMultiLineComment = false
        2. In ScanToken(), check for /* sequence
        3. When found, set flag true and skip until */ found
        4. Remember to increment lineNumber on \n inside comments
        5. Throw error if EOF reached while still in comment
   -->
   ```

**Why the inline comments?**
- Provides **step-by-step implementation guide**
- AI can follow these steps when generating `Lexer.cs`
- Acts as **pseudo-code** without being too prescriptive

---

### Use Case 5: Adding Return-Type Built-ins

**Question:** "How do I add `getCropState(x, y)` that returns a string?"

**Answer:** Section 4.1 - Game Built-ins (same as harvest, different pattern)

**Key difference:** This function **does NOT yield** (instant return)

```xml
<function>
  <n>getCropState</n>
  <parameters>x: int, y: int</parameters>
  <return_type>string</return_type>  ← NOT void!
  <execution_time>Instant (no yield)</execution_time>
  <yields>false</yields>  ← IMPORTANT: No blocking
  <possible_returns>
    "empty", "planted", "growing", "ripe", "withered"
  </possible_returns>  ← Enum-like return values
  <description>Returns current state of crop at position (x, y)</description>
  <implementation_type>Regular method (not IEnumerator)</implementation_type>
</function>
```

**Differences from `harvest()`:**
- `<yields>false</yields>` → No coroutine needed
- `<possible_returns>` → Documents valid return values
- `<implementation_type>Regular method` → Not an IEnumerator

**Pattern recognition:** The AI learns:
- `yields=true` → Use `IEnumerator` pattern
- `yields=false` → Use regular method
- `return_type=void` → No return statement
- `return_type=string` → Must return a string

---

### Use Case 6: Complex Operator Precedence

**Question:** "How do I ensure `a and b & c or 2 == 5 // 2` evaluates correctly?"

**Answer:** Section 3.4 - Operator Precedence + Section 5 - Test Case

**Why this is critical:**

Without proper precedence, this expression could evaluate incorrectly!

**Step 1: Define precedence in Section 3.4:**

```xml
<operator_precedence>
  <level priority="4">
    <operators>* / // %</operators>  ← // has high precedence
  </level>
  
  <level priority="10">
    <operators>== != &lt; &gt;</operators>  ← == comes later
  </level>
  
  <level priority="7">
    <operators>&amp;</operators>  ← Bitwise AND
  </level>
  
  <level priority="11">
    <operators>and</operators>  ← Logical AND
  </level>
  
  <level priority="12">
    <operators>or</operators>  ← Logical OR (lowest)
  </level>
</operator_precedence>
```

**Step 2: Add example with evaluation order:**

```xml
<example>
  <expression>a and b &amp; c or 2 == 5 // 2</expression>
  <evaluation_order>
    1. 5 // 2 → 2 (highest precedence)
    2. 2 == 2 → True (comparison)
    3. b &amp; c → bitwise result
    4. a and (result from 3) (logical AND)
    5. (result from 4) or True (logical OR)
  </evaluation_order>
</example>
```

**Step 3: Add test case in Section 5:**

```xml
<test_case id="OP-COMPLEX">
  <description>Complex operator precedence</description>
  <code>
a = True
b = 3
c = 5
if a and b &amp; c or 2 == 5 // 2:
    print("PASS")
  </code>
  <expected_behavior>Should print "PASS"</expected_behavior>
  <operator_precedence_check>
    // > == > &amp; > and > or
  </operator_precedence_check>
</test_case>
```

**Why this three-part approach?**
1. **Section 3.4** gives the AI the **precedence table** to build the parser
2. **Example** shows **step-by-step evaluation** so AI understands order
3. **Test case** provides **validation** - AI can verify correctness

---

## 📝 XML vs Prose: When to Use Each

### Use XML When:

✅ **Structured data** with clear fields
```xml
<function>
  <n>harvest</n>
  <parameters>x: int, y: int</parameters>
  <return_type>void</return_type>
</function>
```
→ AI can parse this unambiguously

✅ **Lists of items** with properties
```xml
<operators>
  <arithmetic>PLUS, MINUS, STAR, SLASH</arithmetic>
  <comparison>EQUAL_EQUAL, BANG_EQUAL</comparison>
</operators>
```
→ Clear categorization

✅ **Hierarchical relationships**
```xml
<operator_precedence>
  <level priority="1">...</level>
  <level priority="2">...</level>
</operator_precedence>
```
→ Priority is explicit

✅ **Enum-like values**
```xml
<possible_returns>
  "empty", "planted", "growing", "ripe", "withered"
</possible_returns>
```
→ Closed set of values

---

### Use Prose When:

✅ **Explaining behavior** or algorithms
```markdown
When `instructionCount >= INSTRUCTIONS_PER_FRAME`:
  - Yield control to Unity
  - Reset counter
  - Continue execution
```
→ Procedural flow is clearer in prose

✅ **Providing context** or rationale
```markdown
**Purpose:** Prevent frame drops while maintaining instant execution for small loops.
```
→ Human-readable explanation

✅ **Code examples**
```python
for i in range(50):
    print("iteration:", i)
    sleep(0.5)  # Pauses for 0.5 seconds
```
→ Concrete demonstration

✅ **Error message templates**
```
RuntimeError (Line X): [Detailed message]
```
→ Format specification

---

### Hybrid Approach (Best Practice):

Combine XML structure with prose examples:

```xml
<function>
  <n>harvest</n>
  <parameters>x: int, y: int</parameters>
  <return_type>void</return_type>
  <description>
    Harvests the crop at grid position (x, y).
    Execution pauses during harvest animation.
  </description>
  
  <example>
# Check before harvesting
if getCropState(x, y) == "ripe":
    harvest(x, y)  # Waits for animation
    print("Harvested!")
  </example>
</function>
```

**Why this works:**
- XML provides **machine-readable structure**
- Prose provides **human-readable context**
- Examples provide **concrete usage patterns**

---

## ✅ Test-Driven Prompting

### The Philosophy

**Traditional approach:**
1. Describe feature
2. AI generates code
3. Hope it works correctly

**Test-driven approach:**
1. Describe feature
2. **Provide test cases**
3. AI generates code **that passes tests**
4. Validation is built-in

### Why It Works

The AI has a **concrete success criterion**:
```xml
<meta_instruction>
  All generated code MUST pass every test case.
  If you cannot generate code that passes all tests,
  explain which test is ambiguous.
</meta_instruction>
```

This creates a **feedback loop** where the AI self-validates.

---

### Test Case Anatomy

Every test case should have:

```xml
<test_case id="UNIQUE-ID">
  <description>Brief description</description>
  
  <code>
# The test code
  </code>
  
  <expected_behavior>
What should happen, in detail
  </expected_behavior>
  
  <expected_output>Exact output if applicable</expected_output>
  
  <expected_error>Error message if testing errors</expected_error>
  
  <validation_points>
- Specific point to check
- Another specific point
  </validation_points>
</test_case>
```

---

### Example: Testing Instruction Budget

```xml
<test_case id="BUDGET-2">
  <description>Large loop time-slices across frames</description>
  
  <code>
sum = 0
for i in range(500):
    sum += i
print("Sum:", sum)
  </code>
  
  <expected_behavior>
    ~500 operations total (5x instruction budget of 100).
    Should yield approximately 5 times (once per 100 operations).
    Must produce correct sum (124750).
  </expected_behavior>
  
  <validation_points>
    - Instruction counter increments on each iteration
    - Yields occur when counter reaches 100
    - Counter resets after each yield
    - Final sum is mathematically correct
    - Total frame count is ~5-6 frames
  </validation_points>
</test_case>
```

**What this achieves:**
1. ✅ AI knows **exactly** what behavior to implement
2. ✅ AI has **specific numbers** to validate (500 ops, 5 yields, sum=124750)
3. ✅ AI has **multiple validation points** to check

---

### The "One-Liner" Technique

At the end of the test suite, add:

```xml
<critical_rule>
  All generated code must pass every test case in the test suite.
  Validate implementation logic against each test before outputting code.
</critical_rule>
```

This simple instruction **dramatically improves** code quality because:
- AI explicitly checks each test
- AI self-corrects before generating
- Edge cases are caught early

---

## 🎓 Advanced Techniques

### Technique 1: Cascading Modifications

When adding a feature, you often need to modify **multiple sections**.

**Pattern:** Use **internal references** to guide the AI:

```xml
<arithmetic_operators>
  <tokens>PLUS, MINUS, STAR, SLASH, PERCENT</tokens>
  
  <!-- TO ADD NEW ARITHMETIC OPERATOR:
       Example: Add exponentiation (**)
       1. Add token here: POWER
       2. Add to operator precedence (Section 3.4)
       3. Implement in evaluateExpression() (Section 3.3)
       4. Add test case (Section 5)
  -->
</arithmetic_operators>
```

This creates a **checklist** the AI follows automatically.

---

### Technique 2: Constraint Propagation

Use XML attributes to **propagate constraints**:

```xml
<function>
  <n>harvest</n>
  <yields>true</yields>  ← This attribute...
  <implementation_type>IEnumerator</implementation_type>  ← ...implies this
</function>
```

The AI learns the pattern:
- `yields=true` → **MUST** use IEnumerator
- `yields=false` → Use regular method

---

### Technique 3: Example-Based Learning

Provide **concrete examples** alongside abstract rules:

```xml
<operator_precedence>
  <level priority="4">
    <operators>* / %</operators>
  </level>
  <level priority="5">
    <operators>+ -</operators>
  </level>
  
  <example>
    <expression>2 + 3 * 4</expression>
    <r>14 (not 20)</r>
    <explanation>Multiply first: 3*4=12, then add: 2+12=14</explanation>
  </example>
</operator_precedence>
```

**Why both?**
- **Abstract rule** (precedence table) → Parser structure
- **Concrete example** (2+3*4) → Validation

---

### Technique 4: Progressive Disclosure

Start with **high-level structure**, then add detail:

```xml
<game_builtins>
  <description>Unity-specific functions accessible from Python scripts</description>
  
  <function>
    <n>harvest</n>
    <quick_ref>Harvests crop at (x, y), pauses ~0.2 sec</quick_ref>
    
    <full_spec>
      <parameters>x: int, y: int</parameters>
      <return_type>void</return_type>
      <execution_time>~0.2 seconds</execution_time>
      ...detailed info...
    </full_spec>
  </function>
</game_builtins>
```

AI can:
1. Scan `<quick_ref>` for overview
2. Dive into `<full_spec>` for implementation details

---

### Technique 5: Error-Driven Specification

Specify what **errors** should occur:

```xml
<test_case id="ERR-1">
  <description>Index out of range</description>
  <code>
items = [1, 2, 3]
print(items[5])  # Line 2
  </code>
  
  <expected_error>
    RuntimeError (Line 2): IndexError - list index out of range
  </expected_error>
  
  <error_handling_note>
    Must include:
    - Error type (RuntimeError)
    - Line number (Line 2)
    - Error category (IndexError)
    - Descriptive message
  </error_handling_note>
</test_case>
```

This ensures the AI implements **proper error handling**, not just happy paths.

---

## 🎯 Summary: Your Modification Workflow

1. **Identify the section** you need to modify
   - Use the Quick Modification Guide table
   - Search for `[MODIFY HERE - ...]` markers

2. **Follow the template** already present
   - XML structure shows the format
   - Inline comments provide step-by-step guidance

3. **Update related sections**
   - Check inline comments for references
   - Add corresponding test cases (Section 5)

4. **Test your changes**
   - Add test cases that validate the new feature
   - Use the `<expected_behavior>` field to be specific

5. **Generate code**
   - The AI will follow your modifications
   - Test cases provide automatic validation

---

## 🔄 Quick Modification Cheat Sheet

| **Feature** | **Primary Section** | **Related Sections** | **Test Required?** |
|-------------|---------------------|----------------------|-------------------|
| New operator | 2.1.1 (Tokens) | 3.3 (Eval), 3.4 (Precedence) | ✅ Yes |
| New keyword | 2.1.1 (Tokens) | 2.4 (Parser), 3.3 (Execution) | ✅ Yes |
| Game function | 4.1 (Game Built-ins) | None | ✅ Yes |
| Indentation rules | 2.2.2 (Indentation) | None | ✅ Yes |
| Comments | 2.2.3 (Comments) | 2.2 (Lexer) | ✅ Yes |
| Data type | 1.2.1 (Type System) | 3.3 (Eval), 3.4 (Operations) | ✅ Yes |
| Instruction budget | 1.1.2 (Budget) | None | ✅ Yes |
| Error format | 8.2 (Error Template) | All sections (error handling) | ⚠️ Optional |

---

## 🚀 Final Tips

1. **Always add test cases** when modifying features
2. **Use XML for structure**, prose for explanation
3. **Reference related sections** in inline comments
4. **Provide examples** alongside abstract rules
5. **Specify error behavior**, not just happy paths
6. **Let the AI self-validate** with test-driven approach

---

## 📚 Further Reading

For more on prompt engineering patterns:
- System prompts: Defining AI behavior
- Few-shot learning: Example-based specification
- Chain-of-thought: Step-by-step reasoning
- Test-driven AI: Validation-first approach

---

**End of Anatomy Guide**

You now understand:
✅ The three-layer structure of the prompt
✅ How to locate modification points
✅ When to use XML vs prose
✅ How test-driven prompting works
✅ Advanced techniques for complex features

**Ready to modify your LOOP language specification!** 🎉