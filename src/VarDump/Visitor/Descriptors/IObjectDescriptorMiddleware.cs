using System;

namespace VarDump.Visitor.Descriptors;

public interface IObjectDescriptorMiddleware
{
    IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev);
}