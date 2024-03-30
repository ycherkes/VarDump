using BenchmarkDotNet.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VarDump.Performance;

[MemoryDiagnoser]
public class BenchmarkCustomObject
{
    private static readonly object O = new
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
    
    private static readonly object Variable = Enumerable.Range(0, 10000).Select(_ => O);

    private static readonly CSharpDumper CSharpDumper = new CSharpDumper();
    private static readonly VisualBasicDumper VisualBasicDumper = new VisualBasicDumper();

    [Benchmark]
    public void CSharpDumper_CustomObject()
    {
        using var streamWriter = new StreamWriter(Stream.Null);
        CSharpDumper.Dump(Variable, streamWriter);
    }

    [Benchmark]
    public void VisualBasicDumper_CustomObject()
    {
        using var streamWriter = new StreamWriter(Stream.Null);
        VisualBasicDumper.Dump(Variable, streamWriter);
    }

    [Benchmark]
    public string ObjectDumperNet_CustomObject()
    {
        return Variable.Dump(DumpStyle.CSharp);
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Benchmark]
    public void MsJson_CustomObject()
    {
        JsonSerializer.Serialize(Stream.Null, Variable, Options);
    }
}