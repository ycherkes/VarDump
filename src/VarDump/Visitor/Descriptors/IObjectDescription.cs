using System.Collections.Generic;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor.Descriptors;

public interface IObjectDescription
{
    IEnumerable<ConstructorParameterDescription> ConstructorParameters { get; set; }
    IEnumerable<MemberDescription> Members { get; set; }
    CodeTypeInfo Type { get; set; }
}