using Xunit;

namespace VarDump.UnitTests
{
    public class PrimitiveTypesSpec
    {
        [Fact]
        public void DumpDecimalCSharp()
        {
            const decimal value = 0.00000M;

            var dumper = new CSharpDumper();

            var result = dumper.Dump(value);

            Assert.Equal("var decimalValue = 0.00000m;\r\n", result);
        }

        [Fact]
        public void DumpDecimalVb()
        {
            const decimal value = 0.00000M;

            var dumper = new VisualBasicDumper();

            var result = dumper.Dump(value);

            Assert.Equal("Dim decimalValue = 0.00000D\r\n", result);
        }
    }
}
