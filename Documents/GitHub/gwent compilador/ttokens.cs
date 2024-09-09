using System;
public enum Ttokenlist
{
    // literal tokens
    BIN_NUMBER,
    CHARACTER,
    FLOAT,
    HEX_NUMBER,
    NUMBER,
    NULL,
    STRING,
    TRUE,
    FALSE,

    ID,

    // arithmetic operators
    DIVIDE,
    MINUS,
    MODULO,
    PLUS,
    TIMES,

    // comparison operetors
    EQUAL,
    GREATER_EQUAL,
    GREATER_THAN,
    LESS_EQUAL,
    LESS_THAN,
    NOT_EQUAL,


    // logical operators
    AND,
    OR,
    NOT,

    // assignment operators
    ASSIGN,
    AND_ASSIGN,
    DIVIDE_ASSIGN,
    MINUS_ASSIGN,
    MODULO_ASSIGN,
    OR_ASSIGN,
    PLUS_ASSIGN,
    SHIFT_LEFT_ASSIGN,
    SHIFT_RIGHT_ASSIGN,
    TIMES_ASSIGN,
    XOR_ASSIGN,

    // decrease and increase operators
    DECREMENT,
    INCREMENT,

    // bit to bit operators
    BIT_AND,
    BIT_NOT,
    BIT_OR,
    BIT_XOR,
    SHIFT_LEFT,
    SHIFT_RIGHT,

    // separators
    BACKSLASH,
    BACKTICK,
    COLON,
    COMMA,
    DOT,
    DOLLAR,
    HASH,
    QUESTION,
    SEMICOLON,

    // parenthisis
    LBRACE,
    LBRACKET,
    LPAREN,
    RBRACE,
    RBRACKET,
    RPAREN,

    // key words
    BREAK,
    CASE,
    CATCH,
    CLASS,
    CONTINUE,
    DEFAULT,
    ELSE,
    ENUM,
    EXTENDS,
    FINALLY,
    FOR,
    FUN,
    IF,
    IMPORT,
    PRIVATE,
    PROTECTED,
    PUBLIC,
    RETURN,
    STATIC,
    STRUCT,
    SWITCH,
    THROW,
    TRY,
    TYPE,
    VOID,
    WHILE,

    // others
    EOF,
    NEWLINE,
    CONSOLE_WRITELINE,
}

public class Ttokens
{
    public Ttokenlist Type { get; }
    public string Lexeme { get; }
    public object? Literal { get; }
    public int Line { get; }
    public int Column { get; }

    public Ttokens(Ttokenlist type, string lexeme, object? literal, int line, int column)
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
        Column = column;
    } 
     public override string ToString() => $"{Type} {Lexeme} {Literal} (Line: {Line}, Column: {Column})";

    public bool Equals(Ttokens? other) =>
        other != null &&
        Type == other.Type &&
        Lexeme == other.Lexeme &&
        Equals(Literal, other.Literal) &&
        Line == other.Line &&
        Column == other.Column;

    public override bool Equals(object? obj) => Equals(obj as Ttokens);

    public override int GetHashCode() => HashCode.Combine(Type, Lexeme, Literal, Line, Column);
}
