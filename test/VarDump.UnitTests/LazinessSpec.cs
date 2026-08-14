using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;
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

    private sealed class ThrowingProperty
    {
        public string Value => throw new InvalidOperationException("The getter must not be evaluated.");
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
