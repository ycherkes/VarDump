using System.Collections.Generic;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.Describers;

public sealed partial class ObjectDescriptor
{
    public IEnumerable<IReflectionDescriptor> ConstructorParameters { get; set; } = [];
    public IEnumerable<IReflectionDescriptor> Members { get; set; } = [];
    public CodeTypeInfo Type { get; set; }
}