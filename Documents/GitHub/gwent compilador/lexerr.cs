using System;
using System.Collections.Generic;
using System.Text;

public class Lexernonvalid : Exception
{
    public Lexernonvalid(string message) : base(message) { }
}

public static class Lexerr
{
    private enum Structure
    {
        Start,
        Character,
        Comment,
        SingleLineComment,
        MultiLineComment,
        String,
        Number,
        Identifier,
        Done
    }

    private static readonly Dictionary<string, Ttokenlist> keywords = new Dictionary<string, Ttokenlist>
    {
        { "if", Ttokenlist.IF },
        { "else", Ttokenlist.ELSE },
        { "while", Ttokenlist.WHILE },
        { "for", Ttokenlist.FOR },
        { "return", Ttokenlist.RETURN },
        { "int", Ttokenlist.TYPE },
        { "float", Ttokenlist.TYPE },
        { "char", Ttokenlist.TYPE },
        { "bool", Ttokenlist.TYPE },
        { "empty", Ttokenlist.TYPE },
        { "string", Ttokenlist.TYPE },
        { "true", Ttokenlist.TRUE },
        { "false", Ttokenlist.FALSE },
        { "null", Ttokenlist.NULL },
        { "break", Ttokenlist.BREAK },
        { "continue", Ttokenlist.CONTINUE },
        { "switch", Ttokenlist.SWITCH },
        { "case", Ttokenlist.CASE },
        { "default", Ttokenlist.DEFAULT },
        { "fun", Ttokenlist.FUN },
        { "struct", Ttokenlist.STRUCT },
        { "enum", Ttokenlist.ENUM },
        { "import", Ttokenlist.IMPORT },
        { "try", Ttokenlist.TRY },
        { "catch", Ttokenlist.CATCH },
        { "finally", Ttokenlist.FINALLY },
        { "throw", Ttokenlist.THROW },
        { "class", Ttokenlist.CLASS },
        { "extends", Ttokenlist.EXTENDS },
        { "public", Ttokenlist.PUBLIC },
        { "private", Ttokenlist.PRIVATE },
        { "protected", Ttokenlist.PROTECTED },
        { "static", Ttokenlist.STATIC },
        { "void", Ttokenlist.VOID },
        { "console_writeline", Ttokenlist.CONSOLE_WRITELINE },

    };

    private static readonly Dictionary<string, Ttokenlist> multiCharTokens = new Dictionary<string, Ttokenlist>
    {
        { "==", Ttokenlist.EQUAL },
        { "!=", Ttokenlist.NOT_EQUAL },
        { "<=", Ttokenlist.LESS_EQUAL },
        { ">=", Ttokenlist.GREATER_EQUAL },
        { "&&", Ttokenlist.AND },
        { "||", Ttokenlist.OR },
        { "+=", Ttokenlist.PLUS_ASSIGN },
        { "-=", Ttokenlist.MINUS_ASSIGN },
        { "*=", Ttokenlist.TIMES_ASSIGN },
        { "/=", Ttokenlist.DIVIDE_ASSIGN },
        { "%=", Ttokenlist.MODULO_ASSIGN },
        { "&=", Ttokenlist.AND_ASSIGN },
        { "|=", Ttokenlist.OR_ASSIGN },
        { "^=", Ttokenlist.XOR_ASSIGN },
        { "<<=", Ttokenlist.SHIFT_LEFT_ASSIGN },
        { ">>=", Ttokenlist.SHIFT_RIGHT_ASSIGN },
        { "++", Ttokenlist.INCREMENT },
        { "--", Ttokenlist.DECREMENT },
        { "<<", Ttokenlist.SHIFT_LEFT },
        { ">>", Ttokenlist.SHIFT_RIGHT }
    };

