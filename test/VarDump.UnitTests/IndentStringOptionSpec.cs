using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests;

public class IndentStringOptionSpec
{
    [Fact]
    public void DumpWithModifiedIndentation()
    {
        var obj = new
        {
            Level1 = new
            {
                Level2 = new
                {
                    Level3 = "Level3"
                }
            }
        };

        var dumpOptions = new DumpOptions
        {
            IndentString = " "
        };

        var dumper = new CSharpDumper(dumpOptions);

        var result = dumper.Dump(obj);

        Assert.Equal(
            """
            var anonymousType = new 
            {
             Level1 = new 
             {
              Level2 = new 
              {
               Level3 = "Level3"
              }
             }
            };
            
            """, result);
    }
}