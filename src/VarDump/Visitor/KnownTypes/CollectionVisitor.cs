using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class CollectionVisitor : IKnownObjectVisitor
{
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly int _maxCollectionSize;

    public CollectionVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
    {
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;

        if (options.MaxCollectionSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxCollectionSize));
        }

        _maxCollectionSize = options.MaxCollectionSize;

        _rootObjectVisitor = rootObjectVisitor;
    }

    public string Id => "Collection";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IEnumerable;
    }

    public CodeExpression Visit(object obj, Type collectionType)
    {
        IEnumerable collection = (IEnumerable)obj;
        if (_rootObjectVisitor.IsVisited(collection))
        {
            return CodeDomUtils.GetCircularReferenceDetectedExpression();
        }

        _rootObjectVisitor.PushVisited(collection);

        try
        {
            var elementType = ReflectionUtils.GetInnerElementType(collectionType);

            if (elementType.IsGrouping())
            {
                return VisitGroupingCollection(collection);
            }

            var result = collectionType.ContainsAnonymousType()
                ? VisitAnonymousCollection(collection)
                : VisitSimpleCollection(collection, elementType);

            return result;
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }

    private CodeExpression VisitGroupingCollection(IEnumerable collection)
    {
        var type = collection.GetType();

        var items = VisitGroupings(collection.Cast<object>());

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, CodeDomUtils.GetTooManyItemsExpression(_maxCollectionSize));
        }

        CodeExpression expr = new CodeArrayCreateExpression(new CodeAnonymousTypeReference { ArrayRank = 1 }, items);

        var variableReferenceExpression = new CodeVariableReferenceExpression("grp");
        var keyLambdaExpression =
            new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Key"),
                variableReferenceExpression);
        var valueLambdaExpression =
            new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Element"),
                variableReferenceExpression);

        var isLookup = type.IsLookup();

        expr = isLookup
            ? new CodeMethodInvokeExpression(expr, "ToLookup", keyLambdaExpression, valueLambdaExpression)
            : new CodeMethodInvokeExpression(expr, "GroupBy", keyLambdaExpression, valueLambdaExpression);

        if (type.IsArray)
        {
            expr = new CodeMethodInvokeExpression(expr, "ToArray");
        }
        else if (collection is IList)
        {
            expr = new CodeMethodInvokeExpression(expr, "ToList");
        }

        return expr;
    }

    private CodeExpression VisitSimpleCollection(IEnumerable enumerable, Type elementType)
    {
        var items = enumerable.Cast<object>().Select(_rootObjectVisitor.Visit);

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, CodeDomUtils.GetTooManyItemsExpression(_maxCollectionSize));
        }

        var type = enumerable.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();
        var isCollection = IsCollection(enumerable);

        if (type.IsArray || isImmutableOrFrozen || !type.IsPublic || !isCollection)
        {
            if (type.IsArray && ((Array)enumerable).Rank > 1)
            {
                items = ChunkMultiDimensionalArrayExpression((Array)enumerable, items);
            }

            CodeExpression expr = new CodeArrayCreateExpression(
                new CodeTypeReference(isImmutableOrFrozen || !type.IsPublic ? elementType.MakeArrayType() : type,
                    _typeReferenceOptions),
                items);

            if (isImmutableOrFrozen)
                expr = new CodeMethodInvokeExpression(expr, $"To{type.GetImmutableOrFrozenTypeName()}");

            return expr;
        }

        if (type.IsReadonlyCollection())
        {
            var typeReference =
                new CodeCollectionTypeReference(typeof(List<>).MakeGenericType(elementType), _typeReferenceOptions);

            var expression = new CodeObjectCreateAndInitializeExpression(
                typeReference,
                items);

            return new CodeMethodInvokeExpression(expression, "AsReadOnly");
        }

        var initializeExpression = new CodeObjectCreateAndInitializeExpression(
            new CodeCollectionTypeReference(type, _typeReferenceOptions),
            items);

        return initializeExpression;
    }

    private static IEnumerable<CodeExpression> ChunkMultiDimensionalArrayExpression(Array array,
        IEnumerable<CodeExpression> enumerable)
    {
        var dimensions = new int[array.Rank - 1];

        for (var i = 0; i < dimensions.Length; i++)
        {
            dimensions[i] = array.GetLength(i + 1);
        }

        IEnumerable<CodeExpression> result = enumerable;

        for (var index = dimensions.Length - 1; index >= 0; index--)
        {
            var dimension = dimensions[index];
            result = result.Chunk(dimension).Select(x => new CodeArrayDimensionExpression(x));
        }

        return result;
    }

    private CodeExpression VisitAnonymousCollection(IEnumerable enumerable)
    {
        var items = enumerable.Cast<object>().Select(_rootObjectVisitor.Visit);

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, CodeDomUtils.GetTooManyItemsExpression(_maxCollectionSize));
        }

        var type = enumerable.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        var typeReference = new CodeAnonymousTypeReference { ArrayRank = 1 };

        if (type.IsArray && ((Array)enumerable).Rank > 1)
        {
            typeReference.ArrayRank = ((Array)enumerable).Rank;
            items = ChunkMultiDimensionalArrayExpression((Array)enumerable, items);
        }

        CodeExpression expr = new CodeArrayCreateExpression(typeReference, items);

        if (isImmutableOrFrozen || enumerable is IList && !type.IsArray)
            expr = new CodeMethodInvokeExpression(expr, $"To{type.GetImmutableOrFrozenTypeName()}");

        return expr;
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

    private IEnumerable<CodeExpression> VisitGroupings(IEnumerable<object> objects)
    {
        var items = objects.Select(GetIGroupingValue)
            .SelectMany(g => g.Value.Cast<object>().Select(e => new { g.Key, Element = e }));

        return items.Select(_rootObjectVisitor.Visit);
    }
}