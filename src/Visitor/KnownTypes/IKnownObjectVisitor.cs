using System;
using VarDumpExtended.CodeDom.Common;

namespace VarDumpExtended.Visitor.KnownTypes;

public interface IKnownObjectVisitor
{
    public string Id { get; }

    bool IsSuitableFor(object obj, Type objectType);

    CodeExpression Visit(object obj, Type objectType);
}