using VarDump.CodeDom.Common;

namespace VarDump.Visitor;

internal interface IObjectVisitor
{
    CodeExpression Visit(object @object);
    void RegisterVisited(object value);
    void UnregisterVisited(object value);
    bool IsVisited(object value);
}