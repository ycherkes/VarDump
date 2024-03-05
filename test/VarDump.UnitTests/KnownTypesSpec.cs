using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.UnitTests.TestModel;
using VarDump.Visitor;
using VarDump.Visitor.KnownTypes;
using Xunit;

namespace VarDump.UnitTests;

public class KnownTypesSpec
{
    [Fact]
    public void DumpServiceDescriptorSpecCsharp()
    {
        var serviceCollection = new ServiceCollection
        {
            ServiceDescriptor.Transient<IPerson>(_ => new Person()), // It's not possible to reconstruct the expression by existing Func
            ServiceDescriptor.Singleton<IPerson, Person>(),
            ServiceDescriptor.Scoped<IPerson, Person>()
        };

        var dumpOptions = DumpOptions.Default;

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor, _, codeWriter) =>
        {
            var sdv = new ServiceDescriptorVisitor(rootObjectVisitor, codeWriter);
            knownObjects.Add(sdv.Id, sdv);
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
    public void DumpServiceDescriptorSpecVb()
    {
        var personServiceDescriptor = ServiceDescriptor.Transient<IPerson, Person>();

        var dumpOptions = DumpOptions.Default;

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor, _, codeWriter) =>
        {
            var sdv = new ServiceDescriptorVisitor(rootObjectVisitor, codeWriter);
            knownObjects.Add(sdv.Id, sdv);
        };

        var dumper = new VisualBasicDumper(dumpOptions);

        var result = dumper.Dump(personServiceDescriptor);

        Assert.Equal(
            @"Dim serviceDescriptorValue = ServiceDescriptor.Transient(Of IPerson, Person)()
", result);
    }

    [Fact]
    public void DumpFormattableStringCsharp()
    {
        const string name = "World";
        FormattableString str = $"Hello, {name}";

        var dumpOptions = DumpOptions.Default;

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor, _, codeWriter) =>
        {
            var sdv = new FormattableStringVisitor(rootObjectVisitor, codeWriter);
            knownObjects.Add(sdv.Id, sdv);
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

        var dumpOptions = DumpOptions.Default;

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor, _, codeWriter) =>
        {
            var sdv = new FormattableStringVisitor(rootObjectVisitor, codeWriter);
            knownObjects.Add(sdv.Id, sdv);
        };

        var dumper = new VisualBasicDumper(dumpOptions);

        var result = dumper.Dump(str);

        Assert.Equal(
            """
            Dim concreteFormattableStringValue = FormattableStringFactory.Create("Hello, {0}", "World")
            
            """, result);
    }

    private class ServiceDescriptorVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
    {
        public string Id => "ServiceDescriptor";
        public bool IsSuitableFor(object obj, Type objectType)
        {
            return obj is ServiceDescriptor;
        }

        public void Visit(object obj, Type objectType)
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
                parameters.Add(() => rootObjectVisitor.Visit(serviceDescriptor.ImplementationInstance));
            }

            if (serviceDescriptor.ImplementationFactory != null)
            {
                var typeInfo = serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType;

                parameters.Add(() => codeWriter.WriteLambdaExpression(() => codeWriter.WriteDefaultValue(typeInfo), [ () => codeWriter.WriteVariableReference("serviceProvider")]));
               
            }

            codeWriter.WriteMethodInvoke(() => 
                codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(typeof(ServiceDescriptor)),
                    serviceDescriptor.Lifetime.ToString(), typeParameters.ToArray()
                    ), parameters);
        }
    }

    private class FormattableStringVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
    {
        public string Id => "ServiceDescriptor";
        public bool IsSuitableFor(object obj, Type objectType)
        {
            return obj is FormattableString;
        }

        public void Visit(object obj, Type objectType)
        {
            var formattableString = (FormattableString)obj;

            IEnumerable<Action> argumentActions =
            [
                () => codeWriter.WritePrimitive(formattableString.Format)
            ];

            argumentActions = argumentActions.Concat(formattableString.GetArguments().Select(a => (Action)(() => rootObjectVisitor.Visit(a))));

            codeWriter.WriteMethodInvoke(() =>
                codeWriter.WriteMethodReference(
                    () => codeWriter.WriteType(typeof(FormattableStringFactory)),
                    nameof(FormattableStringFactory.Create)),
                argumentActions);
        }
    }
}