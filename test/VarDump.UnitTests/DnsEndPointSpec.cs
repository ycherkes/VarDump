using System.Net;
using Xunit;

namespace VarDump.UnitTests;

public class DnsEndPointSpec
{
    [Fact]
    public void DumpDnsEndPointCsharp()
    {
        var dnsEndPoint = new DnsEndPoint("google.com", 12345);

        var dumper = new CSharpDumper();

        var result = dumper.Dump(dnsEndPoint);

        Assert.Equal(
            @"var dnsEndPoint = new DnsEndPoint(""google.com"", 12345);
", result);
    }


    [Fact]
    public void DumpDnsEndPointVb()
    {
        var dnsEndPoint = new DnsEndPoint("google.com", 12345);

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(dnsEndPoint);

        Assert.Equal(
            @"Dim dnsEndPointValue = New DnsEndPoint(""google.com"", 12345)
", result);
    }
}