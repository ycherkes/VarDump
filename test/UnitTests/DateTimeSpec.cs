//using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using VarDump;
using VarDump.Visitor;
using Xunit;

namespace UnitTests
{
    public class DateTimeSpec
    {
        [Fact]
        public async Task DumpDateTimeCsharp()
        {
            var dateTime = DateTime.ParseExact("2023-08-05T12:47:09.9361937+02:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            var dumper = new CSharpDumper(new DumpOptions
            {
                UseTypeFullName = false,
                DateTimeInstantiation = DateTimeInstantiation.New,
                DateKind = DateKind.ConvertToUtc,
                GenerateVariableInitializer = false
            });

            var expectedResult = "new DateTime(2023, 8, 5, 10, 47, 9, 936, DateTimeKind.Utc).AddTicks(1937)";

            var result = dumper.Dump(dateTime);

            var evaluatedResult = await CSharpScript.EvaluateAsync<DateTime>(result, ScriptOptions.Default.WithImports("System"));

            Assert.Equal(dateTime.ToUniversalTime(), evaluatedResult.ToUniversalTime());
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void DumpDateTimeVb()
        {
            var dateTime = DateTime.ParseExact("2023-08-05T12:47:09.9361937+02:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            var dumper = new VisualBasicDumper(new DumpOptions
            {
                UseTypeFullName = false,
                DateTimeInstantiation = DateTimeInstantiation.New,
                DateKind = DateKind.ConvertToUtc,
                GenerateVariableInitializer = false
            });

            var expectedResult = "New Date(2023, 8, 5, 10, 47, 9, 936, DateTimeKind.Utc).AddTicks(1937)";

            var result = dumper.Dump(dateTime);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void DumpDateTimeOffsetCsharp()
        {
            var dto = DateTimeOffset.ParseExact("2022-06-24T11:59:21.7961218+03:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            
            var dumper = new CSharpDumper();

            var result = dumper.Dump(dto);

            Assert.Equal(
@"var dateTimeOffset = DateTimeOffset.ParseExact(""2022-06-24T11:59:21.7961218+03:00"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
", result);
        }

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