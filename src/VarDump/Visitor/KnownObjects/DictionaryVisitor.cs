using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownObjects;

internal sealed class DictionaryVisitor : IKnownObjectVisitor
{
    private readonly INextDepthVisitor _nextDepthVisitor;
    private readonly ICodeWriter _codeWriter;
    private DumpOptions _options;

    public DictionaryVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, DumpOptions options)
    {
        if (options.MaxCollectionSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxCollectionSize));
        }

        _nextDepthVisitor = nextDepthVisitor;
        _codeWriter = codeWriter;
        _options = options;
    }

    public string Id => "Dictionary";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IDictionary;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        _options = _options.Clone();
        configure?.Invoke(_options);
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        IDictionary dict = (IDictionary)obj;
        if (!context.TryAddVisited(dict))
        {
            _codeWriter.WriteCircularReferenceDetected();
            return;
        }

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
            context.RemoveVisited(dict);
        }
    }

    private void VisitSimpleDictionary(IDictionary dict, VisitContext context)
    {
        var items = GetItems(dict);

        var type = dict.GetType();
        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        if (isImmutableOrFrozen)
        {
            var keyType = ReflectionUtils.GetInnerElementType(dict.Keys.GetType());
            var valueType = ReflectionUtils.GetInnerElementType(dict.Values.GetType());

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            var dictionaryCreateAction = WriteDictionaryCreate(new CodeCollectionTypeInfo(dictionaryType));

            _codeWriter.WriteMethodInvoke(() => _codeWriter.WriteMethodReference(dictionaryCreateAction, $"To{type.GetImmutableOrFrozenTypeName()}"), []);

            return;
        }

        WriteDictionaryCreate(new CodeCollectionTypeInfo(type))();

        return;

        Action WriteDictionaryCreate(CodeTypeInfo dictionaryTypeInfo)
        {
            return () => _codeWriter.WriteDictionaryCreateItems(dictionaryTypeInfo, items,
                item => WriteDictionaryItem(item, context));
        }
    }

    private void VisitAnonymousDictionary(IEnumerable dictionary, VisitContext context)
    {
        const string keyName = "Key";
        const string valueName = "Value";
        var items = GetItems(dictionary);
        
        var type = dictionary.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        var methodName = isImmutableOrFrozen
            ? $"To{type.GetImmutableOrFrozenTypeName()}"
            : "ToDictionary";

        _codeWriter.WriteMethodInvoke(() =>
                _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteArrayCreateItems(new CodeAnonymousTypeInfo { ArrayRank = 1 }, items,
                        item => WriteAnonymousDictionaryItem(item, keyName, valueName, context), false),
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
        _codeWriter.WriteImplicitKeyValuePairCreate(() => _nextDepthVisitor.Visit(propertyValues[0], context), () => _nextDepthVisitor.Visit(propertyValues[1], context));
    }

    private void WriteDictionaryItem(object item, VisitContext context)
    {
        if (ReferenceEquals(item, CollectionItemMarker.TooManyItems))
        {
            _codeWriter.WriteTooManyItems(_options.MaxCollectionSize);
            return;
        }

        VisitKeyValuePairWriteImplicitly(item, context);
    }

    private void WriteAnonymousDictionaryItem(object item, string keyName, string valueName, VisitContext context)
    {
        if (ReferenceEquals(item, CollectionItemMarker.TooManyItems))
        {
            _codeWriter.WriteTooManyItems(_options.MaxCollectionSize);
            return;
        }

        VisitKeyValuePairWriteAnonymousType(item, keyName, valueName, context);
    }

    private IEnumerable GetItems(IEnumerable items)
    {
        return _options.MaxCollectionSize == int.MaxValue ? items : TakeItems(items);
    }

    private IEnumerable<object> TakeItems(IEnumerable items)
    {
        var count = 0;
        foreach (var item in items)
        {
            if (count++ == _options.MaxCollectionSize)
            {
                yield return CollectionItemMarker.TooManyItems;
                yield break;
            }

            yield return item;
        }
    }

    private void VisitKeyValuePairWriteAnonymousType(object o, string keyName, string valueName, VisitContext context)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Take(2).ToArray();
        
        _codeWriter.WriteObjectCreateAndInitializeItems(
            new CodeAnonymousTypeInfo(),
            [],
            new[]
            {
                new KeyValuePair<string, object>(keyName, propertyValues[0]),
                new KeyValuePair<string, object>(valueName, propertyValues[1])
            },
            initializer =>
            {
                var pair = (KeyValuePair<string, object>)initializer;
                _codeWriter.WriteMemberAssignmentStart(pair.Key);
                _nextDepthVisitor.Visit(pair.Value, context);
            });
    }
}
