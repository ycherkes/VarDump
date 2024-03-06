using System;

namespace VarDump.Visitor.Describers;

public interface IObjectDescriber
{
    ObjectDescriptor DescribeObject(object @object, Type objectType);
}