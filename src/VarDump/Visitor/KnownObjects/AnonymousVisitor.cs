using System;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.KnownObjects;

internal sealed class AnonymousVisitor : IKnownObjectVisitor
{
    private readonly INextDepthVisitor _nextDepthVisitor;
    private readonly IObjectDescriptor _anonymousObjectDescriptor;
    private readonly ICodeWriter _codeWriter;

    public AnonymousVisitor(INextDepthVisitor nextDepthVisitor,
        ICodeWriter codeWriter,
        DumpOptions options)
    {
        _nextDepthVisitor = nextDepthVisitor;
        _codeWriter = codeWriter;
        _anonymousObjectDescriptor = new ObjectPropertiesDescriptor(options.GetPropertiesBindingFlags, false);

        if (options.Descriptors?.Count > 0)
            _anonymousObjectDescriptor = _anonymousObjectDescriptor.ApplyMiddleware(options.Descriptors);
    }

    public string Id => "Anonymous";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsAnonymousType();
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var properties = _anonymousObjectDescriptor.GetObjectDescription(obj, objectType).Properties;

        _codeWriter.WriteObjectCreateAndInitializeItems(new CodeAnonymousTypeInfo(), [], properties, property =>
        {
            var description = (PropertyDescription)property;
            _codeWriter.WriteMemberAssignmentStart(description.Name);
            if (description.Type.IsNullableType() || description.Value == null)
            {
                _codeWriter.WriteCast(description.Type, () => _nextDepthVisitor.Visit(description.Value, context));
            }
            else
            {
                _nextDepthVisitor.Visit(description.Value, context);
            }
        });
    }
}
