namespace VarDump.Visitor;

public enum TypeNamingPolicy
{
    ShortName = 0,       // Short type name
    NestedQualified = 1, // Include the enclosing class name for nested types, but not the namespace.
    FullName = 2         // Full type name
}