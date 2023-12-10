using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VarDumpExtended.CodeDom.Common;
using VarDumpExtended.Utils;

namespace VarDumpExtended.Visitor.KnownTypes;

internal sealed class DictionaryVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;

    public DictionaryVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
        _rootObjectVisitor = rootObjectVisitor;
    }

    public string Id => "Dictionary";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IDictionary;
    }

    public CodeExpression Visit(object obj, Type objectType)
    {
        IDictionary dict = (IDictionary)obj;
        if (_rootObjectVisitor.IsVisited(dict))
        {
            return CodeDomUtils.GetCircularReferenceDetectedExpression();
        }

        _rootObjectVisitor.PushVisited(dict);

        try
        {
            var valuesType = dict.Values.GetType();
            var keysType = dict.Keys.GetType();

            var result = keysType.ContainsAnonymousType() ||
                         valuesType.ContainsAnonymousType()
                ? VisitAnonymousDictionary(dict)
                : VisitSimpleDictionary(dict);

            return result;
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }

    private CodeExpression VisitSimpleDictionary(IDictionary dict)
    {
        var items = dict.Cast<object>().Select(VisitKeyValuePairGenerateImplicitly);

        var type = dict.GetType();
        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        if (isImmutableOrFrozen)
        {
            var keyType = ReflectionUtils.GetInnerElementType(dict.Keys.GetType());
            var valueType = ReflectionUtils.GetInnerElementType(dict.Values.GetType());

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            CodeExpression dictionaryCreateExpression = new CodeObjectCreateAndInitializeExpression(new CodeCollectionTypeReference(dictionaryType, _typeReferenceOptions), items);

            dictionaryCreateExpression = new CodeMethodInvokeExpression(dictionaryCreateExpression, $"To{type.GetImmutableOrFrozenTypeName()}");

            return dictionaryCreateExpression;
        }
        else
        {
            CodeExpression dictionaryCreateExpression = new CodeObjectCreateAndInitializeExpression(new CodeCollectionTypeReference(type, _typeReferenceOptions), items);
            return dictionaryCreateExpression;
        }
    }

    private CodeExpression VisitAnonymousDictionary(IEnumerable dictionary)
    {
        const string keyName = "Key";
        const string valueName = "Value";
        var items = dictionary.Cast<object>().Select(o => VisitKeyValuePairGenerateAnonymousType(o, keyName, valueName));
        var type = dictionary.GetType();

        CodeExpression expr = new CodeArrayCreateExpression(new CodeAnonymousTypeReference { ArrayRank = 1 }, items.ToArray());

        var variableReferenceExpression = new CodeVariableReferenceExpression("kvp");
        var keyLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, keyName), variableReferenceExpression);
        var valueLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, valueName), variableReferenceExpression);

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        expr = isImmutableOrFrozen
            ? new CodeMethodInvokeExpression(expr, $"To{ReflectionUtils.GetImmutableOrFrozenTypeName(type)}", keyLambdaExpression, valueLambdaExpression)
            : new CodeMethodInvokeExpression(expr, "ToDictionary", keyLambdaExpression, valueLambdaExpression);

        return expr;
    }

    private CodeExpression VisitKeyValuePairGenerateImplicitly(object o)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Select(_rootObjectVisitor.Visit).Take(2).ToArray();
        return new CodeImplicitKeyValuePairCreateExpression(propertyValues.First(), propertyValues.Last());
    }

    private CodeExpression VisitKeyValuePairGenerateAnonymousType(object o, string keyName, string valueName)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Select(_rootObjectVisitor.Visit).ToArray();
        var result = new CodeObjectCreateAndInitializeExpression(new CodeAnonymousTypeReference())
        {
            InitializeExpressions = new CodeExpressionCollection(new[]
            {
                (CodeExpression)new CodeAssignExpression(new CodePropertyReferenceExpression(null, keyName), propertyValues[0]),
                new CodeAssignExpression(new CodePropertyReferenceExpression(null, valueName), propertyValues[1])
            })
        };

        return result;
    }
}