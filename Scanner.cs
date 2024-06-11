using System.Diagnostics.Tracing;

class Scanner
{
    private string Source;
    private List<Token> tokens = new List<Token>();

    private int start = 0;
    private int current = 0;
    private int line = 1;

    // Make a Dictionary of reserved keywords for us to fetch when lexing reserved identifiers.
    public static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
    {
        {"and", TokenType.AND},
        {"class", TokenType.CLASS},
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"for", TokenType.FOR},
        {"fun", TokenType.FUN},
        {"if", TokenType.IF},
        {"nil", TokenType.NIL},
        {"or", TokenType.OR},
        {"print", TokenType.PRINT},
        {"return", TokenType.RETURN},
        {"super", TokenType.SUPER},
        {"this", TokenType.THIS},
        {"true", TokenType.TRUE},
        {"var", TokenType.VAR},
        {"while", TokenType.WHILE},
    };

    public Scanner(string source)
    {
        Source = source;
    }

    // Main function that loops over the source checking if it is at the end
    // If it is not at the end it will set start to be the current position of what is being tokenized then call ScanTokens
    // After that when it finally reaches the end it adds the EOF token setting the type, lexeme, literal and line
    // Finally it returns the tokens.
    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(TokenType.EOF, "", null, line));

        return tokens;
    }

    // This private method is the bread and butter of the entire lexer
    // It's main job is to scan the tokens in the source.
    // What it first does is calls Advance (more on what this does later) saving the result to a variable
    // It then uses that variable in a switch/case statement that handles lexing chars for multiple single-line tokens.
    // Now for tokens that can be used in a number of ways such as ! can be used as != or = can be used as ==
    // We call a Match function to handle checking if the next char after that is an accepted combo char.

    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '+': AddToken(TokenType.PLUS); break;
            case '*': AddToken(TokenType.STAR); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '.': AddToken(TokenType.DOT); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
            case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
            case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
            case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(TokenType.SLASH);
                }
                break;
            // We handle whitespace and other special whitespace characters by ignoring them. They aren't important in lexing
            case ' ':
            case '\r':
            case '\t':
                break;
            // If we run into a new line, we simply incremement the line variable.
            case '\n':
                line++;
                break;
            case '"': HandleString(); break;
            default:

                // Here we perform a number of checks so we can handle Numbers AND Identifiers.
                // We also gracefully send an error if none of the cases fall through and the two if/else if fail as well.
                // It is important to note, we do not stop lexing of the source
                // We do this so we can make sure there are no other errors in the source provided.
                // This prevents annoying situations where the dev fixes one error, only to be greeted by another immediately after
                // A Error rabbit-hole if you will.
                if (IsDigit(c))
                {
                    HandleNumber();
                }
                else if (IsAlpha(c))
                {
                    HandleIdentifer();
                }
                else
                {
                    Lox.Error(line, "Unpexected character.");
                }
                break;

        }
    }

    // From my understanding what we are essentially doing is is grabbing the "current" char in the Source we are lexing
    // While also incrementing it making sure it moves along and in turn consuming it
    private char Advance()
    {
        return Source[current++];
    }

    // Here we are just setting a base AddToken method for when we only care about the Type
    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    // Here we are making an overload to the AddToken method
    // What we are doing is grabbing a SubString of the source, the starting position being well start
    // Remember start is set to be start = current which is updated on each while loop invokation in `ScanTokens`
    // We then set the ending positon to be the current place we are lexing, minus the start
    // I am a little fuzzy on why this is the case so whoever reads this please reach out to me at misty.dev and give me an explanation
    private void AddToken(TokenType type, object? literal)
    {
        string text = Source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }

    // This method here is very important, it is the backbones of parsing combo tokens e.g !=, <=, >= and even // for comments
    // What it does is check if its at the end, if so it will return fals meaning that was the end of the source; 
    // there is nothing left to lex. It then checks to see if what we are currently looking at is the expected char
    // If not then it returns false and increments current returning true
    // The reason it increments current is because its safe to move on and that it does match and it also makes sure that the = in !=
    // Is not consumed by the lexer on tis own allowing the token to be comboed like BANG_EQUAL != instead of BANG ! EQUAL = 
    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (Source[current] != expected) return false;

        current++;
        return true;
    }

    // This allows us perform a 1 depth lookahead without consuming the character
    // This is useful for performing checks such as seeing if a string starts and ends with a " or if the next char in a source is a number, etc
    // It also checks if it is at the end of a source and if so it returns a \0 which is a string termininating character in C/C++/C#
    // It then returns the char at the Source location you are peeking at 
    // NOTE: once again reach out to me at misty.dev if you can explain how Source[current] shows the next unconsumed char
    private char Peek()
    {
        if (IsAtEnd()) return '\0';

        return Source[current];
    }

    // Handle string is rather straight forward if you take the time to look at it
    // Essentially what is going on is we are saying
    // while next character is not a " and its not at the end, we then check if the next character is a new line if so increment line and Advance consuming the \n
    // If the while loop finally returns false we break out of it and check if we are at the end once again
    // The reason for this is because multi-line strings are supported simply because it is too annoying to prevent them
    // This means that cases like var x = "Hello, world are possible (note the missing ending ") so we gracefully send an error
    // stopping execution of adding the token
    // Now if there is no problems it will Advance consuming the closing "
    // Then it will get a substring at the start position + 1 (so it ignores the opening ") 
    // then the ending positon of current - start - 2 (so it ignores the ending ") (ahah again please reach out if you can explain current - start - 2 here)
    // I know the - 2 is to ignore the closing position but not current - start
    // It will then add a token with type STRING and that string value.
    private void HandleString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Lox.Error(line, "Unpexected string.");
            return;
        }

        // The closing "
        Advance();

        // Trim surrounding quotes
        string value = Source.Substring(start + 1, current - start - 2);
        AddToken(TokenType.STRING, value);
    }

    // HandleNumber is fundamentally the same as HandleString with some slight differences
    // What we do is peek into the next chars checking if its a digit, and if so consuming it until we don't have anymore
    // We then also check if there is a `.` as ints in this language are just floating-point integers meaning they can also be doubles
    // If there is, and the next char after the `.` is a number then we advance while also performing that same while loop
    // consuming the numbers until there are none left. I will explain more on what PeekNext does in the comments of that method.
    // Like always we add the token parsing the substring into a double once again using the start positon, and the end position being current - start

    private void HandleNumber()
    {
        while (IsDigit(Peek()))
        {
            Advance();
        };

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.NUMBER, Double.Parse(Source.Substring(start, current - start)));
    }

    // HandleIdentifer is a little special, it needs to be able to check that identifiers programmers set are not reserved
    // It does this by first checking if the char in the identifier is AlphaNumberic e.g A-Za-z0-9 while also support _
    // If so it will Advance consuming that char and then creating a substring in a similar way to HandleNumber
    // The key is the check though, it makes a call to the TryGetValue on the Dictionary class and checks if the bool returned from it is false
    // If it is, it sets tokenType (the out value of the TryGetValue method) to be an identifer, because it doesn't exist as a reserved keyword
    // it is fine to classify it as a identifier else it is a reserved keyword. We then add the token.
    // One thing to note for anyone reading this, I am new to C# and the output of parsing a identifer is 
    // IDENTIFIER x which makes me curious as I dont think I specify the identifier lexeme unless it is some sort of function overload mumbo jumbo 
    // reach out at misty.dev if you know the answer!

    private void HandleIdentifer()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        string text = Source.Substring(start, current - start);

        if (keywords.TryGetValue(text, out TokenType tokenType) == false) tokenType = TokenType.IDENTIFIER;

        AddToken(tokenType);
    }

    // Right so PeekNext is literally just Peek, but adding 1 to also get the character after the one you are currently consuming.
    private char PeekNext()
    {
        if (current + 1 >= Source.Length) return '\0';

        return Source[current + 1];
    }

    // I know char.IsDigit exists I just wanted to be different
    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    // Same with this method, I know you can easily check this on char class but wanted to be different :p.
    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    }

    // Checks if the char is both Alphabetical + _ OR A number/digit
    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    // As the name implies it checks if the current is greater than or equal to the length of the source, if so then it is at the end.
    private bool IsAtEnd()
    {
        return current >= Source.Length;
    }
}
