// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace VarDump.CodeDom.Common;

internal class CodeCastExpression : CodeExpression
{
    private CodeTypeReference _targetType;

    public CodeCastExpression() { }

    public CodeCastExpression(CodeTypeReference targetType, CodeExpression expression)
    {
        TargetType = targetType;
        Expression = expression;
    }

    public CodeCastExpression(CodeTypeReference targetType, CodeExpression expression, bool useSimpleParentheses)
    {
        TargetType = targetType;
        Expression = expression;
        SimpleParentheses = useSimpleParentheses;
    }

    public CodeCastExpression(string targetType, CodeExpression expression)
    {
        TargetType = new CodeTypeReference(targetType);
        Expression = expression;
    }

    public CodeCastExpression(Type targetType, CodeExpression expression, bool useSimpleParentheses)
    {
        TargetType = new CodeTypeReference(targetType);
        Expression = expression;
        SimpleParentheses = useSimpleParentheses;
    }

    public CodeTypeReference TargetType
    {
        get => _targetType ??= new CodeTypeReference("");
        set => _targetType = value;
    }

    public CodeExpression Expression { get; set; }
    public bool SimpleParentheses { get; set; }
}