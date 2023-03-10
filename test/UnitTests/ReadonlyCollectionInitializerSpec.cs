using System.Collections.Generic;
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
    }

    public class CatDictionaryOwner
    {
        public IDictionary<string, Cat> Cats { get; } = new Dictionary<string, Cat>();
    }

    public class CatOwner
    {
        public ICollection<Cat> Cats { get; init; } = new List<Cat>();
    }

    public class Cat
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}