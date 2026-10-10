namespace Patra.Core;

public sealed class LexingException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public LexingException(string message, int line, int column)
        : base($"{message} (السطر {line}، العمود {column})")
    {
        Line = line;
        Column = column;
    }
}
