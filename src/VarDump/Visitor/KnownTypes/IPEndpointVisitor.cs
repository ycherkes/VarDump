using System.Net;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

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
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is IPEndPoint;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var ipEndPoint = (IPEndPoint)valueDescriptor.Value;
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(IPEndPoint), _typeReferenceOptions),
            _rootObjectVisitor.Visit(new ValueDescriptor { Value = ipEndPoint.Address, Type = typeof(IPAddress) }),
            new CodePrimitiveExpression(ipEndPoint.Port));
    }
}