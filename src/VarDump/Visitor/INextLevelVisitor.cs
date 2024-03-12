namespace VarDump.Visitor;

public interface INextLevelVisitor
{
    void Visit(object @object, VisitContext context);
}