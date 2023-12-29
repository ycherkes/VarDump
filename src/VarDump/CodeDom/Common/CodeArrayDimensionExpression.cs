namespace VarDump.CodeDom.Common;

internal class CodeArrayDimensionExpression : CodeExpression
{
    public CodeExpressionCollection Initializers { get; } = new();

    public CodeArrayDimensionExpression(CodeExpression[] initializers)
    {
        Initializers.AddRange(initializers);
    }
}