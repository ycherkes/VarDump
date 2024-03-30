// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;

namespace VarDump.CodeDom.CSharp;

internal static class CSharpHelpers
{
    private static readonly HashSet<string> Lookup;

    static CSharpHelpers()
    {
        Lookup = new HashSet<string>();
        for (int i = 0; i < Keywords.Length; i++)
        {
            var values = Keywords[i];
            if (values != null)
            {
                for (int j = 0; j < values.Length; j++)
                {
                    Lookup.Add(values[j]);
                }
            }
        }
    }

    public static string CreateEscapedIdentifier(string name)
    {
        // Any identifier started with two consecutive underscores are 
        // reserved by CSharp.
        if (IsKeyword(name) || IsPrefixTwoUnderscore(name))
        {
            return "@" + name;
        }
        return name;
    }

    public static bool IsValidIdentifier(string value)
    {
        // identifiers must be 1 char or longer
        //
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (value.Length > 512)
        {
            return false;
        }

        // identifiers cannot be a keyword, unless they are escaped with an '@'
        //
        if (value[0] != '@')
        {
            if (IsKeyword(value))
            {
                return false;
            }
        }
        else
        {
            value = value.Substring(1);
        }

        return IsValidTypeNameOrIdentifier(value, false);
    }

    private static readonly string[][] Keywords = [
        null,           // 1 character
        [  // 2 characters
            "as",
            "do",
            "if",
            "in",
            "is",
        ],
        [  // 3 characters
            "for",
            "int",
            "new",
            "out",
            "ref",
            "try",
        ],
        [  // 4 characters
            "base",
            "bool",
            "byte",
            "case",
            "char",
            "else",
            "enum",
            "goto",
            "lock",
            "long",
            "null",
            "this",
            "true",
            "uint",
            "void",
        ],
        [  // 5 characters
            "break",
            "catch",
            "class",
            "const",
            "event",
            "false",
            "fixed",
            "float",
            "sbyte",
            "short",
            "throw",
            "ulong",
            "using",
            "where",
            "while",
            "yield",
        ],
        [  // 6 characters
            "double",
            "extern",
            "object",
            "params",
            "public",
            "return",
            "sealed",
            "sizeof",
            "static",
            "string",
            "struct",
            "switch",
            "typeof",
            "unsafe",
            "ushort",
        ],
        [  // 7 characters
            "checked",
            "decimal",
            "default",
            "finally",
            "foreach",
            "partial",
            "private",
            "virtual",
        ],
        [  // 8 characters
            "abstract",
            "continue",
            "delegate",
            "explicit",
            "implicit",
            "internal",
            "operator",
            "override",
            "readonly",
            "volatile",
        ],
        [  // 9 characters
            "__arglist",
            "__makeref",
            "__reftype",
            "interface",
            "namespace",
            "protected",
            "unchecked",
        ],
        [  // 10 characters
            "__refvalue",
            "stackalloc",
        ],
    ];

    internal static bool IsKeyword(string value)
    {
        return Lookup.Contains(value);
    }

    internal static bool IsPrefixTwoUnderscore(string value)
    {
        if (value.Length < 3)
        {
            return false;
        }

        return value[0] == '_' && value[1] == '_' && value[2] != '_';
    }

    internal static bool IsValidTypeNameOrIdentifier(string value, bool isTypeName)
    {
        bool nextMustBeStartChar = true;

        if (value.Length == 0)
            return false;

        // each char must be Lu, Ll, Lt, Lm, Lo, Nd, Mn, Mc, Pc
        //
        for (int i = 0; i < value.Length; i++)
        {
            char ch = value[i];
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            switch (uc)
            {
                case UnicodeCategory.UppercaseLetter:        // Lu
                case UnicodeCategory.LowercaseLetter:        // Ll
                case UnicodeCategory.TitlecaseLetter:        // Lt
                case UnicodeCategory.ModifierLetter:         // Lm
                case UnicodeCategory.LetterNumber:           // Lm
                case UnicodeCategory.OtherLetter:            // Lo
                    nextMustBeStartChar = false;
                    break;

                case UnicodeCategory.NonSpacingMark:         // Mn
                case UnicodeCategory.SpacingCombiningMark:   // Mc
                case UnicodeCategory.ConnectorPunctuation:   // Pc
                case UnicodeCategory.DecimalDigitNumber:     // Nd
                    // Underscore is a valid starting character, even though it is a ConnectorPunctuation.
                    if (nextMustBeStartChar && ch != '_')
                        return false;

                    nextMustBeStartChar = false;
                    break;
                default:
                    // We only check the special Type chars for type names.
                    if (isTypeName && IsSpecialTypeChar(ch, ref nextMustBeStartChar))
                    {
                        break;
                    }

                    return false;
            }
        }

        return true;
    }

    // This can be a special character like a separator that shows up in a type name
    // This is an odd set of characters.  Some come from characters that are allowed by C++, like < and >.
    // Others are characters that are specified in the type and assembly name grammar.
    private static bool IsSpecialTypeChar(char ch, ref bool nextMustBeStartChar)
    {
        switch (ch)
        {
            case ':':
            case '.':
            case '$':
            case '+':
            case '<':
            case '>':
            case '-':
            case '[':
            case ']':
            case ',':
            case '&':
            case '*':
                nextMustBeStartChar = true;
                return true;

            case '`':
                return true;
        }
        return false;
    }
}