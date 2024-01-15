// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace VarDump.CodeDom.Common;

internal class CodeObjectCreateExpression : CodeExpression
{
    private CodeTypeReference _createType;

    public CodeObjectCreateExpression() { }

    public CodeObjectCreateExpression(CodeTypeReference createType, params CodeExpression[] parameters)
    :this(createType, (IEnumerable<CodeExpression>)parameters)
    {
    }

    public CodeObjectCreateExpression(CodeTypeReference createType, IEnumerable<CodeExpression> parameters)
    {
        CreateType = createType;
        if (parameters != null)
        {
            Parameters.AddRange(parameters);
        }
    }

    public CodeObjectCreateExpression(string createType, params CodeExpression[] parameters)
    {
        CreateType = new CodeTypeReference(createType);
        Parameters.AddRange(parameters);
    }

    public CodeObjectCreateExpression(Type createType, params CodeExpression[] parameters)
    {
        CreateType = new CodeTypeReference(createType);
        Parameters.AddRange(parameters);
    }

    public CodeTypeReference CreateType
    {
        get => _createType ??= new CodeTypeReference("");
        set => _createType = value;
    }

    public CodeExpressionCollection Parameters { get; } = new CodeExpressionCollection();
}