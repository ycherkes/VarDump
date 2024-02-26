using System;
using System.IO;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.CodeDom.CSharp;
using VarDump.Utils;
using VarDump.Visitor;

namespace VarDump;

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
        if (textWriter == null)
        {
            throw new ArgumentNullException(nameof(textWriter));
        }

        DumpImpl(obj, textWriter);
    }

    private void DumpImpl(object obj, TextWriter textWriter)
    {
        ICodeGenerator codeGenerator = new CSharpCodeGenerator(textWriter, new CodeGeneratorOptions{ UseFullTypeName = _options.UseTypeFullName });

        var objectVisitor = new ObjectVisitor(_options, codeGenerator);

        if (_options.GenerateVariableInitializer)
        {
            codeGenerator.GenerateVariableDeclarationStatement(new CodeImplicitlyTypedTypeReference(),
                obj != null ? ReflectionUtils.ComposeCsharpVariableName(obj.GetType()) : "nullValue", () => objectVisitor.Visit(obj));
        }
        else
        {
            objectVisitor.Visit(obj);
        }
    }
}