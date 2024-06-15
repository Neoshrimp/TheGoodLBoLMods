using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Randoms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RngFix.CustomRngs.Sampling
{
    public class UniformUniqueRandomPool<T> : IRandomPool<T>, ICollection<T>
    {

        private List<T> elems = new List<T>();
        private Dictionary<T, int> e2index = new Dictionary<T, int>();

        public UniformUniqueRandomPool() { }

        public UniformUniqueRandomPool(IEnumerable<T> other) : this()
        {
            AddRange(other);
        }

        public int Count => elems.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if (!e2index.ContainsKey(item))
            {
                elems.Add(item);
                e2index[item] = elems.Count - 1;
            }
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var r in range)
                Add(r);
        }

        public T Get(int index)
        {
            return elems[index];
        }

        public void Clear()
        {
            elems.Clear();
            e2index.Clear();
        }

        public bool Contains(T item)
        {
            return e2index.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            elems.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (e2index.TryGetValue(item, out int index))
            {
                int lastIndex = elems.Count - 1;
                T lastItem = elems[lastIndex];

                elems[index] = lastItem;
                elems.RemoveAt(lastIndex);

                e2index[lastItem] = index;

                e2index.Remove(item);
                return true;
            }
            return false;
        }

        public T Sample(RandomGen rng)
        {
            if (elems.Empty())
                throw new InvalidOperationException("Cannot sample from empty pool.");

            var i = rng.NextInt(0, Count - 1);
            var e = Get(i);
            Remove(e);

            return e;

        }

        public T SampleOrDefault(RandomGen rng)
        {
            if (elems.Empty())
                return default;
            return Sample(rng);

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return elems.GetEnumerator();
        }

        public IEnumerator<RandomPoolEntry<T>> GetEnumerator()
        {
            return elems.Select(e => new RandomPoolEntry<T>(e, 1f)).GetEnumerator();
        }


    }
}
