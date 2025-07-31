https://claude.ai/share/e0453487-aa06-467a-960c-0a6620decd6b

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using SPACE_UTIL;

public class PythonStyleParser : MonoBehaviour
{
	private Dictionary<string, object> variables = new Dictionary<string, object>();
	private Dictionary<string, Func<object[], object>> methods = new Dictionary<string, Func<object[], object>>();

	[TextArea(minLines: 40, maxLines:50)]
	[SerializeField] string pythonCode = @"
x = 5
y = 10
name = ""Unity""
is_active = True

if x < y:
    Debug.Log(""x is less than y"")
    z = x + y
    Debug.Log(z)
elif x == y:
    Debug.Log(""x equals y"")
else:
    Debug.Log(""x is greater than y"")

counter = 0
while counter < 3:
    Debug.Log(counter)
    counter = counter + 1
    if counter == 2:
        Debug.Log(""Counter reached 2"")

# Bitwise operations
bit_result = x & y
Debug.Log(bit_result)
bit_result = x | y
Debug.Log(bit_result)
bit_result = x ^ y
Debug.Log(bit_result)

# Boolean operations
result = x < y and is_active
Debug.Log(result)
result = x > y or name == ""Unity""
Debug.Log(result)
result = not is_active
Debug.Log(result)
";

	private void Start()
	{
		// Initialize built-in methods
		InitializeMethods();
	}

	private void Update()
	{
		// Example Usage
		if (INPUT.M.InstantDown(0))
			ExecuteCode(this.pythonCode);
	}

	private void InitializeMethods()
	{
		// Debug.Log method
		methods["Debug.Log"] = args =>
		{
			if (args.Length > 0)
			{
				Debug.Log(args[0]?.ToString() ?? "null");
			}
			return null;
		};

		// Add more built-in methods as needed
		methods["print"] = args =>
		{
			if (args.Length > 0)
			{
				Debug.Log(args[0]?.ToString() ?? "null");
			}
			return null;
		};
	}

	public void ExecuteCode(string code)
	{
		try
		{
			var tokens = Tokenize(code);
			var statements = Parse(tokens);
			Execute(statements);
		}
		catch (Exception e)
		{
			Debug.LogError($"Execution error: {e.Message}");
		}
	}

	private List<Token> Tokenize(string code)
	{
		var tokens = new List<Token>();
		var lines = code.Split('\n');

		for (int lineNum = 0; lineNum < lines.Length; lineNum++)
		{
			string line = lines[lineNum];
			if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
				continue;

			int indent = 0;
			while (indent < line.Length && line[indent] == ' ')
				indent++;

			tokens.Add(new Token(TokenType.Indent, indent.ToString(), lineNum));

			string trimmedLine = line.Trim();
			TokenizeLine(trimmedLine, tokens, lineNum);
			tokens.Add(new Token(TokenType.Newline, "\n", lineNum));
		}

		return tokens;
	}

