using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WsiuEngine.Collections
{
    public class ConcurrentHashSet<T> : ICollection<T>, IReadOnlyCollection<T>, ISet<T>, IReadOnlySet<T>, IEnumerable<T> where T : notnull
    {
        private readonly ConcurrentDictionary<T, T> _dictionary;

        public ConcurrentHashSet()
        {
            _dictionary = [];
        }
        public ConcurrentHashSet(IEnumerable<T> collection)
        {
            var kvpCollection = CollectionToKeValuePair(collection);
            _dictionary = new(kvpCollection);
        }
        public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer)
        {
            var kvpCollection = CollectionToKeValuePair(collection);
            _dictionary = new(kvpCollection, comparer);
        }
        public ConcurrentHashSet(IEqualityComparer<T> comparer)
        {
            _dictionary = new(comparer);
        }
        public ConcurrentHashSet(Int32 concurrencyLevel, Int32 capacity)
        {
            _dictionary = new(concurrencyLevel, capacity);
        }
        public ConcurrentHashSet(Int32 concurrencyLevel, Int32 capacity, IEqualityComparer<T>? comparer)
        {
            _dictionary = new(concurrencyLevel, capacity, comparer);
        }

        public T GetOrAdd(T item) => _dictionary.GetOrAdd(item, item);
        public bool Add(T item) => _dictionary.TryAdd(item, item);
        public bool Contains(T item) => _dictionary.ContainsKey(item);
        public bool Remove(T item) => _dictionary.TryRemove(item, out _);
        public void Clear() => _dictionary.Clear();
        public int Count => _dictionary.Count;
        public bool IsEmpty => _dictionary.IsEmpty;
        public IEqualityComparer<T> Comparer => _dictionary.Comparer;
        public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool IsReadOnly => false;
        public void CopyTo(T[] array, int arrayIndex)
        {
            _dictionary.Keys.CopyTo(array, arrayIndex);
        }

        void ICollection<T>.Add(T item) => Add(item);

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var item in other) Add(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other) Remove(item);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            foreach (var item in this)
            {
                if (!ConcurrentHashSet<T>.ContainsInOther(other, item)) Remove(item);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                if (!Add(item)) Remove(item);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other) => Enumerable.All(this, item => ConcurrentHashSet<T>.ContainsInOther(other, item));
        public bool IsSupersetOf(IEnumerable<T> other) => Enumerable.All(other, item => Contains(item));
        public bool IsProperSubsetOf(IEnumerable<T> other) => IsSubsetOf(other) && Count < ConcurrentHashSet<T>.GetCountInOther(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => IsSupersetOf(other) && Count > ConcurrentHashSet<T>.GetCountInOther(other);
        public bool Overlaps(IEnumerable<T> other) => Enumerable.Any(other, item => Contains(item));
        public bool SetEquals(IEnumerable<T> other) => Count == ConcurrentHashSet<T>.GetCountInOther(other) && IsSubsetOf(other);

        private static bool ContainsInOther(IEnumerable<T> other, T item)
        {
            if (other is ICollection<T> collection) return collection.Contains(item);
            return Enumerable.Contains(other, item);
        }

        private static int GetCountInOther(IEnumerable<T> other)
        {
            if (other is ICollection<T> collection) return collection.Count;
            return Enumerable.Count(other);
        }

        private static IEnumerable<KeyValuePair<T, T>> CollectionToKeValuePair(IEnumerable<T> collection)
        {
            return collection.Select(item => new KeyValuePair<T, T>(item, item));
        }
    }
}