using System;
using System.Collections.Generic;
using System.Linq;

namespace VarDump.Visitor.Descriptors.Implementation;

internal class ConcatenatedObjectDescriptor : IObjectDescriptor
{
    private readonly IObjectDescriptor _first;
    private readonly IObjectDescriptor _second;

    public ConcatenatedObjectDescriptor(IObjectDescriptor first, IObjectDescriptor second)
    {
        _first = first;
        _second = second;
    }

    public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType)
    {
        return _first.Describe(@object, objectType).Concat(_second.Describe(@object, objectType));
    }
}