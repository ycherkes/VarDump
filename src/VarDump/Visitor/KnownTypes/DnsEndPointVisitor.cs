using System.Net;
using System.Net.Sockets;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DnsEndPointVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public DnsEndPointVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => nameof(DnsEndPoint);
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is DnsEndPoint;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var dnsEndPoint = (DnsEndPoint)valueDescriptor.Value;
        return dnsEndPoint.AddressFamily == AddressFamily.Unspecified ?
            new CodeObjectCreateExpression(new CodeTypeReference(typeof(DnsEndPoint), _typeReferenceOptions),
                new CodePrimitiveExpression(dnsEndPoint.Host),
                new CodePrimitiveExpression(dnsEndPoint.Port))
            : new CodeObjectCreateExpression(new CodeTypeReference(typeof(DnsEndPoint), _typeReferenceOptions),
                new CodePrimitiveExpression(dnsEndPoint.Host),
                new CodePrimitiveExpression(dnsEndPoint.Port),
                _rootObjectVisitor.Visit(new ValueDescriptor{Value = dnsEndPoint.AddressFamily, Type = typeof(AddressFamily)}));
    }
}