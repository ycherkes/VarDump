using System;

namespace VarDump.Visitor.KnownTypes;

public interface IKnownObjectVisitor
{
    public string Id { get; }

    bool IsSuitableFor(object obj, Type objectType);

    void Visit(object obj, Type objectType, VisitContext context);
}