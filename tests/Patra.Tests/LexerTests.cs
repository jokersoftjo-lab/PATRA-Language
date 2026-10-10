using Patra.Core;
using Xunit;

namespace Patra.Tests;

public sealed class LexerTests
{
    [Fact]
    public void TokenizesEnglishIdentifiersAndOperators()
    {
        var tokens = Lexer.Tokenize("name = 12 + 3;");

        Assert.Collection(tokens,
            t => Assert.Equal(TokenKind.Identifier, t.Kind),
            t => Assert.Equal(TokenKind.Assign, t.Kind),
            t => Assert.Equal("12", t.Lexeme),
            t => Assert.Equal(TokenKind.Plus, t.Kind),
            t => Assert.Equal("3", t.Lexeme),
            t => Assert.Equal(TokenKind.Semicolon, t.Kind),
            t => Assert.Equal(TokenKind.EndOfFile, t.Kind));
    }

    [Fact]
    public void TokenizesArabicIdentifiersAndStrings()
    {
        var tokens = Lexer.Tokenize("متغير الاسم = \"بيترا\";");

        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);
        Assert.Equal("متغير", tokens[0].Lexeme);
        Assert.Equal("الاسم", tokens[1].Lexeme);
        Assert.Equal(TokenKind.Assign, tokens[2].Kind);
        Assert.Equal(TokenKind.String, tokens[3].Kind);
        Assert.Equal("\"بيترا\"", tokens[3].Lexeme);
    }

    [Fact]
    public void TokenizesDecimalNumbersAndComparisonOperators()
    {
        var tokens = Lexer.Tokenize("price = 12.50 >= 10;");

        Assert.Contains(tokens, t =>
            t.Kind == TokenKind.Number && t.Lexeme == "12.50");

        Assert.Contains(tokens, t => t.Kind == TokenKind.GreaterEqual);
    }

    [Fact]
    public void SkipsLineComments()
    {
        var tokens = Lexer.Tokenize("name // comment\nvalue");

        Assert.Equal(
            new[] { "name", "value", "" },
            tokens.Select(t => t.Lexeme));
    }

    [Fact]
    public void TracksLineAndColumn()
    {
        var tokens = Lexer.Tokenize("first\n  الاسم");

        Assert.Equal(2, tokens[1].Line);
        Assert.Equal(3, tokens[1].Column);
    }

    [Fact]
    public void ReportsUnclosedString()
    {
        var error = Assert.Throws<LexingException>(
            () => Lexer.Tokenize("\"hello"));

        Assert.Equal(1, error.Line);
        Assert.Contains("غير مغلق", error.Message);
    }

    [Fact]
    public void ReportsUnknownCharacter()
    {
        var error = Assert.Throws<LexingException>(
            () => Lexer.Tokenize("name @ value"));

        Assert.Equal(1, error.Line);
        Assert.Equal(6, error.Column);
    }

    [Fact]
    public void SupportsEscapedQuotesInStrings()
    {
        var tokens = Lexer.Tokenize("\"hello \\\"PATRA\\\"\"");

        Assert.Equal(TokenKind.String, tokens[0].Kind);
        Assert.Equal(TokenKind.EndOfFile, tokens[1].Kind);
    }
}
