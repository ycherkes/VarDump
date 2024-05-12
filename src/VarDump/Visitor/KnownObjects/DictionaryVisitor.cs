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

        if (_options.MaxCollectionSize < int.MaxValue)
        {
            items = items.Take(_options.MaxCollectionSize + 1).Replace(_options.MaxCollectionSize, () => _codeWriter.WriteTooManyItems(_options.MaxCollectionSize));
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

        if (_options.MaxCollectionSize < int.MaxValue)
        {
            items = items.Take(_options.MaxCollectionSize + 1).Replace(_options.MaxCollectionSize, () => _codeWriter.WriteTooManyItems(_options.MaxCollectionSize));
        }
        
        var type = dictionary.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        var methodName = isImmutableOrFrozen
            ? $"To{type.GetImmutableOrFrozenTypeName()}"
            : "ToDictionary";

        _codeWriter.WriteMethodInvoke(() =>
                _codeWriter.WriteMethodReference(
                    () => _codeWriter.WriteArrayCreate(new CodeAnonymousTypeInfo { ArrayRank = 1 }, items, false),
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

    private void VisitKeyValuePairWriteAnonymousType(object o, string keyName, string valueName, VisitContext context)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => ReflectionUtils.GetValue(p, o)).Take(2).ToArray();
        
        _codeWriter.WriteObjectCreateAndInitialize(new CodeAnonymousTypeInfo(), [],
            [
                () => _codeWriter.WriteAssign(() => _codeWriter.WritePropertyReference(keyName, null), () => _nextDepthVisitor.Visit(propertyValues[0], context)),
                () => _codeWriter.WriteAssign(() => _codeWriter.WritePropertyReference(valueName, null), () => _nextDepthVisitor.Visit(propertyValues[1], context)),
            ]);
    }
}