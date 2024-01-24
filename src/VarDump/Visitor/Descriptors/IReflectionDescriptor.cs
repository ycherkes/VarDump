using System;

namespace VarDump.Visitor.Descriptors;

public interface IReflectionDescriptor : IValueDescriptor
{
    public Type MemberType { get; }
    ReflectionType ReflectionType { get; }
    string Name { get; }
}