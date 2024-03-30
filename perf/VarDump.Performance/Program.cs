using BenchmarkDotNet.Running;

BenchmarkRunner.Run(typeof(Program).Assembly);

//using VarDump;
//using VarDump.Visitor;

//var o = new
//{
//    Name = "Test".PadRight(50),
//    Id = Guid.NewGuid(),
//    GroupId = "Group Id".PadRight(50),
//    ParentGroup = "Parent Group".PadRight(12),
//    GroupName = "Group Name".PadRight(100),
//    ObjectType = "Object Type",
//    IsObject = true,
//    IsActive = true,
//    CreateDate = DateTime.Now,
//    CreateUser = "Create User".PadRight(50),
//    ObjectCode = (string?)null,
//    IsOwned = true,
//    IsValid = true,
//    IsStandard = true,
//    Description = "Description",
//    Nested = new
//    {
//        Name = "Test".PadRight(50),
//        Id = Guid.NewGuid(),
//        GroupId = "Group Id".PadRight(50),
//        ParentGroup = "Parent Group".PadRight(12),
//        GroupName = "Group Name".PadRight(100),
//        ObjectType = "Object Type",
//        IsObject = true,
//        IsActive = true,
//        CreateDate = DateTime.Now,
//        CreateUser = "Create User".PadRight(50),
//        ObjectCode = (string?)null,
//        IsOwned = true,
//        IsValid = true,
//        IsStandard = true,
//        Description = "Description"
//    }
//};

//var data = Enumerable.Range(0, 10000).Select(_ => o);

//using var streamWriter = new StreamWriter(Stream.Null);

//new CSharpDumper(new DumpOptions { CacheIndentation = true }).Dump(data, streamWriter);

//----------------------------------------------------

//using var file = new FileStream("./tmp/test.cs", FileMode.OpenOrCreate, FileAccess.Write);

//using var streamWriter = new StreamWriter(file);

//new CSharpDumper().Dump(data, streamWriter);

//using var file1 = new FileStream("./tmp/test.json", FileMode.OpenOrCreate, FileAccess.Write);
//JsonSerializer.Serialize(file1, data, new JsonSerializerOptions
//{
//    WriteIndented = true
//});
//file1.Flush(true);