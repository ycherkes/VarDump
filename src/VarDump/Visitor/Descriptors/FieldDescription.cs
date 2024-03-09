using System;

namespace VarDump.Visitor.Descriptors;

public sealed record FieldDescription : MemberDescription
{
    public FieldDescription(object value) : base(value)
    {
    }

    public FieldDescription(Func<object> getValueFunc) : base(getValueFunc)
    {
    }

    public override ReflectionType ReflectionType => ReflectionType.Field;
    public override string Name { get; set; }
    public override Type Type { get; set; }
}