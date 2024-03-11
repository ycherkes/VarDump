using System.Collections.Generic;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.Descriptors;

public interface IObjectDescription
{
    IEnumerable<ConstructorArgumentDescription> ConstructorArguments { get; set; }
    IEnumerable<PropertyDescription> Properties { get; set; }
    IEnumerable<FieldDescription> Fields { get; set; }
    CodeTypeInfo Type { get; set; }
}