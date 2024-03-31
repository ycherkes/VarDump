// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CodeDom.Compiler;
using System.IO;

namespace VarDump.CodeDom.Compiler;

internal sealed class ExposedTabStringIndentedTextWriter(TextWriter writer, string tabString)
    : IndentedTextWriter(writer, tabString)
{
    internal void OutputIndents()
    {
        OutputIndents(Indent);
    }

    internal void OutputIndents(int indent)
    {
        TextWriter inner = InnerWriter;
        for (int i = 0; i < indent; i++)
        {
            inner.Write(TabString);
        }
    }

    internal string TabString { get; } = tabString ?? DefaultTabString; // IndentedTextWriter doesn't expose this publicly
}