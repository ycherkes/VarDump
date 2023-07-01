using UnitTests.TestModel;
using VarDump;
using Xunit;

namespace UnitTests
{
    public class ReadonlyCollectionInitializerSpec
    {
        [Fact(Skip = "Not implemented yet")]
        public void DumpReadonlyPropertyCollectionInitializerCsharp()
        {
            CatOwner owner = new CatOwner
            {
                Cats =
                {
                    new Cat { Name = "Sylvester", Age = 8 },
                    new Cat { Name = "Whiskers", Age = 2 },
                    new Cat { Name = "Sasha", Age = 14 }
                }
            };

            var dumper = new CSharpDumper();

            var result = dumper.Dump(owner);

            Assert.Equal(
@"var arrayOfArrayOfInt = new int[][]
{
    new int[]
    {
        1
    }
};
", result);
        }

        [Fact(Skip = "Not implemented yet")]
        public void DumpReadonlyPropertyDictionaryInitializerCsharp()
        {
            var owner = new CatDictionaryOwner
            {
                Cats =
                {
                    { "Sylvester", new Cat { Name = "Sylvester", Age = 8 } },
                    { "Whiskers", new Cat { Name = "Whiskers", Age = 2 } },
                    { "Sasha", new Cat { Name = "Sasha", Age = 14 } }
                }
            };

            var dumper = new CSharpDumper();

            var result = dumper.Dump(owner);

            Assert.Equal(
@"var arrayOfArrayOfInt = new int[][]
{
    new int[]
    {
        1
    }
};
", result);
        }

        [Fact]
        public void DumpCustomCollectionCsharp()
        {
            CatCollection collection = new CatCollection
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