using System;

namespace VarDump.Visitor;

public interface ICurrentLevelVisitor
{
    void Visit(object obj, Type objectType, VisitContext context);
}