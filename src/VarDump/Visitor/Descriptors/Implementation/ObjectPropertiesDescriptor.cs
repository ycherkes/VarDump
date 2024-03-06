using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal class ObjectPropertiesDescriptor(BindingFlags getPropertiesBindingFlags, bool writablePropertiesOnly)
    : IObjectDescriptor
{
    public ObjectDescriptionInfo Describe(object @object, Type objectType)
    {
        var properties = EnumerableExtensions.AsEnumerable(() => objectType
            .GetProperties(getPropertiesBindingFlags))
            .Where(p => p.CanRead &&
                        (p.CanWrite || !writablePropertiesOnly) &&
                        !ReflectionUtils.IsIndexer(p))
            .Select(p => new ReflectionDescriptor(() => ReflectionUtils.GetValue(p, @object))
            {
                Name = p.Name,
                Type = p.PropertyType,
                ReflectionType = ReflectionType.Property
            });

        var info = new ObjectDescriptionInfo
        {
            Type = new CodeTypeInfo(objectType),
            Members = properties
        };

        return info;
    }
}