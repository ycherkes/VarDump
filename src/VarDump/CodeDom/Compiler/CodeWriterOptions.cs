// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using VarDump.Visitor;

namespace VarDump.CodeDom.Compiler;

public sealed class CodeWriterOptions
{
    public CSharpCollectionLiteralStyle CSharpCollectionLiteralStyle { get; set; }
    public CSharpStringLiteralStyle CSharpStringLiteralStyle { get; set; }
    public string IndentString { get; set; }
    public NewLineStyle NewLineStyle { get; set; }
    public TypeNamingPolicy TypeNamePolicy { get; set; }
}
