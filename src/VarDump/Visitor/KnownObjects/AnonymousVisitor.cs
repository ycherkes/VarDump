using System;
using System.Linq;
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
        {
            _anonymousObjectDescriptor = _anonymousObjectDescriptor.ApplyMiddleware(options.Descriptors);
        }
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
        var initializeActions = _anonymousObjectDescriptor.GetObjectDescription(obj, objectType)
            .Properties
            .Select(pv => (Action)(() => _codeWriter.WriteAssign(
                () => _codeWriter.WritePropertyReference(pv.Name, null),
                () =>
                {
                    if (pv.Type.IsNullableType() || pv.Value == null)
                    {
                        _codeWriter.WriteCast(pv.Type, () => _nextDepthVisitor.Visit(pv.Value, context));
                    }
                    else
                    {
                        _nextDepthVisitor.Visit(pv.Value, context);
                    }
                })));

        _codeWriter.WriteObjectCreateAndInitialize(new CodeAnonymousTypeInfo(), [], initializeActions);
    }
}