using System;

namespace VarDump.Visitor.Descriptors;

public interface IReflectionDescription
{
    ReflectionType ReflectionType { get; }
    string Name { get; }
    Type Type { get; }
    object Value { get; }
}