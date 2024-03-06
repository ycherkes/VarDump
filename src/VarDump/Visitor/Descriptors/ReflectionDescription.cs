using System;

namespace VarDump.Visitor.Descriptors;

public sealed class ReflectionDescription : IReflectionDescription
{
    private readonly Func<object> _getValueFunc;
    private object _value;
    private bool _isValueInitialized;

    public ReflectionDescription(object value)
    {
        _value = value;
        _isValueInitialized = true;
    }

    public ReflectionDescription(Func<object> getValueFunc)
    {
        _getValueFunc = getValueFunc;
        _isValueInitialized = false;
    }

    public ReflectionType ReflectionType { get; set; }
    public string Name { get; set; }

    public Type Type { get; set; }

    public object Value
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
}