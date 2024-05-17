using System;
using VarDump.Visitor;

namespace VarDump.Utils
{
    internal static class NumericUtil
    {
        public static string ToString(object obj, IntegralNumericFormat numericFormat)
        {
            return numericFormat.Format switch
            {
                NumericFormat.Decimal => ToString(obj, numericFormat.ToString()),
                NumericFormat.Binary => numericFormat.Precision == 0 ? ConvertToString(obj, 2) : ConvertToString(obj, 2).PadLeft(numericFormat.Precision),
                NumericFormat.HexadecimalLowerCase => ToString(obj, numericFormat.ToString()),
                NumericFormat.HexadecimalUpperCase => ToString(obj, numericFormat.ToString()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static string ConvertToString(object value, int toBase)
        {
            return value switch
            {
                sbyte sb => Convert.ToString(sb, toBase),
                byte b => Convert.ToString(b, toBase),
                short s => Convert.ToString(s, toBase),
                int i => Convert.ToString(i, toBase),
                long l => Convert.ToString(l, toBase),
                ushort us => Convert.ToString(us, toBase),
                uint ui => Convert.ToString(ui, toBase),
                ulong ul => Convert.ToString((long)ul, toBase),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static string ToString(object value, string format)
        {
            return value switch
            {
                sbyte sb => sb.ToString(format),
                byte b => b.ToString(format),
                short s => s.ToString(format),
                int i => i.ToString(format),
                long l => l.ToString(format),
                ushort us => us.ToString(format),
                uint ui => ui.ToString(format),
                ulong ul => ul.ToString(format),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
