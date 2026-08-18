using System;
using System.Reflection;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors;

public sealed record FieldDescription : MemberDescription
{
    private readonly FieldInfo _fieldInfo;
    private readonly object _instance;

    public FieldDescription(FieldInfo fieldInfo, object instance)
    {
        _fieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));
        _instance = instance;
    }

    protected override object GetValueCore() => ReflectionUtils.GetValue(_fieldInfo, _instance);

    public override ReflectionType ReflectionType => ReflectionType.Field;
    public override string Name { get; set; }
    public override Type Type { get; set; }
}
