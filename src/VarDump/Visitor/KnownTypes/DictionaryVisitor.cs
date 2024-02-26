using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DictionaryVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeGenerator _codeGenerator;
    private readonly int _maxCollectionSize;

    public DictionaryVisitor(IObjectVisitor rootObjectVisitor, ICodeGenerator codeGenerator, int maxCollectionSize)
    {
        if (maxCollectionSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCollectionSize));
        }
        
        _maxCollectionSize = maxCollectionSize;
        _rootObjectVisitor = rootObjectVisitor;
        _codeGenerator = codeGenerator;
    }

    public string Id => "Dictionary";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IDictionary;
    }

    public void Visit(object obj, Type objectType)
    {
        IDictionary dict = (IDictionary)obj;
        if (_rootObjectVisitor.IsVisited(dict))
        {
            CodeDomUtils.WriteCircularReferenceDetectedExpression(_codeGenerator);
            return;
        }

        _rootObjectVisitor.PushVisited(dict);

        try
        {
            var valuesType = dict.Values.GetType();
            var keysType = dict.Keys.GetType();

            if (keysType.ContainsAnonymousType() ||
                valuesType.ContainsAnonymousType())
            {
                VisitAnonymousDictionary(dict);
                return;
            }

            VisitSimpleDictionary(dict);
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }

    private void VisitSimpleDictionary(IDictionary dict)
    {
        var items = dict.Cast<object>().Select(item => (Action)(() => VisitKeyValuePairGenerateImplicitly(item)));

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, () => CodeDomUtils.WriteTooManyItemsExpression(_codeGenerator, _maxCollectionSize));
        }

        var type = dict.GetType();
        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        if (isImmutableOrFrozen)
        {
            var keyType = ReflectionUtils.GetInnerElementType(dict.Keys.GetType());
            var valueType = ReflectionUtils.GetInnerElementType(dict.Values.GetType());

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            var dictionaryCreateAction = () =>
                _codeGenerator.GenerateObjectCreateAndInitialize(
                    new CodeCollectionTypeReference(dictionaryType), [], items);

            _codeGenerator.GenerateMethodInvoke(() => _codeGenerator.GenerateMethodReference(dictionaryCreateAction, $"To{type.GetImmutableOrFrozenTypeName()}"), []);

            return;
        }

        _codeGenerator.GenerateObjectCreateAndInitialize(
            new CodeCollectionTypeReference(type), [], items);
    }

    private void VisitAnonymousDictionary(IEnumerable dictionary)
    {
        const string keyName = "Key";
        const string valueName = "Value";
        var items = dictionary.Cast<object>().Select(o => (Action)(() => VisitKeyValuePairGenerateAnonymousType(o, keyName, valueName)));

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, () => CodeDomUtils.WriteTooManyItemsExpression(_codeGenerator, _maxCollectionSize));
        }
        
        var type = dictionary.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        var methodName = isImmutableOrFrozen
            ? $"To{type.GetImmutableOrFrozenTypeName()}"
            : "ToDictionary";

        _codeGenerator.GenerateMethodInvoke(() =>
                _codeGenerator.GenerateMethodReference(
                    () => _codeGenerator.GenerateArrayCreate(new CodeAnonymousTypeReference { ArrayRank = 1 },
                        items),
                    methodName),
            [
                GenerateKeyLambdaExpression,
                GenerateValueLambdaExpression
            ]);

        void GenerateVariableReference() => _codeGenerator.GenerateVariableReference("kvp");
        void GenerateKeyLambdaPropertyExpression() => _codeGenerator.GeneratePropertyReference(keyName, GenerateVariableReference);
        void GenerateKeyLambdaExpression() => _codeGenerator.GenerateLambdaExpression(GenerateKeyLambdaPropertyExpression, [GenerateVariableReference]);
        void GenerateValueLambdaPropertyExpression() => _codeGenerator.GeneratePropertyReference(valueName, GenerateVariableReference);
        void GenerateValueLambdaExpression() => _codeGenerator.GenerateLambdaExpression(GenerateValueLambdaPropertyExpression, [GenerateVariableReference]);
    }

    private void VisitKeyValuePairGenerateImplicitly(object o)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Take(2).ToArray();
        _codeGenerator.GenerateCodeImplicitKeyValuePairCreate(() => _rootObjectVisitor.Visit(propertyValues[0]), () => _rootObjectVisitor.Visit(propertyValues[1]));
    }

    private void VisitKeyValuePairGenerateAnonymousType(object o, string keyName, string valueName)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Take(2).ToArray();
        
        _codeGenerator.GenerateObjectCreateAndInitialize(new CodeAnonymousTypeReference(), [],
            [
                () => _codeGenerator.GenerateCodeAssign(() => _codeGenerator.GeneratePropertyReference(keyName, null), () => _rootObjectVisitor.Visit(propertyValues[0])),
                () => _codeGenerator.GenerateCodeAssign(() => _codeGenerator.GeneratePropertyReference(valueName, null), () => _rootObjectVisitor.Visit(propertyValues[1])),
            ]);
    }
}