using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ObjectFieldsDescriptor(BindingFlags getFieldsBindingFlags, bool getBaseClassFields) : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        var fields = GetFields(objectType)
            .Select(f => new FieldDescription(() => ReflectionUtils.GetValue(f, @object))
            {
                DefaultValueAttributeValue = f.GetCustomAttribute<DefaultValueAttribute>()?.Value,
                Name = f.Name,
                Type = f.FieldType
            });

        return new ObjectDescription
        {
            Fields = fields,
            Type = objectType
        };
    }

    private IEnumerable<FieldInfo> GetFields(Type type)
    {
        if (!getBaseClassFields)
        {
            foreach (var field in type.GetFields(getFieldsBindingFlags))
            {
                yield return field;
            }

            yield break;
        }

        foreach (var currentType in GetInheritanceHierarchy(type).Reverse())
        {
            foreach (var field in currentType.GetFields(getFieldsBindingFlags))
            {
                yield return field;
            }
        }
    }

    private static IEnumerable<Type> GetInheritanceHierarchy(Type type)
    {
        for (var current = type; current != null; current = current.BaseType)
            yield return current;
    }
}
