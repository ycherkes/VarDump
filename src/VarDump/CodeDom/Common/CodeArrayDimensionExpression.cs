using System.Collections.Generic;

namespace VarDump.CodeDom.Common;

internal class CodeArrayDimensionExpression : CodeExpression
{
    public CodeExpressionContainer Initializers { get; }

    public CodeArrayDimensionExpression(IEnumerable<CodeExpression> initializers)
    {
        Initializers = new CodeExpressionContainer(initializers);
    }
}