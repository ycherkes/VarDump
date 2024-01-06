﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal class ObjectFieldsDescriptor : IObjectDescriptor
{
    private readonly BindingFlags _getFieldsBindingFlags;
    private readonly NullabilityInfoContext _nullabilityContext = new();

    public ObjectFieldsDescriptor() : this(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
    {
    }

    public ObjectFieldsDescriptor(BindingFlags getFieldsBindingFlags)
    {
        _getFieldsBindingFlags = getFieldsBindingFlags;
    }

    public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType)
    {
        var fields = EnumerableExtensions.AsEnumerable(() => objectType
                .GetFields(_getFieldsBindingFlags))
            .Select(f => new ReflectionDescriptor(() => ReflectionUtils.GetValue(f, @object))
            {
                Name = f.Name,
                MemberType = f.FieldType,
                ReflectionType = ReflectionType.Field,
                GenericTypeArguments = _nullabilityContext.Create(f).GenericTypeArguments
            });

        return fields;
    }
}