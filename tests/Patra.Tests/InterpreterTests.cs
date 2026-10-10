using Patra.Core;
using Xunit;

namespace Patra.Tests;

public sealed class InterpreterTests
{
    [Fact]
    public void EvaluatesArithmeticWithCorrectPrecedence()
    {
        var interpreter = new Interpreter();

        var output = interpreter.Execute(
            "متغير الناتج = (10 + 5) * 2; اطبع الناتج;");

        Assert.Equal(new[] { "30" }, output);
    }

    [Fact]
    public void SupportsEnglishAndArabicVariableKeywords()
    {
        var interpreter = new Interpreter();

        var output = interpreter.Execute(
            "let score = 20; متغير النتيجة = score + 2; " +
            "print score; اطبع النتيجة;");

        Assert.Equal(new[] { "20", "22" }, output);
    }

    [Fact]
    public void SupportsAssignment()
    {
        var interpreter = new Interpreter();

        var output = interpreter.Execute(
            "متغير العدد = 4; العدد = العدد + 3; اطبع العدد;");

        Assert.Equal(new[] { "7" }, output);
    }

    [Fact]
    public void ConcatenatesArabicStrings()
    {
        var interpreter = new Interpreter();

        var output = interpreter.Execute(
            "متغير الاسم = \"بيترا\"; اطبع الاسم + \" لغة\";");

        Assert.Equal(new[] { "بيترا لغة" }, output);
    }

    [Fact]
    public void SupportsDecimalDivision()
    {
        var interpreter = new Interpreter();

        var output = interpreter.Execute(
            "اطبع 7 / 2;");

        Assert.Equal(new[] { "3.5" }, output);
    }

    [Fact]
    public void ParsesComparisonExpressions()
    {
        var statements = Parser.Parse("اطبع 5 >= 3;");

        Assert.Single(statements);
        Assert.IsType<PrintStatement>(statements[0]);
    }

    [Fact]
    public void RejectsMissingSemicolon()
    {
        Assert.Throws<SyntaxException>(
            () => Parser.Parse("متغير العدد = 10"));
    }

    [Fact]
    public void RejectsUndefinedVariable()
    {
        var interpreter = new Interpreter();

        Assert.Throws<InvalidOperationException>(
            () => interpreter.Execute("اطبع غير_معرف;"));
    }

    [Fact]
    public void RejectsDivisionByZero()
    {
        var interpreter = new Interpreter();

        Assert.Throws<InvalidOperationException>(
            () => interpreter.Execute("اطبع 10 / 0;"));
    }
}
