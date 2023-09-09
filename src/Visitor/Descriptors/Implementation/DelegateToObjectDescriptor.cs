using System;
using System.Collections.Generic;

namespace VarDumpExtended.Visitor.Descriptors.Implementation;

internal class DelegateToObjectDescriptor : IObjectDescriptor
{
    private readonly Func<object, Type, IEnumerable<IReflectionDescriptor>> _describe;

    public DelegateToObjectDescriptor(Func<object, Type, IEnumerable<IReflectionDescriptor>> describe)
    {
        _describe = describe;
    }

    public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType)
    {
        return _describe(@object, objectType);
    }
}