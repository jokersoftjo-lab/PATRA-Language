namespace Patra.Core;

/// <summary>
/// محلل نحوي أولي لتعليمات المتغيرات والإسناد والطباعة والتعبيرات.
/// </summary>
public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _current;

    public Parser(IReadOnlyList<Token> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        if (tokens.Count == 0 || tokens[^1].Kind != TokenKind.EndOfFile)
            throw new ArgumentException("يجب أن تنتهي قائمة الرموز بـ EndOfFile.");

        _tokens = tokens;
    }

    public static IReadOnlyList<Statement> Parse(string source) =>
        new Parser(Lexer.Tokenize(source)).ParseProgram();

    public IReadOnlyList<Statement> ParseProgram()
    {
        var statements = new List<Statement>();

        while (!IsAtEnd)
            statements.Add(ParseStatement());

        return statements.AsReadOnly();
    }

    private Statement ParseStatement()
    {
        if (Check(TokenKind.Identifier) &&
            (Peek().Lexeme == "متغير" || Peek().Lexeme == "let"))
        {
            Advance();
            return ParseVariableDeclaration();
        }

        if (Check(TokenKind.Identifier) &&
            (Peek().Lexeme == "اطبع" || Peek().Lexeme == "print"))
        {
            Advance();
            Expression value = ParseExpression();
            Consume(TokenKind.Semicolon, "توقعنا ';' بعد أمر الطباعة.");
            return new PrintStatement(value);
        }

        if (Check(TokenKind.Identifier) &&
            CheckNext(TokenKind.Assign))
        {
            Token name = Advance();
            Advance(); // =
            Expression value = ParseExpression();
            Consume(TokenKind.Semicolon, "توقعنا ';' بعد الإسناد.");
            return new AssignmentStatement(name.Lexeme, name, value);
        }

        Expression expression = ParseExpression();
        Consume(TokenKind.Semicolon, "توقعنا ';' بعد التعبير.");
        return new ExpressionStatement(expression);
    }

    private Statement ParseVariableDeclaration()
    {
        Token name = Consume(TokenKind.Identifier, "توقعنا اسم المتغير.");
        Consume(TokenKind.Assign, "توقعنا '=' بعد اسم المتغير.");
        Expression initializer = ParseExpression();
        Consume(TokenKind.Semicolon, "توقعنا ';' بعد تعريف المتغير.");

        return new VariableStatement(name.Lexeme, name, initializer);
    }

    private Expression ParseExpression() => ParseEquality();

    private Expression ParseEquality()
    {
        Expression expression = ParseComparison();

        while (Match(TokenKind.Equal, TokenKind.BangEqual))
        {
            Token op = Previous();
            Expression right = ParseComparison();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression ParseComparison()
    {
        Expression expression = ParseTerm();

        while (Match(TokenKind.Greater, TokenKind.GreaterEqual,
                     TokenKind.Less, TokenKind.LessEqual))
        {
            Token op = Previous();
            Expression right = ParseTerm();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression ParseTerm()
    {
        Expression expression = ParseFactor();

        while (Match(TokenKind.Plus, TokenKind.Minus))
        {
            Token op = Previous();
            Expression right = ParseFactor();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression ParseFactor()
    {
        Expression expression = ParseUnary();

        while (Match(TokenKind.Star, TokenKind.Slash, TokenKind.Percent))
        {
            Token op = Previous();
            Expression right = ParseUnary();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression ParseUnary()
    {
        if (Match(TokenKind.Minus, TokenKind.Plus, TokenKind.Bang))
        {
            Token op = Previous();
            return new UnaryExpression(op, ParseUnary());
        }

        return ParsePrimary();
    }

    private Expression ParsePrimary()
    {
        if (Match(TokenKind.Number))
        {
            if (!double.TryParse(
                    Previous().Lexeme,
                    System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double number))
            {
                throw new SyntaxException("رقم غير صالح.", Previous());
            }

            return new LiteralExpression(number);
        }

        if (Match(TokenKind.String))
        {
            string text = Previous().Lexeme[1..^1]
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");

            return new LiteralExpression(text);
        }

        if (Match(TokenKind.Identifier))
            return new VariableExpression(Previous().Lexeme, Previous());

        if (Match(TokenKind.LeftParen))
        {
            Expression expression = ParseExpression();
            Consume(TokenKind.RightParen, "توقعنا ')' لإغلاق التعبير.");
            return expression;
        }

        throw new SyntaxException("تعبير غير متوقع.", Peek());
    }

    private bool IsAtEnd => Peek().Kind == TokenKind.EndOfFile;

    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];

    private Token Advance()
    {
        if (!IsAtEnd)
            _current++;

        return Previous();
    }

    private bool Check(TokenKind kind) => Peek().Kind == kind;

    private bool CheckNext(TokenKind kind) =>
        _current + 1 < _tokens.Count &&
        _tokens[_current + 1].Kind == kind;

    private bool Match(params TokenKind[] kinds)
    {
        foreach (TokenKind kind in kinds)
        {
            if (Check(kind))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Consume(TokenKind kind, string message)
    {
        if (Check(kind))
            return Advance();

        throw new SyntaxException(message, Peek());
    }
}
