using System;
using System.IO;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.CodeDom.VisualBasic;
using VarDump.Utils;
using VarDump.Visitor;
using VarDump.Visitor.Format;

namespace VarDump;

public sealed class VisualBasicDumper : IDumper
{
    private readonly DumpOptions _options;

    public VisualBasicDumper()
    {
        _options = new DumpOptions();
    }

    public VisualBasicDumper(DumpOptions options)
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
        if (!IntegralNumericFormat.TryParse(options.IntegralNumericFormat, out _))
        {
            throw new FormatException($"Bad format specifier. {options.IntegralNumericFormat}");
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
            NamingPolicy = _options.NamingPolicy,
            IndentString = _options.IndentString
        };

        ICodeWriter codeWriter = new VisualBasicCodeWriter(textWriter, codeWriterOptions);

        var objectVisitor = new ObjectVisitor(_options, codeWriter);

        if (_options.GenerateVariableInitializer)
        {
            codeWriter.WriteVariableDeclarationStatement(new CodeVarTypeInfo(),
                obj != null ? ReflectionUtils.ComposeVisualBasicVariableName(obj.GetType()) : "nullValue", () => objectVisitor.Visit(obj));
        }
        else
        {
            objectVisitor.Visit(obj);
        }
    }
}