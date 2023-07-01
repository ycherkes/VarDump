using UnitTests.TestModel;
using VarDump;
using Xunit;

namespace UnitTests
{
    public class CustomCollectionSpec
    {
        [Fact]
        public void DumpCustomCollectionVisualBasic()
        {
            var collection = new CatCollection
            {
                new Cat { Name = "Sylvester", Age = 8 },
                new Cat { Name = "Whiskers", Age = 2 },
                new Cat { Name = "Sasha", Age = 14 }
            };

            var dumper = new VisualBasicDumper();

            var result = dumper.Dump(collection);

            Assert.Equal(
@"Dim catCollectionOfObject = New CatCollection{
    New Cat With {
        .Age = 8,
        .Name = ""Sylvester""
    },
    New Cat With {
        .Age = 2,
        .Name = ""Whiskers""
    },
    New Cat With {
        .Age = 14,
        .Name = ""Sasha""
    }
}
", result);
        }

        [Fact]
        public void DumpCustomCollectionCsharp()
        {
            var collection = new CatCollection
            {
                new Cat { Name = "Sylvester", Age = 8 },
                new Cat { Name = "Whiskers", Age = 2 },
                new Cat { Name = "Sasha", Age = 14 }
            };

            var dumper = new CSharpDumper();

            var result = dumper.Dump(collection);

            Assert.Equal(
                @"var catCollectionOfObject = new CatCollection
{
    new Cat
    {
        Age = 8,
        Name = ""Sylvester""
    },
    new Cat
    {
        Age = 2,
        Name = ""Whiskers""
    },
    new Cat
    {
        Age = 14,
        Name = ""Sasha""
    }
};
", result);
        }

    }
}
