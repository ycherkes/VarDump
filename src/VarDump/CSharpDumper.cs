using System;
using System.IO;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.CodeDom.CSharp;
using VarDump.Utils;
using VarDump.Visitor;
using VarDump.Visitor.Format;

namespace VarDump;

public sealed class CSharpDumper : IDumper
{
    private readonly DumpOptions _options;

    public CSharpDumper()
    {
        _options = new DumpOptions();
    }

    public CSharpDumper(DumpOptions options)
    {
        ValidateOptions(options);
        _options = options.Clone();
    }

    private void ValidateOptions(DumpOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        if (options.Formatting == null)
        {
            throw new ArgumentNullException(nameof(options.Formatting));
        }
        if (!IntegralNumericFormat.TryParse(options.Formatting.IntegralNumericFormat, out _))
        {
            throw new FormatException($"Bad format specifier. {options.Formatting.IntegralNumericFormat}");
        }
    }

    public string Dump(object obj)
    {
        using var writer = new StringWriter();

        DumpImpl(obj, writer);

        var codeString = writer.ToString();

        return codeString;
    }

    public void Dump(object obj, TextWriter textWriter)
    {
        if (textWriter == null)
        {
            throw new ArgumentNullException(nameof(textWriter));
        }

        DumpImpl(obj, textWriter);
    }

    private void DumpImpl(object obj, TextWriter textWriter)
    {
        var codeWriterOptions = new CodeWriterOptions
        {
            UseFullTypeName = _options.UseTypeFullName,
            IndentString = _options.IndentString
        };

        ICodeWriter codeWriter = new CSharpCodeWriter(textWriter, codeWriterOptions);

        var objectVisitor = new ObjectVisitor(_options, codeWriter);

        if (_options.GenerateVariableInitializer)
        {
            codeWriter.WriteVariableDeclarationStatement(new CodeVarTypeInfo(),
                obj != null ? ReflectionUtils.ComposeCSharpVariableName(obj.GetType()) : "nullValue", () => objectVisitor.Visit(obj));
        }
        else
        {
            objectVisitor.Visit(obj);
        }
    }
}