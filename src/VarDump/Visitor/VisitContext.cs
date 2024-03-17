using System.Collections.Generic;
using System.Linq;
using VarDump.Collections;

namespace VarDump.Visitor;

public sealed class VisitContext(int maxDepth)
{
    private static readonly IEqualityComparer<object> IdentityComparer = new ObjectIdentityComparer();
    private readonly Stack<object> _visitedObjects = new();

    public int CurrentDepth { get; set; }

    public void PushVisited(object value)
    {
        _visitedObjects.Push(value);
    }

    public void PopVisited()
    {
        _visitedObjects.Pop();
    }

    public bool IsVisited(object value)
    {
        return value != null && _visitedObjects.Contains(value, IdentityComparer);
    }

    public bool IsMaxDepth()
    {
        return CurrentDepth > maxDepth;
    }
}
