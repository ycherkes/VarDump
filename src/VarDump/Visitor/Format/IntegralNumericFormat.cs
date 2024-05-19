using System;
using System.Collections.Concurrent;

namespace VarDump.Visitor.Format;

internal struct IntegralNumericFormat
{
    public NumericFormat Format { get; set; }
    public int Digits { get; set; }
    public int UnderscorePosition { get; set; }

    private static readonly ConcurrentDictionary<string, Result> Cache = new();

    public static bool TryParse(string formatString, out IntegralNumericFormat result)
    {
        if (IsDefault(formatString))
        {
            result = new IntegralNumericFormat
            {
                Format = NumericFormat.Decimal
            };

            return true;
        }

        result = default;

        var parseResult = Cache.GetOrAdd(formatString, ParseFormatSpecifier);

        if (!parseResult.Success)
        {
            return false;
        }

        var numericFormat = parseResult.Format switch
        {
            'd' => NumericFormat.Decimal,
            'D' => NumericFormat.Decimal | NumericFormat.UpperCase,
            'b' => NumericFormat.Binary,
            'B' => NumericFormat.Binary | NumericFormat.UpperCase,
            'x' => NumericFormat.Hexadecimal,
            'X' => NumericFormat.Hexadecimal | NumericFormat.UpperCase,
            _ => (NumericFormat)0
        };

        if (numericFormat == 0)
        {
            return false;
        }

        result = new IntegralNumericFormat
        {
            Format = numericFormat,
            Digits = parseResult.Digits,
            UnderscorePosition = parseResult.Underscores
        };

        return true;
    }

    private static bool IsDefault(string formatString)
    {
        return string.IsNullOrEmpty(formatString)
               || "D".Equals(formatString, StringComparison.OrdinalIgnoreCase)
               || "D0".Equals(formatString, StringComparison.OrdinalIgnoreCase);
    }

    private struct Result
    {
        public bool Success;
        public char Format;
        public int Digits;
        public int Underscores;
    }

    private static Result ParseFormatSpecifier(string format)
    {
        // Default empty format to be "D".
        if (format.Length == 0)
        {
            return new Result { Success = true, Format = 'D', Digits = -1, Underscores = -1 };
        }

        // If the format begins with a symbol, see if it's a standard format
        // with or without a specified number of digits.
        var c = format[0];
        if (!char.IsLetter(c))
        {
            return new Result { Success = false, Format = 'D', Digits = -1, Underscores = -1 };
        }

        // Fast path for sole symbol, e.g. "D"
        if (format.Length == 1)
        {
            return new Result { Success = true, Format = c, Digits = -1, Underscores = -1 };
        }

        if (format.Length == 2)
        {
            // Fast path for symbol and single digit, e.g. "X4"
            int d = format[1] - '0';
            if ((uint)d < 10)
            {
                return new Result { Success = true, Format = c, Digits = d, Underscores = -1 };
            }
        }
        else if (format.Length == 3)
        {
            // Fast path for symbol and double digit, e.g. "B12"
            int d1 = format[1] - '0', d2 = format[2] - '0';
            if ((uint)d1 < 10 && (uint)d2 < 10)
            {
                return new Result { Success = true, Format = c, Digits = d1 * 10 + d2, Underscores = -1 };
            }
        }

        // Fallback for symbol and any length digits.  The digits value must be >= 0 && <= 999_999_999,
        // but it can begin with any number of 0s, and thus we may need to check more than 9
        // digits.  Further, for compat, we need to stop when we hit a null char.
        int n = 0;
        int i = 1;
        while ((uint)i < (uint)format.Length && char.IsDigit(format[i]))
        {
            // Check if we are about to overflow past our limit of 9 digits
            if (n >= 100_000_000)
            {
                return new Result { Success = false, Format = c, Digits = -1, Underscores = -1 };
            }
            n = n * 10 + format[i++] - '0';
        }

        int u = 0;
        int ui = 0;
        if ((uint)i < (uint)format.Length && format[i] == '_')
        {
            ui = i + 1;
            while ((uint)ui < (uint)format.Length && char.IsDigit(format[ui]))
            {
                // Check if we are about to overflow past our limit of 9 digits
                if (u >= 100_000_000)
                {
                    return new Result { Success = false, Format = c, Digits = n, Underscores = -1 };
                }
                u = u * 10 + format[ui++] - '0';
            }
        }

        // If we're at the end of the digits rather than having stopped because we hit something
        // other than a digit or overflowed, return the standard format info.
        if ((uint)i >= (uint)format.Length || (uint)ui >= (uint)format.Length || format[i] == '\0')
        {
            return new Result { Success = true, Format = c, Digits = n, Underscores = u };
        }

        return new Result { Success = false, Format = c, Digits = n, Underscores = u };
    }

    public readonly override string ToString()
    {
        var fmt = Format switch
        {
            NumericFormat.Binary => "b",
            NumericFormat.Binary | NumericFormat.UpperCase => "B",
            NumericFormat.Decimal => "d",
            NumericFormat.Decimal | NumericFormat.UpperCase => "D",
            NumericFormat.Hexadecimal => "x",
            NumericFormat.Hexadecimal | NumericFormat.UpperCase => "X",
            _ => throw new ArgumentOutOfRangeException()
        };

        if (Digits > 0)
        {
            fmt += Digits;
        }

        if (UnderscorePosition > 0)
        {
            fmt += $"_{UnderscorePosition}";
        }

        return fmt;
    }

    internal readonly string ToCompatibleString()
    {
        var fmt = Format switch
        {
            NumericFormat.Binary => "b",
            NumericFormat.Binary | NumericFormat.UpperCase => "B",
            NumericFormat.Decimal => "d",
            NumericFormat.Decimal | NumericFormat.UpperCase => "D",
            NumericFormat.Hexadecimal => "x",
            NumericFormat.Hexadecimal | NumericFormat.UpperCase => "X",
            _ => throw new ArgumentOutOfRangeException()
        };

        if (Digits > 0)
        {
            fmt += Digits;
        }

        return fmt;
    }
}