using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;

namespace VarDump.Visitor.KnownObjects;

internal sealed class AnonymousVisitor : IKnownObjectVisitor
{
    private static readonly ConcurrentDictionary<PropertiesCacheKey, AnonymousProperty[]> PropertiesCache = new();
    private readonly INextDepthVisitor _nextDepthVisitor;
    private readonly IObjectDescriptor _anonymousObjectDescriptor;
    private readonly ICodeWriter _codeWriter;
    private readonly BindingFlags _getPropertiesBindingFlags;

    public AnonymousVisitor(INextDepthVisitor nextDepthVisitor,
        ICodeWriter codeWriter,
        DumpOptions options)
    {
        _nextDepthVisitor = nextDepthVisitor;
        _codeWriter = codeWriter;
        _getPropertiesBindingFlags = options.GetPropertiesBindingFlags;

        if (options.Descriptors == null || options.Descriptors.Count == 0)
            return;

        _anonymousObjectDescriptor = new ObjectPropertiesDescriptor(options.GetPropertiesBindingFlags, false)
            .ApplyMiddleware(options.Descriptors);
    }

    public string Id => "Anonymous";

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsAnonymousType();
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        if (_anonymousObjectDescriptor == null)
        {
            VisitCachedProperties(obj, objectType, context);
            return;
        }

        var properties = _anonymousObjectDescriptor.GetObjectDescription(obj, objectType).Properties;

        _codeWriter.WriteObjectCreateAndInitializeItems(new CodeAnonymousTypeInfo(), [], properties, property =>
        {
            var description = (PropertyDescription)property;
            _codeWriter.WriteMemberAssignmentStart(description.Name);
            if (description.Type.IsNullableType() || description.Value == null)
            {
                _codeWriter.WriteCast(description.Type, () => _nextDepthVisitor.Visit(description.Value, context));
            }
            else
            {
                _nextDepthVisitor.Visit(description.Value, context);
            }
        });
    }

    private void VisitCachedProperties(object obj, Type objectType, VisitContext context)
    {
        var properties = PropertiesCache.GetOrAdd(
            new PropertiesCacheKey(objectType, _getPropertiesBindingFlags),
            static key => key.Type
                .GetProperties(key.BindingFlags)
                .Where(property => property.CanRead && !ReflectionUtils.IsIndexer(property))
                .Select(property => new AnonymousProperty(property))
                .ToArray());

        _codeWriter.WriteObjectCreateAndInitializeItems(new CodeAnonymousTypeInfo(), [], properties, property =>
        {
            var description = (AnonymousProperty)property;
            var value = ReflectionUtils.GetValue(description.Property, obj);
            _codeWriter.WriteMemberAssignmentStart(description.Name);
            if (description.Type.IsNullableType() || value == null)
            {
                _codeWriter.WriteCast(description.Type, () => _nextDepthVisitor.Visit(value, context));
            }
            else
            {
                _nextDepthVisitor.Visit(value, context);
            }
        });
    }

    private sealed class AnonymousProperty(PropertyInfo property)
    {
        public PropertyInfo Property { get; } = property;
        public string Name { get; } = property.Name;
        public Type Type { get; } = property.PropertyType;
    }

    private struct PropertiesCacheKey : IEquatable<PropertiesCacheKey>
    {
        public PropertiesCacheKey(Type type, BindingFlags bindingFlags)
        {
            Type = type;
            BindingFlags = bindingFlags;
        }

        public Type Type { get; }
        public BindingFlags BindingFlags { get; }

        public bool Equals(PropertiesCacheKey other)
        {
            return Type == other.Type && BindingFlags == other.BindingFlags;
        }

        public override bool Equals(object obj)
        {
            return obj is PropertiesCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type?.GetHashCode() ?? 0) * 397) ^ (int)BindingFlags;
            }
        }
    }
}
