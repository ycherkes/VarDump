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
    
    public bool CanWrite { get; set; }
    public override string Name { get; set; }
    public override ReflectionType ReflectionType => ReflectionType.Property;
    public override Type Type { get; set; }
}