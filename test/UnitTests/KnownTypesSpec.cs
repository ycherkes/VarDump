using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.TestModel;
using VarDumpExtended;
using VarDumpExtended.CodeDom.Common;
using VarDumpExtended.Visitor;
using VarDumpExtended.Visitor.KnownTypes;
using Xunit;

namespace UnitTests;

public class KnownTypesSpec
{
    class ServiceDescriptorVisitor : IKnownObjectVisitor
    {
        private readonly IObjectVisitor _rootObjectVisitor;
        private readonly CodeTypeReferenceOptions _typeReferenceOptions;

        public ServiceDescriptorVisitor(IObjectVisitor rootObjectVisitor, DumpOptions options)
        {
            _typeReferenceOptions = options.UseTypeFullName
                ? CodeTypeReferenceOptions.FullTypeName
                : CodeTypeReferenceOptions.ShortTypeName;

            _rootObjectVisitor = rootObjectVisitor;
        }

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

            var parameters = serviceDescriptor.ImplementationInstance != null 
                ? new[]
                  {
                       _rootObjectVisitor.Visit(serviceDescriptor.ImplementationInstance)
                  }
                : Array.Empty<CodeExpression>();

            return new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(new CodeTypeReference(typeof(ServiceDescriptor), _typeReferenceOptions)),
                            serviceDescriptor.Lifetime.ToString(), typeParameters.ToArray()),
                        parameters);
        }
    }

    [Fact]
    public void DumpServiceDescriptorSpecCsharp()
    {
        var personServiceDescriptor = ServiceDescriptor.Transient<Person, Person>();

        var dumpOptions = DumpOptions.Default;

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor) =>
        {
            var sdv = new ServiceDescriptorVisitor(rootObjectVisitor, dumpOptions);
            knownObjects.Add(sdv.Id, sdv);
        };

        var dumper = new CSharpDumper(dumpOptions);

        var result = dumper.Dump(personServiceDescriptor);

        Assert.Equal(
            @"var serviceDescriptor = ServiceDescriptor.Transient<Person, Person>();
", result);
    }


    [Fact]
    public void DumpServiceDescriptorSpecVb()
    {
        var personServiceDescriptor = ServiceDescriptor.Transient<Person, Person>();

        var dumpOptions = DumpOptions.Default;

        dumpOptions.ConfigureKnownTypes = (knownObjects, rootObjectVisitor) =>
        {
            var sdv = new ServiceDescriptorVisitor(rootObjectVisitor, dumpOptions);
            knownObjects.Add(sdv.Id, sdv);
        };

        var dumper = new VisualBasicDumper(dumpOptions);

        var result = dumper.Dump(personServiceDescriptor);

        Assert.Equal(
            @"Dim serviceDescriptorValue = ServiceDescriptor.Transient(Of Person, Person)()
", result);
    }
}