namespace Patra.Core;

/// <summary>
/// يحوّل نص PATRA إلى رموز أولية.
/// يدعم الحروف العربية والإنجليزية ضمن Unicode BMP.
/// هذه المرحلة لا تحلل القواعد النحوية أو تنفذ البرنامج.
/// </summary>
public sealed class Lexer
{
    private readonly string _source;
    private readonly List<Token> _tokens = [];
    private int _start;
    private int _current;
    private int _line = 1;
    private int _column = 1;
    private int _startLine = 1;
    private int _startColumn = 1;

    public Lexer(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        // توحيد نهايات الأسطر لتبسيط حساب المواقع.
        _source = source.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    public static IReadOnlyList<Token> Tokenize(string source) =>
        new Lexer(source).ScanTokens();

    public IReadOnlyList<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            _start = _current;
            _startLine = _line;
            _startColumn = _column;
            ScanToken();
        }

        _tokens.Add(new Token(
            TokenKind.EndOfFile, string.Empty, _line, _column));

        return _tokens.AsReadOnly();
    }

    private bool IsAtEnd => _current >= _source.Length;

    private char Advance()
    {
        char c = _source[_current++];

        if (c == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        return c;
    }

    private char Peek() => IsAtEnd ? '\0' : _source[_current];

    private char PeekNext() =>
        _current + 1 >= _source.Length ? '\0' : _source[_current + 1];

    private bool Match(char expected)
    {
        if (IsAtEnd || _source[_current] != expected)
            return false;

        Advance();
        return true;
    }

    private void Add(TokenKind kind) =>
        _tokens.Add(new Token(
            kind,
            _source[_start.._current],
            _startLine,
            _startColumn));

    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            case ' ':
            case '\t':
            case '\n':
            case '\f':
                return;

            case '+': Add(TokenKind.Plus); return;
            case '-': Add(TokenKind.Minus); return;
            case '*': Add(TokenKind.Star); return;
            case '%': Add(TokenKind.Percent); return;

            case '/':
                if (Match('/'))
                {
                    while (!IsAtEnd && Peek() != '\n')
                        Advance();
                }
                else
                {
                    Add(TokenKind.Slash);
                }
                return;

            case '=':
                Add(Match('=') ? TokenKind.Equal : TokenKind.Assign);
                return;

            case '!':
                Add(Match('=') ? TokenKind.BangEqual : TokenKind.Bang);
                return;

            case '<':
                Add(Match('=') ? TokenKind.LessEqual : TokenKind.Less);
                return;

            case '>':
                Add(Match('=') ? TokenKind.GreaterEqual : TokenKind.Greater);
                return;

            case '(': Add(TokenKind.LeftParen); return;
            case ')': Add(TokenKind.RightParen); return;
            case '{': Add(TokenKind.LeftBrace); return;
            case '}': Add(TokenKind.RightBrace); return;
            case ',': Add(TokenKind.Comma); return;
            case ';': Add(TokenKind.Semicolon); return;
            case ':': Add(TokenKind.Colon); return;
            case '.':
                if (char.IsDigit(Peek()))
                {
                    ScanNumber();
                }
                else
                {
                    Add(TokenKind.Dot);
                }
                return;

            case '"':
                ScanString();
                return;
        }

        if (char.IsDigit(c))
        {
            ScanNumber();
            return;
        }

        if (IsIdentifierStart(c))
        {
            ScanIdentifier();
            return;
        }

        throw Error($"رمز غير معروف: '{c}'");
    }

    private void ScanIdentifier()
    {
        while (IsIdentifierPart(Peek()))
            Advance();

        Add(TokenKind.Identifier);
    }

    private void ScanNumber()
    {
        while (char.IsDigit(Peek()))
            Advance();

        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance();

            while (char.IsDigit(Peek()))
                Advance();
        }

        Add(TokenKind.Number);
    }

    private void ScanString()
    {
        while (!IsAtEnd && Peek() != '"')
        {
            if (Peek() == '\n')
                throw Error("لا يُسمح بسطر جديد داخل النص");

            if (Peek() == '\\')
            {
                Advance();

                if (IsAtEnd)
                    throw Error("نهاية غير متوقعة داخل النص");

                char escaped = Advance();

                if (escaped is not ('"' or '\\' or 'n' or 'r' or 't'))
                    throw Error($"تسلسل هروب غير مدعوم: '\\{escaped}'");
            }
            else
            {
                Advance();
            }
        }

        if (IsAtEnd)
            throw Error("نص غير مغلق؛ علامة الاقتباس مفقودة");

        Advance(); // علامة الاقتباس الختامية
        Add(TokenKind.String);
    }

    private static bool IsIdentifierStart(char c) =>
        c == '_' || char.IsLetter(c);

    private static bool IsIdentifierPart(char c) =>
        c == '_' || char.IsLetterOrDigit(c);

    private LexingException Error(string message) =>
        new(message, _line, Math.Max(1, _column - 1));
}
