using System.Collections.Generic;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.Descriptors;

public sealed partial class ObjectDescription : IObjectDescription
{
    public IEnumerable<ConstructorParameterDescription> ConstructorParameters { get; set; } = [];
    public IEnumerable<MemberDescription> Members { get; set; } = [];
    public CodeTypeInfo Type { get; set; }
}