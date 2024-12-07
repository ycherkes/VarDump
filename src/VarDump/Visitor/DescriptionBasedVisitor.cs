using System;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor;

internal sealed class DescriptionBasedVisitor : ISpecificVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly IObjectDescriptor _objectDescriptor;
    private readonly DumpOptions _options;
    private readonly ObjectDescriptionWriter _descriptionWriter;

    public DescriptionBasedVisitor(ICodeWriter codeWriter,
        INextDepthVisitor nextDepthVisitor,
        DumpOptions options)
    {
        _codeWriter = codeWriter;
        _options = options;

        _objectDescriptor = new ObjectPropertiesDescriptor(options.GetPropertiesBindingFlags, options.IgnoreReadonlyProperties);

        if (options.GetFieldsBindingFlags != null)
        {
            _objectDescriptor = _objectDescriptor.Concat(new ObjectFieldsDescriptor(options));
        }

        if (options.Descriptors?.Count > 0)
        {
            _objectDescriptor = _objectDescriptor.ApplyMiddleware(options.Descriptors);
        }

        _descriptionWriter = new ObjectDescriptionWriter(nextDepthVisitor, codeWriter);
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
            objectDescription.Type ??= objectType;
            _descriptionWriter.Write(objectDescription, context, _options);
        }
        finally
        {
            context.PopVisited();
        }
    }
}