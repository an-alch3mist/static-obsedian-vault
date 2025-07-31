https://grok.com/share/c2hhcmQtMg%3D%3D_c04a1c71-d9a8-4d14-bfa3-993fe3e2e491



Below is a complete C# solution for Unity that parses and executes a Python-like scripting language, as per your requirements. The language supports indentation-based syntax, while loops, if-elif-else statements, variable assignments (integers, strings, booleans), and bitwise operations. It logs messages using `Debug.Log` when predefined methods like `move()`, `harvest()`, and `plant()` are executed. The script is designed to be used with a Unity `InputField` and `Button` for input and execution.

### Features
- **Indentation-based syntax**: Uses 4 spaces per indent level, similar to Python.
- **Control Structures**: Supports `while` loops and `if-elif-else` statements with boolean conditions, comparisons, and logical operators (`and`, `or`, `not`).
- **Variables**: Can store and manipulate integers, strings, and booleans.
- **Bitwise Operations**: Supports `&`, `|`, `^`, `~`, `<<`, `>>` on integers.
- **Predefined Functions**: Includes `move()`, `harvest()`, `plant()`, `is_sunny()` (returns boolean), and `print(value)` for logging.
- **Unity Integration**: Executes code from an `InputField` when a `Button` is clicked, with errors logged to the console.

### Example Behaviors
1. **While Loop**:
   ```python
   count = 3
   while count > 0:
       move()
       count = count - 1
   ```
   Logs "Move called" three times.

2. **If-Elif-Else**:
   ```python
   x = 5
   y = 3
   if x < y:
       print("x is less")
   elif x == y:
       print("x equals y")
   else:
       print("x is greater")
   ```
   Logs "x is greater".

3. **Variable Get/Set**:
   ```python
   flag = True
   message = "Hello"
   number = 42
   print(message)
   print(number)
   ```
   Logs "Hello" and "42".

4. **Bitwise Operation**:
   ```python
   a = 5  # 101 in binary
   b = 3  # 011 in binary
   c = a & b  # 001 in binary (1)
   print(c)
   ```
   Logs "1".

### Script
The solution is provided in a single C# file, `ScriptExecutor.cs`, which includes all necessary classes (`Token`, `Lexer`, `Statement`, `Expression`, `Parser`, `Interpreter`) and integrates with Unity.

