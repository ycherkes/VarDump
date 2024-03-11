using System.Collections.Generic;
using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;
using VarDump.Visitor.KnownObjects;

namespace VarDump.Visitor;

internal sealed class ObjectVisitor : IObjectVisitor, IRootVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly List<IKnownObjectVisitor> _knownObjects;
    private readonly int _maxDepth;
    private readonly ISpecificObjectVisitor _unknownObjectVisitor;

    public ObjectVisitor(DumpOptions options, ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
        _maxDepth = options.MaxDepth;

        IObjectDescriptor anonymousObjectDescriptor = new ObjectPropertiesDescriptor(options.GetPropertiesBindingFlags, false);
        IObjectDescriptor objectDescriptor = new ObjectPropertiesDescriptor(options.GetPropertiesBindingFlags, options.WritablePropertiesOnly);

        if (options.GetFieldsBindingFlags != null)
        {
            objectDescriptor = objectDescriptor.Concat(new ObjectFieldsDescriptor(options.GetFieldsBindingFlags.Value));
        }

        if (options.Descriptors?.Count > 0)
        {
            objectDescriptor = objectDescriptor.ApplyMiddleware(options.Descriptors);
            anonymousObjectDescriptor = anonymousObjectDescriptor.ApplyMiddleware(options.Descriptors);
        }

        _unknownObjectVisitor = new UnknownObjectVisitor(codeWriter, this, objectDescriptor, options);

        _knownObjects =
        [
            new PrimitiveVisitor(codeWriter),
            new TimeSpanVisitor(codeWriter, options.DateTimeInstantiation),
            new DateTimeVisitor(codeWriter, options.DateTimeInstantiation, options.DateKind),
            new DateTimeOffsetVisitor(this, codeWriter, options.DateTimeInstantiation),
            new EnumVisitor(codeWriter),
            new GuidVisitor(codeWriter),
            new CultureInfoVisitor(codeWriter),
            new TypeVisitor(codeWriter),
            new IPAddressVisitor(codeWriter),
            new IPEndpointVisitor(this, codeWriter),
            new DnsEndPointVisitor(this, codeWriter),
            new VersionVisitor(codeWriter),
            new DateOnlyVisitor(codeWriter, options.DateTimeInstantiation),
            new TimeOnlyVisitor(codeWriter, options.DateTimeInstantiation),
            new RecordVisitor(this, codeWriter, options.UseNamedArgumentsInConstructors),
            new AnonymousVisitor(this, anonymousObjectDescriptor, codeWriter),
            new KeyValuePairVisitor(this, codeWriter),
            new TupleVisitor(this, codeWriter),
            new ValueTupleVisitor(this, codeWriter),
            new UriVisitor(codeWriter),
            new GroupingVisitor(this, codeWriter),
            new DictionaryVisitor(this, codeWriter, options.MaxCollectionSize),
            new CollectionVisitor(this, codeWriter, options.MaxCollectionSize),
        ];

        options.ConfigureKnownObjects?.Invoke(_knownObjects, this, options, codeWriter);
    }

    public void Visit(object @object)
    {
        Visit(@object, new VisitContext(_maxDepth));
    }

    public void Visit(object @object, VisitContext context)
    {
        if (context.IsMaxDepth())
        {
            _codeWriter.WriteMaxDepthExpression(@object);
            return;
        }

        try
        {
            context.CurrentDepth++;

            var objectType = @object?.GetType();

            var suitableVisitor = _knownObjects.FirstOrDefault(v => v.IsSuitableFor(@object, objectType))
                                  ?? _unknownObjectVisitor;

            suitableVisitor.Visit(@object, objectType, context);
        }
        finally
        {
            context.CurrentDepth--;
        }
    }
}