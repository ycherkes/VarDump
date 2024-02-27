using System.Collections.Generic;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.Descriptors;

public sealed class ObjectDescriptionInfo
{
    public IEnumerable<IReflectionDescriptor> Members { get; set; } = [];

    public IEnumerable<IReflectionDescriptor> ConstructorParameters { get; set; } = [];

    public CodeDotnetTypeReference Type { get; set; }
}