using System.Globalization;

namespace Patra.Core;

/// <summary>
/// مفسّر بسيط ينفذ AST ويجمع مخرجات أوامر الطباعة.
/// </summary>
public sealed class Interpreter
{
    private readonly Dictionary<string, object> _variables =
        new(StringComparer.Ordinal);

    private readonly List<string> _output = [];

    public IReadOnlyList<string> Output => _output.AsReadOnly();

    public IReadOnlyDictionary<string, object> Variables => _variables;

    public IReadOnlyList<string> Execute(string source)
    {
        IReadOnlyList<Statement> program = Parser.Parse(source);

        foreach (Statement statement in program)
            ExecuteStatement(statement);

        return Output;
    }

    private void ExecuteStatement(Statement statement)
    {
        switch (statement)
        {
            case VariableStatement variable:
                if (_variables.ContainsKey(variable.Name))
                    throw new InvalidOperationException(
                        $"المتغير '{variable.Name}' معرّف مسبقًا.");

                _variables[variable.Name] = Evaluate(variable.Initializer);
                break;

            case AssignmentStatement assignment:
                if (!_variables.ContainsKey(assignment.Name))
                    throw new InvalidOperationException(
                        $"المتغير '{assignment.Name}' غير معرّف.");

                _variables[assignment.Name] = Evaluate(assignment.Value);
                break;

            case PrintStatement print:
                _output.Add(Format(Evaluate(print.Value)));
                break;

            case ExpressionStatement expression:
                _ = Evaluate(expression.Value);
                break;

            default:
                throw new NotSupportedException(
                    $"تعليمة غير مدعومة: {statement.GetType().Name}");
        }
    }

    private object Evaluate(Expression expression)
    {
        switch (expression)
        {
            case LiteralExpression literal:
                return literal.Value;

            case VariableExpression variable:
                if (_variables.TryGetValue(variable.Name, out object? value))
                    return value;

                throw new InvalidOperationException(
                    $"المتغير '{variable.Name}' غير معرّف.");

            case UnaryExpression unary:
            {
                object right = Evaluate(unary.Right);

                return unary.Operator.Kind switch
                {
                    TokenKind.Minus => -AsNumber(right, unary.Operator),
                    TokenKind.Plus => AsNumber(right, unary.Operator),
                    TokenKind.Bang => !AsBoolean(right, unary.Operator),
                    _ => throw new NotSupportedException("عامل أحادي غير مدعوم.")
                };
            }

            case BinaryExpression binary:
                return EvaluateBinary(binary);

            default:
                throw new NotSupportedException(
                    $"تعبير غير مدعوم: {expression.GetType().Name}");
        }
    }

    private object EvaluateBinary(BinaryExpression expression)
    {
        object left = Evaluate(expression.Left);
        object right = Evaluate(expression.Right);

        switch (expression.Operator.Kind)
        {
            case TokenKind.Plus:
                if (left is string || right is string)
                    return Format(left) + Format(right);

                return AsNumber(left, expression.Operator) +
                       AsNumber(right, expression.Operator);

            case TokenKind.Minus:
                return AsNumber(left, expression.Operator) -
                       AsNumber(right, expression.Operator);

            case TokenKind.Star:
                return AsNumber(left, expression.Operator) *
                       AsNumber(right, expression.Operator);

            case TokenKind.Slash:
            {
                double divisor = AsNumber(right, expression.Operator);
                if (divisor == 0)
                    throw new InvalidOperationException("لا يمكن القسمة على صفر.");

                return AsNumber(left, expression.Operator) / divisor;
            }

            case TokenKind.Percent:
            {
                double divisor = AsNumber(right, expression.Operator);
                if (divisor == 0)
                    throw new InvalidOperationException("لا يمكن باقي القسمة على صفر.");

                return AsNumber(left, expression.Operator) % divisor;
            }

            case TokenKind.Equal:
                return Equals(left, right);

            case TokenKind.BangEqual:
                return !Equals(left, right);

            case TokenKind.Greater:
                return AsNumber(left, expression.Operator) >
                       AsNumber(right, expression.Operator);

            case TokenKind.GreaterEqual:
                return AsNumber(left, expression.Operator) >=
                       AsNumber(right, expression.Operator);

            case TokenKind.Less:
                return AsNumber(left, expression.Operator) <
                       AsNumber(right, expression.Operator);

            case TokenKind.LessEqual:
                return AsNumber(left, expression.Operator) <=
                       AsNumber(right, expression.Operator);

            default:
                throw new NotSupportedException("عامل ثنائي غير مدعوم.");
        }
    }

    private static double AsNumber(object value, Token token)
    {
        if (value is double number)
            return number;

        throw new InvalidOperationException(
            $"العامل '{token.Lexeme}' يتطلب أرقامًا.");
    }

    private static bool AsBoolean(object value, Token token)
    {
        if (value is bool boolean)
            return boolean;

        throw new InvalidOperationException(
            $"العامل '{token.Lexeme}' يتطلب قيمة منطقية.");
    }

    private static string Format(object value) => value switch
    {
        double number => number.ToString("G", CultureInfo.InvariantCulture),
        bool boolean => boolean ? "true" : "false",
        string text => text,
        _ => value.ToString() ?? string.Empty
    };
}
