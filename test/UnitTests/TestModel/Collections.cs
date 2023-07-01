using System.Collections;
using System.Collections.Generic;

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

    public class CatCollection : IEnumerable
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
