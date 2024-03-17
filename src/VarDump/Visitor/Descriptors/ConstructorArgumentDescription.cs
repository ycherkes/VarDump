using System;

namespace VarDump.Visitor.Descriptors;

public sealed record ConstructorArgumentDescription : ReflectionDescription
{
    public override ReflectionType ReflectionType => ReflectionType.ConstructorArgument;
    public override object Value { get; set; }
    public override string Name { get; set; }
    public override Type Type { get; set; }
}