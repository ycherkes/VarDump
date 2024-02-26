using System;
using System.Net;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class IPEndpointVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeGenerator _codeGenerator;

    public IPEndpointVisitor(IObjectVisitor rootObjectVisitor, ICodeGenerator codeGenerator)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeGenerator = codeGenerator;
    }

    public string Id => nameof(IPEndPoint);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPEndPoint;
    }

    public void Visit(object obj, Type objectType)
    {
        var ipEndPoint = (IPEndPoint)obj;

        _codeGenerator.GenerateObjectCreateAndInitialize(
            new CodeTypeReference(typeof(IPEndPoint)),
            [
                () => _rootObjectVisitor.Visit(ipEndPoint.Address),
                () => _codeGenerator.GeneratePrimitive(ipEndPoint.Port)
            ],
            []);
    }
}