using System;
using System.IO;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Extensions
{
    internal static class CodeGeneratorExtensions
    {
        public static void GenerateCode(this ICodeGenerator codeGenerator, CodeObject co, TextWriter w,
            CodeGeneratorOptions o)
        {
            switch (co)
            {
                case CodeExpression ce:
                    codeGenerator.GenerateCodeFromExpression(ce, w, o);
                    break;
                case CodeStatement cs:
                    codeGenerator.GenerateCodeFromStatement(cs, w, o);
                    break;
                default:
                    throw new InvalidOperationException("CodeExpression and CodeStatement supported only.");
            }
        }
    }
}
