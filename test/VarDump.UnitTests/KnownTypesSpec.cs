using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using VarDump.CodeDom.Common;
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

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor, opts) =>
        {
            var sdv = new ServiceDescriptorVisitor(rootObjectVisitor, opts);
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

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor, opts) =>
        {
            var sdv = new ServiceDescriptorVisitor(rootObjectVisitor, opts);
            knownObjects.Add(sdv.Id, sdv);
        };

        var dumper = new VisualBasicDumper(dumpOptions);

        var result = dumper.Dump(personServiceDescriptor);

        Assert.Equal(
            @"Dim serviceDescriptorValue = ServiceDescriptor.Transient(Of IPerson, Person)()
", result);
    }

    private class ServiceDescriptorVisitor(IObjectVisitor rootObjectVisitor, DumpOptions options) : IKnownObjectVisitor
    {
        private readonly CodeTypeReferenceOptions _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;

        public string Id => "ServiceDescriptor";
        public bool IsSuitableFor(object obj, Type objectType)
        {
            return obj is ServiceDescriptor;
        }

        public CodeExpression Visit(object obj, Type objectType)
        {
            var serviceDescriptor = (ServiceDescriptor)obj;

            var typeParameters = new List<CodeTypeReference>
            {
                new(serviceDescriptor.ServiceType, _typeReferenceOptions)
            };

            if (serviceDescriptor.ImplementationType != null)
            {
                typeParameters.Add(new CodeTypeReference(serviceDescriptor.ImplementationType, _typeReferenceOptions));
            }

            var parameters = new List<CodeExpression>(1);

            if (serviceDescriptor.ImplementationInstance != null)
            {
                parameters.Add(rootObjectVisitor.Visit(serviceDescriptor.ImplementationInstance));
            }

            if (serviceDescriptor.ImplementationFactory != null)
            {
                parameters.Add(new CodeLambdaExpression(new CodeDefaultValueExpression(new CodeTypeReference(
                    serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType, _typeReferenceOptions)),
                    new CodeVariableReferenceExpression("serviceProvider")));
            }

            return new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(new CodeTypeReference(typeof(ServiceDescriptor), _typeReferenceOptions)),
                            serviceDescriptor.Lifetime.ToString(), typeParameters.ToArray()),
                        parameters.ToArray());
        }
    }
}