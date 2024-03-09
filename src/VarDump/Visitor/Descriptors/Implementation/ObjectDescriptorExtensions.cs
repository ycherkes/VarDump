using System.Collections.Generic;
using System.Linq;

namespace VarDump.Visitor.Descriptors.Implementation;

internal static class ObjectDescriptorExtensions
{
    public static IObjectDescriptor Concat(this IObjectDescriptor first, IObjectDescriptor second)
    {
        return new ConcatenatedObjectDescriptor(first, second);
    }

    public static IObjectDescriptor ApplyMiddleware(this IObjectDescriptor objectDescriptor, IEnumerable<IObjectDescriptorMiddleware> middleware)
    {
        var descriptionFunc = objectDescriptor.GetObjectDescription;

        foreach (var item in middleware.Reverse())
        {
            var prevDescriptionFunc = descriptionFunc;
            descriptionFunc = (@object, objectType) =>
            {
                return item.GetObjectDescription(@object, objectType, () => prevDescriptionFunc(@object, objectType));
            };
        }
        return new FuncToObjectDescriptor(descriptionFunc);
    }
}
