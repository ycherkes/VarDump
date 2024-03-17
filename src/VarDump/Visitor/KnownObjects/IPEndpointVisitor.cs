using System;
using System.Collections.Generic;
using System.Net;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class IPEndpointVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => nameof(IPEndPoint);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPEndPoint;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        options = options.Clone();
        configure?.Invoke(options);
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var ipEndPoint = (IPEndPoint)obj;

        var constructorArguments = options.UseNamedArgumentsInConstructors
            ? GetNamedConstructorArguments()
            : GetConstructorArguments();

        codeWriter.WriteObjectCreate(typeof(IPEndPoint), constructorArguments);
        return;

        IEnumerable<Action> GetConstructorArguments()
        {
            yield return WriteAddress;
            yield return WritePort;
        }

        IEnumerable<Action> GetNamedConstructorArguments()
        {
            yield return () => codeWriter.WriteNamedArgument("address", WriteAddress);
            yield return () => codeWriter.WriteNamedArgument("port", WritePort);
        }

        void WriteAddress() => nextDepthVisitor.Visit(ipEndPoint.Address, context);
        void WritePort() => codeWriter.WritePrimitive(ipEndPoint.Port);
    }
}