using System;
using System.Linq;

namespace VarDump.Visitor.Describers.Implementation;

internal class ConcatenatedObjectDescriber(IObjectDescriber first, IObjectDescriber second) : IObjectDescriber
{
    public ObjectDescriptor DescribeObject(object @object, Type objectType)
    {
        var firstInfo = first.DescribeObject(@object, objectType);
        var secondInfo = second.DescribeObject(@object, objectType);

        return new ObjectDescriptor
        {
            Type = secondInfo.Type ?? firstInfo.Type,
            Members = firstInfo.Members.Concat(secondInfo.Members),
            ConstructorParameters = firstInfo.ConstructorParameters.Concat(secondInfo.ConstructorParameters)
        };
    }
}