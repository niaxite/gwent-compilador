using System;
using System.Collections.Generic;

public class SemanticAnalyzer : IVisitor<object?>
{
    private Scoope currentScope = new Scoope(); 
    private Type? currentReturnType = null; 

    public object? VisitBE(BinaryExpression expr)
    {
        var left = expr.Left.Accept(this);
        var right = expr.Right.Accept(this);

        if (left == null || right == null)
        {
            throw new Exception("Error: Operando en expresión binaria no puede ser nulo.");
        }

        if (left.GetType() != right.GetType())
        {
            throw new Exception($"Error: Los tipos de los operandos en la expresión binaria no coinciden. Left: {left.GetType()}, Right: {right.GetType()}");
        }

        Console.WriteLine($"Comparando operandos: left = {left} ({left.GetType()}), right = {right} ({right.GetType()})");

        switch (expr.Operator.Type)
        {
            case Ttokenlist.PLUS:
                if (left is int && right is int)
                    return (int)left + (int)right;
                if (left is float && right is float)
                    return (float)left + (float)right;
                if (left is double && right is double)
                    return (double)left + (double)right;
                break;
            case Ttokenlist.MINUS:
                if (left is int && right is int)
                    return (int)left - (int)right;
                if (left is float && right is float)
                    return (float)left - (float)right;
                if (left is double && right is double)
                    return (double)left - (double)right;
                break;
            case Ttokenlist.TIMES:
                if (left is int && right is int)
                    return (int)left * (int)right;
                if (left is float && right is float)
                    return (float)left * (float)right;
                if (left is double && right is double)
                    return (double)left * (double)right;
                break;
            case Ttokenlist.DIVIDE:
                if (left is int && right is int)
                {
                    if ((int)right == 0)
                        throw new Exception("Error: División por cero.");
                    return (int)left / (int)right;
                }
                if (left is float && right is float)
                {
                    if ((float)right == 0)
                        throw new Exception("Error: División por cero.");
                    return (float)left / (float)right;
                }
                if (left is double && right is double)
                {
                    if ((double)right == 0)
                        throw new Exception("Error: División por cero.");
                    return (double)left / (double)right;
                }
                break;
            case Ttokenlist.MODULO:
                if (left is int && right is int)
                    return (int)left % (int)right;
                if (left is float && right is float)
                    return (float)left % (float)right;
                if (left is double && right is double)
                    return (double)left % (double)right;
                break;

            case Ttokenlist.AND:
            case Ttokenlist.OR:
                if (left is bool && right is bool)
                {
                    return expr.Operator.Type == Ttokenlist.AND ? (bool)left && (bool)right : (bool)left || (bool)right;
                }
                break;

            case Ttokenlist.LESS_THAN:
            case Ttokenlist.LESS_EQUAL:
            case Ttokenlist.GREATER_THAN:
            case Ttokenlist.GREATER_EQUAL:
                if (left is int && right is int)
                    return expr.Operator.Type switch
                    {
                        Ttokenlist.LESS_THAN => (int)left < (int)right,
                        Ttokenlist.LESS_EQUAL => (int)left <= (int)right,
                        Ttokenlist.GREATER_THAN => (int)left > (int)right,
                        Ttokenlist.GREATER_EQUAL => (int)left >= (int)right,
                        _ => throw new Exception("Operador no válido para comparación.")
                    };
                if (left is float && right is float)
                    return expr.Operator.Type switch
                    {
                        Ttokenlist.LESS_THAN => (float)left < (float)right,
                        Ttokenlist.LESS_EQUAL => (float)left <= (float)right,
                        Ttokenlist.GREATER_THAN => (float)left > (float)right,
                        Ttokenlist.GREATER_EQUAL => (float)left >= (float)right,
                        _ => throw new Exception("Operador no válido para comparación.")
                    };
                if (left is double && right is double)
                    return expr.Operator.Type switch
                    {
                        Ttokenlist.LESS_THAN => (double)left < (double)right,
                        Ttokenlist.LESS_EQUAL => (double)left <= (double)right,
                        Ttokenlist.GREATER_THAN => (double)left > (double)right,
                        Ttokenlist.GREATER_EQUAL => (double)left >= (double)right,
                        _ => throw new Exception("Operador no válido para comparación.")
                    };
                break;

            case Ttokenlist.EQUAL:
            case Ttokenlist.NOT_EQUAL:
                return expr.Operator.Type == Ttokenlist.EQUAL ? Equals(left, right) : !Equals(left, right);

            default:
                throw new Exception($"Error: Operador '{expr.Operator.Lexeme}' no soportado en esta operación.");
        }

        throw new Exception($"Error: Operandos no compatibles para la operación '{expr.Operator.Lexeme}'");
    }

