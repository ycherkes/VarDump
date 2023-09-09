using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class PrimitiveVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public PrimitiveVisitor(DumpOptions options)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
    }

    public string Id => "Primitive";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return ReflectionUtils.IsPrimitiveOrNull(obj);
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        if (obj == null || ValueEquality(obj, 0) || obj is byte)
        {
            return new CodePrimitiveExpression(obj);
        }

        var specialValueExpression = new[]
            {
                nameof(int.MaxValue),
                nameof(int.MinValue),
                nameof(float.PositiveInfinity),
                nameof(float.NegativeInfinity),
                nameof(float.Epsilon),
                nameof(float.NaN)
            }
            .Select(specialValue => GetSpecialValue(obj, objectType, specialValue))
            .FirstOrDefault(x => x != null);

        return specialValueExpression ?? new CodePrimitiveExpression(obj);
    }

    private CodeExpression GetSpecialValue(object @object, Type objectType, string fieldName)
    {
        var field = objectType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

        if (field == null) return null;

        return Equals(ReflectionUtils.GetValue(field, null), @object)
            ? new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(objectType, _typeReferenceOptions)), fieldName)
            : null;
    }
    private static bool ValueEquality(object val1, object val2)
    {
        if (val1 is not IConvertible) return false;
        if (val2 is not IConvertible) return false;

        var converted2 = Convert.ChangeType(val2, val1.GetType());

        return val1.Equals(converted2);
    }
}