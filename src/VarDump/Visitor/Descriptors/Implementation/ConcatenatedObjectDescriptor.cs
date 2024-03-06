using System;
using System.Linq;

namespace VarDump.Visitor.Descriptors.Implementation;

internal class ConcatenatedObjectDescriptor(IObjectDescriptor first, IObjectDescriptor second) : IObjectDescriptor
{
    public ObjectDescriptionInfo Describe(object @object, Type objectType)
    {
        var firstInfo = first.Describe(@object, objectType);
        var secondInfo = second.Describe(@object, objectType);

        return new ObjectDescriptionInfo
        {
            Type = secondInfo.Type ?? firstInfo.Type,
            Members = firstInfo.Members.Concat(secondInfo.Members),
            ConstructorParameters = firstInfo.ConstructorParameters.Concat(secondInfo.ConstructorParameters)
        };
    }
}