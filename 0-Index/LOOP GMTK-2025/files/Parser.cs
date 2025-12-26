using System;
using System.Collections.Generic;
using UnityEngine;

namespace PythonInterpreter
{
    /// <summary>
    /// Recursive descent parser for Python-like language
    /// Handles operator precedence, list comprehensions, lambdas, and indentation-based syntax
    /// </summary>
    public class Parser
    {
        #region Private Fields
        private List<Token> tokens;
        private int current;
        #endregion

        #region Public API
        /// <summary>
        /// Parses a list of tokens into an AST
        /// </summary>
        public List<Stmt> Parse(List<Token> tokens)
        {
            this.tokens = tokens;
            this.current = 0;

            List<Stmt> statements = new List<Stmt>();

            while (!IsAtEnd())
            {
                // Skip newlines at the top level
                if (Match(TokenType.NEWLINE))
                    continue;

                try
                {
                    Stmt stmt = ParseStatement();
                    if (stmt != null)
                        statements.Add(stmt);
                }
                catch (ParserError)
                {
                    // Re-throw parser errors
                    throw;
                }
            }

            return statements;
        }
        #endregion

        #region Private Methods - Statement Parsing
        private Stmt ParseStatement()
        {
            try
            {
                // Skip newlines
                while (Match(TokenType.NEWLINE)) { }

                if (IsAtEnd()) return null;

                int lineNum = Peek().LineNumber;

                // Control flow
                if (Match(TokenType.IF)) return ParseIfStatement();
                if (Match(TokenType.WHILE)) return ParseWhileStatement();
                if (Match(TokenType.FOR)) return ParseForStatement();
                if (Match(TokenType.DEF)) return ParseFunctionDef();
                if (Match(TokenType.CLASS)) return ParseClassDef();
                if (Match(TokenType.RETURN)) return ParseReturnStatement();
                if (Match(TokenType.BREAK)) return ParseBreakStatement();
                if (Match(TokenType.CONTINUE)) return ParseContinueStatement();
                if (Match(TokenType.PASS)) return ParsePassStatement();
                if (Match(TokenType.GLOBAL)) return ParseGlobalStatement();

                // Assignment or expression statement
                return ParseExpressionStatement();
            }
            catch (ParserError)
            {
                throw;
            }
        }

