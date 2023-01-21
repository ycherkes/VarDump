using System;
using System.IO;
using System.Text;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.CodeDom.CSharp;
using VarDump.Utils;
using VarDump.Visitor;

namespace VarDump
{
    public class CSharpDumper : IDumper
    {
        private readonly ObjectVisitor _objectVisitor;
        public CSharpDumper()
        {
            _objectVisitor = new ObjectVisitor(DumpOptions.Default);
        }

        public CSharpDumper(DumpOptions options)
        {
            _objectVisitor = new ObjectVisitor(options?.Clone() ?? throw new ArgumentNullException(nameof(options)));
        }

        public string Dump(object obj)
        {
            var expression = _objectVisitor.Visit(obj);
            var variableDeclaration = new CodeVariableDeclarationStatement(new CodeImplicitlyTypedTypeReference(),
                obj != null ? ReflectionUtils.ComposeCsharpVariableName(obj.GetType()) : "nullValue")
            {
                InitExpression = expression
            };

            CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions
            {
                BracingStyle = "C"
            };

            ICodeGenerator generator = new CSharpCodeGenerator();

            var stringBuilder = new StringBuilder();

            using (var sourceWriter = new StringWriter(stringBuilder))
            {
                generator.GenerateCodeFromStatement(variableDeclaration, sourceWriter, codeGeneratorOptions);
            }

            var csCodeString = stringBuilder.ToString();

            return csCodeString;
        }
    }
}
