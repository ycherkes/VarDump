// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace VarDump.CodeDom.Common
{
    internal class CodeStatementExpression : CodeExpression
    {
        public CodeStatementExpression() { }

        public CodeStatementExpression(CodeStatement statement)
        {
            Statement = statement;
        }

        public CodeStatement Statement { get; set; }
    }
}
