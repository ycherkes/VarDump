using System.Collections.Generic;

namespace VarDump.CodeDom.Common;

internal class CodeArrayDimensionExpression : CodeExpression
{
    public CodeExpressionCollection Initializers { get; }

    public CodeArrayDimensionExpression(IEnumerable<CodeExpression> initializers)
    {
        Initializers = new CodeExpressionCollection(initializers);
    }
}