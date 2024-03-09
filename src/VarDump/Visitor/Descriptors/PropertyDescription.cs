using System;

namespace VarDump.Visitor.Descriptors;

public sealed record PropertyDescription : MemberDescription
{
    public PropertyDescription(object value) : base(value)
    {
    }

    public PropertyDescription(Func<object> getValueFunc) : base(getValueFunc)
    {
    }

    public override ReflectionType ReflectionType => ReflectionType.Property;
    public override string Name { get; set; }
    public override Type Type { get; set; }
}