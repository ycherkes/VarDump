using System;
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
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsAnonymousType();
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var result = new CodeObjectCreateAndInitializeExpression(new CodeAnonymousTypeReference())
        {
            InitializeExpressions = new CodeExpressionCollection(_anonymousObjectDescriptor.Describe(obj, objectType)
                .Select(pv => (CodeExpression)new CodeAssignExpression(
                    new CodePropertyReferenceExpression(null, pv.Name),
                    pv.Type.IsNullableType() || pv.Value == null ? new CodeCastExpression(new CodeTypeReference(pv.Type, _typeReferenceOptions), _rootObjectVisitor.Visit(pv.Value), true) : _rootObjectVisitor.Visit(pv.Value)))
                )
        };

        return result;
    }
}