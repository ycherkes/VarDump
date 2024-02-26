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
        using var sourceWriter = new StringWriter();

        dumper.Dump(GetItems(), sourceWriter);

        var result = sourceWriter.ToString();

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

        IEnumerable<int> GetItems()
        {
            yield return 1;
            Assert.Contains("1", sourceWriter.ToString());
            yield return 2;
            Assert.Contains("2", sourceWriter.ToString());
            yield return 3;
            Assert.Contains("3", sourceWriter.ToString());
        }
    }
}