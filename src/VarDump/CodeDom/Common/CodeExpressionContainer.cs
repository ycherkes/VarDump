// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VarDump.CodeDom.Common;

internal class CodeExpressionContainer : IEnumerable<CodeExpression>
{
    private IEnumerable<CodeExpression> _expressions;

    public CodeExpressionContainer()
    {
        _expressions = Enumerable.Empty<CodeExpression>();
    }

    public CodeExpressionContainer(CodeExpressionContainer value)
    {
        _expressions = value._expressions;
    }

    public CodeExpressionContainer(IEnumerable<CodeExpression> value)
    {
        _expressions = value;
    }

    public void AddRange(IEnumerable<CodeExpression> value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        _expressions = _expressions.Concat(value);
    }

    public IEnumerator<CodeExpression> GetEnumerator()
    {
        return _expressions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}