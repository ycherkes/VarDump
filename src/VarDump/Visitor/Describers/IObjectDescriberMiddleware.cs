using System;

namespace VarDump.Visitor.Describers;

public interface IObjectDescriberMiddleware
{
    ObjectDescriptor DescribeObject(object @object, Type objectType, Func<ObjectDescriptor> prev);
}