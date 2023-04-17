using System;
using VarDump;
using Xunit;

namespace UnitTests
{
    public class DelegateSpec
    {
        [Fact]
        public void DumpDelegateCsharp()
        {
            void EventHandler(object sender, EventArgs args)
            {
            }

            var dumper = new CSharpDumper();

            var result = dumper.Dump((EventHandler)EventHandler);

            Assert.Equal(
@"var eventHandler = default(EventHandler);
", result);
        }


        [Fact]
        public void DumpDelegateVb()
        {
            void EventHandler(object sender, EventArgs args)
            {
            }

            var dumper = new VisualBasicDumper();

            var result = dumper.Dump((EventHandler)EventHandler);

            Assert.Equal(
@"Dim eventHandlerValue = CType(Nothing, EventHandler)
", result);
        }
    }
}