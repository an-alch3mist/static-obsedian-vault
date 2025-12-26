using System;

namespace PythonInterpreter
{
    /// <summary>
    /// Represents a single token from the lexer
    /// Stores type, lexeme, literal value, and line number for error reporting
    /// </summary>
    public class Token
    {
        #region Public Properties
        public TokenType Type { get; private set; }
        public string Lexeme { get; private set; }
        public object Literal { get; private set; }
        public int LineNumber { get; private set; }
        #endregion

        #region Constructor
        public Token(TokenType type, string lexeme, object literal, int lineNumber)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            LineNumber = lineNumber;
        }
        #endregion

        #region Public API
        public override string ToString()
        {
            string literalStr = Literal != null ? Literal.ToString() : "null";
            return string.Format("[Line {0}] {1} '{2}' ({3})", LineNumber, Type, Lexeme, literalStr);
        }
        #endregion
    }
}
