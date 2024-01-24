// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace VarDump.CodeDom.Common;

internal class CodeFlagsBinaryOperatorExpression : CodeExpression
{
    public CodeFlagsBinaryOperatorExpression() { }

    public CodeFlagsBinaryOperatorExpression(CodeBinaryOperatorType op, IEnumerable<CodeExpression> expressions)
    {
        Operator = op;
        Expressions = new CodeExpressionContainer(expressions);
    }

    public CodeExpressionContainer Expressions { get; set; }

    public CodeBinaryOperatorType Operator { get; set; }
}