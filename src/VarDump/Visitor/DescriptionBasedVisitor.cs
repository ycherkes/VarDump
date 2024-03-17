using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor;

internal sealed class DescriptionBasedVisitor : ISpecificVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly INextDepthVisitor _nextDepthVisitor;
    private readonly IObjectDescriptor _objectDescriptor;
    private readonly DumpOptions _options;

    public DescriptionBasedVisitor(ICodeWriter codeWriter,
        INextDepthVisitor nextDepthVisitor,
        DumpOptions options)
    {
        _codeWriter = codeWriter;
        _nextDepthVisitor = nextDepthVisitor;
        _options = options;

        _objectDescriptor = new ObjectPropertiesDescriptor(options.GetPropertiesBindingFlags, options.WritablePropertiesOnly);

        if (options.GetFieldsBindingFlags != null)
        {
            _objectDescriptor = _objectDescriptor.Concat(new ObjectFieldsDescriptor(options.GetFieldsBindingFlags.Value));
        }

        if (options.Descriptors?.Count > 0)
        {
            _objectDescriptor = _objectDescriptor.ApplyMiddleware(options.Descriptors);
        }
    }

    public void Visit(object o, Type objectType, VisitContext context)
    {
        if (context.IsVisited(o))
        {
            _codeWriter.WriteCircularReferenceDetected();
            return;
        }

        context.PushVisited(o);

        try
        {
            var objectDescription = _objectDescriptor.GetObjectDescription(o, objectType);

            var members = ((IEnumerable<MemberDescription>)objectDescription.Fields).Concat(objectDescription.Properties);

            if (_options.SortDirection != null)
            {
                members = _options.SortDirection == ListSortDirection.Ascending
                    ? members.OrderBy(m => m.Name)
                    : members.OrderByDescending(m => m.Name);
            }

            var constructorArguments = objectDescription.ConstructorArguments
                .Select(ca => !string.IsNullOrWhiteSpace(ca.Name) && _options.UseNamedArgumentsInConstructors
                    ? () => _codeWriter.WriteNamedArgument(ca.Name, () => _nextDepthVisitor.Visit(ca.Value, context))
                    : (Action)(() => _nextDepthVisitor.Visit(ca.Value, context)));

            var memberInitializers = members
                .Where(m => (!_options.IgnoreNullValues || _options.IgnoreNullValues && m.Value != null) &&
                             (!_options.IgnoreDefaultValues || !m.Type.IsValueType || _options.IgnoreDefaultValues &&
                                 ReflectionUtils.GetDefaultValue(m.Type)?.Equals(m.Value) != true))
                .Select(m => (Action)(() => _codeWriter.WriteAssign(
                    () => _codeWriter.WritePropertyReference(m.Name, null),
                    () => _nextDepthVisitor.Visit(m.Value, context))));

            _codeWriter.WriteObjectCreateAndInitialize
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