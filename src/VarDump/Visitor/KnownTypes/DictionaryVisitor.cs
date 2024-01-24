using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.KnownTypes;

internal sealed class DictionaryVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly int _maxCollectionSize;
    private readonly IObjectDescriptor _descriptor;

    public DictionaryVisitor(DumpOptions options, IObjectVisitor rootObjectVisitor)
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
        _descriptor = new ObjectPropertiesDescriptor();
    }

    public string Id => "Dictionary";
    public bool IsSuitableFor(IValueDescriptor valueDescriptor)
    {
        return valueDescriptor.Value is IDictionary;
    }

    public CodeExpression Visit(IValueDescriptor valueDescriptor)
    {
        IDictionary dict = (IDictionary)valueDescriptor.Value;
        if (_rootObjectVisitor.IsVisited(dict))
        {
            return CodeDomUtils.GetCircularReferenceDetectedExpression();
        }

        _rootObjectVisitor.PushVisited(dict);

        try
        {
            var valuesType = dict.Values.GetType();
            var keysType = dict.Keys.GetType();

            var result = keysType.ContainsAnonymousType() ||
                         valuesType.ContainsAnonymousType()
                ? VisitAnonymousDictionary(dict)
                : VisitSimpleDictionary(valueDescriptor, dict);

            return result;
        }
        finally
        {
            _rootObjectVisitor.PopVisited();
        }
    }

    private CodeExpression VisitSimpleDictionary(ITypeDescriptor valueDescriptor, IDictionary dict)
    {
        var items = dict.Cast<object>().Select(VisitKeyValuePairGenerateImplicitly);

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, CodeDomUtils.GetTooManyItemsExpression(_maxCollectionSize));
        }

        var type = dict.GetType();
        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        if (isImmutableOrFrozen)
        {
            var keyType = ReflectionUtils.GetInnerElementType(dict.Keys.GetType());
            var valueType = ReflectionUtils.GetInnerElementType(dict.Values.GetType());

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            CodeExpression dictionaryCreateExpression;

            if (valueDescriptor.GenericTypeArguments.Length > 0)
            {
                var typeDescriptor = new TypeDescriptor
                {
                    Type = dictionaryType,
                    GenericTypeArguments = valueDescriptor.GenericTypeArguments
                };

                dictionaryCreateExpression =
                    new CodeObjectCreateAndInitializeExpression(
                        new CodeCollectionTypeReference(typeDescriptor, _typeReferenceOptions), items);
            }
            else
            {
                dictionaryCreateExpression =
                    new CodeObjectCreateAndInitializeExpression(
                        new CodeCollectionTypeReference(dictionaryType, _typeReferenceOptions), items);
            }

            dictionaryCreateExpression = new CodeMethodInvokeExpression(dictionaryCreateExpression, $"To{valueDescriptor.Type.GetImmutableOrFrozenTypeName()}");

            return dictionaryCreateExpression;
        }
        else
        {
            CodeTypeReference collectionType = valueDescriptor.GenericTypeArguments.Length > 0
                ? new CodeCollectionTypeReference(valueDescriptor, _typeReferenceOptions)
                : new CodeCollectionTypeReference(valueDescriptor.Type, _typeReferenceOptions);

            CodeExpression dictionaryCreateExpression = new CodeObjectCreateAndInitializeExpression(collectionType, items);
            return dictionaryCreateExpression;
        }
    }

    private CodeExpression VisitAnonymousDictionary(IEnumerable dictionary)
    {
        const string keyName = "Key";
        const string valueName = "Value";
        var items = dictionary.Cast<object>().Select(o => VisitKeyValuePairGenerateAnonymousType(o, keyName, valueName));

        if (_maxCollectionSize < int.MaxValue)
        {
            items = items.Take(_maxCollectionSize + 1).Replace(_maxCollectionSize, CodeDomUtils.GetTooManyItemsExpression(_maxCollectionSize));
        }
        
        var type = dictionary.GetType();

        CodeExpression expr = new CodeArrayCreateExpression(new CodeAnonymousTypeReference { ArrayRank = 1 }, items);

        var variableReferenceExpression = new CodeVariableReferenceExpression("kvp");
        var keyLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, keyName), variableReferenceExpression);
        var valueLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, valueName), variableReferenceExpression);

        var isImmutableOrFrozen = type.IsPublicImmutableOrFrozenCollection();

        expr = isImmutableOrFrozen
            ? new CodeMethodInvokeExpression(expr, $"To{type.GetImmutableOrFrozenTypeName()}", keyLambdaExpression, valueLambdaExpression)
            : new CodeMethodInvokeExpression(expr, "ToDictionary", keyLambdaExpression, valueLambdaExpression);

        return expr;
    }

    private CodeExpression VisitKeyValuePairGenerateImplicitly(object o)
    {
        var objectType = o.GetType();
        var propertyValues = _descriptor.Describe(o, objectType).Select(rd => _rootObjectVisitor.Visit(rd)).Take(2).ToArray();
        return new CodeImplicitKeyValuePairCreateExpression(propertyValues.First(), propertyValues.Last());
    }

    private CodeExpression VisitKeyValuePairGenerateAnonymousType(object o, string keyName, string valueName)
    {
        var objectType = o.GetType();
        var propertyValues = _descriptor.Describe(o, objectType).Select(rd => _rootObjectVisitor.Visit(rd)).ToArray();
        var result = new CodeObjectCreateAndInitializeExpression(new CodeAnonymousTypeReference())
        {
            InitializeExpressions = new CodeExpressionContainer(new[]
            {
                (CodeExpression)new CodeAssignExpression(new CodePropertyReferenceExpression(null, keyName), propertyValues[0]),
                new CodeAssignExpression(new CodePropertyReferenceExpression(null, valueName), propertyValues[1])
            })
        };

        return result;
    }
}