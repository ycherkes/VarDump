using System;
using System.Text.RegularExpressions;
using VarDump.Visitor;
using Xunit;

namespace VarDump.UnitTests
{
    public class RegexSpec
    {
        [Fact]
        public void DumpRegexCSharp()
        {
            var currencyRegex = new Regex(@"\p{Sc}+\s*\d+", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

            var dumper = new CSharpDumper();

            var result = dumper.Dump(currencyRegex);

            Assert.Equal(
                """
                var regex = new Regex("\\p{Sc}+\\s*\\d+", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

                """, result);
        }

        [Fact]
        public void DumpRegexWithNamedArgumentsCSharp()
        {
            var currencyRegex = new Regex(@"\p{Sc}+\s*\d+", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

            var dumper = new CSharpDumper(new DumpOptions{ UseNamedArgumentsInConstructors = true});

            var result = dumper.Dump(currencyRegex);

            Assert.Equal(
                """
                var regex = new Regex(pattern: "\\p{Sc}+\\s*\\d+", options: RegexOptions.Compiled, matchTimeout: TimeSpan.FromSeconds(5));
                
                """, result);
        }

        [Fact]
        public void DumpRegexVb()
        {
            var currencyRegex = new Regex(@"\p{Sc}+\s*\d+", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

            var dumper = new VisualBasicDumper();

            var result = dumper.Dump(currencyRegex);

            Assert.Equal(
                """
                Dim regexValue = New Regex("\p{Sc}+\s*\d+", RegexOptions.Compiled, TimeSpan.FromSeconds(5))
                
                """, result);
        }

        [Fact]
        public void DumpRegexWithNamedArgumentsVb()
        {
            var currencyRegex = new Regex(@"\p{Sc}+\s*\d+", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

            var dumper = new VisualBasicDumper(new DumpOptions { UseNamedArgumentsInConstructors = true });

            var result = dumper.Dump(currencyRegex);

            Assert.Equal(
                """
                Dim regexValue = New Regex(pattern:="\p{Sc}+\s*\d+", options:=RegexOptions.Compiled, matchTimeout:=TimeSpan.FromSeconds(5))
                
                """, result);
        }
    }
}
