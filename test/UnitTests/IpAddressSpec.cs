using System.Net;
using VarDump;
using Xunit;

namespace UnitTests
{
    public class IpAddressSpec
    {
        [Fact]
        public void DumpIpAddressCsharp()
        {
            var ipAddress = IPAddress.Parse("10.202.112.232");

            var dumper = new CSharpDumper();

            var result = dumper.Dump(ipAddress);

            Assert.Equal(
@"var iPAddress = IPAddress.Parse(""10.202.112.232"");
", result);
        }


        [Fact]
        public void DumpIpAddressVb()
        {
            var ipAddress = IPAddress.Parse("10.202.112.232");

            var dumper = new VisualBasicDumper();

            var result = dumper.Dump(ipAddress);

            Assert.Equal(
@"Dim iPAddressValue = IPAddress.Parse(""10.202.112.232"")
", result);
        }
    }
}