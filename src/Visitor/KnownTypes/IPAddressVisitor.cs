using System;
using System.Net;
using VarDumpExtended.CodeDom.Common;

namespace VarDumpExtended.Visitor.KnownTypes;

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
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPAddress;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        var ipAddress = (IPAddress)obj;
        return new CodeMethodInvokeExpression(
            new CodeMethodReferenceExpression(
                new CodeTypeReferenceExpression(
                    new CodeTypeReference(typeof(IPAddress), _typeReferenceOptions)),
                nameof(IPAddress.Parse)),
            new CodePrimitiveExpression(ipAddress.ToString()));
    }
}