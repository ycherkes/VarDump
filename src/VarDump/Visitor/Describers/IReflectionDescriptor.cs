using System;

namespace VarDump.Visitor.Describers;

public interface IReflectionDescriptor
{
    ReflectionType ReflectionType { get; }
    string Name { get; }
    Type Type { get; }
    object Value { get; }
}