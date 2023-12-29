using System.Collections.Generic;
using System.Reflection;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.Descriptors;

public static class Descriptor
{
    private static readonly IObjectDescriptor PropertiesDescriptor = new ObjectPropertiesDescriptor(BindingFlags.Public | BindingFlags.Instance, false);

    public static IEnumerable<IReflectionDescriptor> FromObject(object @object)
    {
        return PropertiesDescriptor.Describe(@object, @object.GetType());
    }
}