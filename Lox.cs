using System.Text;

class Lox
{
    private static readonly Interpreter interpreter = new Interpreter();
    static bool HadError = false;
    static bool HadRuntimeError = false;
    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("[Usage]: clox [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunFile(string path)
    {
        if (HadError) Environment.Exit(65);
        if (HadRuntimeError) Environment.Exit(70);

        byte[] bytes = File.ReadAllBytes(Path.GetFullPath(path));
        Run(Encoding.Default.GetString(bytes));
    }
    private static void RunPrompt()
    {
        for (; ; )
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) break;
            Run(input);
            HadError = false;
        }
    }
    private static void Run(string source)
    {
        Scanner scanner = new Scanner(source);

        List<Token> tokens = scanner.ScanTokens();

        Parser parser = new Parser(tokens);
        List<Stmt> statements = parser.Parse();

        if (HadError) return;

        interpreter.Interpret(statements);

        foreach (Token token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        HadError = true;
    }

    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, $" at '{token.lexeme}'", message);
        }
    }

    public static void RuntimeError(RuntimeError err)
    {
        Console.Error.WriteLine($"{err.GetMessage()}\n[line {err.Token.line}]");
        HadRuntimeError = true;
    }
}