class Parser
{
    private class ParserError : RuntimeException
    {
        public ParserError(string message, int line) : base(message, line)
        {
        }
    }
    private readonly List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    private Expr Expression()
    {
        return Equality();
    }

    private Expr Equality()
    {
        Expr expr = Compare();

        while (MatchExpr(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token op = Previous();
            Expr right = Compare();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Compare()
    {

        Expr expr = Term();

        while (MatchExpr(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token op = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (MatchExpr(TokenType.MINUS, TokenType.PLUS))
        {
            Token op = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (MatchExpr(TokenType.STAR, TokenType.SLASH))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (MatchExpr(TokenType.BANG, TokenType.MINUS))
        {
            Token op = Previous();
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (MatchExpr(TokenType.FALSE)) return new Expr.Literal(false);
        if (MatchExpr(TokenType.TRUE)) return new Expr.Literal(true);
        if (MatchExpr(TokenType.NIL)) return new Expr.Literal(Nil.Instance);

        if (MatchExpr(TokenType.NUMBER, TokenType.STRING))
        {
            return new Expr.Literal(Previous().literal);
        }

        if (MatchExpr(TokenType.LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected a ')' after expression");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expected expression");
    }

    public Expr? Parse()
    {
        try
        {
            return Expression();
        }
        catch
        {
            return null;
        }
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    private ParserError Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParserError(message, token.line);
    }

    private void Syncronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().type == TokenType.SEMICOLON) return;

            switch (Peek().type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            Advance();
        }
    }

    private bool MatchExpr(params TokenType[] tokenTypes)
    {
        foreach (TokenType type in tokenTypes)
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
        return Peek().type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().type == TokenType.EOF;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }
}