using System.Collections.Generic;
using VarDump.Collections;

namespace VarDump.Visitor;

public sealed class VisitContext(int maxDepth)
{
    private static readonly IEqualityComparer<object> IdentityComparer = new ObjectIdentityComparer();
    private readonly HashSet<object> _visitedObjects = new(IdentityComparer);

    public int CurrentDepth { get; set; }

    public bool TryAddVisited(object value)
    {
        return value is null
               || value.GetType().IsValueType
               || _visitedObjects.Add(value);
    }

    public void RemoveVisited(object value)
    {
        if(value is null || value.GetType().IsValueType) return;

        _visitedObjects.Remove(value);
    }

    public bool IsMaxDepth()
    {
        return CurrentDepth > maxDepth;
    }
}
