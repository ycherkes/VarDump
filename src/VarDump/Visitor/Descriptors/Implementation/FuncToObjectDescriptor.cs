using System;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class FuncToObjectDescriptor(Func<object, Type, IObjectDescription> getObjectDescription) : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        return getObjectDescription(@object, objectType);
    }
}