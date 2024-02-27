using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Collections;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;
using VarDump.Visitor.KnownTypes;

namespace VarDump.Visitor;

internal sealed class ObjectVisitor : IObjectVisitor
{
    private readonly ICodeGenerator _codeGenerator;
    private readonly ICollection<string> _excludeTypes;
    private readonly bool _ignoreDefaultValues;
    private readonly bool _ignoreNullValues;
    private readonly int _maxDepth;
    private readonly Stack<object> _visitedObjects;
    private readonly ListSortDirection? _sortDirection;
    private readonly IObjectDescriptor _objectDescriptor;
    private readonly OrderedDictionary<string, IKnownObjectVisitor> _knownTypes;

    private int _depth;

    public ObjectVisitor(DumpOptions options, ICodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
        _maxDepth = options.MaxDepth;
        _ignoreDefaultValues = options.IgnoreDefaultValues;
        _ignoreNullValues = options.IgnoreNullValues;
        _excludeTypes = options.ExcludeTypes ?? [];
        _sortDirection = options.SortDirection;

        IObjectDescriptor anonymousObjectDescriptor = new ObjectPropertiesDescriptor(options.GetPropertiesBindingFlags, false);
        _objectDescriptor = new ObjectPropertiesDescriptor(options.GetPropertiesBindingFlags, options.WritablePropertiesOnly);

        if (options.GetFieldsBindingFlags != null)
        {
            _objectDescriptor = anonymousObjectDescriptor.Concat(new ObjectFieldsDescriptor(options.GetFieldsBindingFlags.Value));
        }

        if (options.Descriptors.Count > 0)
        {
            _objectDescriptor = _objectDescriptor.ApplyMiddleware(options.Descriptors);
            anonymousObjectDescriptor = anonymousObjectDescriptor.ApplyMiddleware(options.Descriptors);
        }

        _visitedObjects = new Stack<object>();

        _knownTypes = new[]
        {
            (IKnownObjectVisitor)new PrimitiveVisitor(codeGenerator),
            new TimeSpanVisitor(codeGenerator, options.DateTimeInstantiation),
            new DateTimeVisitor(codeGenerator, options.DateTimeInstantiation, options.DateKind),
            new DateTimeOffsetVisitor(this, codeGenerator, options.DateTimeInstantiation),
            new EnumVisitor(codeGenerator),
            new GuidVisitor(codeGenerator),
            new CultureInfoVisitor(codeGenerator),
            new TypeVisitor(codeGenerator),
            new IPAddressVisitor(codeGenerator),
            new IPEndpointVisitor(this, codeGenerator),
            new DnsEndPointVisitor(this, codeGenerator),
            new VersionVisitor(codeGenerator),
            new DateOnlyVisitor(codeGenerator, options.DateTimeInstantiation),
            new TimeOnlyVisitor(codeGenerator, options.DateTimeInstantiation),
            new RecordVisitor(this, codeGenerator, options.UseNamedArgumentsForReferenceRecordTypes),
            new AnonymousTypeVisitor(this, anonymousObjectDescriptor, codeGenerator),
            new KeyValuePairVisitor(this, codeGenerator),
            new TupleVisitor(this, codeGenerator),
            new ValueTupleVisitor(this, codeGenerator),
            new GroupingVisitor(this, codeGenerator),
            new DictionaryVisitor(this, codeGenerator, options.MaxCollectionSize),
            new CollectionVisitor(this, codeGenerator, options.MaxCollectionSize),
        }.ToOrderedDictionary(v => v.Id);

        options.ConfigureKnownTypes?.Invoke(_knownTypes, this, options, codeGenerator);
    }

    public void Visit(object @object)
    {
        if (IsMaxDepth())
        {
            _codeGenerator.WriteMaxDepthExpression(@object);
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
            _codeGenerator.WriteCircularReferenceDetected();
            return;
        }

        PushVisited(o);

        try
        {
            var membersAndConstructorParams = _objectDescriptor.Describe(o, objectType).ToArray();

            var members = membersAndConstructorParams.Where(m => m.ReflectionType != ReflectionType.ConstructorParameter);

            if (_sortDirection != null)
            {
                members = _sortDirection == ListSortDirection.Ascending
                    ? members.OrderBy(x => x.Name)
                    : members.OrderByDescending(x => x.Name);
            }

            var constructorParams = membersAndConstructorParams
                    .Where(mc => mc.ReflectionType == ReflectionType.ConstructorParameter)
                    .Select(cp => (Action)(() => Visit(cp.Value)));

            var initializeActions = members
                    .Where(pv => !_excludeTypes.Contains(pv.Type.FullName) &&
                                 (!_ignoreNullValues || _ignoreNullValues && pv.Value != null) &&
                                 (!_ignoreDefaultValues || !pv.Type.IsValueType || _ignoreDefaultValues &&
                                     ReflectionUtils.GetDefaultValue(pv.Type)?.Equals(pv.Value) != true))
                    .Select(pv => (Action)(() => _codeGenerator.GenerateCodeAssign(
                        () => _codeGenerator.GeneratePropertyReference(pv.Name, null),
                        () => Visit(pv.Value))));

            _codeGenerator.GenerateObjectCreateAndInitialize(new CodeTypeReference(objectType),
                constructorParams,
                initializeActions);
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