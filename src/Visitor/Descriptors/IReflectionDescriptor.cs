using System;

namespace VarDump.Visitor.Descriptors;

public interface IReflectionDescriptor
{
    ReflectionDetails ReflectionDetails { get; }
    ReflectionType ReflectionType { get; }
    string Name { get; }
    Type Type { get; }
    object Value { get; }
}