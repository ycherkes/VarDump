using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownObjects;

internal sealed class AnonymousVisitor(
    INextDepthVisitor nextDepthVisitor,
    IObjectDescriptor anonymousObjectDescriptor,
    ICodeWriter codeWriter,
    DumpOptions options)
    : IKnownObjectVisitor
{
    public string Id => "Anonymous";

    public DumpOptions Options => options;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsAnonymousType();
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var initializeActions = anonymousObjectDescriptor.GetObjectDescription(obj, objectType)
            .Properties
            .Select(pv => (Action)(() => codeWriter.WriteAssign(
                () => codeWriter.WritePropertyReference(pv.Name, null),
                () =>
                {
                    if (pv.Type.IsNullableType() || pv.Value == null)
                    {
                        codeWriter.WriteCast(pv.Type, () => nextDepthVisitor.Visit(pv.Value, context));
                    }
                    else
                    {
                        nextDepthVisitor.Visit(pv.Value, context);
                    }
                })));

        codeWriter.WriteObjectCreateAndInitialize(new CodeAnonymousTypeInfo(), [], initializeActions);
    }
}