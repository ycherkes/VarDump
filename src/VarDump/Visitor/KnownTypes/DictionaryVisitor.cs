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
    private readonly IRootObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;
    private readonly int _maxCollectionSize;

    public DictionaryVisitor(IRootObjectVisitor rootObjectVisitor, ICodeWriter codeWriter, int maxCollectionSize)
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

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        IDictionary dict = (IDictionary)obj;
        if (context.IsVisited(dict))
        {
            _codeWriter.WriteCircularReferenceDetected();
            return;
        }

        context.PushVisited(dict);

        try
        {
            var valuesType = dict.Values.GetType();
            var keysType = dict.Keys.GetType();

            if (keysType.ContainsAnonymousType() ||
                valuesType.ContainsAnonymousType())
            {
                VisitAnonymousDictionary(dict, context);
                return;
            }

            VisitSimpleDictionary(dict, context);
        }
        finally
        {
            context.PopVisited();
        }
    }

    private void VisitSimpleDictionary(IDictionary dict, VisitContext context)
    {
        var items = dict.Cast<object>().Select(item => (Action)(() => VisitKeyValuePairWriteImplicitly(item, context)));

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
                    new CodeCollectionTypeInfo(dictionaryType), [], items);

            _codeWriter.WriteMethodInvoke(() => _codeWriter.WriteMethodReference(dictionaryCreateAction, $"To{type.GetImmutableOrFrozenTypeName()}"), []);

            return;
        }

        _codeWriter.WriteObjectCreateAndInitialize(new CodeCollectionTypeInfo(type), [], items);
    }

    private void VisitAnonymousDictionary(IEnumerable dictionary, VisitContext context)
    {
        const string keyName = "Key";
        const string valueName = "Value";
        var items = dictionary.Cast<object>().Select(o => (Action)(() => VisitKeyValuePairWriteAnonymousType(o, keyName, valueName, context)));

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
                    () => _codeWriter.WriteArrayCreate(new CodeAnonymousTypeInfo { ArrayRank = 1 }, items),
                    methodName),
            [
                WriteKeyLambda,
                WriteValueLambda
            ]);
        return;

        void WriteVariable() => _codeWriter.WriteVariableReference("kvp");
        void WriteKeyLambdaProperty() => _codeWriter.WritePropertyReference(keyName, WriteVariable);
        void WriteKeyLambda() => _codeWriter.WriteLambdaExpression(WriteKeyLambdaProperty, [WriteVariable]);
        void WriteValueLambdaProperty() => _codeWriter.WritePropertyReference(valueName, WriteVariable);
        void WriteValueLambda() => _codeWriter.WriteLambdaExpression(WriteValueLambdaProperty, [WriteVariable]);
    }

    private void VisitKeyValuePairWriteImplicitly(object o, VisitContext context)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Take(2).ToArray();
        _codeWriter.WriteImplicitKeyValuePairCreate(() => _rootObjectVisitor.Visit(propertyValues[0], context), () => _rootObjectVisitor.Visit(propertyValues[1], context));
    }

    private void VisitKeyValuePairWriteAnonymousType(object o, string keyName, string valueName, VisitContext context)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Take(2).ToArray();
        
        _codeWriter.WriteObjectCreateAndInitialize(new CodeAnonymousTypeInfo(), [],
            [
                () => _codeWriter.WriteAssign(() => _codeWriter.WritePropertyReference(keyName, null), () => _rootObjectVisitor.Visit(propertyValues[0], context)),
                () => _codeWriter.WriteAssign(() => _codeWriter.WritePropertyReference(valueName, null), () => _rootObjectVisitor.Visit(propertyValues[1], context)),
            ]);
    }
}