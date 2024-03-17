using VarDump.CodeDom.Compiler;
using VarDump.Visitor;
using VarDump.Utils;

namespace VarDump.Extensions;

public static class CodeWriterExtensions
{
    public static void WriteErrorDetected(this ICodeWriter codeWriter, string errorMessage)
    {
        codeWriter.WritePrimitive(null);
        codeWriter.WriteSeparator();
        codeWriter.WriteComment(errorMessage, true);
    }

    public static void WriteCircularReferenceDetected(this ICodeWriter codeWriter)
    {
        codeWriter.WritePrimitive(null);
        codeWriter.WriteSeparator();
        codeWriter.WriteComment("Circular reference detected", true);
    }

    public static void WriteTooManyItems(this ICodeWriter codeWriter, int maxCollectionSize)
    {
        codeWriter.WriteComment($"Too many items (> {maxCollectionSize}). Consider increasing the {nameof(DumpOptions.MaxCollectionSize)} option.", noNewLine: true);
    }

    public static void WriteMaxDepthExpression(this ICodeWriter codeWriter, object @object)
    {
        if (@object == null || @object.GetType().IsAnonymousType())
        {
            codeWriter.WritePrimitive(null);
        }
        else
        {
            codeWriter.WriteDefaultValue(@object.GetType());
        }

        codeWriter.WriteSeparator();
        codeWriter.WriteComment("Max depth", true);
    }
}