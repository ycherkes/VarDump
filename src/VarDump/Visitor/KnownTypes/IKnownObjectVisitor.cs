using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal interface IKnownObjectVisitor
{
    public string Id { get; }

    bool IsSuitableFor(IValueDescriptor valueDescriptor);

    CodeExpression Visit(IValueDescriptor valueDescriptor);
}