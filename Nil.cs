class Nil : Expr
{
    private Nil() { }

    public static Nil Instance { get; } = new();

    public override R Accept<R>(Visitor<R> visitor)
    {
        throw new NotImplementedException();
    }
}