using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class AnonymousTypeVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly IObjectDescriptor _anonymousObjectDescriptor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public AnonymousTypeVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor,
        IObjectDescriptor anonymousObjectDescriptor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _anonymousObjectDescriptor = anonymousObjectDescriptor;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => "Anonymous";
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Type.IsAnonymousType();
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var result = new CodeObjectCreateAndInitializeExpression(new CodeAnonymousTypeReference())
        {
            InitializeExpressions = new CodeExpressionContainer(_anonymousObjectDescriptor.Describe(valueDescriptor.Value, valueDescriptor.Type)
                .Select(pv => (CodeExpression)new CodeAssignExpression(
                    new CodePropertyReferenceExpression(null, pv.Name),
                    pv.MemberType.IsNullableType() || pv.Value == null ? new CodeCastExpression(new CodeTypeReference(pv.MemberType, _typeReferenceOptions), _rootObjectVisitor.Visit(pv), true) : _rootObjectVisitor.Visit(pv)))
                )
        };

        return result;
    }
}