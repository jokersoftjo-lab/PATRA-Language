namespace Patra.Core;

public enum TokenKind
{
    Identifier,
    Number,
    String,

    Plus,
    Minus,
    Star,
    Slash,
    Percent,

    Assign,
    Equal,
    Bang,
    BangEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,

    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    Comma,
    Semicolon,
    Colon,
    Dot,

    EndOfFile
}
