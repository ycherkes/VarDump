using VarDump.CodeDom.Common;
using VarDump.Visitor;

namespace VarDump.Utils;

internal static class CodeDomUtils
{
    public static CodeExpression GetErrorDetectedExpression(string errorMessage)
    {
        return new CodeSeparatedExpressionCollection(
        [
            new CodePrimitiveExpression(null),
            new CodeStatementExpression(new CodeCommentStatement(new CodeComment(errorMessage) { NoNewLine = true }))
        ], ", ");
    }

    public static CodeExpression GetCircularReferenceDetectedExpression()
    {
        return new CodeSeparatedExpressionCollection(
        [
            new CodePrimitiveExpression(null),
            new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { NoNewLine = true }))
        ], ", ");
    }

    public static CodeExpression GetTooManyItemsExpression(int maxCollectionSize)
    {
        return new CodeStatementExpression(new CodeCommentStatement(new CodeComment($"Too many items (> {maxCollectionSize}). Consider increasing the {nameof(DumpOptions.MaxCollectionSize)} option.") { NoNewLine = true }));
    }

    public static CodeExpression GetMaxDepthExpression(object @object, CodeTypeReferenceOptions typeReferenceOptions)
    {
        return new CodeSeparatedExpressionCollection(
        [
            @object == null || @object.GetType().IsAnonymousType()
                ? new CodePrimitiveExpression(null)
                : new CodeDefaultValueExpression(new CodeTypeReference(@object.GetType(), typeReferenceOptions)),
            new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Max depth") { NoNewLine = true }))
        ], ", ");
    }
}