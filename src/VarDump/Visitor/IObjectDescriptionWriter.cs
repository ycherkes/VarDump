using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor;

public interface IObjectDescriptionWriter
{
    void Write(IObjectDescription objectDescription, VisitContext context, DumpOptions options);
}