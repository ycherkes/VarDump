using VarDumpExtended.CodeDom.Common;

namespace VarDumpExtended.Visitor;

internal interface IObjectVisitor
{
    CodeExpression Visit(object @object);
    void PushVisited(object value);
    void PopVisited();
    bool IsVisited(object value);
}