using System.Net;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class IPAddressVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public IPAddressVisitor(DumpOptions options)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => nameof(IPAddress);
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is IPAddress;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        var ipAddress = (IPAddress)valueDescriptor.Value;
        return new CodeMethodInvokeExpression(
            new CodeMethodReferenceExpression(
                new CodeTypeReferenceExpression(
                    new CodeTypeReference(typeof(IPAddress), _typeReferenceOptions)),
                nameof(IPAddress.Parse)),
            new CodePrimitiveExpression(ipAddress.ToString()));
    }
}