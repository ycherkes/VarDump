using System.IO;
using System.Reflection;
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
        private static DumpOptions DefaultOptions => new()
        {
            IgnoreDefaultValues = true,
            IgnoreNullValues = true,
            MaxDepth = 25,
            ExcludeTypes = new[] { "Avro.Schema" },
            UseTypeFullName = false,
            DateTimeInstantiation = DateTimeInstantiation.New,
            DateKind = DateKind.ConvertToUtc,
            UseNamedArgumentsForReferenceRecordTypes = false,
            GetPropertiesBindingFlags = BindingFlags.Instance | BindingFlags.Public,
            WritablePropertiesOnly = true
        };

        public string Dump(object obj, DumpOptions options = null)
        {
            var objVisitor = new ObjectVisitor(options ?? DefaultOptions);
            var expression = objVisitor.Visit(obj);
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
            var result = stringBuilder.ToString();
            return result;
        }
    }
}
