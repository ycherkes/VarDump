using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor;

public sealed class ObjectDescriptionWriter(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter) : IObjectDescriptionWriter
{
    public void Write(IObjectDescription objectDescription, VisitContext context, DumpOptions options)
    {
        var members = ((IEnumerable<MemberDescription>)objectDescription.Fields).Concat(objectDescription.Properties);

        if (options.SortDirection != null)
        {
            members = options.SortDirection == ListSortDirection.Ascending
                ? members.OrderBy(m => m.Name)
                : members.OrderByDescending(m => m.Name);
        }

        var constructorArguments = objectDescription.ConstructorArguments
            .Select(ca => !string.IsNullOrWhiteSpace(ca.Name) && options.UseNamedArgumentsInConstructors
                ? () => codeWriter.WriteNamedArgument(ca.Name, () => nextDepthVisitor.Visit(ca.Value, context))
                : (Action)(() => nextDepthVisitor.Visit(ca.Value, context)));

        codeWriter.WriteObjectCreateAndInitializeItems(
            objectDescription.Type,
            constructorArguments,
            FilterMembers(members, options),
            member =>
            {
                var description = (MemberDescription)member;
                codeWriter.WriteMemberAssignmentStart(description.Name);
                nextDepthVisitor.Visit(description.Value, context);
            });
    }

    private static IEnumerable<MemberDescription> FilterMembers(IEnumerable<MemberDescription> members, DumpOptions options)
    {
        foreach (var member in members)
        {
            var value = member.Value;
            if (options.IgnoreNullValues && value == null)
                continue;
            if (options.IgnoreDefaultValues && ReflectionUtils.GetDefaultValue(member)?.Equals(value) == true)
                continue;

            yield return member;
        }
    }
}
