using System;
using System.Collections.Generic;

namespace PythonInterpreter
{
    #region Base Classes
    /// <summary>
    /// Base class for all AST nodes
    /// </summary>
    public abstract class AstNode
    {
        public int LineNumber { get; set; }
    }

    /// <summary>
    /// Base class for all statements
    /// </summary>
    public abstract class Stmt : AstNode
    {
    }

    /// <summary>
    /// Base class for all expressions
    /// </summary>
    public abstract class Expr : AstNode
    {
    }
    #endregion

    #region Statements
    /// <summary>
    /// Expression statement (standalone expression)
    /// </summary>
    public class ExprStmt : Stmt
    {
        public Expr Expression { get; set; }

        public ExprStmt(Expr expression)
        {
            Expression = expression;
        }
    }

    /// <summary>
    /// Assignment statement
    /// </summary>
    public class AssignStmt : Stmt
    {
        public Expr Target { get; set; }
        public Expr Value { get; set; }

        public AssignStmt(Expr target, Expr value)
        {
            Target = target;
            Value = value;
        }
    }

    /// <summary>
    /// If statement with optional elif and else branches
    /// </summary>
    public class IfStmt : Stmt
    {
        public Expr Condition { get; set; }
        public List<Stmt> ThenBranch { get; set; }
        public List<Tuple<Expr, List<Stmt>>> ElifBranches { get; set; } // (condition, body)
        public List<Stmt> ElseBranch { get; set; }

        public IfStmt(Expr condition, List<Stmt> thenBranch, List<Tuple<Expr, List<Stmt>>> elifBranches, List<Stmt> elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElifBranches = elifBranches ?? new List<Tuple<Expr, List<Stmt>>>();
            ElseBranch = elseBranch;
        }
    }

    /// <summary>
    /// While loop statement
    /// </summary>
    public class WhileStmt : Stmt
    {
        public Expr Condition { get; set; }
        public List<Stmt> Body { get; set; }

        public WhileStmt(Expr condition, List<Stmt> body)
        {
            Condition = condition;
            Body = body;
        }
    }

    /// <summary>
    /// For loop statement
    /// </summary>
    public class ForStmt : Stmt
    {
        public string Variable { get; set; }
        public Expr Iterable { get; set; }
        public List<Stmt> Body { get; set; }

        public ForStmt(string variable, Expr iterable, List<Stmt> body)
        {
            Variable = variable;
            Iterable = iterable;
            Body = body;
        }
    }

    /// <summary>
    /// Function definition statement
    /// </summary>
    public class FunctionDef : Stmt
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public List<Stmt> Body { get; set; }

        public FunctionDef(string name, List<string> parameters, List<Stmt> body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }

    /// <summary>
    /// Class definition statement
    /// </summary>
    public class ClassDef : Stmt
    {
        public string Name { get; set; }
        public List<FunctionDef> Methods { get; set; }

        public ClassDef(string name, List<FunctionDef> methods)
        {
            Name = name;
            Methods = methods;
        }
    }

    /// <summary>
    /// Return statement
    /// </summary>
    public class ReturnStmt : Stmt
    {
        public Expr Value { get; set; }