    public object? VisitUE(UnaryExpression expr)
    {
        var operand = expr.Right.Accept(this);
        if (operand == null)
        {
            throw new Exception("Error: Operando en expresión unaria no puede ser nulo.");
        }
        return operand;
    }

    public object? VisitLE(LiteralExpression expr)
    {
        return expr.Value;
    }

    public object? VisitGE(GroupingExpression expr)
    {
        return expr.Expression.Accept(this);
    }

    public object? VisitVE(VariableExpression expr)
    {
        if (!currentScope.SetDone(expr.Name.Lexeme))
        {
            throw new Exception($"Error: Variable '{expr.Name.Lexeme}' no está definida.");
        }
        return currentScope.Obtain(expr.Name.Lexeme);
    }

    public object? VisitAE(AssignmentExpression expr)
    {
        var value = expr.Value.Accept(this);
        if (!currentScope.SetDone(expr.Name.Lexeme))
        {

            throw new Exception($"Error: Variable '{expr.Name.Lexeme}' no está definida.");
        }
        currentScope.Director(expr.Name.Lexeme, value ?? throw new Exception("Error: Asignación de un valor nulo."));
        return value;
    }
    public object? VisitCE(CallExpression expr)
    {
        var callee = expr.Callee.Accept(this);
        if (callee == null || !(callee is FunctionDeclaration function))
        {
            throw new Exception($"Error: Llamada a una función no definida o inválida.");
        }

        if (function.Parameters.Count != expr.Arguments.Count)
        {
            throw new Exception("Error: Número incorrecto de argumentos en la llamada a la función.");
        }

        var functionScope = currentScope.CreateScoopeHeir();
        var previousScope = currentScope;
        currentScope = functionScope;

        for (int i = 0; i < expr.Arguments.Count; i++)
        {
            var argumentValue = expr.Arguments[i].Accept(this);
            var param = function.Parameters[i];

            currentScope.Setter(param.Name.Lexeme, argumentValue);
        }

        function.Body.Accept(this);

        currentScope = previousScope;

        return null;
    }

    public object? VisitFD(FunctionDeclaration stmt)
{
    var functionScope = currentScope.CreateScoopeHeir();
    var previousScope = currentScope;
    currentScope = functionScope;

    foreach (var param in stmt.Parameters)
    {
        currentScope.DefineValue(param.Name.Lexeme, null);
    }

    stmt.Body.Accept(this);

    currentScope = previousScope;

    return null;
}


    public object? VisitVD(VariableDeclaration stmt)
    {
        var initializer = stmt.Initializer?.Accept(this);
        currentScope.Setter(stmt.Name.Lexeme, initializer ?? throw new Exception("Error: Inicialización de un valor nulo."));
        return null;
    }

    public object? VisitES(ExpressionStatement stmt)
    {
        return stmt.Expression.Accept(this);
    }

    public object? VisitIS(IfStatement stmt)
    {
        var condition = stmt.Condition.Accept(this);
        Console.WriteLine($"Evaluando condición del 'if': {condition} (Tipo: {condition?.GetType()?.Name})");

        if (condition == null || condition.GetType() != typeof(bool))
        {
            throw new Exception("Error: La condición de la declaración 'if' no puede ser nula o no booleana.");
        }

        stmt.ThenBranch.Accept(this);
        stmt.ElseBranch?.Accept(this);
        return null;
    }

    public object? VisitWS(WhileStatement stmt)
    {
        object? conditionResult = stmt.Condition.Accept(this);

        if (conditionResult == null)
        {
            throw new Exception("Error: La condición de la declaración 'while' no puede ser nula.");
        }

        if (conditionResult is not bool conditionValue)
        {
            throw new Exception($"Error: La condición de la declaración 'while' debe ser booleana, pero es de tipo {conditionResult.GetType().Name}.");
        }

        Console.WriteLine($"Evaluando condición del 'while': {conditionValue} (Tipo: {conditionValue.GetType().Name})");

        while (conditionValue)
        {
            stmt.Body.Accept(this);

            conditionResult = stmt.Condition.Accept(this);

            if (conditionResult == null)
            {
                throw new Exception("Error: La condición de la declaración 'while' se volvió nula durante la ejecución.");
            }

            if (conditionResult is not bool newConditionValue)
            {
                throw new Exception($"Error: La condición de la declaración 'while' se volvió no booleana durante la ejecución. Tipo actual: {conditionResult.GetType().Name}.");
            }

            conditionValue = newConditionValue;
        }

        Console.WriteLine("El 'while' ha terminado.");
        return null;
    }
    public object? VisitFS(ForStatement stmt)
    {
        var forScope = currentScope.CreateScoopeHeir();
        var previousScope = currentScope;
        currentScope = forScope;

