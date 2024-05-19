using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VarDump.Visitor;

namespace VarDump.Utils
{
    internal static class NumericUtil
    {
        public static string ToString(object obj, IntegralNumericFormat numericFormat)
        {
            var value = ToStringInternal(obj, numericFormat);
            if (numericFormat.UnderscorePosition > 0)
            {
                var characters = new List<char>(value.Length + value.Length / numericFormat.UnderscorePosition);
 
                for (var index = 0; index < value.Length; index++)
                {
                    var c = value[value.Length - 1 - index];
                    if (index % numericFormat.UnderscorePosition == 0 && index > 0)
                    {
                        characters.Add('_');
                    }

                    characters.Add(c);
                }

                char[] charArray = characters.ToArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }

            return value;
        }

        private static string ToStringInternal(object obj, IntegralNumericFormat numericFormat)
        {
            return numericFormat.Format switch
            {
                NumericFormat.Decimal => ToString(obj, numericFormat.ToCompatibleString()),
                NumericFormat.Decimal | NumericFormat.UpperCase => ToString(obj, numericFormat.ToCompatibleString()),
                NumericFormat.Binary => numericFormat.Digits < 0 ? ConvertToString(obj, 2) : ConvertToString(obj, 2).PadLeft(numericFormat.Digits),
                NumericFormat.Binary | NumericFormat.UpperCase => numericFormat.Digits < 0 ? ConvertToString(obj, 2).ToUpper() : ConvertToString(obj, 2).PadLeft(numericFormat.Digits).ToUpper(),
                NumericFormat.Hexadecimal => ToString(obj, numericFormat.ToCompatibleString()),
                NumericFormat.Hexadecimal | NumericFormat.UpperCase => ToString(obj, numericFormat.ToCompatibleString()),
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
