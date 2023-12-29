using System;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

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
            var dumper = new VisualBasicDumper(new DumpOptions
            {
                WritablePropertiesOnly = false,
                MaxDepth = 1
            });

            var result = dumper.Dump(e);

            Assert.Contains(".Message = \"Index was outside the bounds of the array.\"", result);
        }
    }
}