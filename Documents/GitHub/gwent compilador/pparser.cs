using System;
using System.Collections.Generic;

public class Pparsernonvalid : Exception
{
    public Ttokens Ttokens { get; }
    public Pparsernonvalid(string message, Ttokens ttokens) : base(message)
    {
        Ttokens = ttokens;
    }
}

public class Pparser
{
    private readonly List<Ttokens> ttokens;
    private int current = 0;

    public Pparser(List<Ttokens> ttokens)
    {
        this.ttokens = ttokens;
    }

    public List<ASTNode> Parse()
    {
        List<ASTNode> statements = new List<ASTNode>();
        while (!IsAtEnd())
        {
            ASTNode? declaration = Declaration();
            if (declaration != null)
            {
                statements.Add(declaration);
            }
        }
        return statements;
    }

    private ASTNode? Declaration()
    {
        try
        {
            if (Match(Ttokenlist.TYPE))
            {
                return VD();
            }
            if (Match(Ttokenlist.FUN))
            {
                return FD();
            }
            return Statement();
        }
        catch (Pparsernonvalid)
        {
            Synchronize();
            return null;
        }
    }

    private ASTNode VD()
    {
        Ttokens type = Previous();
        Ttokens name = Consume(Ttokenlist.ID, "Expect variable name.");
        Console.WriteLine($"Parsing VariableDeclaration: Type = {type.Lexeme}, Name = {name.Lexeme}");

        ASTNode? initializer = null;
        if (Match(Ttokenlist.ASSIGN))
        {
            Console.WriteLine("Parsing VariableDeclaration: Found assignment operator.");
            initializer = Expression();
        }
        else if (Match(Ttokenlist.LBRACKET))
        {
            Console.WriteLine("Parsing VariableDeclaration: Found array declaration.");
            ASTNode size = Expression();
            Consume(Ttokenlist.RBRACKET, "Expect ']' after array size.");
            if (Match(Ttokenlist.ASSIGN))
            {
                Console.WriteLine("Parsing VariableDeclaration: Found array initializer.");
                List<ASTNode> elements = new List<ASTNode>();
                if (Match(Ttokenlist.LBRACE))
                {
                    if (!Check(Ttokenlist.RBRACE))
                    {
                        do
                        {
                            elements.Add(Expression());
                        } while (Match(Ttokenlist.COMMA));
                    }
                    Consume(Ttokenlist.RBRACE, "Expect '}' after array initializer.");
                }
                initializer = new ArrayDeclaration(type, name, size, elements);
            }
            else
            {
                initializer = new ArrayDeclaration(type, name, size, new List<ASTNode>());
            }
        }

        Consume(Ttokenlist.SEMICOLON, "Expect ';' after variable declaration.");
        Console.WriteLine($"Completed VariableDeclaration for {name.Lexeme}");
        return new VariableDeclaration(type, name, initializer ?? new LiteralExpression(null));
    }

    private ASTNode FD()
{
    Ttokens name = Consume(Ttokenlist.ID, "Expect function name.");
    Consume(Ttokenlist.LPAREN, "Expect '(' after function name.");
    List<VariableDeclaration> parameters = new List<VariableDeclaration>();
    if (!Check(Ttokenlist.RPAREN))
    {
        do
        {
            if (parameters.Count >= 255)
            {
                Error(Peek(), "Can't have more than 255 parameters.");
            }

            Ttokens paramType = Consume(Ttokenlist.TYPE, "Expect parameter type.");
            Ttokens paramName = Consume(Ttokenlist.ID, "Expect parameter name.");
            parameters.Add(new VariableDeclaration(paramType, paramName, null));
        } while (Match(Ttokenlist.COMMA));
    }
    Consume(Ttokenlist.RPAREN, "Expect ')' after parameters.");

    Consume(Ttokenlist.LBRACE, "Expect '{' before function body.");
    BlockStatement body = (BlockStatement)Block();
    return new FunctionDeclaration(name, parameters, body);
}

    private ASTNode Statement()
    {
        if (Match(Ttokenlist.IF)) return IS();
        if (Match(Ttokenlist.WHILE)) return WS();
        if (Match(Ttokenlist.FOR)) return FS();
        if (Match(Ttokenlist.RETURN)) return RS();
        if (Match(Ttokenlist.LBRACE)) return Block();

        return ES();
    }

