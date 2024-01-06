using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal class ObjectPropertiesDescriptor : IObjectDescriptor
{
    private readonly BindingFlags _getPropertiesBindingFlags;
    private readonly bool _writablePropertiesOnly;
    private readonly NullabilityInfoContext _nullabilityContext = new();

    public ObjectPropertiesDescriptor(BindingFlags getPropertiesBindingFlags, bool writablePropertiesOnly)
    {
        _getPropertiesBindingFlags = getPropertiesBindingFlags;
        _writablePropertiesOnly = writablePropertiesOnly;
    }

    public ObjectPropertiesDescriptor() : this(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, false)
    {
    }

    public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType)
    {
        var properties = EnumerableExtensions.AsEnumerable(() => objectType
            .GetProperties(_getPropertiesBindingFlags))
            .Where(p => p.CanRead &&
                        (p.CanWrite || !_writablePropertiesOnly) &&
                        !ReflectionUtils.IsIndexer(p))
            .Select(p => new ReflectionDescriptor(() => ReflectionUtils.GetValue(p, @object))
            {
                Name = p.Name,
                MemberType = p.PropertyType,
                ReflectionType = ReflectionType.Property,
                GenericTypeArguments = _nullabilityContext.Create(p).GenericTypeArguments
            });

        return properties;
    }
}