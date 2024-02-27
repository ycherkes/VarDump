using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;
using VarDump.Visitor;

namespace VarDump.Extensions;

public static class CodeGeneratorExtensions
{
    public static void GenerateErrorDetected(this IDotnetCodeGenerator codeGenerator, string errorMessage)
    {
        codeGenerator.GeneratePrimitive(null);
        codeGenerator.GenerateSeparator();
        codeGenerator.GenerateComment(errorMessage, true);
    }

    public static void GenerateCircularReferenceDetected(this IDotnetCodeGenerator codeGenerator)
    {
        codeGenerator.GeneratePrimitive(null);
        codeGenerator.GenerateSeparator();
        codeGenerator.GenerateComment("Circular reference detected", true);
    }

    public static void GenerateTooManyItems(this IDotnetCodeGenerator codeGenerator, int maxCollectionSize)
    {
        codeGenerator.GenerateComment($"Too many items (> {maxCollectionSize}). Consider increasing the {nameof(DumpOptions.MaxCollectionSize)} option.", noNewLine: true);
    }

    public static void GenerateMaxDepthExpression(this IDotnetCodeGenerator codeGenerator, object @object)
    {
        if (@object == null || @object.GetType().IsAnonymousType())
        {
            codeGenerator.GeneratePrimitive(null);
        }
        else
        {
            codeGenerator.GenerateDefaultValue(new CodeDotnetTypeReference(@object.GetType()));
        }

        codeGenerator.GenerateSeparator();
        codeGenerator.GenerateComment("Max depth", true);
    }
}