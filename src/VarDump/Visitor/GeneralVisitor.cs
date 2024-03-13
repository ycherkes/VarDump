﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor;

internal sealed class GeneralVisitor(
    ICodeWriter codeWriter,
    INextDepthVisitor nextDepthVisitor,
    IObjectDescriptor objectDescriptor,
    DumpOptions dumpOptions) : ICurrentDepthVisitor
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

            var members = ((IEnumerable<MemberDescription>)objectDescription.Fields).Concat(objectDescription.Properties);

            if (dumpOptions.SortDirection != null)
            {
                members = dumpOptions.SortDirection == ListSortDirection.Ascending
                    ? members.OrderBy(m => m.Name)
                    : members.OrderByDescending(m => m.Name);
            }

            var constructorArguments = objectDescription.ConstructorArguments
                .Select(ca => !string.IsNullOrWhiteSpace(ca.Name) && dumpOptions.UseNamedArgumentsInConstructors
                    ? () => codeWriter.WriteNamedArgument(ca.Name, () => nextDepthVisitor.Visit(ca.Value, context))
                    : (Action)(() => nextDepthVisitor.Visit(ca.Value, context)));

            var memberInitializers = members
                .Where(m => !dumpOptions.ExcludeTypes.Contains(m.Type.FullName) &&
                             (!dumpOptions.IgnoreNullValues || dumpOptions.IgnoreNullValues && m.Value != null) &&
                             (!dumpOptions.IgnoreDefaultValues || !m.Type.IsValueType || dumpOptions.IgnoreDefaultValues &&
                                 ReflectionUtils.GetDefaultValue(m.Type)?.Equals(m.Value) != true))
                .Select(m => (Action)(() => codeWriter.WriteAssign(
                    () => codeWriter.WritePropertyReference(m.Name, null),
                    () => nextDepthVisitor.Visit(m.Value, context))));

            codeWriter.WriteObjectCreateAndInitialize
            (
                objectDescription.Type ?? objectType,
                constructorArguments,
                memberInitializers
            );
        }
        finally
        {
            context.PopVisited();
        }
    }
}