namespace Patra.Core;

public abstract record Expression;
public sealed record LiteralExpression(object Value) : Expression;
public sealed record VariableExpression(string Name, Token Token) : Expression;
public sealed record UnaryExpression(Token Operator, Expression Right) : Expression;
public sealed record BinaryExpression(
    Expression Left, Token Operator, Expression Right) : Expression;

public abstract record Statement;
public sealed record VariableStatement(
    string Name, Token NameToken, Expression Initializer) : Statement;
public sealed record AssignmentStatement(
    string Name, Token NameToken, Expression Value) : Statement;
public sealed record PrintStatement(Expression Value) : Statement;
public sealed record ExpressionStatement(Expression Value) : Statement;
