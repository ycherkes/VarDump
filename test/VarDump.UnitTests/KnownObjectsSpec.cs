using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.UnitTests.TestModel;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;
using Xunit;

namespace VarDump.UnitTests;

public class KnownObjectsSpec
{
    [Fact]
    public void UriVisitorWithObjectDescriptionWriterCSharp()
    {
        // This example shows another way how to implement the KnownObjectVisitor
        // with using the ObjectDescriptionWriter. The code looks simpler but is less performant than
        // classical approach - see VarDump.Visitor.KnownObjects.UriVisitor

        var uri = new Uri("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName");

        var dumpOptions = new DumpOptions
        {
            ConfigureKnownObjects = (knownObjects, nextDepthVisitor, options, codeWriter) =>
            {
                knownObjects[nameof(Uri)] = new CustomUriVisitor(nextDepthVisitor, options, codeWriter);
            },
            UseNamedArgumentsInConstructors = true
        };

        var dumper = new CSharpDumper(dumpOptions);

        var result = dumper.Dump(uri);

        Assert.Equal("""
                     var uri = new Uri(uriString: "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName");
                     
                     """, result);
    }

    [Fact]
    public void ApplyDifferentSettingsToDifferentKnownObjectVisitorsCSharp()
    {
        var anonymous = new
        {
            Version = new Version("1.2.3.4"),
            Regex = new Regex("\\D{4}", RegexOptions.Compiled),
            KeyValuePair = new KeyValuePair<string, string>("1", "2"),
            Tuple = new Tuple<string, string>("3", "4"),
            ValueTuple = new ValueTuple<string, string>("5", "6")
        };

        var dumpOptions = new DumpOptions
        {
            ConfigureKnownObjects = (knownObjects, _, _, _) =>
            {
                knownObjects["KeyValuePair"].ConfigureOptions(DisableNamedArguments);
                knownObjects["Tuple"].ConfigureOptions(DisableNamedArguments);
                knownObjects["ValueTuple"].ConfigureOptions(DisableNamedArguments);
            },
            UseNamedArgumentsInConstructors = true
        };

        var dumper = new CSharpDumper(dumpOptions);

        var result = dumper.Dump(anonymous);

        Assert.Equal("""
                     var anonymousType = new 
                     {
                         Version = new Version(version: "1.2.3.4"),
                         Regex = new Regex(pattern: "\\D{4}", options: RegexOptions.Compiled),
                         KeyValuePair = new KeyValuePair<string, string>("1", "2"),
                         Tuple = new Tuple<string, string>("3", "4"),
                         ValueTuple = ("5", "6")
                     };
                     
                     """, result);
        return;

        static void DisableNamedArguments(DumpOptions opts)
        {
            opts.UseNamedArgumentsInConstructors = false;
        }
    }

    [Fact]
    public void DumpServiceDescriptorCSharp()
    {
        var serviceCollection = new ServiceCollection
        {
            ServiceDescriptor.Transient<IPerson>(_ => new TestModel.Person()), // It's not possible to reconstruct the expression by existing Func
            ServiceDescriptor.Singleton<IPerson, TestModel.Person>(),
            ServiceDescriptor.Scoped<IPerson, TestModel.Person>()
        };

        var dumpOptions = new DumpOptions
        {
            ConfigureKnownObjects = (knownObjects, nextLevelVisitor, _, codeWriter) =>
            {
                knownObjects.Add(new ServiceDescriptorVisitor(nextLevelVisitor, codeWriter));
            }
        };

        var dumper = new CSharpDumper(dumpOptions);

        var result = dumper.Dump(serviceCollection);

        Assert.Equal("""
                var serviceCollectionOfServiceDescriptor = new ServiceCollection
                {
                    ServiceDescriptor.Transient<IPerson>(serviceProvider => default(IPerson)),
                    ServiceDescriptor.Singleton<IPerson, Person>(),
                    ServiceDescriptor.Scoped<IPerson, Person>()
                };

                """, result);
    }

    [Fact]
    public void DumpServiceDescriptorVb()
    {
        var personServiceDescriptor = ServiceDescriptor.Transient<IPerson, TestModel.Person>();

        var dumpOptions = new DumpOptions
        {
            ConfigureKnownObjects = (knownObjects, nextLevelVisitor, _, codeWriter) =>
            {
                knownObjects.Add(new ServiceDescriptorVisitor(nextLevelVisitor, codeWriter));
            }
        };

        var dumper = new VisualBasicDumper(dumpOptions);

        var result = dumper.Dump(personServiceDescriptor);

        Assert.Equal(
            """
            Dim serviceDescriptorValue = ServiceDescriptor.Transient(Of IPerson, Person)()

            """, result);
    }

