using LBoL.Base;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using RngFix.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine.InputSystem.XR;

namespace RngFix.CustomRngs.Sampling.UniformPools
{
    public class RepeatableUniformRandomPool<T> : IRandomPool<T>, ICollection<T>
    {

        private List<T> items = new List<T>();
        public int Count => items.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            items.Add(item);
        }
        public void AddRange(IEnumerable<T> range)
        {
            items.AddRange(range);
        }

        public T Get(int index)
        {
            return items[index];
        }

        public void Set(int index, T item)
        {
            items[index] = item;
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return items.Remove(item);
        }

        public T Sample(RandomGen rng)
        {
            if (items.Count <= 0)
                throw new InvalidOperationException("Cannot sample from empty pool.");

            var i = rng.NextInt(0, Count - 1);
            var e = Get(i);
            return e;
        }

        public T SampleOrDefault(RandomGen rng)
        {
            if (items.Count <= 0)
                return default;
            return Sample(rng);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public IEnumerator<RandomPoolEntry<T>> GetEnumerator()
        {
            return items.Select(i => new RandomPoolEntry<T>(i, 1f)).GetEnumerator();
        }
    }
}
