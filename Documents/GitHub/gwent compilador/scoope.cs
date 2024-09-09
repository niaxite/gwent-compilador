using System;
using System.Collections.Generic;

public class Scoope
{
    private readonly Dictionary<string, ScoopeVariable> variables;
    private readonly Scoope? parentScope;

    public Scoope(Scoope? parentScope = null)
    {
        this.parentScope = parentScope;
        this.variables = new Dictionary<string, ScoopeVariable>(StringComparer.OrdinalIgnoreCase);
    }

    private class ScoopeVariable
    {
        public object? Value { get; set; }
        public bool IsParameter { get; set; }
    }

    public void Setter(string name, object? value, bool isParameter = false)
    {
        variables[name] = new ScoopeVariable { Value = value, IsParameter = isParameter };
    }

    public object? Obtain(string name)
    {
        if (variables.TryGetValue(name, out var variable))
        {
            return variable.Value;
        }
        if (parentScope != null)
        {
            return parentScope.Obtain(name);
        }
        throw new KeyNotFoundException($"Variable '{name}' is not defined.");
    }

    public void Director(string name, object value)
    {
        if (variables.TryGetValue(name, out var variable))
        {
            variable.Value = value;
        }
        else if (parentScope != null)
        {
            parentScope.Director(name, value);
        }
        else
        {
            throw new KeyNotFoundException($"Variable '{name}' is not defined.");
        }
    }

    public bool SetDone(string name)
    {
        return variables.ContainsKey(name) || (parentScope?.SetDone(name) ?? false);
    }

    public bool TryToObtain(string name, out object? value)
    {
        if (variables.TryGetValue(name, out var variable))
        {
            value = variable.Value;
            return true;
        }
        if (parentScope != null)
        {
            return parentScope.TryToObtain(name, out value);
        }
        value = null;
        return false;
    }

    public Scoope CreateScoopeHeir()
    {
        return new Scoope(this);
    }

    public void DefineValue(string name, object? value)
    {
        Setter(name, value, true);
    }

    public bool ValueDefined(string name)
    {
        return variables.TryGetValue(name, out var variable) && variable.IsParameter;
    }
}