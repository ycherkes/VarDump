using System;
using System.Reflection;

namespace VarDump.Visitor.Descriptors;

public interface IValueDescriptor : ITypeDescriptor
{
    object Value { get; }
}

public class ValueDescriptor : TypeDescriptor, IValueDescriptor
{
    public virtual object Value { get; set; }
}

public class TypeDescriptor : ITypeDescriptor
{
    public virtual Type Type { get; set; }

    public NullabilityInfo[] GenericTypeArguments { get; set; } = [];
}

public interface ITypeDescriptor
{
    Type Type { get; }
    NullabilityInfo[] GenericTypeArguments { get; }
}