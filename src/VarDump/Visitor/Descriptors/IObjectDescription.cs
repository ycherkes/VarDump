using System.Collections.Generic;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.Descriptors;

public interface IObjectDescription
{
    IEnumerable<IReflectionDescription> ConstructorParameters { get; set; }
    IEnumerable<IReflectionDescription> Members { get; set; }
    CodeTypeInfo Type { get; set; }
}