using System.Collections.Generic;
using VarDump.Visitor;
using Xunit;

namespace VarDump.Extensions.UnitTests;

public class VarDumpExtensionsSpec
{
    [Fact]
    public void DumpAnonymousTypeCsharp()
    {
        var anonymous = new[]
        {
            new { Name = "Steeve", Age = (int?)int.MaxValue, Reference = "Test reference" },
            new { Name = "Peter", Age = (int?)null, Reference = (string)null }
        };

        VarDumpExtensions.VarDumpFactory = VarDumpFactories.CSharp;

        var result = anonymous.Dump();

        Assert.Equal(
            """
            var arrayOfAnonymousType = new []
            {
                new
                {
                    Name = "Steeve",
                    Age = (int?)int.MaxValue,
                    Reference = "Test reference"
                },
                new
                {
                    Name = "Peter",
                    Age = (int?)null,
                    Reference = (string)null
                }
            };

            """, result);
    }

    [Fact]
    public void DumpAnonymousTypeCustomOptionsCsharp()
    {
        var anonymous = new[]
        {
            new
            {
                d = new Dictionary<string, string>
                {
                    {"test", "test"}
                }
            },
            new
            {
                d = (Dictionary<string, string>)null
            }
        };

        VarDumpExtensions.VarDumpFactory = VarDumpFactories.CSharp;
        VarDumpExtensions.DefaultDumpOptions = new DumpOptions
        {
            UseTypeFullName = true
        };

        var result = anonymous.Dump();

        Assert.Equal(
            """
            var arrayOfAnonymousType = new []
            {
                new 
                {
                    d = new System.Collections.Generic.Dictionary<string, string>
                    {
                        {
                            "test",
                            "test"
                        }
                    }
                },
                new 
                {
                    d = (System.Collections.Generic.Dictionary<string, string>)null
                }
            };
            
            """, result);
    }

    [Fact]
    public void DumpAnonymousTypeVb()
    {
        VarDumpExtensions.VarDumpFactory = VarDumpFactories.VisualBasic;

        var anonymous = new[]
        {
            new { Name = "Steeve", Age = (int?)int.MaxValue, Reference = "Test reference" },
            new { Name = "Peter", Age = (int?)null, Reference = (string)null }
        };

        var result = anonymous.Dump();

        Assert.Equal(
            """
            Dim arrayOfAnonymousType = {
                New With {
                    .Name = "Steeve",
                    .Age = CType(Integer.MaxValue, Integer?),
                    .Reference = "Test reference"
                },
                New With {
                    .Name = "Peter",
                    .Age = CType(Nothing, Integer?),
                    .Reference = CType(Nothing, String)
                }
            }

            """, result);
    }
}