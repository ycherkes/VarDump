using System;

namespace VarDump.Visitor;

public interface ISpecificVisitor
{
    void Visit(object obj, Type objectType, VisitContext context);
}