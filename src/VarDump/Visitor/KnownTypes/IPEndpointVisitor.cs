using System;
using System.Net;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class IPEndpointVisitor(IRootObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(IPEndPoint);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPEndPoint;
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var ipEndPoint = (IPEndPoint)obj;

        codeWriter.WriteObjectCreate(
            typeof(IPEndPoint),
            [
                () => rootObjectVisitor.Visit(ipEndPoint.Address, context),
                () => codeWriter.WritePrimitive(ipEndPoint.Port)
            ]);
    }
}