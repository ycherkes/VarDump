using System;

namespace VarDump.Visitor.Descriptors.Implementation;

internal class DelegateToObjectDescriptor(Func<object, Type, ObjectDescriptionInfo> describe) : IObjectDescriptor
{
    public ObjectDescriptionInfo Describe(object @object, Type objectType)
    {
        return describe(@object, objectType);
    }
}