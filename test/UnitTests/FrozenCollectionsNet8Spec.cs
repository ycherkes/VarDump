#if NET8_0_OR_GREATER

using System.Collections.Frozen;
using System.Collections.Generic;
using VarDumpExtended;
using Xunit;

namespace UnitTests
{
    public class FrozenCollectionsNet8Spec
    {

        [Fact]
        public void DumpFrozenSetCsharp()
        {
            FrozenSet<int> frozenSet = new[] { 1 }.ToFrozenSet();

            var dumper = new CSharpDumper();

            var result = dumper.Dump(frozenSet);

            Assert.Equal("""
                var smallValueTypeComparableFrozenSetOfInt = new int[]
                {
                    1
                }.ToFrozenSet();

                """, result);
        }

        [Fact]
        public void DumpFrozenSetVb()
        {
            FrozenSet<int> frozenSet = new[] { 1 }.ToFrozenSet();

            var dumper = new VisualBasicDumper();

            var result = dumper.Dump(frozenSet);

            Assert.Equal("""
                Dim smallValueTypeComparableFrozenSetOfInteger = New Integer(){
                    1
                }.ToFrozenSet()
                
                """, result);
        }

        [Fact]
        public void DumpFrozenDictionaryCsharp()
        {
            var frozenDictionary = new Dictionary<string, string>
            {
                { "Steeve", "Test reference" }
            }.ToFrozenDictionary();

            var dumper = new CSharpDumper();

            var result = dumper.Dump(frozenDictionary);

            Assert.Equal("""
                var lengthBucketsFrozenDictionaryOfString = new Dictionary<string, string>
                {
                    {
                        "Steeve",
                        "Test reference"
                    }
                }.ToFrozenDictionary();
                
                """, result);
        }

        [Fact]
        public void DumpFrozenDictionaryVb()
        {
            var frozenDictionary = new Dictionary<string, string>
            {
                { "Steeve", "Test reference" }
            }.ToFrozenDictionary();

            var dumper = new VisualBasicDumper();

            var result = dumper.Dump(frozenDictionary);

            Assert.Equal("""
                Dim lengthBucketsFrozenDictionaryOfString = New Dictionary(Of String, String) From {
                    {
                        "Steeve",
                        "Test reference"
                    }
                }.ToFrozenDictionary()
                
                """, result);
        }
    }
}

#endif