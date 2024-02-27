using System;
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

    public ObjectDescriptionInfo Describe(object @object, Type objectType)
    {
        var firstInfo = _first.Describe(@object, objectType);
        var secondInfo = _second.Describe(@object, objectType);

        return new ObjectDescriptionInfo
        {
            Type = secondInfo.Type ?? firstInfo.Type,
            Members = firstInfo.Members.Concat(secondInfo.Members),
            ConstructorParameters = firstInfo.ConstructorParameters.Concat(secondInfo.ConstructorParameters)
        };
    }
}