using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.KnownTypes;

internal sealed class TupleVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly IObjectDescriptor _descriptor;

    public TupleVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
        _descriptor = new ObjectPropertiesDescriptor();
    }

    public string Id => "Tuple";
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Type.IsTuple();
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        if (_rootObjectVisitor.IsVisited(valueDescriptor.Value))
        {
            return CodeDomUtils.GetCircularReferenceDetectedExpression();
        }

        _rootObjectVisitor.PushVisited(valueDescriptor.Value);

        try
        {
            var propertyValues = _descriptor.Describe(valueDescriptor.Value, valueDescriptor.Type).Select(_rootObjectVisitor.Visit);
            var result = new CodeObjectCreateExpression(new CodeTypeReference(valueDescriptor.Type, _typeReferenceOptions), propertyValues);
            return result;
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }
}