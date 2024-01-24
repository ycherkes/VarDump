using System;
using System.IO;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.CodeDom.VisualBasic;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;

namespace VarDump;

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
        if(textWriter == null)
        {
            throw new ArgumentNullException(nameof(textWriter));
        }
        
        DumpImpl(obj, textWriter);
    }

    private void DumpImpl(object obj, TextWriter textWriter)
    {
        var objectVisitor = new ObjectVisitor(_options);

        var expression = objectVisitor.Visit(new ValueDescriptor { Value = obj, Type = obj?.GetType() });

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