using System;
using System.Collections.Generic;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class CachingObjectPropertiesDescriptor(ObjectPropertiesDescriptor objectPropertiesDescriptor)
    : IObjectDescriptor
{
    private readonly Dictionary<Type, List<ObjectPropertiesDescriptor.PropertyMetadata>> _propertiesByType = [];

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
        if (!_propertiesByType.TryGetValue(objectType, out var properties))
        {
            properties = objectPropertiesDescriptor.GetPropertyMetadata(objectType);
            _propertiesByType.Add(objectType, properties);
        }

        for (var index = 0; index < properties.Count; index++)
        {
            yield return ObjectPropertiesDescriptor.CreatePropertyDescription(@object, properties[index]);
        }
    }
}
