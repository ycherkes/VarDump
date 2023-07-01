using System;
using System.Globalization;
using VarDump;
using VarDump.Visitor;
using Xunit;

namespace UnitTests
{
    public class DateTimeSpec
    {
        [Fact]
        public void DumpDateOnlyCsharp()
        {
            var anonymous = new
            {
                DateOnly = DateOnly.ParseExact("2022-12-10", "O")
            };

            var dumper = new CSharpDumper(new DumpOptions
            {
                UseTypeFullName = false,
                DateTimeInstantiation = DateTimeInstantiation.Parse
            });

            var result = dumper.Dump(anonymous);

            Assert.Equal(
@"var anonymousType = new 
{
    DateOnly = DateOnly.ParseExact(""2022-12-10"", ""O"")
};
", result);
        }
        
        [Fact]
        public void DumpDateTimeOffsetCsharp()
        {
            var dto = DateTimeOffset.ParseExact("2022-06-24T11:59:21.7961218+03:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            
            var dumper = new CSharpDumper();

            var result = dumper.Dump(dto);

            Assert.Equal(
@"var dateTimeOffset = new DateTimeOffset(ticks: 637916687617961218, TimeSpan.FromHours(3));
", result);
        }

        [Fact]
        public void DumpTimeOnlyCsharp()
        {
            var anonymous = new
            {
                TimeOnly = TimeOnly.ParseExact("22:55:33.1220000", "O")
            };

            var dumper = new CSharpDumper(new DumpOptions
            {
                DateTimeInstantiation = DateTimeInstantiation.Parse
            });

            var result = dumper.Dump(anonymous);

            Assert.Equal(
@"var anonymousType = new 
{
    TimeOnly = TimeOnly.ParseExact(""22:55:33.1220000"", ""O"")
};
", result);
        }

        [Fact]
        public void DumpDateOnlyVb()
        {
            var anonymous = new
            {
                DateOnly = DateOnly.ParseExact("2022-12-10", "O")
            };

            var dumper = new VisualBasicDumper(new DumpOptions
            {
                DateTimeInstantiation = DateTimeInstantiation.Parse
            });

            var result = dumper.Dump(anonymous);

            Assert.Equal(
@"Dim anonymousType = New With {
    .DateOnly = DateOnly.ParseExact(""2022-12-10"", ""O"")
}
", result);
        }

        [Fact]
        public void DumpTimeOnlyVb()
        {
            var anonymous = new
            {
                TimeOnly = TimeOnly.ParseExact("22:55:33.1220000", "O")
            };

            var dumper = new VisualBasicDumper(new DumpOptions
            {
                DateTimeInstantiation = DateTimeInstantiation.Parse
            });

            var result = dumper.Dump(anonymous);

            Assert.Equal(
@"Dim anonymousType = New With {
    .TimeOnly = TimeOnly.ParseExact(""22:55:33.1220000"", ""O"")
}
", result);
        }
    }
}