using VarDump.CodeDom.Common;

namespace VarDump.Utils
{
    internal static class CodeDomUtils
    {
        public static CodeExpression GetErrorDetectedExpression(string errorMessage)
        {
            return new CodeSeparatedExpressionCollection(new CodeExpression[]
            {
                new CodePrimitiveExpression(null),
                new CodeStatementExpression(new CodeCommentStatement(new CodeComment(errorMessage) { NoNewLine = true }))
            }, ", ");
        }

        public static CodeExpression GetCircularReferenceDetectedExpression()
        {
            return new CodeSeparatedExpressionCollection(new CodeExpression[]
            {
                new CodePrimitiveExpression(null),
                new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { NoNewLine = true }))
            }, ", ");
        }

        public static CodeExpression GetMaxDepthExpression(object @object, CodeTypeReferenceOptions typeReferenceOptions)
        {
            return new CodeSeparatedExpressionCollection(new CodeExpression[]
            {
                @object == null
                    ? new CodePrimitiveExpression(null)
                    : new CodeDefaultValueExpression(new CodeTypeReference(@object.GetType(), typeReferenceOptions)),
                new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Max depth") { NoNewLine = true }))
            }, ", ");
        }
    }
}