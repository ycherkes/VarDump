using System.Net;
using VarDump;
using Xunit;

namespace UnitTests;

public class IpEndPointSpec
{
    [Fact]
    public void DumpIpEndPointCsharp()
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Parse("142.250.74.110"), 12345);

        var dumper = new CSharpDumper();

        var result = dumper.Dump(ipEndPoint);

        Assert.Equal(
            @"var iPEndPoint = new IPEndPoint(IPAddress.Parse(""142.250.74.110""), 12345);
", result);
    }


    [Fact]
    public void DumpIpEndPointVb()
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Parse("142.250.74.110"), 12345);

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(ipEndPoint);

        Assert.Equal(
            @"Dim iPEndPointValue = New IPEndPoint(IPAddress.Parse(""142.250.74.110""), 12345)
", result);
    }
}