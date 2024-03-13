using System;
using System.Collections.Generic;
using System.Net;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownObjects;

internal sealed class IPEndpointVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, DumpOptions options) : IKnownObjectVisitor
{
    public string Id => nameof(IPEndPoint);

    public DumpOptions Options => options;

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IPEndPoint;
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
            yield return () => nextDepthVisitor.Visit(ipEndPoint.Address, context);
            yield return () => codeWriter.WritePrimitive(ipEndPoint.Port);
        }

        IEnumerable<Action> GetNamedConstructorArguments()
        {
            yield return () => codeWriter.WriteNamedArgument("address", () => nextDepthVisitor.Visit(ipEndPoint.Address, context));
            yield return () => codeWriter.WriteNamedArgument("port", () => codeWriter.WritePrimitive(ipEndPoint.Port));
        }
    }
}