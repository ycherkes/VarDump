using Xunit;

namespace VarDump.UnitTests;

public class IgnoreIndexersSpec
{
    [Fact]
    public void IgnoreIndexersCsharp()
    {
        var index = new MyClassWithIndexer();

        var dumper = new CSharpDumper();

        var result = dumper.Dump(index);

        Assert.Equal("var myClassWithIndexer = new MyClassWithIndexer\r\n{\r\n    Caption = \"A Default caption\"\r\n};\r\n", result);
    }

    [Fact]
    public void IgnoreIndexersVb()
    {
        var index = new MyClassWithIndexer();

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(index);

        Assert.Equal("Dim myClassWithIndexerValue = New MyClassWithIndexer With {\r\n    .Caption = \"A Default caption\"\r\n}\r\n", result);
    }

    private class MyClassWithIndexer
    {
        public string Caption { get; set; } = "A Default caption";

        private readonly string[] _strings = { "abc", "def", "ghi", "jkl" };
        public string this[int index]
        {
            get => _strings[index];
            set => _strings[index] = value;
        }
    }
}