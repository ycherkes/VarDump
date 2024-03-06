using System;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Visitor.Describers.Implementation;

namespace VarDump.Visitor.Describers;

public sealed partial class ObjectDescriptor
{
    private static readonly IObjectDescriber PropertiesDescriber = new ObjectPropertiesDescriber(BindingFlags.Public | BindingFlags.Instance, false);

    public static ObjectDescriptor FromObject(object @object, Type declaredType)
    {
        return FromObject(@object, new CodeTypeInfo(declaredType));
    }

    public static ObjectDescriptor FromObject(object @object, CodeTypeInfo declaredType)
    {
        var descriptor = PropertiesDescriber.DescribeObject(@object, @object.GetType());
        descriptor.Type = declaredType;
        return descriptor;
    }
}