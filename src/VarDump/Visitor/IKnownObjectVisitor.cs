using System;

namespace VarDump.Visitor;

public interface IKnownObjectVisitor : ICurrentDepthVisitor
{
    bool IsSuitableFor(object obj, Type objectType);
}