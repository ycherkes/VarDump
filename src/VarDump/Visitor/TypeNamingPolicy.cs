namespace VarDump.Visitor;

public enum TypeNamingPolicy
{
    ShortName = 0,  // Uses short type name
    NormalName = 1, // Uses short type name for regular types and partially full name for nested types.
    FullName = 2    // Uses full type name
}