using System;

namespace VarDump.Visitor;

public interface IKnownObjectVisitor : ISpecificObjectVisitor
{
    bool IsSuitableFor(object obj, Type objectType);
}