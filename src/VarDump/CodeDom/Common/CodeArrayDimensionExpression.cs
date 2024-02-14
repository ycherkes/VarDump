namespace VarDump.CodeDom.Common;

internal class CodeArrayDimensionExpression : CodeExpression
{
    public CodeExpressionCollection Initializers { get; }

    public CodeArrayDimensionExpression(CodeExpression[] initializers)
    {
        Initializers = new CodeExpressionCollection(initializers);
    }
}