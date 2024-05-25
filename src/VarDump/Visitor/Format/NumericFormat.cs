using System;

namespace VarDump.Visitor.Format;

[Flags]
internal enum NumericFormat
{
    Decimal = 2,
    Binary = 4,
    Hexadecimal = 8,
    UpperCase = 0x40000000
}