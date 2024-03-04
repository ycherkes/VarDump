using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class AnonymousTypeVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly IObjectDescriptor _anonymousObjectDescriptor;
    private readonly ICodeWriter _codeWriter;

    public AnonymousTypeVisitor(IObjectVisitor rootObjectVisitor,
        IObjectDescriptor anonymousObjectDescriptor, ICodeWriter codeWriter)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _anonymousObjectDescriptor = anonymousObjectDescriptor;
        _codeWriter = codeWriter;
    }

    public string Id => "Anonymous";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsAnonymousType();
    }

    public void Visit(object obj, Type objectType)
    {
        var initializeActions = _anonymousObjectDescriptor.Describe(obj, objectType)
            .Members
            .Select(pv => (Action)(() => _codeWriter.WriteAssign(
                () => _codeWriter.WritePropertyReference(pv.Name, null),
                () =>
                {
                    if (pv.Type.IsNullableType() || pv.Value == null)
                    {
                        _codeWriter.WriteCast(pv.Type, 
                            () => _rootObjectVisitor.Visit(pv.Value));
                    }
                    else
                    {
                        _rootObjectVisitor.Visit(pv.Value);
                    }
                })));

        _codeWriter.WriteObjectCreateAndInitialize(new AnonymousTypeReference(),
            [],
            initializeActions);
    }
}