        private Stmt ParseIfStatement()
        {
            int lineNum = Previous().LineNumber;
            
            Expr condition = ParseExpression();
            Consume(TokenType.COLON, "Expected ':' after if condition");
            ConsumeNewline();
            Consume(TokenType.INDENT, "Expected indented block after ':'");
            
            List<Stmt> thenBranch = ParseBlock();
            
            List<Tuple<Expr, List<Stmt>>> elifBranches = new List<Tuple<Expr, List<Stmt>>>();
            List<Stmt> elseBranch = null;

            // Handle elif branches
            while (Check(TokenType.ELIF))
            {
                Advance(); // Consume 'elif'
                Expr elifCondition = ParseExpression();
                Consume(TokenType.COLON, "Expected ':' after elif condition");
                ConsumeNewline();
                Consume(TokenType.INDENT, "Expected indented block after ':'");
                List<Stmt> elifBody = ParseBlock();
                elifBranches.Add(new Tuple<Expr, List<Stmt>>(elifCondition, elifBody));
            }

            // Handle else branch
            if (Match(TokenType.ELSE))
            {
                Consume(TokenType.COLON, "Expected ':' after else");
                ConsumeNewline();
                Consume(TokenType.INDENT, "Expected indented block after ':'");
                elseBranch = ParseBlock();
            }

            IfStmt stmt = new IfStmt(condition, thenBranch, elifBranches, elseBranch);
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParseWhileStatement()
        {
            int lineNum = Previous().LineNumber;
            
            Expr condition = ParseExpression();
            Consume(TokenType.COLON, "Expected ':' after while condition");
            ConsumeNewline();
            Consume(TokenType.INDENT, "Expected indented block after ':'");
            
            List<Stmt> body = ParseBlock();

            WhileStmt stmt = new WhileStmt(condition, body);
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParseForStatement()
        {
            int lineNum = Previous().LineNumber;
            
            Token varToken = Consume(TokenType.IDENTIFIER, "Expected variable name after 'for'");
            string variable = varToken.Lexeme;
            
            Consume(TokenType.IN, "Expected 'in' after for variable");
            Expr iterable = ParseExpression();
            Consume(TokenType.COLON, "Expected ':' after for clause");
            ConsumeNewline();
            Consume(TokenType.INDENT, "Expected indented block after ':'");
            
            List<Stmt> body = ParseBlock();

            ForStmt stmt = new ForStmt(variable, iterable, body);
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParseFunctionDef()
        {
            int lineNum = Previous().LineNumber;
            
            Token nameToken = Consume(TokenType.IDENTIFIER, "Expected function name");
            string name = nameToken.Lexeme;
            
            Consume(TokenType.LEFT_PAREN, "Expected '(' after function name");
            
            List<string> parameters = new List<string>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    Token param = Consume(TokenType.IDENTIFIER, "Expected parameter name");
                    parameters.Add(param.Lexeme);
                } while (Match(TokenType.COMMA));
            }
            
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameters");
            Consume(TokenType.COLON, "Expected ':' after function signature");
            ConsumeNewline();
            Consume(TokenType.INDENT, "Expected indented block after ':'");
            
            List<Stmt> body = ParseBlock();

            FunctionDef stmt = new FunctionDef(name, parameters, body);
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParseClassDef()
        {
            int lineNum = Previous().LineNumber;
            
            Token nameToken = Consume(TokenType.IDENTIFIER, "Expected class name");
            string name = nameToken.Lexeme;
            
            Consume(TokenType.COLON, "Expected ':' after class name");
            ConsumeNewline();
            Consume(TokenType.INDENT, "Expected indented block after ':'");
            
            List<FunctionDef> methods = new List<FunctionDef>();
            
            while (!Check(TokenType.DEDENT) && !IsAtEnd())
            {
                while (Match(TokenType.NEWLINE)) { }
                
                if (Check(TokenType.DEDENT)) break;
                
                if (Match(TokenType.DEF))
                {
                    FunctionDef method = ParseFunctionDef() as FunctionDef;
                    methods.Add(method);
                }
                else
                {
                    throw new ParserError(Peek(), "Only function definitions allowed in class body");
                }
            }
            
            Consume(TokenType.DEDENT, "Expected dedent after class body");

            ClassDef stmt = new ClassDef(name, methods);
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParseReturnStatement()
        {
            int lineNum = Previous().LineNumber;
            
            Expr value = null;
            if (!Check(TokenType.NEWLINE) && !IsAtEnd())
            {
                value = ParseExpression();
            }

            ReturnStmt stmt = new ReturnStmt(value);
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParseBreakStatement()
        {
            int lineNum = Previous().LineNumber;
            BreakStmt stmt = new BreakStmt();
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParseContinueStatement()
        {
            int lineNum = Previous().LineNumber;
            ContinueStmt stmt = new ContinueStmt();
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParsePassStatement()
        {
            int lineNum = Previous().LineNumber;
            PassStmt stmt = new PassStmt();
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParseGlobalStatement()
        {
            int lineNum = Previous().LineNumber;
            
            List<string> names = new List<string>();
            do
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expected variable name");
                names.Add(name.Lexeme);
            } while (Match(TokenType.COMMA));

            GlobalStmt stmt = new GlobalStmt(names);
            stmt.LineNumber = lineNum;
            return stmt;
        }

        private Stmt ParseExpressionStatement()
        {
            int lineNum = Peek().LineNumber;
            Expr expr = ParseExpression();

            // Check for assignment
            if (Match(TokenType.EQUAL, TokenType.PLUS_EQUAL, TokenType.MINUS_EQUAL, 
                      TokenType.STAR_EQUAL, TokenType.SLASH_EQUAL))
            {
                TokenType op = Previous().Type;
                Expr value = ParseExpression();

                // Handle compound assignment (e.g., +=, -=)
                if (op != TokenType.EQUAL)
                {
                    TokenType binaryOp = TokenType.PLUS;
                    if (op == TokenType.PLUS_EQUAL) binaryOp = TokenType.PLUS;
                    else if (op == TokenType.MINUS_EQUAL) binaryOp = TokenType.MINUS;
                    else if (op == TokenType.STAR_EQUAL) binaryOp = TokenType.STAR;
                    else if (op == TokenType.SLASH_EQUAL) binaryOp = TokenType.SLASH;

                    value = new BinaryExpr(expr, binaryOp, value);
                    value.LineNumber = lineNum;
                }

                AssignStmt stmt = new AssignStmt(expr, value);
                stmt.LineNumber = lineNum;
                return stmt;
            }

            ExprStmt stmt2 = new ExprStmt(expr);
            stmt2.LineNumber = lineNum;
            return stmt2;
        }

        private List<Stmt> ParseBlock()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.DEDENT) && !IsAtEnd())
            {
                // Skip empty lines
                if (Match(TokenType.NEWLINE))
                    continue;

                Stmt stmt = ParseStatement();
                if (stmt != null)
                    statements.Add(stmt);
            }

            Consume(TokenType.DEDENT, "Expected dedent after block");

            return statements;
        }
        #endregion

        #region Private Methods - Expression Parsing
        private Expr ParseExpression()
        {
            return ParseLogicOr();
        }

        private Expr ParseLogicOr()
        {
            Expr expr = ParseLogicAnd();

            while (Match(TokenType.OR))
            {
                int lineNum = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseLogicAnd();
                expr = new BinaryExpr(expr, op, right);
                expr.LineNumber = lineNum;
            }

            return expr;
        }

        private Expr ParseLogicAnd()
        {
            Expr expr = ParseBitwise();

            while (Match(TokenType.AND))
            {
                int lineNum = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseBitwise();
                expr = new BinaryExpr(expr, op, right);
                expr.LineNumber = lineNum;
            }

            return expr;
        }

        private Expr ParseBitwise()
        {
            Expr expr = ParseEquality();

            while (Match(TokenType.AMPERSAND, TokenType.PIPE, TokenType.CARET, 
                         TokenType.LEFT_SHIFT, TokenType.RIGHT_SHIFT))
            {
                int lineNum = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseEquality();
                expr = new BinaryExpr(expr, op, right);
                expr.LineNumber = lineNum;
            }

            return expr;
        }

        private Expr ParseEquality()
        {
            Expr expr = ParseComparison();

            while (Match(TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL))
            {
                int lineNum = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseComparison();
                expr = new BinaryExpr(expr, op, right);
                expr.LineNumber = lineNum;
            }

            return expr;
        }

        private Expr ParseComparison()
        {
            Expr expr = ParseTerm();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, 
                         TokenType.LESS, TokenType.LESS_EQUAL, TokenType.IN))
            {
                int lineNum = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseTerm();
                expr = new BinaryExpr(expr, op, right);
                expr.LineNumber = lineNum;
            }

            return expr;
        }

        private Expr ParseTerm()
        {
            Expr expr = ParseFactor();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                int lineNum = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseFactor();
                expr = new BinaryExpr(expr, op, right);
                expr.LineNumber = lineNum;
            }

            return expr;
        }

