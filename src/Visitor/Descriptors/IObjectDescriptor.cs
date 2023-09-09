using System;
using System.Collections.Generic;

namespace VarDumpExtended.Visitor.Descriptors;

public interface IObjectDescriptor
{
    IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType);
}