using System.Collections.Generic;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.Descriptors;

public sealed partial class ObjectDescription : IObjectDescription
{
    public IEnumerable<IReflectionDescription> ConstructorParameters { get; set; } = [];
    public IEnumerable<IReflectionDescription> Members { get; set; } = [];
    public CodeTypeInfo Type { get; set; }
}