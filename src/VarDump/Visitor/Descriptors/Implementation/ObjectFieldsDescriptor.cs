using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ObjectFieldsDescriptor(DumpOptions dumpOptions) : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        var fields = GetFields(objectType, dumpOptions)
            .Select(f => new FieldDescription(() => ReflectionUtils.GetValue(f, @object))
            {
                Name = f.Name,
                Type = f.FieldType
            });

        return new ObjectDescription
        {
            Fields = fields,
            Type = objectType
        };
    }

    private static IEnumerable<FieldInfo> GetFields(Type type, DumpOptions dumpOptions)
    {
        if (!dumpOptions.GetBaseClassFields)
        {
            foreach (var field in type.GetFields(dumpOptions.GetFieldsBindingFlags!.Value))
            {
                yield return field;
            }

            yield break;
        }

        foreach (var currentType in GetInheritanceHierarchy(type).Reverse())
        {
            foreach (var field in currentType.GetFields(dumpOptions.GetFieldsBindingFlags!.Value))
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