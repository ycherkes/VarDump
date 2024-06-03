using System;
using VarDump.Utils;
using Xunit;

namespace VarDump.UnitTests.Utils;

public class ReflectionUtilsSpec
{
    [Theory]
    [InlineData(typeof(char))]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(string))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(short))]
    [InlineData(typeof(int))]
    [InlineData(typeof(long))]
    [InlineData(typeof(float))]
    [InlineData(typeof(double))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(bool))]
    public void IsPrimitive(Type type)
    {
        Assert.True(ReflectionUtils.IsPrimitive(type));
    }
}