        private Expr ParseFactor()
        {
            Expr expr = ParsePower();

            while (Match(TokenType.STAR, TokenType.SLASH, TokenType.PERCENT))
            {
                int lineNum = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParsePower();
                expr = new BinaryExpr(expr, op, right);
                expr.LineNumber = lineNum;
            }

            return expr;
        }

        private Expr ParsePower()
        {
            Expr expr = ParseUnary();

            if (Match(TokenType.POWER))
            {
                int lineNum = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParsePower(); // Right associative
                expr = new BinaryExpr(expr, op, right);
                expr.LineNumber = lineNum;
            }

            return expr;
        }

        private Expr ParseUnary()
        {
            if (Match(TokenType.NOT, TokenType.MINUS, TokenType.TILDE))
            {
                int lineNum = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseUnary();
                UnaryExpr expr = new UnaryExpr(op, right);
                expr.LineNumber = lineNum;
                return expr;
            }

            return ParseCall();
        }

        private Expr ParseCall()
        {
            Expr expr = ParsePrimary();

            while (true)
            {
                int lineNum = Peek().LineNumber;

                if (Match(TokenType.LEFT_PAREN))
                {
                    List<Expr> arguments = new List<Expr>();
                    if (!Check(TokenType.RIGHT_PAREN))
                    {
                        do
                        {
                            arguments.Add(ParseExpression());
                        } while (Match(TokenType.COMMA));
                    }
                    Consume(TokenType.RIGHT_PAREN, "Expected ')' after arguments");
                    expr = new CallExpr(expr, arguments);
                    expr.LineNumber = lineNum;
                }
                else if (Match(TokenType.DOT))
                {
                    Token name = Consume(TokenType.IDENTIFIER, "Expected property name after '.'");
                    expr = new GetExpr(expr, name.Lexeme);
                    expr.LineNumber = lineNum;
                }
                else if (Match(TokenType.LEFT_BRACKET))
                {
                    Expr index = ParseExpression();
                    Expr endIndex = null;

                    // Check for slice
                    if (Match(TokenType.COLON))
                    {
                        if (!Check(TokenType.RIGHT_BRACKET))
                        {
                            endIndex = ParseExpression();
                        }
                        Consume(TokenType.RIGHT_BRACKET, "Expected ']' after slice");
                        expr = new SliceExpr(expr, index, endIndex);
                    }
                    else
                    {
                        Consume(TokenType.RIGHT_BRACKET, "Expected ']' after index");
                        expr = new SliceExpr(expr, index);
                    }
                    expr.LineNumber = lineNum;
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr ParsePrimary()
        {
            int lineNum = Peek().LineNumber;

            // Literals
            if (Match(TokenType.TRUE))
            {
                LiteralExpr expr = new LiteralExpr(true);
                expr.LineNumber = lineNum;
                return expr;
            }
            if (Match(TokenType.FALSE))
            {
                LiteralExpr expr = new LiteralExpr(false);
                expr.LineNumber = lineNum;
                return expr;
            }
            if (Match(TokenType.NONE))
            {
                LiteralExpr expr = new LiteralExpr(null);
                expr.LineNumber = lineNum;
                return expr;
            }

            if (Match(TokenType.NUMBER))
            {
                LiteralExpr expr = new LiteralExpr(Previous().Literal);
                expr.LineNumber = lineNum;
                return expr;
            }

            if (Match(TokenType.STRING))
            {
                LiteralExpr expr = new LiteralExpr(Previous().Literal);
                expr.LineNumber = lineNum;
                return expr;
            }

            // Lambda
            if (Match(TokenType.LAMBDA))
            {
                return ParseLambda();
            }

            // Identifier
            if (Match(TokenType.IDENTIFIER))
            {
                string name = Previous().Lexeme;
                
                // Check for 'self'
                if (name == "self")
                {
                    SelfExpr expr = new SelfExpr();
                    expr.LineNumber = lineNum;
                    return expr;
                }

                VariableExpr expr2 = new VariableExpr(name);
                expr2.LineNumber = lineNum;
                return expr2;
            }

            // List
            if (Match(TokenType.LEFT_BRACKET))
            {
                return ParseList();
            }

            // Dictionary
            if (Match(TokenType.LEFT_BRACE))
            {
                return ParseDict();
            }

            // Grouped expression
            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression");
                return expr;
            }

            throw new ParserError(Peek(), "Expected expression");
        }

        private Expr ParseList()
        {
            int lineNum = Previous().LineNumber;
            List<Expr> elements = new List<Expr>();

            if (!Check(TokenType.RIGHT_BRACKET))
            {
                // Parse first element
                Expr firstElement = ParseExpression();

                // Check for list comprehension
                if (Check(TokenType.FOR))
                {
                    Advance(); // Consume 'for'
                    Token varToken = Consume(TokenType.IDENTIFIER, "Expected variable name");
                    string variable = varToken.Lexeme;
                    Consume(TokenType.IN, "Expected 'in' after variable");
                    Expr iterable = ParseExpression();
                    
                    Expr condition = null;
                    if (Match(TokenType.IF))
                    {
                        condition = ParseExpression();
                    }

                    Consume(TokenType.RIGHT_BRACKET, "Expected ']' after list comprehension");
                    
                    ListCompExpr expr = new ListCompExpr(firstElement, variable, iterable, condition);
                    expr.LineNumber = lineNum;
                    return expr;
                }

                // Regular list
                elements.Add(firstElement);
                while (Match(TokenType.COMMA))
                {
                    if (Check(TokenType.RIGHT_BRACKET)) break;
                    elements.Add(ParseExpression());
                }
            }

            Consume(TokenType.RIGHT_BRACKET, "Expected ']' after list elements");
            
            ListExpr listExpr = new ListExpr(elements);
            listExpr.LineNumber = lineNum;
            return listExpr;
        }

        private Expr ParseDict()
        {
            int lineNum = Previous().LineNumber;
            List<Tuple<Expr, Expr>> pairs = new List<Tuple<Expr, Expr>>();

            if (!Check(TokenType.RIGHT_BRACE))
            {
                do
                {
                    if (Check(TokenType.RIGHT_BRACE)) break;
                    
                    Expr key = ParseExpression();
                    Consume(TokenType.COLON, "Expected ':' after dictionary key");
                    Expr value = ParseExpression();
                    
                    pairs.Add(new Tuple<Expr, Expr>(key, value));
                } while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_BRACE, "Expected '}' after dictionary");
            
            DictExpr expr = new DictExpr(pairs);
            expr.LineNumber = lineNum;
            return expr;
        }

        private Expr ParseLambda()
        {
            int lineNum = Previous().LineNumber;
            
            List<string> parameters = new List<string>();
            
            // Parse parameters (no parentheses)
            if (!Check(TokenType.COLON))
            {
                do
                {
                    Token param = Consume(TokenType.IDENTIFIER, "Expected parameter name");
                    parameters.Add(param.Lexeme);
                } while (Match(TokenType.COMMA));
            }
            
            Consume(TokenType.COLON, "Expected ':' after lambda parameters");
            
            // Parse single expression
            Expr body = ParseExpression();
            
            LambdaExpr expr = new LambdaExpr(parameters, body);
            expr.LineNumber = lineNum;
            return expr;
        }
        #endregion

        #region Private Methods - Utilities
        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return current >= tokens.Count || Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            if (current >= tokens.Count) 
                return tokens[tokens.Count - 1]; // Return EOF
            return tokens[current];
        }

        private Token Previous()
        {
            return tokens[current - 1];
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw new ParserError(Peek(), message);
        }

        private void ConsumeNewline()
        {
            while (Match(TokenType.NEWLINE)) { }
        }
        #endregion
    }

    #region Exception Classes
    /// <summary>
    /// Exception thrown during parsing
    /// </summary>
    public class ParserError : Exception
    {
        public Token Token { get; private set; }

        public ParserError(Token token, string message) : base(message)
        {
            Token = token;
        }

        public override string ToString()
        {
            return string.Format("ParserError at line {0}: {1}", Token.LineNumber, Message);
        }
    }
    #endregion
}
