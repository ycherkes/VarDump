using System;

namespace VarDump.Visitor.Descriptors;

public interface IObjectDescriptor
{
    ObjectDescriptionInfo Describe(object @object, Type objectType);
}