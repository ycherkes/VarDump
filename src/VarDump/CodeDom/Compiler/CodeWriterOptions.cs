// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace VarDump.CodeDom.Compiler;

public sealed class CodeWriterOptions
{
    public string IndentString { get; set; }

    public bool UseFullTypeName { get; set; }
}