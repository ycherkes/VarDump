using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

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
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is IEnumerable;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        if (_rootObjectVisitor.IsVisited(valueDescriptor.Value))
        {
            return CodeDomUtils.GetCircularReferenceDetectedExpression();
        }

        _rootObjectVisitor.PushVisited(valueDescriptor.Value);

        try
        {
            var elementType = ReflectionUtils.GetInnerElementType(valueDescriptor.Type);

            if (elementType.IsGrouping())
            {
                return VisitGroupingCollection(valueDescriptor);
            }

            var result = valueDescriptor.Type.ContainsAnonymousType()
            ? VisitAnonymousCollection(valueDescriptor)
                : VisitSimpleCollection(valueDescriptor, elementType);

            return result;
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }

    private CodeExpression VisitGroupingCollection(IValueDescriptor valueDescriptor)
    {
        var items = VisitGroupings(((IEnumerable)valueDescriptor.Value).Cast<object>());

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

        var isLookup = valueDescriptor.Type.IsLookup();

        expr = isLookup
            ? new CodeMethodInvokeExpression(expr, "ToLookup", keyLambdaExpression, valueLambdaExpression)
            : new CodeMethodInvokeExpression(expr, "GroupBy", keyLambdaExpression, valueLambdaExpression);

        if (valueDescriptor.Type.IsArray)
        {
            expr = new CodeMethodInvokeExpression(expr, "ToArray");
        }
        else if (valueDescriptor.Value is IList)
        {
            expr = new CodeMethodInvokeExpression(expr, "ToList");
        }

        return expr;
    }

    private CodeExpression VisitSimpleCollection(IValueDescriptor valueDescriptor, Type elementType)
    {
        var items = ((IEnumerable)valueDescriptor.Value).Cast<object>().Select(o => new ValueDescriptor{Value = o, Type = o?.GetType()}).Select(_rootObjectVisitor.Visit);

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, CodeDomUtils.GetTooManyItemsExpression(_maxCollectionSize));
        }

        var isImmutableOrFrozen = valueDescriptor.Type.IsPublicImmutableOrFrozenCollection();
        var isCollection = IsCollection(valueDescriptor.Value);
        
        if (valueDescriptor.Type.IsArray || isImmutableOrFrozen || !valueDescriptor.Type.IsPublic || !isCollection)
        {
            if (valueDescriptor.Type.IsArray && ((Array)valueDescriptor.Value).Rank > 1)
            {
                items = ChunkMultiDimensionalArrayExpression((Array)valueDescriptor.Value, items);
            }

            CodeTypeReference typeReference;

            if (valueDescriptor.GenericTypeArguments.Length > 0)
            {
                var typeDescriptor = new TypeDescriptor
                {
                    Type = isImmutableOrFrozen || !valueDescriptor.Type.IsPublic
                        ? elementType.MakeArrayType()
                        : valueDescriptor.Type,
                    GenericTypeArguments = valueDescriptor.GenericTypeArguments
                };
                typeReference = new CodeTypeReference(typeDescriptor, _typeReferenceOptions);
            }
            else
            {
                var type = isImmutableOrFrozen || !valueDescriptor.Type.IsPublic
                    ? elementType.MakeArrayType()
                    : valueDescriptor.Type;
                typeReference = new CodeTypeReference(type, _typeReferenceOptions);
            }
            
            CodeExpression expr = new CodeArrayCreateExpression(
                typeReference,
                items);

            if (isImmutableOrFrozen)
                expr = new CodeMethodInvokeExpression(expr, $"To{valueDescriptor.Type.GetImmutableOrFrozenTypeName()}");

            return expr;
        }

        if (valueDescriptor.Type.IsReadonlyCollection())
        {
            CodeTypeReference typeReference;

            if (valueDescriptor.GenericTypeArguments.Length > 0)
            {
                var typeDescriptor = new TypeDescriptor
                {
                    Type = typeof(List<>).MakeGenericType(elementType),
                    GenericTypeArguments = valueDescriptor.GenericTypeArguments
                };
                typeReference = new CodeCollectionTypeReference(typeDescriptor, _typeReferenceOptions);
            }
            else
            {
                typeReference = new CodeCollectionTypeReference(typeof(List<>).MakeGenericType(elementType), _typeReferenceOptions);
            }

            var expression = new CodeObjectCreateAndInitializeExpression(
                typeReference,
                items);

            return new CodeMethodInvokeExpression(expression, "AsReadOnly");
        }

        var typeReference1 = valueDescriptor.GenericTypeArguments.Length > 0 ? new CodeCollectionTypeReference(valueDescriptor, _typeReferenceOptions) : new CodeCollectionTypeReference(valueDescriptor.Type, _typeReferenceOptions);

        var initializeExpression = new CodeObjectCreateAndInitializeExpression(
            typeReference1,
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

    private CodeExpression VisitAnonymousCollection(IValueDescriptor valueDescriptor)
    {
        var items = ((IEnumerable)valueDescriptor.Value).Cast<object>().Select(o => new ValueDescriptor { Value = o, Type = o?.GetType() }).Select(_rootObjectVisitor.Visit);

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, CodeDomUtils.GetTooManyItemsExpression(_maxCollectionSize));
        }

        var typeReference = new CodeAnonymousTypeReference { ArrayRank = 1 };

        if (valueDescriptor.Type.IsArray && ((Array)valueDescriptor.Value).Rank > 1)
        {
            typeReference.ArrayRank = ((Array)valueDescriptor.Value).Rank;
            items = ChunkMultiDimensionalArrayExpression((Array)valueDescriptor.Value, items);
        }

        CodeExpression expr = new CodeArrayCreateExpression(typeReference, items);

        var isImmutableOrFrozen = valueDescriptor.Type.IsPublicImmutableOrFrozenCollection();

        if (isImmutableOrFrozen || valueDescriptor.Value is IList && !valueDescriptor.Type.IsArray)
            expr = new CodeMethodInvokeExpression(expr, $"To{valueDescriptor.Type.GetImmutableOrFrozenTypeName()}");

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

        return items.Select(i => _rootObjectVisitor.Visit(new ValueDescriptor { Value = i, Type = i.GetType() }));
    }
}