```cs
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public enum TokenType {
    KEYWORD, IDENTIFIER, NUMBER, STRING, OPERATOR, PUNCTUATION, INDENT, DEDENT, EOF
}

public class Token {
    public TokenType Type;
    public string Value;
}

public class Lexer {
    private string code;
    private List<Token> tokens = new List<Token>();
    private int currentLine = 0;
    private Stack<int> indentStack = new Stack<int>();
    private int indentSize = 4;

    public Lexer(string code) {
        this.code = code;
        indentStack.Push(0);
    }

    public List<Token> Tokenize() {
        string[] lines = code.Split('\n');
        foreach (var line in lines) {
            currentLine++;
            ProcessLine(line);
        }
        while (indentStack.Count > 1) {
            tokens.Add(new Token { Type = TokenType.DEDENT, Value = "" });
            indentStack.Pop();
        }
        tokens.Add(new Token { Type = TokenType.EOF, Value = "" });
        return tokens;
    }

    private void ProcessLine(string line) {
        int indent = 0;
        while (indent < line.Length && line[indent] == ' ') indent++;
        int currentIndent = indent / indentSize;
        if (currentIndent > indentStack.Peek()) {
            indentStack.Push(currentIndent);
            tokens.Add(new Token { Type = TokenType.INDENT, Value = "" });
        } else if (currentIndent < indentStack.Peek()) {
            while (currentIndent < indentStack.Peek()) {
                indentStack.Pop();
                tokens.Add(new Token { Type = TokenType.DEDENT, Value = "" });
            }
            if (currentIndent != indentStack.Peek()) {
                throw new Exception($"Indentation error at line {currentLine}");
            }
        }
        string content = line.Trim();
        if (string.IsNullOrEmpty(content)) return;
        TokenizeContent(content);
    }

    private void TokenizeContent(string content) {
        string pattern = @"\s+|if|elif|else|while|True|False|and|or|not|[a-zA-Z_]\w*|\d+|""[^""]*""|==|!=|<=|>=|<<|>>|[\+\-\*/%&|^~]=?|:|\(|\)";
        var matches = Regex.Matches(content, pattern);
        foreach (Match match in matches) {
            string text = match.Value;
            if (string.IsNullOrWhiteSpace(text)) continue;
            if ("if elif else while True False and or not".Contains(text)) {
                tokens.Add(new Token { Type = TokenType.KEYWORD, Value = text });
            } else if (char.IsLetter(text[0]) || text[0] == '_') {
                tokens.Add(new Token { Type = TokenType.IDENTIFIER, Value = text });
            } else if (char.IsDigit(text[0])) {
                tokens.Add(new Token { Type = TokenType.NUMBER, Value = text });
            } else if (text.StartsWith("\"")) {
                tokens.Add(new Token { Type = TokenType.STRING, Value = text.Trim('"') });
            } else if ("== != <= >= < > << >> + - * / & | ^ ~".Contains(text)) {
                tokens.Add(new Token { Type = TokenType.OPERATOR, Value = text });
            } else if (":()".Contains(text)) {
                tokens.Add(new Token { Type = TokenType.PUNCTUATION, Value = text });
            } else {
                throw new Exception($"Unknown token: {text}");
            }
        }
    }
}

public abstract class Statement { }
public class AssignmentStatement : Statement {
    public string Variable;
    public Expression Value;
}
public class IfStatement : Statement {
    public Expression Condition;
    public List<Statement> ThenBlock;
    public List<Statement> ElseBlock;
}
public class WhileStatement : Statement {
    public Expression Condition;
    public List<Statement> Block;
}
public class ExpressionStatement : Statement {
    public Expression Expr;
}
public abstract class Expression { }
public class LiteralExpression : Expression {
    public object Value;
}
public class VariableExpression : Expression {
    public string Name;
}
public class BinaryExpression : Expression {
    public Expression Left;
    public string Operator;
    public Expression Right;
}
public class UnaryExpression : Expression {
    public string Operator;
    public Expression Operand;
}
public class FunctionCallExpression : Expression {
    public string FunctionName;
    public List<Expression> Arguments;
}

public class Parser {
    private List<Token> tokens;
    private int currentPos = 0;

    public Parser(List<Token> tokens) {
        this.tokens = tokens;
    }

    private Token Current => tokens[currentPos];
    private void Advance() => currentPos++;
    private void Expect(TokenType type, string value) {
        if (Current.Type == type && Current.Value == value) Advance();
        else throw new Exception($"Expected {type} '{value}', got {Current.Type} '{Current.Value}'");
    }

    public List<Statement> ParseProgram() {
        var statements = new List<Statement>();
        while (Current.Type != TokenType.EOF) statements.Add(ParseStatement());
        return statements;
    }

    private Statement ParseStatement() {
        if (Current.Type == TokenType.KEYWORD && Current.Value == "if") return ParseIfStatement();
        if (Current.Type == TokenType.KEYWORD && Current.Value == "while") return ParseWhileStatement();
        if (Current.Type == TokenType.IDENTIFIER) {
            if (currentPos + 1 < tokens.Count && tokens[currentPos + 1].Value == "=") return ParseAssignment();
            return ParseExpressionStatement();
        }
        throw new Exception($"Invalid statement at {Current.Value}");
    }

    private IfStatement ParseIfStatement() {
        Expect(TokenType.KEYWORD, "if");
        var condition = ParseExpression();
        Expect(TokenType.PUNCTUATION, ":");
        Expect(TokenType.INDENT);
        var thenBlock = ParseBlock();
        List<Statement> elseBlock = null;
        if (Current.Type == TokenType.KEYWORD && Current.Value == "elif") {
            Advance();
            Expect(TokenType.PUNCTUATION, ":");
            Expect(TokenType.INDENT);
            var elifStmt = ParseIfStatement();
            elseBlock = new List<Statement> { elifStmt };
        } else if (Current.Type == TokenType.KEYWORD && Current.Value == "else") {
            Advance();
            Expect(TokenType.PUNCTUATION, ":");
            Expect(TokenType.INDENT);
            elseBlock = ParseBlock();
        }
        return new IfStatement { Condition = condition, ThenBlock = thenBlock, ElseBlock = elseBlock };
    }

    private WhileStatement ParseWhileStatement() {
        Expect(TokenType.KEYWORD, "while");
        var condition = ParseExpression();
        Expect(TokenType.PUNCTUATION, ":");
        Expect(TokenType.INDENT);
        var block = ParseBlock();
        return new WhileStatement { Condition = condition, Block = block };
    }

    private AssignmentStatement ParseAssignment() {
        var variable = Current.Value;
        Advance();
        Expect(TokenType.OPERATOR, "=");
        var value = ParseExpression();
        return new AssignmentStatement { Variable = variable, Value = value };
    }

    private ExpressionStatement ParseExpressionStatement() {
        return new ExpressionStatement { Expr = ParseExpression() };
    }

    private List<Statement> ParseBlock() {
        var statements = new List<Statement>();
        while (Current.Type != TokenType.DEDENT && Current.Type != TokenType.EOF) {
            statements.Add(ParseStatement());
        }
        if (Current.Type == TokenType.DEDENT) Advance();
        return statements;
    }

    private Expression ParseExpression() => ParseOrExpression();

    private Expression ParseOrExpression() {
        var left = ParseAndExpression();
        while (Current.Type == TokenType.KEYWORD && Current.Value == "or") {
            var op = Current.Value;
            Advance();
            var right = ParseAndExpression();
            left = new BinaryExpression { Left = left, Operator = op, Right = right };
        }
        return left;
    }

    private Expression ParseAndExpression() {
        var left = ParseNotExpression();
        while (Current.Type == TokenType.KEYWORD && Current.Value == "and") {
            var op = Current.Value;
            Advance();
            var right = ParseNotExpression();
            left = new BinaryExpression { Left = left, Operator = op, Right = right };
        }
        return left;
    }

    private Expression ParseNotExpression() {
        if (Current.Type == TokenType.KEYWORD && Current.Value == "not") {
            Advance();
            var operand = ParseNotExpression();
            return new UnaryExpression { Operator = "not", Operand = operand };
        }
        return ParseComparison();
    }

    private Expression ParseComparison() {
        var left = ParseBitwiseOr();
        while (Current.Type == TokenType.OPERATOR && IsComparisonOperator(Current.Value)) {
            var op = Current.Value;
            Advance();
            var right = ParseBitwiseOr();
            left = new BinaryExpression { Left = left, Operator = op, Right = right };
        }
        return left;
    }

    private bool IsComparisonOperator(string op) => new[] { "==", "!=", "<", ">", "<=", ">=" }.Contains(op);

    private Expression ParseBitwiseOr() {
        var left = ParseBitwiseXor();
        while (Current.Type == TokenType.OPERATOR && Current.Value == "|") {
            var op = Current.Value;
            Advance();
            var right = ParseBitwiseXor();
            left = new BinaryExpression { Left = left, Operator = op, Right = right };
        }
        return left;
    }

    private Expression ParseBitwiseXor() {
        var left = ParseBitwiseAnd();
        while (Current.Type == TokenType.OPERATOR && Current.Value == "^") {
            var op = Current.Value;
            Advance();
            var right = ParseBitwiseAnd();
            left = new BinaryExpression { Left = left, Operator = op, Right = right };
        }
        return left;
    }

    private Expression ParseBitwiseAnd() {
        var left = ParseShift();
        while (Current.Type == TokenType.OPERATOR && Current.Value == "&") {
            var op = Current.Value;
            Advance();
            var right = ParseShift();
            left = new BinaryExpression { Left = left, Operator = op, Right = right };
        }
        return left;
    }

    private Expression ParseShift() {
        var left = ParseAdditive();
        while (Current.Type == TokenType.OPERATOR && (Current.Value == "<<" || Current.Value == ">>")) {
            var op = Current.Value;
            Advance();
            var right = ParseAdditive();
            left = new BinaryExpression { Left = left, Operator = op, Right = right };
        }
        return left;
    }

    private Expression ParseAdditive() {
        var left = ParseMultiplicative();
        while (Current.Type == TokenType.OPERATOR && (Current.Value == "+" || Current.Value == "-")) {
            var op = Current.Value;
            Advance();
            var right = ParseMultiplicative();
            left = new BinaryExpression { Left = left, Operator = op, Right = right };
        }
        return left;
    }

    private Expression ParseMultiplicative() {
        var left = ParseUnary();
        while (Current.Type == TokenType.OPERATOR && (Current.Value == "*" || Current.Value == "/")) {
            var op = Current.Value;
            Advance();
            var right = ParseUnary();
            left = new BinaryExpression { Left = left, Operator = op, Right = right };
        }
        return left;
    }

    private Expression ParseUnary() {
        if (Current.Type == TokenType.OPERATOR && (Current.Value == "-" || Current.Value == "~")) {
            var op = Current.Value;
            Advance();
            var operand = ParseUnary();
            return new UnaryExpression { Operator = op, Operand = operand };
        }
        return ParsePrimary();
    }

    private Expression ParsePrimary() {
        if (Current.Type == TokenType.NUMBER) {
            var value = int.Parse(Current.Value);
            Advance();
            return new LiteralExpression { Value = value };
        }
        if (Current.Type == TokenType.STRING) {
            var value = Current.Value;
            Advance();
            return new LiteralExpression { Value = value };
        }
        if (Current.Type == TokenType.KEYWORD && (Current.Value == "True" || Current.Value == "False")) {
            var value = Current.Value == "True";
            Advance();
            return new LiteralExpression { Value = value };
        }
        if (Current.Type == TokenType.IDENTIFIER) {
            var name = Current.Value;
            Advance();
            if (Current.Type == TokenType.PUNCTUATION && Current.Value == "(") {
                Advance();
                var args = new List<Expression>();
                if (Current.Type != TokenType.PUNCTUATION || Current.Value != ")") {
                    args.Add(ParseExpression());
                }
                Expect(TokenType.PUNCTUATION, ")");
                return new FunctionCallExpression { FunctionName = name, Arguments = args };
            }
            return new VariableExpression { Name = name };
        }
        if (Current.Type == TokenType.PUNCTUATION && Current.Value == "(") {
            Advance();
            var expr = ParseExpression();
            Expect(TokenType.PUNCTUATION, ")");
            return expr;
        }
        throw new Exception($"Invalid expression at {Current.Value}");
    }
}

public class Interpreter {
    private Dictionary<string, object> variables = new Dictionary<string, object>();
    private Dictionary<string, Func<object[], object>> functions;

    public Interpreter(Dictionary<string, Func<object[], object>> functions) {
        this.functions = functions;
    }

    public void Execute(List<Statement> program) {
               foreach (Statement stmt in program) ExecuteStatement(stmt);
    }

    private void ExecuteStatement(Statement stmt) {
        switch (stmt) {
            case AssignmentStatement assign:
                variables[assign.Variable] = EvaluateExpression(assign.Value);
                break;
            case IfStatement ifStmt:
                if ((bool)EvaluateExpression(ifStmt.Condition)) {
                    foreach (var s in ifStmt.ThenBlock) ExecuteStatement(s);
                } else if (ifStmt.ElseBlock != null) {
                    foreach (var s in ifStmt.ElseBlock) ExecuteStatement(s);
                }
                break;
            case WhileStatement whileStmt:
                while ((bool)EvaluateExpression(whileStmt.Condition)) {
                    foreach (var s in whileStmt.Block) ExecuteStatement(s);
                }
                break;
            case ExpressionStatement exprStmt:
                EvaluateExpression(exprStmt.Expr);
                break;
        }
    }

    private object EvaluateExpression(Expression expr) {
        switch (expr) {
            case LiteralExpression lit:
                return lit.Value;
            case VariableExpression varExpr:
                if (variables.TryGetValue(varExpr.Name, out var value)) return value;
                throw new Exception($"Undefined variable: {varExpr.Name}");
            case BinaryExpression binExpr:
                var left = EvaluateExpression(binExpr.Left);
                var right = EvaluateExpression(binExpr.Right);
                return EvaluateBinary(left, binExpr.Operator, right);
            case UnaryExpression unExpr:
                var operand = EvaluateExpression(unExpr.Operand);
                return EvaluateUnary(unExpr.Operator, operand);
            case FunctionCallExpression funcExpr:
                if (functions.TryGetValue(funcExpr.FunctionName, out var func)) {
                    var args = funcExpr.Arguments.Select(EvaluateExpression).ToArray();
                    return func(args);
                }
                throw new Exception($"Unknown function: {funcExpr.FunctionName}");
            default:
                throw new Exception("Invalid expression");
        }
    }

    private object EvaluateBinary(object left, string op, object right) {
        if (left is int li && right is int ri) {
            switch (op) {
                case "+": return li + ri;
                case "-": return li - ri;
                case "*": return li * ri;
                case "/": return li / ri;
                case "&": return li & ri;
                case "|": return li | ri;
                case "^": return li ^ ri;
                case "<<": return li << ri;
                case ">>": return li >> ri;
                case "==": return li == ri;
                case "!=": return li != ri;
                case "<": return li < ri;
                case ">": return li > ri;
                case "<=": return li <= ri;
                case ">=": return li >= ri;
                default: throw new Exception($"Invalid operator {op} for integers");
            }
        } else if (left is string ls && right is string rs) {
            switch (op) {
                case "+": return ls + rs;
                case "==": return ls == rs;
                case "!=": return ls != rs;
                case "<": return string.Compare(ls, rs) < 0;
                case ">": return string.Compare(ls, rs) > 0;
                case "<=": return string.Compare(ls, rs) <= 0;
                case ">=": return string.Compare(ls, rs) >= 0;
                default: throw new Exception($"Invalid operator {op} for strings");
            }
        } else if (left is bool lb && right is bool rb) {
            switch (op) {
                case "and": return lb && rb;
                case "or": return lb || rb;
                default: throw new Exception($"Invalid operator {op} for booleans");
            }
        }
        throw new Exception($"Type mismatch for operator {op}");
    }

    private object EvaluateUnary(string op, object operand) {
        if (op == "-") {
            if (operand is int i) return -i;
            throw new Exception("Unary - expects an integer");
        } else if (op == "~") {
            if (operand is int i) return ~i;
            throw new Exception("Unary ~ expects an integer");
        } else if (op == "not") {
            if (operand is bool b) return !b;
            throw new Exception("not expects a boolean");
        }
        throw new Exception($"Unknown unary operator {op}");
    }
}

public class ScriptExecutor : MonoBehaviour {
    public InputField codeInput;
    public Button executeButton;

    private Dictionary<string, Func<object[], object>> functions;

    void Start() {
        functions = new Dictionary<string, Func<object[], object>> {
            {"move", args => {
                if (args.Length != 0) throw new Exception("move expects no arguments");
                Debug.Log("Move called");
                return null;
            }},
            {"harvest", args => {
                if (args.Length != 0) throw new Exception("harvest expects no arguments");
                Debug.Log("Harvest called");
                return null;
            }},
            {"plant", args => {
                if (args.Length != 0) throw new Exception("plant expects no arguments");
                Debug.Log("Plant called");
                return null;
            }},
            {"is_sunny", args => {
                if (args.Length != 0) throw new Exception("is_sunny expects no arguments");
                return true;
            }},
            {"print", args => {
                if (args.Length != 1) throw new Exception("print expects one argument");
                Debug.Log(args[0].ToString());
                return null;
            }}
        };

        executeButton.onClick.AddListener(ExecuteCode);
    }

    void ExecuteCode() {
        string code = codeInput.text;
        try {
            var lexer = new Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens);
            var program = parser.ParseProgram();
            var interpreter = new Interpreter(functions);
            interpreter.Execute(program);
        } catch (Exception e) {
            Debug.LogError($"Error: {e.Message}");
        }
    }
}
```

### Setup Instructions
1. **Create a Unity Scene**:
   - Add a UI `InputField` and a `Button` to your scene via `GameObject > UI > Input Field` and `GameObject > UI > Button`.
2. **Attach the Script**:
   - Create a new script named `ScriptExecutor.cs` in your Unity project.
   - Copy the code above into `ScriptExecutor.cs`.
   - Attach the script to a GameObject (e.g., an empty GameObject).
3. **Assign References**:
   - In the Inspector, drag the `InputField` and `Button` GameObjects into the corresponding fields of the `ScriptExecutor` component.
4. **Test the Script**:
   - Enter any of the example codes above into the `InputField` in Play mode.
   - Click the button to execute the code and observe the output in the Unity Console.

This implementation provides a robust foundation for a Python-like scripting language in Unity, with room for future enhancements like additional operators or function arguments.