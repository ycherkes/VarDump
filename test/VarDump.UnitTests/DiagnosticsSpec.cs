﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class DiagnosticsSpec
{
    [Fact(Skip = "Skip")]
    public void DumpDiagnosticsCsharp()
    {
        var code =
            """
            namespace Debuggable
                    {
                        public class HelloWorld
                        {
                            public string Greet(string name)
                            {
                                var result = "Hello, " + name;
                                var anonymous = new { Name = "Boris" };
                                Console.WriteLine(result);
                                return result;
                            }
                        }
                    }
                    
            """;
        var failures = CreateAssembly(code, DefaultReferences);

        var dumper = new CSharpDumper(new DumpOptions
        {
            WritablePropertiesOnly = false
        });

        var result = dumper.Dump(failures);
    }

    [Fact(Skip = "Skip")]
    public void DumpDiagnosticsVb()
    {
        var code =
            """
            namespace Debuggable
                    {
                        public class HelloWorld
                        {
                            public string Greet(string name)
                            {
                                var result = "Hello, " + name;
                                var anonymous = new { Name = "Boris" };
                                Console.WriteLine(result);
                                return result;
                            }
                        }
                    }
                    
            """;
        var failures = CreateAssembly(code, DefaultReferences);

        var dumper = new VisualBasicDumper(new DumpOptions
        {
            WritablePropertiesOnly = false
        });

        var result = dumper.Dump(failures);
    }

    private static string runtimePath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\{0}.dll";

    private static readonly IEnumerable<MetadataReference> DefaultReferences =
        new[]
        {
            MetadataReference.CreateFromFile(string.Format(runtimePath, "mscorlib")),
            MetadataReference.CreateFromFile(string.Format(runtimePath, "System")),
            MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Core"))
        };

    private static IEnumerable<Diagnostic> CreateAssembly(string code, IEnumerable<MetadataReference> references)
    {
        var encoding = Encoding.UTF8;

        var assemblyName = Path.GetRandomFileName();
        var symbolsName = Path.ChangeExtension(assemblyName, "pdb");
        var sourceCodePath = "generated.cs";

        var buffer = encoding.GetBytes(code);
        var sourceText = SourceText.From(buffer, buffer.Length, encoding, canBeEmbedded: true);

        var syntaxTree = CSharpSyntaxTree.ParseText(
            sourceText,
            new CSharpParseOptions(),
            path: sourceCodePath);

        var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;
        var encoded = CSharpSyntaxTree.Create(syntaxRootNode, null, sourceCodePath, encoding);

        var optimizationLevel = OptimizationLevel.Debug;

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { encoded },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(optimizationLevel)
                .WithPlatform(Platform.AnyCpu)
        );

        using var assemblyStream = new MemoryStream();
        using var symbolsStream = new MemoryStream();
        var emitOptions = new EmitOptions(
            debugInformationFormat: DebugInformationFormat.PortablePdb,
            pdbFilePath: symbolsName);

        var embeddedTexts = new List<EmbeddedText>
        {
            EmbeddedText.FromSource(sourceCodePath, sourceText),
        };

        EmitResult result = compilation.Emit(
            peStream: assemblyStream,
            pdbStream: symbolsStream,
            embeddedTexts: embeddedTexts,
            options: emitOptions);

        if (!result.Success)
        {
            var errors = new List<string>();

            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            return failures;
        }

        return null;
        //Console.WriteLine(code);

        //assemblyStream.Seek(0, SeekOrigin.Begin);
        //symbolsStream?.Seek(0, SeekOrigin.Begin);

        //var assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream, symbolsStream);
        //return assembly;
    }

}