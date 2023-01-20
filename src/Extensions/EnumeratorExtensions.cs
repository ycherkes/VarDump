using System.Collections;
using System.Collections.Generic;

namespace VarDump.Extensions
{
    internal static class EnumeratorExtensions
    {
        public static IEnumerable<T> Cast<T>(this IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return (T)enumerator.Current;
            }
        }
    }
}
