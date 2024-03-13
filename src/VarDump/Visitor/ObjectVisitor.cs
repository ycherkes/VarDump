using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Collections;
using VarDump.Extensions;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;
using VarDump.Visitor.KnownObjects;

namespace VarDump.Visitor;

internal sealed class ObjectVisitor : IObjectVisitor, INextDepthVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly IKnownObjectsOrderedDictionary _knownObjects;
    private readonly int _maxDepth;
    private readonly ICurrentDepthVisitor _generalVisitor;

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

        _generalVisitor = new GeneralVisitor(codeWriter, this, objectDescriptor, options);

        _knownObjects = new KnownObjectsOrderedDictionary
        {
            new PrimitiveVisitor(codeWriter, options.Clone()),
            new TimeSpanVisitor(codeWriter, options.Clone()),
            new DateTimeVisitor(codeWriter, options.Clone()),
            new DateTimeOffsetVisitor(this, codeWriter, options.Clone()),
            new EnumVisitor(codeWriter, options.Clone()),
            new GuidVisitor(codeWriter, options.Clone()),
            new CultureInfoVisitor(codeWriter, options.Clone()),
            new TypeVisitor(codeWriter, options.Clone()),
            new IPAddressVisitor(codeWriter, options.Clone()),
            new IPEndpointVisitor(this, codeWriter, options.Clone()),
            new DnsEndPointVisitor(this, codeWriter, options.Clone()),
            new VersionVisitor(codeWriter, options.Clone()),
            new DateOnlyVisitor(codeWriter, options.Clone()),
            new TimeOnlyVisitor(codeWriter, options.Clone()),
            new RecordVisitor(this, codeWriter, options.Clone()),
            new AnonymousVisitor(this, anonymousObjectDescriptor, codeWriter, options.Clone()),
            new KeyValuePairVisitor(this, codeWriter, options.Clone()),
            new TupleVisitor(this, codeWriter, options.Clone()),
            new ValueTupleVisitor(this, codeWriter, options.Clone()),
            new UriVisitor(codeWriter, options.Clone()),
            new GroupingVisitor(this, codeWriter, options.Clone()),
            new DictionaryVisitor(this, codeWriter, options.Clone()),
            new CollectionVisitor(this, codeWriter, options.Clone()),
        };

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

            var suitableVisitor = _knownObjects.Values.FirstOrDefault(v => v.IsSuitableFor(@object, objectType))
                                  ?? _generalVisitor;

            suitableVisitor.Visit(@object, objectType, context);
        }
        finally
        {
            context.CurrentDepth--;
        }
    }
}