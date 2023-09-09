using System;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.KnownTypes;

internal interface IKnownObjectVisitor
{
    public string Id { get; }

    bool IsSuitableFor(object obj, Type objectType);

    CodeExpression Visit(object obj, Type objectType);
}