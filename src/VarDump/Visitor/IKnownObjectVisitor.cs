using System;

namespace VarDump.Visitor;

public interface IKnownObjectVisitor : ICurrentDepthVisitor
{
    public string Id { get; }
    public DumpOptions Options { get; }
    bool IsSuitableFor(object obj, Type objectType);
}