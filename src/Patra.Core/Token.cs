namespace Patra.Core;

public sealed record Token(
    TokenKind Kind,
    string Lexeme,
    int Line,
    int Column)
{
    public override string ToString() =>
        $"{Kind} '{Lexeme}' ({Line}:{Column})";
}
