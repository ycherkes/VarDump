using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
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

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor, opts, codeGenerator) =>
        {
            var sdv = new ServiceDescriptorVisitor(rootObjectVisitor, codeGenerator);
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

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor, opts, codeGenerator) =>
        {
            var sdv = new ServiceDescriptorVisitor(rootObjectVisitor, codeGenerator);
            knownObjects.Add(sdv.Id, sdv);
        };

        var dumper = new VisualBasicDumper(dumpOptions);

        var result = dumper.Dump(personServiceDescriptor);

        Assert.Equal(
            @"Dim serviceDescriptorValue = ServiceDescriptor.Transient(Of IPerson, Person)()
", result);
    }

    private class ServiceDescriptorVisitor(IObjectVisitor rootObjectVisitor, ICodeGenerator codeGenerator) : IKnownObjectVisitor
    {
        public string Id => "ServiceDescriptor";
        public bool IsSuitableFor(object obj, Type objectType)
        {
            return obj is ServiceDescriptor;
        }

        public void Visit(object obj, Type objectType)
        {
            var serviceDescriptor = (ServiceDescriptor)obj;

            var typeParameters = new List<CodeTypeReference>
            {
                new(serviceDescriptor.ServiceType)
            };

            if (serviceDescriptor.ImplementationType != null)
            {
                typeParameters.Add(new CodeTypeReference(serviceDescriptor.ImplementationType));
            }

            var parameters = new List<Action>(1);

            if (serviceDescriptor.ImplementationInstance != null)
            {
                parameters.Add(() => rootObjectVisitor.Visit(serviceDescriptor.ImplementationInstance));
            }

            if (serviceDescriptor.ImplementationFactory != null)
            {
                var typeRef = new CodeTypeReference(
                    serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType);

                parameters.Add(() => codeGenerator.GenerateLambdaExpression(() => codeGenerator.GenerateDefaultValue(typeRef), [ () => codeGenerator.GenerateVariableReference("serviceProvider")]));
               
            }

            codeGenerator.GenerateMethodInvoke(() => 
                codeGenerator.GenerateMethodReference(
                    () => codeGenerator.GenerateTypeReference(new CodeTypeReference(typeof(ServiceDescriptor))),
                    serviceDescriptor.Lifetime.ToString(), typeParameters.ToArray()
                    ), parameters);
        }
    }
}