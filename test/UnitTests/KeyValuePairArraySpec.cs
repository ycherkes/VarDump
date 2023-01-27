using System.Collections.Generic;
using VarDump;
using VarDump.Visitor;
using Xunit;

namespace UnitTests
{
    public class KeyValuePairArraySpec
    {
        [Fact]
        public void DumpKeyValuePairArrayVisualBasic()
        {
            var kvpArray = new KeyValuePair<int, string>[]
            {
                new(1, "First"),
                new(2, "Second")
            };

            var dumper = new VisualBasicDumper(new DumpOptions
            {
                IgnoreDefaultValues = true,
                IgnoreNullValues = true,
                MaxDepth = 5,
                UseTypeFullName = false,
                DateTimeInstantiation = DateTimeInstantiation.New,
                DateKind = DateKind.ConvertToUtc
            });

            var result = dumper.Dump(kvpArray);

            Assert.Equal(
@"Dim arrayOfKeyValuePair = New KeyValuePair(Of Integer, String)(){
    New KeyValuePair(Of Integer, String)(1, ""First""),
    New KeyValuePair(Of Integer, String)(2, ""Second"")
}
", result);
        }

        [Fact]
        public void DumpKeyValuePairArrayCSharp()
        {
            var kvpArray = new KeyValuePair<int, string>[]
            {
                new(1, "First"),
                new(2, "Second")
            };

            var Dumpr = new CSharpDumper(new DumpOptions
            {
                IgnoreDefaultValues = true,
                IgnoreNullValues = true,
                MaxDepth = 5,
                UseTypeFullName = false,
                DateTimeInstantiation = DateTimeInstantiation.New,
                DateKind = DateKind.ConvertToUtc
            });

            var result = Dumpr.Dump(kvpArray);

            Assert.Equal(
@"var arrayOfKeyValuePair = new KeyValuePair<int, string>[]
{
    new KeyValuePair<int, string>(1, ""First""),
    new KeyValuePair<int, string>(2, ""Second"")
};
", result);
        }
    }
}
