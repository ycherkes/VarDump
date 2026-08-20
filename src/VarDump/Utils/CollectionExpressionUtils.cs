using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace VarDump.Utils;

internal static class CollectionExpressionUtils
{
    private const string CollectionBuilderAttributeName =
        "System.Runtime.CompilerServices.CollectionBuilderAttribute";

    public static bool CanEmitDirectly(object collection, Type collectionType)
    {
        if (collection is not IEnumerable || collection is IDictionary)
        {
            return false;
        }

        if (collectionType.ContainsAnonymousType())
        {
            return false;
        }

        var elementType = ReflectionUtils.GetInnerElementType(collectionType);
        if (elementType.IsGrouping())
        {
            return false;
        }

        if (collectionType.IsArray)
        {
            return ((Array)collection).Rank == 1;
        }

        if (!collectionType.IsVisible || collectionType.IsReadonlyCollection())
        {
            return false;
        }

        if (HasCollectionBuilder(collectionType))
        {
            return true;
        }

        return HasParameterlessConstructor(collectionType)
               && HasApplicableAddMethod(collectionType, elementType);
    }

    private static bool HasCollectionBuilder(Type type)
    {
        return type.GetCustomAttributesData()
            .Any(attribute => attribute.AttributeType.FullName == CollectionBuilderAttributeName);
    }

    private static bool HasParameterlessConstructor(Type type)
    {
        return type.IsValueType || type.GetConstructors()
            .Any(constructor => constructor.GetParameters()
                .All(parameter => parameter.IsOptional
                                  || parameter.IsDefined(typeof(ParamArrayAttribute), inherit: false)));
    }

    private static bool HasApplicableAddMethod(Type type, Type elementType)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(method => method.Name == "Add")
            .Select(method => method.GetParameters())
            .Any(parameters => parameters.Length == 1
                               && parameters[0].ParameterType.IsAssignableFrom(elementType));
    }
}
