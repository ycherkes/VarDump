using System.Net;
using Xunit;

namespace VarDump.UnitTests;

public class IpAddressSpec
{
    [Fact]
    public void DumpIpAddressCsharp()
    {
        var ipAddress = IPAddress.Parse("142.250.74.110");

        var dumper = new CSharpDumper();

        var result = dumper.Dump(ipAddress);

        Assert.Equal(
            @"var iPAddress = IPAddress.Parse(""142.250.74.110"");
", result);
    }


    [Fact]
    public void DumpIpAddressVb()
    {
        var ipAddress = IPAddress.Parse("142.250.74.110");

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(ipAddress);

        Assert.Equal(
            @"Dim iPAddressValue = IPAddress.Parse(""142.250.74.110"")
", result);
    }
}