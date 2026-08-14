using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ObjectFieldsDescriptor(BindingFlags getFieldsBindingFlags, bool getBaseClassFields) : IObjectDescriptor
{
    // Values remain per-object and lazy; only immutable field metadata is shared.
    private readonly Dictionary<Type, List<CachedField>> _fieldsByType = [];

    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        return new ObjectDescription
        {
            Fields = GetFields(@object, objectType),
            Type = objectType
        };
    }

    private IEnumerable<FieldDescription> GetFields(object @object, Type objectType)
    {
        foreach (var field in GetFields(objectType))
        {
            yield return new FieldDescription(field.FieldInfo, @object)
            {
                DefaultValueAttributeValue = field.DefaultValueAttributeValue,
                Name = field.Name,
                Type = field.Type
            };
        }
    }

    private List<CachedField> GetFields(Type objectType)
    {
        if (_fieldsByType.TryGetValue(objectType, out var cachedFields))
            return cachedFields;

        cachedFields = [];

        if (!getBaseClassFields)
        {
            AddFields(objectType, getFieldsBindingFlags, cachedFields);
        }
        else
        {
            var hierarchy = new Stack<Type>();

            for (var currentType = objectType; currentType is not null; currentType = currentType.BaseType)
                hierarchy.Push(currentType);

            while (hierarchy.Count > 0)
                AddFields(hierarchy.Pop(), getFieldsBindingFlags | BindingFlags.DeclaredOnly, cachedFields);
        }

        _fieldsByType.Add(objectType, cachedFields);

        return cachedFields;
    }

    private static void AddFields(Type type, BindingFlags bindingFlags, List<CachedField> cachedFields)
    {
        var fields = type.GetFields(bindingFlags);

        for (var index = 0; index < fields.Length; index++)
        {
            var field = fields[index];
            cachedFields.Add(new CachedField(
                field,    
                field.Name,
                field.FieldType,
                field.GetCustomAttribute<DefaultValueAttribute>()?.Value));
        }
    }

    private sealed class CachedField(
        FieldInfo fieldInfo,
        string name,
        Type type,
        object defaultValueAttributeValue)
    {
        public FieldInfo FieldInfo { get; } = fieldInfo;
        public string Name { get; } = name;
        public Type Type { get; } = type;
        public object DefaultValueAttributeValue { get; } = defaultValueAttributeValue;
    }
}
