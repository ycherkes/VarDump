using System;
using System.Linq;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ConcatenatedObjectDescriptor(IObjectDescriptor first, IObjectDescriptor second) : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        var firstInfo = first.GetObjectDescription(@object, objectType);
        var secondInfo = second.GetObjectDescription(@object, objectType);

        return new ObjectDescription
        {
            ConstructorArguments = firstInfo.ConstructorArguments.Concat(secondInfo.ConstructorArguments),
            Properties = firstInfo.Properties.Concat(secondInfo.Properties),
            Fields = firstInfo.Fields.Concat(secondInfo.Fields),
            Type = secondInfo.Type ?? firstInfo.Type
        };
    }
}