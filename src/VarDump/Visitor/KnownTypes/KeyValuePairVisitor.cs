using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.KnownTypes;

internal sealed class KeyValuePairVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly IObjectDescriptor _descriptor;

    public KeyValuePairVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
        _descriptor = new ObjectPropertiesDescriptor();
    }

    public string Id => "KeyValuePair";
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Type.IsKeyValuePair();
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var propertyValues = _descriptor.Describe(valueDescriptor.Value, valueDescriptor.Type).Select(rd => _rootObjectVisitor.Visit(rd));
        return new CodeObjectCreateExpression(new CodeTypeReference(valueDescriptor.Type, _typeReferenceOptions),
            propertyValues.ToArray());
    }
}