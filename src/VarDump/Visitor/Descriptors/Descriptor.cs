using System;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.Descriptors;

public static class Descriptor
{
    private static readonly IObjectDescriptor PropertiesDescriptor = new ObjectPropertiesDescriptor(BindingFlags.Public | BindingFlags.Instance, false);

    public static ObjectDescriptionInfo FromObject(object @object, Type declaredType)
    {
        return FromObject(@object, new TypeReference(declaredType));
    }

    public static ObjectDescriptionInfo FromObject(object @object, TypeReference declaredType)
    {
        var info = PropertiesDescriptor.Describe(@object, @object.GetType());
        info.Type = declaredType;
        return info;
    }
}