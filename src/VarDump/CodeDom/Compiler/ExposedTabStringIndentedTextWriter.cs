// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;

namespace VarDump.CodeDom.Compiler;

internal sealed class ExposedTabStringIndentedTextWriter(TextWriter writer, string tabString) 
{
    public string TabString { get; } = tabString;
    private readonly TextWriter _innerWriter = writer ?? throw new ArgumentNullException(nameof(writer));
    private int _indentLevel;
    private bool _tabsPending = true;

    public const string DefaultTabString = "    ";

    public ExposedTabStringIndentedTextWriter(TextWriter writer) : this(writer, DefaultTabString) { }

    public string NewLine
    {
        get => _innerWriter.NewLine;
        set => _innerWriter.NewLine = value;
    }

    public int Indent
    {
        get => _indentLevel;
        set => _indentLevel = Math.Max(value, 0);
    }

    // This property is not used by the writer itself, but can be used by external code to track the current alignment after indents and tabs.
    public int ExtraAlignment { get; private set; }

    public void Close() => _innerWriter.Close();

    public void Flush() => _innerWriter.Flush();

    private void OutputTabs()
    {
        if (!_tabsPending) return;

        for (var i = 0; i < _indentLevel; i++)
        {
            _innerWriter.Write(TabString);
        }
        ExtraAlignment = 0;
        _tabsPending = false;
    }

    public void Write(string s)
    {
        OutputTabs();

        if (s == null)
            return;

        foreach (var c in s)
        {
            WriteWithoutIndents(c);
        }
    }

    public void Write(int value)
    {
        var stringValue = value.ToString(null, _innerWriter.FormatProvider);
        Write(stringValue);
    }

    public void Write(object value)
    {
        var stringValue = value is IFormattable formattable
            ? formattable.ToString(null, _innerWriter.FormatProvider)
            : value?.ToString();

        Write(stringValue);
    }

    public void WriteLine(string s)
    {
        OutputTabs();
        _innerWriter.WriteLine(s);
        _tabsPending = true;
    }

    public void WriteLine()
    {
        OutputTabs();
        _innerWriter.WriteLine();
        _tabsPending = true;
    }

    public void WriteLine(char value)
    {
        OutputTabs();
        _innerWriter.WriteLine(value);
        _tabsPending = true;
    }

    internal void OutputIndents()
    {
        OutputIndents(Indent);
    }

    internal void OutputIndents(int indent)
    {
        for (int i = 0; i < indent; i++)
        {
            _innerWriter.Write(TabString);
        }
        ExtraAlignment = 0;
    }

    public void Write(char value)
    {
        OutputTabs();
        WriteWithoutIndents(value);
    }

    private void WriteWithoutIndents(char value)
    {
        _innerWriter.Write(value);

        if (value is '\n' or '\r')
        {
            ExtraAlignment = 0;
            return;
        }

        ExtraAlignment++;
    }
}