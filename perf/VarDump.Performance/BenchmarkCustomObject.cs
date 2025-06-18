using BenchmarkDotNet.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VarDump.Performance;

[MemoryDiagnoser]
public class BenchmarkCustomObject
{
    private static object GetObjectInstance(int index) => new
    {
        Index = index,
        Name = "Test".PadRight(50),
        Id = Guid.NewGuid(),
        GroupId = "Group Id".PadRight(50),
        ParentGroup = "Parent Group".PadRight(12),
        GroupName = "Group Name".PadRight(100),
        ObjectType = "Object Type",
        IsObject = true,
        IsActive = true,
        CreateDate = DateTime.Now,
        CreateUser = "Create User".PadRight(50),
        ObjectCode = (string?)null,
        IsOwned = true,
        IsValid = true,
        IsStandard = true,
        Description = "Description",
        Nested = new
        {
            Name = "Test".PadRight(50),
            Id = Guid.NewGuid(),
            GroupId = "Group Id".PadRight(50),
            ParentGroup = "Parent Group".PadRight(12),
            GroupName = "Group Name".PadRight(100),
            ObjectType = "Object Type",
            IsObject = true,
            IsActive = true,
            CreateDate = DateTime.Now,
            CreateUser = "Create User".PadRight(50),
            ObjectCode = (string?)null,
            IsOwned = true,
            IsValid = true,
            IsStandard = true,
            Description = "Description"
        }
    };
    
    private static readonly object Variable = Enumerable.Range(0, 10000).Select(GetObjectInstance).ToArray();

    private static readonly CSharpDumper CSharpDumper = new CSharpDumper();
    private static readonly VisualBasicDumper VisualBasicDumper = new VisualBasicDumper();

    [Benchmark]
    public string CSharpDumper_Perf()
    {
        return CSharpDumper.Dump(Variable);
    }

    [Benchmark]
    public string VisualBasicDumper_Perf()
    {
        return VisualBasicDumper.Dump(Variable);
    }

    [Benchmark]
    public string ObjectDumperNet_Perf()
    {
        return ObjectDumper.Dump(Variable, DumpStyle.CSharp);
    }

    private static readonly JsonSerializerOptions MsOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Benchmark]
    public string MsJson_Perf()
    {
        return JsonSerializer.Serialize(Variable, MsOptions);
    }

    private static readonly JsonSerializerSettings NkSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    [Benchmark]
    public string NewtonsoftJson_Perf()
    {
        return JsonConvert.SerializeObject(Variable, NkSettings);
    }
}