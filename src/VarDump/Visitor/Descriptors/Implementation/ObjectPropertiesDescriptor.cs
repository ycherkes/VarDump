using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ObjectPropertiesDescriptor(BindingFlags getPropertiesBindingFlags, bool writablePropertiesOnly)
    : IObjectDescriptor
{
    // The descriptor is scoped to a single dump operation, so this avoids global
    // type retention while still reusing metadata for repeated objects.
    private readonly Dictionary<Type, List<CachedProperty>> _propertiesByType = [];

    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        return new ObjectDescription
        {
            Properties = GetProperties(@object, objectType),
            Type = objectType
        };
    }

    private IEnumerable<PropertyDescription> GetProperties(object @object, Type objectType)
    {
        var properties = GetProperties(objectType);

        for (var index = 0; index < properties.Count; index++)
        {
            var property = properties[index];
            yield return new PropertyDescription(property.PropertyInfo, @object)
            {
                CanWrite = property.CanWrite,
                DefaultValueAttributeValue = property.DefaultValueAttributeValue,
                Name = property.Name,
                Type = property.Type
            };
        }
    }

    private List<CachedProperty> GetProperties(Type objectType)
    {
        if (_propertiesByType.TryGetValue(objectType, out var properties))
        {
            return properties;
        }

        var cachedProperties = new List<CachedProperty>();

        foreach (var property in objectType.GetProperties(getPropertiesBindingFlags))
        {
            if (!property.CanRead ||
                ((property.CanWrite && MatchesAccessibility(property.SetMethod, getPropertiesBindingFlags)) || !writablePropertiesOnly) == false ||
                ReflectionUtils.IsIndexer(property))
            {
                continue;
            }

            cachedProperties.Add(new CachedProperty(
                property,
                property.Name,
                property.PropertyType,
                property.CanWrite,
                property.GetCustomAttribute<DefaultValueAttribute>()?.Value));
        }

        _propertiesByType.Add(objectType, cachedProperties);
        return cachedProperties;
    }

    private static bool MatchesAccessibility(MethodInfo methodInfo, BindingFlags flags)
    {
        if (methodInfo == null)
        {
            return false;
        }

        var anyAccess = flags.HasFlag(BindingFlags.Public | BindingFlags.NonPublic);
        
        if (anyAccess)
        {
            return true;
        }

        if (flags.HasFlag(BindingFlags.Public))
        {
            return methodInfo.IsPublic;
        }
        
        return methodInfo.IsPrivate || 
               methodInfo.IsFamily ||            // protected
               methodInfo.IsAssembly ||          // internal
               methodInfo.IsFamilyOrAssembly ||  // protected internal
               methodInfo.IsFamilyAndAssembly;   // private protected
    }

    private sealed class CachedProperty(
        PropertyInfo propertyInfo,
        string name,
        Type type,
        bool canWrite,
        object defaultValueAttributeValue)
    {
        public PropertyInfo PropertyInfo { get; } = propertyInfo;
        public string Name { get; } = name;
        public Type Type { get; } = type;
        public bool CanWrite { get; } = canWrite;
        public object DefaultValueAttributeValue { get; } = defaultValueAttributeValue;
    }
}
