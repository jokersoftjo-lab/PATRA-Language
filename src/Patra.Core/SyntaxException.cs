namespace Patra.Core;

public sealed class SyntaxException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public SyntaxException(string message, Token token)
        : base($"{message} (السطر {token.Line}، العمود {token.Column})")
    {
        Line = token.Line;
        Column = token.Column;
    }
}
