using System;
using System.Net;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class IPEndpointVisitor(IRootVisitor rootVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
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
                () => rootVisitor.Visit(ipEndPoint.Address, context),
                () => codeWriter.WritePrimitive(ipEndPoint.Port)
            ]);
    }
}