using System.Collections.Generic;
using System.IO;
using Xunit;

namespace VarDump.UnitTests;

public class LazinessSpec
{
    [Fact]
    public void DumpEnumerableCsharp()
    {
        var dumper = new CSharpDumper();
        using var writer = new StringWriter();

        dumper.Dump(GetItems(writer), writer);

        var result = writer.ToString();

        Assert.Equal(
            """
            var dumpEnumerableCsharpOfInt = new int[]
            {
                1,
                2,
                3
            };

            """, result);
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
}