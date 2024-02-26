namespace VarDump.Visitor;

public interface IObjectVisitor
{
    void Visit(object @object);
    void PushVisited(object value);
    void PopVisited();
    bool IsVisited(object value);
}