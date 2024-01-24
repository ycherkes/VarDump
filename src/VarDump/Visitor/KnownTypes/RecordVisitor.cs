using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.KnownTypes;

internal sealed class RecordVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly bool _useNamedArgumentsForReferenceRecordTypes;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly IObjectDescriptor _descriptor;

    public RecordVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _useNamedArgumentsForReferenceRecordTypes = options.UseNamedArgumentsForReferenceRecordTypes;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
        _descriptor = new ObjectPropertiesDescriptor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, true);
    }

    public string Id => "Record";
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Type.IsRecord();
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var properties = _descriptor.Describe(valueDescriptor.Value, valueDescriptor.Type);
        var argumentValues = _useNamedArgumentsForReferenceRecordTypes 
            ? properties.Select(p => (CodeExpression)new CodeNamedArgumentExpression(p.Name, _rootObjectVisitor.Visit(p)))
            : properties.Select(_rootObjectVisitor.Visit);

        return new CodeObjectCreateExpression(
            new CodeTypeReference(valueDescriptor.Type, _typeReferenceOptions),
            argumentValues);
    }
}