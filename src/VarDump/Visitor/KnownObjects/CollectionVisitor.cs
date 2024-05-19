using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor.Format;

namespace VarDump.Visitor.KnownObjects;

internal sealed class CollectionVisitor : IKnownObjectVisitor
{
    private readonly INextDepthVisitor _nextDepthVisitor;
    private readonly ICodeWriter _codeWriter;
    private DumpOptions _options;

    public CollectionVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter, DumpOptions options)
    {
        if (options.MaxCollectionSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxCollectionSize));
        }

        _nextDepthVisitor = nextDepthVisitor;
        _codeWriter = codeWriter;
        _options = options;
    }

    public string Id => "Collection";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IEnumerable;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
        _options = _options.Clone();
        configure?.Invoke(_options);
    }

    public void Visit(object obj, Type collectionType, VisitContext context)
    {
        IEnumerable collection = (IEnumerable)obj;
        if (context.IsVisited(collection))
        {
            _codeWriter.WriteCircularReferenceDetected();
            return;
        }

        context.PushVisited(collection);

        try
        {
            var elementType = ReflectionUtils.GetInnerElementType(collectionType);

            if (elementType.IsGrouping())
            {
                VisitGroupingCollection(collection, context);
                return;
            }

            if (collectionType.ContainsAnonymousType())
            {
                VisitAnonymousCollection(collection, context);
                return;
            }

            VisitSimpleCollection(collection, elementType, context);
        }
        finally
        {
            context.PopVisited();
        }
    }

    private void VisitGroupingCollection(IEnumerable collection, VisitContext context)
    {
        var type = collection.GetType();

        var items = VisitGroupings(collection.Cast<object>(), context);

        if (_options.MaxCollectionSize < int.MaxValue)
        {
            items = items.Take(_options.MaxCollectionSize + 1).Replace(_options.MaxCollectionSize, () => _codeWriter.WriteTooManyItems(_options.MaxCollectionSize));
        }

        var isLookup = type.IsLookup();

        var methodName = isLookup
            ? "ToLookup"
            : "GroupBy";

        if (type.IsArray)
        {
           _codeWriter.WriteMethodInvoke(() =>
                    _codeWriter.WriteMethodReference(WriteLambda, "ToArray"), []);

           return;

        }

        if (collection is IList)
        {
            _codeWriter.WriteMethodInvoke(() =>
                _codeWriter.WriteMethodReference(WriteLambda, "ToList"), []);

            return;
        }

        WriteLambda();

        return;

        
        void WriteLambda() => _codeWriter.WriteMethodInvoke(() =>
                _codeWriter.WriteMethodReference(WriteArrayCreate, methodName),
            [
                WriteKeyLambdaExpression,
                WriteValueLambdaExpression
            ]);

        void WriteArrayCreate() => _codeWriter.WriteArrayCreate(new CodeAnonymousTypeInfo { ArrayRank = 1 }, items, false);
        void WriteVariableReference() => _codeWriter.WriteVariableReference("grp");
        void WriteKeyLambdaPropertyExpression() => _codeWriter.WritePropertyReference("Key", WriteVariableReference);
        void WriteKeyLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteKeyLambdaPropertyExpression, [WriteVariableReference]);
        void WriteValueLambdaPropertyExpression() => _codeWriter.WritePropertyReference("Element", WriteVariableReference);
        void WriteValueLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteValueLambdaPropertyExpression, [WriteVariableReference]);
    }

    private void VisitSimpleCollection(IEnumerable enumerable, Type elementType, VisitContext context)
    {
        var items = enumerable.Cast<object>().Select(item => (Action)(() => _nextDepthVisitor.Visit(item, context)));

        if (_options.MaxCollectionSize < int.MaxValue)
        {
            items = items.Take(_options.MaxCollectionSize + 1).Replace(_options.MaxCollectionSize, () => _codeWriter.WriteTooManyItems(_options.MaxCollectionSize));
        }

        var type = enumerable.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();
        var isCollection = IsCollection(enumerable);

        var singleLine = typeof(string) != elementType
                         && ReflectionUtils.IsPrimitive(elementType)
                         && _options.Formatting?.PrimitiveCollection == CollectionFormat.SingleLine;

        if (type.IsArray || isImmutableOrFrozen || !type.IsPublic || !isCollection)
        {
            if (type.IsArray && ((Array)enumerable).Rank > 1 && ((Array)enumerable).Length > 0)
            {
                items = ChunkMultiDimensionalArrayExpression((Array)enumerable, items, singleLine);
                singleLine = false;
            }

            var arrayType = isImmutableOrFrozen || !type.IsPublic ? elementType.MakeArrayType() : type;

            void WriteArrayCreate() => _codeWriter.WriteArrayCreate(arrayType, items, singleLine: singleLine);

            if (isImmutableOrFrozen)
            {
               _codeWriter.WriteMethodInvoke(() =>
                    _codeWriter.WriteMethodReference(WriteArrayCreate, $"To{type.GetImmutableOrFrozenTypeName()}"), []);
            }
            else
            {
                WriteArrayCreate();
            }

            return;
        }

        if (type.IsReadonlyCollection())
        {
            var typeInfo =
                new CodeCollectionTypeInfo(typeof(List<>).MakeGenericType(elementType));

            _codeWriter.WriteMethodInvoke(() => _codeWriter.WriteMethodReference(() => 
               _codeWriter.WriteObjectCreateAndInitialize(typeInfo, [], items), "AsReadOnly"), []);

            return;
        }

        _codeWriter.WriteObjectCreateAndInitialize(new CodeCollectionTypeInfo(type), [], items, singleLine);
    }

    private IEnumerable<Action> ChunkMultiDimensionalArrayExpression(Array array, IEnumerable<Action> enumerable,
        bool singleLine)
    {
        var dimensions = new int[array.Rank - 1];

        for (var i = 0; i < dimensions.Length; i++)
        {
            dimensions[i] = array.GetLength(i + 1);
        }

        IEnumerable<Action> result = enumerable;

        for (var index = dimensions.Length - 1; index >= 0; index--)
        {
            var dimension = dimensions[index];
            result = result.Chunk(dimension).Select(x => (Action) (()=> _codeWriter.WriteArrayDimension(x, singleLine)));
        }

        return result;
    }

    private void VisitAnonymousCollection(IEnumerable enumerable, VisitContext context)
    {
        var items = enumerable.Cast<object>().Select(item => (Action)(() => _nextDepthVisitor.Visit(item, context)));

        if (_options.MaxCollectionSize < int.MaxValue)
        {
            items = items.Take(_options.MaxCollectionSize + 1).Replace(_options.MaxCollectionSize, () => _codeWriter.WriteTooManyItems(_options.MaxCollectionSize));
        }

        var type = enumerable.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        var typeInfo = new CodeAnonymousTypeInfo { ArrayRank = 1 };

        if (type.IsArray && ((Array)enumerable).Rank > 1 && ((Array)enumerable).Length > 0)
        {
            typeInfo.ArrayRank = ((Array)enumerable).Rank;
            items = ChunkMultiDimensionalArrayExpression((Array)enumerable, items, false);
        }

        Action createAction = () => _codeWriter.WriteArrayCreate(typeInfo, items, false);

        if (isImmutableOrFrozen || enumerable is IList && !type.IsArray)
        {
            _codeWriter.WriteMethodInvoke(() =>
                _codeWriter.WriteMethodReference(createAction, $"To{type.GetImmutableOrFrozenTypeName()}"), []);
        }
        else
        {
            createAction();
        }
    }

    private static bool IsCollection(object obj)
    {
        return obj is ICollection || obj.GetType().IsGenericCollection();
    }

    private static KeyValuePair<object, IEnumerable> GetIGroupingValue(object o)
    {
        var objectType = o.GetType();
        var fieldValues = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(x => x.Name is "_key" or "key" or "_elements" or "elements")
            .Select(p => ReflectionUtils.GetValue(p, o))
            .ToArray();

        return new KeyValuePair<object, IEnumerable>(fieldValues[0], (IEnumerable)fieldValues[1]);
    }

    private IEnumerable<Action> VisitGroupings(IEnumerable<object> objects, VisitContext context)
    {
        var items = objects.Select(GetIGroupingValue)
            .SelectMany(g => g.Value.Cast<object>().Select(e => new { g.Key, Element = e }));

        return items.Select(item =>(Action)(() => _nextDepthVisitor.Visit(item, context)));
    }
}