using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.UnitTests.TestModel;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class KnownObjectsSpec
{
    [Fact]
    public void ApplyDifferentSettingsToDifferentKnownObjectVisitorsCSharp()
    {
        var anonymous = new
        {
            DateTime = DateTime.ParseExact("2023-08-05T12:47:09.9361937+02:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            TimeSpan = new TimeSpan(12, 11, 10)
        };

        var dumpOptions = new DumpOptions
        {
            ConfigureKnownObjects = (knownObjects, _, _, _) =>
            {
                var dateTimeVisitor = knownObjects[nameof(DateTime)];
                dateTimeVisitor.ConfigureOptions(dateTimeOptions =>
                {
                    dateTimeOptions.DateTimeInstantiation = DateTimeInstantiation.New;
                    dateTimeOptions.DateKind = DateKind.ConvertToUtc;
                });
                var timeSpanVisitor = knownObjects[nameof(TimeSpan)];
                timeSpanVisitor.ConfigureOptions(timeSpanOptions =>
                {
                    timeSpanOptions.DateTimeInstantiation = DateTimeInstantiation.Parse;
                });
            }
        };

        var dumper = new CSharpDumper(dumpOptions);

        var result = dumper.Dump(anonymous);

        Assert.Equal("""
                     var anonymousType = new 
                     {
                         DateTime = new DateTime(2023, 8, 5, 10, 47, 9, 936, DateTimeKind.Utc).AddTicks(1937),
                         TimeSpan = TimeSpan.ParseExact("12:11:10", "c", CultureInfo.InvariantCulture, TimeSpanStyles.None)
                     };
                     
                     """, result);
    }

    [Fact]
    public void DumpServiceDescriptorCSharp()
    {
        var serviceCollection = new ServiceCollection
        {
            ServiceDescriptor.Transient<IPerson>(_ => new Person()), // It's not possible to reconstruct the expression by existing Func
            ServiceDescriptor.Singleton<IPerson, Person>(),
            ServiceDescriptor.Scoped<IPerson, Person>()
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
        var personServiceDescriptor = ServiceDescriptor.Transient<IPerson, Person>();

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
            ConfigureKnownObjects = (knownObjects, nextLevelVisitor, options, codeWriter) =>
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
            ConfigureKnownObjects = (knownObjects, nextLevelVisitor, options, codeWriter) =>
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

                parameters.Add(() => codeWriter.WriteLambdaExpression(() => codeWriter.WriteDefaultValue(typeInfo), [ () => codeWriter.WriteVariableReference("serviceProvider")]));
               
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
}