        public ReturnStmt(Expr value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Break statement
    /// </summary>
    public class BreakStmt : Stmt
    {
    }

    /// <summary>
    /// Continue statement
    /// </summary>
    public class ContinueStmt : Stmt
    {
    }

    /// <summary>
    /// Pass statement (no-op)
    /// </summary>
    public class PassStmt : Stmt
    {
    }

    /// <summary>
    /// Global declaration statement
    /// </summary>
    public class GlobalStmt : Stmt
    {
        public List<string> Names { get; set; }

        public GlobalStmt(List<string> names)
        {
            Names = names;
        }
    }
    #endregion

    #region Expressions
    /// <summary>
    /// Binary expression (e.g., a + b, a == b)
    /// </summary>
    public class BinaryExpr : Expr
    {
        public Expr Left { get; set; }
        public TokenType Operator { get; set; }
        public Expr Right { get; set; }

        public BinaryExpr(Expr left, TokenType op, Expr right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    /// <summary>
    /// Unary expression (e.g., -x, not x)
    /// </summary>
    public class UnaryExpr : Expr
    {
        public TokenType Operator { get; set; }
        public Expr Right { get; set; }

        public UnaryExpr(TokenType op, Expr right)
        {
            Operator = op;
            Right = right;
        }
    }

    /// <summary>
    /// Literal expression (numbers, strings, booleans, None)
    /// </summary>
    public class LiteralExpr : Expr
    {
        public object Value { get; set; }

        public LiteralExpr(object value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Variable expression
    /// </summary>
    public class VariableExpr : Expr
    {
        public string Name { get; set; }

        public VariableExpr(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Function call expression
    /// </summary>
    public class CallExpr : Expr
    {
        public Expr Callee { get; set; }
        public List<Expr> Arguments { get; set; }

        public CallExpr(Expr callee, List<Expr> arguments)
        {
            Callee = callee;
            Arguments = arguments;
        }
    }

    /// <summary>
    /// Property get expression (e.g., obj.property)
    /// </summary>
    public class GetExpr : Expr
    {
        public Expr Object { get; set; }
        public string Name { get; set; }

        public GetExpr(Expr obj, string name)
        {
            Object = obj;
            Name = name;
        }
    }

    /// <summary>
    /// Property set expression (e.g., obj.property = value)
    /// </summary>
    public class SetExpr : Expr
    {
        public Expr Object { get; set; }
        public string Name { get; set; }
        public Expr Value { get; set; }

        public SetExpr(Expr obj, string name, Expr value)
        {
            Object = obj;
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// Index/slice expression (e.g., list[0], list[1:3])
    /// </summary>
    public class SliceExpr : Expr
    {
        public Expr Object { get; set; }
        public Expr Index { get; set; }        // Single index or start of slice
        public Expr EndIndex { get; set; }     // End of slice (null for single index)
        public bool IsSlice { get; set; }      // True if this is a slice operation

        public SliceExpr(Expr obj, Expr index, Expr endIndex = null)
        {
            Object = obj;
            Index = index;
            EndIndex = endIndex;
            IsSlice = endIndex != null;
        }
    }

    /// <summary>
    /// List expression
    /// </summary>
    public class ListExpr : Expr
    {
        public List<Expr> Elements { get; set; }

        public ListExpr(List<Expr> elements)
        {
            Elements = elements;
        }
    }

    /// <summary>
    /// Dictionary expression
    /// </summary>
    public class DictExpr : Expr
    {
        public List<Tuple<Expr, Expr>> Pairs { get; set; } // (key, value)

        public DictExpr(List<Tuple<Expr, Expr>> pairs)
        {
            Pairs = pairs;
        }
    }

    /// <summary>
    /// Self expression (used in class methods)
    /// </summary>
    public class SelfExpr : Expr
    {
    }

    /// <summary>
    /// List comprehension expression
    /// [expr for var in iterable if condition]
    /// </summary>
    public class ListCompExpr : Expr
    {
        public Expr Element { get; set; }
        public string Variable { get; set; }
        public Expr Iterable { get; set; }
        public Expr Condition { get; set; } // Optional

        public ListCompExpr(Expr element, string variable, Expr iterable, Expr condition = null)
        {
            Element = element;
            Variable = variable;
            Iterable = iterable;
            Condition = condition;
        }
    }

    /// <summary>
    /// Lambda expression
    /// lambda args: expr
    /// </summary>
    public class LambdaExpr : Expr
    {
        public List<string> Parameters { get; set; }
        public Expr Body { get; set; }

        public LambdaExpr(List<string> parameters, Expr body)
        {
            Parameters = parameters;
            Body = body;
        }
    }
    #endregion
}
