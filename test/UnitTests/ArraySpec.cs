using System.Collections.Immutable;
using VarDump;
using Xunit;

namespace UnitTests;

public class ArraySpec
{
    [Fact]
    public void DumpArrayOfArraysCsharp()
    {
        int[][] array = { new[] { 1 } };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

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
    public void DumpImmutableArrayOfArraysCsharp()
    {
        var array = new[] { new[] { 1 } }.ToImmutableArray();

        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            @"var immutableArrayOfArrayOfInt = new int[][]
{
    new int[]
    {
        1
    }
}.ToImmutableArray();
", result);
    }

    [Fact]
    public void Dump2DimensionalArrayCsharp()
    {
        var array = new[,] { { 2, 3, 4 }, { 5, 6, 7 } };
        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            @"var arrayOfInt = new int[,]
{
    {
        2,
        3,
        4
    },
    {
        5,
        6,
        7
    }
};
", result);
    }

    [Fact]
    public void Dump2DimensionalAnonymousArrayCsharp()
    {
        var array = new[,] { { new { Name = "Test1" } }, { new { Name = "Test2" } } };
        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            @"var arrayOfAnonymousType = new [,]
{
    {
        new 
        {
            Name = ""Test1""
        }
    },
    {
        new 
        {
            Name = ""Test2""
        }
    }
};
", result);
    }

    [Fact]
    public void DumpArrayOfArraysAnonymousCsharp()
    {
        var array = new[] { new[] { new { Name = "Clark" } } };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            @"var arrayOfArrayOfAnonymousType = new []
{
    new []
    {
        new 
        {
            Name = ""Clark""
        }
    }
};
", result);
    }

    [Fact]
    public void DumpArrayOfArraysVb()
    {
        int[][] array = { new[] { 1 } };

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            @"Dim arrayOfArrayOfInteger = New Integer()(){
    New Integer(){
        1
    }
}
", result);
    }

    [Fact]
    public void Dump2DimensionalArrayVb()
    {
        var array = new[,] { { 2, 3, 4 }, { 5, 6, 7 } };
        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            @"Dim arrayOfInteger = New Integer(,){
    {
        2,
        3,
        4
    },
    {
        5,
        6,
        7
    }
}
", result);
    }

    [Fact]
    public void Dump2DimensionalAnonymousArrayVb()
    {
        var array = new[,] { { new { Name = "Test1" } }, { new { Name = "Test2" } } };
        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            @"Dim arrayOfAnonymousType = {
    {
        New With {
            .Name = ""Test1""
        }
    },
    {
        New With {
            .Name = ""Test2""
        }
    }
}
", result);
    }

    [Fact]
    public void DumpArrayOfArraysAnonymousVb()
    {
        var array = new[] { new[] { new { Name = "Clark" } } };

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            @"Dim arrayOfArrayOfAnonymousType = {
    {
        New With {
            .Name = ""Clark""
        }
    }
}
", result);
    }
}