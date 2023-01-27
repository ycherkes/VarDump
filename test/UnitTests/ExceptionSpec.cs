using System;
using VarDump;
using Xunit;

namespace UnitTests
{
    public class ExceptionSpec
    {
        [Fact]
        public void DumpExceptionVisualBasic()
        {
            try
            {
                _ = new[] { "test" }[1];
            }
            catch (Exception e)
            {
                var dumper = new VisualBasicDumper();

                var result = dumper.Dump(e);

                Assert.Contains(".Message = \"Index was outside the bounds of the array.\"", result);
            }
        }
    }
}
