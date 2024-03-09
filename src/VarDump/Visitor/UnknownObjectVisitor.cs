using System;
using System.ComponentModel;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor;

internal sealed class UnknownObjectVisitor(
    ICodeWriter codeWriter,
    IRootObjectVisitor rootObjectVisitor,
    IObjectDescriptor objectDescriptor,
    DumpOptions dumpOptions)
{
    public void Visit(object o, Type objectType, VisitContext context)
    {
        if (context.IsVisited(o))
        {
            codeWriter.WriteCircularReferenceDetected();
            return;
        }

        context.PushVisited(o);

        try
        {
            var objectDescription = objectDescriptor.GetObjectDescription(o, objectType);

            var members = objectDescription.Members;

            if (dumpOptions.SortDirection != null)
            {
                members = dumpOptions.SortDirection == ListSortDirection.Ascending
                    ? members.OrderBy(x => x.Name)
                    : members.OrderByDescending(x => x.Name);
            }

            var constructorArguments = objectDescription.ConstructorParameters
                .Select(cp => !string.IsNullOrWhiteSpace(cp.Name) && dumpOptions.UseNamedArguments
                    ? () => codeWriter.WriteNamedArgument(cp.Name, () => rootObjectVisitor.Visit(cp.Value, context))
                    : (Action)(() => rootObjectVisitor.Visit(cp.Value, context)));

            var initializers = members
                .Where(pv => !dumpOptions.ExcludeTypes.Contains(pv.Type.FullName) &&
                             (!dumpOptions.IgnoreNullValues || dumpOptions.IgnoreNullValues && pv.Value != null) &&
                             (!dumpOptions.IgnoreDefaultValues || !pv.Type.IsValueType || dumpOptions.IgnoreDefaultValues &&
                                 ReflectionUtils.GetDefaultValue(pv.Type)?.Equals(pv.Value) != true))
                .Select(pv => (Action)(() => codeWriter.WriteAssign(
                    () => codeWriter.WritePropertyReference(pv.Name, null),
                    () => rootObjectVisitor.Visit(pv.Value, context))));

            codeWriter.WriteObjectCreateAndInitialize
            (
                objectDescription.Type ?? objectType,
                constructorArguments,
                initializers
            );
        }
        finally
        {
            context.PopVisited();
        }
    }
}