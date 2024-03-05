using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class CollectionVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;
    private readonly int _maxCollectionSize;

    public CollectionVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter, int maxCollectionSize)
    {
        if (maxCollectionSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCollectionSize));
        }

        _maxCollectionSize = maxCollectionSize;
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
    }

    public string Id => "Collection";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is IEnumerable;
    }

    public void Visit(object obj, Type collectionType)
    {
        IEnumerable collection = (IEnumerable)obj;
        if (_rootObjectVisitor.IsVisited(collection))
        {
            _codeWriter.WriteCircularReferenceDetected();
            return;
        }

        _rootObjectVisitor.PushVisited(collection);

        try
        {
            var elementType = ReflectionUtils.GetInnerElementType(collectionType);

            if (elementType.IsGrouping())
            {
                VisitGroupingCollection(collection);
                return;
            }

            if (collectionType.ContainsAnonymousType())
            {
                VisitAnonymousCollection(collection);
                return;
            }

            VisitSimpleCollection(collection, elementType);
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }

    private void VisitGroupingCollection(IEnumerable collection)
    {
        var type = collection.GetType();

        var items = VisitGroupings(collection.Cast<object>());

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, () => _codeWriter.WriteTooManyItems(_maxCollectionSize));
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

        void WriteArrayCreate() => _codeWriter.WriteArrayCreate(new CodeAnonymousTypeInfo { ArrayRank = 1 }, items);
        void WriteVariableReference() => _codeWriter.WriteVariableReference("grp");
        void WriteKeyLambdaPropertyExpression() => _codeWriter.WritePropertyReference("Key", WriteVariableReference);
        void WriteKeyLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteKeyLambdaPropertyExpression, [WriteVariableReference]);
        void WriteValueLambdaPropertyExpression() => _codeWriter.WritePropertyReference("Element", WriteVariableReference);
        void WriteValueLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteValueLambdaPropertyExpression, [WriteVariableReference]);
    }

    private void VisitSimpleCollection(IEnumerable enumerable, Type elementType)
    {
        var items = enumerable.Cast<object>().Select(item => (Action)(() => _rootObjectVisitor.Visit(item)));

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, () => _codeWriter.WriteTooManyItems(_maxCollectionSize));
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

            var arrayType = isImmutableOrFrozen || !type.IsPublic ? elementType.MakeArrayType() : type;

            void WriteArrayCreate() => _codeWriter.WriteArrayCreate(arrayType, items);

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

        _codeWriter.WriteObjectCreateAndInitialize(
            new CodeCollectionTypeInfo(type), [], items);
    }

    private IEnumerable<Action> ChunkMultiDimensionalArrayExpression(Array array, IEnumerable<Action> enumerable)
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
            result = result.Chunk(dimension).Select(x => (Action) (()=> _codeWriter.WriteArrayDimension(x)));
        }

        return result;
    }

    private void VisitAnonymousCollection(IEnumerable enumerable)
    {
        var items = enumerable.Cast<object>().Select(item => (Action)(() => _rootObjectVisitor.Visit(item)));

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, () => _codeWriter.WriteTooManyItems(_maxCollectionSize));
        }

        var type = enumerable.GetType();

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        var typeInfo = new CodeAnonymousTypeInfo { ArrayRank = 1 };

        if (type.IsArray && ((Array)enumerable).Rank > 1)
        {
            typeInfo.ArrayRank = ((Array)enumerable).Rank;
            items = ChunkMultiDimensionalArrayExpression((Array)enumerable, items);
        }

        Action createAction = () => _codeWriter.WriteArrayCreate(typeInfo, items);

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

    private IEnumerable<Action> VisitGroupings(IEnumerable<object> objects)
    {
        var items = objects.Select(GetIGroupingValue)
            .SelectMany(g => g.Value.Cast<object>().Select(e => new { g.Key, Element = e }));

        return items.Select(item =>(Action)(() => _rootObjectVisitor.Visit(item)));
    }
}