	private void TokenizeLine(string line, List<Token> tokens, int lineNum)
	{
		var patterns = new Dictionary<TokenType, string>
		{
			{ TokenType.Number, @"\d+(\.\d+)?" },
			{ TokenType.String, @"""[^""]*""|'[^']*'" },
			{ TokenType.Boolean, @"\b(True|False)\b" },
			{ TokenType.Keyword, @"\b(if|elif|else|while|and|or|not)\b" },
			{ TokenType.Identifier, @"\b[a-zA-Z_][a-zA-Z0-9_]*(\.[a-zA-Z_][a-zA-Z0-9_]*)?\b" },
			{ TokenType.Operator, @"==|!=|<=|>=|<<|>>|&|\||\^|~|\+|-|\*|/|%|<|>|=" },
			{ TokenType.Colon, @":" },
			{ TokenType.LeftParen, @"\(" },
			{ TokenType.RightParen, @"\)" },
			{ TokenType.Comma, @"," }
		};

		int pos = 0;
		while (pos < line.Length)
		{
			if (char.IsWhiteSpace(line[pos]))
			{
				pos++;
				continue;
			}

			bool matched = false;
			foreach (var pattern in patterns)
			{
				var regex = new Regex($"^{pattern.Value}");
				var match = regex.Match(line.Substring(pos));

				if (match.Success)
				{
					tokens.Add(new Token(pattern.Key, match.Value, lineNum));
					pos += match.Length;
					matched = true;
					break;
				}
			}

			if (!matched)
			{
				pos++; // Skip unknown character
			}
		}
	}

	private List<Statement> Parse(List<Token> tokens)
	{
		var statements = new List<Statement>();
		int pos = 0;

		while (pos < tokens.Count)
		{
			if (tokens[pos].Type == TokenType.Newline)
			{
				pos++;
				continue;
			}

			var stmt = ParseStatement(tokens, ref pos);
			if (stmt != null)
				statements.Add(stmt);
		}

		return statements;
	}

	private Statement ParseStatement(List<Token> tokens, ref int pos)
	{
		if (pos >= tokens.Count) return null;

		int indent = 0;
		if (tokens[pos].Type == TokenType.Indent)
		{
			indent = int.Parse(tokens[pos].Value);
			pos++;
		}

		if (pos >= tokens.Count) return null;

		if (tokens[pos].Type == TokenType.Keyword)
		{
			string keyword = tokens[pos].Value;
			pos++;

			switch (keyword)
			{
				case "if":
				case "elif":
					return ParseIfStatement(tokens, ref pos, indent, keyword);
				case "else":
					return ParseElseStatement(tokens, ref pos, indent);
				case "while":
					return ParseWhileStatement(tokens, ref pos, indent);
			}
		}
		else if (tokens[pos].Type == TokenType.Identifier)
		{
			// Check if it's an assignment or method call
			string identifier = tokens[pos].Value;
			pos++;

			if (pos < tokens.Count && tokens[pos].Type == TokenType.Operator && tokens[pos].Value == "=")
			{
				pos++; // Skip '='
				var expr = ParseExpression(tokens, ref pos);
				SkipToNewline(tokens, ref pos);
				return new AssignmentStatement { Identifier = identifier, Expression = expr, Indent = indent };
			}
			else if (pos < tokens.Count && tokens[pos].Type == TokenType.LeftParen)
			{
				pos--; // Go back to identifier
				var expr = ParseExpression(tokens, ref pos);
				SkipToNewline(tokens, ref pos);
				return new ExpressionStatement { Expression = expr, Indent = indent };
			}
		}

		SkipToNewline(tokens, ref pos);
		return null;
	}

	private Statement ParseIfStatement(List<Token> tokens, ref int pos, int indent, string type)
	{
		var condition = ParseExpression(tokens, ref pos);

		if (pos < tokens.Count && tokens[pos].Type == TokenType.Colon)
			pos++;

		SkipToNewline(tokens, ref pos);

		var body = ParseBlock(tokens, ref pos, indent + 4);

		return new IfStatement { Type = type, Condition = condition, Body = body, Indent = indent };
	}

	private Statement ParseElseStatement(List<Token> tokens, ref int pos, int indent)
	{
		if (pos < tokens.Count && tokens[pos].Type == TokenType.Colon)
			pos++;

		SkipToNewline(tokens, ref pos);

		var body = ParseBlock(tokens, ref pos, indent + 4);

		return new ElseStatement { Body = body, Indent = indent };
	}

	private Statement ParseWhileStatement(List<Token> tokens, ref int pos, int indent)
	{
		var condition = ParseExpression(tokens, ref pos);

		if (pos < tokens.Count && tokens[pos].Type == TokenType.Colon)
			pos++;

		SkipToNewline(tokens, ref pos);

		var body = ParseBlock(tokens, ref pos, indent + 4);

		return new WhileStatement { Condition = condition, Body = body, Indent = indent };
	}

	private List<Statement> ParseBlock(List<Token> tokens, ref int pos, int expectedIndent)
	{
		var statements = new List<Statement>();

		while (pos < tokens.Count)
		{
			if (tokens[pos].Type == TokenType.Newline)
			{
				pos++;
				continue;
			}

			if (tokens[pos].Type == TokenType.Indent)
			{
				int currentIndent = int.Parse(tokens[pos].Value);
				if (currentIndent < expectedIndent)
					break; // End of block

				var stmt = ParseStatement(tokens, ref pos);
				if (stmt != null)
					statements.Add(stmt);
			}
			else
			{
				break; // End of block
			}
		}

		return statements;
	}

	private Expression ParseExpression(List<Token> tokens, ref int pos)
	{
		return ParseOrExpression(tokens, ref pos);
	}

	private Expression ParseOrExpression(List<Token> tokens, ref int pos)
	{
		var left = ParseAndExpression(tokens, ref pos);

		while (pos < tokens.Count && tokens[pos].Type == TokenType.Keyword && tokens[pos].Value == "or")
		{
			pos++;
			var right = ParseAndExpression(tokens, ref pos);
			left = new BinaryExpression { Left = left, Operator = "or", Right = right };
		}

		return left;
	}

	private Expression ParseAndExpression(List<Token> tokens, ref int pos)
	{
		var left = ParseNotExpression(tokens, ref pos);

		while (pos < tokens.Count && tokens[pos].Type == TokenType.Keyword && tokens[pos].Value == "and")
		{
			pos++;
			var right = ParseNotExpression(tokens, ref pos);
			left = new BinaryExpression { Left = left, Operator = "and", Right = right };
		}

		return left;
	}

	private Expression ParseNotExpression(List<Token> tokens, ref int pos)
	{
		if (pos < tokens.Count && tokens[pos].Type == TokenType.Keyword && tokens[pos].Value == "not")
		{
			pos++;
			var expr = ParseComparisonExpression(tokens, ref pos);
			return new UnaryExpression { Operator = "not", Expression = expr };
		}

		return ParseComparisonExpression(tokens, ref pos);
	}

	private Expression ParseComparisonExpression(List<Token> tokens, ref int pos)
	{
		var left = ParseBitwiseExpression(tokens, ref pos);

		while (pos < tokens.Count && tokens[pos].Type == TokenType.Operator &&
			   (tokens[pos].Value == "==" || tokens[pos].Value == "!=" ||
				tokens[pos].Value == "<" || tokens[pos].Value == ">" ||
				tokens[pos].Value == "<=" || tokens[pos].Value == ">="))
		{
			string op = tokens[pos].Value;
			pos++;
			var right = ParseBitwiseExpression(tokens, ref pos);
			left = new BinaryExpression { Left = left, Operator = op, Right = right };
		}

		return left;
	}

	private Expression ParseBitwiseExpression(List<Token> tokens, ref int pos)
	{
		var left = ParseArithmeticExpression(tokens, ref pos);

		while (pos < tokens.Count && tokens[pos].Type == TokenType.Operator &&
			   (tokens[pos].Value == "&" || tokens[pos].Value == "|" || tokens[pos].Value == "^"))
		{
			string op = tokens[pos].Value;
			pos++;
			var right = ParseArithmeticExpression(tokens, ref pos);
			left = new BinaryExpression { Left = left, Operator = op, Right = right };
		}

		return left;
	}

	private Expression ParseArithmeticExpression(List<Token> tokens, ref int pos)
	{
		var left = ParsePrimaryExpression(tokens, ref pos);

		while (pos < tokens.Count && tokens[pos].Type == TokenType.Operator &&
			   (tokens[pos].Value == "+" || tokens[pos].Value == "-" ||
				tokens[pos].Value == "*" || tokens[pos].Value == "/" || tokens[pos].Value == "%"))
		{
			string op = tokens[pos].Value;
			pos++;
			var right = ParsePrimaryExpression(tokens, ref pos);
			left = new BinaryExpression { Left = left, Operator = op, Right = right };
		}

		return left;
	}

	private Expression ParsePrimaryExpression(List<Token> tokens, ref int pos)
	{
		if (pos >= tokens.Count) return null;

		var token = tokens[pos];

		switch (token.Type)
		{
			case TokenType.Number:
				pos++;
				return new LiteralExpression { Value = ParseNumber(token.Value) };

			case TokenType.String:
				pos++;
				return new LiteralExpression { Value = token.Value.Substring(1, token.Value.Length - 2) };

			case TokenType.Boolean:
				pos++;
				return new LiteralExpression { Value = token.Value == "True" };

			case TokenType.Identifier:
				pos++;

				// Check for method call
				if (pos < tokens.Count && tokens[pos].Type == TokenType.LeftParen)
				{
					pos++; // Skip '('
					var args = new List<Expression>();

					while (pos < tokens.Count && tokens[pos].Type != TokenType.RightParen)
					{
						args.Add(ParseExpression(tokens, ref pos));

						if (pos < tokens.Count && tokens[pos].Type == TokenType.Comma)
							pos++;
					}

					if (pos < tokens.Count && tokens[pos].Type == TokenType.RightParen)
						pos++;

					return new MethodCallExpression { MethodName = token.Value, Arguments = args };
				}
				else
				{
					return new VariableExpression { Name = token.Value };
				}

			case TokenType.LeftParen:
				pos++; // Skip '('
				var expr = ParseExpression(tokens, ref pos);
				if (pos < tokens.Count && tokens[pos].Type == TokenType.RightParen)
					pos++;
				return expr;

			default:
				return null;
		}
	}

	private object ParseNumber(string value)
	{
		if (value.Contains("."))
			return float.Parse(value);
		else
			return int.Parse(value);
	}

	private void SkipToNewline(List<Token> tokens, ref int pos)
	{
		while (pos < tokens.Count && tokens[pos].Type != TokenType.Newline)
			pos++;
	}

	private void Execute(List<Statement> statements)
	{
		var context = new ExecutionContext();
		foreach (var statement in statements)
		{
			ExecuteStatement(statement, context);
		}
	}

	private void ExecuteStatement(Statement statement, ExecutionContext context)
	{
		switch (statement)
		{
			case AssignmentStatement assign:
				var value = EvaluateExpression(assign.Expression, context);
				variables[assign.Identifier] = value;
				Debug.Log($"Assigned {assign.Identifier} = {value}");
				break;

			case ExpressionStatement exprStmt:
				EvaluateExpression(exprStmt.Expression, context);
				break;

			case IfStatement ifStmt:
				var condition = EvaluateExpression(ifStmt.Condition, context);
				if (IsTruthy(condition))
				{
					Debug.Log($"Executing {ifStmt.Type} block");
					foreach (var stmt in ifStmt.Body)
						ExecuteStatement(stmt, context);
				}
				break;

			case ElseStatement elseStmt:
				Debug.Log("Executing else block");
				foreach (var stmt in elseStmt.Body)
					ExecuteStatement(stmt, context);
				break;

			case WhileStatement whileStmt:
				Debug.Log("Starting while loop");
				while (IsTruthy(EvaluateExpression(whileStmt.Condition, context)))
				{
					foreach (var stmt in whileStmt.Body)
						ExecuteStatement(stmt, context);
				}
				Debug.Log("While loop ended");
				break;
		}
	}

	private object EvaluateExpression(Expression expression, ExecutionContext context)
	{
		switch (expression)
		{
			case LiteralExpression literal:
				return literal.Value;

			case VariableExpression variable:
				return variables.ContainsKey(variable.Name) ? variables[variable.Name] : null;

			case BinaryExpression binary:
				var left = EvaluateExpression(binary.Left, context);
				var right = EvaluateExpression(binary.Right, context);
				return EvaluateBinaryOperation(left, binary.Operator, right);

			case UnaryExpression unary:
				var operand = EvaluateExpression(unary.Expression, context);
				return EvaluateUnaryOperation(unary.Operator, operand);

			case MethodCallExpression methodCall:
				var args = methodCall.Arguments.Select(arg => EvaluateExpression(arg, context)).ToArray();
				if (methods.ContainsKey(methodCall.MethodName))
				{
					Debug.Log($"Executing method: {methodCall.MethodName}");
					return methods[methodCall.MethodName](args);
				}
				break;
		}

		return null;
	}

	private object EvaluateBinaryOperation(object left, string op, object right)
	{
		switch (op)
		{
			case "+":
				if (left is int l1 && right is int r1) return l1 + r1;
				if (left is float l2 && right is float r2) return l2 + r2;
				if (left is string || right is string) return left?.ToString() + right?.ToString();
				break;

			case "-":
				if (left is int l3 && right is int r3) return l3 - r3;
				if (left is float l4 && right is float r4) return l4 - r4;
				break;

			case "*":
				if (left is int l5 && right is int r5) return l5 * r5;
				if (left is float l6 && right is float r6) return l6 * r6;
				break;

			case "/":
				if (left is int l7 && right is int r7) return r7 != 0 ? l7 / r7 : 0;
				if (left is float l8 && right is float r8) return r8 != 0 ? l8 / r8 : 0;
				break;

			case "%":
				if (left is int l9 && right is int r9) return r9 != 0 ? l9 % r9 : 0;
				break;

			case "&":
				if (left is int l10 && right is int r10) return l10 & r10;
				break;

			case "|":
				if (left is int l11 && right is int r11) return l11 | r11;
				break;

			case "^":
				if (left is int l12 && right is int r12) return l12 ^ r12;
				break;

			case "==":
				return Equals(left, right);

			case "!=":
				return !Equals(left, right);

			case "<":
				if (left is int l13 && right is int r13) return l13 < r13;
				if (left is float l14 && right is float r14) return l14 < r14;
				break;

			case ">":
				if (left is int l15 && right is int r15) return l15 > r15;
				if (left is float l16 && right is float r16) return l16 > r16;
				break;

			case "<=":
				if (left is int l17 && right is int r17) return l17 <= r17;
				if (left is float l18 && right is float r18) return l18 <= r18;
				break;

			case ">=":
				if (left is int l19 && right is int r19) return l19 >= r19;
				if (left is float l20 && right is float r20) return l20 >= r20;
				break;

			case "and":
				return IsTruthy(left) && IsTruthy(right);

			case "or":
				return IsTruthy(left) || IsTruthy(right);
		}

		return null;
	}

	private object EvaluateUnaryOperation(string op, object operand)
	{
		switch (op)
		{
			case "not":
				return !IsTruthy(operand);
		}

		return null;
	}

	private bool IsTruthy(object value)
	{
		if (value == null) return false;
		if (value is bool b) return b;
		if (value is int i) return i != 0;
		if (value is float f) return f != 0f;
		if (value is string s) return !string.IsNullOrEmpty(s);
		return true;
	}
}

// Token classes
public enum TokenType
{
	Identifier, Number, String, Boolean, Keyword, Operator,
	LeftParen, RightParen, Colon, Comma, Indent, Newline
}

public class Token
{
	public TokenType Type { get; set; }
	public string Value { get; set; }
	public int Line { get; set; }

	public Token(TokenType type, string value, int line)
	{
		Type = type;
		Value = value;
		Line = line;
	}
}

// AST classes
public abstract class Statement
{
	public int Indent { get; set; }
}

public abstract class Expression { }

public class AssignmentStatement : Statement
{
	public string Identifier { get; set; }
	public Expression Expression { get; set; }
}

public class ExpressionStatement : Statement
{
	public Expression Expression { get; set; }
}

public class IfStatement : Statement
{
	public string Type { get; set; } // "if" or "elif"
	public Expression Condition { get; set; }
	public List<Statement> Body { get; set; }
}

public class ElseStatement : Statement
{
	public List<Statement> Body { get; set; }
}

public class WhileStatement : Statement
{
	public Expression Condition { get; set; }
	public List<Statement> Body { get; set; }
}

public class LiteralExpression : Expression
{
	public object Value { get; set; }
}

public class VariableExpression : Expression
{
	public string Name { get; set; }
}

public class BinaryExpression : Expression
{
	public Expression Left { get; set; }
	public string Operator { get; set; }
	public Expression Right { get; set; }
}

public class UnaryExpression : Expression
{
	public string Operator { get; set; }
	public Expression Expression { get; set; }
}

public class MethodCallExpression : Expression
{
	public string MethodName { get; set; }
	public List<Expression> Arguments { get; set; }
}

public class ExecutionContext
{
	// Additional context for execution if needed
}
```