    private ASTNode IS()
    {
        Console.WriteLine("Parsing IfStatement");
        Consume(Ttokenlist.LPAREN, "Expect '(' after 'if'.");
        ASTNode condition = Expression();
        Console.WriteLine("IfStatement: Parsed condition");
        Consume(Ttokenlist.RPAREN, "Expect ')' after if condition.");


        ASTNode thenBranch = Statement();
        Console.WriteLine("IfStatement: Parsed then branch");
        ASTNode? elseBranch = null;
        if (Match(Ttokenlist.ELSE))
        {
            Console.WriteLine("IfStatement: Found else branch");
            elseBranch = Statement();
        }
        Console.WriteLine("Completed IfStatement");
        return new IfStatement(condition, thenBranch, elseBranch ?? new BlockStatement(new List<ASTNode>()));
    }

    private ASTNode WS()
    {
        Consume(Ttokenlist.LPAREN, "Expect '(' after 'while'.");
        ASTNode condition = Expression();
        Consume(Ttokenlist.RPAREN, "Expect ')' after while condition.");
        ASTNode body = Statement();

        return new WhileStatement(condition, body);
    }

    private ASTNode FS()
    {
        Consume(Ttokenlist.LPAREN, "Expect '(' after 'for'.");

        ASTNode? initializer;
        if (Match(Ttokenlist.SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(Ttokenlist.TYPE))
        {
            initializer = VD();
        }
        else
        {
            initializer = ES();
        }

        ASTNode condition = new LiteralExpression(true);
        if (!Check(Ttokenlist.SEMICOLON))
        {
            condition = Expression();
        }
        Consume(Ttokenlist.SEMICOLON, "Expect ';' after loop condition.");

        ASTNode? increment = null;
        if (!Check(Ttokenlist.RPAREN))
        {
            increment = Expression();
        }
        Consume(Ttokenlist.RPAREN, "Expect ')' after for clauses.");

        ASTNode body = Statement();

        if (increment != null)
        {
            body = new BlockStatement(new List<ASTNode>
            {
                body,
                new ExpressionStatement(increment)
            });
        }

        body = new WhileStatement(condition, body);

        if (initializer != null)
        {
            body = new BlockStatement(new List<ASTNode>
            {
                initializer,
                body
            });
        }

        return body;
    }

    private ASTNode RS()
    {
        Ttokens keyword = Previous();
        ASTNode value = new LiteralExpression(null);
        if (!Check(Ttokenlist.SEMICOLON))
        {
            value = Expression();
        }
        Consume(Ttokenlist.SEMICOLON, "Expect ';' after return value.");
        return new ReturnStatement(keyword, value);
    }

    private ASTNode Block()
    {
        Console.WriteLine("Parsing BlockStatement");
        List<ASTNode> statements = new List<ASTNode>();

        while (!Check(Ttokenlist.RBRACE) && !IsAtEnd())
        {
            ASTNode? declaration = Declaration();
            if (declaration != null)
            {
                statements.Add(declaration);
                Console.WriteLine("BlockStatement: Added declaration");
            }
        }

        Consume(Ttokenlist.RBRACE, "Expect '}' after block.");
        Console.WriteLine("Completed BlockStatement");
        return new BlockStatement(statements);
    }

    private ASTNode ES()
    {
        ASTNode expr = Expression();
        Consume(Ttokenlist.SEMICOLON, "Expect ';' after expression.");
        return new ExpressionStatement(expr);
    }

    private ASTNode Expression()
    {
        return Assignment();
    }

    private ASTNode Assignment()
    {
        ASTNode expr = LogicalOr();
        Console.WriteLine("Assignment: Parsed left-hand expression");

        if (Match(Ttokenlist.ASSIGN))
        {
            Ttokens equals = Previous();
            ASTNode value = Expression();
            Console.WriteLine("Assignment: Found assignment operator and right-hand expression");

            if (expr is VariableExpression ve)
            {
                Ttokens name = ve.Name;
                Console.WriteLine($"Assignment: Variable {name.Lexeme}");
                return new AssignmentExpression(name, value);
            }
            else if (expr is ArrayAccess aa)
            {
                Console.WriteLine("Assignment: Array access assignment");
                return new ArrayAssignmentExpression(aa.Array, aa.Index, value);
            }

            throw Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private ASTNode LogicalOr()
    {
        ASTNode expr = LogicalAnd();

        while (Match(Ttokenlist.OR))
        {
            Ttokens op = Previous();
            ASTNode right = LogicalAnd();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private ASTNode LogicalAnd()
    {
        ASTNode expr = Equality();

        while (Match(Ttokenlist.AND))
        {
            Ttokens op = Previous();
            ASTNode right = Equality();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private ASTNode Equality()
    {
        ASTNode expr = Comparison();

        while (Match(Ttokenlist.EQUAL, Ttokenlist.NOT_EQUAL))
        {
            Ttokens op = Previous();
            ASTNode right = Comparison();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private ASTNode Comparison()
    {
        ASTNode expr = Term();

        while (Match(Ttokenlist.GREATER_THAN, Ttokenlist.GREATER_EQUAL, Ttokenlist.LESS_THAN, Ttokenlist.LESS_EQUAL))
        {
            Ttokens op = Previous();
            ASTNode right = Term();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private ASTNode Term()
    {
        ASTNode expr = Factor();

        while (Match(Ttokenlist.PLUS, Ttokenlist.MINUS))
        {
            Ttokens op = Previous();
            ASTNode right = Factor();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private ASTNode Factor()
    {
        ASTNode expr = Unary();

        while (Match(Ttokenlist.TIMES, Ttokenlist.DIVIDE, Ttokenlist.MODULO))
        {
            Ttokens op = Previous();
            ASTNode right = Unary();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    private ASTNode Unary()
    {
        if (Match(Ttokenlist.NOT, Ttokenlist.MINUS))
        {
            Ttokens op = Previous();
            ASTNode right = Unary();
            return new UnaryExpression(op, right);
        }

        return Call();
    }

    private ASTNode Call()
    {
        ASTNode expr = Primary();

        while (true)
        {
            if (Match(Ttokenlist.LPAREN))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private ASTNode FinishCall(ASTNode callee)
    {
        List<ASTNode> arguments = new List<ASTNode>();
        if (!Check(Ttokenlist.RPAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(Ttokenlist.COMMA));
        }

        Ttokens paren = Consume(Ttokenlist.RPAREN, "Expect ')' after arguments.");

        return new CallExpression(callee, paren, arguments);
    }

    private ASTNode Primary()
    {
        if (Match(Ttokenlist.FALSE)) return new LiteralExpression(false);
        if (Match(Ttokenlist.TRUE)) return new LiteralExpression(true);
        if (Match(Ttokenlist.NULL)) return new LiteralExpression(null);

        if (Match(Ttokenlist.NUMBER, Ttokenlist.STRING))
        {
            return new LiteralExpression(Previous().Literal);
        }

        if (Match(Ttokenlist.ID))
        {
            Ttokens name = Previous();
            if (Match(Ttokenlist.LBRACKET))
            {
                ASTNode index = Expression();
                Consume(Ttokenlist.RBRACKET, "Expect ']' after array index.");
                return new ArrayAccess(new VariableExpression(name), index);
            }
            return new VariableExpression(name);
        }

        if (Match(Ttokenlist.LPAREN))
        {
            ASTNode expr = Expression();
            Consume(Ttokenlist.RPAREN, "Expect ')' after expression.");
            return new GroupingExpression(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private bool Match(params Ttokenlist[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private bool Check(Ttokenlist type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Ttokens Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == Ttokenlist.EOF;
    }

    private Ttokens Peek()
    {
        return ttokens[current];
    }

    private Ttokens Previous()
    {
        return ttokens[current - 1];
    }

    private Ttokens Consume(Ttokenlist type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private Pparsernonvalid Error(Ttokens token, string message)
    {
        return new Pparsernonvalid(message, token);
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == Ttokenlist.SEMICOLON) return;

            switch (Peek().Type)
            {
                case Ttokenlist.TYPE:
                case Ttokenlist.FUN:
                case Ttokenlist.IF:
                case Ttokenlist.WHILE:
                case Ttokenlist.FOR:
                case Ttokenlist.RETURN:
                    return;
            }

            Advance();
        }
    }
}