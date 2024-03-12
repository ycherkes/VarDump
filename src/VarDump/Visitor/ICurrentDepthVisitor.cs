using System;

namespace VarDump.Visitor;

public interface ICurrentDepthVisitor
{
    void Visit(object obj, Type objectType, VisitContext context);
}