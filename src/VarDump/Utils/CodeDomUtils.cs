using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Visitor;

namespace VarDump.Utils;

internal static class CodeDomUtils
{
    public static void WriteErrorDetectedExpression(ICodeGenerator codeGenerator, string errorMessage)
    {
        codeGenerator.GeneratePrimitive(null);

        codeGenerator.GenerateSeparator();

        codeGenerator.GenerateComment(errorMessage, true);
    }

    public static void WriteCircularReferenceDetectedExpression(ICodeGenerator codeGenerator)
    {
        codeGenerator.GeneratePrimitive(null);

        codeGenerator.GenerateSeparator();

        codeGenerator.GenerateComment("Circular reference detected", true);
    }

    public static void WriteTooManyItemsExpression(ICodeGenerator codeGenerator, int maxCollectionSize)
    {
        codeGenerator.GenerateComment($"Too many items (> {maxCollectionSize}). Consider increasing the {nameof(DumpOptions.MaxCollectionSize)} option.", noNewLine: true);
    }

    public static void WriteMaxDepthExpression(object @object, ICodeGenerator codeGenerator)
    {
        if (@object == null || @object.GetType().IsAnonymousType())
        {
            codeGenerator.GeneratePrimitive(null);
        }
        else
        {
            codeGenerator.GenerateDefaultValue(new CodeTypeReference(@object.GetType()));
        }

        codeGenerator.GenerateSeparator();

        codeGenerator.GenerateComment("Max depth", true);
    }
}