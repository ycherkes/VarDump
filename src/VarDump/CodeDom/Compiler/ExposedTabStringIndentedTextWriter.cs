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
    
    public void Close() => _innerWriter.Close();

    public void Flush() => _innerWriter.Flush();

    private void OutputTabs()
    {
        if (!_tabsPending) return;

        for (var i = 0; i < _indentLevel; i++)
        {
            _innerWriter.Write(TabString);
        }
        _tabsPending = false;
    }

    public void Write(string s)
    {
        OutputTabs();
        _innerWriter.Write(s);
    }

    public void Write(int value)
    {
        OutputTabs();
        _innerWriter.Write(value);
    }

    public void Write(object value)
    {
        OutputTabs();
        _innerWriter.Write(value);
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
            _currentColumn += TabString.Length;
        }
    }

    private int _currentColumn;

    internal int GetCurrentColumnFromBuffer()
    {
        if (_innerWriter is not StringWriter sw)
        {
            return _currentColumn;
        }

        var text = sw.ToString();
        var lastLf = text.LastIndexOf('\n');
        var lastCr = text.LastIndexOf('\r');
        var lineStart = Math.Max(lastLf, lastCr) + 1;

        return text.Length - lineStart;
    }

    public void Write(char value)
    {
        OutputTabs();
        _innerWriter.Write(value);

        if (value is '\n' or '\r')
        {
            _currentColumn = 0;
            return;
        }

        _currentColumn++;
    }
}