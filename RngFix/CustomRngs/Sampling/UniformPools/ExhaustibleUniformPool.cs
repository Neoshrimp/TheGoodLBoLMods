using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Randoms;
using RngFix.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine.InputSystem.XR;


namespace RngFix.CustomRngs.Sampling.UniformPools
{
    /// <summary>
    /// doesnt sample consistently
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("fuck this bitch")]
    public class ExhaustibleUniformPool<T> : IRandomPool<T>
    {

        InCountedPool<T> pool;



        UniformUniqueRandomPool<T> indexStore = new UniformUniqueRandomPool<T>();

        int nullCount = 0;

        public ExhaustibleUniformPool(IEnumerable<T> items, int? targetPadding = null)
        {

            Dictionary<T, int> countedPool = new Dictionary<T, int>();

            int count = 0;
            foreach (var i in items)
            {
                if (i == null)
                {
                    continue;
                }

                countedPool.GetOrCreateVal(i, () => 0, out int _);
                countedPool[i]++;
                count++;

                if (!indexStore.Contains(i))
                    indexStore.Add(i);
            }


            pool = new InCountedPool<T>(countedPool, count);
        }

        public ExhaustibleUniformPool(IEnumerable<Tuple<T, int>> pairs, int? targetPadding = null)
        {

            this.pool = new InCountedPool<T>();
            foreach ((var g, var c) in pairs)
            {
            }


            if (targetPadding != null)
            {
                int actualPad = targetPadding.Value - ItemCount;
                if (actualPad < 0)
                {
                    actualPad = 0;
                    BepinexPlugin.log.LogWarning($"Target padding {targetPadding.Value} exceeded by {ItemCount}");
                }
                this.nullCount = actualPad;

            }
        }

        private T GetGroup(int i)
        {
            if (i >= GroupCount)
            {
                if(nullCount > 0)
                    return default;
                else
                    throw new ArgumentException($"{i} is out of range {GroupCount}");
            }

            return indexStore.Get(i);
        }

        public void AddNulls(int amount) 
        {
            if (amount < 0)
                throw new ArgumentException($"Amount {amount} must be greater than 0");
            nullCount += amount; 
        }

        public bool HasNulls() => nullCount > 0;

        public int ItemCount => pool.Count;

        public int GroupCount => pool.CountedPool.Count;

        public int GroupsAndNulls => GroupCount + nullCount;

        public bool ReduceOrRemoveGroup(T item)
        {
            if (item == null && nullCount > 0)
            {
                nullCount--;
                return true;
            }

            var rez = pool.ReduceCount(item);
            if (pool.RemoveItem(item))
                indexStore.Remove(item);
            return rez;
        }

        public T Sample(RandomGen rng)
        {
           if(GroupsAndNulls <= 0)
                throw new InvalidOperationException("Cannot sample from empty pool.");

            // nulls are represented as GroupCount
            var i = rng.NextInt(0, GroupsAndNulls - 1);
            

            var g = GetGroup(i);

            ReduceOrRemoveGroup(g);

            return g;
        }



        public T SampleOrDefault(RandomGen rng)
        {
            if (GroupsAndNulls <= 0)
                return default;
            return Sample(rng);
        }

        public IEnumerator<RandomPoolEntry<T>> GetEnumerator()
        {
            return pool.CountedPool.Keys.Select(g => new RandomPoolEntry<T>(g, 1f)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return pool.CountedPool.Keys.GetEnumerator();
        }
    }
}
