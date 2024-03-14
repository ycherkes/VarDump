using System;

namespace VarDump.Visitor;

public interface IKnownObjectVisitor : ISpecificVisitor
{
    string Id { get; }
    bool IsSuitableFor(object obj, Type objectType);
    void ConfigureOptions(Action<DumpOptions> configure);
}