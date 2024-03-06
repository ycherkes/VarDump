using System;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class DelegateToObjectDescriptor(Func<object, Type, IObjectDescription> describe) : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        return describe(@object, objectType);
    }
}