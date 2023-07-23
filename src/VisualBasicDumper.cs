using System;
using System.IO;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.CodeDom.VisualBasic;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor;

namespace VarDump
{
    public class VisualBasicDumper : IDumper
    {
        private readonly DumpOptions _options;

        public VisualBasicDumper()
        {
            _options = DumpOptions.Default;
        }

        public VisualBasicDumper(DumpOptions options)
        {
            _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
        }

        public string Dump(object obj)
        {
            var objectVisitor = new ObjectVisitor(_options);

            var expression = objectVisitor.Visit(obj);

            CodeObject codeObject = _options.GenerateVariableInitializer
                ? new CodeVariableDeclarationStatement(new CodeImplicitlyTypedTypeReference(),
                    obj != null ? ReflectionUtils.ComposeVisualBasicVariableName(obj.GetType()) : "nullValue")
                {
                    InitExpression = expression
                }
                : expression;

            ICodeGenerator generator = new VBCodeGenerator();
            using var sourceWriter = new StringWriter();

            generator.GenerateCode(codeObject, sourceWriter, new CodeGeneratorOptions());

            var vbCodeString = sourceWriter.ToString();

            return vbCodeString;
        }
    }
}
