﻿using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal class ObjectPropertiesDescriptor : IObjectDescriptor
{
    private readonly BindingFlags _getPropertiesBindingFlags;
    private readonly bool _writablePropertiesOnly;

    public ObjectPropertiesDescriptor(BindingFlags getPropertiesBindingFlags, bool writablePropertiesOnly)
    {
        _getPropertiesBindingFlags = getPropertiesBindingFlags;
        _writablePropertiesOnly = writablePropertiesOnly;
    }

    public ObjectDescriptionInfo Describe(object @object, Type objectType)
    {
        var properties = EnumerableExtensions.AsEnumerable(() => objectType
            .GetProperties(_getPropertiesBindingFlags))
            .Where(p => p.CanRead &&
                        (p.CanWrite || !_writablePropertiesOnly) &&
                        !ReflectionUtils.IsIndexer(p))
            .Select(p => new ReflectionDescriptor(() => ReflectionUtils.GetValue(p, @object))
            {
                Name = p.Name,
                Type = p.PropertyType,
                ReflectionType = ReflectionType.Property
            });

        var info = new ObjectDescriptionInfo
        {
            Type = new TypeReference(objectType),
            Members = properties
        };

        return info;
    }
}