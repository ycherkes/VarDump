using System;
using System.Collections.Generic;

namespace VarDump.Visitor.Descriptors;

public interface IObjectDescriptorMiddleware
{
    IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType, Func<IEnumerable<IReflectionDescriptor>> prev);
}