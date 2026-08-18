using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ObjectPropertiesDescriptor(BindingFlags getPropertiesBindingFlags, bool writablePropertiesOnly)
    : IObjectDescriptor
{
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
        var properties = GetPropertyMetadata(objectType);

        for (var index = 0; index < properties.Count; index++)
        {
            yield return CreatePropertyDescription(@object, properties[index]);
        }
    }

    internal static PropertyDescription CreatePropertyDescription(object @object, PropertyMetadata property)
    {
        return new PropertyDescription(property.PropertyInfo, @object)
        {
            CanWrite = property.CanWrite,
            DefaultValueAttributeValue = property.DefaultValueAttributeValue,
            Name = property.Name,
            Type = property.Type
        };
    }

    internal List<PropertyMetadata> GetPropertyMetadata(Type objectType)
    {
        var properties = new List<PropertyMetadata>();

        foreach (var property in objectType.GetProperties(getPropertiesBindingFlags))
        {
            if (!property.CanRead ||
                ((property.CanWrite && MatchesAccessibility(property.SetMethod, getPropertiesBindingFlags)) || !writablePropertiesOnly) == false ||
                ReflectionUtils.IsIndexer(property))
            {
                continue;
            }

            properties.Add(new PropertyMetadata(
                property,
                property.Name,
                property.PropertyType,
                property.CanWrite,
                property.GetCustomAttribute<DefaultValueAttribute>()?.Value));
        }

        return properties;
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

    internal sealed class PropertyMetadata(
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
