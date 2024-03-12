using System;

namespace VarDump.Visitor;

public interface IKnownObjectVisitor : ICurrentLevelVisitor
{
    bool IsSuitableFor(object obj, Type objectType);
}