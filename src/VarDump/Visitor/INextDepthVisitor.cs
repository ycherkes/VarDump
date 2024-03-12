namespace VarDump.Visitor;

public interface INextDepthVisitor
{
    void Visit(object @object, VisitContext context);
}