using System.Collections.Immutable;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class ArraySpec
{
    [Fact]
    public void DumpArrayOfArraysCSharp()
    {
        int[][] array = [[1]];

        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

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

    [Fact]
    public void DumpArrayOfArraysCSharpSingleLine()
    {
        int[][] array = [[1, 2, 3]];

        var dumper = new CSharpDumper(new DumpOptions
        {
            Format =
            {
                CollectionOfPrimitivesAsSingleLine = true
            }
        });

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            var arrayOfArrayOfInt = new int[][]
            {
                new int[]{ 1, 2, 3 }
            };
            
            """, result);
    }

    [Fact]
    public void Dump2DimensionalArrayCSharpSingleLine()
    {
        var array = new byte [,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 }
        };

        var dumper = new CSharpDumper(new DumpOptions
        {
            Format =
            {
                CollectionOfPrimitivesAsSingleLine = true
            }
        });

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            var arrayOfByte = new byte[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };
            
            """, result);
    }

    [Fact]
    public void DumpEmptyArrayCSharp()
    {
        var array = new int[0, 3];

        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            var arrayOfInt = new int[0, 0];
            
            """, result);
    }

    [Fact]
    public void DumpImmutableArrayOfArraysCSharp()
    {
        var array = new[] { new[] { 1 } }.ToImmutableArray();

        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            var immutableArrayOfArrayOfInt = new int[][]
            {
                new int[]
                {
                    1
                }
            }.ToImmutableArray();

            """, result);
    }

    [Fact]
    public void Dump2DimensionalArrayCSharp()
    {
        var array = new[,] { { 2, 3, 4 }, { 5, 6, 7 } };
        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            var arrayOfInt = new int[,]
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

            """, result);
    }

    [Fact]
    public void Dump2DimensionalAnonymousArrayCSharp()
    {
        var array = new[,] { { new { Name = "Test1" } }, { new { Name = "Test2" } } };
        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            var arrayOfAnonymousType = new [,]
            {
                {
                    new 
                    {
                        Name = "Test1"
                    }
                },
                {
                    new 
                    {
                        Name = "Test2"
                    }
                }
            };

            """, result);
    }

    [Fact]
    public void DumpArrayOfArraysAnonymousCSharp()
    {
        var array = new[] { new[] { new { Name = "Clark" } } };

        var dumper = new CSharpDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            var arrayOfArrayOfAnonymousType = new []
            {
                new []
                {
                    new 
                    {
                        Name = "Clark"
                    }
                }
            };

            """, result);
    }

    [Fact]
    public void DumpArrayOfArraysVb()
    {
        int[][] array = [[1]];

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            Dim arrayOfArrayOfInteger = New Integer()(){
                New Integer(){
                    1
                }
            }

            """, result);
    }

    [Fact]
    public void DumpArrayOfArraysVbSingleLine()
    {
        int[][] array = [[1, 2, 3]];

        var dumper = new VisualBasicDumper(new DumpOptions
        {
            Format =
            {
                CollectionOfPrimitivesAsSingleLine = true
            }
        });

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            Dim arrayOfArrayOfInteger = New Integer()(){
                New Integer(){ 1, 2, 3 }
            }
            
            """, result);
    }

    [Fact]
    public void Dump2DimensionalArrayVbSingleLine()
    {
        var array = new byte[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 }
        };

        var dumper = new VisualBasicDumper(new DumpOptions
        {
            Format =
            {
                CollectionOfPrimitivesAsSingleLine = true
            }
        });

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            Dim arrayOfByte = New Byte(,){
                { 1, 2, 3 },
                { 4, 5, 6 }
            }
            
            """, result);
    }

    [Fact]
    public void DumpEmptyArrayVb()
    {
        var array = new int[0, 3];

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            Dim arrayOfInteger = New Integer(0, 0) {}
            
            """, result);
    }

    [Fact]
    public void Dump2DimensionalArrayVb()
    {
        var array = new[,] { { 2, 3, 4 }, { 5, 6, 7 } };
        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            Dim arrayOfInteger = New Integer(,){
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

            """, result);
    }

    [Fact]
    public void Dump2DimensionalAnonymousArrayVb()
    {
        var array = new[,] { { new { Name = "Test1" } }, { new { Name = "Test2" } } };
        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            Dim arrayOfAnonymousType = {
                {
                    New With {
                        .Name = "Test1"
                    }
                },
                {
                    New With {
                        .Name = "Test2"
                    }
                }
            }

            """, result);
    }

    [Fact]
    public void DumpArrayOfArraysAnonymousVb()
    {
        var array = new[] { new[] { new { Name = "Clark" } } };

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(array);

        Assert.Equal(
            """
            Dim arrayOfArrayOfAnonymousType = {
                {
                    New With {
                        .Name = "Clark"
                    }
                }
            }

            """, result);
    }
}