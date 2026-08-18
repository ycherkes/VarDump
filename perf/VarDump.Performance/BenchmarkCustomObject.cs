using BenchmarkDotNet.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VarDump.Performance;

[MemoryDiagnoser]
public class BenchmarkCustomObject
{
    private static TestObject GetObjectInstance(int index) => new TestObject
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
        ObjectCode = null,
        IsOwned = true,
        IsValid = true,
        IsStandard = true,
        Description = "Description",
        Nested = new TestObject
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
            ObjectCode = null,
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
    public void CSharpDumper_TextWriterNull()
    {
        CSharpDumper.Dump(Variable, TextWriter.Null);
    }

    [Benchmark]
    public void VisualBasicDumper_TextWriterNull()
    {
        VisualBasicDumper.Dump(Variable, TextWriter.Null);
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

internal class TestObject
{
    public int Index { get; set; }
    public string? Name { get; set; }
    public Guid Id { get; set; }
    public string? GroupId { get; set; }
    public string? ParentGroup { get; set; }
    public string? GroupName { get; set; }
    public string? ObjectType { get; set; }
    public bool IsObject { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDate { get; set; }
    public string? CreateUser { get; set; }
    public string? ObjectCode { get; set; }
    public bool IsOwned { get; set; }
    public bool IsValid { get; set; }
    public bool IsStandard { get; set; }
    public string? Description { get; set; }
    public TestObject? Nested { get; set; }
}
