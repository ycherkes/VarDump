namespace VarDump.Visitor;

public interface IRootObjectVisitor
{
    void Visit(object @object, VisitContext context);
}