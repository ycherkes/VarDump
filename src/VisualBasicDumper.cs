using System;
using System.IO;
using VarDumpExtended.CodeDom.Common;
using VarDumpExtended.CodeDom.Compiler;
using VarDumpExtended.CodeDom.VisualBasic;
using VarDumpExtended.Extensions;
using VarDumpExtended.Utils;
using VarDumpExtended.Visitor;

namespace VarDumpExtended;

public class VisualBasicDumper : IDumper
{
    private readonly DumpOptions _options;

    public VisualBasicDumper()
    {
        _options = DumpOptions.Default;
    }

    public VisualBasicDumper(DumpOptions options)
    {
        _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
    }

    public string Dump(object obj)
    {
        using var sourceWriter = new StringWriter();

        DumpImpl(obj, sourceWriter);

        var vbCodeString = sourceWriter.ToString();

        return vbCodeString;
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
                obj != null ? ReflectionUtils.ComposeVisualBasicVariableName(obj.GetType()) : "nullValue")
            {
                InitExpression = expression
            }
            : expression;

        ICodeGenerator generator = new VBCodeGenerator();

        generator.GenerateCode(codeObject, textWriter, new CodeGeneratorOptions());
    }
}