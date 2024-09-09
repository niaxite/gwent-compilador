using System.Collections.Generic;

public abstract class ASTNode
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}

public interface IVisitor<T>
{
    T VisitBE(BinaryExpression expr);
    T VisitUE(UnaryExpression expr);
    T VisitLE(LiteralExpression expr);
    T VisitGE(GroupingExpression expr);
    T VisitVE(VariableExpression expr);
    T VisitAE(AssignmentExpression expr);
    T VisitCE(CallExpression expr);
    T VisitVD(VariableDeclaration stmt);
    T VisitFD(FunctionDeclaration stmt);
    T VisitES(ExpressionStatement stmt);
    T VisitIS(IfStatement stmt);
    T VisitWS(WhileStatement stmt);
    T VisitFS(ForStatement stmt);
    T VisitBS(BlockStatement stmt);
    T VisitRS(ReturnStatement stmt);
    T VisitAD(ArrayDeclaration stmt);
    T VisitAA(ArrayAccess expr);
    T VisitAAE(ArrayAssignmentExpression expr);
    T VisitSD(StructDeclaration stmt);
    T VisitED(EnumDeclaration stmt);
    T VisitTE(TernaryExpression expr);
    T VisitLaE(LambdaExpression expr);
    T VisitImS(ImportStatement stmt);
    T VisitTCS(TryCatchStatement stmt);
    T VisitTS(ThrowStatement stmt);
    T VisitLoE(LogicalExpression expr);
    T VisitSS(SwitchStatement stmt);
    T VisitCaS(CaseStatement stmt);
    T VisitBrS(BreakStatement stmt);
    T VisitCS(ContinueStatement stmt);
}

public class BinaryExpression : ASTNode
{
    public ASTNode Left { get; }
    public Ttokens Operator { get; }
    public ASTNode Right { get; }

    public BinaryExpression(ASTNode left, Ttokens op, ASTNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitBE(this);
    }
}

public class UnaryExpression : ASTNode
{
    public Ttokens Operator { get; }
    public ASTNode Right { get; }

    public UnaryExpression(Ttokens op, ASTNode right)
    {
        Operator = op;
        Right = right;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitUE(this);
    }
}

public class LiteralExpression : ASTNode
{
    public object? Value { get; }

    public LiteralExpression(object? value)
    {
        Value = value;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitLE(this);
    }
}

public class GroupingExpression : ASTNode
{
    public ASTNode Expression { get; }

    public GroupingExpression(ASTNode expression)
    {
        Console.WriteLine("Flattening nested grouping expression");
        if (expression is GroupingExpression innerGroup)
        {
            Expression = innerGroup.Expression;
        }
        else
        {
            Expression = expression;
        }
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitGE(this);
    }
}


public class VariableExpression : ASTNode
{
    public Ttokens Name { get; }

    public VariableExpression(Ttokens name)
    {
        Name = name;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitVE(this);
    }
}

public class AssignmentExpression : ASTNode
{
    public Ttokens Name { get; }
    public ASTNode Value { get; }

    public AssignmentExpression(Ttokens name, ASTNode value)
    {
        Name = name;
        Value = value;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitAE(this);
    }
}

public class CallExpression : ASTNode
{
    public ASTNode Callee { get; }
    public Ttokens Paren { get; }
    public List<ASTNode> Arguments { get; }

    public CallExpression(ASTNode callee, Ttokens paren, List<ASTNode> arguments)
    {
        Callee = callee;
        Paren = paren;
        Arguments = arguments;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitCE(this);
    }
}
public class LogicalExpression : ASTNode
{
    public ASTNode Left { get; }
    public Ttokens Operator { get; }
    public ASTNode Right { get; }

    public LogicalExpression(ASTNode left, Ttokens op, ASTNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitLoE(this);
    }
}

public class VariableDeclaration : ASTNode
{
    public Ttokens Type { get; }
    public Ttokens Name { get; }
    public ASTNode? Initializer { get; }

    public VariableDeclaration(Ttokens type, Ttokens name, ASTNode? initializer)
    {
        Type = type;
        Name = name;
        Initializer = initializer;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitVD(this);
    }
}

public class FunctionDeclaration : ASTNode
{
    public Ttokens Name { get; }
    public List<VariableDeclaration> Parameters { get; }
    public BlockStatement Body { get; }

    public FunctionDeclaration(Ttokens name, List<VariableDeclaration> parameters, BlockStatement body)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitFD(this);
    }
}

public class ExpressionStatement : ASTNode
{
    public ASTNode Expression { get; }

    public ExpressionStatement(ASTNode expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitES(this);
    }
}

public class IfStatement : ASTNode
{
    public ASTNode Condition { get; }
    public ASTNode ThenBranch { get; }
    public ASTNode ElseBranch { get; }

    public IfStatement(ASTNode condition, ASTNode thenBranch, ASTNode elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitIS(this);
    }
}

public class WhileStatement : ASTNode
{
    public ASTNode Condition { get; }
    public ASTNode Body { get; }

