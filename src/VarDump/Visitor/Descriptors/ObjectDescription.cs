using System.Collections.Generic;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.Descriptors;

public sealed partial class ObjectDescription : IObjectDescription
{
    public IEnumerable<ConstructorArgumentDescription> ConstructorArguments { get; set; } = [];
    public IEnumerable<PropertyDescription> Properties { get; set; } = [];
    public IEnumerable<FieldDescription> Fields { get; set; } = [];
    public CodeTypeInfo Type { get; set; }
}