using System;
using System.Linq;

namespace VarDump.Visitor.Descriptors.Specific;

public class ObjectMembersFilter : IObjectDescriptorMiddleware
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
    {
        var objectDescription = prev();

        return new ObjectDescription
        {
            Type = objectDescription.Type,
            ConstructorArguments = objectDescription.ConstructorArguments,
            Properties = objectDescription.Properties.Where(IsMatch),
            Fields = objectDescription.Fields.Where(IsMatch)
        };
    }

    public Func<MemberDescription, bool> Condition { get; set; } = _ => true;

    private bool IsMatch<T>(T member) where T : MemberDescription
    {
        return Condition?.Invoke(member) ?? true;
    }
}