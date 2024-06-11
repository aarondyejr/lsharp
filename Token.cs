class Token
{
    public TokenType type;
    public string lexeme;
    public object? literal;
    public int line;

    public Token(TokenType t, string lex, object? lit, int lin)
    {
        type = t;
        lexeme = lex;
        literal = lit;
        line = lin;
    }

    // My own ToString method to easily display tokens
    public override string ToString()
    {
        return $"{type} {lexeme} {literal}";
    }
}