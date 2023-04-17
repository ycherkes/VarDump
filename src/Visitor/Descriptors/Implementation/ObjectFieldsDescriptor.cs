using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation
{
    internal class ObjectFieldsDescriptor : IObjectDescriptor
    {
        private readonly BindingFlags _getFieldsBindingFlags;

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
                    Type = f.FieldType,
                    ReflectionType = ReflectionType.Field
                });

            return fields;
        }
    }
}
