using System;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class FuncToObjectDescriptor(Func<object, Type, IObjectDescription> describe) : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        return describe(@object, objectType);
    }
}