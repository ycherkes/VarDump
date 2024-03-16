using System;
using System.Linq;

namespace VarDump.Visitor.Descriptors.Specific
{
    public class ObjectContentReplacer : IObjectDescriptorMiddleware
    {
        public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
        {
            var objectDescription = prev();

            return new ObjectDescription
            {
                Type = objectDescription.Type,
                ConstructorArguments = objectDescription.ConstructorArguments.Select(Replace),
                Properties = objectDescription.Properties.Select(Replace),
                Fields = objectDescription.Fields.Select(Replace)
            };
        }

        public Func<ReflectionDescription, ReflectionDescription> Replacement { get; set; } = description => description;

        private T Replace<T>(T member) where T : ReflectionDescription
        {
            return (T)Replacement?.Invoke(member) ?? member;
        }
    }
}
