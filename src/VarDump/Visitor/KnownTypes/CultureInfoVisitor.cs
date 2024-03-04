using System;
using System.Globalization;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;

namespace VarDump.Visitor.KnownTypes;

internal sealed class CultureInfoVisitor : IKnownObjectVisitor
{
    private readonly ICodeWriter _codeWriter;

    public CultureInfoVisitor(ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
    }

    public string Id => nameof(CultureInfo);
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is CultureInfo;
    }

    public void Visit(object obj, Type objectType)
    {
        var cultureInfo = (CultureInfo)obj;

        _codeWriter.WriteObjectCreateAndInitialize(typeof(CultureInfo),
            [
                () => _codeWriter.WritePrimitive(cultureInfo.ToString())
            ],
            []);
    }
}