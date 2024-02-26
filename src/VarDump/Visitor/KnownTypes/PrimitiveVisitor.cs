using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class PrimitiveVisitor : IKnownObjectVisitor
{
    private readonly ICodeGenerator _codeGenerator;

    public PrimitiveVisitor(ICodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
    }

    public string Id => "Primitive";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return ReflectionUtils.IsPrimitiveOrNull(obj);
    }

    public void Visit(object obj, Type objectType)
    {
        if (obj == null || ValueEquality(obj, 0) || obj is byte)
        {
            _codeGenerator.GeneratePrimitive(obj);
            return;
        }

        var specialValueName = new[]
        {
            nameof(int.MaxValue),
            nameof(int.MinValue),
            nameof(float.PositiveInfinity),
            nameof(float.NegativeInfinity),
            nameof(float.Epsilon),
            nameof(float.NaN)
        }
        .Where(specialValue => IsSpecialValueField(obj, objectType, specialValue))
        .FirstOrDefault(x => x != null);

        if (specialValueName != null)
        {
            _codeGenerator.GenerateFieldReference(specialValueName, () => _codeGenerator.GenerateTypeReference(new CodeTypeReference(objectType)));
            return;
        }

        _codeGenerator.GeneratePrimitive(obj);
    }

    private static bool IsSpecialValueField(object @object, IReflect objectType, string fieldName)
    {
        var field = objectType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

        return field != null && Equals(ReflectionUtils.GetValue(field, null), @object);
    }

    private static bool ValueEquality(object val1, object val2)
    {
        if (val1 is not IConvertible) return false;
        if (val2 is not IConvertible) return false;

        var converted2 = Convert.ChangeType(val2, val1.GetType());

        return val1.Equals(converted2);
    }
}