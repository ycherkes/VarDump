using System;
using System.Net;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class IPEndpointVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(IPEndPoint);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPEndPoint;
    }

    public void Visit(object obj, Type objectType)
    {
        var ipEndPoint = (IPEndPoint)obj;

        codeWriter.WriteObjectCreate(
            typeof(IPEndPoint),
            [
                () => rootObjectVisitor.Visit(ipEndPoint.Address),
                () => codeWriter.WritePrimitive(ipEndPoint.Port)
            ]);
    }
}