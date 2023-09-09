using System;
using VarDumpExtended;
using Xunit;

namespace UnitTests;

public class VersionSpec
{
    [Fact]
    public void DumpVersionSpecCsharp()
    {
        var version = new Version("1.2.3.4");

        var dumper = new CSharpDumper();

        var result = dumper.Dump(version);

        Assert.Equal(
            @"var version = new Version(""1.2.3.4"");
", result);
    }


    [Fact]
    public void DumpVersionSpecVb()
    {
        var version = new Version("1.2.3.4");

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(version);

        Assert.Equal(
            @"Dim versionValue = New Version(""1.2.3.4"")
", result);
    }
}