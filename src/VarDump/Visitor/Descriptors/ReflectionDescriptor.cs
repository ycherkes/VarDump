using System;

namespace VarDump.Visitor.Descriptors;

public class ReflectionDescriptor : ValueDescriptor, IReflectionDescriptor
{
    private readonly Func<object> _getValueFunc;
    private object _value;
    private bool _isValueInitialized;

    public ReflectionDescriptor(object value)
    {
        _value = value;
        _isValueInitialized = true;
    }

    public ReflectionDescriptor(Func<object> getValueFunc)
    {
        _getValueFunc = getValueFunc;
        _isValueInitialized = false;
    }

    public Type MemberType { get; set; }
    public ReflectionType ReflectionType { get; set; }
    public string Name { get; set; }

    public override object Value
    {
        get
        {
            if (_isValueInitialized)
            {
                return _value;
            }

            _value = _getValueFunc();
            _isValueInitialized = true;
            return _value;
        }
    }

    public override Type Type => Value?.GetType();
}