using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;
using VarDump.Visitor.Describers;

namespace VarDump.Visitor.KnownTypes;

internal sealed class AnonymousTypeVisitor(
    IObjectVisitor rootObjectVisitor,
    IObjectDescriber anonymousObjectDescriber,
    ICodeWriter codeWriter)
    : IKnownObjectVisitor
{
    public string Id => "Anonymous";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsAnonymousType();
    }

    public void Visit(object obj, Type objectType)
    {
        var initializeActions = anonymousObjectDescriber.DescribeObject(obj, objectType)
            .Members
            .Select(pv => (Action)(() => codeWriter.WriteAssign(
                () => codeWriter.WritePropertyReference(pv.Name, null),
                () =>
                {
                    if (pv.Type.IsNullableType() || pv.Value == null)
                    {
                        codeWriter.WriteCast(pv.Type, 
                            () => rootObjectVisitor.Visit(pv.Value));
                    }
                    else
                    {
                        rootObjectVisitor.Visit(pv.Value);
                    }
                })));

        codeWriter.WriteObjectCreateAndInitialize(new CodeAnonymousTypeInfo(),
            [],
            initializeActions);
    }
}