    [Fact]
    public void DumpFormattableStringCSharp()
    {
        const string name = "World";
        FormattableString str = $"Hello, {name}";

        var dumpOptions = new DumpOptions
        {
            ConfigureKnownObjects = (knownObjects, nextLevelVisitor, _, codeWriter) =>
            {
                knownObjects.Add(new FormattableStringVisitor(nextLevelVisitor, codeWriter));
            }
        };

        var dumper = new CSharpDumper(dumpOptions);

        var result = dumper.Dump(str);

        Assert.Equal(
            """
            var concreteFormattableString = FormattableStringFactory.Create("Hello, {0}", "World");

            """, result);
    }

    [Fact]
    public void DumpFormattableStringVb()
    {
        const string name = "World";
        FormattableString str = $"Hello, {name}";

        var dumpOptions = new DumpOptions
        {
            ConfigureKnownObjects = (knownObjects, nextLevelVisitor, _, codeWriter) =>
            {
                knownObjects.Add(new FormattableStringVisitor(nextLevelVisitor, codeWriter));
            }
        };

        var dumper = new VisualBasicDumper(dumpOptions);

        var result = dumper.Dump(str);

        Assert.Equal(
            """
            Dim concreteFormattableStringValue = FormattableStringFactory.Create("Hello, {0}", "World")
            
            """, result);
    }

    private class ServiceDescriptorVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
    {
        public string Id => nameof(ServiceDescriptor);

        public bool IsSuitableFor(object obj, Type objectType)
        {
            return obj is ServiceDescriptor;
        }

        public void ConfigureOptions(Action<DumpOptions> configure)
        {
        }

        public void Visit(object obj, Type objectType, VisitContext context)
        {
            var serviceDescriptor = (ServiceDescriptor)obj;

            var typeParameters = new List<CodeTypeInfo>
            {
                serviceDescriptor.ServiceType
            };

            if (serviceDescriptor.ImplementationType != null)
            {
                typeParameters.Add(serviceDescriptor.ImplementationType);
            }

            var parameters = new List<Action>(1);

            if (serviceDescriptor.ImplementationInstance != null)
            {
                parameters.Add(() => nextDepthVisitor.Visit(serviceDescriptor.ImplementationInstance, context));
            }

            if (serviceDescriptor.ImplementationFactory != null)
            {
                var typeInfo = serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType;

                parameters.Add(() => codeWriter.WriteLambdaExpression(() => codeWriter.WriteDefaultValue(typeInfo), [() => codeWriter.WriteVariableReference("serviceProvider")]));

            }

            codeWriter.WriteMethodInvoke(() =>
                codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(typeof(ServiceDescriptor)),
                    serviceDescriptor.Lifetime.ToString(), typeParameters.ToArray()),
                parameters);
        }
    }

    private class FormattableStringVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
    {
        public string Id => nameof(FormattableString);

        public bool IsSuitableFor(object obj, Type objectType)
        {
            return obj is FormattableString;
        }

        public void ConfigureOptions(Action<DumpOptions> configure)
        {
        }

        public void Visit(object obj, Type objectType, VisitContext context)
        {
            var formattableString = (FormattableString)obj;

            IEnumerable<Action> arguments =
            [
                () => codeWriter.WritePrimitive(formattableString.Format)
            ];

            arguments = arguments.Concat(formattableString.GetArguments().Select(a => (Action)(() => nextDepthVisitor.Visit(a, context))));

            codeWriter.WriteMethodInvoke(() =>
                codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(typeof(FormattableStringFactory)),
                    nameof(FormattableStringFactory.Create)),
                arguments);
        }
    }

    internal sealed class CustomUriVisitor(INextDepthVisitor nextDepthVisitor, DumpOptions options, ICodeWriter codeWriter) : IKnownObjectVisitor
    {
        private readonly ObjectDescriptionWriter _descriptionWriter = new(nextDepthVisitor, codeWriter);

        public string Id => nameof(Uri);

        public bool IsSuitableFor(object obj, Type objectType)
        {
            return obj is Uri;
        }

        public void ConfigureOptions(Action<DumpOptions> configure)
        {
            options = options.Clone();
            configure?.Invoke(options);
        }

        public void Visit(object obj, Type objectType, VisitContext context)
        {
            var uri = (Uri)obj;

            var objectDescription = new ObjectDescription
            {
                Type = objectType,
                ConstructorArguments = GetConstructorArguments()
            };

            _descriptionWriter.Write(objectDescription, context, options);

            return;

            IEnumerable<ConstructorArgumentDescription> GetConstructorArguments()
            {
                yield return new ConstructorArgumentDescription
                {
                    Type = typeof(string),
                    Name = "uriString",
                    Value = uri.OriginalString
                };

                if (!uri.IsAbsoluteUri)
                {
                    yield return new ConstructorArgumentDescription
                    {
                        Type = typeof(UriKind),
                        Name = "uriKind",
                        Value = UriKind.Relative
                    };
                }
            }
        }
    }
}