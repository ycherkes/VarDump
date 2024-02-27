using System;
using System.Globalization;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;

namespace VarDump.Visitor.KnownTypes;

internal sealed class CultureInfoVisitor : IKnownObjectVisitor
{
    private readonly IDotnetCodeGenerator _codeGenerator;

    public CultureInfoVisitor(IDotnetCodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
    }

    public string Id => nameof(CultureInfo);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is CultureInfo;
    }

    public void Visit(object obj, Type objectType)
    {
        var cultureInfo = (CultureInfo)obj;

        _codeGenerator.GenerateObjectCreateAndInitialize(new CodeDotnetTypeReference(typeof(CultureInfo)),
            [
                () => _codeGenerator.GeneratePrimitive(cultureInfo.ToString())
            ],
            []);
    }
}