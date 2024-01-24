using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.KnownTypes;

internal sealed class ValueTupleVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly IObjectDescriptor _descriptor;

    public ValueTupleVisitor(IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _descriptor = new ObjectFieldsDescriptor();
    }
    public string Id => "ValueTuple";
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Type.IsValueTuple();
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var propertyValues = _descriptor.Describe(valueDescriptor.Value, valueDescriptor.Type).Select(_rootObjectVisitor.Visit);

        return new CodeValueTupleCreateExpression(propertyValues);
    }
}