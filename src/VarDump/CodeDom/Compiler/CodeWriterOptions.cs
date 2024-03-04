// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Specialized;

namespace VarDump.CodeDom.Compiler;

public class CodeWriterOptions
{
    private readonly IDictionary _options = new ListDictionary();

    public string IndentString
    {
        get
        {
                var o = _options[nameof(IndentString)];
                return o != null ? (string)o : "    ";
            }
        set => _options[nameof(IndentString)] = value;
    }

    public bool UseFullTypeName
    {
        get
        {
                var o = _options[nameof(UseFullTypeName)];
                return o != null && (bool)o;
            }
        set => _options[nameof(UseFullTypeName)] = value;
    }
}