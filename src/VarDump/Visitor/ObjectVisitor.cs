﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Collections;
using VarDump.Extensions;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;
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
    private readonly IObjectDescriptor _objectDescriptor;
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
            new AnonymousTypeVisitor(this, anonymousObjectDescriptor, codeWriter),
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
            var objectDescription = _objectDescriptor.Describe(o, objectType);

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

            var initializeActions = members
                    .Where(pv => !_excludeTypes.Contains(pv.Type.FullName) &&
                                 (!_ignoreNullValues || _ignoreNullValues && pv.Value != null) &&
                                 (!_ignoreDefaultValues || !pv.Type.IsValueType || _ignoreDefaultValues &&
                                     ReflectionUtils.GetDefaultValue(pv.Type)?.Equals(pv.Value) != true))
                    .Select(pv => (Action)(() => _codeWriter.WriteAssign(
                        () => _codeWriter.WritePropertyReference(pv.Name, null),
                        () => Visit(pv.Value))));

            _codeWriter.WriteObjectCreateAndInitialize(objectDescription.Type ?? new TypeReference(objectType),
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