        if (stmt.Initializer != null)
        {
            stmt.Initializer.Accept(this);
        }

        if (stmt.Condition != null)
        {
            var condition = stmt.Condition.Accept(this);
            if (condition == null || condition.GetType() != typeof(bool))
            {
                throw new Exception("Error: La condición del ciclo 'for' debe ser booleana.");
            }
        }

        stmt.Body.Accept(this);

        if (stmt.Increment != null)
        {
            stmt.Increment.Accept(this);
        }

        currentScope = previousScope;

        return null;
    }


    public object? VisitBS(BlockStatement stmt)
    {
        var blockScope = currentScope.CreateScoopeHeir();
        var previousScope = currentScope;
        currentScope = blockScope;

        foreach (var statement in stmt.Statements)
        {
            statement.Accept(this);
        }

        currentScope = previousScope;
        return null;
    }

    public object? VisitRS(ReturnStatement stmt)
    {
        if (currentReturnType == null)
        {
            throw new Exception("Error: La instrucción 'return' solo puede usarse dentro de una función.");
        }

        var returnValue = stmt.Value?.Accept(this);
        if (returnValue != null && returnValue.GetType() != currentReturnType)
        {
            throw new Exception($"Error: Tipo de retorno no coincide con el esperado. Se esperaba {currentReturnType}, pero se encontró {returnValue.GetType()}.");
        }

        return returnValue;
    }

    public object? VisitAD(ArrayDeclaration stmt)
    {
        var size = stmt.Size.Accept(this);
        if (size == null)
        {
            throw new Exception("Error: El tamaño del arreglo no puede ser nulo.");
        }
        currentScope.Setter(stmt.Name.Lexeme, stmt);
        return null;
    }

    public object? VisitAA(ArrayAccess expr)
    {
        var array = expr.Array.Accept(this);
        var index = expr.Index.Accept(this);
        if (array == null)
        {
            throw new Exception("Error: El arreglo accedido no puede ser nulo.");
        }
        if (index == null)
        {
            throw new Exception("Error: El índice del arreglo no puede ser nulo.");
        }
        return null;
    }

    public object? VisitAAE(ArrayAssignmentExpression expr)
    {
        var array = expr.Array.Accept(this);
        var index = expr.Index.Accept(this);
        var value = expr.Value.Accept(this);
        if (array == null || index == null || value == null)
        {
            throw new Exception("Error: Acceso o asignación en el arreglo tiene valores nulos.");
        }
        return value;
    }

    public object? VisitSD(StructDeclaration stmt)
    {
        currentScope.Setter(stmt.Name.Lexeme, stmt);
        return null;
    }

    public object? VisitED(EnumDeclaration stmt)
    {
        currentScope.Setter(stmt.Name.Lexeme, stmt);
        return null;
    }

    public object? VisitTE(TernaryExpression expr)
    {
        var condition = expr.Condition.Accept(this);
        if (condition == null || condition.GetType() != typeof(bool))
        {
            throw new Exception("Error: La condición en la expresión ternaria no puede ser nula o no booleana.");
        }
        var trueExpr = expr.TrueExpr.Accept(this);
        var falseExpr = expr.FalseExpr.Accept(this);
        return condition != null ? trueExpr : falseExpr;
    }

    public object? VisitLaE(LambdaExpression expr)
    {
        currentScope.Setter("lambda", expr);
        return expr;
    }

    public object? VisitImS(ImportStatement stmt)
    {
        throw new NotImplementedException("Las declaraciones de importación no están implementadas en el analizador semántico.");
    }

    public object? VisitTCS(TryCatchStatement stmt)
    {
        stmt.TryBlock.Accept(this);
        stmt.CatchBlock.Accept(this);
        stmt.FinallyBlock?.Accept(this);
        return null;
    }

    public object? VisitTS(ThrowStatement stmt)
    {
        stmt.Expression.Accept(this);
        return null;
    }

    public object? VisitLoE(LogicalExpression expr)
    {
        var left = expr.Left.Accept(this);
        var right = expr.Right.Accept(this);
        if (left == null || right == null)
        {
            throw new Exception("Error: Operando en expresión lógica no puede ser nulo.");
        }
        return null;
    }

    public object? VisitSS(SwitchStatement stmt)
    {
        stmt.Expression.Accept(this);
        foreach (var caseStmt in stmt.Cases)
        {
            caseStmt.Accept(this);
        }
        stmt.DefaultCase?.Accept(this);
        return null;
    }

    public object? VisitCaS(CaseStatement stmt)
    {
        stmt.Value.Accept(this);
        stmt.Body.Accept(this);
        return null;
    }

    public object? VisitBrS(BreakStatement stmt)
    {
        return null;
    }

    public object? VisitCS(ContinueStatement stmt)
    {
        return null;
    }
}