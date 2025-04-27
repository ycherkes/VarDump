using System;
using System.Linq;
using System.Reflection;
using VarDump.Extensions;
using VarDump.Utils;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ObjectPropertiesDescriptor(BindingFlags getPropertiesBindingFlags, bool writablePropertiesOnly)
    : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        var properties = EnumerableExtensions.AsEnumerable(() => objectType
            .GetProperties(getPropertiesBindingFlags))
            .Where(p => p.CanRead &&
                        ((p.CanWrite && MatchesAccessibility(p.SetMethod, getPropertiesBindingFlags)) || !writablePropertiesOnly) &&
                        !ReflectionUtils.IsIndexer(p))
            .Select(p => new PropertyDescription(() => ReflectionUtils.GetValue(p, @object))
            {
                CanWrite = p.CanWrite,
                Name = p.Name,
                Type = p.PropertyType
            });

        return new ObjectDescription
        {
            Properties = properties,
            Type = objectType
        };
    }

    private static bool MatchesAccessibility(MethodInfo methodInfo, BindingFlags flags)
    {
        if (methodInfo == null)
        {
            return false;
        }

        var anyAccess = flags.HasFlag(BindingFlags.Public | BindingFlags.NonPublic);
        
        if (anyAccess)
        {
            return true;
        }

        if (flags.HasFlag(BindingFlags.Public))
        {
            return methodInfo.IsPublic;
        }
        
        return methodInfo.IsPrivate || 
               methodInfo.IsFamily ||            // protected
               methodInfo.IsAssembly ||          // internal
               methodInfo.IsFamilyOrAssembly ||  // protected internal
               methodInfo.IsFamilyAndAssembly;   // private protected
    }
}