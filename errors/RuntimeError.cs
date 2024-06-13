class RuntimeError : RuntimeException
{
    public Token Token { get; private set; }

    public RuntimeError(Token token, string message) : base(message, token.line)
    {
        Token = token;
    }

    public string GetMessage()
    {
        return base.Message;
    }
}