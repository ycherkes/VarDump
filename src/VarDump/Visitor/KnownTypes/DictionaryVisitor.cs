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
    private readonly ICodeWriter _codeWriter;
    private readonly int _maxCollectionSize;

    public DictionaryVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter, int maxCollectionSize)
    {
        if (maxCollectionSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCollectionSize));
        }
        
        _maxCollectionSize = maxCollectionSize;
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
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
            _codeWriter.WriteCircularReferenceDetected();
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
        var items = dict.Cast<object>().Select(item => (Action)(() => VisitKeyValuePairWriteImplicitly(item)));

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, () => _codeWriter.WriteTooManyItems(_maxCollectionSize));
        }

        var type = dict.GetType();
        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        if (isImmutableOrFrozen)
        {
            var keyType = ReflectionUtils.GetInnerElementType(dict.Keys.GetType());
            var valueType = ReflectionUtils.GetInnerElementType(dict.Values.GetType());

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            var dictionaryCreateAction = () =>
                _codeWriter.WriteObjectCreateAndInitialize(
                    new CollectionTypeReference(dictionaryType), [], items);

            _codeWriter.WriteMethodInvoke(() => _codeWriter.WriteMethodReference(dictionaryCreateAction, $"To{type.GetImmutableOrFrozenTypeName()}"), []);

            return;
        }

        _codeWriter.WriteObjectCreateAndInitialize(
            new CollectionTypeReference(type), [], items);
    }

    private void VisitAnonymousDictionary(IEnumerable dictionary)
    {
        const string keyName = "Key";
        const string valueName = "Value";
        var items = dictionary.Cast<object>().Select(o => (Action)(() => VisitKeyValuePairWriteAnonymousType(o, keyName, valueName)));

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, () => _codeWriter.WriteTooManyItems(_maxCollectionSize));
        }
        
        var type = dictionary.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        var methodName = isImmutableOrFrozen
            ? $"To{type.GetImmutableOrFrozenTypeName()}"
            : "ToDictionary";

        _codeWriter.WriteMethodInvoke(() =>
                _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteArrayCreate(new AnonymousTypeReference { ArrayRank = 1 },
                        items),
                    methodName),
            [
                WriteKeyLambdaExpression,
                WriteValueLambdaExpression
            ]);

        void WriteVariableReference() => _codeWriter.WriteVariableReference("kvp");
        void WriteKeyLambdaPropertyExpression() => _codeWriter.WritePropertyReference(keyName, WriteVariableReference);
        void WriteKeyLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteKeyLambdaPropertyExpression, [WriteVariableReference]);
        void WriteValueLambdaPropertyExpression() => _codeWriter.WritePropertyReference(valueName, WriteVariableReference);
        void WriteValueLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteValueLambdaPropertyExpression, [WriteVariableReference]);
    }

    private void VisitKeyValuePairWriteImplicitly(object o)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Take(2).ToArray();
        _codeWriter.WriteImplicitKeyValuePairCreate(() => _rootObjectVisitor.Visit(propertyValues[0]), () => _rootObjectVisitor.Visit(propertyValues[1]));
    }

    private void VisitKeyValuePairWriteAnonymousType(object o, string keyName, string valueName)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Take(2).ToArray();
        
        _codeWriter.WriteObjectCreateAndInitialize(new AnonymousTypeReference(), [],
            [
                () => _codeWriter.WriteAssign(() => _codeWriter.WritePropertyReference(keyName, null), () => _rootObjectVisitor.Visit(propertyValues[0])),
                () => _codeWriter.WriteAssign(() => _codeWriter.WritePropertyReference(valueName, null), () => _rootObjectVisitor.Visit(propertyValues[1])),
            ]);
    }
}