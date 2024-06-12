using System.Text;

class AstPrinter : Expr.Visitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return parenthesize(expr.op.lexeme, expr.left, expr.right);
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
        return parenthesize("group", expr.expression);
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.value == null ? "nil" : expr.value.ToString()!;
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return parenthesize(expr.op.lexeme, expr.right);
    }

    private string parenthesize(string name, params Expr[] exprs)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("(").Append(name);

        foreach (Expr expr in exprs)
        {
            builder.Append(" ");
            builder.Append(expr.Accept(this));
        }
        builder.Append(")");

        return builder.ToString();
    }
}