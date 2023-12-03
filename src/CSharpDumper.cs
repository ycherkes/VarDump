using System;
using System.IO;
using VarDumpExtended.CodeDom.Common;
using VarDumpExtended.CodeDom.Compiler;
using VarDumpExtended.CodeDom.CSharp;
using VarDumpExtended.Extensions;
using VarDumpExtended.Utils;
using VarDumpExtended.Visitor;

namespace VarDumpExtended;

public class CSharpDumper : IDumper
{
    private readonly DumpOptions _options;

    public CSharpDumper()
    {
        _options = DumpOptions.Default;
    }

    public CSharpDumper(DumpOptions options)
    {
        _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
    }

    public string Dump(object obj)
    {
        using var sourceWriter = new StringWriter();

        DumpImpl(obj, sourceWriter);

        var csCodeString = sourceWriter.ToString();

        return csCodeString;
    }

    public void Dump(object obj, TextWriter textWriter)
    {
        DumpImpl(obj, textWriter);
    }

    private void DumpImpl(object obj, TextWriter textWriter)
    {
        var objectVisitor = new ObjectVisitor(_options);

        var expression = objectVisitor.Visit(obj);

        CodeObject codeObject = _options.GenerateVariableInitializer
            ? new CodeVariableDeclarationStatement(new CodeImplicitlyTypedTypeReference(),
                obj != null ? ReflectionUtils.ComposeCsharpVariableName(obj.GetType()) : "nullValue")
            {
                InitExpression = expression
            }
            : expression;

        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            BracingStyle = "C"
        };

        ICodeGenerator generator = new CSharpCodeGenerator();

        generator.GenerateCode(codeObject, textWriter, codeGeneratorOptions);
    }
}