using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Globalization;
using System.Threading.Tasks;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class DateTimeSpec
{
    [Fact]
    public async Task DumpDateTimeCSharp()
    {
        var dateTime = DateTime.ParseExact("2023-08-05T12:47:09.9361937+02:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        var dumper = new CSharpDumper(new DumpOptions
        {
            TypeNamePolicy = TypeNamingPolicy.ShortName,
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
    public void DumpDateTimeWithArgumentNamesCSharp()
    {
        var dateTime = DateTime.ParseExact("2023-08-05T12:47:09.9361937+02:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        var dumper = new CSharpDumper(new DumpOptions
        {
            TypeNamePolicy = TypeNamingPolicy.ShortName,
            DateTimeInstantiation = DateTimeInstantiation.New,
            DateKind = DateKind.ConvertToUtc,
            GenerateVariableInitializer = false,
            UseNamedArgumentsInConstructors = true
        });

        var expectedResult = "new DateTime(year: 2023, month: 8, day: 5, hour: 10, minute: 47, second: 9, millisecond: 936, kind: DateTimeKind.Utc).AddTicks(1937)";

        var result = dumper.Dump(dateTime);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task DumpDateTimeOffsetNewCSharp()
    {
        var dateTimeOffset = DateTimeOffset.ParseExact("2022-06-24T11:59:21.7961218+03:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        var dumper = new CSharpDumper(new DumpOptions
        {
            TypeNamePolicy = TypeNamingPolicy.ShortName,
            DateTimeInstantiation = DateTimeInstantiation.New,
            GenerateVariableInitializer = false
        });

        var expectedResult = "new DateTimeOffset(2022, 6, 24, 11, 59, 21, 796, TimeSpan.FromHours(3)).AddTicks(1218)";

        var result = dumper.Dump(dateTimeOffset);

        var evaluatedResult = await CSharpScript.EvaluateAsync<DateTimeOffset>(result, ScriptOptions.Default.WithImports("System"));

        Assert.Equal(dateTimeOffset, evaluatedResult);
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void DumpDateTimeOffsetWithArgumentNamesNewCSharp()
    {
        var dateTimeOffset = DateTimeOffset.ParseExact("2022-06-24T11:59:21.7961218+03:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        var dumper = new CSharpDumper(new DumpOptions
        {
            TypeNamePolicy = TypeNamingPolicy.ShortName,
            DateTimeInstantiation = DateTimeInstantiation.New,
            GenerateVariableInitializer = false,
            UseNamedArgumentsInConstructors = true
        });

        var expectedResult = "new DateTimeOffset(year: 2022, month: 6, day: 24, hour: 11, minute: 59, second: 21, millisecond: 796, offset: TimeSpan.FromHours(3)).AddTicks(1218)";

        var result = dumper.Dump(dateTimeOffset);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void DumpDateTimeVb()
    {
        var dateTime = DateTime.ParseExact("2023-08-05T12:47:09.9361937+02:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        var dumper = new VisualBasicDumper(new DumpOptions
        {
            TypeNamePolicy = TypeNamingPolicy.ShortName,
            DateTimeInstantiation = DateTimeInstantiation.New,
            DateKind = DateKind.ConvertToUtc,
            GenerateVariableInitializer = false
        });

        var expectedResult = "New Date(2023, 8, 5, 10, 47, 9, 936, DateTimeKind.Utc).AddTicks(1937)";

        var result = dumper.Dump(dateTime);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void DumpDateTimeOffsetCSharp()
    {
        var dto = DateTimeOffset.ParseExact("2022-06-24T11:59:21.7961218+03:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        var dumper = new CSharpDumper();

        var result = dumper.Dump(dto);

        Assert.Equal(
            """
            var dateTimeOffset = DateTimeOffset.ParseExact("2022-06-24T11:59:21.7961218+03:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            """, result);
    }

    [Fact]
    public void DumpDateOnlyCSharp()
    {
        var anonymous = new
        {
            DateOnly = DateOnly.ParseExact("2022-12-10", "O")
        };

        var dumper = new CSharpDumper(new DumpOptions
        {
            TypeNamePolicy = TypeNamingPolicy.ShortName,
            DateTimeInstantiation = DateTimeInstantiation.Parse
        });

        var result = dumper.Dump(anonymous);

        Assert.Equal(
            """
            var anonymousType = new 
            {
                DateOnly = DateOnly.ParseExact("2022-12-10", "O")
            };

            """, result);
    }

    [Fact]
    public void DumpTimeOnlyCSharp()
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
            """
            var anonymousType = new 
            {
                TimeOnly = TimeOnly.ParseExact("22:55:33.1220000", "O")
            };

            """, result);
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
            """
            Dim anonymousType = New With {
                .DateOnly = DateOnly.ParseExact("2022-12-10", "O")
            }

            """, result);
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
            """
            Dim anonymousType = New With {
                .TimeOnly = TimeOnly.ParseExact("22:55:33.1220000", "O")
            }

            """, result);
    }
}