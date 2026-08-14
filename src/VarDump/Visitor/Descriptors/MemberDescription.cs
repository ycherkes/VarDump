namespace VarDump.Visitor.Descriptors;

public abstract record MemberDescription : ReflectionDescription
{
    private bool _isValueInitialized;

    protected abstract object GetValueCore();

    public override object Value
    {
        get
        {
            if (_isValueInitialized)
            {
                return field;
            }

            field = GetValueCore();
            _isValueInitialized = true;
            return field;
        }

        set
        {
            _isValueInitialized = true;
            field = value;
        }
    }

    public object DefaultValueAttributeValue { get; set; }
}
