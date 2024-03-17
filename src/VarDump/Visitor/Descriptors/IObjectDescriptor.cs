using System;

namespace VarDump.Visitor.Descriptors;

public interface IObjectDescriptor
{
    IObjectDescription GetObjectDescription(object @object, Type objectType);
}