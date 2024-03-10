using System;
using System.Linq;
using System.Reflection;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ObjectFieldsDescriptor(BindingFlags getFieldsBindingFlags) : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        var fields = EnumerableExtensions.AsEnumerable(() => objectType.GetFields(getFieldsBindingFlags))
            .Select(f => new FieldDescription(() => ReflectionUtils.GetValue(f, @object))
            {
                Name = f.Name,
                Type = f.FieldType
            });

        return new ObjectDescription
        {
            Members = fields,
            Type = objectType
        };
    }
}