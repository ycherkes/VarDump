using VarDump.CodeDom.Common;

namespace VarDump.Visitor
{
    internal interface IObjectVisitor
    {
        CodeExpression Visit(object @object);
        void PushVisited(object value);
        void PopVisited();
        bool IsVisited(object value);
    }
}
