using System;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Describers.Implementation;

internal class ObjectFieldsDescriber(BindingFlags getFieldsBindingFlags) : IObjectDescriber
{
    public ObjectDescriptor DescribeObject(object @object, Type objectType)
    {
        var fields = EnumerableExtensions.AsEnumerable(() => objectType
                .GetFields(getFieldsBindingFlags))
            .Select(f => new ReflectionDescriptor(() => ReflectionUtils.GetValue(f, @object))
            {
                Name = f.Name,
                Type = f.FieldType,
                ReflectionType = ReflectionType.Field
            });

        return new ObjectDescriptor
        {
            Type = new CodeTypeInfo(objectType),
            Members = fields
        };
    }
}