using System;
using System.IO;
using System.Text;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.CodeDom.VisualBasic;
using VarDump.Utils;
using VarDump.Visitor;

namespace VarDump
{
    public class VisualBasicDumper : IDumper
    {
        private readonly ObjectVisitor _objectVisitor;

        public VisualBasicDumper()
        {
            _objectVisitor = new ObjectVisitor(DumpOptions.Default);
        }

        public VisualBasicDumper(DumpOptions options)
        {
            _objectVisitor = new ObjectVisitor(options?.Clone() ?? throw new ArgumentNullException(nameof(options)));
        }

        public string Dump(object obj)
        {
            var expression = _objectVisitor.Visit(obj);
            var variableDeclaration = new CodeVariableDeclarationStatement(new CodeImplicitlyTypedTypeReference(),
                obj != null ? ReflectionUtils.ComposeVisualBasicVariableName(obj.GetType()) : "nullValue")
            {
                InitExpression = expression
            };

            ICodeGenerator generator = new VBCodeGenerator();

            var stringBuilder = new StringBuilder();

            using (var sourceWriter = new StringWriter(stringBuilder))
            {
                generator.GenerateCodeFromStatement(variableDeclaration, sourceWriter, new CodeGeneratorOptions());
            }

            var vbCodeString = stringBuilder.ToString();

            return vbCodeString;
        }
    }
}
