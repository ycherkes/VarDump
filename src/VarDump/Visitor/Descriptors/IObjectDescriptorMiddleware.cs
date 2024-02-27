using System;

namespace VarDump.Visitor.Descriptors;

public interface IObjectDescriptorMiddleware
{
    ObjectDescriptionInfo Describe(object @object, Type objectType, Func<ObjectDescriptionInfo> prev);
}