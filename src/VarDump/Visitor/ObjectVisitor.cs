using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.Collections;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;
using VarDump.Visitor.KnownTypes;

namespace VarDump.Visitor;

internal sealed class ObjectVisitor : IObjectVisitor
{
    private readonly ICollection<string> _excludeTypes;
    private readonly bool _ignoreDefaultValues;
    private readonly bool _ignoreNullValues;
    private readonly int _maxDepth;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly Stack<object> _visitedObjects;
    private readonly ListSortDirection? _sortDirection;
    private readonly IObjectDescriptor _objectDescriptor;
    private readonly OrderedDictionary<string, IKnownObjectVisitor> _knownTypes;

    private int _depth;

    public ObjectVisitor(DumpOptions options)
    {
        _maxDepth = options.MaxDepth;
        _ignoreDefaultValues = options.IgnoreDefaultValues;
        _ignoreNullValues = options.IgnoreNullValues;
        _typeReferenceOptions = options.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
        _excludeTypes = options.ExcludeTypes ?? new List<string>();
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
            (IKnownObjectVisitor)new PrimitiveVisitor(options),
            new TimeSpanVisitor(options),
            new DateTimeVisitor(options),
            new DateTimeOffsetVisitor(options, this),
            new EnumVisitor(options),
            new GuidVisitor(options),
            new CultureInfoVisitor(options),
            new TypeVisitor(options),
            new IPAddressVisitor(options),
            new IPEndpointVisitor(options, this),
            new DnsEndPointVisitor(options, this),
            new VersionVisitor(options),
            new DateOnlyVisitor(options),
            new TimeOnlyVisitor(options),
            new RecordVisitor(options, this),
            new AnonymousTypeVisitor(options, this, anonymousObjectDescriptor),
            new KeyValuePairVisitor(options, this),
            new TupleVisitor(options, this),
            new ValueTupleVisitor(this),
            new GroupingVisitor(this),
            new DictionaryVisitor(options, this),
            new CollectionVisitor(options, this)
        }.ToOrderedDictionary(v => v.Id);

        options.ConfigureKnownTypes?.Invoke(_knownTypes, this, options);
    }

    public CodeExpression Visit(IValueDescriptor descriptor)
    {
        if (IsMaxDepth())
        {
            return CodeDomUtils.GetMaxDepthExpression(descriptor.Value, _typeReferenceOptions);
        }
        try
        {
            _depth++;

            var knownObjectVisitor = _knownTypes.Values.FirstOrDefault(v => v.IsSuitableFor(descriptor));
            
            if (knownObjectVisitor != null)
            {
                return knownObjectVisitor.Visit(descriptor);
            }

            return VisitObject(descriptor);
        }
        finally
        {
            _depth--;
        }
    }

    private CodeExpression VisitObject(IValueDescriptor valueDescriptor)
    {
        if (IsVisited(valueDescriptor.Value))
        {
            return CodeDomUtils.GetCircularReferenceDetectedExpression();
        }

        PushVisited(valueDescriptor.Value);

        try
        {
            var membersAndConstructorParams = _objectDescriptor.Describe(valueDescriptor.Value, valueDescriptor.Type).ToArray();

            var members = membersAndConstructorParams.Where(m => m.ReflectionType != ReflectionType.ConstructorParameter);

            if (_sortDirection != null)
            {
                members = _sortDirection == ListSortDirection.Ascending
                    ? members.OrderBy(x => x.Name)
                    : members.OrderByDescending(x => x.Name);
            }

            var constructorParams = membersAndConstructorParams
                .Where(mc => mc.ReflectionType == ReflectionType.ConstructorParameter)
                .Select(Visit)
                .ToArray();

            var initializeExpressions = members
                .Where(pv => !_excludeTypes.Contains(pv.MemberType.FullName) &&
                             (!_ignoreNullValues || _ignoreNullValues && pv.Value != null) &&
                             (!_ignoreDefaultValues || !pv.MemberType.IsValueType || _ignoreDefaultValues &&
                                 ReflectionUtils.GetDefaultValue(pv.MemberType)?.Equals(pv.Value) != true))
                .Select(pv => (CodeExpression)new CodeAssignExpression(new CodePropertyReferenceExpression(null, pv.Name), Visit(pv)));
            
            var result = new CodeObjectCreateAndInitializeExpression(new CodeTypeReference(valueDescriptor.Type, _typeReferenceOptions), initializeExpressions, constructorParams);

            return result;
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