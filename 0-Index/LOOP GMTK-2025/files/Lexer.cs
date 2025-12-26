using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PythonInterpreter
{
    /// <summary>
    /// Lexer for Python-like language
    /// Handles indentation-based syntax, keywords, operators, and literals
    /// </summary>
    public class Lexer
    {
        #region Private Fields
        private string source;
        private List<Token> tokens;
        private int start;
        private int current;
        private int line;
        private Stack<int> indentStack;
        
        private static Dictionary<string, TokenType> keywords;
        #endregion

        #region Constructor & Static Initialization
        static Lexer()
        {
            keywords = new Dictionary<string, TokenType>
            {
                { "if", TokenType.IF },
                { "elif", TokenType.ELIF },
                { "else", TokenType.ELSE },
                { "while", TokenType.WHILE },
                { "for", TokenType.FOR },
                { "def", TokenType.DEF },
                { "return", TokenType.RETURN },
                { "class", TokenType.CLASS },
                { "break", TokenType.BREAK },
                { "continue", TokenType.CONTINUE },
                { "pass", TokenType.PASS },
                { "global", TokenType.GLOBAL },
                { "and", TokenType.AND },
                { "or", TokenType.OR },
                { "not", TokenType.NOT },
                { "in", TokenType.IN },
                { "True", TokenType.TRUE },
                { "False", TokenType.FALSE },
                { "None", TokenType.NONE },
                { "lambda", TokenType.LAMBDA }
            };
        }

        public Lexer()
        {
            tokens = new List<Token>();
            indentStack = new Stack<int>();
            indentStack.Push(0); // Base indentation level
        }
        #endregion

        #region Public API
        /// <summary>
        /// Tokenizes the input source code
        /// </summary>
        public List<Token> Tokenize(string sourceCode)
        {
            source = ValidateAndClean(sourceCode);
            tokens = new List<Token>();
            start = 0;
            current = 0;
            line = 1;
            indentStack.Clear();
            indentStack.Push(0);

            bool atLineStart = true;

            while (!IsAtEnd())
            {
                // Handle indentation at the start of non-empty lines
                if (atLineStart && !IsAtEnd())
                {
                    char c = Peek();
                    // Skip empty lines and lines with only whitespace
                    if (c == '\n' || c == '\r')
                    {
                        Advance();
                        if (c == '\n') line++;
                        continue;
                    }

                    // Check for comment lines
                    if (c == '#')
                    {
                        // Skip entire comment line
                        while (!IsAtEnd() && Peek() != '\n')
                            Advance();
                        if (!IsAtEnd() && Peek() == '\n')
                        {
                            Advance();
                            line++;
                        }
                        continue;
                    }

                    // Calculate indentation
                    int indentLevel = 0;
                    while (!IsAtEnd() && Peek() == ' ')
                    {
                        indentLevel++;
                        Advance();
                    }

                    // After counting spaces, check if it's a comment or empty line
                    if (!IsAtEnd() && (Peek() == '#' || Peek() == '\n'))
                    {
                        // Skip comment or empty line
                        while (!IsAtEnd() && Peek() != '\n')
                            Advance();
                        if (!IsAtEnd() && Peek() == '\n')
                        {
                            Advance();
                            line++;
                        }
                        continue;
                    }

                    // Process indentation changes
                    int currentIndent = indentStack.Peek();
                    if (indentLevel > currentIndent)
                    {
                        indentStack.Push(indentLevel);
                        AddToken(TokenType.INDENT);
                    }
                    else if (indentLevel < currentIndent)
                    {
                        // Emit DEDENT tokens
                        while (indentStack.Count > 0 && indentStack.Peek() > indentLevel)
                        {
                            indentStack.Pop();
                            AddToken(TokenType.DEDENT);
                        }

                        if (indentStack.Count == 0 || indentStack.Peek() != indentLevel)
                        {
                            throw new LexerError(line, "Indentation error: inconsistent indentation");
                        }
                    }

                    atLineStart = false;
                }

                start = current;
                ScanToken(ref atLineStart);
            }

            // Emit remaining DEDENT tokens
            while (indentStack.Count > 1)
            {
                indentStack.Pop();
                AddToken(TokenType.DEDENT);
            }

            AddToken(TokenType.EOF);
            return tokens;
        }
        #endregion

        #region Private Methods - Sanitization
        /// <summary>
        /// Validates and cleans the input string
        /// Replaces tabs with spaces, removes BOM and other control characters
        /// </summary>
        private string ValidateAndClean(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            StringBuilder cleaned = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                // Replace CRLF with LF
                if (c == '\r')
                {
                    if (i + 1 < input.Length && input[i + 1] == '\n')
                    {
                        // Skip \r, next iteration will add \n
                        continue;
                    }
                    else
                    {
                        // Standalone \r becomes \n
                        cleaned.Append('\n');
                    }
                }
                // Replace tabs with 4 spaces
                else if (c == '\t')
                {
                    cleaned.Append("    ");
                }
                // Remove BOM and other control characters
                else if (c == '\uFEFF' || c == '\v' || c == '\f')
                {
                    // Skip these characters
                    continue;
                }
                else
                {
                    cleaned.Append(c);
                }
            }

            return cleaned.ToString();
        }
        #endregion

        #region Private Methods - Token Scanning
        private void ScanToken(ref bool atLineStart)
        {
            char c = Advance();

            switch (c)
            {
                // Whitespace (spaces already handled in indentation)
                case ' ':
                    break; // Ignore spaces within lines

                // Newline
                case '\n':
                    AddToken(TokenType.NEWLINE);
                    line++;
                    atLineStart = true;
                    break;

                // Comments
                case '#':
                    // Skip until end of line
                    while (!IsAtEnd() && Peek() != '\n')
                        Advance();
                    break;

                // Operators - Single Character
                case '+':
                    if (Match('='))
                        AddToken(TokenType.PLUS_EQUAL);
                    else
                        AddToken(TokenType.PLUS);
                    break;
                case '-':
                    if (Match('='))
                        AddToken(TokenType.MINUS_EQUAL);
                    else
                        AddToken(TokenType.MINUS);
                    break;
                case '*':
                    if (Match('*'))
                        AddToken(TokenType.POWER);
                    else if (Match('='))
                        AddToken(TokenType.STAR_EQUAL);
                    else
                        AddToken(TokenType.STAR);
                    break;
                case '/':
                    if (Match('='))
                        AddToken(TokenType.SLASH_EQUAL);
                    else
                        AddToken(TokenType.SLASH);
                    break;
                case '%':
                    AddToken(TokenType.PERCENT);
                    break;

                // Comparison Operators
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '!':
                    if (Match('='))
                        AddToken(TokenType.BANG_EQUAL);
                    else
                        throw new LexerError(line, "Unexpected character '!'");
                    break;
                case '<':
                    if (Match('<'))
                        AddToken(TokenType.LEFT_SHIFT);
                    else if (Match('='))
                        AddToken(TokenType.LESS_EQUAL);
                    else
                        AddToken(TokenType.LESS);
                    break;
                case '>':
                    if (Match('>'))
                        AddToken(TokenType.RIGHT_SHIFT);
                    else if (Match('='))
                        AddToken(TokenType.GREATER_EQUAL);
                    else
                        AddToken(TokenType.GREATER);
                    break;

                // Bitwise Operators
                case '&':
                    AddToken(TokenType.AMPERSAND);
                    break;
                case '|':
                    AddToken(TokenType.PIPE);
                    break;
                case '^':
                    AddToken(TokenType.CARET);
                    break;
                case '~':
                    AddToken(TokenType.TILDE);
                    break;

                // Delimiters
                case '(':
                    AddToken(TokenType.LEFT_PAREN);
                    break;
                case ')':
                    AddToken(TokenType.RIGHT_PAREN);
                    break;
                case '[':
                    AddToken(TokenType.LEFT_BRACKET);
                    break;
                case ']':
                    AddToken(TokenType.RIGHT_BRACKET);
                    break;
                case '{':
                    AddToken(TokenType.LEFT_BRACE);
                    break;
                case '}':
                    AddToken(TokenType.RIGHT_BRACE);
                    break;
                case ',':
                    AddToken(TokenType.COMMA);
                    break;
                case ':':
                    AddToken(TokenType.COLON);
                    break;
                case '.':
                    AddToken(TokenType.DOT);
                    break;

                // String Literals
                case '"':
                case '\'':
                    ScanString(c);
                    break;

                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        throw new LexerError(line, "Unexpected character: '" + c + "'");
                    }
                    break;
            }
        }

        private void ScanString(char quote)
        {
            StringBuilder value = new StringBuilder();

            while (!IsAtEnd() && Peek() != quote)
            {
                char c = Advance();
                
                // Handle escape sequences
                if (c == '\\' && !IsAtEnd())
                {
                    char escaped = Advance();
                    switch (escaped)
                    {
                        case 'n': value.Append('\n'); break;
                        case 't': value.Append('\t'); break;
                        case 'r': value.Append('\r'); break;
                        case '\\': value.Append('\\'); break;
                        case '\'': value.Append('\''); break;
                        case '"': value.Append('"'); break;
                        default: 
                            value.Append('\\');
                            value.Append(escaped);
                            break;
                    }
                }
                else
                {
                    if (c == '\n') line++;
                    value.Append(c);
                }
            }

            if (IsAtEnd())
            {
                throw new LexerError(line, "Unterminated string");
            }

            // Consume closing quote
            Advance();

            AddToken(TokenType.STRING, value.ToString());
        }

        private void ScanNumber()
        {
            while (IsDigit(Peek()))
                Advance();

            // Check for decimal point
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the '.'
                Advance();

                while (IsDigit(Peek()))
                    Advance();
            }

            string numberStr = source.Substring(start, current - start);
            double value;
            if (double.TryParse(numberStr, out value))
            {
                AddToken(TokenType.NUMBER, value);
            }
            else
            {
                throw new LexerError(line, "Invalid number format: " + numberStr);
            }
        }

        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek()))
                Advance();

            string text = source.Substring(start, current - start);
            TokenType type;

            if (keywords.TryGetValue(text, out type))
            {
                // Handle boolean and None literals
                if (type == TokenType.TRUE)
                    AddToken(type, true);
                else if (type == TokenType.FALSE)
                    AddToken(type, false);
                else if (type == TokenType.NONE)
                    AddToken(type, null);
                else
                    AddToken(type);
            }
            else
            {
                AddToken(TokenType.IDENTIFIER);
            }
        }
        #endregion

        #region Private Methods - Helpers
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private char Advance()
        {
            return source[current++];
        }

        private bool IsAtEnd()
        {
            return current >= source.Length;
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }
        #endregion
    }

    #region Exception Classes
    /// <summary>
    /// Exception thrown during lexical analysis
    /// </summary>
    public class LexerError : Exception
    {
        public int LineNumber { get; private set; }

        public LexerError(int line, string message) : base(message)
        {
            LineNumber = line;
        }

        public override string ToString()
        {
            return string.Format("LexerError at line {0}: {1}", LineNumber, Message);
        }
    }
    #endregion
}
