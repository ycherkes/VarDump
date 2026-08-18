using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Implementation;
using Xunit;

namespace VarDump.UnitTests;

public class LazinessSpec
{
    [Fact]
    public void DumpEnumerableCSharp()
    {
        var dumper = new CSharpDumper();
        using var writer = new StringWriter();

        dumper.Dump(GetItems(writer), writer);

        var result = writer.ToString();

        Assert.Equal(
            """
            var dumpEnumerableCSharpOfInt = new int[]
            {
                1,
                2,
                3
            };

            """, result, ignoreLineEndingDifferences: true);
        return;

        static IEnumerable<int> GetItems(StringWriter writer)
        {
            yield return 1;
            Assert.Contains("1", writer.ToString());
            yield return 2;
            Assert.Contains("2", writer.ToString());
            yield return 3;
            Assert.Contains("3", writer.ToString());
        }
    }

    [Fact]
    public void FilteredPropertyGetterRemainsLazy()
    {
        var dumper = new CSharpDumper(new DumpOptions
        {
            Descriptors =
            {
                new SkipPropertyMiddleware(nameof(ThrowingProperty.Value))
            }
        });

        var result = dumper.Dump(new ThrowingProperty());

        Assert.Equal("var throwingProperty = new ThrowingProperty();\r\n", result,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ReflectionBackedPropertyValueIsLazyAndMemoized()
    {
        var source = new CountingProperty();
        var propertyInfo = typeof(CountingProperty).GetProperty(nameof(CountingProperty.Value));
        var description = new PropertyDescription(propertyInfo, source);

        Assert.Equal(0, source.ReadCount);
        Assert.Equal("value", description.Value);
        Assert.Equal("value", description.Value);
        Assert.Equal(1, source.ReadCount);
    }

    [Fact]
    public void CachingDescriptorCachesMetadataButBindsEachObjectSeparately()
    {
        CountingDefaultValueAttribute.InstanceCount = 0;
        var descriptor = new CachingObjectPropertiesDescriptor(
            new ObjectPropertiesDescriptor(BindingFlags.Public | BindingFlags.Instance, writablePropertiesOnly: false));

        var first = descriptor.GetObjectDescription(new CachedPropertySource("first"), typeof(CachedPropertySource));
        var second = descriptor.GetObjectDescription(new CachedPropertySource("second"), typeof(CachedPropertySource));

        Assert.Equal(0, CountingDefaultValueAttribute.InstanceCount);
        Assert.Equal("first", Assert.Single(first.Properties).Value);
        Assert.Equal("second", Assert.Single(second.Properties).Value);
        Assert.Equal(1, CountingDefaultValueAttribute.InstanceCount);
    }

    private sealed class ThrowingProperty
    {
        public string Value => throw new InvalidOperationException("The getter must not be evaluated.");
    }

    private sealed class CountingProperty
    {
        public int ReadCount { get; private set; }
        public string Value => ++ReadCount == 1 ? "value" : throw new InvalidOperationException("The getter must only be evaluated once.");
    }

    private sealed class CachedPropertySource(string value)
    {
        [CountingDefaultValue]
        public string Value { get; } = value;
    }

    private sealed class CountingDefaultValueAttribute : DefaultValueAttribute
    {
        public static int InstanceCount { get; set; }

        public CountingDefaultValueAttribute()
            : base(null)
        {
            InstanceCount++;
        }
    }

    private sealed class SkipPropertyMiddleware(string propertyName) : IObjectDescriptorMiddleware
    {
        public IObjectDescription GetObjectDescription(object @object, System.Type objectType,
            System.Func<IObjectDescription> prev)
        {
            var description = prev();
            description.Properties = description.Properties.Where(property => property.Name != propertyName);
            return description;
        }
    }
}
