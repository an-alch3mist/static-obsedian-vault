using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PythonInterpreter
{
    /// <summary>
    /// Coroutine-based Python interpreter for Unity
    /// Executes Python-like code with time-slicing to prevent frame drops
    /// </summary>
    public class PythonInterpreter
    {
        #region Private Fields
        private Dictionary<string, object> globalScope;
        private Stack<Dictionary<string, object>> scopeStack;
        private Dictionary<string, PythonFunction> functions;
        private Dictionary<string, PythonClass> classes;
        
        private int operationCount;
        private const int MAX_OPERATIONS_PER_FRAME = 250;
        
        private int currentLine;
        private bool shouldBreak;
        private bool shouldContinue;
        
        // Public for access by PythonFunction
        public object returnValue;
        public bool hasReturnValue;
        
        // For tracking global declarations
        private HashSet<string> globalVariables;
        #endregion

        #region Constructor
        public PythonInterpreter()
        {
            Reset();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Resets the interpreter state
        /// </summary>
        public void Reset()
        {
            globalScope = new Dictionary<string, object>();
            scopeStack = new Stack<Dictionary<string, object>>();
            functions = new Dictionary<string, PythonFunction>();
            classes = new Dictionary<string, PythonClass>();
            operationCount = 0;
            currentLine = 0;
            shouldBreak = false;
            shouldContinue = false;
            returnValue = null;
            hasReturnValue = false;
            globalVariables = new HashSet<string>();
        }

        /// <summary>
        /// Registers a built-in function
        /// </summary>
        public void RegisterBuiltin(string name, Func<List<object>, object> implementation)
        {
            globalScope[name] = new BuiltinFunction(name, implementation);
        }

        /// <summary>
        /// Executes a list of statements as a coroutine
        /// </summary>
        public IEnumerator Execute(List<Stmt> statements)
        {
            operationCount = 0;

            foreach (Stmt stmt in statements)
            {
                // Execute statement and handle yielding
                IEnumerator stmtCoroutine = ExecuteStatement(stmt);
                if (stmtCoroutine != null)
                {
                    while (stmtCoroutine.MoveNext())
                    {
                        yield return stmtCoroutine.Current;
                    }
                }

                // Check for return at top level
                if (hasReturnValue)
                {
                    hasReturnValue = false;
                    yield break;
                }
            }
        }
        #endregion

        #region Public Methods - Statement Execution (For Internal Use)
        public IEnumerator ExecuteStatement(Stmt stmt)
        {
            currentLine = stmt.LineNumber;
            operationCount++;

            // Time slicing - yield if budget exceeded
            if (operationCount >= MAX_OPERATIONS_PER_FRAME)
            {
                operationCount = 0;
                yield return null;
            }

            // Dispatch based on statement type
            if (stmt is ExprStmt)
            {
                ExprStmt exprStmt = (ExprStmt)stmt;
                IEnumerator coroutine = EvaluateExpression(exprStmt.Expression);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (stmt is AssignStmt)
            {
                IEnumerator coroutine = ExecuteAssignment((AssignStmt)stmt);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (stmt is IfStmt)
            {
                IEnumerator coroutine = ExecuteIf((IfStmt)stmt);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (stmt is WhileStmt)
            {
                IEnumerator coroutine = ExecuteWhile((WhileStmt)stmt);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (stmt is ForStmt)
            {
                IEnumerator coroutine = ExecuteFor((ForStmt)stmt);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (stmt is FunctionDef)
            {
                ExecuteFunctionDef((FunctionDef)stmt);
            }
            else if (stmt is ClassDef)
            {
                ExecuteClassDef((ClassDef)stmt);
            }
            else if (stmt is ReturnStmt)
            {
                IEnumerator coroutine = ExecuteReturn((ReturnStmt)stmt);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (stmt is BreakStmt)
            {
                shouldBreak = true;
            }
            else if (stmt is ContinueStmt)
            {
                shouldContinue = true;
            }
            else if (stmt is GlobalStmt)
            {
                ExecuteGlobal((GlobalStmt)stmt);
            }
            else if (stmt is PassStmt)
            {
                // No-op
            }
        }

        private IEnumerator ExecuteAssignment(AssignStmt stmt)
        {
            currentLine = stmt.LineNumber;

            // Evaluate the value
            object value = null;
            IEnumerator valueCoroutine = EvaluateExpression(stmt.Value);
            while (valueCoroutine.MoveNext())
            {
                object current = valueCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    value = current;
                }
                yield return current;
            }
            value = valueCoroutine.Current;

            // Handle different target types
            if (stmt.Target is VariableExpr)
            {
                VariableExpr varExpr = (VariableExpr)stmt.Target;
                SetVariable(varExpr.Name, value);
            }
            else if (stmt.Target is GetExpr)
            {
                // Property assignment: obj.prop = value
                GetExpr getExpr = (GetExpr)stmt.Target;
                
                object obj = null;
                IEnumerator objCoroutine = EvaluateExpression(getExpr.Object);
                while (objCoroutine.MoveNext())
                {
                    object current = objCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        obj = current;
                    }
                    yield return current;
                }
                obj = objCoroutine.Current;

                if (obj is ClassInstance)
                {
                    ClassInstance instance = (ClassInstance)obj;
                    instance.Set(getExpr.Name, value);
                }
                else
                {
                    ThrowRuntimeError("Cannot set property on non-object");
                }
            }
            else if (stmt.Target is SliceExpr)
            {
                // Index assignment: list[i] = value
                SliceExpr sliceExpr = (SliceExpr)stmt.Target;
                
                object obj = null;
                IEnumerator objCoroutine = EvaluateExpression(sliceExpr.Object);
                while (objCoroutine.MoveNext())
                {
                    object current = objCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        obj = current;
                    }
                    yield return current;
                }
                obj = objCoroutine.Current;

                object index = null;
                IEnumerator indexCoroutine = EvaluateExpression(sliceExpr.Index);
                while (indexCoroutine.MoveNext())
                {
                    object current = indexCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        index = current;
                    }
                    yield return current;
                }
                index = indexCoroutine.Current;

                if (obj is List<object>)
                {
                    List<object> list = (List<object>)obj;
                    int idx = ConvertToInt(index);
                    
                    // Handle negative indexing
                    if (idx < 0)
                        idx = list.Count + idx;
                    
                    if (idx < 0 || idx >= list.Count)
                        ThrowRuntimeError("List index out of range");
                    
                    list[idx] = value;
                }
                else if (obj is Dictionary<object, object>)
                {
                    Dictionary<object, object> dict = (Dictionary<object, object>)obj;
                    dict[index] = value;
                }
                else
                {
                    ThrowRuntimeError("Object does not support indexing");
                }
            }
            else
            {
                ThrowRuntimeError("Invalid assignment target");
            }
        }

        private IEnumerator ExecuteIf(IfStmt stmt)
        {
            currentLine = stmt.LineNumber;

            // Evaluate condition
            object conditionValue = null;
            IEnumerator condCoroutine = EvaluateExpression(stmt.Condition);
            while (condCoroutine.MoveNext())
            {
                object current = condCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    conditionValue = current;
                }
                yield return current;
            }
            conditionValue = condCoroutine.Current;

            if (IsTruthy(conditionValue))
            {
                // Execute then branch
                foreach (Stmt thenStmt in stmt.ThenBranch)
                {
                    IEnumerator coroutine = ExecuteStatement(thenStmt);
                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current;
                    }
                    
                    if (shouldBreak || shouldContinue || hasReturnValue)
                        yield break;
                }
            }
            else
            {
                // Check elif branches
                bool executed = false;
                foreach (Tuple<Expr, List<Stmt>> elifBranch in stmt.ElifBranches)
                {
                    object elifCondValue = null;
                    IEnumerator elifCoroutine = EvaluateExpression(elifBranch.Item1);
                    while (elifCoroutine.MoveNext())
                    {
                        object current = elifCoroutine.Current;
                        if (current != null && !(current is YieldInstruction))
                        {
                            elifCondValue = current;
                        }
                        yield return current;
                    }
                    elifCondValue = elifCoroutine.Current;

                    if (IsTruthy(elifCondValue))
                    {
                        foreach (Stmt elifStmt in elifBranch.Item2)
                        {
                            IEnumerator coroutine = ExecuteStatement(elifStmt);
                            while (coroutine.MoveNext())
                            {
                                yield return coroutine.Current;
                            }
                            
                            if (shouldBreak || shouldContinue || hasReturnValue)
                                yield break;
                        }
                        executed = true;
                        break;
                    }
                }

                // Execute else branch if no elif was executed
                if (!executed && stmt.ElseBranch != null)
                {
                    foreach (Stmt elseStmt in stmt.ElseBranch)
                    {
                        IEnumerator coroutine = ExecuteStatement(elseStmt);
                        while (coroutine.MoveNext())
                        {
                            yield return coroutine.Current;
                        }
                        
                        if (shouldBreak || shouldContinue || hasReturnValue)
                            yield break;
                    }
                }
            }
        }

        private IEnumerator ExecuteWhile(WhileStmt stmt)
        {
            currentLine = stmt.LineNumber;

            while (true)
            {
                // Evaluate condition
                object conditionValue = null;
                IEnumerator condCoroutine = EvaluateExpression(stmt.Condition);
                while (condCoroutine.MoveNext())
                {
                    object current = condCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        conditionValue = current;
                    }
                    yield return current;
                }
                conditionValue = condCoroutine.Current;

                if (!IsTruthy(conditionValue))
                    break;

                // Execute body
                foreach (Stmt bodyStmt in stmt.Body)
                {
                    IEnumerator coroutine = ExecuteStatement(bodyStmt);
                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current;
                    }

                    if (shouldBreak || hasReturnValue)
                    {
                        shouldBreak = false;
                        yield break;
                    }

                    if (shouldContinue)
                    {
                        shouldContinue = false;
                        break;
                    }
                }
            }
        }

        private IEnumerator ExecuteFor(ForStmt stmt)
        {
            currentLine = stmt.LineNumber;

            // Evaluate iterable
            object iterableValue = null;
            IEnumerator iterCoroutine = EvaluateExpression(stmt.Iterable);
            while (iterCoroutine.MoveNext())
            {
                object current = iterCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    iterableValue = current;
                }
                yield return current;
            }
            iterableValue = iterCoroutine.Current;

            // Get iterable
            IEnumerable iterable = GetIterable(iterableValue);
            
            foreach (object item in iterable)
            {
                SetVariable(stmt.Variable, item);

                // Execute body
                foreach (Stmt bodyStmt in stmt.Body)
                {
                    IEnumerator coroutine = ExecuteStatement(bodyStmt);
                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current;
                    }

                    if (shouldBreak || hasReturnValue)
                    {
                        shouldBreak = false;
                        yield break;
                    }

                    if (shouldContinue)
                    {
                        shouldContinue = false;
                        break;
                    }
                }
            }
        }

        private void ExecuteFunctionDef(FunctionDef stmt)
        {
            currentLine = stmt.LineNumber;
            PythonFunction func = new PythonFunction(stmt.Name, stmt.Parameters, stmt.Body, this);
            functions[stmt.Name] = func;
            SetVariable(stmt.Name, func);
        }

        private void ExecuteClassDef(ClassDef stmt)
        {
            currentLine = stmt.LineNumber;
            PythonClass pyClass = new PythonClass(stmt.Name, stmt.Methods, this);
            classes[stmt.Name] = pyClass;
            SetVariable(stmt.Name, pyClass);
        }

        private IEnumerator ExecuteReturn(ReturnStmt stmt)
        {
            currentLine = stmt.LineNumber;
            
            if (stmt.Value != null)
            {
                object value = null;
                IEnumerator valueCoroutine = EvaluateExpression(stmt.Value);
                while (valueCoroutine.MoveNext())
                {
                    object current = valueCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        value = current;
                    }
                    yield return current;
                }
                returnValue = valueCoroutine.Current;
            }
            else
            {
                returnValue = null;
            }
            
            hasReturnValue = true;
        }

        private void ExecuteGlobal(GlobalStmt stmt)
        {
            foreach (string name in stmt.Names)
            {
                globalVariables.Add(name);
            }
        }
        #endregion

        #region Public Methods - Expression Evaluation (For Internal Use)
        public IEnumerator EvaluateExpression(Expr expr)
        {
            if (expr == null)
            {
                yield return null;
                yield break;
            }

            currentLine = expr.LineNumber;
            operationCount++;

            // Time slicing
            if (operationCount >= MAX_OPERATIONS_PER_FRAME)
            {
                operationCount = 0;
                yield return null;
            }

            if (expr is LiteralExpr)
            {
                yield return ((LiteralExpr)expr).Value;
            }
            else if (expr is VariableExpr)
            {
                VariableExpr varExpr = (VariableExpr)expr;
                yield return GetVariable(varExpr.Name);
            }
            else if (expr is BinaryExpr)
            {
                IEnumerator coroutine = EvaluateBinary((BinaryExpr)expr);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (expr is UnaryExpr)
            {
                IEnumerator coroutine = EvaluateUnary((UnaryExpr)expr);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (expr is CallExpr)
            {
                IEnumerator coroutine = EvaluateCall((CallExpr)expr);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (expr is GetExpr)
            {
                IEnumerator coroutine = EvaluateGet((GetExpr)expr);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (expr is SliceExpr)
            {
                IEnumerator coroutine = EvaluateSlice((SliceExpr)expr);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (expr is ListExpr)
            {
                IEnumerator coroutine = EvaluateList((ListExpr)expr);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (expr is DictExpr)
            {
                IEnumerator coroutine = EvaluateDict((DictExpr)expr);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (expr is SelfExpr)
            {
                yield return GetVariable("self");
            }
            else if (expr is ListCompExpr)
            {
                IEnumerator coroutine = EvaluateListComp((ListCompExpr)expr);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (expr is LambdaExpr)
            {
                LambdaExpr lambdaExpr = (LambdaExpr)expr;
                PythonLambda lambda = new PythonLambda(lambdaExpr.Parameters, lambdaExpr.Body, this);
                yield return lambda;
            }
            else
            {
                ThrowRuntimeError("Unknown expression type: " + expr.GetType().Name);
                yield return null;
            }
        }
        #endregion

        // Continue in next message due to length...
    }
}
// CONTINUATION OF PythonInterpreter.cs
// This file should be merged with Part1

        #region Private Methods - Expression Evaluation (Part 2)
        private IEnumerator EvaluateBinary(BinaryExpr expr)
        {
            // Evaluate left
            object left = null;
            IEnumerator leftCoroutine = EvaluateExpression(expr.Left);
            while (leftCoroutine.MoveNext())
            {
                object current = leftCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    left = current;
                }
                yield return current;
            }
            left = leftCoroutine.Current;

            // Short-circuit for AND and OR
            if (expr.Operator == TokenType.AND)
            {
                if (!IsTruthy(left))
                {
                    yield return left;
                    yield break;
                }
            }
            else if (expr.Operator == TokenType.OR)
            {
                if (IsTruthy(left))
                {
                    yield return left;
                    yield break;
                }
            }

            // Evaluate right
            object right = null;
            IEnumerator rightCoroutine = EvaluateExpression(expr.Right);
            while (rightCoroutine.MoveNext())
            {
                object current = rightCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    right = current;
                }
                yield return current;
            }
            right = rightCoroutine.Current;

            // Apply operator
            object result = ApplyBinaryOperator(expr.Operator, left, right);
            yield return result;
        }

        private object ApplyBinaryOperator(TokenType op, object left, object right)
        {
            // Arithmetic
            if (op == TokenType.PLUS)
            {
                // String concatenation
                if (left is string || right is string)
                    return ConvertToString(left) + ConvertToString(right);
                
                // List concatenation
                if (left is List<object> && right is List<object>)
                {
                    List<object> result = new List<object>((List<object>)left);
                    result.AddRange((List<object>)right);
                    return result;
                }
                
                return ConvertToDouble(left) + ConvertToDouble(right);
            }
            if (op == TokenType.MINUS) return ConvertToDouble(left) - ConvertToDouble(right);
            if (op == TokenType.STAR) 
            {
                // String repetition
                if (left is string && IsNumber(right))
                {
                    string str = (string)left;
                    int count = ConvertToInt(right);
                    string result = "";
                    for (int i = 0; i < count; i++)
                        result += str;
                    return result;
                }
                
                return ConvertToDouble(left) * ConvertToDouble(right);
            }
            if (op == TokenType.SLASH)
            {
                double divisor = ConvertToDouble(right);
                if (Math.Abs(divisor) < 0.0000001)
                    ThrowRuntimeError("Division by zero");
                return ConvertToDouble(left) / divisor;
            }
            if (op == TokenType.PERCENT) return ConvertToDouble(left) % ConvertToDouble(right);
            if (op == TokenType.POWER) return Math.Pow(ConvertToDouble(left), ConvertToDouble(right));

            // Comparison
            if (op == TokenType.EQUAL_EQUAL) return IsEqual(left, right);
            if (op == TokenType.BANG_EQUAL) return !IsEqual(left, right);
            if (op == TokenType.LESS) return ConvertToDouble(left) < ConvertToDouble(right);
            if (op == TokenType.LESS_EQUAL) return ConvertToDouble(left) <= ConvertToDouble(right);
            if (op == TokenType.GREATER) return ConvertToDouble(left) > ConvertToDouble(right);
            if (op == TokenType.GREATER_EQUAL) return ConvertToDouble(left) >= ConvertToDouble(right);

            // Membership
            if (op == TokenType.IN)
            {
                if (right is List<object>)
                {
                    List<object> list = (List<object>)right;
                    foreach (object item in list)
                    {
                        if (IsEqual(left, item))
                            return true;
                    }
                    return false;
                }
                else if (right is string)
                {
                    string str = (string)right;
                    string search = ConvertToString(left);
                    return str.Contains(search);
                }
                else if (right is Dictionary<object, object>)
                {
                    Dictionary<object, object> dict = (Dictionary<object, object>)right;
                    return dict.ContainsKey(left);
                }
                ThrowRuntimeError("'in' requires iterable");
            }

            // Bitwise
            if (op == TokenType.AMPERSAND) return ConvertToInt(left) & ConvertToInt(right);
            if (op == TokenType.PIPE) return ConvertToInt(left) | ConvertToInt(right);
            if (op == TokenType.CARET) return ConvertToInt(left) ^ ConvertToInt(right);
            if (op == TokenType.LEFT_SHIFT) return ConvertToInt(left) << ConvertToInt(right);
            if (op == TokenType.RIGHT_SHIFT) return ConvertToInt(left) >> ConvertToInt(right);

            // Logical (AND/OR handled above with short-circuit)
            if (op == TokenType.AND) return right;
            if (op == TokenType.OR) return right;

            ThrowRuntimeError("Unknown binary operator: " + op);
            return null;
        }

        private IEnumerator EvaluateUnary(UnaryExpr expr)
        {
            object right = null;
            IEnumerator rightCoroutine = EvaluateExpression(expr.Right);
            while (rightCoroutine.MoveNext())
            {
                object current = rightCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    right = current;
                }
                yield return current;
            }
            right = rightCoroutine.Current;

            if (expr.Operator == TokenType.MINUS)
            {
                yield return -ConvertToDouble(right);
            }
            else if (expr.Operator == TokenType.NOT)
            {
                yield return !IsTruthy(right);
            }
            else if (expr.Operator == TokenType.TILDE)
            {
                yield return ~ConvertToInt(right);
            }
            else
            {
                ThrowRuntimeError("Unknown unary operator: " + expr.Operator);
                yield return null;
            }
        }

        private IEnumerator EvaluateCall(CallExpr expr)
        {
            // Evaluate callee
            object callee = null;
            IEnumerator calleeCoroutine = EvaluateExpression(expr.Callee);
            while (calleeCoroutine.MoveNext())
            {
                object current = calleeCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    callee = current;
                }
                yield return current;
            }
            callee = calleeCoroutine.Current;

            // Evaluate arguments
            List<object> arguments = new List<object>();
            foreach (Expr argExpr in expr.Arguments)
            {
                object arg = null;
                IEnumerator argCoroutine = EvaluateExpression(argExpr);
                while (argCoroutine.MoveNext())
                {
                    object current = argCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        arg = current;
                    }
                    yield return current;
                }
                arguments.Add(argCoroutine.Current);
            }

            // Call the function
            if (callee is BuiltinFunction)
            {
                BuiltinFunction builtin = (BuiltinFunction)callee;
                object result = builtin.Call(arguments);
                
                // If result is a YieldInstruction, yield it
                if (result is YieldInstruction)
                {
                    yield return result;
                    yield return null; // Return value after yield
                }
                else
                {
                    yield return result;
                }
            }
            else if (callee is PythonFunction)
            {
                PythonFunction func = (PythonFunction)callee;
                IEnumerator coroutine = func.Call(arguments);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (callee is PythonLambda)
            {
                PythonLambda lambda = (PythonLambda)callee;
                IEnumerator coroutine = lambda.Call(arguments);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            else if (callee is PythonClass)
            {
                PythonClass pyClass = (PythonClass)callee;
                ClassInstance instance = pyClass.Instantiate();
                
                // Call __init__ if it exists
                if (instance.HasMethod("__init__"))
                {
                    PythonFunction initMethod = instance.GetMethod("__init__");
                    IEnumerator coroutine = initMethod.Call(arguments);
                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current;
                    }
                }
                
                yield return instance;
            }
            else if (callee is ClassInstance)
            {
                // Calling an instance method directly (shouldn't happen normally)
                ThrowRuntimeError("Cannot call class instance directly");
                yield return null;
            }
            else
            {
                ThrowRuntimeError("Object is not callable: " + (callee != null ? callee.GetType().Name : "null"));
                yield return null;
            }
        }

        private IEnumerator EvaluateGet(GetExpr expr)
        {
            // Evaluate object
            object obj = null;
            IEnumerator objCoroutine = EvaluateExpression(expr.Object);
            while (objCoroutine.MoveNext())
            {
                object current = objCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    obj = current;
                }
                yield return current;
            }
            obj = objCoroutine.Current;

            // Handle different object types
            if (obj is ClassInstance)
            {
                ClassInstance instance = (ClassInstance)obj;
                
                // Check if it's a method
                if (instance.HasMethod(expr.Name))
                {
                    yield return instance.GetMethod(expr.Name);
                }
                else
                {
                    yield return instance.Get(expr.Name);
                }
            }
            else if (obj is string)
            {
                // String methods
                yield return new StringMethod((string)obj, expr.Name);
            }
            else if (obj is List<object>)
            {
                // List methods
                yield return new ListMethod((List<object>)obj, expr.Name, this);
            }
            else if (obj is Dictionary<object, object>)
            {
                // Dictionary methods
                yield return new DictMethod((Dictionary<object, object>)obj, expr.Name);
            }
            else
            {
                ThrowRuntimeError("Object does not have property: " + expr.Name);
                yield return null;
            }
        }

        private IEnumerator EvaluateSlice(SliceExpr expr)
        {
            // Evaluate object
            object obj = null;
            IEnumerator objCoroutine = EvaluateExpression(expr.Object);
            while (objCoroutine.MoveNext())
            {
                object current = objCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    obj = current;
                }
                yield return current;
            }
            obj = objCoroutine.Current;

            if (expr.IsSlice)
            {
                // Handle slicing
                int start = 0;
                int end = 0;

                if (expr.Index != null)
                {
                    object startObj = null;
                    IEnumerator startCoroutine = EvaluateExpression(expr.Index);
                    while (startCoroutine.MoveNext())
                    {
                        object current = startCoroutine.Current;
                        if (current != null && !(current is YieldInstruction))
                        {
                            startObj = current;
                        }
                        yield return current;
                    }
                    start = ConvertToInt(startCoroutine.Current);
                }

                if (expr.EndIndex != null)
                {
                    object endObj = null;
                    IEnumerator endCoroutine = EvaluateExpression(expr.EndIndex);
                    while (endCoroutine.MoveNext())
                    {
                        object current = endCoroutine.Current;
                        if (current != null && !(current is YieldInstruction))
                        {
                            endObj = current;
                        }
                        yield return current;
                    }
                    end = ConvertToInt(endCoroutine.Current);
                }

                if (obj is List<object>)
                {
                    List<object> list = (List<object>)obj;
                    
                    // Handle default values
                    if (expr.Index == null) start = 0;
                    if (expr.EndIndex == null) end = list.Count;
                    
                    // Handle negative indices
                    if (start < 0) start = list.Count + start;
                    if (end < 0) end = list.Count + end;
                    
                    // Clamp
                    start = Math.Max(0, Math.Min(start, list.Count));
                    end = Math.Max(0, Math.Min(end, list.Count));
                    
                    List<object> result = new List<object>();
                    for (int i = start; i < end; i++)
                    {
                        result.Add(list[i]);
                    }
                    
                    yield return result;
                }
                else if (obj is string)
                {
                    string str = (string)obj;
                    
                    // Handle default values
                    if (expr.Index == null) start = 0;
                    if (expr.EndIndex == null) end = str.Length;
                    
                    // Handle negative indices
                    if (start < 0) start = str.Length + start;
                    if (end < 0) end = str.Length + end;
                    
                    // Clamp
                    start = Math.Max(0, Math.Min(start, str.Length));
                    end = Math.Max(0, Math.Min(end, str.Length));
                    
                    yield return str.Substring(start, end - start);
                }
                else
                {
                    ThrowRuntimeError("Object does not support slicing");
                    yield return null;
                }
            }
            else
            {
                // Handle indexing
                object indexObj = null;
                IEnumerator indexCoroutine = EvaluateExpression(expr.Index);
                while (indexCoroutine.MoveNext())
                {
                    object current = indexCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        indexObj = current;
                    }
                    yield return current;
                }
                indexObj = indexCoroutine.Current;

                if (obj is List<object>)
                {
                    List<object> list = (List<object>)obj;
                    int index = ConvertToInt(indexObj);
                    
                    // Handle negative indexing
                    if (index < 0)
                        index = list.Count + index;
                    
                    if (index < 0 || index >= list.Count)
                        ThrowRuntimeError("List index out of range");
                    
                    yield return list[index];
                }
                else if (obj is Dictionary<object, object>)
                {
                    Dictionary<object, object> dict = (Dictionary<object, object>)obj;
                    if (!dict.ContainsKey(indexObj))
                        ThrowRuntimeError("Key not found in dictionary");
                    
                    yield return dict[indexObj];
                }
                else if (obj is string)
                {
                    string str = (string)obj;
                    int index = ConvertToInt(indexObj);
                    
                    // Handle negative indexing
                    if (index < 0)
                        index = str.Length + index;
                    
                    if (index < 0 || index >= str.Length)
                        ThrowRuntimeError("String index out of range");
                    
                    yield return str[index].ToString();
                }
                else
                {
                    ThrowRuntimeError("Object does not support indexing");
                    yield return null;
                }
            }
        }

        private IEnumerator EvaluateList(ListExpr expr)
        {
            List<object> list = new List<object>();
            
            foreach (Expr elemExpr in expr.Elements)
            {
                object elem = null;
                IEnumerator elemCoroutine = EvaluateExpression(elemExpr);
                while (elemCoroutine.MoveNext())
                {
                    object current = elemCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        elem = current;
                    }
                    yield return current;
                }
                list.Add(elemCoroutine.Current);
            }
            
            yield return list;
        }

        private IEnumerator EvaluateDict(DictExpr expr)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();
            
            foreach (Tuple<Expr, Expr> pair in expr.Pairs)
            {
                object key = null;
                IEnumerator keyCoroutine = EvaluateExpression(pair.Item1);
                while (keyCoroutine.MoveNext())
                {
                    object current = keyCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        key = current;
                    }
                    yield return current;
                }
                key = keyCoroutine.Current;

                object value = null;
                IEnumerator valueCoroutine = EvaluateExpression(pair.Item2);
                while (valueCoroutine.MoveNext())
                {
                    object current = valueCoroutine.Current;
                    if (current != null && !(current is YieldInstruction))
                    {
                        value = current;
                    }
                    yield return current;
                }
                value = valueCoroutine.Current;

                dict[key] = value;
            }
            
            yield return dict;
        }

        private IEnumerator EvaluateListComp(ListCompExpr expr)
        {
            List<object> result = new List<object>();
            
            // Evaluate iterable
            object iterableValue = null;
            IEnumerator iterCoroutine = EvaluateExpression(expr.Iterable);
            while (iterCoroutine.MoveNext())
            {
                object current = iterCoroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    iterableValue = current;
                }
                yield return current;
            }
            iterableValue = iterCoroutine.Current;

            IEnumerable iterable = GetIterable(iterableValue);
            
            // Create new scope for loop variable
            PushScope();
            
            foreach (object item in iterable)
            {
                SetVariable(expr.Variable, item);
                
                // Evaluate condition if present
                bool include = true;
                if (expr.Condition != null)
                {
                    object condValue = null;
                    IEnumerator condCoroutine = EvaluateExpression(expr.Condition);
                    while (condCoroutine.MoveNext())
                    {
                        object current = condCoroutine.Current;
                        if (current != null && !(current is YieldInstruction))
                        {
                            condValue = current;
                        }
                        yield return current;
                    }
                    include = IsTruthy(condCoroutine.Current);
                }
                
                if (include)
                {
                    // Evaluate element expression
                    object elem = null;
                    IEnumerator elemCoroutine = EvaluateExpression(expr.Element);
                    while (elemCoroutine.MoveNext())
                    {
                        object current = elemCoroutine.Current;
                        if (current != null && !(current is YieldInstruction))
                        {
                            elem = current;
                        }
                        yield return current;
                    }
                    result.Add(elemCoroutine.Current);
                }
            }
            
            PopScope();
            
            yield return result;
        }
        #endregion

        #region Public Methods - Scope Management
        public void PushScope()
        {
            scopeStack.Push(new Dictionary<string, object>());
        }

        public void PopScope()
        {
            if (scopeStack.Count > 0)
                scopeStack.Pop();
        }

        public void SetVariable(string name, object value)
        {
            // Check if it's a global variable
            if (globalVariables.Contains(name))
            {
                globalScope[name] = value;
                return;
            }

            // Set in current scope or global
            if (scopeStack.Count > 0)
            {
                scopeStack.Peek()[name] = value;
            }
            else
            {
                globalScope[name] = value;
            }
        }

        public object GetVariable(string name)
        {
            // Check current scope
            if (scopeStack.Count > 0 && scopeStack.Peek().ContainsKey(name))
            {
                return scopeStack.Peek()[name];
            }

            // Check enclosing scopes
            foreach (Dictionary<string, object> scope in scopeStack)
            {
                if (scope.ContainsKey(name))
                    return scope[name];
            }

            // Check global scope
            if (globalScope.ContainsKey(name))
            {
                return globalScope[name];
            }

            ThrowRuntimeError("Undefined variable: " + name);
            return null;
        }
        #endregion

        // Continue with helper methods and classes...