    public static List<Ttokens> Tokengen(string text)
    {
        text = Preprocess(text);
        List<Ttokens> tokens = new List<Ttokens>();
        int i = 0;
        Structure structure = Structure.Start;
        StringBuilder currentToken = new StringBuilder();
        char[] buffer = text.ToCharArray();
        int line = 1;
        int column = 1;

        while (i < buffer.Length)
        {
            char c = buffer[i];

            switch (structure)
            {
                case Structure.Start:
                    if (char.IsWhiteSpace(c))
                    {
                        if (c == '\n')
                        {
                            line++;
                            column = 1;
                        }
                        else
                        {
                            column++;
                        }
                        i++;
                        continue;
                    }
                    if (char.IsLetter(c) || c == '_')
                    {
                        structure = Structure.Identifier;
                        currentToken.Append(c);
                    }
                    else if (char.IsDigit(c))
                    {
                        structure = Structure.Number;
                        currentToken.Append(c);
                    }
                    else if (c == '"')
                    {
                        structure = Structure.String;
                        currentToken.Append(c);
                    }
                    else if (c == '\'')
                    {
                        structure = Structure.Character;
                        currentToken.Append(c);
                    }
                    else if (c == '/' && i + 1 < buffer.Length)
                    {
                        if (buffer[i + 1] == '/')
                        {
                            structure = Structure.SingleLineComment;
                            i++;
                        }
                        else if (buffer[i + 1] == '*')
                        {
                            structure = Structure.MultiLineComment;
                            i++; 
                        }
                        else
                        {
                            tokens.Add(new Ttokens(Ttokenlist.DIVIDE, c.ToString(), null, line, column));
                        }
                    }
                    else if (i + 1 < buffer.Length && multiCharTokens.ContainsKey($"{c}{buffer[i + 1]}"))
                    {
                        tokens.Add(new Ttokens(multiCharTokens[$"{c}{buffer[i + 1]}"], $"{c}{buffer[i + 1]}", null, line, column));
                        column += 2;
                        i += 2;
                        continue;
                    }
                    else
                    {
                        Ttokenlist type = GetSingleTtokens(c);
                        tokens.Add(new Ttokens(type, c.ToString(), null, line, column));
                    }
                    column++;
                    i++;
                    break;

                case Structure.Number:
                    if (char.IsDigit(c) || c == '.' || c == 'x' || c == 'b')
                    {
                        currentToken.Append(c);
                        i++;
                        column++;
                    }
                    else
                    {
                        string number = currentToken.ToString();
                        Ttokenlist numberType = NumberDefiner(number);
                        tokens.Add(new Ttokens(numberType, number, ParseNumber(number, numberType), line, column - number.Length));
                        currentToken.Clear();
                        structure = Structure.Start;
                    }
                    break;

                case Structure.Identifier:
                    if (char.IsLetterOrDigit(c) || c == '_')
                    {
                        currentToken.Append(c);
                        i++;
                        column++;
                    }
                    else
                    {
                        string identifier = currentToken.ToString();
                        if (identifier == "Console" && i < buffer.Length - 1 && buffer[i] == '.')
                        {
                            currentToken.Append(buffer[i]);
                            i++;
                            column++;

                            if (i + 8 <= buffer.Length && new string(buffer, i, 8) == "WriteLine")
                            {
                                currentToken.Append("WriteLine");
                                tokens.Add(new Ttokens(Ttokenlist.CONSOLE_WRITELINE, currentToken.ToString(), null, line, column - currentToken.Length));
                                i += 8;
                                column += 8;
                            }
                            else
                            {
                                tokens.Add(new Ttokens(Ttokenlist.ID, identifier, null, line, column - identifier.Length));
                                i--;
                                column--;
                            }
                        }
                        else if (keywords.TryGetValue(identifier, out Ttokenlist keywordType))
                        {
                            tokens.Add(new Ttokens(keywordType, identifier, null, line, column - identifier.Length));
                        }
                        else
                        {
                            tokens.Add(new Ttokens(Ttokenlist.ID, identifier, null, line, column - identifier.Length));
                        }
                        currentToken.Clear();
                        structure = Structure.Start;
                    }
                    break;

                case Structure.String:
                    if (c == '"' && buffer[i - 1] != '\\')
                    {
                        currentToken.Append(c);
                        tokens.Add(new Ttokens(Ttokenlist.STRING, currentToken.ToString(), currentToken.ToString().Substring(1, currentToken.Length - 2), line, column - currentToken.Length));
                        currentToken.Clear();
                        structure = Structure.Start;
                        i++;
                        column++;
                    }
                    else
                    {
                        currentToken.Append(c);
                        i++;
                        column++;
                    }
                    break;

                case Structure.Character:
                    if (c == '\'' && buffer[i - 1] != '\\')
                    {
                        currentToken.Append(c);
                        tokens.Add(new Ttokens(Ttokenlist.CHARACTER, currentToken.ToString(), currentToken.ToString()[1], line, column - currentToken.Length));
                        currentToken.Clear();
                        structure = Structure.Start;
                        i++;
                        column++;
                    }
                    else
                    {
                        currentToken.Append(c);
                        i++;
                        column++;
                    }
                    break;

                case Structure.SingleLineComment:
                    if (c == '\n')
                    {
                        structure = Structure.Start;
                        line++;
                        column = 1;
                    }
                    i++;
                    break;

                case Structure.MultiLineComment:
                    if (c == '*' && i + 1 < buffer.Length && buffer[i + 1] == '/')
                    {
                        structure = Structure.Start;
                        i += 2;
                        column += 2;
                    }
                    else
                    {
                        if (c == '\n')
                        {
                            line++;
                            column = 1;
                        }
                        else
                        {
                            column++;
                        }
                        i++;
                    }
                    break;
            }
        }

        if (currentToken.Length > 0)
        {
            throw new Lexernonvalid($"Unexpected end of input: {currentToken}");
        }

        tokens.Add(new Ttokens(Ttokenlist.EOF, "", null, line, column));
        return tokens;
    }

