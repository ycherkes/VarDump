using VarDumpExtended.CodeDom.Common;

namespace VarDumpExtended.Visitor;

public interface IObjectVisitor
{
    CodeExpression Visit(object @object);
    void PushVisited(object value);
    void PopVisited();
    bool IsVisited(object value);
}