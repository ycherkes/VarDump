using System;
using System.Reflection;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors;

public sealed record PropertyDescription : MemberDescription
{
    private readonly PropertyInfo _propertyInfo;
    private readonly object _instance;

    public PropertyDescription(PropertyInfo propertyInfo, object instance)
    {
        _propertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
        _instance = instance;
    }

    protected override object GetValueCore() => ReflectionUtils.GetValue(_propertyInfo, _instance);
    
    public bool CanWrite { get; set; }
    public override string Name { get; set; }
    public override ReflectionType ReflectionType => ReflectionType.Property;
    public override Type Type { get; set; }
}