    public WhileStatement(ASTNode condition, ASTNode body)
    {
        Condition = condition;
        Body = body;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitWS(this);
    }
}

public class ForStatement : ASTNode
{
    public ASTNode Initializer { get; }
    public ASTNode Condition { get; }
    public ASTNode Increment { get; }
    public ASTNode Body { get; }

    public ForStatement(ASTNode initializer, ASTNode condition, ASTNode increment, ASTNode body)
    {
        Initializer = initializer;
        Condition = condition;
        Increment = increment;
        Body = body;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitFS(this);
    }

}

public class BlockStatement : ASTNode
{
    public List<ASTNode> Statements { get; }

    public BlockStatement(List<ASTNode> statements)
    {
        Statements = statements;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitBS(this);
    }
}

public class ReturnStatement : ASTNode
{
    public Ttokens Keyword { get; }
    public ASTNode Value { get; }

    public ReturnStatement(Ttokens keyword, ASTNode value)
    {
        Keyword = keyword;
        Value = value;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitRS(this);
    }
}
public class SwitchStatement : ASTNode
{
    public ASTNode Expression { get; }
    public List<CaseStatement> Cases { get; }
    public BlockStatement DefaultCase { get; }

    public SwitchStatement(ASTNode expression, List<CaseStatement> cases, BlockStatement defaultCase)
    {
        Expression = expression;
        Cases = cases;
        DefaultCase = defaultCase;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitSS(this);
    }
}

public class CaseStatement : ASTNode
{
    public ASTNode Value { get; }
    public BlockStatement Body { get; }

    public CaseStatement(ASTNode value, BlockStatement body)
    {
        Value = value;
        Body = body;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitCaS(this);
    }
}

public class BreakStatement : ASTNode
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitBrS(this);
    }
}

public class ContinueStatement : ASTNode
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitCS(this);
    }
}
public class ArrayDeclaration : ASTNode
{
    public Ttokens Type { get; }
    public Ttokens Name { get; }
    public ASTNode Size { get; }
    public List<ASTNode> Initializer { get; }

    public ArrayDeclaration(Ttokens type, Ttokens name, ASTNode size, List<ASTNode> initializer)
    {
        Type = type;
        Name = name;
        Size = size;
        Initializer = initializer;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitAD(this);
    }
}

public class ArrayAccess : ASTNode
{
    public ASTNode Array { get; }
    public ASTNode Index { get; }

    public ArrayAccess(ASTNode array, ASTNode index)
    {
        Array = array;
        Index = index;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitAA(this);
    }
}

public class ArrayAssignmentExpression : ASTNode
{
    public ASTNode Array { get; }
    public ASTNode Index { get; }
    public ASTNode Value { get; }

    public ArrayAssignmentExpression(ASTNode array, ASTNode index, ASTNode value)
    {
        Array = array;
        Index = index;
        Value = value;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitAAE(this);
    }
}

public class StructDeclaration : ASTNode
{
    public Ttokens Name { get; }
    public List<VariableDeclaration> Fields { get; }

    public StructDeclaration(Ttokens name, List<VariableDeclaration> fields)
    {
        Name = name;
        Fields = fields;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitSD(this);
    }
}

public class EnumDeclaration : ASTNode
{
    public Ttokens Name { get; }
    public List<Ttokens> Values { get; }

    public EnumDeclaration(Ttokens name, List<Ttokens> values)
    {
        Name = name;
        Values = values;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitED(this);
    }
}

public class TernaryExpression : ASTNode
{
    public ASTNode Condition { get; }
    public ASTNode TrueExpr { get; }
    public ASTNode FalseExpr { get; }

    public TernaryExpression(ASTNode condition, ASTNode trueExpr, ASTNode falseExpr)
    {
        Condition = condition;
        TrueExpr = trueExpr;
        FalseExpr = falseExpr;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitTE(this);
    }
}

public class LambdaExpression : ASTNode
{
    public List<VariableDeclaration> Parameters { get; }
    public ASTNode Body { get; }

    public LambdaExpression(List<VariableDeclaration> parameters, ASTNode body)
    {
        Parameters = parameters;
        Body = body;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitLaE(this);
    }
}

public class ImportStatement : ASTNode
{
    public Ttokens Module { get; }

    public ImportStatement(Ttokens module)
    {
        Module = module;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitImS(this);
    }
}

public class TryCatchStatement : ASTNode
{
    public BlockStatement TryBlock { get; }
    public VariableDeclaration CatchParameter { get; }
    public BlockStatement CatchBlock { get; }
    public BlockStatement FinallyBlock { get; }

    public TryCatchStatement(BlockStatement tryBlock, VariableDeclaration catchParameter, BlockStatement catchBlock, BlockStatement finallyBlock)
    {
        TryBlock = tryBlock;
        CatchParameter = catchParameter;
        CatchBlock = catchBlock;
        FinallyBlock = finallyBlock;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitTCS(this);
    }
}

public class ThrowStatement : ASTNode
{
    public ASTNode Expression { get; }

    public ThrowStatement(ASTNode expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitTS(this);
    }
}