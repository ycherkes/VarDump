using System;
using System.Net;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class IPEndpointVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;

    public IPEndpointVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
    }

    public string Id => nameof(IPEndPoint);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPEndPoint;
    }

    public void Visit(object obj, Type objectType)
    {
        var ipEndPoint = (IPEndPoint)obj;

        _codeWriter.WriteObjectCreateAndInitialize(
            typeof(IPEndPoint),
            [
                () => _rootObjectVisitor.Visit(ipEndPoint.Address),
                () => _codeWriter.WritePrimitive(ipEndPoint.Port)
            ],
            []);
    }
}