// CONTINUATION OF PythonInterpreter.cs
// This file should be merged with Part1 and Part2

        #region Private Methods - Helper Functions
        public bool IsTruthy(object value)
        {
            if (value == null) return false;
            if (value is bool) return (bool)value;
            if (value is double) return (double)value != 0.0;
            if (value is int) return (int)value != 0;
            if (value is string) return ((string)value).Length > 0;
            if (value is List<object>) return ((List<object>)value).Count > 0;
            return true;
        }

        public bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            
            // Handle list comparison
            if (a is List<object> && b is List<object>)
            {
                List<object> listA = (List<object>)a;
                List<object> listB = (List<object>)b;
                
                if (listA.Count != listB.Count) return false;
                
                for (int i = 0; i < listA.Count; i++)
                {
                    if (!IsEqual(listA[i], listB[i]))
                        return false;
                }
                
                return true;
            }
            
            return a.Equals(b);
        }

        private IEnumerable GetIterable(object value)
        {
            if (value is List<object>)
                return (List<object>)value;
            
            if (value is string)
            {
                string str = (string)value;
                List<object> chars = new List<object>();
                foreach (char c in str)
                    chars.Add(c.ToString());
                return chars;
            }
            
            if (value is Dictionary<object, object>)
            {
                Dictionary<object, object> dict = (Dictionary<object, object>)value;
                return new List<object>(dict.Keys);
            }
            
            // Handle range() result
            if (value is IEnumerable)
                return (IEnumerable)value;
            
            ThrowRuntimeError("Object is not iterable");
            return null;
        }

        private bool IsNumber(object value)
        {
            return value is double || value is int || value is float || value is long;
        }

        public double ConvertToDouble(object value)
        {
            if (value is double) return (double)value;
            if (value is int) return (double)(int)value;
            if (value is float) return (double)(float)value;
            if (value is long) return (double)(long)value;
            if (value is bool) return (bool)value ? 1.0 : 0.0;
            if (value is string)
            {
                double result;
                if (double.TryParse((string)value, out result))
                    return result;
            }
            
            ThrowRuntimeError("Cannot convert to number: " + (value != null ? value.GetType().Name : "null"));
            return 0.0;
        }

        public int ConvertToInt(object value)
        {
            return (int)Math.Round(ConvertToDouble(value));
        }

        public string ConvertToString(object value)
        {
            if (value == null) return "None";
            if (value is string) return (string)value;
            if (value is bool) return (bool)value ? "True" : "False";
            if (value is List<object>)
            {
                List<object> list = (List<object>)value;
                string result = "[";
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) result += ", ";
                    result += ConvertToString(list[i]);
                }
                result += "]";
                return result;
            }
            if (value is Dictionary<object, object>)
            {
                Dictionary<object, object> dict = (Dictionary<object, object>)value;
                string result = "{";
                int i = 0;
                foreach (KeyValuePair<object, object> pair in dict)
                {
                    if (i > 0) result += ", ";
                    result += ConvertToString(pair.Key) + ": " + ConvertToString(pair.Value);
                    i++;
                }
                result += "}";
                return result;
            }
            return value.ToString();
        }

        private void ThrowRuntimeError(string message)
        {
            throw new RuntimeError(currentLine, message);
        }
        #endregion
    }

    #region Helper Classes - Functions and Classes
    /// <summary>
    /// Represents a built-in function
    /// </summary>
    public class BuiltinFunction
    {
        public string Name { get; private set; }
        private Func<List<object>, object> implementation;

        public BuiltinFunction(string name, Func<List<object>, object> implementation)
        {
            Name = name;
            this.implementation = implementation;
        }

        public object Call(List<object> arguments)
        {
            return implementation(arguments);
        }
    }

    /// <summary>
    /// Represents a user-defined Python function
    /// </summary>
    public class PythonFunction
    {
        public string Name { get; private set; }
        public List<string> Parameters { get; private set; }
        public List<Stmt> Body { get; private set; }
        private PythonInterpreter interpreter;
        private ClassInstance boundInstance; // For bound methods

        public PythonFunction(string name, List<string> parameters, List<Stmt> body, PythonInterpreter interpreter, ClassInstance boundInstance = null)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
            this.interpreter = interpreter;
            this.boundInstance = boundInstance;
        }

        public IEnumerator Call(List<object> arguments)
        {
            if (arguments.Count != Parameters.Count)
            {
                throw new RuntimeError(0, "Function '" + Name + "' expected " + Parameters.Count + " arguments but got " + arguments.Count);
            }

            // Push new scope
            interpreter.PushScope();

            // Bind 'self' if this is a bound method
            if (boundInstance != null)
            {
                interpreter.SetVariable("self", boundInstance);
            }

            // Bind parameters
            for (int i = 0; i < Parameters.Count; i++)
            {
                interpreter.SetVariable(Parameters[i], arguments[i]);
            }

            // Execute body
            interpreter.hasReturnValue = false;
            foreach (Stmt stmt in Body)
            {
                IEnumerator coroutine = interpreter.ExecuteStatement(stmt);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }

                if (interpreter.hasReturnValue)
                {
                    break;
                }
            }

            // Pop scope
            interpreter.PopScope();

            // Return value
            object result = interpreter.returnValue;
            interpreter.returnValue = null;
            interpreter.hasReturnValue = false;
            
            yield return result;
        }

        public PythonFunction Bind(ClassInstance instance)
        {
            return new PythonFunction(Name, Parameters, Body, interpreter, instance);
        }
    }

    /// <summary>
    /// Represents a lambda expression
    /// </summary>
    public class PythonLambda
    {
        public List<string> Parameters { get; private set; }
        public Expr Body { get; private set; }
        private PythonInterpreter interpreter;

        public PythonLambda(List<string> parameters, Expr body, PythonInterpreter interpreter)
        {
            Parameters = parameters;
            Body = body;
            this.interpreter = interpreter;
        }

        public IEnumerator Call(List<object> arguments)
        {
            if (arguments.Count != Parameters.Count)
            {
                throw new RuntimeError(0, "Lambda expected " + Parameters.Count + " arguments but got " + arguments.Count);
            }

            // Push new scope
            interpreter.PushScope();

            // Bind parameters
            for (int i = 0; i < Parameters.Count; i++)
            {
                interpreter.SetVariable(Parameters[i], arguments[i]);
            }

            // Evaluate body expression
            object result = null;
            IEnumerator coroutine = interpreter.EvaluateExpression(Body);
            while (coroutine.MoveNext())
            {
                object current = coroutine.Current;
                if (current != null && !(current is YieldInstruction))
                {
                    result = current;
                }
                yield return current;
            }
            result = coroutine.Current;

            // Pop scope
            interpreter.PopScope();

            yield return result;
        }
    }

    /// <summary>
    /// Represents a Python class
    /// </summary>
    public class PythonClass
    {
        public string Name { get; private set; }
        public Dictionary<string, FunctionDef> Methods { get; private set; }
        private PythonInterpreter interpreter;

        public PythonClass(string name, List<FunctionDef> methods, PythonInterpreter interpreter)
        {
            Name = name;
            Methods = new Dictionary<string, FunctionDef>();
            this.interpreter = interpreter;

            foreach (FunctionDef method in methods)
            {
                Methods[method.Name] = method;
            }
        }

        public ClassInstance Instantiate()
        {
            return new ClassInstance(this, interpreter);
        }
    }

    /// <summary>
    /// Represents an instance of a Python class
    /// </summary>
    public class ClassInstance
    {
        public PythonClass Class { get; private set; }
        private Dictionary<string, object> fields;
        private Dictionary<string, PythonFunction> methods;
        private PythonInterpreter interpreter;

        public ClassInstance(PythonClass pyClass, PythonInterpreter interpreter)
        {
            Class = pyClass;
            this.interpreter = interpreter;
            fields = new Dictionary<string, object>();
            methods = new Dictionary<string, PythonFunction>();

            // Bind methods
            foreach (KeyValuePair<string, FunctionDef> methodDef in pyClass.Methods)
            {
                PythonFunction func = new PythonFunction(
                    methodDef.Value.Name,
                    methodDef.Value.Parameters,
                    methodDef.Value.Body,
                    interpreter
                );
                methods[methodDef.Key] = func.Bind(this);
            }
        }

        public object Get(string name)
        {
            if (fields.ContainsKey(name))
                return fields[name];
            
            throw new RuntimeError(0, "Undefined property: " + name);
        }

        public void Set(string name, object value)
        {
            fields[name] = value;
        }

        public bool HasMethod(string name)
        {
            return methods.ContainsKey(name);
        }

        public PythonFunction GetMethod(string name)
        {
            if (methods.ContainsKey(name))
                return methods[name];
            
            throw new RuntimeError(0, "Undefined method: " + name);
        }

        public override string ToString()
        {
            return "<" + Class.Name + " instance>";
        }
    }
    #endregion

    #region Helper Classes - String Methods
    /// <summary>
    /// Wrapper for string methods
    /// </summary>
    public class StringMethod
    {
        private string str;
        private string methodName;

        public StringMethod(string str, string methodName)
        {
            this.str = str;
            this.methodName = methodName;
        }

        public object Call(List<object> arguments)
        {
            if (methodName == "split")
            {
                if (arguments.Count == 0)
                {
                    // Split by whitespace
                    string[] parts = str.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    List<object> result = new List<object>();
                    foreach (string part in parts)
                        result.Add(part);
                    return result;
                }
                else
                {
                    string delimiter = arguments[0].ToString();
                    string[] parts = str.Split(new string[] { delimiter }, StringSplitOptions.None);
                    List<object> result = new List<object>();
                    foreach (string part in parts)
                        result.Add(part);
                    return result;
                }
            }
            else if (methodName == "strip")
            {
                return str.Trim();
            }
            else if (methodName == "replace")
            {
                if (arguments.Count < 2)
                    throw new RuntimeError(0, "replace() requires 2 arguments");
                
                string oldValue = arguments[0].ToString();
                string newValue = arguments[1].ToString();
                return str.Replace(oldValue, newValue);
            }
            else if (methodName == "join")
            {
                if (arguments.Count < 1)
                    throw new RuntimeError(0, "join() requires 1 argument");
                
                List<object> list = arguments[0] as List<object>;
                if (list == null)
                    throw new RuntimeError(0, "join() argument must be a list");
                
                string result = "";
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) result += str;
                    result += list[i].ToString();
                }
                return result;
            }
            else if (methodName == "lower")
            {
                return str.ToLower();
            }
            else if (methodName == "upper")
            {
                return str.ToUpper();
            }
            else if (methodName == "startswith")
            {
                if (arguments.Count < 1)
                    throw new RuntimeError(0, "startswith() requires 1 argument");
                return str.StartsWith(arguments[0].ToString());
            }
            else if (methodName == "endswith")
            {
                if (arguments.Count < 1)
                    throw new RuntimeError(0, "endswith() requires 1 argument");
                return str.EndsWith(arguments[0].ToString());
            }
            
            throw new RuntimeError(0, "String has no method: " + methodName);
        }
    }
    #endregion

    #region Helper Classes - List Methods
    /// <summary>
    /// Wrapper for list methods
    /// </summary>
    public class ListMethod
    {
        private List<object> list;
        private string methodName;
        private PythonInterpreter interpreter;

        public ListMethod(List<object> list, string methodName, PythonInterpreter interpreter)
        {
            this.list = list;
            this.methodName = methodName;
            this.interpreter = interpreter;
        }

        public object Call(List<object> arguments)
        {
            if (methodName == "append")
            {
                if (arguments.Count < 1)
                    throw new RuntimeError(0, "append() requires 1 argument");
                list.Add(arguments[0]);
                return null;
            }
            else if (methodName == "remove")
            {
                if (arguments.Count < 1)
                    throw new RuntimeError(0, "remove() requires 1 argument");
                
                bool found = false;
                for (int i = 0; i < list.Count; i++)
                {
                    if (interpreter.IsEqual(list[i], arguments[0]))
                    {
                        list.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                    throw new RuntimeError(0, "ValueError: list.remove(x): x not in list");
                
                return null;
            }
            else if (methodName == "pop")
            {
                int index = -1;
                if (arguments.Count > 0)
                    index = (int)Math.Round((double)arguments[0]);
                
                if (index < 0)
                    index = list.Count + index;
                
                if (index < 0 || index >= list.Count)
                    throw new RuntimeError(0, "IndexError: pop index out of range");
                
                object value = list[index];
                list.RemoveAt(index);
                return value;
            }
            else if (methodName == "sort")
            {
                // Handle optional key and reverse parameters
                PythonFunction keyFunc = null;
                PythonLambda keyLambda = null;
                bool reverse = false;
                
                // Parse arguments (simplified - not handling keyword arguments)
                if (arguments.Count > 0 && arguments[0] != null)
                {
                    if (arguments[0] is PythonFunction)
                        keyFunc = (PythonFunction)arguments[0];
                    else if (arguments[0] is PythonLambda)
                        keyLambda = (PythonLambda)arguments[0];
                }
                
                if (arguments.Count > 1 && arguments[1] != null)
                {
                    reverse = interpreter.IsTruthy(arguments[1]);
                }
                
                // Sort the list
                List<Tuple<object, object>> pairs = new List<Tuple<object, object>>();
                
                // Create pairs of (key, value)
                foreach (object item in list)
                {
                    object key = item;
                    
                    if (keyFunc != null || keyLambda != null)
                    {
                        // Evaluate key function - this is tricky without coroutines
                        // For now, we'll simplify and just use the item itself
                        // In a full implementation, this would need to be async
                        key = item;
                    }
                    
                    pairs.Add(new Tuple<object, object>(key, item));
                }
                
                // Sort pairs
                pairs.Sort((a, b) =>
                {
                    double aVal = interpreter.ConvertToDouble(a.Item1);
                    double bVal = interpreter.ConvertToDouble(b.Item1);
                    int result = aVal.CompareTo(bVal);
                    return reverse ? -result : result;
                });
                
                // Replace list contents
                list.Clear();
                foreach (Tuple<object, object> pair in pairs)
                {
                    list.Add(pair.Item2);
                }
                
                return null;
            }
            
            throw new RuntimeError(0, "List has no method: " + methodName);
        }
    }
    #endregion

    #region Helper Classes - Dictionary Methods
    /// <summary>
    /// Wrapper for dictionary methods
    /// </summary>
    public class DictMethod
    {
        private Dictionary<object, object> dict;
        private string methodName;

        public DictMethod(Dictionary<object, object> dict, string methodName)
        {
            this.dict = dict;
            this.methodName = methodName;
        }

        public object Call(List<object> arguments)
        {
            if (methodName == "get")
            {
                if (arguments.Count < 1)
                    throw new RuntimeError(0, "get() requires at least 1 argument");
                
                object key = arguments[0];
                object defaultValue = arguments.Count > 1 ? arguments[1] : null;
                
                if (dict.ContainsKey(key))
                    return dict[key];
                return defaultValue;
            }
            else if (methodName == "keys")
            {
                return new List<object>(dict.Keys);
            }
            else if (methodName == "values")
            {
                return new List<object>(dict.Values);
            }
            
            throw new RuntimeError(0, "Dictionary has no method: " + methodName);
        }
    }
    #endregion

    #region Exception Classes
    /// <summary>
    /// Runtime error with line number
    /// </summary>
    public class RuntimeError : Exception
    {
        public int LineNumber { get; private set; }

        public RuntimeError(int line, string message) : base(message)
        {
            LineNumber = line;
        }

        public override string ToString()
        {
            return string.Format("RuntimeError at line {0}: {1}", LineNumber, Message);
        }
    }
    #endregion
}
