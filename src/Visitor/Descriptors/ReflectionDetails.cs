using System;

namespace VarDump.Visitor.Descriptors;

[Flags]
public enum ReflectionDetails
{
    None,
    ReadonlyCollectionProperty
}