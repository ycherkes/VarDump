using System;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.Descriptors;

public sealed partial class ObjectDescription
{
    private static readonly IObjectDescriptor PropertiesInfoProvider = new ObjectPropertiesDescriptor(BindingFlags.Public | BindingFlags.Instance, false);

    public static IObjectDescription FromObject(object @object, Type declaredType)
    {
        return FromObject(@object, new CodeTypeInfo(declaredType));
    }

    public static IObjectDescription FromObject(object @object, CodeTypeInfo declaredType)
    {
        var descriptor = PropertiesInfoProvider.GetObjectDescription(@object, @object.GetType());
        descriptor.Type = declaredType;
        return descriptor;
    }
}