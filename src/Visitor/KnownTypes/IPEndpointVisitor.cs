using System;
using System.Net;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.KnownTypes;

internal sealed class IPEndpointVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public IPEndpointVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => nameof(IPEndPoint);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPEndPoint;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var ipEndPoint = (IPEndPoint)obj;
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(IPEndPoint), _typeReferenceOptions),
            _rootObjectVisitor.Visit(ipEndPoint.Address),
            new CodePrimitiveExpression(ipEndPoint.Port));
    }
}