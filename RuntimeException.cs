class RuntimeException : Exception
{
    public int Line { get; }

    public RuntimeException(string message, int line) : base(message)
    {
        Line = line;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, Line: {Line}";
    }
}