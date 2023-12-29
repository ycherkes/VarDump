// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace VarDump.CodeDom.Common;

internal class CodeAssignExpression : CodeExpression
{
    public CodeAssignExpression() { }

    public CodeAssignExpression(CodeExpression left, CodeExpression right)
    {
        Left = left;
        Right = right;
    }

    public CodeExpression Left { get; set; }

    public CodeExpression Right { get; set; }
}