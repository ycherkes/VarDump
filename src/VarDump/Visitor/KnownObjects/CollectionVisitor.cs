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

internal sealed class CollectionVisitor : IKnownObjectVisitor, ICollectionInitializerBodyWriter
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
        if (!context.TryAddVisited(obj))
        {
            _codeWriter.WriteCircularReferenceDetected();
            return;
        }

        try
        {
            IEnumerable collection = (IEnumerable)obj;

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
            context.RemoveVisited(obj);
        }
    }

    public void WriteCollectionInitializerBody(object value, Type valueType, VisitContext context)
    {
        if (!context.TryAddVisited(value))
        {
            _codeWriter.WriteCircularReferenceDetected();
            return;
        }

        try
        {
            var collection = (IEnumerable)value;
            var elementType = ReflectionUtils.GetInnerElementType(valueType);
            var items = GetItems(collection);
            var singleLine = typeof(string) != elementType
                             && ReflectionUtils.IsPrimitive(elementType)
                             && _options.PrimitiveCollectionLayout == CollectionLayout.SingleLine;

            _codeWriter.WriteArrayDimensionItems(items,
                item => WriteCollectionItem(item, context, singleLine), singleLine);
        }
        finally
        {
            context.RemoveVisited(value);
        }
    }

    private void VisitGroupingCollection(IEnumerable collection, VisitContext context)
    {
        var type = collection.GetType();

        var items = GetItems(VisitGroupings(collection.Cast<object>()));

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

        void WriteArrayCreate() => _codeWriter.WriteArrayCreateItems(
            new CodeAnonymousTypeInfo { ArrayRank = 1 }, items, item => WriteCollectionItem(item, context), false);
        void WriteVariableReference() => _codeWriter.WriteVariableReference("grp");
        void WriteKeyLambdaPropertyExpression() => _codeWriter.WritePropertyReference("Key", WriteVariableReference);
        void WriteKeyLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteKeyLambdaPropertyExpression, [WriteVariableReference]);
        void WriteValueLambdaPropertyExpression() => _codeWriter.WritePropertyReference("Element", WriteVariableReference);
        void WriteValueLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteValueLambdaPropertyExpression, [WriteVariableReference]);
    }

    private void VisitSimpleCollection(IEnumerable enumerable, Type elementType, VisitContext context)
    {
        IEnumerable items = GetItems(enumerable);

        var type = enumerable.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();
        var isCollection = IsCollection(enumerable);
        var useCollectionExpression = _options.CollectionLiteralStyle == CollectionLiteralStyle.Expression
                                      && _codeWriter.SupportsCollectionExpression;
        var canEmitCollectionExpressionDirectly = useCollectionExpression
                                                  && CollectionExpressionUtils.CanEmitDirectly(enumerable, type);

        var singleLine = typeof(string) != elementType
                         && ReflectionUtils.IsPrimitive(elementType)
                         && _options.PrimitiveCollectionLayout == CollectionLayout.SingleLine;

        if (type.IsArray || isImmutableOrFrozen || !type.IsPublic || !isCollection)
        {
            if (type.IsArray && ((Array)enumerable).Rank > 1 && ((Array)enumerable).Length > 0)
            {
                items = ChunkMultiDimensionalArrayItems((Array)enumerable, items, singleLine);
                singleLine = false;
            }

            var isQueryable = IsQueryable(type);

            var arrayType = isImmutableOrFrozen || !type.IsPublic || isQueryable ? elementType.MakeArrayType() : type;

            void WriteArrayCreate() => _codeWriter.WriteArrayCreateItems(
                arrayType, items, item => WriteArrayItem(item, context, singleLine), singleLine);

            void WriteArrayCollectionExpression() => _codeWriter.WriteCollectionExpressionItems(
                items, item => WriteArrayItem(item, context, singleLine), singleLine);

            void WriteTypedArrayCollectionExpression() =>
                _codeWriter.WriteCast(arrayType, WriteArrayCollectionExpression);

            void WriteStaticCollectionConversion(Type conversionType, string methodName) =>
                _codeWriter.WriteMethodInvoke(
                    () => _codeWriter.WriteMethodReference(
                        () => _codeWriter.WriteType(conversionType), methodName, elementType),
                    [WriteArrayCollectionExpression]);

            void WriteArrayOrCollectionExpression()
            {
                if (canEmitCollectionExpressionDirectly)
                {
                    WriteArrayCollectionExpression();
                    return;
                }

                if (useCollectionExpression
                    && !type.IsArray
                    && (!type.IsPublic || isQueryable))
                {
                    WriteTypedArrayCollectionExpression();
                    return;
                }

                WriteArrayCreate();
            }

            if (isImmutableOrFrozen)
            {
                if (canEmitCollectionExpressionDirectly)
                {
                    WriteArrayCollectionExpression();
                }
                else
                {
                    var methodName = $"To{type.GetImmutableOrFrozenTypeName()}";
                    var conversionType = type.Assembly.GetType($"{type.Namespace}.{type.GetImmutableOrFrozenTypeName()}");

                    if (useCollectionExpression && conversionType != null)
                    {
                        WriteStaticCollectionConversion(conversionType, methodName);
                    }
                    else
                    {
                        _codeWriter.WriteMethodInvoke(() =>
                            _codeWriter.WriteMethodReference(WriteArrayCreate, methodName), []);
                    }
                }
            }
            else if (isQueryable)
            {
                if (useCollectionExpression)
                {
                    WriteStaticCollectionConversion(typeof(Queryable), "AsQueryable");
                }
                else
                {
                    _codeWriter.WriteMethodInvoke(() =>
                        _codeWriter.WriteMethodReference(WriteArrayCreate, "AsQueryable"), []);
                }
            }
            else
            {
                WriteArrayOrCollectionExpression();
            }

            return;
        }

        if (type.IsReadonlyCollection())
        {
            var typeInfo =
                new CodeCollectionTypeInfo(typeof(List<>).MakeGenericType(elementType));

            if (useCollectionExpression)
            {
                _codeWriter.WriteObjectCreate(type,
                [
                    () => _codeWriter.WriteCollectionExpressionItems(items,
                        item => WriteCollectionItem(item, context, singleLine), singleLine)
                ]);

                return;
            }

            var createAction = ResolveCollectionCreateAction(typeInfo, items, singleLine);

            _codeWriter.WriteMethodInvoke(() => _codeWriter.WriteMethodReference(createAction, "AsReadOnly"), []);

            return;
        }

        var collectionTypeInfo = new CodeCollectionTypeInfo(type);
        ResolveCollectionCreateAction(collectionTypeInfo, items, singleLine)();

        return;

        Action ResolveCollectionCreateAction(CodeTypeInfo collectionType, IEnumerable initializers, bool useSingleLine)
        {
            if (canEmitCollectionExpressionDirectly)
            {
                return () => _codeWriter.WriteCollectionExpressionItems(initializers,
                    item => WriteCollectionItem(item, context, useSingleLine), useSingleLine);
            }

            return () => _codeWriter.WriteObjectCreateAndInitializeItems(collectionType, [], initializers,
                item => WriteCollectionItem(item, context, useSingleLine), useSingleLine);
        }
    }

    private static bool IsQueryable(Type type)
    {
        var isGenericType = type.IsGenericType;

        if (!isGenericType)
        {
            return false;
        }

        if (type.GetGenericTypeDefinition() == typeof(EnumerableQuery<>))
        {
            return true;
        }

        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>));
    }

    private static IEnumerable<object> ChunkMultiDimensionalArrayItems(Array array, IEnumerable enumerable,
        bool singleLine)
    {
        var dimensions = new int[array.Rank - 1];

        for (var i = 0; i < dimensions.Length; i++)
        {
            dimensions[i] = array.GetLength(i + 1);
        }

        IEnumerable<object> result = enumerable.Cast<object>();

        for (var index = dimensions.Length - 1; index >= 0; index--)
        {
            var dimension = dimensions[index];
            var index1 = index;
            result = result.Chunk(dimension).Select(object (x) => new ArrayDimension(x,
                singleLine && index1 == dimensions.Length - 1));
        }

        return result;
    }

    private void VisitAnonymousCollection(IEnumerable enumerable, VisitContext context)
    {
        IEnumerable items = GetItems(enumerable);

        var type = enumerable.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        var typeInfo = new CodeAnonymousTypeInfo { ArrayRank = 1 };

        if (type.IsArray && ((Array)enumerable).Rank > 1 && ((Array)enumerable).Length > 0)
        {
            typeInfo.ArrayRank = ((Array)enumerable).Rank;
            items = ChunkMultiDimensionalArrayItems((Array)enumerable, items, false);
        }

        Action createAction = () => _codeWriter.WriteArrayCreateItems(typeInfo, items,
            item => WriteArrayItem(item, context), false);

        if (isImmutableOrFrozen || (enumerable is IList && !type.IsArray))
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

    private void WriteArrayItem(object item, VisitContext context, bool singleLine = false)
    {
        if (item is ArrayDimension dimension)
        {
            _codeWriter.WriteArrayDimensionItems(dimension.Items,
                nestedItem => WriteArrayItem(nestedItem, context, dimension.SingleLine), dimension.SingleLine);
            return;
        }

        WriteCollectionItem(item, context, singleLine);
    }

    private void WriteCollectionItem(object item, VisitContext context, bool singleLine = false)
    {
        if (ReferenceEquals(item, CollectionItemMarker.TooManyItems))
        {
            _codeWriter.WriteTooManyItems(_options.MaxCollectionSize, terminateLine: singleLine);
            return;
        }

        _nextDepthVisitor.Visit(item, context);
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

    private static IEnumerable<object> VisitGroupings(IEnumerable<object> objects)
    {
        var items = objects.Select(GetIGroupingValue)
            .SelectMany(g => g.Value.Cast<object>().Select(e => new { g.Key, Element = e }));

        return items;
    }

    private sealed class ArrayDimension(IEnumerable items, bool singleLine)
    {
        public IEnumerable Items { get; } = items;
        public bool SingleLine { get; } = singleLine;
    }
}
