using System;

namespace VarDump.Visitor;

public interface IKnownObjectVisitor : ISpecificVisitor
{
    public string Id { get; }
    public DumpOptions Options { get; }
    bool IsSuitableFor(object obj, Type objectType);
}