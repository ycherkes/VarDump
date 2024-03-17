using System.Linq;
using VarDump.CodeDom.Compiler;
using VarDump.Collections;
using VarDump.Extensions;
using VarDump.Visitor.KnownObjects;

namespace VarDump.Visitor;

internal sealed class ObjectVisitor : IObjectVisitor, INextDepthVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly IKnownObjectsCollection _knownObjects;
    private readonly int _maxDepth;
    private readonly ISpecificVisitor _descriptionBasedVisitor;

    public ObjectVisitor(DumpOptions options, ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
        _maxDepth = options.MaxDepth;

        _descriptionBasedVisitor = new DescriptionBasedVisitor(codeWriter, this, options);

        _knownObjects = new KnownObjectsCollection
        {
            new PrimitiveVisitor(codeWriter),
            new TimeSpanVisitor(codeWriter, options),
            new DateTimeVisitor(codeWriter, options),
            new DateTimeOffsetVisitor(this, codeWriter, options),
            new EnumVisitor(codeWriter),
            new GuidVisitor(codeWriter, options),
            new CultureInfoVisitor(codeWriter, options),
            new TypeVisitor(codeWriter),
            new IPAddressVisitor(codeWriter),
            new IPEndpointVisitor(this, codeWriter, options),
            new DnsEndPointVisitor(this, codeWriter, options),
            new VersionVisitor(codeWriter, options),
            new DateOnlyVisitor(codeWriter, options),
            new TimeOnlyVisitor(codeWriter, options),
            new RecordVisitor(this, codeWriter, options),
            new AnonymousVisitor(this, codeWriter, options),
            new KeyValuePairVisitor(this, codeWriter, options),
            new TupleVisitor(this, codeWriter, options),
            new ValueTupleVisitor(this, codeWriter, options),
            new UriVisitor(codeWriter, options),
            new RegexVisitor(codeWriter, this, options),
            new GroupingVisitor(this, codeWriter),
            new DictionaryVisitor(this, codeWriter, options),
            new CollectionVisitor(this, codeWriter, options)
        };

        options.ConfigureKnownObjects?.Invoke(_knownObjects, this, options.Clone(), codeWriter);
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

            var specificVisitor = _knownObjects.Values.FirstOrDefault(v => v.IsSuitableFor(@object, objectType))
                                  ?? _descriptionBasedVisitor;

            specificVisitor.Visit(@object, objectType, context);
        }
        finally
        {
            context.CurrentDepth--;
        }
    }
}