    private static Ttokenlist GetSingleTtokens(char c)
    {
        switch (c)
        {
            case '+': return Ttokenlist.PLUS;
            case '-': return Ttokenlist.MINUS;
            case '*': return Ttokenlist.TIMES;
            case '/': return Ttokenlist.DIVIDE;
            case '%': return Ttokenlist.MODULO;
            case '=': return Ttokenlist.ASSIGN;
            case '<': return Ttokenlist.LESS_THAN;
            case '>': return Ttokenlist.GREATER_THAN;
            case '!': return Ttokenlist.NOT;
            case '&': return Ttokenlist.BIT_AND;
            case '|': return Ttokenlist.BIT_OR;
            case '^': return Ttokenlist.BIT_XOR;
            case '~': return Ttokenlist.BIT_NOT;
            case '.': return Ttokenlist.DOT;
            case ',': return Ttokenlist.COMMA;
            case ';': return Ttokenlist.SEMICOLON;
            case ':': return Ttokenlist.COLON;
            case '(': return Ttokenlist.LPAREN;
            case ')': return Ttokenlist.RPAREN;
            case '{': return Ttokenlist.LBRACE;
            case '}': return Ttokenlist.RBRACE;
            case '[': return Ttokenlist.LBRACKET;
            case ']': return Ttokenlist.RBRACKET;
            case '?': return Ttokenlist.QUESTION;
            case '#': return Ttokenlist.HASH;
            case '$': return Ttokenlist.DOLLAR;
            case '\\': return Ttokenlist.BACKSLASH;
            case '`': return Ttokenlist.BACKTICK;
            default: throw new Lexernonvalid($"Unexpected character: {c}");
        }
    }

    private static Ttokenlist NumberDefiner(string number)
    {
        if (number.Contains("."))
            return Ttokenlist.FLOAT;
        if (number.StartsWith("0x") || number.StartsWith("0X"))
            return Ttokenlist.HEX_NUMBER;
        if (number.StartsWith("0b") || number.StartsWith("0B"))
            return Ttokenlist.BIN_NUMBER;
        return Ttokenlist.NUMBER;
    }

    private static object ParseNumber(string number, Ttokenlist type)
    {
        switch (type)
        {
            case Ttokenlist.FLOAT:
                return double.Parse(number);
            case Ttokenlist.HEX_NUMBER:
                return Convert.ToInt32(number.Substring(2), 16);
            case Ttokenlist.BIN_NUMBER:
                return Convert.ToInt32(number.Substring(2), 2);
            case Ttokenlist.NUMBER:
                return int.Parse(number);
            default:
                throw new Lexernonvalid($"Invalid number type: {type}");
        }
    }

    private static string Preprocess(string text)
    {
        StringBuilder result = new StringBuilder();
        bool inMultiLineComment = false;
        int i = 0;

        while (i < text.Length)
        {
            if (!inMultiLineComment && text[i] == '/' && i + 1 < text.Length && text[i + 1] == '*')
            {
                inMultiLineComment = true;
                i += 2;
            }
            else if (inMultiLineComment && text[i] == '*' && i + 1 < text.Length && text[i + 1] == '/')
            {
                inMultiLineComment = false;
                i += 2;
            }
            else if (!inMultiLineComment)
            {
                result.Append(text[i]);
                i++;
            }
            else
            {
                i++;
            }
        }

        return result.ToString();
    }
}