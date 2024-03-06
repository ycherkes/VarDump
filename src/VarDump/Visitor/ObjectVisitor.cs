using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Collections;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor.Describers;
using VarDump.Visitor.Describers.Implementation;
using VarDump.Visitor.KnownTypes;

namespace VarDump.Visitor;

internal sealed class ObjectVisitor : IObjectVisitor
{
    private readonly ICodeWriter _codeWriter;
    private readonly ICollection<string> _excludeTypes;
    private readonly bool _ignoreDefaultValues;
    private readonly bool _ignoreNullValues;
    private readonly int _maxDepth;
    private readonly Stack<object> _visitedObjects;
    private readonly ListSortDirection? _sortDirection;
    private readonly IObjectDescriber _objectDescriber;
    private readonly OrderedDictionary<string, IKnownObjectVisitor> _knownTypes;

    private int _depth;

    public ObjectVisitor(DumpOptions options, ICodeWriter codeWriter)
    {
        _codeWriter = codeWriter;
        _maxDepth = options.MaxDepth;
        _ignoreDefaultValues = options.IgnoreDefaultValues;
        _ignoreNullValues = options.IgnoreNullValues;
        _excludeTypes = options.ExcludeTypes ?? [];
        _sortDirection = options.SortDirection;

        IObjectDescriber anonymousObjectDescriber = new ObjectPropertiesDescriber(options.GetPropertiesBindingFlags, false);
        _objectDescriber = new ObjectPropertiesDescriber(options.GetPropertiesBindingFlags, options.WritablePropertiesOnly);

        if (options.GetFieldsBindingFlags != null)
        {
            _objectDescriber = anonymousObjectDescriber.Concat(new ObjectFieldsDescriber(options.GetFieldsBindingFlags.Value));
        }

        if (options.Describers.Count > 0)
        {
            _objectDescriber = _objectDescriber.ApplyMiddleware(options.Describers);
            anonymousObjectDescriber = anonymousObjectDescriber.ApplyMiddleware(options.Describers);
        }

        _visitedObjects = new Stack<object>();

        _knownTypes = new[]
        {
            (IKnownObjectVisitor)new PrimitiveVisitor(codeWriter),
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
            new RecordVisitor(this, codeWriter, options.UseNamedArgumentsForReferenceRecordTypes),
            new AnonymousTypeVisitor(this, anonymousObjectDescriber, codeWriter),
            new KeyValuePairVisitor(this, codeWriter),
            new TupleVisitor(this, codeWriter),
            new ValueTupleVisitor(this, codeWriter),
            new UriVisitor(codeWriter),
            new GroupingVisitor(this, codeWriter),
            new DictionaryVisitor(this, codeWriter, options.MaxCollectionSize),
            new CollectionVisitor(this, codeWriter, options.MaxCollectionSize),
        }.ToOrderedDictionary(v => v.Id);

        options.ConfigureKnownTypes?.Invoke(_knownTypes, this, options, codeWriter);
    }

    public void Visit(object @object)
    {
        if (IsMaxDepth())
        {
            _codeWriter.WriteMaxDepthExpression(@object);
            return;
        }

        try
        {
            _depth++;

            var objectType = @object?.GetType();

            var knownObjectVisitor = _knownTypes.Values.FirstOrDefault(v => v.IsSuitableFor(@object, objectType));
            
            if (knownObjectVisitor != null)
            {
                knownObjectVisitor.Visit(@object, objectType);
                return;
            }

            VisitObject(@object, objectType);
        }
        finally
        {
            _depth--;
        }
    }

    private void VisitObject(object o, Type objectType)
    {
        if (IsVisited(o))
        {
            _codeWriter.WriteCircularReferenceDetected();
            return;
        }

        PushVisited(o);

        try
        {
            var objectDescription = _objectDescriber.DescribeObject(o, objectType);

            var members = objectDescription.Members;

            if (_sortDirection != null)
            {
                members = _sortDirection == ListSortDirection.Ascending
                    ? members.OrderBy(x => x.Name)
                    : members.OrderByDescending(x => x.Name);
            }

            var constructorParams = objectDescription.ConstructorParameters
                    .Where(mc => mc.ReflectionType == ReflectionType.ConstructorParameter)
                    .Select(cp => (Action)(() => Visit(cp.Value)));

            var initializers = members
                    .Where(pv => !_excludeTypes.Contains(pv.Type.FullName) &&
                                 (!_ignoreNullValues || _ignoreNullValues && pv.Value != null) &&
                                 (!_ignoreDefaultValues || !pv.Type.IsValueType || _ignoreDefaultValues &&
                                     ReflectionUtils.GetDefaultValue(pv.Type)?.Equals(pv.Value) != true))
                    .Select(pv => (Action)(() => _codeWriter.WriteAssign(
                        () => _codeWriter.WritePropertyReference(pv.Name, null),
                        () => Visit(pv.Value))));

            _codeWriter.WriteObjectCreateAndInitialize(objectDescription.Type ?? new CodeTypeInfo(objectType),
                constructorParams,
                initializers);
        }
        finally
        {
            PopVisited();
        }
    }

    public void PushVisited(object value)
    {
        _visitedObjects.Push(value);
    }

    public void PopVisited()
    {
        _visitedObjects.Pop();
    }

    public bool IsVisited(object value)
    {
        return value != null && _visitedObjects.Contains(value);
    }

    private bool IsMaxDepth()
    {
        return _depth > _maxDepth;
    }
}