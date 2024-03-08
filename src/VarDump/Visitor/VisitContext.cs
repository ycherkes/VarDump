using System.Collections.Generic;

namespace VarDump.Visitor;

public sealed class VisitContext(int maxDepth)
{
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
        return value != null && _visitedObjects.Contains(value);
    }

    public bool IsMaxDepth()
    {
        return CurrentDepth > maxDepth;
    }
}