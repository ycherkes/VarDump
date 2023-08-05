using System.Collections;
using System.Collections.Generic;

#if !NET6_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    using ComponentModel;
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
#endif

namespace UnitTests.TestModel
{
    public class CatDictionaryOwner
    {
        public IDictionary<string, Cat> Cats { get; } = new Dictionary<string, Cat>();
    }

    public class CatOwner
    {
        public ICollection<Cat> Cats { get; init; } = new List<Cat>();
    }

    public class CatPublicCollection : IEnumerable
    {
        private readonly ArrayList _list = new();

        public void Add(Cat cat) => _list.Add(cat);

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }

    internal class CatNonPublicCollection : IEnumerable
    {
        private readonly ArrayList _list = new();

        public void Add(Cat cat) => _list.Add(cat);

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }

    public class Cat
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}
