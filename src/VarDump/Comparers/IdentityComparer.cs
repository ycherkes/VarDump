using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VarDump.Comparers;

// see https://stackoverflow.com/questions/11240036/what-does-runtimehelpers-gethashcode-do
internal sealed class IdentityComparer<T> : IEqualityComparer<T> where T : class
{
    public bool Equals(T x, T y)
    {
        return ReferenceEquals(x, y);
    }

    public int GetHashCode(T x)
    {
        return RuntimeHelpers.GetHashCode(x);
    }
}