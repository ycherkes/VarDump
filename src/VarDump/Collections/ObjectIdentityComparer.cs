using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VarDump.Collections;

/// <summary>
/// Source: https://stackoverflow.com/questions/11240036/what-does-runtimehelpers-gethashcode-do
/// </summary>
internal class ObjectIdentityComparer : IEqualityComparer<object>
{
    bool IEqualityComparer<object>.Equals(object x, object y)
    {
        return ReferenceEquals(x, y);
    }

    int IEqualityComparer<object>.GetHashCode(object x)
    {
        return RuntimeHelpers.GetHashCode(x);
    }
}