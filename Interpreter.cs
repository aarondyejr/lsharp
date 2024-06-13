class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
{

    private Env env = new Env();

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError err)
        {
            Lox.RuntimeError(err);
        }
    }
    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.value!;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.expression);
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        object right = Evaluate(expr.right);

        switch (expr.op.type)
        {
            case TokenType.BANG:
                return !IsTruthy(right);
            case TokenType.MINUS:
                CheckNumberOperands(expr.op, right);
                return -(double)right;
        }

        return Nil.Instance;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        object left = Evaluate(expr.left);
        object right = Evaluate(expr.right);

        switch (expr.op.type)
        {
            case TokenType.MINUS:
                CheckNumberOperands(expr.op, left, right);
                return (double)left - (double)right;
            case TokenType.SLASH:
                CheckNumberOperands(expr.op, left, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                CheckNumberOperands(expr.op, left, right);
                return (double)left * (double)right;
            case TokenType.GREATER:
                CheckNumberOperands(expr.op, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr.op, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(expr.op, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr.op, left, right);
                return (double)left <= (double)right;
            case TokenType.BANG_EQUAL:
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
            case TokenType.PLUS:
                if (left is double v1 && right is double v2)
                {
                    return v1 + v2;
                }

                if (left is string v3 && right is string v4)
                {
                    return v3 + v4;
                }
                throw new RuntimeError(expr.op, "Operands must be two numbers or two strings.");
        }

        return Nil.Instance;
    }

    public object VisitExpressionStmt(Stmt.Expression statement)
    {
        Evaluate(statement.expression);
        return Nil.Instance;
    }
    public object VisitPrintStmt(Stmt.Print statement)
    {
        object value = Evaluate(statement.expression);
        Console.WriteLine(Stringify(value));
        return Nil.Instance;
    }

    public object VisitVarStmt(Stmt.Var statement)
    {
        object? value = Nil.Instance;
        if (statement.initializer != Nil.Instance)
        {
            value = Evaluate(statement.initializer);
        }

        env.Define(statement.name.lexeme, value);
        return Nil.Instance;
    }

    public object VisitVariableExpr(Expr.Variable variable)
    {
        return env.Get(variable.name);
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private bool IsTruthy(object value)
    {
        if (value == Nil.Instance) return false;
        if (value is bool) return (bool)value;

        return true;
    }

    private bool IsEqual(object left, object right)
    {
        if (left == Nil.Instance && right == Nil.Instance) return true;
        if (left == Nil.Instance) return false;

        return left.Equals(right);
    }
    private void CheckNumberOperands(Token token, object right)
    {
        if (right is double) return;
        throw new RuntimeError(token, "Operand must be a number.");
    }
    private void CheckNumberOperands(Token token, object left, object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(token, "Operands must be numbers.");
    }

    private string Stringify(object obj)
    {
        if (obj == Nil.Instance) return "nil";

        if (obj is double)
        {
            string text = obj.ToString()!;

            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }
            return text;
        }

        return obj.ToString()!;
    }

    private void Execute(Stmt statement)
    {
        statement.Accept(this);
    }
}