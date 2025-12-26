using System;

namespace PythonInterpreter
{
    /// <summary>
    /// Token types for the Python-like language lexer
    /// </summary>
    public enum TokenType
    {
        // ===== SPECIAL TOKENS =====
        EOF,
        NEWLINE,
        INDENT,
        DEDENT,

        // ===== LITERALS =====
        NUMBER,
        STRING,
        IDENTIFIER,

        // ===== KEYWORDS =====
        IF,
        ELIF,
        ELSE,
        WHILE,
        FOR,
        DEF,
        RETURN,
        CLASS,
        BREAK,
        CONTINUE,
        PASS,
        GLOBAL,
        AND,
        OR,
        NOT,
        IN,
        TRUE,
        FALSE,
        NONE,
        LAMBDA,

        // ===== OPERATORS - ARITHMETIC =====
        PLUS,           // +
        MINUS,          // -
        STAR,           // *
        SLASH,          // /
        PERCENT,        // %
        POWER,          // **

        // ===== OPERATORS - COMPARISON =====
        EQUAL_EQUAL,    // ==
        BANG_EQUAL,     // !=
        LESS,           // <
        GREATER,        // >
        LESS_EQUAL,     // <=
        GREATER_EQUAL,  // >=

        // ===== OPERATORS - ASSIGNMENT =====
        EQUAL,          // =
        PLUS_EQUAL,     // +=
        MINUS_EQUAL,    // -=
        STAR_EQUAL,     // *=
        SLASH_EQUAL,    // /=

        // ===== OPERATORS - BITWISE =====
        AMPERSAND,      // &
        PIPE,           // |
        CARET,          // ^
        TILDE,          // ~
        LEFT_SHIFT,     // <<
        RIGHT_SHIFT,    // >>

        // ===== DELIMITERS =====
        LEFT_PAREN,     // (
        RIGHT_PAREN,    // )
        LEFT_BRACKET,   // [
        RIGHT_BRACKET,  // ]
        LEFT_BRACE,     // {
        RIGHT_BRACE,    // }
        COMMA,          // ,
        COLON,          // :
        DOT,            // .
    }
}
