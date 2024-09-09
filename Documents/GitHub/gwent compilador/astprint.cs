using System;
using System.Collections.Generic;
using System.Text;

public class ASTPrinter : IVisitor<string>
{
    public string VisitBE(BinaryExpression expr)
    {
        string left = expr.Left.Accept(this);
        string right = expr.Right.Accept(this);
        return $"({left} {expr.Operator.Lexeme} {right})";
    }

    public string VisitUE(UnaryExpression expr)
    {
        string right = expr.Right.Accept(this);
        return $"({expr.Operator.Lexeme}{right})";
    }

    public string VisitLE(LiteralExpression expr)
    {
        return expr.Value?.ToString() ?? "null";
    }
    public string VisitGE(GroupingExpression expr)
    {
        string expression = expr.Expression.Accept(this);
        Console.WriteLine($"Printing grouping expression: {expression}");
        return $"({expression})";
    }

    public string VisitVE(VariableExpression expr)
    {
        return expr.Name.Lexeme;
    }

    public string VisitAE(AssignmentExpression expr)
    {
        string name = expr.Name.Lexeme;
        string value = expr.Value.Accept(this);
        return $"{name} = {value}";
    }

    public string VisitCE(CallExpression expr)
    {
        string callee = expr.Callee.Accept(this);
        string arguments = string.Join(", ", expr.Arguments.ConvertAll(arg => arg.Accept(this)));
        return $"{callee}({arguments})";
    }

    public string VisitAA(ArrayAccess expr)
    {
        string array = expr.Array.Accept(this);
        string index = expr.Index.Accept(this);
        return $"{array}[{index}]";
    }

    public string VisitAAE(ArrayAssignmentExpression expr)
    {
        string arrayAccess = expr.Array.Accept(this);
        string index = expr.Index.Accept(this);
        string value = expr.Value.Accept(this);
        return $"{arrayAccess}[{index}] = {value}";
    }

    public string VisitLoE(LogicalExpression expr)
    {
        string left = expr.Left.Accept(this);
        string right = expr.Right.Accept(this);
        return $"({left} {expr.Operator.Lexeme} {right})";
    }

    public string VisitTE(TernaryExpression expr)
    {
        string condition = expr.Condition.Accept(this);
        string trueExpr = expr.TrueExpr.Accept(this);
        string falseExpr = expr.FalseExpr.Accept(this);
        return $"({condition} ? {trueExpr} : {falseExpr})";
    }

    public string VisitLaE(LambdaExpression expr)
    {
        string parameters = string.Join(", ", expr.Parameters.ConvertAll(param => param.Accept(this)));
        string body = expr.Body.Accept(this);
        return $"({parameters}) => {body}";
    }

    public string VisitTS(ThrowStatement stmt)
    {
        string expression = stmt.Expression.Accept(this);
        return $"throw {expression};";
    }

    public string VisitVD(VariableDeclaration stmt)
    {
        string type = stmt.Type.Lexeme;
        string name = stmt.Name.Lexeme;
        string? initializer = stmt.Initializer?.Accept(this);
        return $"{type} {name} = {initializer};";
    }
    public string VisitFD(FunctionDeclaration stmt)
{
    string name = stmt.Name.Lexeme;
    string parameters = string.Join(", ", stmt.Parameters.Select(param => $"{param.Type.Lexeme} {param.Name.Lexeme}"));
    string body = stmt.Body.Accept(this);
    return $"function {name}({parameters}) {body}";
}

    public string VisitES(ExpressionStatement stmt)
    {
        string expression = stmt.Expression.Accept(this);
        return $"{expression};";
    }

    public string VisitIS(IfStatement stmt)
    {
        string condition = stmt.Condition.Accept(this);
        string thenBranch = stmt.ThenBranch.Accept(this);
        string elseBranch = stmt.ElseBranch != null ? $" else {stmt.ElseBranch.Accept(this)}" : string.Empty;
        return $"if ({condition}) {thenBranch}{elseBranch}";
    }

    public string VisitWS(WhileStatement stmt)
    {
        string condition = stmt.Condition.Accept(this);
        string body = stmt.Body.Accept(this);
        return $"while ({condition}) {body}";
    }

    public string VisitFS(ForStatement stmt)
    {
        string initializer = stmt.Initializer.Accept(this);
        string condition = stmt.Condition.Accept(this);
        string increment = stmt.Increment.Accept(this);
        string body = stmt.Body.Accept(this);
        return $"for ({initializer}; {condition}; {increment}) {body}";
    }

    public string VisitBS(BlockStatement stmt)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("{");
        foreach (var statement in stmt.Statements)
        {
            string stmtString = statement.Accept(this);
            builder.AppendLine($"  {stmtString}");
        }
        builder.Append("}");
        return builder.ToString();
    }

    public string VisitRS(ReturnStatement stmt)
    {
        string value = stmt.Value.Accept(this);
        return $"return {value};";
    }

    public string VisitAD(ArrayDeclaration stmt)
    {
        string type = stmt.Type.Lexeme;
        string name = stmt.Name.Lexeme;
        string size = stmt.Size.Accept(this);
        string initializer = stmt.Initializer.Count > 0
            ? $" = {{ {string.Join(", ", stmt.Initializer.ConvertAll(init => init.Accept(this)))} }}"
            : string.Empty;
        return $"{type} {name}[{size}]{initializer};";
    }

    public string VisitSD(StructDeclaration stmt)
    {
        string name = stmt.Name.Lexeme;
        string fields = string.Join("\n  ", stmt.Fields.ConvertAll(field => field.Accept(this)));
        return $"struct {name} {{\n  {fields}\n}}";
    }

    public string VisitED(EnumDeclaration stmt)
    {
        string name = stmt.Name.Lexeme;
        string values = string.Join(", ", stmt.Values.ConvertAll(value => value.Lexeme));
        return $"enum {name} {{ {values} }}";
    }

    public string VisitImS(ImportStatement stmt)
    {
        return $"import {stmt.Module.Lexeme};";
    }

    public string VisitTCS(TryCatchStatement stmt)
    {
        string tryBlock = stmt.TryBlock.Accept(this);
        string catchParam = stmt.CatchParameter.Accept(this);
        string catchBlock = stmt.CatchBlock.Accept(this);
        string finallyBlock = stmt.FinallyBlock != null ? $" finally {stmt.FinallyBlock.Accept(this)}" : string.Empty;
        return $"try {tryBlock} catch ({catchParam}) {catchBlock}{finallyBlock}";
    }

    public string VisitSS(SwitchStatement stmt)
    {
        string expression = stmt.Expression.Accept(this);
        string cases = string.Join("\n", stmt.Cases.ConvertAll(caseStmt => caseStmt.Accept(this)));
        string defaultCase = stmt.DefaultCase != null ? $"default: {stmt.DefaultCase.Accept(this)}" : string.Empty;
        return $"switch ({expression}) {{\n{cases}\n{defaultCase}\n}}";
    }

    public string VisitCaS(CaseStatement stmt)
    {
        string value = stmt.Value.Accept(this);
        string body = stmt.Body.Accept(this);
        return $"case {value}: {body}";
    }

    public string VisitBrS(BreakStatement stmt)
    {
        return "break;";
    }

    public string VisitCS(ContinueStatement stmt)
    {
        return "continue;";
    }
}