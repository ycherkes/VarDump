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
        if (!IntegralNumericFormat.TryParse(options.IntegralNumericFormat, out _))
        {
            throw new FormatException($"Bad format specifier. {options.IntegralNumericFormat}");
        }

        if (!Enum.IsDefined(typeof(StringLiteralStyle), options.StringLiteralStyle))
        {
            throw new ArgumentOutOfRangeException(nameof(options.StringLiteralStyle));
        }

        if (!Enum.IsDefined(typeof(CollectionLiteralStyle), options.CollectionLiteralStyle))
        {
            throw new ArgumentOutOfRangeException(nameof(options.CollectionLiteralStyle));
        }

        if (!Enum.IsDefined(typeof(NewLineStyle), options.NewLineStyle))
        {
            throw new ArgumentOutOfRangeException(nameof(options.NewLineStyle));
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
            CSharpCollectionLiteralStyle = _options.CollectionLiteralStyle,
            CSharpStringLiteralStyle = _options.StringLiteralStyle,
            TypeNamePolicy = _options.TypeNamePolicy,
            IndentString = _options.IndentString,
            NewLineStyle = _options.NewLineStyle
        };

        ICodeWriter codeWriter = new CSharpCodeWriter(textWriter, codeWriterOptions);

        var objectVisitor = new ObjectVisitor(_options, codeWriter);

        if (_options.GenerateVariableInitializer)
        {
            CodeTypeInfo declarationType = new CodeVarTypeInfo();

            if (_options.CollectionLiteralStyle == CollectionLiteralStyle.Expression
                && obj is System.Collections.IEnumerable)
            {
                declarationType = obj.GetType();
            }

            codeWriter.WriteVariableDeclarationStatement(declarationType,
                obj != null ? ReflectionUtils.ComposeCSharpVariableName(obj.GetType()) : "nullValue", () => objectVisitor.Visit(obj));
        }
        else
        {
            objectVisitor.Visit(obj);
        }
    }
}
