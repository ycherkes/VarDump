namespace VarDump.Visitor;

public interface IRootVisitor
{
    void Visit(object @object, VisitContext context);
}