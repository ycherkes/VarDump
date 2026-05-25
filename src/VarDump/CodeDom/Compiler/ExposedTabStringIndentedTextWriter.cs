// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.CodeDom.Compiler;
using System.IO;

namespace VarDump.CodeDom.Compiler;

internal sealed class ExposedTabStringIndentedTextWriter(TextWriter writer, string tabString)
    : IndentedTextWriter(writer, tabString)
{
    private int _currentColumn;

    internal int CurrentColumn => _currentColumn;

    internal int GetCurrentColumnFromBuffer()
    {
        if (InnerWriter is not StringWriter sw)
        {
            return _currentColumn;
        }

        var text = sw.ToString();
        var lastLf = text.LastIndexOf('\n');
        var lastCr = text.LastIndexOf('\r');
        var lineStart = Math.Max(lastLf, lastCr) + 1;

        return text.Length - lineStart;
    }

    public override void Write(char value)
    {
        base.Write(value);

        if (value == '\n' || value == '\r')
        {
            _currentColumn = 0;
            return;
        }

        _currentColumn++;
    }

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
            _currentColumn += TabString.Length;
        }
    }

    internal string TabString { get; } = tabString ?? DefaultTabString; // IndentedTextWriter doesn't expose this publicly
}
