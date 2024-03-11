using System;

namespace VarDump.Visitor;

public interface ISpecificObjectVisitor
{
    void Visit(object obj, Type objectType, VisitContext context);
}