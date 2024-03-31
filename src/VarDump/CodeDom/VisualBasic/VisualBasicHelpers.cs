// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using VarDump.CodeDom.CSharp;

namespace VarDump.CodeDom.VisualBasic;

internal static class VisualBasicHelpers
{
    private static readonly HashSet<string> Lookup;

    static VisualBasicHelpers()
    {
        Lookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

    // This is the keyword list.
    private static readonly string[][] Keywords =
    [
        null,           // 1 character
        [  // 2 characters            
            "as",
            "do",
            "if",
            "in",
            "is",
            "me",
            "of",
            "on",
            "or",
            "to",
        ],
        [  // 3 characters
            "and",
            "dim",
            "end",
            "for",
            "get",
            "let",
            "lib",
            "mod",
            "new",
            "not",
            "rem",
            "set",
            "sub",
            "try",
            "xor",
        ],
        [  // 4 characters
            "ansi",
            "auto",
            "byte",
            "call",
            "case",
            "cdbl",
            "cdec",
            "char",
            "cint",
            "clng",
            "cobj",
            "csng",
            "cstr",
            "date",
            "each",
            "else",
            "enum",
            "exit",
            "goto",
            "like",
            "long",
            "loop",
            "next",
            "step",
            "stop",
            "then",
            "true",
            "wend",
            "when",
            "with",
        ],
        [  // 5 characters
            "alias",
            "byref",
            "byval",
            "catch",
            "cbool",
            "cbyte",
            "cchar",
            "cdate",
            "class",
            "const",
            "ctype",
            "cuint",
            "culng",
            "endif",
            "erase",
            "error",
            "event",
            "false",
            "gosub",
            "isnot",
            "redim",
            "sbyte",
            "short",
            "throw",
            "ulong",
            "until",
            "using",
            "while",
        ],
        [  // 6 characters
            "csbyte",
            "cshort",
            "double",
            "elseif",
            "friend",
            "global",
            "module",
            "mybase",
            "object",
            "option",
            "orelse",
            "public",
            "resume",
            "return",
            "select",
            "shared",
            "single",
            "static",
            "string",
            "typeof",
            "ushort",
        ],
        [ // 7 characters
            "andalso",
            "boolean",
            "cushort",
            "decimal",
            "declare",
            "default",
            "finally",
            "gettype",
            "handles",
            "imports",
            "integer",
            "myclass",
            "nothing",
            "partial",
            "private",
            "shadows",
            "trycast",
            "unicode",
            "variant",
        ],
        [  // 8 characters
            "assembly",
            "continue",
            "delegate",
            "function",
            "inherits",
            "operator",
            "optional",
            "preserve",
            "property",
            "readonly",
            "synclock",
            "uinteger",
            "widening"
        ],
        [ // 9 characters
            "addressof",
            "interface",
            "namespace",
            "narrowing",
            "overloads",
            "overrides",
            "protected",
            "structure",
            "writeonly",
        ],
        [ // 10 characters
            "addhandler",
            "directcast",
            "implements",
            "paramarray",
            "raiseevent",
            "withevents",
        ],
        [  // 11 characters
            "mustinherit",
            "overridable",
        ],
        [ // 12 characters
            "mustoverride",
        ],
        [ // 13 characters
            "removehandler",
        ],
        // class_finalize and class_initialize are not keywords anymore,
        // but it will be nice to escape them to avoid warning
        [ // 14 characters
            "class_finalize",
            "notinheritable",
            "notoverridable",
        ],
        null,           // 15 characters
        [
            "class_initialize",
        ]
    ];

    public static bool IsKeyword(string value)
    {
        return Lookup.Contains(value);
    }

    public static bool IsValidIdentifier(string value)
    {
        // identifiers must be 1 char or longer
        //
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (value.Length > 1023)
            return false;

        // identifiers cannot be a keyword unless surrounded by []'s
        //
        if (value[0] != '[' || value[value.Length - 1] != ']')
        {
            if (IsKeyword(value))
            {
                return false;
            }
        }
        else
        {
            value = value.Substring(1, value.Length - 2);
        }

        // just _ as an identifier is not valid. 
        if (value.Length == 1 && value[0] == '_')
            return false;

        return CSharpHelpers.IsValidTypeNameOrIdentifier(value, false);
    }

    public static string CreateValidIdentifier(string name)
    {
        if (IsKeyword(name))
        {
            return "_" + name;
        }
        return name;
    }

    public static string CreateEscapedIdentifier(string name)
    {
        if (IsKeyword(name))
        {
            return "[" + name + "]";
        }
        return name;
    }
}