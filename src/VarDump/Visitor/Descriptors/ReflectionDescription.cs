using System;

namespace VarDump.Visitor.Descriptors;

public abstract record ReflectionDescription
{
    public abstract ReflectionType ReflectionType { get; }
    public abstract object Value { get; set; }
    public abstract string Name { get; set; }
    public abstract Type Type { get; set; }
}