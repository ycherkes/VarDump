using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor;

internal interface IObjectVisitor
{
    CodeExpression Visit(IValueDescriptor descriptor);
    void PushVisited(object value);
    void PopVisited();
    bool IsVisited(object value);
}