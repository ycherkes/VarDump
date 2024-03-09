using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ObjectPropertiesDescriptor(BindingFlags getPropertiesBindingFlags, bool writablePropertiesOnly)
    : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        var properties = EnumerableExtensions.AsEnumerable(() => objectType
            .GetProperties(getPropertiesBindingFlags))
            .Where(p => p.CanRead &&
                        (p.CanWrite || !writablePropertiesOnly) &&
                        !ReflectionUtils.IsIndexer(p))
            .Select(p => new PropertyDescription(() => ReflectionUtils.GetValue(p, @object))
            {
                Name = p.Name,
                Type = p.PropertyType
            });

        return new ObjectDescription
        {
            Type = new CodeTypeInfo(objectType),
            Members = properties
        };
    }
}