﻿using System;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor.KnownTypes;

internal sealed class AnonymousTypeVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly IObjectDescriptor _anonymousObjectDescriptor;
    private readonly ICodeGenerator _codeGenerator;

    public AnonymousTypeVisitor(IObjectVisitor rootObjectVisitor,
        IObjectDescriptor anonymousObjectDescriptor, ICodeGenerator codeGenerator)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _anonymousObjectDescriptor = anonymousObjectDescriptor;
        _codeGenerator = codeGenerator;
    }

    public string Id => "Anonymous";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsAnonymousType();
    }

    public void Visit(object obj, Type objectType)
    {
        var initializeActions = _anonymousObjectDescriptor.Describe(obj, objectType)
            .Select(pv => (Action)(() => _codeGenerator.GenerateCodeAssign(
                () => _codeGenerator.GeneratePropertyReference(pv.Name, null),
                () =>
                {
                    if (pv.Type.IsNullableType() || pv.Value == null)
                    {
                        _codeGenerator.GenerateCast(new CodeTypeReference(pv.Type), 
                            () => _rootObjectVisitor.Visit(pv.Value));
                    }
                    else
                    {
                        _rootObjectVisitor.Visit(pv.Value);
                    }
                })));

        _codeGenerator.GenerateObjectCreateAndInitialize(new CodeAnonymousTypeReference(),
            [],
            initializeActions);
    }
}