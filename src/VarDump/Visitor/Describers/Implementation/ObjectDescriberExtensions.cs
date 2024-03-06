using System.Collections.Generic;
using System.Linq;

namespace VarDump.Visitor.Describers.Implementation;

internal static class ObjectDescriberExtensions
{
    public static IObjectDescriber Concat(this IObjectDescriber first, IObjectDescriber second)
    {
        return new ConcatenatedObjectDescriber(first, second);
    }

    public static IObjectDescriber ApplyMiddleware(this IObjectDescriber objectDescriber, IEnumerable<IObjectDescriberMiddleware> middleware)
    {
        var describe = objectDescriber.DescribeObject;

        foreach (var item in middleware.Reverse())
        {
            var prevDescribe = describe;
            describe = (@object, objectType) =>
            {
                return item.DescribeObject(@object, objectType, () => prevDescribe(@object, objectType));
            };
        }
        return new DelegateToObjectDescriber(describe);
    }
}
