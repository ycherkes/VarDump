using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal class ObjectFieldsDescriptor(BindingFlags getFieldsBindingFlags) : IObjectDescriptor
{
    public ObjectDescriptionInfo Describe(object @object, Type objectType)
    {
        var fields = EnumerableExtensions.AsEnumerable(() => objectType
                .GetFields(getFieldsBindingFlags))
            .Select(f => new ReflectionDescriptor(() => ReflectionUtils.GetValue(f, @object))
            {
                Name = f.Name,
                Type = f.FieldType,
                ReflectionType = ReflectionType.Field
            });

        var info = new ObjectDescriptionInfo
        {
            Type = new CodeTypeInfo(objectType),
            Members = fields
        };

        return info;
    }
}