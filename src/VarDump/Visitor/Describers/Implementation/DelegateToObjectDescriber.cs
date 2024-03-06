using System;

namespace VarDump.Visitor.Describers.Implementation;

internal class DelegateToObjectDescriber(Func<object, Type, ObjectDescriptor> describe) : IObjectDescriber
{
    public ObjectDescriptor DescribeObject(object @object, Type objectType)
    {
        return describe(@object, objectType);
    }
}