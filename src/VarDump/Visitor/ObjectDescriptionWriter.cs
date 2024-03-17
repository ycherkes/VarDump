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

        var memberInitializers = members
            .Where(m => (!options.IgnoreNullValues || options.IgnoreNullValues && m.Value != null) &&
                        (!options.IgnoreDefaultValues || !m.Type.IsValueType || options.IgnoreDefaultValues &&
                            ReflectionUtils.GetDefaultValue(m.Type)?.Equals(m.Value) != true))
            .Select(m => (Action)(() => codeWriter.WriteAssign(
                () => codeWriter.WritePropertyReference(m.Name, null),
                () => nextDepthVisitor.Visit(m.Value, context))));

        codeWriter.WriteObjectCreateAndInitialize
        (
            objectDescription.Type,
            constructorArguments,
            memberInitializers
        );
    }
}