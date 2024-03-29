using VarDump.UnitTests.TestModel;
using Xunit;

namespace VarDump.UnitTests;

public class ReadonlyCollectionInitializerSpec
{
    [Fact(Skip = "Not implemented yet")]
    public void DumpReadonlyPropertyCollectionInitializerCSharp()
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
            """
            var arrayOfArrayOfInt = new int[][]
            {
                new int[]
                {
                    1
                }
            };

            """, result);
    }

    [Fact(Skip = "Not implemented yet")]
    public void DumpReadonlyPropertyDictionaryInitializerCSharp()
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
            """
            var arrayOfArrayOfInt = new int[][]
            {
                new int[]
                {
                    1
                }
            };

            """, result);
    }
}