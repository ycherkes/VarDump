// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace VarDump.CodeDom.Common;

internal class CodeNamedArgumentExpression : CodeExpression
{
    public CodeNamedArgumentExpression(string name, CodeExpression value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public CodeExpression Value { get; }
}