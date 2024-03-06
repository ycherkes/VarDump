using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Describers.Implementation;

internal class ObjectPropertiesDescriber(BindingFlags getPropertiesBindingFlags, bool writablePropertiesOnly)
    : IObjectDescriber
{
    public ObjectDescriptor DescribeObject(object @object, Type objectType)
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

        return new ObjectDescriptor
        {
            Type = new CodeTypeInfo(objectType),
            Members = properties
        };
    }
}