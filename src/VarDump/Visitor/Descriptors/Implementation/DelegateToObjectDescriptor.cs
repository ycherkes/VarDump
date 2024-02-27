using System;

namespace VarDump.Visitor.Descriptors.Implementation;

internal class DelegateToObjectDescriptor : IObjectDescriptor
{
    private readonly Func<object, Type, ObjectDescriptionInfo> _describe;

    public DelegateToObjectDescriptor(Func<object, Type, ObjectDescriptionInfo> describe)
    {
        _describe = describe;
    }

    public ObjectDescriptionInfo Describe(object @object, Type objectType)
    {
        return _describe(@object, objectType);
    }
}