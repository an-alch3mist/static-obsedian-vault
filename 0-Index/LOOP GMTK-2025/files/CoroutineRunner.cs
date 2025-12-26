using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PythonInterpreter
{
    /// <summary>
    /// Safely executes Python scripts in a coroutine
    /// Handles errors and integrates with Unity
    /// .NET 2.0 Compatible - no yield inside try-catch
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        #region Public Fields
        public ConsoleManager ConsoleManager;
        #endregion

        #region Private Fields
        private PythonInterpreter interpreter;
        private Coroutine currentExecution;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            interpreter = new PythonInterpreter();
            RegisterBuiltins();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Gets the interpreter instance (for registering additional builtins)
        /// </summary>
        public PythonInterpreter GetInterpreter()
        {
            return interpreter;
        }

        /// <summary>
        /// Runs a Python script
        /// </summary>
        public void RunScript(string sourceCode)
        {
            if (currentExecution != null)
            {
                StopCoroutine(currentExecution);
            }

            interpreter.Reset();
            RegisterBuiltins(); // Re-register after reset
            
            currentExecution = StartCoroutine(SafeExecute(sourceCode));
        }

        /// <summary>
        /// Stops the current script execution
        /// </summary>
        public void Stop()
        {
            if (currentExecution != null)
            {
                StopCoroutine(currentExecution);
                currentExecution = null;
            }
            interpreter.Reset();
        }
        #endregion

        #region Private Methods - Execution
        /// <summary>
        /// Executes a script with error handling
        /// .NET 2.0 compatible - uses flag-based error handling instead of yield in try-catch
        /// </summary>
        private IEnumerator SafeExecute(string sourceCode)
        {
            // Parse and execute - using flags to avoid yield in try-catch
            bool hasError = false;
            string errorMessage = "";
            int errorLine = 0;
            List<Stmt> statements = null;

            // PHASE 1: Lexing
            try
            {
                Lexer lexer = new Lexer();
                List<Token> tokens = lexer.Tokenize(sourceCode);

                // PHASE 2: Parsing
                Parser parser = new Parser();
                statements = parser.Parse(tokens);
            }
            catch (LexerError e)
            {
                hasError = true;
                errorLine = e.LineNumber;
                errorMessage = e.Message;
            }
            catch (ParserError e)
            {
                hasError = true;
                errorLine = e.Token.LineNumber;
                errorMessage = e.Message;
            }
            catch (Exception e)
            {
                hasError = true;
                errorLine = 0;
                errorMessage = "Unexpected error: " + e.Message;
            }

            // Handle parse errors
            if (hasError)
            {
                LogError(errorLine, errorMessage);
                interpreter.Reset();
                yield break;
            }

            // PHASE 3: Execution
            // Use a wrapper coroutine to catch runtime errors
            IEnumerator execution = ExecuteWithErrorHandling(statements);
            while (true)
            {
                bool hasNext = false;
                try
                {
                    hasNext = execution.MoveNext();
                }
                catch (RuntimeError e)
                {
                    hasError = true;
                    errorLine = e.LineNumber;
                    errorMessage = e.Message;
                    break;
                }
                catch (Exception e)
                {
                    hasError = true;
                    errorLine = 0;
                    errorMessage = "Unexpected runtime error: " + e.Message + "\nStack: " + e.StackTrace;
                    break;
                }

                if (!hasNext)
                    break;

                yield return execution.Current;
            }

            // Handle runtime errors
            if (hasError)
            {
                LogError(errorLine, errorMessage);
                interpreter.Reset();
            }
        }

        /// <summary>
        /// Wrapper for execution that can be iterated safely
        /// </summary>
        private IEnumerator ExecuteWithErrorHandling(List<Stmt> statements)
        {
            IEnumerator execution = interpreter.Execute(statements);
            while (execution.MoveNext())
            {
                yield return execution.Current;
            }
        }

        private void LogError(int line, string message)
        {
            string errorText = string.Format("<color=red>Error at line {0}: {1}</color>", line, message);
            
            if (ConsoleManager != null)
            {
                ConsoleManager.LogError(errorText);
            }
            else
            {
                Debug.LogError(errorText);
            }
        }
        #endregion

        #region Private Methods - Builtin Registration
        private void RegisterBuiltins()
        {
            // Print function
            interpreter.RegisterBuiltin("print", (args) =>
            {
                string output = "";
                for (int i = 0; i < args.Count; i++)
                {
                    if (i > 0) output += " ";
                    output += interpreter.ConvertToString(args[i]);
                }
                
                if (ConsoleManager != null)
                {
                    ConsoleManager.Log(output);
                }
                else
                {
                    Debug.Log(output);
                }
                
                return null;
            });

            // Range function
            interpreter.RegisterBuiltin("range", (args) =>
            {
                if (args.Count == 0)
                    throw new RuntimeError(0, "range() requires at least 1 argument");
                
                int start = 0;
                int end = 0;
                int step = 1;
                
                if (args.Count == 1)
                {
                    end = (int)Math.Round((double)args[0]);
                }
                else if (args.Count == 2)
                {
                    start = (int)Math.Round((double)args[0]);
                    end = (int)Math.Round((double)args[1]);
                }
                else if (args.Count >= 3)
                {
                    start = (int)Math.Round((double)args[0]);
                    end = (int)Math.Round((double)args[1]);
                    step = (int)Math.Round((double)args[2]);
                }
                
                List<object> result = new List<object>();
                if (step > 0)
                {
                    for (int i = start; i < end; i += step)
                        result.Add((double)i);
                }
                else if (step < 0)
                {
                    for (int i = start; i > end; i += step)
                        result.Add((double)i);
                }
                
                return result;
            });

            // Len function
            interpreter.RegisterBuiltin("len", (args) =>
            {
                if (args.Count < 1)
                    throw new RuntimeError(0, "len() requires 1 argument");
                
                object obj = args[0];
                if (obj is List<object>)
                    return (double)((List<object>)obj).Count;
                else if (obj is string)
                    return (double)((string)obj).Length;
                else if (obj is Dictionary<object, object>)
                    return (double)((Dictionary<object, object>)obj).Count;
                
                throw new RuntimeError(0, "len() argument has no length");
            });

            // Min function
            interpreter.RegisterBuiltin("min", (args) =>
            {
                if (args.Count == 0)
                    throw new RuntimeError(0, "min() requires at least 1 argument");
                
                // Handle list argument
                if (args.Count == 1 && args[0] is List<object>)
                {
                    List<object> list = (List<object>)args[0];
                    if (list.Count == 0)
                        throw new RuntimeError(0, "min() arg is an empty sequence");
                    
                    double minVal = interpreter.ConvertToDouble(list[0]);
                    for (int i = 1; i < list.Count; i++)
                    {
                        double val = interpreter.ConvertToDouble(list[i]);
                        if (val < minVal)
                            minVal = val;
                    }
                    return minVal;
                }
                
                // Handle multiple arguments
                double min = interpreter.ConvertToDouble(args[0]);
                for (int i = 1; i < args.Count; i++)
                {
                    double val = interpreter.ConvertToDouble(args[i]);
                    if (val < min)
                        min = val;
                }
                return min;
            });

            // Max function
            interpreter.RegisterBuiltin("max", (args) =>
            {
                if (args.Count == 0)
                    throw new RuntimeError(0, "max() requires at least 1 argument");
                
                // Handle list argument
                if (args.Count == 1 && args[0] is List<object>)
                {
                    List<object> list = (List<object>)args[0];
                    if (list.Count == 0)
                        throw new RuntimeError(0, "max() arg is an empty sequence");
                    
                    double maxVal = interpreter.ConvertToDouble(list[0]);
                    for (int i = 1; i < list.Count; i++)
                    {
                        double val = interpreter.ConvertToDouble(list[i]);
                        if (val > maxVal)
                            maxVal = val;
                    }
                    return maxVal;
                }
                
                // Handle multiple arguments
                double max = interpreter.ConvertToDouble(args[0]);
                for (int i = 1; i < args.Count; i++)
                {
                    double val = interpreter.ConvertToDouble(args[i]);
                    if (val > max)
                        max = val;
                }
                return max;
            });

            // Abs function
            interpreter.RegisterBuiltin("abs", (args) =>
            {
                if (args.Count < 1)
                    throw new RuntimeError(0, "abs() requires 1 argument");
                
                return Math.Abs(interpreter.ConvertToDouble(args[0]));
            });

            // Str function
            interpreter.RegisterBuiltin("str", (args) =>
            {
                if (args.Count < 1)
                    return "";
                
                return interpreter.ConvertToString(args[0]);
            });

            // Int function
            interpreter.RegisterBuiltin("int", (args) =>
            {
                if (args.Count < 1)
                    return 0.0;
                
                return (double)interpreter.ConvertToInt(args[0]);
            });

            // Float function
            interpreter.RegisterBuiltin("float", (args) =>
            {
                if (args.Count < 1)
                    return 0.0;
                
                return interpreter.ConvertToDouble(args[0]);
            });
        }
        #endregion
    }
}
