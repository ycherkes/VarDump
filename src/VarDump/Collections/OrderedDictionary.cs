using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace VarDump.Collections;

// original version see https://github.com/jehugaleahsa/truncon.collections.OrderedDictionary

public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IList<KeyValuePair<TKey, TValue>>
{
}

/// <summary>
/// Represents a dictionary that tracks the order that items were added.
/// </summary>
/// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
/// <typeparam name="TValue">The type of the dictionary values.</typeparam>
/// <remarks>
/// This dictionary makes it possible to get the index of a key and a key based on an index.
/// It can be costly to find the index of a key because it must be searched for linearly.
/// It can be costly to insert a key/value pair because other key's indexes must be adjusted.
/// It can be costly to remove a key/value pair because other keys' indexes must be adjusted.
/// </remarks>
internal sealed class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
{
    private const string EditReadOnlyList = "An attempt was made to edit a read-only list.";
    private readonly Dictionary<TKey, int> _dictionary;
    private readonly List<TKey> _keys;
    private readonly List<TValue> _values;
    private int _version;

    /// <summary>
    /// Initializes a new instance of an OrderedDictionary.
    /// </summary>
    public OrderedDictionary()
        : this(0, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of an OrderedDictionary.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">The capacity is less than zero.</exception>
    public OrderedDictionary(int capacity)
        : this(capacity, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of an OrderedDictionary.
    /// </summary>
    /// <param name="comparer">The equality comparer to use to compare keys.</param>
    public OrderedDictionary(IEqualityComparer<TKey> comparer)
        : this(0, comparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of an OrderedDictionary.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    /// <param name="comparer">The equality comparer to use to compare keys.</param>
    public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
    {
        _dictionary = new Dictionary<TKey, int>(capacity, comparer ?? EqualityComparer<TKey>.Default);
        _keys = new List<TKey>(capacity);
        _values = new List<TValue>(capacity);
    }

    /// <summary>
    /// Adds the given key/value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key to add to the dictionary.</param>
    /// <param name="value">The value to associated with the key.</param>
    /// <exception cref="System.ArgumentException">The given key already exists in the dictionary.</exception>
    /// <exception cref="System.ArgumentNullException">The key is null.</exception>
    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, _values.Count);
        _keys.Add(key);
        _values.Add(value);
        ++_version;
    }

    /// <summary>
    /// Inserts the given key/value pair at the specified index.
    /// </summary>
    /// <param name="index">The index to insert the key/value pair.</param>
    /// <param name="key">The key to insert.</param>
    /// <param name="value">The value to insert.</param>
    /// <exception cref="System.ArgumentException">The given key already exists in the dictionary.</exception>
    /// <exception cref="System.ArgumentNullException">The key is null.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">The index is negative -or- larger than the size of the dictionary.</exception>
    public void Insert(int index, TKey key, TValue value)
    {
        if (index < 0 || index > _values.Count)
        {
            throw new IndexOutOfRangeException(nameof(index));
        }
        _dictionary.Add(key, index);
        for (int keyIndex = index; keyIndex != _keys.Count; ++keyIndex)
        {
            var otherKey = _keys[keyIndex];
            _dictionary[otherKey] += 1;
        }
        _keys.Insert(index, key);
        _values.Insert(index, value);
        ++_version;
    }

    /// <summary>
    /// Determines whether the given key exists in the dictionary.
    /// </summary>
    /// <param name="key">The key to look for.</param>
    /// <returns>True if the key exists in the dictionary; otherwise, false.</returns>
    /// <exception cref="System.ArgumentNullException">The key is null.</exception>
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    /// <summary>
    /// Gets the keys in the dictionary in the order they were added.
    /// </summary>
    public KeyCollection Keys => new(_dictionary);

    /// <summary>
    /// Removes the key/value pair with the given key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the pair to remove.</param>
    /// <returns>True if the key was found and the pair removed; otherwise, false.</returns>
    /// <exception cref="System.ArgumentNullException">The key is null.</exception>
    public bool Remove(TKey key)
    {
        if (_dictionary.TryGetValue(key, out int index))
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes the key/value pair at the given index.
    /// </summary>
    /// <param name="index">The index of the key/value pair to remove.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">The index is negative -or- larger than the size of the dictionary.</exception>
    public void RemoveAt(int index)
    {
        var key = _keys[index];
        for (int keyIndex = index + 1; keyIndex < _keys.Count; ++keyIndex)
        {
            var otherKey = _keys[keyIndex];
            _dictionary[otherKey] -= 1;
        }
        _dictionary.Remove(key);
        _keys.RemoveAt(index);
        _values.RemoveAt(index);
        ++_version;
    }

    /// <summary>
    /// Tries to get the value associated with the given key. If the key is not found,
    /// default(TValue) value is stored in the value.
    /// </summary>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="value">The value used to hold the results.</param>
    /// <returns>True if the key was found; otherwise, false.</returns>
    /// <exception cref="System.ArgumentNullException">The key is null.</exception>
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_dictionary.TryGetValue(key, out int index))
        {
            value = _values[index];
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Gets the values in the dictionary.
    /// </summary>
    public ValueCollection Values => new(_values);

    /// <summary>
    /// Gets or sets the value at the given index.
    /// </summary>
    /// <param name="index">The index of the value to get.</param>
    /// <returns>The value at the given index.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">The index is negative -or- beyond the length of the dictionary.</exception>
    public TValue this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }

    /// <summary>
    /// Gets or sets the value associated with the given key.
    /// </summary>
    /// <param name="key">The key to get the associated value by or to associate with the value.</param>
    /// <returns>The value associated with the given key.</returns>
    /// <exception cref="System.ArgumentNullException">The key is null.</exception>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">The key is not in the dictionary.</exception>
    public TValue this[TKey key]
    {
        get => _values[_dictionary[key]];
        set
        {
            if (_dictionary.TryGetValue(key, out int index))
            {
                _values[index] = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }

    /// <summary>
    /// Removes all key/value pairs from the dictionary.
    /// </summary>
    public void Clear()
    {
        _dictionary.Clear();
        _keys.Clear();
        _values.Clear();
        ++_version;
    }

    /// <summary>
    /// Gets the number of key/value pairs in the dictionary.
    /// </summary>
    public int Count => _dictionary.Count;

    /// <summary>
    /// Gets the key/value pairs in the dictionary in the order they were added.
    /// </summary>
    /// <returns>An enumerator over the key/value pairs in the dictionary.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        int startVersion = _version;
        for (int index = 0; index != _keys.Count; ++index)
        {
            var key = _keys[index];
            var value = _values[index];
            yield return new KeyValuePair<TKey, TValue>(key, value);
            if (_version != startVersion)
            {
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
            }
        }
    }

    int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
    {
        if (_dictionary.TryGetValue(item.Key, out int index) && Equals(_values[index], item.Value))
        {
            return index;
        }
        return -1;
    }

    void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
    {
        Insert(index, item.Key, item.Value);
    }

    KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
    {
        get
        {
            TKey key = _keys[index];
            TValue value = _values[index];
            return new KeyValuePair<TKey, TValue>(key, value);
        }
        set
        {
            TKey key = _keys[index];
            if (_dictionary.Comparer.Equals(key, value.Key))
            {
                _dictionary[value.Key] = index;
            }
            else
            {
                _dictionary.Add(value.Key, index);  // will throw if key already exists
                _dictionary.Remove(key);
            }
            _keys[index] = value.Key;
            _values[index] = value.Value;
        }
    }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        if (_dictionary.TryGetValue(item.Key, out int index) && Equals(_values[index], item.Value))
        {
            return true;
        }
        return false;
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }
        if (arrayIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, @"The index is negative or outside the bounds of the collection.");
        }
        for (int index = 0; index != _keys.Count && arrayIndex < array.Length; ++index, ++arrayIndex)
        {
            var key = _keys[index];
            var value = _values[index];
            array[arrayIndex] = new KeyValuePair<TKey, TValue>(key, value);
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        ICollection<KeyValuePair<TKey, TValue>> self = this;
        if (self.Contains(item))
        {
            return Remove(item.Key);
        }
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Wraps the keys in an OrderDictionary.
    /// </summary>
    public sealed class KeyCollection : ICollection<TKey>
    {
        private readonly Dictionary<TKey, int> _dictionary;

        /// <summary>
        /// Initializes a new instance of a KeyCollection.
        /// </summary>
        /// <param name="dictionary">The OrderedDictionary whose keys to wrap.</param>
        /// <exception cref="System.ArgumentNullException">The dictionary is null.</exception>
        internal KeyCollection(Dictionary<TKey, int> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// Copies the keys from the OrderedDictionary to the given array, starting at the given index.
        /// </summary>
        /// <param name="array">The array to copy the keys to.</param>
        /// <param name="arrayIndex">The index into the array to start copying the keys.</param>
        /// <exception cref="System.ArgumentNullException">The array is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The arrayIndex is negative.</exception>
        /// <exception cref="System.ArgumentException">The array, starting at the given index, is not large enough to contain all the keys.</exception>
        public void CopyTo(TKey[] array, int arrayIndex)
        {
            _dictionary.Keys.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of keys in the OrderedDictionary.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets an enumerator over the keys in the OrderedDictionary.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<TKey> GetEnumerator() => _dictionary.Keys.GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool ICollection<TKey>.Contains(TKey item) => _dictionary.ContainsKey(item);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException(EditReadOnlyList);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void ICollection<TKey>.Clear() => throw new NotSupportedException(EditReadOnlyList);

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool ICollection<TKey>.IsReadOnly => true;

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException(EditReadOnlyList);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Wraps the keys in an OrderDictionary.
    /// </summary>
    public sealed class ValueCollection : ICollection<TValue>
    {
        private readonly List<TValue> _values;

        /// <summary>
        /// Initializes a new instance of a ValueCollection.
        /// </summary>
        /// <param name="values">The OrderedDictionary whose keys to wrap.</param>
        /// <exception cref="System.ArgumentNullException">The dictionary is null.</exception>
        internal ValueCollection(List<TValue> values)
        {
            _values = values;
        }

        /// <summary>
        /// Copies the values from the OrderedDictionary to the given array, starting at the given index.
        /// </summary>
        /// <param name="array">The array to copy the values to.</param>
        /// <param name="arrayIndex">The index into the array to start copying the values.</param>
        /// <exception cref="System.ArgumentNullException">The array is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The arrayIndex is negative.</exception>
        /// <exception cref="System.ArgumentException">The array, starting at the given index, is not large enough to contain all the values.</exception>
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of values in the OrderedDictionary.
        /// </summary>
        public int Count => _values.Count;

        /// <summary>
        /// Gets an enumerator over the values in the OrderedDictionary.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<TValue> GetEnumerator() => _values.GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool ICollection<TValue>.Contains(TValue item) => _values.Contains(item);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException(EditReadOnlyList);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void ICollection<TValue>.Clear() => throw new NotSupportedException(EditReadOnlyList);

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool ICollection<TValue>.IsReadOnly => true;

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException(EditReadOnlyList);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

internal static class CollectionExtensions
{
    #region ToOrderedDictionary

    /// <summary>
    /// Creates a new OrderedDictionary from the given collection, using the key selector to extract the key.
    /// </summary>
    /// <typeparam name="TSource">The type of the items in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="source">The items to created the OrderedDictionary from.</param>
    /// <param name="keySelector">A delegate that can extract a key from an item in the collection.</param>
    /// <returns>An OrderedDictionary mapping the extracted keys to their values.</returns>
    public static OrderedDictionary<TKey, TSource> ToOrderedDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        return ToOrderedDictionary(source, keySelector, null);
    }

    /// <summary>
    /// Creates a new OrderedDictionary from the given collection, using the key selector to extract the key.
    /// The key comparer is passed to the OrderedDictionary for comparing the extracted keys.
    /// </summary>
    /// <typeparam name="TSource">The type of the items in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="source">The items to created the OrderedDictionary from.</param>
    /// <param name="keySelector">A delegate that can extract a key from an item in the collection.</param>
    /// <param name="comparer">The key equality comparer to use to compare keys in the dictionary.</param>
    /// <returns>An OrderedDictionary mapping the extracted keys to their values.</returns>
    public static OrderedDictionary<TKey, TSource> ToOrderedDictionary<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey> comparer)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (keySelector == null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }
        var dictionary = new OrderedDictionary<TKey, TSource>(comparer);
        foreach (TSource item in source)
        {
            TKey key = keySelector(item);
            dictionary.Add(key, item);
        }
        return dictionary;
    }

    #endregion
}