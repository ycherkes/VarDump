using System;

namespace VarDump.Visitor.Descriptors;

public abstract record MemberDescription: ReflectionDescription
{
    private readonly Func<object> _getValueFunc;
    private object _value;
    private bool _isValueInitialized;

    protected MemberDescription(object value)
    {
        _value = value;
        _isValueInitialized = true;
    }

    protected MemberDescription(Func<object> getValueFunc)
    {
        _getValueFunc = getValueFunc;
        _isValueInitialized = false;
    }

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

        set
        {
            _isValueInitialized = true;
            _value = value;
        }
    }
}