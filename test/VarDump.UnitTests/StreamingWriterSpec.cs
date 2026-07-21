using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace VarDump.UnitTests;

public class StreamingWriterSpec
{
    [Fact]
    public void DumpSingleUseEnumerableCSharp()
    {
        var items = new SingleUseEnumerable();

        var result = new CSharpDumper().Dump(items);

        Assert.Equal(
            """
            var singleUseEnumerableOfInt = new int[]
            {
                1,
                2,
                3
            };

            """, result, ignoreLineEndingDifferences: true);
        Assert.Equal(1, items.EnumerationCount);
    }

    [Fact]
    public void DumpSingleUseEnumerableVisualBasic()
    {
        var items = new SingleUseEnumerable();

        var result = new VisualBasicDumper().Dump(items);

        Assert.Equal(
            """
            Dim singleUseEnumerableOfInteger = New Integer(){
                1,
                2,
                3
            }

            """, result, ignoreLineEndingDifferences: true);
        Assert.Equal(1, items.EnumerationCount);
    }

    private sealed class SingleUseEnumerable : IEnumerable<int>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<int> GetEnumerator()
        {
            if (++EnumerationCount > 1)
                throw new InvalidOperationException("The source can only be enumerated once.");

            yield return 1;
            yield return 2;
            yield return 3;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
