using System;
using Xunit;

namespace VarDump.UnitTests;

public class VersionSpec
{
    [Fact]
    public void DumpVersionCSharp()
    {
        var version = new Version("1.2.3.4");

        var dumper = new CSharpDumper();

        var result = dumper.Dump(version);

        Assert.Equal(
            """
            var version = new Version("1.2.3.4");

            """, result);
    }


    [Fact]
    public void DumpVersionVb()
    {
        var version = new Version("1.2.3.4");

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(version);

        Assert.Equal(
            """
            Dim versionValue = New Version("1.2.3.4")

            """, result);
    }
}